# F-0004: Template Discovery and Execution

- ID: F-0004
- Title: Template discovery, compile, and parse flow
- Date: 2026-02-19

## Context
We need parity on discovery of `.tst` files, template compilation behavior, and parse-time features.

## Evidence
- `origin/src/Typewriter/Generation/Controllers/TemplateController.cs:10` class `TemplateController`.
- `origin/src/Typewriter/Generation/Controllers/TemplateController.cs:30` method `LoadTemplates()`.
- `origin/src/Typewriter/Generation/Controllers/TemplateController.cs:86` discovers templates via `Solution.AllProjects().AllProjectItems(Constants.TemplateExtension)`.
- `origin/src/Typewriter/Generation/Controllers/TemplateController.cs:79` method `ResetTemplates()`.
- `origin/src/Typewriter/Generation/Template.cs:18` class `Template`.
- `origin/src/Typewriter/Generation/Template.cs:71` method `LazyTemplate()` reads file and compiles via `TemplateCodeParser.Parse(...)`.
- `origin/src/Typewriter/Generation/Template.cs:94` method `GetFilesToRender()` collects C# files from included projects.
- `origin/src/Typewriter/Generation/Template.cs:105` method `Render(File file, out bool success)` calls `Parser.Parse(...)`.
- `origin/src/Typewriter/Generation/Template.cs:119` method `Render(File[] files, out bool success)` calls `SingleFileParser.Parse(...)`.
- `origin/src/Typewriter/Generation/TemplateCodeParser.cs:19` method `Parse(...)` handles template pre-processing.
- `origin/src/Typewriter/Generation/TemplateCodeParser.cs:35` `ParseReference(...)` supports `#reference`.
- `origin/src/Typewriter/Generation/TemplateCodeParser.cs:40` `ParseCodeBlock(...)` supports `${ ... }` code blocks.
- `origin/src/Typewriter/Generation/TemplateCodeParser.cs:45` `ParseLambda(...)` rewrites lambda filters.
- `origin/src/Typewriter/Generation/TemplateCodeParser.cs:56` compiles shadow class via `Compiler.Compile(...)`.
- `origin/src/Typewriter/Generation/Parser.cs:57` and `origin/src/Typewriter/Generation/SingleFileParser.cs:57` implement `$Identifier` token expansion, filters, blocks, separators, and reflection-based extension calls.

## Conclusion
Template execution has two phases: compile-time preprocessing (`#reference`, `${}`, lambda rewrite) and runtime parse expansion for single-file and per-file modes.

## Impact
- CLI must preserve parser semantics (filters, separators, bool blocks, extensions, single-file behavior).
- Template compile diagnostics and runtime parse diagnostics must be retained in CLI diagnostics output.
- Template discovery must be decoupled from DTE traversal and based on project graph/project items.

## Next steps
- Add parser/renderer features to parity matrix and golden test plan.
- Design a CLI template loader abstraction independent of Visual Studio project item APIs.
