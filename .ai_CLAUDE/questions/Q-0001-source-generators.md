# Q-0001: Source Generator Support in MSBuildWorkspace

**Date**: 2026-02-19
**Status**: Open

## Question
Does MSBuildWorkspace include source-generator-produced files in the compilation? If user C# code depends on source generators (e.g., `System.Text.Json` serialization), will the generated types be visible to Typewriter templates?

## Context
- Upstream uses `VisualStudioWorkspace` which includes source generator output
- MSBuildWorkspace may require explicit configuration to run generators
- Some popular libraries use source generators (MediatR, AutoMapper, EF Core compiled models)

## Impact
If source-generated types are missing, templates that reference those types will produce incomplete output.

## Investigation Needed
1. Test MSBuildWorkspace with a project that uses source generators
2. Check if `GetCompilationAsync()` includes generated documents
3. Evaluate if `additionalProperties["CompilerGeneratedFilesOutputPath"]` is needed

## Proposed Resolution
- Test during Phase 5 (Semantic model extraction parity)
- If generators are missing: evaluate `GeneratorDriver` API for manual generator execution
