# F-0014: Tests and Parity Signals

- ID: F-0014
- Title: Existing tests identify parity-critical metadata/rendering behavior
- Date: 2026-02-19

## Context
Upstream tests provide direct evidence for parity-critical semantics and should drive CLI regression coverage.

## Evidence
- `origin/src/Tests/CodeModel/ClassTests.cs:35` onward validates class metadata surface:
  - names/full names/namespaces (`:36`-`:44`)
  - attributes (`:54`-`:62`)
  - base classes/interfaces (`:65`-`:97`)
  - generic type params (`:99`-`:117`)
  - constants/delegates/events/fields/methods/properties (`:120`-`:177`)
  - nested members and containing class (`:179`-`:238`)
  - generic base type arguments (`:241`-`:257`)
- `origin/src/Tests/Metadata/Roslyn/RoslynClassMetadataTests.cs:23` uses `PartialRenderingMode`.
- `origin/src/Tests/Metadata/Roslyn/RoslynClassMetadataTests.cs:30` validates merged partial metadata property coverage.
- `origin/src/Tests/Metadata/Roslyn/RoslynClassMetadataTests.cs:41` validates property attributes and attribute argument values.
- `origin/src/Tests/Render/RenderTests.cs:46` validates single-file rendering mode output against expected file.
- `origin/src/Tests/LongPaths/FileWriterTest.cs:11` validates long path write scenarios (Windows-focused).

## Conclusion
Upstream parity is strongly tied to symbol fidelity and deterministic render outputs, especially around partial types and complex metadata surfaces.

## Impact
- CLI testing must include:
  - metadata model regression tests,
  - golden output rendering tests,
  - single-file mode tests,
  - cross-platform path handling tests.
- Existing tests are VS-integrated; new CLI tests need independent harnesses.

## Next steps
- Mirror these test domains in CLI test strategy (unit/integration/golden).
- Use parity matrix rows to map each tested capability to target implementation milestones.
