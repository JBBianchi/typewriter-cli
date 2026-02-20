# R-0002: MSBuild Load and Restore Determinism Risk

- ID: R-0002
- Title: Project loading failures due SDK/restore/workload state
- Date: 2026-02-19

## Context
CLI must reliably load and generate in CI with varying SDK and restore states.

## Evidence
- Spike: invalid `global.json` SDK pin causes hard command failure (PR-0001).
- Spike: `--no-restore` on non-restored project fails with `NETSDK1004` (PR-0001).
- Spike: environment showed `MSB4276` workload locator SDK resolver messages in multi-target builds (`build-multilib-diag.log`).
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:50` currently hides workspace sync issues with fixed delay.

## Conclusion
Loading/build determinism is high-risk without explicit SDK/restore handling and clear diagnostics.

## Impact
- Unreliable CI runs and unclear failures.
- Difficult user support if SDK/restore state is implicit.

## Next steps
- Require explicit load pipeline stages: resolve SDK, optional restore, graph load, semantic load.
- Map all load/restore failures to exit code `3` with actionable messages.
