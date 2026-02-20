# Q-0003: .NET Framework 4.7.2 API Compatibility During Port

**Date**: 2026-02-19
**Status**: Open

## Question
Which .NET Framework 4.7.2 APIs used by upstream code are unavailable or behave differently on .NET 10?

## Context
Upstream targets net472. When porting to net10.0, some APIs may need changes:
- `Assembly.LoadFrom()` — still available but different probing
- `AppDomain.CurrentDomain.BaseDirectory` — different semantics in tool context
- `Path.GetFullPath()` — handles long paths differently
- `File.WriteAllText()` — encoding defaults may differ

## Investigation Needed
1. Compile upstream CodeModel + Metadata + Roslyn code targeting net10.0
2. Identify compilation errors
3. Document each API difference and resolution

## Expected Impact
Low — upstream code is mostly standard .NET APIs. Primary changes:
- Remove `ThreadHelper` calls (VS-specific)
- Replace `Assembly.LoadFrom()` with `AssemblyLoadContext`
- Normalize path handling for cross-platform
