# T026: Implement IProjectGraphService and ProjectGraphService
- Milestone: M3
- Status: Done
- Agent: Claude (Executor)
- Started: 2026-03-02
- Completed: 2026-03-02

## Objective
Implement `IProjectGraphService` and `ProjectGraphService` in `src/Typewriter.Loading.MSBuild/` to load an MSBuild `ProjectGraph`, perform deterministic topological sort of nodes, select target frameworks, verify restore assets, and produce a `ProjectLoadPlan`.

## Approach
Files involved:
- `src/Typewriter.Loading.MSBuild/IProjectGraphService.cs` (interface)
- `src/Typewriter.Loading.MSBuild/ProjectGraphService.cs` (implementation)
- `src/Typewriter.Application/Loading/IProjectGraphService.cs` (interface moved here during T027)
- `src/Typewriter.Application/Orchestration/ProjectLoadPlan.cs` (output DTO, from T021)
- `src/Typewriter.Application/Orchestration/LoadTarget.cs` (target DTO, from T021)

## Journey
### 2026-03-02 — Core implementation

**Topological sort decision (Kahn's algorithm with path tie-breaker)**

`ProjectGraph.ProjectNodes` returns nodes in no guaranteed order. Output ordering must be deterministic (see AGENTS.md §2.7) so that later stages (metadata extraction, code generation) produce stable results regardless of filesystem or evaluation order.

Chose Kahn's algorithm (BFS-based topological sort) over DFS because:
- Natural dependencies-first ordering: libraries are emitted before their consumers, which is the correct load order for Roslyn workspace building.
- Cycle detection is trivial (graph leftover after sort).
- Tie-breaking between ready nodes at each step is straightforward: sort by `FullPath` ascending (ordinal) to get deterministic alphabetical ordering within the same dependency depth.

Implementation uses a `SortedSet<ProjectGraphNode>` keyed by `FullPath` as the "available" queue. At each step `available.Min` is the lexicographically smallest ready node, ensuring a fully deterministic traversal even when multiple nodes have the same in-degree.

**Multi-target framework selection**

When a project declares `<TargetFrameworks>` (plural, semicolon-separated), MSBuild evaluates it as a single TFM-agnostic instance in the `ProjectGraph`. The selected TFM must be determined explicitly:
- If `--framework` is supplied: validate it is present in the declared list; emit TW2002 error if not.
- If `--framework` is absent: default to `tfms[0]` (first declared TFM in document order) and emit a TW2401 informational diagnostic listing all TFMs and advising `--framework`. This preserves determinism (no "random" TFM picked) while informing the user they may want to narrow scope.

**Restore assets check**

`ProjectGraphService.BuildPlanAsync` re-checks `obj/project.assets.json` presence for each node after topological sort. This is a second guard beyond `RestoreService.CheckAssetsAsync` (which checks the root project before the restore decision is made). The in-graph check catches projects that were reachable via `ProjectReference` chains but whose assets were not restored (e.g., added to the solution after the last restore). Emits TW2003 error.

**Error accumulation**

Errors for individual nodes (`TW2002` missing TFM, `TW2003` missing assets) are accumulated across all nodes before returning `null`; this surfaces all problems in one pass rather than short-circuiting on the first node failure.

## Outcome
`ProjectGraphService` produces a `ProjectLoadPlan` with:
- `Targets` in topological (dependencies-first) order, deterministically sorted by path within each level.
- Each `LoadTarget` carries `TargetFramework`, `Configuration`, `RuntimeIdentifier`, and a zero-based `Index`.
- Returns `null` on any error; all errors reported via `IDiagnosticReporter` before returning.

Tests in `tests/Typewriter.UnitTests/Loading/ProjectLoaderTests.cs` (T029) and `tests/Typewriter.IntegrationTests/Loading/CsprojIntegrationTests.cs` (T030) cover the main paths.

## Follow-ups
- M4: extend `IProjectGraphService` or add `ISolutionGraphService` for `.sln` / `.slnx` inputs.
- `ProjectGraph` does not natively support `.sln` files; M4 will need to enumerate solution projects and construct the graph manually.
