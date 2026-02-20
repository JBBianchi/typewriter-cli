# R-0004: NuGet Restore State in CI

**Date**: 2026-02-19
**Status**: Active

## Context
MSBuildWorkspace requires NuGet packages to be restored before it can open projects successfully. CI environments may not have packages restored.

## Risk
**Impact**: High — unresolved packages cause compilation failures → missing types → wrong output
**Likelihood**: Medium — CI pipelines typically restore, but not always guaranteed

## Evidence
- MSBuildWorkspace reports `WorkspaceFailed` events for missing references
- Without restore: types from NuGet packages are unresolved → `object` or error
- `project.assets.json` is the key indicator of restore state

## Mitigation
1. **Pre-check**: Verify `obj/project.assets.json` exists before opening workspace
2. **Clear error**: "NuGet packages not restored. Run `dotnet restore` or use `--restore` flag."
3. **`--restore` flag**: Optionally run `dotnet restore` before loading
4. **Exit code 3**: Return load/restore error exit code
5. **CI docs**: Document recommended pipeline: `dotnet restore` → `typewriter-cli generate`

## Next Steps
- Implement restore detection in Phase 2
- Document CI integration patterns
