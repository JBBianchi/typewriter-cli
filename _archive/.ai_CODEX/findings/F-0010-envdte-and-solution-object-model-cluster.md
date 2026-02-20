# F-0010: EnvDTE and Solution Object Model Dependency Cluster

- ID: F-0010
- Title: DTE-based project/file traversal and mutation dependencies
- Date: 2026-02-19

## Context
EnvDTE usage is one of the largest coupling points. We must classify and replace it for CLI.

## Evidence
- `origin/src/Typewriter/Generation/Controllers/SolutionExtensions.cs:32` `AllProjects(this Solution solution)` traverses DTE solution hierarchy.
- `origin/src/Typewriter/Generation/Controllers/SolutionExtensions.cs:41` `AllProjectItems(this Project project, string extension)` traverses DTE project items recursively.
- `origin/src/Typewriter/Generation/Controllers/TemplateController.cs:86` template discovery depends on `Solution.AllProjects().AllProjectItems(...)`.
- `origin/src/Typewriter/CodeModel/Configuration/ProjectHelpers.cs:20` project lookup by `project.Name` over DTE projects.
- `origin/src/Typewriter/CodeModel/Configuration/ProjectHelpers.cs:55` referenced project traversal via `VSLangProj.VSProject.References`.
- `origin/src/Typewriter/CodeModel/Configuration/ProjectHelpers.cs:105` `ProjectListContainsItem(...)` uses `Dte.Solution.FindProjectItem`.
- `origin/src/Typewriter/Generation/Template.cs:96` included project filtering uses DTE project full names.
- `origin/src/Typewriter/Generation/Template.cs:233` generated file added to project via `ProjectItems.AddFromFile`.
- `origin/src/Typewriter/Generation/Template.cs:317` source mapping stored in project item property `CustomToolNamespace`.
- `origin/src/Typewriter/Generation/Template.cs:500` source control checkout through `dte.SourceControl`.

## Conclusion
Core discovery, include logic, and output integration are tightly coupled to DTE project/item APIs.

## Impact
- Classification:
  - Hard: Template discovery, include-project resolution, referenced-project traversal.
  - Soft: Source control checkout and project-item mutation from generation.
  - Accidental: `CustomToolNamespace` mapping for generated/source linkage (UI/project convenience, not generation core).
- CLI replacement:
  - Use `Microsoft.Build.Graph.ProjectGraph` (and evaluated items) for project and file traversal.
  - Replace project-name include resolution with graph-based project identity mapping.
  - Make project mutation/source-control operations optional or omit in CLI scope.

## Next steps
- Define graph-based abstraction for project/file inventory.
- Add open question for project-name collisions and include-project semantics in CLI.
