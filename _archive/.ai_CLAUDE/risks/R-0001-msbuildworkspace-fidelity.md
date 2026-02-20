# R-0001: MSBuildWorkspace Semantic Model Fidelity

**Date**: 2026-02-19
**Status**: Active

## Context
MSBuildWorkspace may produce different semantic models than VisualStudioWorkspace, potentially causing generation differences.

## Risk
**Impact**: High — incorrect type resolution would produce wrong TypeScript output
**Likelihood**: Low — MSBuildWorkspace uses same Roslyn compiler pipeline

## Evidence
- MSBuildWorkspace creates `Compilation` from the same Roslyn APIs as VS
- Known issues: workspace diagnostics for unresolvable metadata references (typically restore-related)
- Source generators: MSBuildWorkspace may not include generated files in compilation by default

## Mitigation
1. **Golden file tests**: Port upstream test data (support classes + expected outputs) and verify byte-for-byte identical generation
2. **Diagnostic monitoring**: Log all `WorkspaceFailed` events and fail-fast on `Failure` kind
3. **Source generator support**: Ensure `project.GetCompilationAsync()` includes generated sources (may need `GenerateDocumentationFile` property)
4. **Regression suite**: Compare CLI output against upstream VSIX output for a corpus of real projects

## Next Steps
- Build golden file test suite in Phase 7
- Create comparison test with upstream output
