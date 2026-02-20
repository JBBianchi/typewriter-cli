# D-0001: Target Framework

- ID: D-0001
- Title: Target framework for `typewriter-cli`
- Date: 2026-02-19

## Context
Select the runtime baseline for a cross-platform CLI replacing a VSIX/.NET Framework extension.

## Evidence
- Upstream core projects target `.NET Framework v4.7.2`:
  - `origin/src/Typewriter/Typewriter.csproj:18`
  - `origin/src/CodeModel/Typewriter.CodeModel.csproj:12`
  - `origin/src/Metadata/Typewriter.Metadata.csproj:12`
  - `origin/src/Roslyn/Typewriter.Metadata.Roslyn.csproj:12`
- Upstream host is Visual Studio-specific (`AsyncPackage`, VS SDK, VSIX), not cross-platform CLI:
  - `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:28`
  - `origin/src/Typewriter/Typewriter.csproj:240`
  - `origin/src/Typewriter/source.extension.vsixmanifest:18`
- CLI requirements require Linux/macOS/Windows CI and modern .NET.
- Spike environment confirms modern SDK toolchain behavior on SDK `10.0.103` with `.slnx` support and project graph loading:
  - `.ai/prototypes/PR-0001-msbuild-loading-spike.md`

## Conclusion
Use `net8.0` as the primary target framework for `typewriter-cli`.

## Impact
- `net8.0` is LTS and broadly available in CI environments.
- Enables modern cross-platform runtime APIs and current MSBuild/Roslyn packages.
- Keeps migration scope controlled; optional secondary TFM can be added later only if a validated compatibility need emerges.

## Next steps
- Implement CLI projects as SDK-style `net8.0` projects.
- Validate package/runtime behavior on Linux/macOS/Windows in CI.
