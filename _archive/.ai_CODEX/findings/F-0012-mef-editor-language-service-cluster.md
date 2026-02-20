# F-0012: MEF Editor and Language Service Dependency Cluster

- ID: F-0012
- Title: VS editor/language-service features are non-CLI host features
- Date: 2026-02-19

## Context
Template editor and language features may be mistaken for generation-core parity. We need explicit classification.

## Evidence
- `origin/src/Typewriter/VisualStudio/LanguageService.cs:10` `LanguageService : IVsLanguageInfo, IVsLanguageTextOps`.
- `origin/src/Typewriter/VisualStudio/LanguageService.cs:12`/`:17`/`:22` `[Export]` MEF content-type and classification registrations.
- `origin/src/Typewriter/TemplateEditor/Controllers/CompletionController.cs:17` `[Export(typeof(IVsTextViewCreationListener))]`.
- `origin/src/Typewriter/TemplateEditor/Controllers/CompletionSourceProvider.cs:9` `[Export(typeof(ICompletionSourceProvider))]`.
- `origin/src/Typewriter/TemplateEditor/Controllers/QuickInfoController.cs:14` `[Export(typeof(IAsyncQuickInfoSourceProvider))]`.
- `origin/src/Typewriter/TemplateEditor/Controllers/ClassificationController.cs:11` `[Export(typeof(IClassifierProvider))]`.
- `origin/src/Typewriter/TemplateEditor/Controllers/FormattingController.cs:73` `[Export(typeof(IVsTextViewCreationListener))]`.
- `origin/src/Typewriter/TemplateEditor/Editor.cs:17` editor services perform lexing/classification/completion context over VS text buffers.
- `origin/src/Typewriter/source.extension.vsixmanifest:45` VSIX ships `Microsoft.VisualStudio.MefComponent`.

## Conclusion
MEF editor/language-service functionality is Visual Studio UX, not generation-core runtime logic.

## Impact
- Classification:
  - Hard (for VS extension only): editor completion/quick info/classification host integration.
  - Soft (for CLI product): entirely optional and out-of-scope.
  - Accidental: none.
- CLI replacement:
  - No replacement needed for initial CLI scope.
  - Keep template language docs/tests to preserve parser behavior, not editor integrations.

## Next steps
- Mark editor features as explicit non-goal in implementation plan.
- Keep any parser/lexing code only if required by generation semantics.
