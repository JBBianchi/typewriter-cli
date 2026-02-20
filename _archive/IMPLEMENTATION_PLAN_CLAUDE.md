# Implementation Plan: Typewriter CLI Spin-off

## 1. Overview

- **Problem statement**: The Typewriter VS extension generates TypeScript (or other) code from C# type metadata using `.tst` templates. It is tightly coupled to Visual Studio and cannot run in CI pipelines or on non-Windows platforms. This spin-off creates a **standalone CLI tool** with equivalent generation capabilities.

- **Summary of approach**: Extract and port the generation pipeline (template parsing, Roslyn metadata extraction, template evaluation, file writing) from the upstream VSIX into a modern .NET 10 cross-platform CLI. Replace `VisualStudioWorkspace` with `MSBuildWorkspace` for project loading. Replace all VS-specific services (DTE, ProjectItem, ThreadHelper) with standard .NET APIs. Package as a `dotnet tool`.

- **Expected output artifacts**:
  - `Typewriter.Cli` — NuGet package (`dotnet tool install -g typewriter-cli`)
  - `Typewriter.CodeModel` — NuGet library (public API for template extensions)
  - Self-contained binaries (optional, for environments without .NET SDK)

## 2. Goals and Non-goals

### Goals
- Feature parity with upstream for code generation (58 features — see §7)
- Cross-platform: Linux, macOS, Windows
- CI-friendly: deterministic, scriptable, exit codes
- Support `.sln`, `.slnx`, `.csproj` inputs
- Handle multi-targeting, `global.json`, `Directory.Build.props/targets`, NuGet restore
- Same template syntax and semantics as upstream (`.tst` files unchanged)
- Installable as `dotnet tool`

### Non-goals
- VS extension functionality (IntelliSense, syntax highlighting, auto-render)
- Watch mode (deferred to post-v1.0 — see Q-0004)
- Modifying VS project files (adding generated files to `.csproj`)
- TFS/TFVC source control integration
- GUI or interactive mode

## 3. Upstream Architecture Summary

- **Repository layout**: 6 projects + 3 embedded Buildalyzer projects targeting .NET Framework 4.7.2 (see [F-0001](.ai/findings/F-0001-repository-structure.md))
- **Key projects**:
  - `Typewriter.CodeModel` — abstract public API (`Class`, `Type`, `Property`, etc.)
  - `Typewriter.Metadata` — provider interfaces (`IFileMetadata`, `IClassMetadata`, etc.)
  - `Typewriter.Metadata.Roslyn` — Roslyn-based metadata implementation
  - `Typewriter` (main) — VSIX entry point, generation pipeline, CodeModel implementations
- **Generation pipeline**: `.tst` → TemplateCodeParser → ShadowClass → Compiler → Parser → Template.WriteFile (see [F-0004](.ai/findings/F-0004-generation-pipeline.md))
- **Configuration model**: Abstract `Settings` class configured via `${ }` template code blocks. `SettingsImpl` uses DTE for project scoping (see [F-0007](.ai/findings/F-0007-configuration-model.md))
- **How upstream loads projects**: `VisualStudioWorkspace` obtained via VS ComponentModel/MEF service. Provides `Document` → `SemanticModel` for each C# file (see [F-0003](.ai/findings/F-0003-roslyn-metadata-layer.md))

## 4. Visual Studio Dependency Map (with replacement plan)

Full analysis: [F-0002](.ai/findings/F-0002-vs-dependencies.md)

### 4.1 Dependency Inventory

| Dependency Surface | Upstream Location | Why Used | Classification |
|---|---|---|---|
| `AsyncPackage` (VSIX entry) | `VisualStudio/ExtensionPackage.cs` | Extension initialization | Hard |
| `DTE` / `EnvDTE` | `ExtensionPackage.cs`, `SettingsImpl.cs`, `ProjectHelpers.cs`, `Template.cs`, `Log.cs` | Solution navigation, project enumeration, file operations | Hard |
| `VisualStudioWorkspace` | `Roslyn/RoslynMetadataProvider.cs` | Roslyn Document + SemanticModel | Hard |
| `ProjectItem` (EnvDTE) | `Template.cs`, `TemplateCodeParser.cs`, `Compiler.cs`, `Parser.cs`, `SettingsImpl.cs`, `ProjectHelpers.cs` | File paths, project hierarchy, metadata storage | Hard |
| `ThreadHelper` / `JoinableTaskFactory` | `RoslynMetadataProvider.cs`, `RoslynFileMetadata.cs`, `SettingsImpl.cs`, `ProjectHelpers.cs` | UI thread marshaling | Hard |
| `IVsSolution*` events | `SolutionMonitor.cs` | File change detection | Hard |
| `TypewriterOptionsPage` (`DialogPage`) | `TypewriterOptionsPage.cs` | VS options UI | Hard |
| `ErrorListProvider` / `IVsErrorList` | `ErrorList.cs` | Diagnostic display | Hard |
| `IVsOutputWindow` | `Log.cs` | Logging output | Hard |
| `VSLangProj.Reference` (COM) | `ProjectHelpers.cs` | Project reference enumeration | Hard |
| MEF composition | `LanguageService.cs`, `TemplateEditor/*.cs` | Editor features | Soft |
| VSCT commands | `RenderTemplate.cs`, `ExtensionPackage.vsct` | Context menu | Soft |
| Template editor (all) | `TemplateEditor/*.cs` | Syntax highlighting, IntelliSense | Soft |
| `DTE.SourceControl` | `Template.cs` | TFS checkout | Accidental |

### 4.2 Replacement Strategy

| VS Surface | CLI Replacement |
|---|---|
| `AsyncPackage` → | CLI `Main()` entry point with `System.CommandLine` |
| `DTE.Solution.AllProjects()` → | MSBuild project graph / solution parsing |
| `VisualStudioWorkspace` → | `MSBuildWorkspace` via `Microsoft.Build.Locator` |
| `ProjectItem` → | File path strings + `IProjectContext` abstraction |
| `ThreadHelper` / JTF → | Standard `async/await` |
| `IVsSolution*` events → | Not needed (batch mode) |
| `TypewriterOptionsPage` → | CLI flags + optional JSON config |
| `ErrorListProvider` → | Console diagnostics (MSBuild format) + exit codes |
| `IVsOutputWindow` → | `Microsoft.Extensions.Logging` → console |
| `VSLangProj.Reference` → | MSBuild `ProjectReference` items |

### 4.3 Removed / Optional Behaviors

| Behavior | Rationale |
|---|---|
| Auto-render on file save | VS IDE-only; CLI is batch mode |
| .tst IntelliSense | VS IDE-only; potential future VS Code extension |
| Add files to .csproj | Modern SDK projects use globs |
| TFS source control checkout | Legacy; Git doesn't require checkout |

## 5. MSBuild & Project Loading Design (sln, slnx, csproj)

Full analysis: [D-0003](.ai/decisions/D-0003-project-loading-strategy.md), [PR-0001](.ai/prototypes/PR-0001-msbuild-loading-spike.md)

### 5.1 Inputs

- **Supported inputs**: `.sln`, `.slnx`, `.csproj`
- **Discovery rules**: User provides `--solution` or `--project` flag explicitly
- **Template input**: Glob pattern as positional argument (e.g., `"**/*.tst"`)
- **Behavior with no input**: Error with usage help (exit code 2)

### 5.2 Loading Strategy Decision

- **Chosen approach**: Hybrid — `MSBuildWorkspace` for semantic model + custom `.slnx` parser
- **Rationale**: MSBuildWorkspace provides direct `Document` → `SemanticModel` with full fidelity, same Roslyn compiler pipeline as VS. Cross-platform via MSBuildLocator.
- **Tradeoffs**: Requires NuGet restore; no native `.slnx` support (custom parser needed ~50 lines)
- **Evidence**: [D-0003](.ai/decisions/D-0003-project-loading-strategy.md), [PR-0001](.ai/prototypes/PR-0001-msbuild-loading-spike.md), [F-0005](.ai/findings/F-0005-buildalyzer-integration.md)

### 5.3 Restore & SDK Resolution

- **global.json handling**: `MSBuildLocator.RegisterDefaults()` respects global.json SDK selection automatically
- **MSBuildLocator configuration**:
  ```csharp
  // Called once before any MSBuild API usage
  MSBuildLocator.RegisterDefaults();
  ```
- **Restore strategy**:
  1. Check `obj/project.assets.json` for each project
  2. If missing and `--restore` specified: run `dotnet restore`
  3. If missing and no `--restore`: error with message "Run `dotnet restore` first or use `--restore`" (exit code 3)
- **CI environments**: Document recommended pattern: `dotnet restore && typewriter-cli generate`

### 5.4 Multi-targeting & Configurations

- **TFM selection**: Default: first TFM in `TargetFrameworks`. Override: `--framework <TFM>` flag
- **Configuration**: Default: `Debug`. Override via MSBuild global property if needed (future flag)
- **Project graph traversal**: MSBuildWorkspace handles project references transitively. `Settings.IncludeReferencedProjects()` maps to `ProjectReference` items in MSBuild evaluation.

## 6. CLI UX Spec (commands, flags, exit codes)

### 6.1 Commands

```
typewriter-cli generate [options] <templates>
```

Where `<templates>` is a glob pattern for `.tst` files (e.g., `"**/*.tst"`, `"src/Templates/*.tst"`).

Future commands (post-v1.0):
- `typewriter-cli list` — list discovered templates and their output paths
- `typewriter-cli validate` — parse templates without generating

### 6.2 Flags

| Flag | Type | Default | Description |
|------|------|---------|-------------|
| `<templates>` | positional | (required) | Glob pattern for `.tst` files |
| `--solution <path>` | string | (auto-detect) | Path to `.sln` or `.slnx` file |
| `--project <path>` | string | (none) | Path to `.csproj` file (alternative to solution) |
| `--output <dir>` | string | (per-template) | Global output directory override |
| `--framework <TFM>` | string | (first TFM) | Target framework for multi-targeted projects |
| `--restore` | bool | false | Run `dotnet restore` before loading |
| `--verbosity` | enum | normal | `quiet`, `normal`, `detailed` |
| `--fail-on-warnings` | bool | false | Treat template warnings as errors |
| `--version` | flag | — | Print version and exit |
| `--help` | flag | — | Print usage and exit |

### 6.3 Exit Codes & Error Contract

| Code | Meaning | Example |
|------|---------|---------|
| 0 | Success | All templates generated successfully |
| 1 | Generation errors | Template compilation failed, render error |
| 2 | Input/argument errors | Missing required flag, invalid glob, file not found |
| 3 | Load/restore errors | NuGet restore needed, MSBuild load failure, SDK not found |

**Diagnostic format** (MSBuild-compatible):
```
typewriter: error TW001: Template compilation failed: missing reference 'Foo.dll' [src/Templates/Models.tst]
typewriter: warning TW002: No classes found matching filter 'IEntity' [src/Templates/Entities.tst]
typewriter: info TW003: Generated 42 files from 5 templates
```

## 7. Feature Parity Matrix

Full matrix: [P-0001](.ai/parity/P-0001-feature-matrix.md)

**Summary**:
- **58 features**: ✅ identical parity
- **1 feature**: 🟨 partial (long paths — auto-handled by .NET 10)
- **8 features**: ❌ not planned (all VS IDE-specific)
- **7 features**: ✅ new (CLI-specific: .slnx, --framework, --restore, exit codes, cross-platform, glob input)

**Intentional gaps** (all VS IDE-only):
- Auto-render on file save/change → batch mode is CLI design
- IntelliSense / syntax highlighting → IDE-only
- Add generated files to VS project → modern globs handle this
- TFS checkout → Git doesn't need it
- VS Error List / Output Window → console diagnostics replace these

## 8. Target Architecture (modules, APIs, boundaries)

### 8.1 High-level Module Diagram

```
┌─────────────────────────────────────────────────────┐
│                   Typewriter.Cli                      │
│  (Entry point, System.CommandLine, orchestration)     │
│  - Program.cs, GenerateCommand.cs                     │
└──────────────┬──────────────────┬────────────────────┘
               │                  │
┌──────────────▼──────┐  ┌───────▼───────────────────┐
│  Typewriter.Loading  │  │  Typewriter.Generation     │
│  (MSBuild + Roslyn)  │  │  (Template engine)         │
│  - ProjectLoader     │  │  - TemplateCodeParser      │
│  - SolutionLoader    │  │  - ShadowClass             │
│  - SlnxParser        │  │  - Compiler                │
│  - RestoreChecker    │  │  - Parser                  │
│  - CliMetadataProvider│ │  - SingleFileParser        │
└──────────────┬──────┘  │  - ItemFilter              │
               │          │  - Template                 │
               │          └───────┬───────────────────┘
               │                  │
┌──────────────▼──────────────────▼────────────────────┐
│              Typewriter.CodeModel                      │
│  (Shared: abstract API, metadata interfaces, impls)   │
│  - CodeModel/ (Class, Type, Property, etc.)           │
│  - Metadata/ (IFileMetadata, IClassMetadata, etc.)    │
│  - Roslyn/ (RoslynClassMetadata, etc.)                │
│  - Implementation/ (ClassImpl, TypeImpl, etc.)        │
│  - Collections/ (ClassCollectionImpl, etc.)           │
│  - Extensions/ (TypeExtensions, WebApi)               │
│  - Configuration/ (Settings, CliSettingsImpl)         │
│  - Helpers.cs                                         │
└───────────────────────────────────────────────────────┘
```

### 8.2 Key Interfaces

```csharp
// Project loading abstraction
public interface IProjectLoader
{
    Task<Workspace> LoadSolutionAsync(string solutionPath, LoadOptions options);
    Task<Workspace> LoadProjectAsync(string projectPath, LoadOptions options);
}

// Metadata extraction (reused from upstream)
public interface IMetadataProvider
{
    IFileMetadata GetFile(string path, Settings settings, Action<string[]> requestRender);
}

// Template orchestration
public interface ITemplateEngine
{
    TemplateResult ProcessTemplate(string templatePath, IMetadataProvider provider, Settings settings);
}

// Output writing
public interface IOutputWriter
{
    bool WriteFile(string outputPath, string content, OutputOptions options);
}

// Diagnostics
public interface IDiagnosticReporter
{
    void Error(string code, string message, string file, int line = -1);
    void Warning(string code, string message, string file, int line = -1);
    void Info(string message);
    bool HasErrors { get; }
    bool HasWarnings { get; }
}
```

### 8.3 Data Flow

```
CLI args
  → Parse flags (System.CommandLine)
  → Discover templates (glob expansion)
  → Load solution/project (MSBuildWorkspace via ProjectLoader)
  → For each template:
      → Parse template (TemplateCodeParser)
      → Compile template code (ShadowClass → Compiler)
      → Determine included projects (CliSettingsImpl)
      → For each C# file in included projects:
          → Get metadata (CliMetadataProvider → Document → SemanticModel)
          → Create CodeModel (FileImpl → ClassImpl, etc.)
          → Evaluate template (Parser / SingleFileParser)
          → Write output (OutputWriter with change detection)
  → Report summary
  → Exit with appropriate code
```

## 9. Implementation Phases (milestones + acceptance criteria)

### Phase 0 — Repo Bootstrap

**Tasks**:
- Create solution `Typewriter.Cli.sln` with projects:
  - `src/Typewriter.Cli/Typewriter.Cli.csproj` (net10.0, PackAsTool)
  - `src/Typewriter.CodeModel/Typewriter.CodeModel.csproj` (net10.0, classlib)
  - `src/Typewriter.Loading/Typewriter.Loading.csproj` (net10.0, classlib)
  - `src/Typewriter.Generation/Typewriter.Generation.csproj` (net10.0, classlib)
  - `tests/Typewriter.Tests/Typewriter.Tests.csproj` (net10.0, xunit)
- Configure `Directory.Build.props` (nullable enable, implicit usings, analyzers)
- Add `global.json` pinning .NET 10 SDK
- Add `System.CommandLine` package to CLI project
- Add `Microsoft.Build.Locator` + `Microsoft.CodeAnalysis.Workspaces.MSBuild` to Loading project
- Add `Microsoft.CodeAnalysis.CSharp.Workspaces` to CodeModel project
- Create minimal `Program.cs` with `--version` and `--help`

**Acceptance**:
- `dotnet build` succeeds on all three platforms (Windows, Linux, macOS)
- `dotnet run --project src/Typewriter.Cli -- --version` prints version
- `dotnet run --project src/Typewriter.Cli -- --help` prints usage
- CI pipeline (GitHub Actions) passes on ubuntu-latest, windows-latest, macos-latest

### Phase 1 — CLI Skeleton + Logging + Config Parsing

**Tasks**:
- Implement `GenerateCommand` with all flags from §6.2
- Implement `IDiagnosticReporter` → console output with MSBuild-format diagnostics
- Implement logging via `Microsoft.Extensions.Logging` with verbosity levels
- Implement glob expansion for template discovery
- Implement exit code contract
- Wire up `--verbosity`, `--fail-on-warnings`

**Acceptance**:
- `typewriter-cli generate "**/*.tst" --solution Foo.sln` parses all flags correctly
- Invalid args return exit code 2 with clear error message
- `--verbosity quiet` suppresses all output except errors
- `--verbosity detailed` shows debug-level information
- Template glob expands correctly (test with mock `.tst` files)

### Phase 2 — MSBuild Loading (.csproj)

**Tasks**:
- Implement `ProjectLoader` using `MSBuildLocator.RegisterDefaults()` + `MSBuildWorkspace`
- Implement `RestoreChecker` — verify `obj/project.assets.json`
- Implement `--restore` flag behavior (run `dotnet restore`)
- Implement `--framework <TFM>` for multi-targeted projects
- Handle `WorkspaceFailed` events → route to `IDiagnosticReporter`
- Test with sample SDK-style `.csproj` (single and multi-target)

**Acceptance**:
- `typewriter-cli generate "*.tst" --project Sample.csproj` opens project and loads compilation
- Multi-targeted project with `--framework net10.0` loads correct TFM
- Missing restore produces exit code 3 with clear message
- `--restore` runs `dotnet restore` before loading
- Works on Linux (GitHub Actions ubuntu-latest)

### Phase 3 — Solution Loading (.sln)

**Tasks**:
- Extend `ProjectLoader` for `.sln` input via `MSBuildWorkspace.OpenSolutionAsync()`
- Implement solution-level project enumeration for `Settings.IncludeAllProjects()`
- Handle projects excluded from build configuration
- Test with multi-project solutions

**Acceptance**:
- `typewriter-cli generate "*.tst" --solution Sample.sln` loads all projects
- `Settings.IncludeProject("ProjectName")` resolves correctly against solution projects
- `Settings.IncludeReferencedProjects()` follows ProjectReference graph
- Projects excluded from `Debug|Any CPU` build config are handled gracefully

### Phase 4 — Solution Loading (.slnx)

**Tasks**:
- Implement `SlnxParser` — XML parsing of `.slnx` format
- Extract project paths from `<Project Path="..."/>` elements
- Open each project via `MSBuildWorkspace.OpenProjectAsync()`
- Wire up project references across parsed projects

**Acceptance**:
- `typewriter-cli generate "*.tst" --solution Sample.slnx` works identically to `.sln`
- Invalid `.slnx` produces clear error (exit code 3)
- Test with sample `.slnx` containing 3+ projects

### Phase 5 — Semantic Model Extraction Parity

**Tasks**:
- Port all `Roslyn*Metadata` classes (17 files) from upstream to `Typewriter.CodeModel`:
  - Replace `ThreadHelper.JoinableTaskFactory.Run()` with `async/await`
  - Remove `using Microsoft.VisualStudio.*`
  - Target net10.0
- Implement `CliMetadataProvider : IMetadataProvider` using `MSBuildWorkspace`
- Port all `*Impl` classes (ClassImpl, TypeImpl, etc.) from upstream
- Port all collection implementations
- Port `Helpers.cs`, `Extensions/` (TypeExtensions, WebApi extensions)
- Port `ILog` abstraction → wire to `Microsoft.Extensions.Logging`

**Acceptance**:
- Unit tests for each metadata class (port upstream `ClassTests`, `PropertyTests`, `TypeTests`, etc.)
- All type resolution special cases pass: Nullable<T>, Task<T>, enums, arrays, value tuples, generics
- Partial class rendering (both Partial and Combined modes) works correctly
- WebApi extension methods produce correct HTTP method, URL, request data
- Test with real project containing: classes, interfaces, records, enums, delegates, generics, nested types, attributes

### Phase 6 — Template Execution Parity

**Tasks**:
- Port `TemplateCodeParser` — replace `ProjectItem` parameter with file path
- Port `ShadowClass` — adapt for .NET 10 compilation
- Implement `CliCompiler`:
  - Use `CSharpCompilation` targeting net10.0 runtime
  - Use `AssemblyLoadContext` instead of `Assembly.LoadFrom()`
  - Route diagnostics to `IDiagnosticReporter`
- Port `Parser` and `SingleFileParser` — replace `ProjectItem` with file path
- Port `ItemFilter`
- Implement `CliTemplate`:
  - Port `Template.cs` — remove DTE, ProjectItem, SourceControl
  - Implement `GetOutputPath()`, `HasChanged()`, `WriteFile()`
  - Implement path resolution (`~\`, `~~\`, relative)
- Implement `CliSettingsImpl`:
  - Constructor: `(ILogger log, string templatePath, string solutionPath, MSBuildWorkspace workspace)`
  - `IncludeCurrentProject()` → project containing template file
  - `IncludeReferencedProjects()` → MSBuild ProjectReference items
  - `IncludeAllProjects()` → all solution projects
  - `IncludeProject(name)` → find by name in solution
- Port `ContextAttribute` and `Contexts` class for identifier resolution

**Acceptance**:
- Golden file tests: port all `.tstemplate` + `.result` pairs from upstream
- `${ Settings.IncludeCurrentProject(); }` resolves correctly
- `${ Settings.SingleFileMode("output.ts"); }` produces single output file
- `${ Settings.OutputExtension = ".tsx"; }` changes output extension
- `#reference "path.dll"` loads assembly (test with sample extension)
- Lambda filters: `$Classes($x => x.IsPublic)[...]` works
- Wildcard filters: `$Classes(*Base)[...]` works
- Template compilation errors produce exit code 1 with file/line info
- Change detection skips unchanged files (verified via file timestamp)

### Phase 7 — Golden Tests + Sample Repos + Performance

**Tasks**:
- Create test fixture projects:
  - `tests/fixtures/SimpleProject/` — single csproj, basic classes
  - `tests/fixtures/MultiProject/` — solution with references
  - `tests/fixtures/MultiTarget/` — multi-targeted project
  - `tests/fixtures/WebApiProject/` — ASP.NET controllers
  - `tests/fixtures/ComplexTypes/` — generics, nested, partial, enums, records
- Port all upstream golden file tests (from `origin/src/Tests/Render/`)
- Port all upstream CodeModel tests (from `origin/src/Tests/CodeModel/`)
- Port all upstream extension tests (from `origin/src/Tests/Extensions/`)
- Performance benchmark: measure time for 50-project solution
- Verify cross-platform: run full test suite on Linux and macOS

**Acceptance**:
- All golden file tests produce byte-for-byte identical output to upstream `.result` files
- All CodeModel property assertions match upstream test expectations
- Full test suite passes on Windows, Linux, macOS
- 50-project solution generates in < 60 seconds

### Phase 8 — Packaging (dotnet tool) + CI Pipeline

**Tasks**:
- Configure `Typewriter.Cli.csproj`:
  ```xml
  <PackAsTool>true</PackAsTool>
  <ToolCommandName>typewriter-cli</ToolCommandName>
  <PackageId>Typewriter.Cli</PackageId>
  ```
- Configure `Typewriter.CodeModel.csproj` as standalone NuGet library
- Set up `dotnet pack` in CI
- Create GitHub Actions workflow:
  - Build + test on 3 platforms
  - Pack NuGet packages
  - Publish to NuGet.org (on tag)
- Create sample CI integration (GitHub Actions step using `typewriter-cli`)
- Write README with usage instructions

**Acceptance**:
- `dotnet tool install --global --add-source ./nupkg Typewriter.Cli` installs successfully
- `typewriter-cli generate "**/*.tst" --solution Sample.sln` works after tool install
- NuGet package includes correct metadata (license, description, icon)
- CI pipeline green on all 3 platforms
- `dotnet tool restore` works from tool manifest

## 10. Testing Strategy (unit/integration/golden tests)

### Unit Tests
- **Scope**: Individual classes in isolation
- **Targets**: `Helpers.CamelCase()`, `ItemFilter.Apply()`, `SlnxParser.Parse()`, type mapping, path resolution
- **Framework**: xUnit + `FluentAssertions`
- **Mocking**: `NSubstitute` for `IMetadataProvider`, `IProjectLoader`

### Integration Tests
- **Scope**: End-to-end with real MSBuild projects
- **Fixtures**: Sample `.csproj` and `.sln` projects in `tests/fixtures/`
- **Pattern**: Load project → extract metadata → verify CodeModel properties
- **Requires**: .NET SDK installed (MSBuildLocator)

### Golden File Tests
- **Scope**: Template rendering produces expected output
- **Data**: Ported from `origin/src/Tests/Render/` (`.tstemplate` + `.cs` + `.result` triples)
- **Pattern**: Parse template → render against fixture → diff output vs `.result` file
- **Library**: Consider `Verify` for snapshot testing with approval workflow

### Cross-platform Matrix
```yaml
strategy:
  matrix:
    os: [ubuntu-latest, windows-latest, macos-latest]
    dotnet: ['10.0.x']
```

## 11. CI/CD Plan (restore/build/generate verification)

### GitHub Actions Workflow

```yaml
name: Build and Test
on: [push, pull_request]
jobs:
  build:
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal
      - run: dotnet pack --no-build -o ./nupkg
      - uses: actions/upload-artifact@v4
        with:
          name: nupkg-${{ matrix.os }}
          path: ./nupkg/*.nupkg

  publish:
    needs: build
    if: startsWith(github.ref, 'refs/tags/v')
    runs-on: ubuntu-latest
    steps:
      - uses: actions/download-artifact@v4
      - run: dotnet nuget push **/*.nupkg --api-key ${{ secrets.NUGET_KEY }} --source https://api.nuget.org/v3/index.json
```

### Caching Strategy
- Cache `~/.nuget/packages` across builds
- Cache `obj/` directories for faster incremental builds

### Artifacts/Publishing
- NuGet packages published to nuget.org on tagged releases
- Self-contained binaries attached to GitHub Releases (optional)

## 12. Risk Register (top risks + mitigations)

| Risk | Impact | Likelihood | Mitigation | Reference |
|---|---:|---:|---|---|
| MSBuildWorkspace semantic model differs from VS | High | Low | Golden file tests, regression suite | [R-0001](.ai/risks/R-0001-msbuildworkspace-fidelity.md) |
| .slnx format changes before stabilization | Medium | Medium | Minimal parser, monitor dotnet/sdk, adopt official API when available | [R-0002](.ai/risks/R-0002-slnx-format-stability.md) |
| Template assembly loading fails cross-platform | High | Medium | Use AssemblyLoadContext, test on Linux/macOS in CI | [R-0003](.ai/risks/R-0003-assembly-loading-crossplatform.md) |
| NuGet restore state causes silent failures in CI | High | Medium | Pre-check project.assets.json, clear errors, --restore flag | [R-0004](.ai/risks/R-0004-restore-state-ci.md) |
| Source generators not included in MSBuildWorkspace compilation | Medium | Medium | Test with generator projects, evaluate GeneratorDriver API | [Q-0001](.ai/questions/Q-0001-source-generators.md) |
| net472 → net10.0 port introduces subtle API differences | Low | Low | Compile early (Phase 0), fix forward | [Q-0003](.ai/questions/Q-0003-net472-api-compat.md) |

## 13. Open Questions

| ID | Question | Status | Reference |
|---|---|---|---|
| Q-0001 | Does MSBuildWorkspace include source generator output? | Open | [.ai/questions/Q-0001-source-generators.md](.ai/questions/Q-0001-source-generators.md) |
| Q-0002 | Template discovery strategy for CLI | Resolved (glob) | [.ai/questions/Q-0002-template-discovery-cli.md](.ai/questions/Q-0002-template-discovery-cli.md) |
| Q-0003 | net472 API compatibility during port | Open (low risk) | [.ai/questions/Q-0003-net472-api-compat.md](.ai/questions/Q-0003-net472-api-compat.md) |
| Q-0004 | Watch mode for CLI | Deferred to v2.0 | [.ai/questions/Q-0004-watch-mode.md](.ai/questions/Q-0004-watch-mode.md) |

## 14. Appendix

### Key Upstream File References

**Generation Pipeline**:
| File | Class | Purpose |
|------|-------|---------|
| `origin/src/Typewriter/Generation/TemplateCodeParser.cs` | `TemplateCodeParser` | Template code extraction |
| `origin/src/Typewriter/TemplateEditor/Lexing/Roslyn/ShadowClass.cs` | `ShadowClass` | Compilable C# wrapper |
| `origin/src/Typewriter/Generation/Compiler.cs` | `Compiler` | Roslyn compilation |
| `origin/src/Typewriter/Generation/Parser.cs` | `Parser` | Template evaluation (per-file) |
| `origin/src/Typewriter/Generation/SingleFileParser.cs` | `SingleFileParser` | Template evaluation (merged) |
| `origin/src/Typewriter/Generation/ItemFilter.cs` | `ItemFilter` | Collection filtering |
| `origin/src/Typewriter/Generation/Template.cs` | `Template` | Output orchestration |

**Metadata Layer**:
| File | Class | Purpose |
|------|-------|---------|
| `origin/src/Roslyn/RoslynMetadataProvider.cs` | `RoslynMetadataProvider` | Entry point (must rewrite) |
| `origin/src/Roslyn/RoslynFileMetadata.cs` | `RoslynFileMetadata` | Document → SemanticModel |
| `origin/src/Roslyn/RoslynClassMetadata.cs` | `RoslynClassMetadata` | Class symbol wrapper |
| `origin/src/Roslyn/RoslynTypeMetadata.cs` | `RoslynTypeMetadata` | Type resolution |
| `origin/src/Roslyn/RoslynMethodMetadata.cs` | `RoslynMethodMetadata` | Method symbol wrapper |
| `origin/src/Roslyn/RoslynPropertyMetadata.cs` | `RoslynPropertyMetadata` | Property symbol wrapper |

**CodeModel & Configuration**:
| File | Class | Purpose |
|------|-------|---------|
| `origin/src/CodeModel/Configuration/Settings.cs` | `Settings` | Abstract settings (reuse) |
| `origin/src/Typewriter/CodeModel/Configuration/SettingsImpl.cs` | `SettingsImpl` | VS settings (rewrite) |
| `origin/src/Typewriter/CodeModel/Configuration/ProjectHelpers.cs` | `ProjectHelpers` | DTE project nav (rewrite) |
| `origin/src/Typewriter/CodeModel/Implementation/ClassImpl.cs` | `ClassImpl` | Metadata→CodeModel bridge |
| `origin/src/Typewriter/CodeModel/Helpers.cs` | `Helpers` | Type mapping (port) |

**VS Integration (replace/remove)**:
| File | Class | Purpose |
|------|-------|---------|
| `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs` | `ExtensionPackage` | VSIX entry (replace with CLI) |
| `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs` | `SolutionMonitor` | File events (remove) |
| `origin/src/Typewriter/Generation/Controllers/GenerationController.cs` | `GenerationController` | Event dispatch (remove) |
| `origin/src/Typewriter/VisualStudio/ErrorList.cs` | `ErrorList` | VS errors (replace) |
| `origin/src/Typewriter/VisualStudio/Log.cs` | `Log` | VS output (replace) |

### Decisions Summary
| ID | Decision | Reference |
|---|---|---|
| D-0001 | Target .NET 10 (LTS) | [.ai/decisions/D-0001-target-framework.md](.ai/decisions/D-0001-target-framework.md) |
| D-0002 | Package as dotnet global tool | [.ai/decisions/D-0002-packaging-strategy.md](.ai/decisions/D-0002-packaging-strategy.md) |
| D-0003 | MSBuildWorkspace hybrid for project loading | [.ai/decisions/D-0003-project-loading-strategy.md](.ai/decisions/D-0003-project-loading-strategy.md) |
