# F-0015: Buildalyzer and MSBuild Usage Gap

- ID: F-0015
- Title: Buildalyzer appears in solution/references but not in extension runtime flow
- Date: 2026-02-19

## Context
MSBuild loading strategy must be based on actual runtime behavior, not only solution composition.

## Evidence
- `origin/Typewriter.sln:20`/`:22`/`:24` includes Buildalyzer projects.
- `origin/src/Typewriter/Typewriter.csproj:222` has `ProjectReference` to `Buildalyzer`.
- Code search: `rg -n "Buildalyzer|MSBuildWorkspace|ProjectGraph|MSBuildLocator" origin/src` returns:
  - Buildalyzer references only in project files and test project references.
  - Runtime symbol usage in product code is absent.
  - `VisualStudioWorkspace` is used in `origin/src/Roslyn/RoslynMetadataProvider.cs:21`.
  - `MSBuildWorkspace` appears only as commented note in tests (`origin/src/Tests/TestInfrastructure/RoslynMetadataProviderStub.cs:25`).

## Conclusion
Upstream runtime does not currently use standalone MSBuild loading APIs for generation; it depends on VS-hosted workspace context.

## Impact
- CLI cannot reuse an existing upstream MSBuild loader and needs a new dedicated loading subsystem.
- Buildalyzer should be treated as optional/investigational rather than inherited design.

## Next steps
- Use prototype evidence and decision record to define the new loader architecture (`PR-0001`, `D-0003`).
- Keep Buildalyzer out of initial critical path unless a specific gap requires it.
