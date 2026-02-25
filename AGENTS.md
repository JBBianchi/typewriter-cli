# AGENTS.md - Typewriter Standalone (.NET 10 CLI)

Last updated: 2026-02-25

Metadata:
- Phase: Implementation
- Target: net10.0
- CLI Name: typewriter-cli

## 1) Mission

Deliver a cross-platform `typewriter-cli` with upstream Typewriter parity, without Visual Studio runtime dependencies.

Primary references (in priority order):
- `DETAILED_IMPLEMENTATION_PLAN.md` (source of truth for architecture and milestones)
- `README.md` (public contract summary)
- `origin/` (read-only upstream behavior reference)

## 2) Hard Rules (MUST / MUST NOT)

1. MUST NOT edit `origin/` (content, formatting, or commits).
2. MUST target `net10.0` unless a documented decision says otherwise.
3. MUST remain cross-platform on Windows, Linux, and macOS.
4. MUST preserve parity unless an approved parity gap is documented with mitigation.
5. MUST NOT add VS host/runtime dependencies (`EnvDTE`, `Microsoft.VisualStudio.*`, COM, registry-coupled behavior).
6. MUST keep diagnostics stable and CI-parseable with `TWxxxx` codes.
7. MUST keep generation deterministic (input ordering, output ordering, diagnostics ordering).

## 3) Agent Working Rules

1. Map each task to the relevant milestone in `DETAILED_IMPLEMENTATION_PLAN.md`.
2. Prefer vertical slices (`load -> metadata -> render -> write`) over broad refactors.
3. Reuse upstream logic first; rewrite only when required by VS coupling or .NET 10 constraints.
4. Keep changes concrete and repository-specific.
5. When behavior changes, update tests and parity artifacts in the same change.

## 4) Do-Not-Introduce Constraints

1. No reflection-based shortcuts that bypass MSBuild/Roslyn contracts.
2. No static mutable global state for orchestration, caching, or diagnostics.
3. No hidden background threads/tasks that outlive command execution.
4. No non-deterministic filesystem traversal in discovery or generation.
5. No breaking diagnostic contract changes (`TW` code/format/meaning) without explicit versioning notes.

## 5) Required CLI Contract (Do Not Drift)

Command shape:
- `typewriter-cli generate <templates> [--solution <path> | --project <path>] [options]`

Core options:
- `--solution`, `--project`, `--framework`, `--configuration`, `--runtime`
- `--restore`, `--output`, `--verbosity`, `--fail-on-warnings`

Exit codes:
- `0` success
- `1` generation/runtime/template errors (and warnings when elevated)
- `2` argument/input errors
- `3` SDK/restore/load/build errors

## 6) Architecture Boundaries (Implementation Target)

Runtime projects:
- `src/Typewriter.Cli`
- `src/Typewriter.Application`
- `src/Typewriter.Loading.MSBuild`
- `src/Typewriter.CodeModel`
- `src/Typewriter.Metadata`
- `src/Typewriter.Metadata.Roslyn`
- `src/Typewriter.Generation`

Test projects:
- `tests/Typewriter.UnitTests`
- `tests/Typewriter.IntegrationTests`
- `tests/Typewriter.GoldenTests`
- `tests/Typewriter.PerformanceTests`

MSBuild loading strategy:
- `ProjectGraph` for deterministic traversal
- Roslyn workspace for semantic model fidelity
- Support `.csproj`, `.sln`, `.slnx`

## 7) .NET 10 CLI Standards

1. Use `System.CommandLine` for parsing/handlers.
2. Use `PackAsTool` + `ToolCommandName` for tool packaging (not legacy `CommandName`).
3. Register MSBuild once (`Microsoft.Build.Locator`) before any workspace operations.
4. Respect `global.json`, `Directory.Build.props/targets`, and restore state.
5. Keep multi-target default deterministic (first declared TFM unless `--framework` is provided).
6. Never register multiple MSBuild instances in one process.

## 8) Mandatory Pre-Completion Verification

Before concluding any non-doc task, run:

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

If packaging-related files/behavior changed, also run:

```bash
dotnet pack -c Release
```

All required commands MUST succeed before task completion. Docs-only changes are exempt.

## 9) Testing and Verification Rules

1. Add or update tests for every behavior change.
2. Maintain golden parity snapshots for rendering and output behavior.
3. Keep integration fixtures for `.csproj`, `.sln`, `.slnx`, multi-target, and source generators.
4. Validate local tool flow before release:
   - `dotnet pack` CLI project
   - `dotnet tool install --local --add-source <nupkg-dir> <package-id>`
   - smoke `typewriter-cli generate ...`

## 10) Parity Drift Guard

1. Any intentional parity gap MUST have a decision record with rationale and mitigation.
2. Any parity-affecting change MUST update golden baselines/snapshots in the same PR.
3. If a commit is made for an intentional parity gap, include `PARITY-GAP:` in the commit subject.

## 11) Performance and Memory Constraints

1. When `--project` scope is provided, do not load unrelated solution projects.
2. Reuse workspace/process-level services per invocation; avoid repeated workspace creation.
3. Keep MSBuild registration single-pass per process.
4. Avoid loading full semantic state when only template discovery/filtering is needed.
5. Measure before optimizing; keep behavior parity first, then optimize safely.

## 12) CI Expectations

CI must gate on:
1. Cross-platform matrix (`windows-latest`, `ubuntu-latest`, `macos-latest`)
2. Restore -> build -> unit/integration/golden tests
3. Tool packaging + smoke execution
4. Parity and diagnostics stability checks

## 13) Done Checklist for Any Significant Change

1. Change satisfies the milestone acceptance criteria.
2. Required local verification commands have passed.
3. Tests were added/updated where needed and are passing.
4. CLI contract and exit-code behavior are preserved.
5. `origin/` remains unchanged.
6. Docs were updated when behavior/contracts changed.
