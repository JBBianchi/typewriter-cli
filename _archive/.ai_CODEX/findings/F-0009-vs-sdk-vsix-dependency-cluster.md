# F-0009: Visual Studio SDK and VSIX Dependency Cluster

- ID: F-0009
- Title: VS SDK/VSIX packaging dependencies and CLI replacement plan
- Date: 2026-02-19

## Context
Identify host/platform dependencies in packaging/build metadata and classify them for CLI replacement.

## Evidence
- `origin/src/Typewriter/Typewriter.csproj:13` `ProjectTypeGuids` includes VSIX flavor GUID.
- `origin/src/Typewriter/Typewriter.csproj:19` `GeneratePkgDefFile=true`.
- `origin/src/Typewriter/Typewriter.csproj:21` `IncludeAssemblyInVSIXContainer=true`.
- `origin/src/Typewriter/Typewriter.csproj:27` `StartProgram` points to `devenv.exe`.
- `origin/src/Typewriter/Typewriter.csproj:240` `PackageReference` `Microsoft.VisualStudio.Debugger.Contracts`.
- `origin/src/Typewriter/Typewriter.csproj:241` `PackageReference` `Microsoft.VisualStudio.LanguageServices`.
- `origin/src/Typewriter/Typewriter.csproj:242` `PackageReference` `Microsoft.VisualStudio.SDK`.
- `origin/src/Typewriter/Typewriter.csproj:245` `PackageReference` `Microsoft.VSSDK.BuildTools`.
- `origin/src/Typewriter/Typewriter.csproj:255` imports `Microsoft.VsSDK.targets`.
- `origin/src/Typewriter/source.extension.vsixmanifest:18`/`:21`/`:24` VS installation targets.
- `origin/src/Typewriter/source.extension.vsixmanifest:41` VS core editor prerequisite.
- `origin/src/Typewriter/source.extension.vsixmanifest:44`/`:45`/`:47` VS package/MEF/item template assets.

## Conclusion
The build/package layer is explicitly VSIX/VSSDK-centric and cannot be reused in a cross-platform CLI without full replacement.

## Impact
- Classification:
  - Hard: VSIX manifest, VSSDK build targets, VS package references, package attributes.
  - Soft: Item template asset packaging for VS new-item UX.
  - Accidental: None identified in this cluster.
- CLI replacement:
  - Replace VSIX with SDK-style `dotnet` CLI projects.
  - Replace VS packaging with `dotnet tool` packaging.
  - Move non-host-specific resources (template samples/docs) to optional package content.

## Next steps
- Reflect this replacement in architecture and phased plan.
- Keep parity matrix focused on generation/runtime features, not VS installer surfaces.
