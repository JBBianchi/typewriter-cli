# D-0003: Project Loading Strategy

**Date**: 2026-02-19
**Status**: Decided

## Context
The CLI needs to load C# projects/solutions and produce Roslyn `Document` + `SemanticModel` objects for the metadata layer. This is the most critical architectural decision.

## Options Evaluated

### Option A: Roslyn MSBuildWorkspace
- **API**: `Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace`
- **Package**: `Microsoft.CodeAnalysis.Workspaces.MSBuild`
- **How it works**: Opens `.sln` or `.csproj` → evaluates MSBuild → creates Roslyn workspace with full compilation
- **Pros**:
  - Direct `Document` + `SemanticModel` — exactly what metadata layer needs
  - Full symbol fidelity (same as VS)
  - Cross-platform (uses MSBuildLocator)
  - Handles project references, NuGet packages, multi-targeting
  - Well-maintained by Roslyn team
- **Cons**:
  - Requires NuGet restore before opening (packages must be resolved)
  - `.slnx` support: NOT natively supported yet (new format)
  - Can be slow on large solutions (sequential project evaluation)
  - Diagnostics can be opaque

### Option B: Buildalyzer + AdhocWorkspace
- **How it works**: Buildalyzer runs out-of-proc MSBuild → captures compiler args → populates AdhocWorkspace
- **Pros**:
  - Process isolation (avoids DLL conflicts)
  - Already embedded in upstream
- **Cons**:
  - Indirect: build → parse args → reconstruct workspace (lossy)
  - AdhocWorkspace may have lower symbol fidelity than MSBuildWorkspace
  - Embedded copy targets net472 — would need NuGet package for net10.0
  - Out-of-proc overhead
  - Less maintained than MSBuildWorkspace

### Option C: Microsoft.Build.Graph.ProjectGraph
- **How it works**: Static graph evaluation of project dependencies without full build
- **Pros**:
  - Fast dependency graph traversal
  - No build required
- **Cons**:
  - Does NOT produce Roslyn workspace or semantic models
  - Would still need MSBuildWorkspace for compilation
  - Only useful for project discovery, not model extraction

### Option D: Hybrid (MSBuildWorkspace + SolutionFile for graph)
- MSBuildWorkspace for loading + semantic models
- `Microsoft.Build.Construction.SolutionFile` for `.sln` parsing
- Custom `.slnx` parser for new format
- **Pros**: Best of both worlds
- **Cons**: Two code paths for solution parsing

## Decision
**Option D: Hybrid approach**

- **Primary loader**: `MSBuildWorkspace` (via `Microsoft.Build.Locator` for SDK resolution)
- **Solution parsing**: `MSBuildWorkspace.OpenSolutionAsync()` for `.sln`; custom `SlnxParser` for `.slnx`
- **Project loading**: `MSBuildWorkspace.OpenProjectAsync()` for `.csproj`
- **Graph info**: Extract from MSBuild evaluation (`ProjectReference` items)

## Rationale

### Why MSBuildWorkspace (not Buildalyzer)?
1. **Direct semantic model**: No intermediate lossy step
2. **Full fidelity**: Same compilation Roslyn uses in VS
3. **Cross-platform**: Works on Linux/macOS/Windows via MSBuildLocator
4. **Active maintenance**: Microsoft/Roslyn team
5. **Proven**: Used by OmniSharp, Roslynator, and other CLI tools

### Why not pure MSBuildWorkspace?
1. **`.slnx` gap**: MSBuildWorkspace doesn't support `.slnx` yet
2. **Multi-TFM handling**: Need custom logic to select TFM
3. **Restore detection**: Need pre-check before opening

### MSBuildLocator Integration
```csharp
// Register MSBuild before any MSBuild API usage
MSBuildLocator.RegisterDefaults();
// Or: MSBuildLocator.RegisterInstance(specificInstance);
// Respects global.json SDK selection
```

### .slnx Support Strategy
- Parse `.slnx` (XML format, simpler than `.sln`)
- Extract project paths
- Open each project individually via `MSBuildWorkspace.OpenProjectAsync()`
- Wire up project references via workspace API

### Restore Strategy
1. Check if `obj/project.assets.json` exists for each project
2. If missing: warn user, suggest `--restore` flag or prior `dotnet restore`
3. With `--restore`: run `dotnet restore` before opening workspace
4. CI recommendation: always restore before generation

### Multi-targeting Strategy
- Default: use first TFM in `TargetFrameworks` list
- `--framework <TFM>` flag: override to specific framework
- MSBuildWorkspace opens with global property `TargetFramework=<TFM>`

## Tradeoffs

| Aspect | MSBuildWorkspace | Buildalyzer |
|--------|-----------------|-------------|
| Symbol fidelity | Full | Potentially lossy |
| Cross-platform | Yes (via Locator) | Yes (out-of-proc) |
| Performance | Good (in-proc) | Slower (out-of-proc) |
| .sln support | Native | Native |
| .slnx support | No (custom needed) | No |
| Restore requirement | Yes | Yes |
| Maintenance | Active (Microsoft) | Community |
| Dependencies | Roslyn packages | MSBuild + logger packages |

## Evidence
- See PR-0001 for detailed loading spike analysis
- See F-0003 for Roslyn metadata layer requirements
- See F-0005 for Buildalyzer analysis

## Impact
- Primary dependency: `Microsoft.CodeAnalysis.Workspaces.MSBuild` + `Microsoft.Build.Locator`
- Custom code needed for: `.slnx` parsing, restore detection, TFM selection
- Metadata layer changes: Only `RoslynMetadataProvider` needs rewrite (use `MSBuildWorkspace` instead of `VisualStudioWorkspace`)

## Next Steps
- Phase 2: Implement MSBuildWorkspace project loader
- Phase 3: Implement `.sln` loading
- Phase 4: Implement `.slnx` loading
