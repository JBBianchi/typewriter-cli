# D-0003: Project Loading Strategy

- ID: D-0003
- Title: Hybrid loading strategy (`ProjectGraph` + Roslyn semantic loading)
- Date: 2026-02-19

## Context
Select loading architecture that supports `.csproj`, `.sln`, `.slnx`, multi-targeting, `global.json`, and high-fidelity symbol extraction for template generation.

## Evidence
- Upstream runtime metadata provider depends on VS-hosted `VisualStudioWorkspace`:
  - `origin/src/Roslyn/RoslynMetadataProvider.cs:21`
- Upstream product code does not include standalone `MSBuildWorkspace`/`ProjectGraph` usage:
  - `rg -n "MSBuildWorkspace|ProjectGraph|MSBuildLocator" origin/src` (no runtime hits outside tests/comments).
- Spike confirms `ProjectGraph` can load:
  - `.csproj`, `.sln`, `.slnx` using explicit MSBuild environment setup:
    - `.ai/prototypes/spike-msbuild/GraphProbe/Program.cs:51`
  - multi-target expansion and deterministic TFM selection via `TargetFramework` global property:
    - `.ai/prototypes/spike-msbuild/GraphProbe/Program.cs:40`
- Spike confirms `global.json` SDK selection behavior materially changes command outcomes:
  - `.ai/prototypes/spike-msbuild/global.json:1`
  - PR evidence in `.ai/prototypes/PR-0001-msbuild-loading-spike.md`
- Spike confirms missing restore assets produce load/build failure (`NETSDK1004`) in no-restore scenarios (PR note).

## Conclusion
Adopt a hybrid strategy:
1) `Microsoft.Build.Graph.ProjectGraph` for input parsing/traversal (`.csproj`/`.sln`/`.slnx`), project ordering, and global property control (`Configuration`, `TargetFramework`, `RuntimeIdentifier`).
2) Roslyn semantic loading per project (using MSBuild-based Roslyn project loading path) for high-fidelity symbols used by Typewriter templates.

## Impact
- Why not ProjectGraph-only:
  - Graph gives traversal/evaluation but not full semantic model fidelity required for Typewriter metadata surface.
- Why not MSBuildWorkspace-only:
  - traversal/control for `.slnx` and explicit graph-level orchestration is weaker/less explicit than direct graph loading.
- Hybrid provides:
  - deterministic input graph and target framework control,
  - parity-level symbol model for templates,
  - clearer failure handling for restore/load/build stages (exit code `3`).

## Next steps
- Implement loader modules:
  - `InputResolver` (detect input type),
  - `ProjectGraphLoader` (graph + global properties),
  - `SemanticModelProvider` (Roslyn project loading and compilation cache).
- Add integration tests for:
  - `.csproj`, `.sln`, `.slnx`,
  - multi-target with and without `--framework`,
  - missing assets with `--no-restore` and with `--restore`.
