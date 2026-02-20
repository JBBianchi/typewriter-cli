# F-0001: Solution and Project Inventory

- ID: F-0001
- Title: Upstream solution shape and project responsibilities
- Date: 2026-02-19

## Context
The CLI spin-off must replace the Visual Studio extension host. We need an exact inventory of upstream projects and target frameworks.

## Evidence
- `origin/Typewriter.sln:6` project `Typewriter.ItemTemplates` (`src\ItemTemplates\Typewriter.ItemTemplates.csproj`).
- `origin/Typewriter.sln:8` project `Typewriter.Tests` (`src\Tests\Typewriter.Tests.csproj`).
- `origin/Typewriter.sln:10` project `Typewriter.CodeModel` (`src\CodeModel\Typewriter.CodeModel.csproj`).
- `origin/Typewriter.sln:12` project `Typewriter.Metadata` (`src\Metadata\Typewriter.Metadata.csproj`).
- `origin/Typewriter.sln:14` project `Typewriter.Metadata.Roslyn` (`src\Roslyn\Typewriter.Metadata.Roslyn.csproj`).
- `origin/Typewriter.sln:16` project `Typewriter` (`src\Typewriter\Typewriter.csproj`).
- `origin/Typewriter.sln:20`/`origin/Typewriter.sln:22`/`origin/Typewriter.sln:24` includes vendored `Buildalyzer` projects.
- `origin/src/Typewriter/Typewriter.csproj:18` targets `.NET Framework v4.7.2`.
- `origin/src/CodeModel/Typewriter.CodeModel.csproj:12` targets `.NET Framework v4.7.2`.
- `origin/src/Metadata/Typewriter.Metadata.csproj:12` targets `.NET Framework v4.7.2`.
- `origin/src/Roslyn/Typewriter.Metadata.Roslyn.csproj:12` targets `.NET Framework v4.7.2`.

## Conclusion
Upstream is a Visual Studio extension-centric, .NET Framework 4.7.2 solution with a core model library, metadata abstractions, Roslyn metadata implementation, extension host, and a VS-integrated test suite.

## Impact
- The spin-off is a platform migration, not only a host swap.
- Runtime, packaging, and dependency model must move from VSIX/.NET Framework to modern .NET CLI.
- Buildalyzer appears in solution composition and references but not in extension runtime usage (tracked separately in `F-0015`).

## Next steps
- Map VS-specific surfaces in the `Typewriter` and `Roslyn` projects.
- Use this inventory as the baseline in parity mapping and implementation phases.
