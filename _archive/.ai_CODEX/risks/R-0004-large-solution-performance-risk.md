# R-0004: Large Solution Performance Risk

- ID: R-0004
- Title: Performance and memory pressure for large graphs and semantic models
- Date: 2026-02-19

## Context
Typewriter can include current/referenced/all projects and parse many templates/files.

## Evidence
- `origin/src/CodeModel/Configuration/Settings.cs:117` warns `IncludeAllProjects()` can heavily impact performance.
- `origin/src/Typewriter/Generation/Template.cs:94` gathers all files across included projects.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:280` applies actions across all templates and changed paths.
- Spike showed multi-target projects expand graph nodes (`GraphProbe` output in PR-0001).

## Conclusion
Graph + semantic loading can grow quickly with large/multi-target solutions.

## Impact
- Longer generation times and higher memory use.
- Increased CI time/cost and potential timeouts.

## Next steps
- Cache compilations/project metadata during one CLI invocation.
- Provide filters (`--framework`, template/file scopes) and detailed timing diagnostics.
- Add performance benchmark fixtures for representative large inputs.
