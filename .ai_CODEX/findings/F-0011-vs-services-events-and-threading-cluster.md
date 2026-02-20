# F-0011: VS Services, Events, and Threading Dependency Cluster

- ID: F-0011
- Title: IVs services, solution/document events, and UI-thread assumptions
- Date: 2026-02-19

## Context
The extension runtime relies on VS event sources and JTF/UI-thread behavior that has no CLI equivalent.

## Evidence
- `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs:11` implements `IVsSolutionEvents`, `IVsRunningDocTableEvents3`, `IVsTrackProjectDocumentsEvents2`.
- `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs:68` gets `SVsSolution`.
- `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs:86` gets `SVsRunningDocumentTable`.
- `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs:104` gets `SVsTrackProjectDocuments`.
- `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs:217` reacts to document save (`OnAfterSave`).
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:89` `WireUpEvents()` binds DTE and solution monitor events.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:113` document saved event routes `.cs` and `.tst` changes into generation controller.
- `origin/src/Typewriter/Generation/Controllers/EventQueue.cs:81` and `:106` enter UI thread for status bar updates.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:63` and many other files rely on `JoinableTaskFactory`/`ThreadHelper`.

## Conclusion
The runtime model is an event-driven VS host reacting to IDE events and requiring UI-thread transitions.

## Impact
- Classification:
  - Hard: `IVs*` event sources and service retrieval for runtime triggers.
  - Soft: status bar UX updates and other interactive IDE feedback.
  - Accidental: fixed 1-second delays to wait for VS Roslyn workspace refresh.
- CLI replacement:
  - Replace event-driven runtime with explicit command invocation (`typewriter-cli generate ...`).
  - Optionally add file watcher mode later as a non-core feature.
  - Remove UI-thread constraints; keep deterministic processing order.

## Next steps
- Specify CLI orchestration lifecycle and trigger model in implementation plan.
- Keep watch mode (if any) as later phase to avoid initial complexity.
