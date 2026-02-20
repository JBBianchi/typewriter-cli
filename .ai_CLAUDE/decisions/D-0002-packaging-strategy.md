# D-0002: Packaging Strategy

**Date**: 2026-02-19
**Status**: Decided

## Context
The CLI tool needs a distribution mechanism suitable for CI pipelines and developer workstations across platforms.

## Options Considered

### Option A: .NET Global Tool (primary)
- Install via `dotnet tool install -g typewriter-cli`
- Requires .NET 10 SDK on target machine
- Lightweight distribution, auto-updates via NuGet
- CI-friendly: `dotnet tool restore` from tool manifest

### Option B: Self-Contained Executables
- Platform-specific binaries (win-x64, linux-x64, osx-arm64, etc.)
- No SDK requirement on target machine
- Larger binary size (~60-100MB)
- More complex release pipeline

### Option C: Docker Image
- Useful for CI but overly heavy for local use
- Adds Docker dependency

### Option D: MSBuild Task / NuGet Package
- Integrate as `<PackageReference>` + build target
- Alternative distribution for "generate on build" scenario
- Not mutually exclusive with CLI

## Decision
**Primary: .NET Global Tool** (Option A)
**Secondary (future): Self-contained + MSBuild task** (Options B + D, if demand warrants)

## Rationale
1. **CI-native**: `dotnet tool install` / `dotnet tool restore` integrates naturally with CI
2. **Cross-platform**: Single NuGet package works on all platforms
3. **Minimal deps**: Only requires .NET 10 SDK (already present for C# projects)
4. **Versioning**: NuGet semver, easy to pin in CI
5. **Developer UX**: Simple `dotnet tool install -g typewriter-cli`

### Global Tool Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>typewriter-cli</ToolCommandName>
    <PackageId>Typewriter.Cli</PackageId>
  </PropertyGroup>
</Project>
```

### Local Tool Manifest (for CI)
```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "typewriter-cli": {
      "version": "1.0.0",
      "commands": ["typewriter-cli"]
    }
  }
}
```

## Impact
- Primary distribution via NuGet.org
- CI pipelines use `dotnet tool restore` + `typewriter-cli generate`
- Self-contained binaries as optional GitHub Release artifacts

## Next Steps
- Configure PackAsTool in Phase 0
- Set up NuGet packaging in CI
