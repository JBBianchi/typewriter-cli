# F-0006: Configuration Sources and Precedence

- ID: F-0006
- Title: Template settings API and VS options as configuration sources
- Date: 2026-02-19

## Context
CLI design needs concrete configuration precedence and source mapping.

## Evidence
- `origin/src/CodeModel/Configuration/Settings.cs:10` abstract `Settings` defines configuration surface.
- `origin/src/CodeModel/Configuration/Settings.cs:15` `OutputExtension` default `.ts`.
- `origin/src/CodeModel/Configuration/Settings.cs:22` `OutputFilenameFactory`.
- `origin/src/CodeModel/Configuration/Settings.cs:27` `PartialRenderingMode` default `Partial`.
- `origin/src/CodeModel/Configuration/Settings.cs:37` `OutputDirectory`.
- `origin/src/CodeModel/Configuration/Settings.cs:42` `SkipAddingGeneratedFilesToProject`.
- `origin/src/CodeModel/Configuration/Settings.cs:95` `IncludeProject(...)`.
- `origin/src/CodeModel/Configuration/Settings.cs:103` `SingleFileMode(...)`.
- `origin/src/CodeModel/Configuration/Settings.cs:113` `IncludeReferencedProjects(...)`.
- `origin/src/CodeModel/Configuration/Settings.cs:119` `IncludeAllProjects(...)`.
- `origin/src/CodeModel/Configuration/Settings.cs:136` `DisableStrictNullGeneration()`.
- `origin/src/CodeModel/Configuration/Settings.cs:142` `DisableUtf8BomGeneration()`.
- `origin/src/Typewriter/CodeModel/Configuration/SettingsImpl.cs:21` concrete `SettingsImpl` implementation.
- `origin/src/Typewriter/CodeModel/Configuration/SettingsImpl.cs:31` lazy default include behavior: current + referenced projects.
- `origin/src/Typewriter/CodeModel/Configuration/SettingsImpl.cs:107` `IncludeProject` resolved via `ProjectHelpers.AddProject`.
- `origin/src/Typewriter/VisualStudio/TypewriterOptionsPage.cs:7` VS options page defines:
  - `TrackSourceFiles` (`:13`)
  - `RenderOnSave` (`:19`)
  - `AddGeneratedFilesToProject` (`:25`)
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:167` `GetOptions()` binds options to runtime flags.

## Conclusion
Configuration comes from two channels:
1) template-level `Settings` code in `.tst` templates, and
2) global VS options affecting runtime triggers and project mutation behavior.

## Impact
- CLI needs explicit CLI flags and optional config file equivalents for VS option values.
- Template `Settings` API semantics must remain available and authoritative for generation behavior.
- Include-project behavior currently depends on DTE project names and references; replacement logic is required for graph-based loading.

## Next steps
- Define CLI precedence model in implementation plan: CLI args > template settings where applicable > defaults.
- Raise open question for project-name collisions and include resolution policy.
