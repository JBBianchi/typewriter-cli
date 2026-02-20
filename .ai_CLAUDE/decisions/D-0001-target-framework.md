# D-0001: Target Framework Selection

**Date**: 2026-02-19
**Status**: Decided (revised)

## Context
The CLI must target a modern .NET version. The upstream targets .NET Framework 4.7.2 (Windows-only). We need cross-platform support for CI pipelines on Linux, macOS, and Windows.

## Options Considered

### Option A: .NET 10 (LTS) ← Selected
- Current LTS release (November 2025), supported until November 2028
- All required Roslyn and MSBuild packages support it
- `Microsoft.CodeAnalysis.Workspaces.MSBuild` supports .NET 10
- Maximum ecosystem compatibility — pre-installed on CI runners
- Latest Roslyn version ships with .NET 10 SDK (better source generator support, analyzers)

### Option B: .NET 8 (LTS — aging)
- Previous LTS, end-of-support November 2026 (~9 months away)
- Starting a new project on a nearly-EOL framework is unwise
- Would require an upgrade to net10.0 almost immediately after shipping

### Option C: .NET 9 (STS — expired)
- STS release, end-of-support May 2026 — effectively dead
- Not a viable target

### Option D: Multi-target net8.0 + net10.0
- Extra complexity without clear benefit
- dotnet tools must specify a single TFM for the tool shim

## Decision
**Target .NET 10 (net10.0)** as the single target framework.

## Rationale
1. **Current LTS**: Supported until November 2028 — 3-year runway
2. **Cross-platform**: Runs on Linux, macOS, Windows without modification
3. **CI compatibility**: Pre-installed on GitHub Actions, Azure DevOps, GitLab CI runners
4. **Dependency support**: All key packages (Roslyn 4.x, MSBuild 17.x) have net10.0 support
5. **dotnet tool**: `dotnet tool install` works natively with net10.0
6. **Latest APIs**: Benefits from latest BCL improvements, better AOT support, performance

## Impact
- All new projects use `<TargetFramework>net10.0</TargetFramework>`
- Upstream code must be ported from net472 to net10.0 (mostly mechanical: remove `using EnvDTE`, add nullable annotations)
- Some .NET Framework APIs need alternatives (e.g., `Assembly.LoadFrom` → `AssemblyLoadContext`)

## Revision History
- 2026-02-19: Initial decision targeted .NET 8. Revised same day to .NET 10 — .NET 8 is nearing EOL and there's no reason to start a new project on an aging LTS when .NET 10 LTS is current.
