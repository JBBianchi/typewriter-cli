# P-0001: Feature Parity Matrix

**Date**: 2026-02-19
**Status**: Complete

## Feature Matrix

| # | Feature | Upstream Location | CLI Parity | Notes |
|---|---------|-------------------|------------|-------|
| **Template Discovery & Execution** | | | |
| 1 | `.tst` template parsing | `Generation/TemplateCodeParser.cs` | ✅ identical | Port parser logic directly |
| 2 | `${ }` code block extraction | `Generation/TemplateCodeParser.cs` | ✅ identical | Compiled to ShadowClass |
| 3 | `#reference` directive | `Generation/TemplateCodeParser.cs:212-244` | ✅ identical | Assembly reference loading; path resolution needs adaptation (see #30) |
| 4 | Lambda filter expressions | `Generation/TemplateCodeParser.cs:154-210` | ✅ identical | `$Classes($x=>x.IsPublic)[...]` |
| 5 | ShadowClass compilation | `TemplateEditor/Lexing/Roslyn/ShadowClass.cs` | ✅ identical | CSharpCompilation to temp assembly |
| 6 | Template evaluation (multi-file) | `Generation/Parser.cs` | ✅ identical | Per-file rendering |
| 7 | Template evaluation (single-file) | `Generation/SingleFileParser.cs` | ✅ identical | Array of files → single output |
| 8 | Item filtering (wildcard/attr/inherit) | `Generation/ItemFilter.cs` | ✅ identical | `$Classes(filter)[...]` patterns |
| 9 | Separator blocks | `Generation/Parser.cs` | ✅ identical | `[separator]` between items |
| 10 | Boolean conditionals | `Generation/Parser.cs` | ✅ identical | `$IsPublic[true][false]` |
| 11 | `$parent` navigation | `Generation/Parser.cs` | ✅ identical | Context stack navigation |
| **Output & File Writing** | | | |
| 12 | Output path resolution | `Generation/Template.cs:GetOutputPath()` | ✅ identical | Directory + filename + extension |
| 13 | Output extension configuration | `Settings.OutputExtension` | ✅ identical | Default `.ts` |
| 14 | Output filename factory | `Settings.OutputFilenameFactory` | ✅ identical | Custom `Func<File, string>` |
| 15 | Output directory override | `Settings.OutputDirectory` | ✅ identical | Absolute or relative |
| 16 | Change detection (skip unchanged) | `Template.cs:HasChanged()` | ✅ identical | Compare before write |
| 17 | UTF-8 BOM control | `Settings.Utf8BomGeneration` | ✅ identical | Default true |
| 18 | Collision handling (numbered) | `Template.cs:GetOutputPath()` | ✅ identical | `Name (1).ts`, `Name (2).ts` |
| 19 | Long path support | `Template.cs:IsLongPathEnabled()` | 🟨 partial | .NET 10 handles long paths natively; Windows `\\?\` prefix unnecessary |
| **Configuration** | | | |
| 20 | `Settings.IncludeCurrentProject()` | `SettingsImpl.cs:137-146` | ✅ identical | Maps to MSBuild project containing template |
| 21 | `Settings.IncludeReferencedProjects()` | `SettingsImpl.cs:126-135` | ✅ identical | Maps to MSBuild `ProjectReference` items |
| 22 | `Settings.IncludeAllProjects()` | `SettingsImpl.cs:148-162` | ✅ identical | All projects in solution |
| 23 | `Settings.IncludeProject(name)` | `SettingsImpl.cs:107-116` | ✅ identical | Find project by name in solution |
| 24 | `Settings.SingleFileMode(filename)` | `SettingsImpl.cs:118-124` | ✅ identical | Merge all files into one output |
| 25 | `Settings.StringLiteralCharacter` | `SettingsImpl.cs:80` | ✅ identical | Default `"` |
| 26 | `Settings.StrictNullGeneration` | `SettingsImpl.cs:90` | ✅ identical | `T \| null` for nullable types |
| 27 | `Settings.Utf8BomGeneration` | `SettingsImpl.cs:95` | ✅ identical | UTF-8 BOM in output |
| 28 | `Settings.PartialRenderingMode` | `Settings.cs:27` | ✅ identical | Partial vs Combined |
| 29 | `Settings.SkipAddingGeneratedFilesToProject` | `Settings.cs:42` | ❌ not planned | VS-only feature. CLI doesn't modify project files. |
| **Path Resolution** | | | |
| 30 | `~\` project-relative paths | `VisualStudio/PathResolver.cs` | ✅ identical | CLI: relative to project dir |
| 31 | `~~\` solution-relative paths | `VisualStudio/PathResolver.cs` | ✅ identical | CLI: relative to solution dir |
| 32 | Implicit relative paths | `VisualStudio/PathResolver.cs` | ✅ identical | Relative to template dir |
| **Semantic Model / Type System** | | | |
| 33 | Class extraction | `Roslyn/RoslynClassMetadata.cs` | ✅ identical | Via Roslyn INamedTypeSymbol |
| 34 | Interface extraction | `Roslyn/RoslynInterfaceMetadata.cs` | ✅ identical | |
| 35 | Record extraction | `Roslyn/RoslynRecordMetadata.cs` | ✅ identical | |
| 36 | Enum extraction | `Roslyn/RoslynEnumMetadata.cs` | ✅ identical | |
| 37 | Delegate extraction | `Roslyn/RoslynDelegateMetadata.cs` | ✅ identical | |
| 38 | Method metadata | `Roslyn/RoslynMethodMetadata.cs` | ✅ identical | |
| 39 | Property metadata | `Roslyn/RoslynPropertyMetadata.cs` | ✅ identical | |
| 40 | Field/Constant/StaticReadOnly | `Roslyn/RoslynFieldMetadata.cs` + peers | ✅ identical | |
| 41 | Attribute metadata | `Roslyn/RoslynAttributeMetadata.cs` | ✅ identical | |
| 42 | Parameter metadata | `Roslyn/RoslynParameterMetadata.cs` | ✅ identical | |
| 43 | Event metadata | `Roslyn/RoslynEventMetadata.cs` | ✅ identical | |
| 44 | Type parameter metadata | `Roslyn/RoslynTypeParameterMetadata.cs` | ✅ identical | |
| 45 | Nullable<T> unwrapping | `Roslyn/RoslynTypeMetadata.cs` | ✅ identical | |
| 46 | Task<T> unwrapping | `Roslyn/RoslynTypeMetadata.cs` | ✅ identical | |
| 47 | Generic types | `Roslyn/RoslynTypeMetadata.cs` | ✅ identical | |
| 48 | Nested types | `RoslynClassMetadata.cs:NestedClasses` | ✅ identical | |
| 49 | Partial class support | `RoslynClassMetadata.cs:Members` | ✅ identical | Both Partial and Combined modes |
| 50 | Value tuples | `RoslynTypeMetadata.cs:IsValueTuple` | ✅ identical | |
| 51 | Doc comments | `IClassMetadata.DocComment` | ✅ identical | XML doc parsing |
| 52 | Default values | `RoslynParameterMetadata.cs:DefaultValue` | ✅ identical | |
| **Type Mapping (C# → TypeScript)** | | | |
| 53 | Primitive type mapping | `CodeModel/Helpers.cs:ExtractTypeScriptName()` | ✅ identical | bool→boolean, int→number, etc. |
| 54 | Dictionary → Record | `Helpers.cs:GetTypeScriptName()` | ✅ identical | |
| 55 | Enumerable → T[] | `Helpers.cs:GetTypeScriptName()` | ✅ identical | |
| 56 | CamelCase helper | `Helpers.cs:CamelCase()` | ✅ identical | |
| 57 | `$name` (camelCase) | `ClassImpl.cs:25` | ✅ identical | `$name` vs `$Name` |
| **WebApi Extensions** | | | |
| 58 | HttpMethod() extension | `Extensions/WebApi/HttpMethodExtensions.cs` | ✅ identical | |
| 59 | Url() extension | `Extensions/WebApi/UrlExtensions.cs` | ✅ identical | |
| 60 | RequestData() extension | `Extensions/WebApi/RequestDataExtensions.cs` | ✅ identical | |
| **MetadataType Support** | | | |
| 61 | [MetadataType] attribute merge | `ClassImpl.cs:GetPropertiesFromClassMetadata()` | ✅ identical | EF/code-first scenarios |
| 62 | [ModelMetadataType] support | `ClassImpl.cs:156` | ✅ identical | .NET Core variant |
| **VS-Only Features (Not Applicable)** | | | |
| 63 | Auto-render on .cs save | `SolutionMonitor + GenerationController` | ❌ not planned | VS IDE feature. CLI is batch mode. Mitigation: `--watch` flag (future). |
| 64 | Auto-render on .tst save | `SolutionMonitor + GenerationController` | ❌ not planned | Same as above. |
| 65 | IntelliSense for .tst files | `TemplateEditor/*.cs` | ❌ not planned | VS IDE feature. Mitigation: VS Code extension (future). |
| 66 | Syntax highlighting for .tst | `TemplateEditor/Lexing/*.cs` | ❌ not planned | VS IDE feature. |
| 67 | Add generated files to VS project | `Template.cs:SaveProjectFile()` | ❌ not planned | Not needed; use glob patterns in .csproj instead. |
| 68 | TFS source control checkout | `Template.cs:CheckOutFileFromSourceControl()` | ❌ not planned | Legacy TFS. Git doesn't need checkout. |
| 69 | Error List integration | `VisualStudio/ErrorList.cs` | ❌ not planned | Replaced by console diagnostics + exit codes. |
| 70 | VS Output Window logging | `VisualStudio/Log.cs` | ❌ not planned | Replaced by `--verbosity` console logging. |
| **CLI-Only Features (New)** | | | |
| 71 | `.slnx` support | N/A (new) | ✅ new | New solution format |
| 72 | `--framework <TFM>` flag | N/A (new) | ✅ new | Multi-targeting control |
| 73 | `--restore` flag | N/A (new) | ✅ new | Auto-restore before load |
| 74 | `--output <dir>` override | N/A (new) | ✅ new | Global output directory |
| 75 | Deterministic exit codes | N/A (new) | ✅ new | 0/1/2/3 |
| 76 | Cross-platform (Linux/macOS) | N/A (new) | ✅ new | .NET 10 cross-platform |
| 77 | Template glob input | N/A (new) | ✅ new | `**/*.tst` pattern matching |

## Summary
- **✅ identical**: 58 features (full parity)
- **🟨 partial**: 1 feature (long path — auto-handled by .NET 10)
- **❌ not planned**: 8 features (all VS IDE-specific with documented justification)
- **✅ new**: 7 features (CLI-specific improvements)

## Intentional Gaps Justification
All 8 "not planned" features are **Visual Studio IDE features** that have no meaningful CLI equivalent:
- Auto-render triggers → CLI is batch mode (deterministic, CI-friendly)
- IDE editing features → Out of scope (potential VS Code extension future work)
- Project file modification → Modern .csproj uses globs; no manual file adding needed
- TFS checkout → Legacy; Git is standard
- VS Error List/Output → Replaced by structured console output with severity levels
