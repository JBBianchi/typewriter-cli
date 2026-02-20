# Q-0002: Template Discovery Strategy for CLI

**Date**: 2026-02-19
**Status**: Resolved

## Question
How should the CLI discover `.tst` templates? Upstream relies on VS project items; CLI has no project context.

## Context
- Upstream: `TemplateController.LoadTemplates()` recursively searches all VS projects for `.tst` files
- CLI receives input as glob pattern: `typewriter-cli generate "**/*.tst" --solution Foo.sln`

## Resolution
CLI uses file system glob patterns for template discovery:
1. User provides glob: `**/*.tst` (or specific paths)
2. CLI expands glob relative to working directory
3. Each discovered `.tst` file is processed
4. Template's `Settings.IncludedProjects` determines which C# files to analyze

This is simpler and more explicit than VS project scanning. The `--solution` or `--project` flag determines the scope for C# file discovery, not template discovery.
