# F-0002: Extension Entrypoint and Host Lifecycle

- ID: F-0002
- Title: Visual Studio package is the runtime host and orchestrator
- Date: 2026-02-19

## Context
We need to identify upstream entrypoints and host lifecycle assumptions to replace them with a deterministic CLI entrypoint.

## Evidence
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:28` class `ExtensionPackage : AsyncPackage, IDisposable`.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:20` `[ProvideOptionPage]` registration.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:22` `[PackageRegistration(...AllowsBackgroundLoading = true)]`.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:23` `[ProvideAutoLoad(...SolutionExists...)]`.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:25` `[ProvideLanguageService]`.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:27` `[ProvideMenuResource]`.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:53` method `InitializeAsync(...)` is the startup path.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:63` explicit `JoinableTaskFactory.SwitchToMainThreadAsync(...)`.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:65`/`:66` resolves DTE and status bar services.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:71`/`:72`/`:73`/`:74` constructs `SolutionMonitor`, `TemplateController`, `EventQueue`, `GenerationController`.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:89` method `WireUpEvents()` binds solution/document events.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:102`/`:107`/`:113` hooks solution monitor and document save handlers.

## Conclusion
The extension package is both composition root and runtime host, and it depends on VS package initialization, VS service resolution, and UI-thread-affined event wiring.

## Impact
- CLI must replace `InitializeAsync` and event-driven runtime with explicit command execution flow.
- CLI cannot depend on DTE services or VS UI thread semantics.
- Trigger models become explicit (`generate` command inputs/flags) rather than auto-load event hooks.

## Next steps
- Map every host dependency to CLI replacements (`F-0009` to `F-0013`).
- Define CLI orchestration module and startup order in implementation plan.
