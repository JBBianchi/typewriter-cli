# Q-0002: Project Mutation Parity Scope

- ID: Q-0002
- Title: Should CLI modify project files to add generated outputs?
- Date: 2026-02-19

## Context
Upstream optionally mutates VS project items and source mapping metadata.

## Evidence
- `origin/src/Typewriter/Generation/Template.cs:212` branches on `AddGeneratedFilesToProject` and `SkipAddingGeneratedFilesToProject`.
- `origin/src/Typewriter/Generation/Template.cs:233` `ProjectItems.AddFromFile(outputPath)`.
- `origin/src/Typewriter/Generation/Template.cs:317` writes `CustomToolNamespace` metadata mapping.

## Conclusion
Parity scope decision is required because this behavior is VS/DTE-specific and not naturally CLI/cross-platform.

## Impact
- If omitted, generated files still exist but project mutation parity is partial.
- If included, CLI must safely edit SDK-style project XML and possibly legacy project formats.

## Next steps
- Decide default:
  - no project mutation in v1 (recommended),
  - optional explicit flag for SDK-style `.csproj` update only,
  - or full parity implementation later.
