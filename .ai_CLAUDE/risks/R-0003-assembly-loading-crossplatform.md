# R-0003: Template Assembly Loading on Cross-platform

**Date**: 2026-02-19
**Status**: Active

## Context
Template `${ }` code blocks are compiled to temporary assemblies and loaded via `Assembly.LoadFrom()`. This pattern has changed significantly between .NET Framework and .NET Core.

## Risk
**Impact**: High — template compilation failure would break all generation
**Likelihood**: Medium — `Assembly.LoadFrom` behaves differently in .NET 10

## Evidence
- **File**: `origin/src/Typewriter/Generation/Compiler.cs`
- Uses `Assembly.LoadFrom(path)` to load compiled template assembly
- .NET 10 uses `AssemblyLoadContext` — `LoadFrom` still works but with different probing rules
- Template assemblies reference `Typewriter.CodeModel` — must be resolvable at load time
- `#reference` directives load user assemblies — path resolution and deps must work cross-platform

## Mitigation
1. **Use AssemblyLoadContext**: Create isolated `AssemblyLoadContext` for template assemblies
2. **Resolver**: Register `AssemblyLoadContext.Resolving` handler to find `Typewriter.CodeModel` and user-referenced assemblies
3. **Temp directory**: Use `Path.GetTempPath()` (cross-platform) instead of hardcoded paths
4. **Test**: Verify template compilation on Linux and macOS in CI

## Next Steps
- Implement `TemplateAssemblyLoadContext` in Phase 6
- Add Linux/macOS CI matrix
