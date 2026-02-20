# F-0002: Visual Studio Dependency Map

**Date**: 2026-02-19
**Status**: Complete

## Context
Comprehensive inventory of all Visual Studio and Windows-specific dependencies, classified by severity and replacement strategy for cross-platform CLI.

## Evidence

### 1. Extension Package (Entry Point) — HARD
- **File**: `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs`
- **Class**: `ExtensionPackage : AsyncPackage`
- Registrations: `[PackageRegistration]`, `[ProvideAutoLoad]`, `[ProvideOptionPage]`, `[ProvideLanguageService]`, `[ProvideMenuResource]`
- **Services**: `DTE`, `IVsStatusbar`
- **Replacement**: CLI `Main()` entry point with `System.CommandLine`

### 2. Solution Monitor — HARD
- **File**: `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs`
- **Class**: `SolutionMonitor : IVsSolutionEvents, IVsRunningDocTableEvents3, IVsTrackProjectDocumentsEvents2`
- **Services**: `IVsSolution` (SVsSolution), `IVsRunningDocumentTable`, `IVsTrackProjectDocuments2`
- **Replacement**: Not needed in batch CLI; optional `--watch` via FileSystemWatcher

### 3. DTE Usage — HARD
- `ExtensionPackage.cs` — `DTE Dte` property
- `Template.cs` — `dte.SourceControl.CheckOutItem()` for TFS/TFVC
- `Log.cs` — `DTE.Windows.Item(Constants.OutputWindowName)` for output pane
- `SettingsImpl.cs` — `_projectItem.DTE.Solution.FullName` for solution path
- `ProjectHelpers.cs` — `DTE.Solution.AllProjects()`, `DTE.Solution.FindProjectItem()`
- **Replacement**: Direct file system access + MSBuild APIs for project enumeration

### 4. VisualStudioWorkspace — HARD (most critical)
- **File**: `origin/src/Roslyn/RoslynMetadataProvider.cs`
- **Code**: `ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel` → `componentModel.GetService<VisualStudioWorkspace>()`
- **Replacement**: `MSBuildWorkspace` or Buildalyzer+AdhocWorkspace

### 5. ThreadHelper / JoinableTaskFactory — HARD
- `RoslynMetadataProvider.cs` — `ThreadHelper.ThrowIfNotOnUIThread()`
- `RoslynFileMetadata.cs` — `ThreadHelper.JoinableTaskFactory.Run()`
- `SettingsImpl.cs` — `ThreadHelper.JoinableTaskFactory.Run()`
- `ProjectHelpers.cs` — `ThreadHelper.JoinableTaskFactory.Run()` (5 usages)
- **Replacement**: Standard `async/await` — no UI thread in CLI

### 6. ProjectItem (EnvDTE) — HARD
- Pervasive across generation pipeline: `Template.cs`, `TemplateCodeParser.cs`, `Compiler.cs`, `Parser.cs`, `SingleFileParser.cs`, `GenerationController.cs`, `TemplateController.cs`, `SettingsImpl.cs`, `ProjectHelpers.cs`, `PathResolver.cs`, `SolutionExtensions.cs`
- **Used for**: file paths (`.Properties.Item("FullPath")`), project hierarchy (`.ProjectItems`), solution navigation (`.DTE.Solution`), metadata storage (`.Properties["CustomToolNamespace"]`), source control (`.DTE.SourceControl`)
- **Replacement**: File path strings + MSBuild project graph for hierarchy

### 7. VS Options Page — HARD
- **File**: `origin/src/Typewriter/VisualStudio/TypewriterOptionsPage.cs`
- **Class**: `TypewriterOptionsPage : DialogPage`
- **Options**: `TrackSourceFiles`, `RenderOnSave`, `AddGeneratedFilesToProject`
- **Replacement**: CLI flags + optional JSON config file

### 8. Error List — HARD
- **File**: `origin/src/Typewriter/VisualStudio/ErrorList.cs`
- **Services**: `IVsSolution`, `IVsErrorList`, `ErrorListProvider`
- **Replacement**: Structured console diagnostics (MSBuild diagnostic format) + exit codes

### 9. VS Output Pane (Logging) — HARD
- **File**: `origin/src/Typewriter/VisualStudio/Log.cs`
- **Class**: `Log : ILog` — uses EnvDTE Output Window
- **Replacement**: `Microsoft.Extensions.Logging` → console

### 10. MEF Composition — SOFT
- Files: `LanguageService.cs`, `BraceMatchingController.cs`, `ClassificationController.cs`, `CompletionController.cs`, `CompletionSourceProvider.cs`, `FormattingController.cs`, `OutliningController.cs`, `QuickInfoController.cs`, `SignatureHelpController.cs`
- All editor features — not needed in CLI

### 11. Context Menu / VSCT — SOFT
- `RenderTemplate.cs`, `ExtensionPackage.vsct`
- **Replacement**: CLI commands

### 12. Template Editor — SOFT
- All files in `origin/src/Typewriter/TemplateEditor/` — syntax highlighting, IntelliSense, outlining
- **Not needed** in CLI

### 13. Source Control Integration — ACCIDENTAL
- `Template.cs` — `dte.SourceControl.CheckOutItem()` — TFS-specific, not needed for Git

### 14. COM / Windows-specific — HARD
- `VSLangProj.Reference` in `ProjectHelpers.cs` — COM interop for project references
- `IOleMessageFilter` in test infrastructure — COM message filter
- **Replacement**: MSBuild project references via `Microsoft.Build` APIs

## Classification Summary

| Dependency | Classification | Cross-platform Impact | CLI Replacement |
|------------|---------------|----------------------|-----------------|
| AsyncPackage / VSIX | Hard | Windows-only | CLI Main() + System.CommandLine |
| SolutionMonitor | Hard | Windows-only | Batch mode; optional --watch |
| DTE / EnvDTE | Hard | Windows-only, COM | File system + MSBuild APIs |
| VisualStudioWorkspace | Hard | Windows-only | MSBuildWorkspace (cross-platform) |
| ThreadHelper / JTF | Hard | Windows-only | Standard async/await |
| ProjectItem | Hard | Windows-only, COM | File paths + MSBuild graph |
| TypewriterOptionsPage | Hard | Windows-only | CLI args + JSON config |
| ErrorList / IVsErrorList | Hard | Windows-only | Console diagnostics |
| Log (Output Window) | Hard | Windows-only | Microsoft.Extensions.Logging |
| MEF (editor features) | Soft | N/A | Not needed |
| Context menus / VSCT | Soft | N/A | CLI commands |
| Template editor | Soft | N/A | Not needed |
| Source control (TFS) | Accidental | Windows-only | Remove |
| VSLangProj (COM) | Hard | Windows-only | MSBuild APIs |

## Conclusion
There are **10 hard dependencies** on VS/Windows surfaces. The most architecturally significant is `VisualStudioWorkspace` — it determines the entire project loading strategy. `ProjectItem` is the most pervasive — it threads through the entire generation pipeline and must be replaced with a simpler abstraction. All soft dependencies (editor, MEF) are purely UI and can be dropped entirely.

## Impact
- CLI can reuse ~60% of upstream code (CodeModel, Metadata interfaces, Roslyn*Metadata classes, Parser, ItemFilter)
- ~40% needs replacement (VS integration, ProjectItem plumbing, workspace provider)
- Cross-platform: No COM, no registry, no Windows-specific APIs in the CLI

## Next Steps
- D-0003: Loading strategy decision
- F-0007: ProjectItem replacement design
