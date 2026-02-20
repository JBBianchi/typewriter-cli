# F-0001: Repository Structure Inventory

**Date**: 2026-02-19
**Status**: Complete

## Context
Structural inventory of the upstream Typewriter repository to identify projects, layers, and responsibilities.

## Evidence

### Solution File
- **Path**: `origin/Typewriter.sln`
- Visual Studio 2017+ format, 6 Typewriter projects + 3 Buildalyzer projects
- Configurations: Debug|Any CPU, Release|Any CPU

### Project Hierarchy
```
Typewriter (VSIX Extension) — origin/src/Typewriter/Typewriter.csproj
├── Typewriter.CodeModel (Abstract Model) — origin/src/CodeModel/Typewriter.CodeModel.csproj
├── Typewriter.Metadata (Metadata Interfaces) — origin/src/Metadata/Typewriter.Metadata.csproj
├── Typewriter.Metadata.Roslyn (Roslyn Impl) — origin/src/Roslyn/Typewriter.Metadata.Roslyn.csproj
├── Typewriter.ItemTemplates (VS Templates) — origin/src/ItemTemplates/Typewriter.ItemTemplates.csproj
└── Buildalyzer — origin/Buildalyzer/src/Buildalyzer/Buildalyzer.csproj
    ├── Buildalyzer.Logger — origin/Buildalyzer/src/Buildalyzer.Logger/Buildalyzer.Logger.csproj
    └── Buildalyzer.Workspaces — origin/Buildalyzer/src/Buildalyzer.Workspaces/Buildalyzer.Workspaces.csproj

Typewriter.Tests — origin/src/Tests/Typewriter.Tests.csproj (references all above)
```

### Target Framework
- **ALL projects target .NET Framework 4.7.2** — not .NET Core/.NET 5+
- Test project uses SDK-style format but still targets net472

### Layer Responsibilities
1. **Typewriter.CodeModel** — Public API surface for templates: abstract model classes (`Class`, `Interface`, `Type`, `Method`, `Property`, etc.), collection interfaces, extensions (`TypeExtensions`, `WebApi`), configuration abstractions (`Settings`, `PartialRenderingMode`). No external NuGet deps.
2. **Typewriter.Metadata** — Metadata provider contracts (`IFileMetadata`, `IClassMetadata`, `ITypeMetadata`, etc.) + `IMetadataProvider`. References CodeModel only.
3. **Typewriter.Metadata.Roslyn** — Roslyn-based implementations of all metadata interfaces. Depends on VS SDK + Roslyn LanguageServices. Key VS dep: `VisualStudioWorkspace`.
4. **Typewriter (main)** — VSIX entry point, template editor (syntax highlighting, IntelliSense), code generation (Compiler, Parser, TemplateCodeParser), CodeModel implementations (`ClassImpl`, `MethodImpl`, etc.), collection implementations, VS integration (`ExtensionPackage`, commands, options).
5. **Typewriter.ItemTemplates** — VS item templates (`.tst` file templates for Angular, Models, Empty). VS-only.
6. **Buildalyzer** — Embedded fork of Buildalyzer for design-time MSBuild analysis. Referenced by Typewriter but **not actively used at runtime** — VS uses `VisualStudioWorkspace` instead.

### Key External Dependencies
| Package | Version | Used In |
|---------|---------|---------|
| Microsoft.CodeAnalysis.CSharp.Workspaces | 4.14.0 | Roslyn, Buildalyzer.Workspaces |
| Microsoft.VisualStudio.SDK | 17.14.40265 | Roslyn, Typewriter |
| Microsoft.VisualStudio.LanguageServices | 4.14.0 | Roslyn, Typewriter, Tests |
| Microsoft.VSSDK.BuildTools | 17.14.2120 | Typewriter |
| Microsoft.Build | 17.14.28 | Buildalyzer, Tests |
| Newtonsoft.Json | 13.0.4 | Roslyn |

### Directory Configuration
- `origin/Directory.Build.props` — code analysis ruleset, StyleCop config
- `origin/src/Directory.Build.props` — imports parent, suppresses warnings, references `Microsoft.VisualStudio.Threading.Analyzers`
- `origin/nuget.config` — uses api.nuget.org

## Conclusion
The repository has clean layer separation: CodeModel (public API) → Metadata (interfaces) → Roslyn (implementation). The main VSIX project mixes generation logic with VS UI concerns — extraction requires separating these. All projects target net472 (Windows-only), so the CLI must retarget to modern .NET.

## Impact
- CodeModel and Metadata layers are **VS-independent** and can be ported to .NET 10 with minimal changes
- Roslyn layer has hard VS SDK deps that must be replaced
- Main Typewriter project mixes generation core with VS UI — needs surgical extraction
- Buildalyzer is already embedded and provides CLI-compatible project loading patterns

## Next Steps
- F-0002: Map VS dependencies
- F-0003: Analyze Roslyn metadata layer
- F-0004: Analyze generation pipeline
