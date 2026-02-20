# F-0003: Generation Pipeline Orchestration

- ID: F-0003
- Title: Event-driven generation controller with queued rendering actions
- Date: 2026-02-19

## Context
Parity requires understanding exactly how generation is triggered and processed across template changes, C# changes, deletes, and renames.

## Evidence
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:13` class `GenerationController`.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:28` method `OnTemplateChanged(string templatePath, bool force = false)`.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:41` respects option `RenderOnSave`.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:50` hard-coded `Task.Delay(1000)` before enqueue.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:57` single-file mode branch (`template.Settings.IsSingleFileMode`).
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:111` method `OnCsFileChanged(string[] paths)`.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:113` respects option `TrackSourceFiles`.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:171` method `OnCsFileDeleted(string[] paths)`.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:219` method `OnCsFileRenamed(string[] newPaths, string[] oldPaths)`.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:278` generic `Enqueue<T>(...)` applies action across templates and changed paths.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:301` groups by project and saves project file once per project.
- `origin/src/Typewriter/Generation/Controllers/EventQueue.cs:17` class `EventQueue`.
- `origin/src/Typewriter/Generation/Controllers/EventQueue.cs:31` method `ProcessQueue()` serializes actions.
- `origin/src/Typewriter/Generation/Controllers/EventQueue.cs:47` queue coalescing/flush behavior while pending actions remain.

## Conclusion
Generation is an event-queued, serial, mutation-oriented pipeline with explicit handling for file lifecycle events and single-file mode.

## Impact
- CLI can simplify triggers (explicit run) but must preserve rendering semantics per event type.
- Queue semantics should become deterministic batched processing in one command invocation.
- The 1-second Roslyn refresh waits are host-workaround behavior; CLI should avoid fixed delays by using controlled compilation snapshots.

## Next steps
- Capture template loading and rendering behavior details (`F-0004`, `F-0005`).
- Define CLI equivalent event/batch model and deterministic ordering.
