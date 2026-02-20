# Q-0004: Partial Rendering `requestRender` Equivalence

- ID: Q-0004
- Title: How should CLI handle `requestRender` callback behavior for partial rendering?
- Date: 2026-02-19

## Context
Roslyn file metadata can request rendering of another file in combined partial mode.

## Evidence
- `origin/src/Roslyn/RoslynFileMetadata.cs:64` combined mode branch.
- `origin/src/Roslyn/RoslynFileMetadata.cs:78` invokes `_requestRender?.Invoke(new[] { locationToRender })`.
- `origin/src/Typewriter/Generation/Controllers/GenerationController.cs:129` passes callback into metadata provider in C# change flow.

## Conclusion
Callback-driven secondary rendering behavior exists and must be mapped to deterministic CLI batching.

## Impact
- Missing this behavior may cause incomplete generation updates for partial type scenarios.
- Over-implementation can create duplicate work if not deduplicated.

## Next steps
- Define CLI batching semantics for secondary requested files.
- Add dedicated regression test for combined partial mode with cross-file declarations.
