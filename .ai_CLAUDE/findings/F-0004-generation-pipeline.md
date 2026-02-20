# F-0004: Generation Pipeline Architecture

**Date**: 2026-02-19
**Status**: Complete

## Context
How Typewriter discovers, parses, compiles, and executes templates to produce output files.

## Evidence

### Pipeline Flow
```
.tst Template File
    → TemplateCodeParser.Parse() — extract code blocks, lambdas, #references
    → ShadowClass — construct compilable C# wrapper class
    → Compiler.Compile() — Roslyn CSharpCompilation → temp assembly
    → Parser.Parse() / SingleFileParser.Parse() — evaluate template against model
    → Template.WriteFile() — write output with change detection
```

### Step 1: Template Code Extraction
- **File**: `origin/src/Typewriter/Generation/TemplateCodeParser.cs`
- **Method**: `static string Parse(ProjectItem, string template, List<Type> extensions)`
- Extracts: `#reference "path.dll"` directives, `${ ... }` C# code blocks, `$Identifier(=>filter)` lambda filters, `using` statements

### Step 2: ShadowClass Construction
- **File**: `origin/src/Typewriter/TemplateEditor/Lexing/Roslyn/ShadowClass.cs`
- Generates namespace `__Typewriter` with class `Template` containing extracted code + lambda methods
- Methods: `AddUsing()`, `AddBlock()`, `AddLambda()`, `Compile(path)`

### Step 3: Roslyn Compilation
- **File**: `origin/src/Typewriter/Generation/Compiler.cs`
- **Method**: `static Type Compile(ProjectItem, ShadowClass)`
- Copies referenced assemblies to temp dir, compiles via CSharpCompilation, loads assembly, extracts `__Typewriter.Template` type
- Reports diagnostics to VS error list (must redirect to console in CLI)

### Step 4: Template Evaluation
- **Multi-file mode (default)**: `origin/src/Typewriter/Generation/Parser.cs` — evaluates template per-file
- **Single-file mode**: `origin/src/Typewriter/Generation/SingleFileParser.cs` — evaluates against File[] array
- **Resolution**: Reflection on context object (`type.GetProperty(identifier)`) then extension methods
- **Filters**: `origin/src/Typewriter/Generation/ItemFilter.cs` — wildcard, attribute, inheritance filters

### Step 5: Output Writing
- **File**: `origin/src/Typewriter/Generation/Template.cs`
- **Path resolution**: `Settings.OutputDirectory` or template dir; `Settings.OutputFilenameFactory(file)` or `sourceName + extension`
- **Change detection**: `HasChanged()` — skips write if content identical (incremental)
- **UTF-8 BOM**: Controlled by `Settings.Utf8BomGeneration`
- **Collision handling**: Appends `(1)`, `(2)` for same-name outputs

### Template Syntax Summary
| Syntax | Type | Behavior |
|--------|------|----------|
| `$Identifier` | Property | Output value |
| `$Collection[template]` | Collection | Repeat for each item |
| `$Boolean[true][false]` | Boolean | Conditional |
| `$parent` | Navigation | Access parent context |
| `$Collection(filter)[template]` | Filtered | Wildcard/lambda filter |
| `${ code }` | Code block | C# executed at compile time |
| `#reference "path"` | Directive | Assembly reference |
| `[separator]` | Separator | Between collection items |

### Configuration via Template Code (`SettingsImpl`)
- **File**: `origin/src/Typewriter/CodeModel/Configuration/SettingsImpl.cs`
- Templates call: `Settings.IncludeCurrentProject()`, `Settings.IncludeAllProjects()`, `Settings.SingleFileMode("output.ts")`, `Settings.OutputExtension = ".tsx"`
- Properties: `IncludedProjects`, `IsSingleFileMode`, `SingleFileName`, `OutputExtension`, `OutputDirectory`, `OutputFilenameFactory`, `StringLiteralCharacter`, `StrictNullGeneration`, `Utf8BomGeneration`, `PartialRenderingMode`

### VS Dependencies in Pipeline
| Component | VS Dependency | Replaceable? |
|-----------|--------------|--------------|
| `TemplateCodeParser.Parse()` | `ProjectItem` param (for path resolution) | Yes → file path |
| `Compiler.Compile()` | `ProjectItem` param (for error reporting) | Yes → file path |
| `Parser.Parse()` | `ProjectItem` param (for error reporting) | Yes → file path |
| `Template` constructor | `ProjectItem` field (for DTE, project items) | Yes → project context object |
| `Template.SaveProjectFile()` | Adds file to VS project | Remove in CLI |
| `Template.CheckOutFileFromSourceControl()` | TFS checkout | Remove in CLI |
| `ErrorList.Add*()` | VS Error List Provider | Replace → console diagnostics |

### CodeModel Implementation Layer (VS-independent)
- **File**: `origin/src/Typewriter/CodeModel/Implementation/ClassImpl.cs` (and peers)
- Pattern: wraps `IClassMetadata` → exposes abstract `Class` API
- All use lazy evaluation: `_field ?? (_field = XxxImpl.FromMetadata(...))`
- **No VS dependencies** — operates purely on metadata interfaces
- Special: `ClassImpl.GetPropertiesFromClassMetadata()` — handles `[MetadataType]` / `[ModelMetadataType]` attribute merging

## Conclusion
The generation pipeline core (TemplateCodeParser → ShadowClass → Compiler → Parser → Template.WriteFile) is extractable. `ProjectItem` threads through method signatures but is used mainly for file path resolution and error reporting — replaceable with strings. The CodeModel implementation layer is entirely VS-independent.

## Impact
- **Reuse as-is**: Parser, SingleFileParser, ItemFilter, ShadowClass core logic, all *Impl classes, Helpers
- **Adapt**: Template (replace ProjectItem, remove SaveProjectFile/SourceControl), Compiler (redirect diagnostics), TemplateCodeParser (replace ProjectItem with path)
- **Drop**: EventQueue, SolutionMonitor, GenerationController (batch mode replaces event-driven)

## Next Steps
- F-0007: ProjectItem replacement design
- F-0008: Type mapping and extension system
