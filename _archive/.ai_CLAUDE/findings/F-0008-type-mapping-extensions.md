# F-0008: Type Mapping, Extensions, and Helpers

**Date**: 2026-02-19
**Status**: Complete

## Context
Analysis of type mapping (C# → TypeScript), extension methods, and utility functions that are core to generation.

## Evidence

### Helpers.cs (C# → TypeScript Type Mapping)
- **File**: `origin/src/Typewriter/CodeModel/Helpers.cs`
- **Class**: `static Helpers`
- **Key methods**:
  - `CamelCase(string)` — intelligent camelCase conversion (handles acronyms)
  - `GetTypeScriptName(ITypeMetadata, Settings)` — main type mapping
  - `GetOriginalName(ITypeMetadata)` — C# name with primitive aliases
  - `IsPrimitive(ITypeMetadata)` — checks primitive type dictionary
  - `ExtractTypeScriptName(ITypeMetadata, Settings)` — base type → TS mapping

### Type Mapping Rules
| C# Type | TypeScript |
|---------|-----------|
| `bool` | `boolean` |
| `string`, `char`, `Guid`, `TimeSpan` | `string` |
| `byte`, `short`, `int`, `long`, `float`, `double`, `decimal` (all numeric) | `number` |
| `DateTime`, `DateTimeOffset` | `Date` |
| `void` | `void` |
| `object`, `dynamic` | `any` |
| Nullable<T> (when StrictNullGeneration=true) | `T \| null` |
| `Dictionary<K,V>` | `Record<K, V>` |
| `IEnumerable<T>` | `T[]` |
| Value tuples | `{ name: type, ... }` |
| Generic<T> | `Name<T>` |

### WebApi Extensions
- **File**: `origin/src/CodeModel/Extensions/WebApi/HttpMethodExtensions.cs`
- **File**: `origin/src/CodeModel/Extensions/WebApi/UrlExtensions.cs`
- **File**: `origin/src/CodeModel/Extensions/WebApi/RequestDataExtensions.cs`
- Extension methods on `Method` class: `HttpMethod()`, `Url()`, `RequestData()`
- Maps ASP.NET attributes (`[HttpGet]`, `[Route]`, etc.) to HTTP method/URL strings

### TypeExtensions
- **File**: `origin/src/CodeModel/Extensions/Types/TypeExtensions.cs`
- Extension methods on `Type` class for TypeScript generation helpers

### CodeModel Implementation Pattern
- **File**: `origin/src/Typewriter/CodeModel/Implementation/ClassImpl.cs` (representative)
- Pattern: wraps `IClassMetadata`, exposes abstract `Class` properties
- All use lazy `_field ?? (_field = XxxImpl.FromMetadata(...))` pattern
- Static factory: `FromMetadata(IEnumerable<IClassMetadata>, Item parent, Settings)` → collection
- **Special**: `GetPropertiesFromClassMetadata()` — handles `[MetadataType]`/`[ModelMetadataType]` attribute merging for EF/code-first scenarios

### Dependencies
- `Helpers.cs` — uses `ITypeMetadata` from Metadata interfaces. No VS deps.
- WebApi extensions — use `Attribute`, `Method` from CodeModel. No VS deps.
- `ClassImpl` etc. — use metadata interfaces + `Settings`. No VS deps.

## Conclusion
All type mapping, extensions, and CodeModel implementations are **fully VS-independent**. They operate on metadata interfaces and abstract CodeModel classes. These can be ported to .NET 10 with zero functional changes.

## Impact
- All files in `origin/src/CodeModel/Extensions/` → **direct port**
- `origin/src/Typewriter/CodeModel/Helpers.cs` → **direct port**
- All files in `origin/src/Typewriter/CodeModel/Implementation/` → **direct port**
- All files in `origin/src/Typewriter/CodeModel/Collections/` → **direct port**
- These represent the largest reusable code mass (~40+ files)

## Next Steps
- Include in feature parity matrix
