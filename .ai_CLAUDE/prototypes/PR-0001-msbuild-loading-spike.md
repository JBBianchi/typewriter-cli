# PR-0001: MSBuild Loading Spike

**Date**: 2026-02-19
**Status**: Complete (design-only)

## Context
Evaluate MSBuild-based project loading approaches for the CLI, with focus on cross-platform behavior, restore requirements, and semantic model fidelity.

## Spike Design: MSBuildWorkspace Approach

### Minimal Loading Sequence
```csharp
// 1. Register MSBuild (MUST be before any MSBuild API usage)
MSBuildLocator.RegisterDefaults();

// 2. Create workspace
using var workspace = MSBuildWorkspace.Create();
workspace.WorkspaceFailed += (sender, args) =>
    logger.LogWarning("Workspace: {Kind} {Message}", args.Diagnostic.Kind, args.Diagnostic.Message);

// 3a. Open solution
var solution = await workspace.OpenSolutionAsync("path/to/solution.sln");
foreach (var project in solution.Projects)
{
    foreach (var document in project.Documents)
    {
        var semanticModel = await document.GetSemanticModelAsync();
        // → Feed to RoslynFileMetadata
    }
}

// 3b. Or open single project
var project = await workspace.OpenProjectAsync("path/to/project.csproj");
```

### MSBuildLocator Behavior
- `RegisterDefaults()` — finds latest installed .NET SDK
- Respects `global.json` if present in directory hierarchy
- Cross-platform: works on Linux/macOS/Windows
- Must be called before ANY `Microsoft.Build` type is loaded (assembly load order matters)
- Alternative: `RegisterInstance(MSBuildLocator.QueryVisualStudioInstances().First(i => ...))` for explicit SDK selection

### Multi-targeting Handling
```csharp
// For multi-targeted projects, MSBuildWorkspace needs a specific TFM
var properties = new Dictionary<string, string>
{
    ["TargetFramework"] = selectedTfm // e.g., "net10.0"
};
using var workspace = MSBuildWorkspace.Create(properties);
```

If no TFM specified:
1. Parse `.csproj` for `TargetFramework` (single) or `TargetFrameworks` (multi)
2. If multi: use first TFM, or let user specify via `--framework`

### Restore Detection
```csharp
// Check if restore is needed before opening workspace
var assetsPath = Path.Combine(projectDir, "obj", "project.assets.json");
if (!File.Exists(assetsPath))
{
    if (autoRestore)
    {
        // Run dotnet restore
        var process = Process.Start("dotnet", $"restore \"{projectPath}\"");
        await process.WaitForExitAsync();
    }
    else
    {
        logger.LogError("NuGet packages not restored. Run 'dotnet restore' first or use --restore flag.");
        return ExitCode.LoadError; // 3
    }
}
```

### .slnx Support Strategy
`.slnx` is an XML format (simpler than `.sln` text format):
```xml
<Solution>
  <Project Path="src/MyProject/MyProject.csproj" />
  <Project Path="src/OtherProject/OtherProject.csproj" />
</Solution>
```

Parsing approach:
```csharp
if (inputPath.EndsWith(".slnx"))
{
    var doc = XDocument.Load(inputPath);
    var projectPaths = doc.Root.Elements("Project")
        .Select(e => e.Attribute("Path").Value)
        .Select(p => Path.GetFullPath(Path.Combine(solutionDir, p)));

    foreach (var projectPath in projectPaths)
    {
        await workspace.OpenProjectAsync(projectPath);
    }
}
```

### Cross-platform Considerations
| Aspect | Linux/macOS | Windows |
|--------|-------------|---------|
| MSBuildLocator | .NET SDK only | .NET SDK + VS instances |
| Path separators | `/` | `\` (normalize with Path.Combine) |
| Case sensitivity | Filesystem is case-sensitive | Case-insensitive |
| Long paths | No 260-char limit | May need `\\?\` prefix (net10.0 handles this) |
| Line endings | LF | CRLF (normalize in output) |
| global.json | Supported | Supported |

### Performance Considerations
- `OpenSolutionAsync()` evaluates all projects sequentially — can be slow for large solutions
- Mitigation: Load only projects that contain `.tst` templates (filter by project graph)
- Alternative: Parallel project loading with separate workspace instances (but higher memory)
- Caching: Consider persisting project.assets.json check results

### Known Limitations
1. **Analyzers**: MSBuildWorkspace may load Roslyn analyzers that produce warnings — suppress via workspace properties
2. **Source generators**: Generated files may not be available — need `project.GetCompilationAsync()` to include them
3. **Conditional compilation**: Must specify `Configuration` (default: `Debug`)
4. **Design-time build**: MSBuildWorkspace does a design-time build, not a full build

### Error Handling Patterns
```csharp
workspace.WorkspaceFailed += (_, args) =>
{
    switch (args.Diagnostic.Kind)
    {
        case WorkspaceDiagnosticKind.Warning:
            logger.LogWarning(args.Diagnostic.Message);
            break;
        case WorkspaceDiagnosticKind.Failure:
            logger.LogError(args.Diagnostic.Message);
            break;
    }
};
```

## Conclusion
MSBuildWorkspace is the right primary choice. It provides:
- Direct `Document` → `SemanticModel` (no intermediate lossy step)
- Cross-platform via MSBuildLocator
- Full symbol fidelity
- Native `.sln` support

Custom work needed for: `.slnx`, restore detection, multi-TFM selection, performance optimization.

## Tradeoffs Accepted
- Requires .NET SDK on target machine (acceptable for a dotnet tool)
- `.slnx` requires custom parsing (~50 lines of code)
- Restore must happen before load (standard .NET development practice)
- Large solutions may be slow (mitigate by filtering to relevant projects)

## Evidence
- MSBuildWorkspace is used by OmniSharp, Roslynator, and other successful CLI tools
- `Microsoft.CodeAnalysis.Workspaces.MSBuild` v4.14.0 supports net10.0
- `Microsoft.Build.Locator` v1.7+ handles global.json and cross-platform SDK resolution
