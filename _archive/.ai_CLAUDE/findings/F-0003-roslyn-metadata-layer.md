# F-0003: Roslyn Metadata Provider Architecture

**Date**: 2026-02-19
**Status**: Complete

## Context
How upstream Typewriter extracts C# semantic model information using Roslyn, and what must change for CLI.

## Evidence

### Three-Layer Architecture
1. **CodeModel** (`origin/src/CodeModel/`) — Abstract public API exposed to templates
2. **Metadata** (`origin/src/Metadata/`) — Interface contracts
3. **Roslyn** (`origin/src/Roslyn/`) — Concrete Roslyn-based implementation

### Entry Point: RoslynMetadataProvider
- **File**: `origin/src/Roslyn/RoslynMetadataProvider.cs`
- **Interface**: `IMetadataProvider` — single method: `IFileMetadata GetFile(string path, Settings settings, Action<string[]> requestRender)`
- **Workspace acquisition**: `ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel))` → `VisualStudioWorkspace`
- **Document lookup**: `_workspace.CurrentSolution.GetDocumentIdsWithFilePath(path)`

### Semantic Model Acquisition
- **File**: `origin/src/Roslyn/RoslynFileMetadata.cs`
- `document.GetSemanticModelAsync()` → `SemanticModel`
- `_semanticModel.SyntaxTree.GetRootAsync()` → `SyntaxNode`
- Walks syntax tree for namespace/type declarations
- `_semanticModel.GetDeclaredSymbol(node)` → `INamedTypeSymbol`

### Roslyn Metadata Classes (ALL VS-independent once workspace is provided)
| Class | Wraps | File |
|-------|-------|------|
| `RoslynFileMetadata` | `Document` + `SemanticModel` | `origin/src/Roslyn/RoslynFileMetadata.cs` |
| `RoslynClassMetadata` | `INamedTypeSymbol` | `origin/src/Roslyn/RoslynClassMetadata.cs` |
| `RoslynInterfaceMetadata` | `INamedTypeSymbol` | `origin/src/Roslyn/RoslynInterfaceMetadata.cs` |
| `RoslynRecordMetadata` | `INamedTypeSymbol` | `origin/src/Roslyn/RoslynRecordMetadata.cs` |
| `RoslynEnumMetadata` | `INamedTypeSymbol` | `origin/src/Roslyn/RoslynEnumMetadata.cs` |
| `RoslynTypeMetadata` | `ITypeSymbol` | `origin/src/Roslyn/RoslynTypeMetadata.cs` |
| `RoslynMethodMetadata` | `IMethodSymbol` | `origin/src/Roslyn/RoslynMethodMetadata.cs` |
| `RoslynPropertyMetadata` | `IPropertySymbol` | `origin/src/Roslyn/RoslynPropertyMetadata.cs` |
| `RoslynFieldMetadata` | `IFieldSymbol` | `origin/src/Roslyn/RoslynFieldMetadata.cs` |
| `RoslynConstantMetadata` | `IFieldSymbol` (const) | `origin/src/Roslyn/RoslynConstantMetadata.cs` |
| `RoslynStaticReadOnlyFieldMetadata` | `IFieldSymbol` (static readonly) | `origin/src/Roslyn/RoslynStaticReadOnlyFieldMetadata.cs` |
| `RoslynAttributeMetadata` | `INamedTypeSymbol` | `origin/src/Roslyn/RoslynAttributeMetadata.cs` |
| `RoslynAttributeArgumentMetadata` | `TypedConstant` | `origin/src/Roslyn/RoslynAttributeArgumentMetadata.cs` |
| `RoslynParameterMetadata` | `IParameterSymbol` | `origin/src/Roslyn/RoslynParameterMetadata.cs` |
| `RoslynDelegateMetadata` | `INamedTypeSymbol` | `origin/src/Roslyn/RoslynDelegateMetadata.cs` |
| `RoslynEventMetadata` | `IEventSymbol` | `origin/src/Roslyn/RoslynEventMetadata.cs` |
| `RoslynTypeParameterMetadata` | `ITypeParameterSymbol` | `origin/src/Roslyn/RoslynTypeParameterMetadata.cs` |
| `RoslynVoidTaskMetadata` | (synthetic void Task) | `origin/src/Roslyn/RoslynVoidTaskMetadata.cs` |

### Type Resolution Special Cases
- **Nullable<T>**: Unwrapped to T with `isNullable=true`
- **Task<T>**: Unwrapped to T with `isTask=true`
- **Task (void)**: Returns `RoslynVoidTaskMetadata`
- **Enums**: `BaseType?.SpecialType == System_Enum`
- **Value Tuples**: Empty name + ValueType base + System namespace
- **Arrays**: `TypeKind.Array` → `IsEnumerable=true`

### Partial Class Support
- `RoslynClassMetadata.Members` property filters by `PartialRenderingMode`:
  - `Partial`: Only members declared in current file
  - `Combined`: All members + `_requestRender` callback for other locations

### VS-Specific Code in Roslyn Layer (only 4 points)
1. `RoslynMetadataProvider` constructor — `ServiceProvider.GlobalProvider` → `VisualStudioWorkspace`
2. `RoslynMetadataProvider` — `ThreadHelper.ThrowIfNotOnUIThread()`
3. `RoslynFileMetadata.LoadDocument()` — `ThreadHelper.JoinableTaskFactory.Run()`
4. Package refs: `Microsoft.VisualStudio.LanguageServices`, `Microsoft.VisualStudio.SDK`

### Package Dependencies
- **Keep**: `Microsoft.CodeAnalysis.CSharp.Workspaces` v4.14.0
- **Remove**: `Microsoft.VisualStudio.LanguageServices` v4.14.0, `Microsoft.VisualStudio.SDK`, `Microsoft.VisualStudio.Debugger.Contracts`, `StreamJsonRpc`
- **Add**: `Microsoft.CodeAnalysis.Workspaces.MSBuild` (for MSBuildWorkspace)

## Conclusion
The Roslyn metadata layer is the **cleanest extraction target**. The `IMetadataProvider` interface is a natural seam — only `RoslynMetadataProvider` needs a new implementation. The 17 `Roslyn*Metadata` classes operate purely on Roslyn symbols and have **zero VS dependencies** — they can be reused as-is once given a `Document` from any Roslyn `Workspace`.

## Impact
- ~17 metadata classes: **reuse directly** (port to .NET 10, remove ThreadHelper calls from RoslynFileMetadata)
- `RoslynMetadataProvider`: **rewrite** to use MSBuildWorkspace instead of VisualStudioWorkspace
- `RoslynFileMetadata`: **minor edit** — replace `JoinableTaskFactory.Run()` with proper `async/await`
- Total Roslyn layer effort: ~2 files to change, ~17 files to port (minimal changes)

## Next Steps
- D-0003: Loading strategy (MSBuildWorkspace vs Buildalyzer)
- PR-0001: MSBuild loading spike
