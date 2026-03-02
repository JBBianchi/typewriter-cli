# T002: Port Metadata Interfaces (I*.cs)
- Milestone: M1
- Status: Done
- Agent: Executor (claude-sonnet-4-6)
- Started: 2026-03-02
- Completed: 2026-03-02

## Objective

Copy the 19 metadata interface files from `origin/src/Metadata/Interfaces/I*.cs` into `src/Typewriter.Metadata/Interfaces/`, updating namespaces from `Typewriter.Metadata.Interfaces` to `Typewriter.Metadata`.

## Approach

1. Read all 19 origin interface files to confirm content and namespaces.
2. Create `src/Typewriter.Metadata/Interfaces/` directory.
3. Write each file with:
   - File-scoped namespace syntax (`namespace Typewriter.Metadata;`)
   - No BOM
   - Content identical to origin (no VS references to remove — all 19 were clean per T001 audit)

## Journey

### 2026-03-02

- Confirmed 19 files in `origin/src/Metadata/Interfaces/I*.cs` via glob.
- Confirmed all files used block-scoped `namespace Typewriter.Metadata.Interfaces { ... }` in origin.
- T001 audit confirmed: all 19 files are **clean** — no `EnvDTE`, `Microsoft.VisualStudio.*`, or COM references.
- Created `src/Typewriter.Metadata/Interfaces/` directory.
- Wrote all 19 files using file-scoped namespace `Typewriter.Metadata;` (modern .NET 10 style, consistent with `Placeholder.cs`).
- Verified no VS/COM references in ported files.
- Verified all 19 files exist and all have `namespace Typewriter.Metadata;`.
- dotnet SDK not available in execution environment; build verification deferred to CI.

## Outcome

All 19 interface files created under `src/Typewriter.Metadata/Interfaces/`:

| File | Namespace | Notes |
|------|-----------|-------|
| IAttributeArgumentMetadata.cs | Typewriter.Metadata | Clean port |
| IAttributeMetadata.cs | Typewriter.Metadata | Clean port |
| IClassMetadata.cs | Typewriter.Metadata | Clean port |
| IConstantMetadata.cs | Typewriter.Metadata | Clean port |
| IDelegateMetadata.cs | Typewriter.Metadata | Clean port |
| IEnumMetadata.cs | Typewriter.Metadata | Clean port |
| IEnumValueMetadata.cs | Typewriter.Metadata | Clean port |
| IEventMetadata.cs | Typewriter.Metadata | Clean port |
| IFieldMetadata.cs | Typewriter.Metadata | Clean port |
| IFileMetadata.cs | Typewriter.Metadata | Clean port |
| IInterfaceMetadata.cs | Typewriter.Metadata | Clean port |
| IMethodMetadata.cs | Typewriter.Metadata | Clean port |
| INamedItem.cs | Typewriter.Metadata | Clean port |
| IParameterMetadata.cs | Typewriter.Metadata | Clean port |
| IPropertyMetadata.cs | Typewriter.Metadata | Clean port |
| IRecordMetadata.cs | Typewriter.Metadata | Clean port |
| IStaticReadOnlyFieldMetadata.cs | Typewriter.Metadata | Clean port |
| ITypeMetadata.cs | Typewriter.Metadata | Clean port |
| ITypeParameterMetadata.cs | Typewriter.Metadata | Clean port |

`origin/` unchanged.

## Follow-ups

- T003+: Port collections (`origin/src/Typewriter/CodeModel/Collections/`) into `src/Typewriter.CodeModel/`
- Future tasks consuming these interfaces will need `using Typewriter.Metadata;`
