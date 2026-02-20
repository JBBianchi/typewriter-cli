# F-0005: Output Path and File I/O Rules

- ID: F-0005
- Title: Output naming, write behavior, project mutation, and long path handling
- Date: 2026-02-19

## Context
CLI parity requires exact output path selection and file-write semantics, including overwrite behavior and project mutation behavior.

## Evidence
- `origin/src/Typewriter/Generation/Template.cs:198` method `SaveFile(...)` computes output path and writes only on content change.
- `origin/src/Typewriter/Generation/Template.cs:203` prevents source/output path collision.
- `origin/src/Typewriter/Generation/Template.cs:210`/`:460` `HasChanged(...)` guards unnecessary writes.
- `origin/src/Typewriter/Generation/Template.cs:212` honors `AddGeneratedFilesToProject` and `Settings.SkipAddingGeneratedFilesToProject`.
- `origin/src/Typewriter/Generation/Template.cs:233` adds generated file to project (`ProjectItems.AddFromFile(...)`) when enabled.
- `origin/src/Typewriter/Generation/Template.cs:246` persists source mapping via `CustomToolNamespace` property.
- `origin/src/Typewriter/Generation/Template.cs:249` method `DeleteFile(...)` deletes mapped generated project item.
- `origin/src/Typewriter/Generation/Template.cs:255` method `RenameFile(...)` updates mapped item on source rename.
- `origin/src/Typewriter/Generation/Template.cs:352` method `GetOutputPath(...)` resolves collisions using suffix ` (i)` up to 999.
- `origin/src/Typewriter/Generation/Template.cs:387` method `GetOutputDirectory(...)` supports relative path rooted at template directory and creates missing directory.
- `origin/src/Typewriter/Generation/Template.cs:410` method `GetOutputFilename(...)` supports filename factory and illegal-character replacement.
- `origin/src/Typewriter/Generation/Template.cs:448` method `GetOutputExtension()` defaults to `.ts`.
- `origin/src/Typewriter/Generation/Template.cs:168`/`:187` method `WriteFile(...)` uses UTF-8 with configurable BOM (`Settings.Utf8BomGeneration`).
- `origin/src/Typewriter/Generation/Template.cs:555` method `IsLongPathEnabled()` checks `HKLM\...\LongPathsEnabled`.
- `origin/src/Typewriter/Generation/Template.cs:173` applies `\\?\` long-path prefix only when registry flag is enabled.

## Conclusion
Upstream output behavior includes deterministic path computation, change detection, optional project mutation, source-file mapping metadata, and Windows-specific long-path logic.

## Impact
- CLI must preserve filename/output directory/factory semantics and no-op on unchanged content.
- VS project mutation (`AddFromFile`, `CustomToolNamespace`, source control checkout) is not portable and should be CLI parity gap/conditional behavior.
- Long-path handling must be implemented cross-platform without Windows registry dependency.

## Next steps
- Mark project-mutation features as conditional/parity gap candidates.
- Add golden tests for output path collision, BOM settings, and unchanged-content behavior.
