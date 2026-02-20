# R-0003: Project Mutation Parity Gap Risk

- ID: R-0003
- Title: VS project-item mutation features may not translate cleanly to CLI
- Date: 2026-02-19

## Context
Upstream optionally adds generated files to project and maintains source mapping metadata.

## Evidence
- `origin/src/Typewriter/Generation/Template.cs:212` project mutation branch.
- `origin/src/Typewriter/Generation/Template.cs:233` add generated file to project.
- `origin/src/Typewriter/Generation/Template.cs:317` writes `CustomToolNamespace`.
- `origin/src/Typewriter/Generation/Template.cs:500` source-control checkout via DTE.

## Conclusion
These features may become explicit parity gaps or conditional features in CLI v1.

## Impact
- Some teams may rely on automatic project inclusion behavior.
- Potential mismatch between generated files on disk and project file contents.

## Next steps
- Decide scope in `Q-0002`.
- If deferred, document mitigation (`glob includes` / manual project includes / optional post-step).
