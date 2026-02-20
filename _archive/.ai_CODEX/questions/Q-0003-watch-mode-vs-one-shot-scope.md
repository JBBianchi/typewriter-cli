# Q-0003: Watch Mode vs One-shot Scope

- ID: Q-0003
- Title: Should file-watch/event-driven behavior be in initial CLI scope?
- Date: 2026-02-19

## Context
Upstream behavior is event-driven (document save/add/remove/rename) inside Visual Studio.

## Evidence
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:113` reacts to document save events.
- `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs:269` handles file add events.
- `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs:281` handles file remove events.
- `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs:293` handles file rename events.

## Conclusion
A scope decision is needed between:
- initial deterministic one-shot generation command, and
- adding a watch mode that emulates IDE event behavior.

## Impact
- One-shot mode reduces risk and simplifies CI reliability.
- Watch mode improves local DX but adds cross-platform file-system event complexity.

## Next steps
- Decide whether watch mode is deferred to post-v1 milestone.
