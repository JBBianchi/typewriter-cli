# T014: Implement IDiagnosticReporter and TW code catalog in Diagnostics/
- Milestone: M2
- Status: Done
- Agent: Claude (Executor)
- Started: 2026-03-02
- Completed: 2026-03-02

## Objective
Create the full diagnostic infrastructure in `src/Typewriter.Application/Diagnostics/`: severity enum, TW code constants, diagnostic message record, reporter interface, and MSBuild-format console reporter.

## Approach
1. Created `Diagnostics/` subfolder under `src/Typewriter.Application/` with `namespace Typewriter.Application.Diagnostics`.
2. Created five new files: `DiagnosticSeverity`, `DiagnosticCode`, `DiagnosticMessage`, `IDiagnosticReporter`, `MsBuildDiagnosticReporter`.
3. Removed old `IDiagnosticReporter.cs` stub from `src/Typewriter.Application/` (had different interface shape from T013 stub).
4. Removed old `ConsoleDiagnosticReporter.cs` from `src/Typewriter.Cli/` (superseded by `MsBuildDiagnosticReporter`).
5. Updated `ApplicationRunner.cs` to import `Typewriter.Application.Diagnostics`.
6. Updated `Program.cs` to use `MsBuildDiagnosticReporter` from new namespace.
7. Added `DiagnosticFormatTests.cs` to test project with 8 tests covering format, severity labels, location omission, and count tracking.

## Journey
### 2026-03-02
- T013 had left a stub `IDiagnosticReporter` with a different shape (ReportError/ReportWarning/ReportInfo methods, HasErrors/HasWarnings) vs the task-required shape (Report(DiagnosticMessage), WarningCount, ErrorCount).
- Chose `namespace Typewriter.Application.Diagnostics` to match folder structure; updated all callers.
- `MsBuildDiagnosticReporter.Format()` made `public static` to enable direct testing without Console capture.
- Test file initially missing `using Xunit;` (no implicit usings in this project) — added after first build failure.
- Final build: 0 errors, 0 warnings. All 111 unit tests pass (8 new: MsBuildStyleMessage_IsParseable + 4 theory cases + 3 fact cases).

## Outcome
Files created:
- `src/Typewriter.Application/Diagnostics/DiagnosticSeverity.cs`
- `src/Typewriter.Application/Diagnostics/DiagnosticCode.cs`
- `src/Typewriter.Application/Diagnostics/DiagnosticMessage.cs`
- `src/Typewriter.Application/Diagnostics/IDiagnosticReporter.cs`
- `src/Typewriter.Application/Diagnostics/MsBuildDiagnosticReporter.cs`
- `tests/Typewriter.UnitTests/Diagnostics/DiagnosticFormatTests.cs`

Files modified:
- `src/Typewriter.Application/ApplicationRunner.cs` — added `using Typewriter.Application.Diagnostics`
- `src/Typewriter.Cli/Program.cs` — replaced `ConsoleDiagnosticReporter` with `MsBuildDiagnosticReporter`

Files deleted:
- `src/Typewriter.Application/IDiagnosticReporter.cs` — old stub with different interface shape
- `src/Typewriter.Cli/ConsoleDiagnosticReporter.cs` — superseded by `MsBuildDiagnosticReporter`

Build: 0 errors, 0 warnings. Tests: 111/111 passed.

## Follow-ups
- T015/T016: Wire `IDiagnosticReporter` into full `ApplicationRunner` pipeline.
- T016: Implement `--fail-on-warnings` using `reporter.WarningCount > 0`.
