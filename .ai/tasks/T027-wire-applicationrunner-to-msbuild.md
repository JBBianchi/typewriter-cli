# T027: Wire ApplicationRunner to MSBuild Loading Services
- Milestone: M3
- Status: Done
- Agent: Claude (Executor)
- Started: 2026-03-02
- Completed: 2026-03-02

## Objective
Replace the validation-only stub in `ApplicationRunner.RunAsync()` with real calls to the MSBuild loading pipeline: `IInputResolver` → `IRestoreService` → `IProjectGraphService`. Map loader errors to exit code 3.

## Approach

To wire the loading pipeline into `ApplicationRunner` without creating a circular dependency (since `Typewriter.Loading.MSBuild` already references `Typewriter.Application`), the service interfaces were moved to `Typewriter.Application.Loading`:

- `IInputResolver`, `IRestoreService`, `IProjectGraphService`, `IMsBuildLocatorService`, `ResolvedInput` moved from `Typewriter.Loading.MSBuild` → `Typewriter.Application/Loading/` (namespace `Typewriter.Application.Loading`)
- Implementations in `Typewriter.Loading.MSBuild` updated to `using Typewriter.Application.Loading;`
- `ApplicationRunner` now accepts interfaces via constructor injection
- `Typewriter.Cli/Program.cs` creates the concrete implementations and injects them
- `Typewriter.Cli.csproj` gains a reference to `Typewriter.Loading.MSBuild`

## Journey
### 2026-03-02
- Read AGENTS.md and progress.md to orient
- Identified circular dependency: `Loading.MSBuild → Application`, so `Application` cannot reference `Loading.MSBuild` directly
- Solution: move the service interfaces (abstractions) to `Typewriter.Application.Loading` — Dependency Inversion Principle
- Created 5 new files in `src/Typewriter.Application/Loading/`: `ResolvedInput.cs`, `IInputResolver.cs`, `IRestoreService.cs`, `IProjectGraphService.cs`, `IMsBuildLocatorService.cs`
- Deleted old interface/DTO files from `Typewriter.Loading.MSBuild/` (5 files removed)
- Updated implementations: added `using Typewriter.Application.Loading;` to `InputResolver.cs`, `RestoreService.cs`, `ProjectGraphService.cs`, `MsBuildLocatorService.cs`
- Rewrote `ApplicationRunner.cs` with constructor injection and full pipeline: resolve → check assets → restore if needed → build graph
- Updated `Program.cs` to create concrete services and inject into `ApplicationRunner`
- Added `Typewriter.Loading.MSBuild` project reference to `Typewriter.Cli.csproj`
- Updated `CliContractTests.cs` to use stub implementations (tests bypass file system)
- Build: 0 errors, 0 warnings; 129/129 tests pass

## Outcome
Files changed:
- **NEW**: `src/Typewriter.Application/Loading/ResolvedInput.cs`
- **NEW**: `src/Typewriter.Application/Loading/IInputResolver.cs`
- **NEW**: `src/Typewriter.Application/Loading/IRestoreService.cs`
- **NEW**: `src/Typewriter.Application/Loading/IProjectGraphService.cs`
- **NEW**: `src/Typewriter.Application/Loading/IMsBuildLocatorService.cs`
- **DELETED**: `src/Typewriter.Loading.MSBuild/ResolvedInput.cs`
- **DELETED**: `src/Typewriter.Loading.MSBuild/IInputResolver.cs`
- **DELETED**: `src/Typewriter.Loading.MSBuild/IRestoreService.cs`
- **DELETED**: `src/Typewriter.Loading.MSBuild/IProjectGraphService.cs`
- **DELETED**: `src/Typewriter.Loading.MSBuild/IMsBuildLocatorService.cs`
- **MODIFIED**: `src/Typewriter.Loading.MSBuild/InputResolver.cs` (added `using Typewriter.Application.Loading;`)
- **MODIFIED**: `src/Typewriter.Loading.MSBuild/RestoreService.cs` (added `using Typewriter.Application.Loading;`)
- **MODIFIED**: `src/Typewriter.Loading.MSBuild/ProjectGraphService.cs` (added `using Typewriter.Application.Loading;`)
- **MODIFIED**: `src/Typewriter.Loading.MSBuild/MsBuildLocatorService.cs` (added `using Typewriter.Application.Loading;`)
- **MODIFIED**: `src/Typewriter.Application/ApplicationRunner.cs` (full pipeline, constructor injection)
- **MODIFIED**: `src/Typewriter.Cli/Program.cs` (compose services, inject into runner)
- **MODIFIED**: `src/Typewriter.Cli/Typewriter.Cli.csproj` (added Loading.MSBuild reference)
- **MODIFIED**: `tests/Typewriter.UnitTests/Cli/CliContractTests.cs` (stub services for isolation)

## Follow-ups
- T029+ (remaining M3 tasks): integrate `ProjectLoadPlan` into the metadata extraction stage (M5)
