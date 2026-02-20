# F-0005: Buildalyzer Integration and Project Loading

**Date**: 2026-02-19
**Status**: Complete

## Context
Analysis of the embedded Buildalyzer library and its relevance to CLI project loading strategy.

## Evidence

### Buildalyzer Overview
- **Location**: `origin/Buildalyzer/` — embedded fork of https://github.com/dnceng/Buildalyzer
- **Purpose**: Design-time MSBuild analysis — extracts properties, items, compiler arguments, references without full build
- **Target**: .NET Framework 4.7.2

### Key Components
| Component | File | Purpose |
|-----------|------|---------|
| `AnalyzerManager` | `Buildalyzer/src/Buildalyzer/AnalyzerManager.cs` | Entry point — loads `.sln`, manages project analyzers |
| `ProjectAnalyzer` | `Buildalyzer/src/Buildalyzer/ProjectAnalyzer.cs` | Runs MSBuild for individual projects, captures results |
| `AnalyzerResult` | `Buildalyzer/src/Buildalyzer/AnalyzerResult.cs` | Per-TFM build output (properties, items, compiler args) |
| `AnalyzerResultExtensions` | `Buildalyzer.Workspaces/AnalyzerResultExtensions.cs` | Creates Roslyn `AdhocWorkspace` from build results |

### How Buildalyzer Works
1. `new AnalyzerManager(solutionPath)` — parses `.sln` via MSBuild `SolutionFile`
2. `manager.GetProject(csprojPath)` → `ProjectAnalyzer`
3. `analyzer.Build()` — spawns MSBuild/dotnet **out-of-process**
4. `EventProcessor` captures: MSBuild properties, items, compiler command lines
5. `AnalyzerResult` provides: SourceFiles, References, PreprocessorSymbols, ProjectReferences, PackageReferences

### Buildalyzer.Workspaces → Roslyn
- `AnalyzerResultExtensions.AddToWorkspace()`:
  - Creates `ProjectId` from `ProjectGuid`
  - Creates `ProjectInfo` with documents, metadata references, analyzers
  - Sets `ParseOptions` (language version) and `CompilationOptions` (output kind)
  - Adds to `AdhocWorkspace`
  - Resolves project references transitively

### Actual Usage in Typewriter
- `Typewriter.csproj` references Buildalyzer (line 222)
- `Typewriter.Tests.csproj` references Buildalyzer.Workspaces (line 58)
- **But**: `RoslynMetadataProvider` uses `VisualStudioWorkspace`, NOT Buildalyzer
- Buildalyzer is referenced but **not actively used in VSIX runtime**

### Buildalyzer MSBuild Deps
- `Microsoft.Build` v17.14.28, `Microsoft.Build.Framework`, `Microsoft.Build.Tasks.Core`, `Microsoft.Build.Utilities.Core`
- `Microsoft.Extensions.Logging` v10.0.0
- `MsBuildPipeLogger.Server` v1.1.6
- `MSBuild.StructuredLogger` v2.3.71

### Cross-platform Considerations
- Buildalyzer uses out-of-process MSBuild — good isolation but overhead
- `SolutionFile` class (MSBuild) only supports `.sln`, not `.slnx`
- Works on Linux/macOS if MSBuild SDK is installed

## Conclusion
Buildalyzer is embedded and provides project loading without VS, but **MSBuildWorkspace** (from `Microsoft.CodeAnalysis.Workspaces.MSBuild`) is a more direct fit because:
1. It creates `Document` + `SemanticModel` natively (exactly what the metadata layer needs)
2. No out-of-process overhead
3. Better maintained by the Roslyn team
4. Buildalyzer's `AdhocWorkspace` approach loses some fidelity vs direct MSBuildWorkspace

However, Buildalyzer provides useful project graph info that MSBuildWorkspace lacks.

## Impact
- Buildalyzer is a **fallback option** if MSBuildWorkspace proves insufficient
- The hybrid approach (MSBuild for graph, MSBuildWorkspace for semantic model) may be optimal
- Buildalyzer targets net472 — would need to be replaced with NuGet package `Buildalyzer` if used

## Next Steps
- PR-0001: MSBuild loading spike
- D-0003: Loading strategy decision
