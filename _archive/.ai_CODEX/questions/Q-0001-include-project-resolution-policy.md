# Q-0001: IncludeProject Resolution Policy

- ID: Q-0001
- Title: How should `Settings.IncludeProject(string)` resolve projects in CLI?
- Date: 2026-02-19

## Context
Upstream resolves include-project by DTE project `Name`, which can be ambiguous.

## Evidence
- `origin/src/Typewriter/CodeModel/Configuration/ProjectHelpers.cs:24` compares `project.Name` to `projectName`.
- `origin/src/Typewriter/CodeModel/Configuration/ProjectHelpers.cs:36` emits warning if no match.

## Conclusion
Resolution policy is currently under-specified for non-DTE graph loading and duplicate project names.

## Impact
- Affects which source files are included for template rendering.
- Can create silent parity regressions in monorepos with repeated project names.

## Next steps
- Decide deterministic policy:
  - name-first with ambiguity error,
  - full path matching,
  - assembly-name matching,
  - or ordered fallback with warnings.
