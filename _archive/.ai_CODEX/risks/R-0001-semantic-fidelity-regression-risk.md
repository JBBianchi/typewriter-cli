# R-0001: Semantic Fidelity Regression Risk

- ID: R-0001
- Title: Loss of Roslyn metadata fidelity during host migration
- Date: 2026-02-19

## Context
Typewriter templates depend on rich symbol metadata (nullable, records, generics, partial behavior, attributes, tuple/task handling).

## Evidence
- `origin/src/Roslyn/RoslynTypeMetadata.cs:179` nullable handling.
- `origin/src/Roslyn/RoslynTypeMetadata.cs:196` task unwrapping logic.
- `origin/src/Roslyn/RoslynClassMetadata.cs:81` partial member filtering.
- `origin/src/Roslyn/RoslynRecordMetadata.cs:10` record support.
- `origin/src/Tests/CodeModel/ClassTests.cs:35`+ broad metadata surface tests.

## Conclusion
Semantic fidelity is a top risk area in CLI migration.

## Impact
- Incorrect or incomplete generated TypeScript output.
- Hidden regressions in large templates relying on advanced metadata.

## Next steps
- Preserve Roslyn-based model extraction.
- Build golden tests and metadata regression tests early in implementation phases.
