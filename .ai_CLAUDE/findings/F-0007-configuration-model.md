# F-0007: Configuration Model and Settings

**Date**: 2026-02-19
**Status**: Complete

## Context
How upstream configures generation behavior, and what must change for CLI.

## Evidence

### Settings Abstract Class
- **File**: `origin/src/CodeModel/Configuration/Settings.cs`
- **Properties** (non-abstract, settable):
  - `OutputExtension` (string, default `.ts`)
  - `OutputFilenameFactory` (Func<File, string>)
  - `PartialRenderingMode` (enum: Partial | Combined, default Partial)
  - `OutputDirectory` (string)
  - `SkipAddingGeneratedFilesToProject` (bool)
- **Properties** (abstract — VS-dependent in `SettingsImpl`):
  - `SolutionFullName` — from `_projectItem.DTE.Solution.FullName`
  - `IsSingleFileMode`, `SingleFileName`
  - `StringLiteralCharacter` (default `"`)
  - `StrictNullGeneration` (default `true`)
  - `Utf8BomGeneration` (default `true`)
  - `TemplatePath`, `Log`
- **Methods** (abstract):
  - `IncludeProject(string projectName)` — add named project
  - `IncludeCurrentProject()` — add containing project
  - `IncludeReferencedProjects()` — add referenced projects
  - `IncludeAllProjects()` — add all solution projects
  - `SingleFileMode(string singleFilename)`
  - `UseStringLiteralCharacter(char ch)`
  - `DisableStrictNullGeneration()`
  - `DisableUtf8BomGeneration()`

### SettingsImpl (VS-dependent)
- **File**: `origin/src/Typewriter/CodeModel/Configuration/SettingsImpl.cs`
- **Constructor**: `SettingsImpl(ILog log, ProjectItem projectItem, string templatePath)`
- **VS dependencies**:
  - `_projectItem.DTE.Solution.FullName` → `SolutionFullName`
  - `_projectItem.DTE` → `ProjectHelpers.AddAllProjects()`
  - `ProjectHelpers.AddProject(projectItem, ...)` — uses DTE to find projects by name
  - `ProjectHelpers.AddCurrentProject(...)` — uses `ContainingProject.Object as VSProject` (COM)
  - `ProjectHelpers.AddReferencedProjects(...)` — uses `VSProject.References` (COM)
- **Default behavior**: If no `Include*` called, defaults to `IncludeCurrentProject() + IncludeReferencedProjects()`

### ProjectHelpers (VS-heavy)
- **File**: `origin/src/Typewriter/CodeModel/Configuration/ProjectHelpers.cs`
- All methods use `ThreadHelper.JoinableTaskFactory.Run()` + `SwitchToMainThreadAsync()`
- Key methods:
  - `AddProject(projectItem, list, name)` — `DTE.Solution.AllProjects()` to find by name
  - `AddCurrentProject(list, projectItem)` — `projectItem.ContainingProject`
  - `AddReferencedProjects(list, projectItem)` — `VSProject.References` iteration
  - `AddAllProjects(dte, list)` — enumerate all solution projects
  - `GetProjectItems(dte, list, filter)` — find `.cs` files in project directories
  - `ProjectListContainsItem(dte, filename, list)` — check if file is in included projects

### ContextAttribute
- **File**: `origin/src/CodeModel/Attributes/ContextAttribute.cs`
- Marks CodeModel classes for template context registration
- Properties: `Name` (e.g., "Class"), `CollectionName` (e.g., "Classes")
- Used by `Contexts` class to build identifier registry for template IntelliSense and resolution

### Configuration Sources (How Templates Configure Generation)
Templates use `${ }` code blocks to configure generation:
```
${ Settings.IncludeCurrentProject(); }
${ Settings.SingleFileMode("models.ts"); }
${ Settings.OutputExtension = ".tsx"; }
${ Settings.OutputFilenameFactory = file => file.Classes.First().name + ".generated.ts"; }
```
These execute at template compilation time (Step 3 in pipeline).

### CLI Configuration Requirements
For CLI, Settings needs:
1. `SolutionFullName` → provided by CLI args (input path)
2. Project scoping (`IncludeProject`, `IncludeCurrentProject`, etc.) → from MSBuild project graph
3. `IncludeReferencedProjects` → MSBuild `ProjectReference` items
4. `IncludeAllProjects` → all projects in solution
5. `GetProjectItems(filter)` → file system glob within project directories
6. `ProjectListContainsItem` → check against MSBuild project items

## Conclusion
The `Settings` abstract class is VS-independent and reusable. `SettingsImpl` and `ProjectHelpers` are heavily VS-coupled and need complete reimplementation. The CLI version (`CliSettingsImpl`) must use MSBuild project graph for project scoping instead of DTE.

## Impact
- `Settings` abstract class: **reuse** (port to .NET 10)
- `SettingsImpl`: **rewrite** as `CliSettingsImpl` — constructor takes solution path + project graph
- `ProjectHelpers`: **rewrite** — use MSBuild `Project.GetItems("ProjectReference")` instead of DTE/COM
- Template `${ }` code blocks: **unchanged** — same Settings API surface
- `ContextAttribute`: **reuse** as-is

## Next Steps
- Design CliSettingsImpl in architecture section
