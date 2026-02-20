# F-0007: Semantic Model Extraction and Fidelity

- ID: F-0007
- Title: Metadata extraction uses VisualStudioWorkspace-backed Roslyn symbols
- Date: 2026-02-19

## Context
Project-loading strategy depends on how upstream obtains symbols and what fidelity is required.

## Evidence
- `origin/src/Roslyn/RoslynMetadataProvider.cs:13` class `RoslynMetadataProvider : IMetadataProvider`.
- `origin/src/Roslyn/RoslynMetadataProvider.cs:20` acquires `SComponentModel`.
- `origin/src/Roslyn/RoslynMetadataProvider.cs:21` acquires `VisualStudioWorkspace`.
- `origin/src/Roslyn/RoslynMetadataProvider.cs:26` resolves `DocumentId` via `CurrentSolution.GetDocumentIdsWithFilePath(path)`.
- `origin/src/Roslyn/RoslynFileMetadata.cs:12` class `RoslynFileMetadata`.
- `origin/src/Roslyn/RoslynFileMetadata.cs:49`/`:50` loads semantic model and syntax root from Roslyn.
- `origin/src/Roslyn/RoslynFileMetadata.cs:64` partial rendering combined mode picks canonical source file location and can request additional render.
- `origin/src/Roslyn/RoslynClassMetadata.cs:81` partial mode can restrict members to file-local declarations for partial types.
- `origin/src/Roslyn/RoslynTypeMetadata.cs:155` `IsDictionary` logic.
- `origin/src/Roslyn/RoslynTypeMetadata.cs:166` `IsEnumerable` logic excluding `string`.
- `origin/src/Roslyn/RoslynTypeMetadata.cs:179` nullable handling (`NullableAnnotation`).
- `origin/src/Roslyn/RoslynTypeMetadata.cs:196` task unwrapping (`Task<T>` and nullable task cases).
- `origin/src/Roslyn/RoslynTypeMetadata.cs:88` tuple element handling.
- `origin/src/Roslyn/RoslynTypeMetadata.cs:57` file location metadata.
- `origin/src/Roslyn/RoslynRecordMetadata.cs:10` record metadata support.
- Search evidence: `rg -n "MSBuildWorkspace|ProjectGraph|MSBuildLocator" origin/src` found no runtime usage in product code.

## Conclusion
Upstream symbol model fidelity is high and Roslyn-based, but workspace sourcing is Visual Studio-hosted (`VisualStudioWorkspace`), not standalone MSBuild workspace loading.

## Impact
- CLI must preserve Roslyn symbol fidelity while replacing workspace initialization and solution/project loading.
- Semantic model coverage (nullable, records, generics, tuples, partial modes, assembly/type metadata) is a parity-critical area.
- Project loading must be explicitly designed because upstream has no standalone MSBuild load path.

## Next steps
- Finalize project-loading strategy (`D-0003`) with explicit Roslyn + MSBuild integration path.
- Add parity rows and tests for symbol features validated in `src/Tests/CodeModel` and `src/Tests/Metadata`.
