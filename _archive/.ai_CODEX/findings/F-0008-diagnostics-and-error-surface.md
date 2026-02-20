# F-0008: Diagnostics and Error Surface

- ID: F-0008
- Title: Diagnostics are Visual Studio UI surfaces (Output window, Error List, status bar)
- Date: 2026-02-19

## Context
CLI needs deterministic, machine-readable diagnostics and exit codes; upstream diagnostics are VS UI-integrated.

## Evidence
- `origin/src/Typewriter/VisualStudio/Log.cs:9` class `Log`.
- `origin/src/Typewriter/VisualStudio/Log.cs:104` writes to DTE output window (`vsWindowKindOutput`).
- `origin/src/Typewriter/VisualStudio/ErrorList.cs:8` static class `ErrorList`.
- `origin/src/Typewriter/VisualStudio/ErrorList.cs:19`/`:20` resolves `IVsSolution` and `SVsErrorList`.
- `origin/src/Typewriter/VisualStudio/ErrorList.cs:95` creates `ErrorTask` entries for navigation.
- `origin/src/Typewriter/Generation/Compiler.cs:62` maps Roslyn diagnostics to ErrorList warnings/errors.
- `origin/src/Typewriter/Generation/Compiler.cs:87` calls `ErrorList.Show()` on compile errors.
- `origin/src/Typewriter/Generation/Parser.cs:225` parser errors logged and sent to ErrorList.
- `origin/src/Typewriter/Generation/SingleFileParser.cs:275` same error pattern for single-file parser.
- `origin/src/Typewriter/Generation/Controllers/EventQueue.cs:19` `EventQueue` uses `IVsStatusbar`.
- `origin/src/Typewriter/Generation/Controllers/EventQueue.cs:94` status text `Rendering template...`.
- `origin/src/Typewriter/Generation/Controllers/EventQueue.cs:112` status text `Rendering complete`.

## Conclusion
Current diagnostics are designed for interactive Visual Studio UX and not for CI/CLI consumption.

## Impact
- CLI must replace these with console and structured logs (verbosity levels).
- Error categories must map to deterministic exit codes and optional `--fail-on-warnings`.
- Source location and template context in diagnostics should be retained for parity quality.

## Next steps
- Define CLI logger and diagnostic DTOs in target architecture.
- Include snapshot/golden diagnostic tests for template compile errors and parse/runtime errors.
