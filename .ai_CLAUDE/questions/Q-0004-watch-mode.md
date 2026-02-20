# Q-0004: Watch Mode for CLI

**Date**: 2026-02-19
**Status**: Deferred

## Question
Should the CLI support a `--watch` mode that monitors file changes and re-generates, similar to the VS extension's auto-render behavior?

## Context
- Upstream auto-renders on `.cs` save (via `SolutionMonitor` + `DTE.DocumentEvents`)
- CLI is designed for batch mode (CI pipelines)
- Developers may want watch mode for local development

## Resolution
Deferred to post-v1.0. The initial CLI focuses on batch mode for CI. Watch mode can be added later using `FileSystemWatcher` to monitor `.cs` files in included projects and re-trigger generation.

This is documented as an intentional parity gap in P-0001 (features #63, #64).
