# P-0001: Feature Matrix

- ID: P-0001
- Title: Feature parity matrix (upstream Typewriter -> CLI spin-off)
- Date: 2026-02-19

| Feature | Upstream Location | CLI Parity | Notes |
|---|---|---|---|
| Template discovery across solution projects | `origin/src/Typewriter/Generation/Controllers/TemplateController.cs:86` | ✅ identical | Re-implement via project graph/project items instead of DTE traversal. |
| Template compile pre-pass (`#reference`, `${...}`, lambda rewrite) | `origin/src/Typewriter/Generation/TemplateCodeParser.cs:19` | ✅ identical | Keep compile-time semantics and generated extension loading. |
| Runtime parser token expansion (`$Identifier`, blocks, separators) | `origin/src/Typewriter/Generation/Parser.cs:57` | ✅ identical | Golden tests required for syntax edge cases. |
| Item filtering (`*`, attribute `[]`, inheritance `:`) | `origin/src/Typewriter/Generation/ItemFilter.cs:10` | ✅ identical | Preserve selector logic and matching behavior. |
| Single-file mode rendering | `origin/src/Typewriter/Generation/Template.cs:152` | ✅ identical | Preserve `Settings.SingleFileMode` and filename behavior. |
| Include current project | `origin/src/Typewriter/CodeModel/Configuration/SettingsImpl.cs:137` | ✅ identical | Replace DTE project resolution with graph identity mapping. |
| Include referenced projects | `origin/src/Typewriter/CodeModel/Configuration/SettingsImpl.cs:126` | ✅ identical | Replace `VSLangProj` traversal with graph references. |
| Include all projects | `origin/src/Typewriter/CodeModel/Configuration/SettingsImpl.cs:148` | ✅ identical | Performance safeguards needed in large solutions. |
| Include project by name | `origin/src/Typewriter/CodeModel/Configuration/ProjectHelpers.cs:15` | 🟨 partial | Requires policy for duplicate names (`Q-0001`). |
| Output extension and filename factory | `origin/src/CodeModel/Configuration/Settings.cs:15`, `origin/src/Typewriter/Generation/Template.cs:410` | ✅ identical | Keep illegal-char normalization and fallback rules. |
| Output directory resolution (relative to template) | `origin/src/Typewriter/Generation/Template.cs:387` | ✅ identical | Preserve relative path semantics. |
| Output collision naming (`name (i).ext`) | `origin/src/Typewriter/Generation/Template.cs:359` | ✅ identical | Preserve deterministic collision iteration. |
| Skip unchanged output writes | `origin/src/Typewriter/Generation/Template.cs:460` | ✅ identical | Avoid file churn in CI and watch scenarios. |
| UTF-8 BOM toggle | `origin/src/Typewriter/Generation/Template.cs:187`, `origin/src/CodeModel/Configuration/Settings.cs:79` | ✅ identical | Keep default and disable semantics. |
| Partial rendering mode (`Partial`/`Combined`) | `origin/src/Roslyn/RoslynFileMetadata.cs:64`, `origin/src/Roslyn/RoslynClassMetadata.cs:81` | ✅ identical | Preserve request-render behavior for combined mode (`Q-0004`). |
| Class/interface/enum/delegate/event/field/property metadata | `origin/src/Roslyn/RoslynClassMetadata.cs:49` | ✅ identical | High-fidelity Roslyn semantics required. |
| Record metadata | `origin/src/Roslyn/RoslynRecordMetadata.cs:10` | ✅ identical | Include nested/containing record behavior. |
| Type semantics (nullable/task/tuple/dictionary/enumerable) | `origin/src/Roslyn/RoslynTypeMetadata.cs:155` | ✅ identical | Regression tests needed; parity-critical. |
| WebApi extension helpers (`HttpMethod`, `Url`, `RequestData`) | `origin/src/CodeModel/Extensions/WebApi/UrlExtensions.cs:12` | ✅ identical | Keep route parsing and query parameter behavior. |
| Type extension helpers (`Default`, `Unwrap`, `ClassName`) | `origin/src/CodeModel/Extensions/Types/TypeExtensions.cs:10` | ✅ identical | Keep strict-null interactions and string literal char use. |
| Template diagnostics (compile + parse/runtime) | `origin/src/Typewriter/Generation/Compiler.cs:62`, `origin/src/Typewriter/Generation/Parser.cs:214` | ✅ identical | Surface via CLI logs/exit codes instead of VS Error List. |
| Render-on-save / source-file tracking triggers | `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:113`, `origin/src/Typewriter/Generation/Controllers/SolutionMonitor.cs:269` | 🟨 partial | CLI v1 focuses on explicit command execution; watch mode deferred (`Q-0003`). |
| Add generated files to project | `origin/src/Typewriter/Generation/Template.cs:212` | 🟨 partial | Possible parity gap/optional feature; requires scope decision (`Q-0002`). |
| Source mapping via `CustomToolNamespace` | `origin/src/Typewriter/Generation/Template.cs:317` | 🟨 partial | VS project-item metadata has no direct CLI equivalent. |
| Source control checkout before overwrite | `origin/src/Typewriter/Generation/Template.cs:495` | ❌ not planned | VS/DTE-specific; mitigation: rely on VCS workflow outside tool. |
| VS output window + status bar + error list UX | `origin/src/Typewriter/VisualStudio/Log.cs:92`, `origin/src/Typewriter/Generation/Controllers/EventQueue.cs:76`, `origin/src/Typewriter/VisualStudio/ErrorList.cs:8` | 🟨 partial | Replaced by console/structured CLI diagnostics. |
| VS context menu commands (Render one/all) | `origin/src/Typewriter/VisualStudio/ContextMenu/RenderTemplate.cs:23` | 🟨 partial | Replaced by CLI command/options. |
| Template editor language services (completion/quick info/classification) | `origin/src/Typewriter/TemplateEditor/Controllers/CompletionController.cs:17` | ❌ not planned | Out of CLI scope; no generation parity impact. |
| Registry-based icon registration | `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:229` | ❌ not planned | VS/Windows shell integration only. |
| Registry-gated long path behavior | `origin/src/Typewriter/Generation/Template.cs:555` | 🟨 partial | Cross-platform path strategy will replace registry probing. |
