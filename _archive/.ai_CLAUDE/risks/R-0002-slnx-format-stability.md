# R-0002: .slnx Format Stability

**Date**: 2026-02-19
**Status**: Active

## Context
`.slnx` is a new solution format. It may not be fully stabilized and is not supported by MSBuildWorkspace natively.

## Risk
**Impact**: Medium — custom parser may break with format changes
**Likelihood**: Medium — format is relatively new

## Evidence
- `.slnx` is XML-based, simpler than `.sln` text format
- No official MSBuild/Roslyn API for parsing `.slnx` yet
- Format may evolve before full standardization

## Mitigation
1. **Simple parser**: Minimal XML parsing (just `<Project Path="..."/>` elements)
2. **Version detection**: Check for schema version attribute
3. **Graceful degradation**: If parsing fails, suggest using `.sln` or `.csproj` input
4. **Monitor**: Track dotnet/sdk repo for `.slnx` API additions — adopt official parser when available

## Next Steps
- Implement minimal `.slnx` parser in Phase 4
- Add integration tests with sample `.slnx` files
