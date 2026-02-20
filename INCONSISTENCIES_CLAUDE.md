# Inconsistencies Between `.ai/` Documents and `IMPLEMENTATION_PLAN.md`

**Date**: 2026-02-19

---

## 1. Decision ID Numbering vs AGENTS.md Mandate

**Severity**: Low (cosmetic)

`AGENTS.md:132` mandates the loading strategy decision go in `D-0001-project-loading-strategy.md`. The actual files are:

| AGENTS.md Spec | Actual File |
|---|---|
| `D-0001-project-loading-strategy.md` | `D-0003-project-loading-strategy.md` |
| `D-000X-target-framework.md` (placeholder) | `D-0001-target-framework.md` |
| `D-000Y-packaging.md` (placeholder) | `D-0002-packaging-strategy.md` |

Target framework took `D-0001`, pushing the loading strategy to `D-0003`. The `X` and `Y` placeholders in AGENTS.md suggest flexibility for those two, but the loading strategy ID is explicitly specified as `D-0001`.

**Fix**: Either renumber the decisions or accept the deviation and note it.

---

## 2. Prototype Filename Mismatch

**Severity**: Low (cosmetic)

| AGENTS.md Spec (line 131) | Actual File |
|---|---|
| `PR-0001-msbuild-load-spike.md` | `PR-0001-msbuild-loading-spike.md` |

"load" vs "loading" — minor but breaks any tooling that expects the exact filename.

**Fix**: Rename to match AGENTS.md or update AGENTS.md.

---

## 3. Roslyn Metadata Class Count: 17 vs 18

**Severity**: Low (accuracy)

F-0003's **table** (lines 32–49) lists **18** Roslyn metadata classes:
`RoslynFileMetadata`, `RoslynClassMetadata`, `RoslynInterfaceMetadata`, `RoslynRecordMetadata`, `RoslynEnumMetadata`, `RoslynTypeMetadata`, `RoslynMethodMetadata`, `RoslynPropertyMetadata`, `RoslynFieldMetadata`, `RoslynConstantMetadata`, `RoslynStaticReadOnlyFieldMetadata`, `RoslynAttributeMetadata`, `RoslynAttributeArgumentMetadata`, `RoslynParameterMetadata`, `RoslynDelegateMetadata`, `RoslynEventMetadata`, `RoslynTypeParameterMetadata`, `RoslynVoidTaskMetadata`

But:
- F-0003 conclusion (line 76): "The 17 `Roslyn*Metadata` classes"
- F-0003 impact (line 79): "~17 metadata classes"
- IMPLEMENTATION_PLAN Phase 5 (line 375): "Port all `Roslyn*Metadata` classes (17 files)"

The table has 18 entries. The text consistently says 17. It appears `RoslynVoidTaskMetadata` (synthetic, no backing file) may have been excluded from the count, but this isn't stated.

**Fix**: Clarify: either "18 classes (17 files + 1 synthetic)" or correct the table/text to match.

---

## 4. Watch Mode Deferral: "post-v1.0" vs "v2.0"

**Severity**: Low (ambiguity)

| Document | Wording |
|---|---|
| IMPLEMENTATION_PLAN §2 (line 27) | "deferred to post-v1.0" |
| IMPLEMENTATION_PLAN §6.1 (line 139) | "Future commands (post-v1.0)" |
| IMPLEMENTATION_PLAN §13 (line 562) | "Deferred to v2.0" |
| Q-0004 (line 15) | "Deferred to post-v1.0" |

"post-v1.0" and "v2.0" are not the same — post-v1.0 could mean v1.1. Three sources say "post-v1.0", one says "v2.0".

**Fix**: Align on a single term across all documents.

---

## 5. ShadowClass: "Reuse as-is" vs "Adapt for .NET 10"

**Severity**: Medium (contradictory guidance)

| Document | Claim |
|---|---|
| F-0004 impact (line 89) | "**Reuse as-is**: Parser, SingleFileParser, ItemFilter, ShadowClass core logic, all *Impl classes, Helpers" |
| IMPLEMENTATION_PLAN Phase 6 (line 396) | "Port `ShadowClass` — adapt for .NET 10 compilation" |
| P-0001 (item #5) | "ShadowClass compilation ... ✅ identical ... CSharpCompilation to temp assembly" |

F-0004 says ShadowClass can be reused as-is. The plan says it needs adaptation for .NET 10. The parity matrix says "identical". These can't all be true — the ShadowClass compiles C# code blocks via `CSharpCompilation` and uses `Assembly.LoadFrom()`, which needs `AssemblyLoadContext` on .NET 10 (as documented in R-0003).

**Fix**: F-0004 should say "adapt" not "reuse as-is" for ShadowClass. P-0001 parity is about *template behavior* (correctly "identical") but the *implementation* does need changes.

---

## 6. F-0004 and F-0002 "Next Steps" Point to Wrong Finding

**Severity**: Low (cross-reference error)

| Document | Next Steps Text | Actual Content |
|---|---|---|
| F-0004 (line 94) | "F-0007: ProjectItem replacement design" | F-0007 is "Configuration Model and Settings" |
| F-0002 (line 114) | "F-0007: ProjectItem replacement design" | Same — F-0007 doesn't cover ProjectItem replacement |

Both findings point to F-0007 for "ProjectItem replacement design", but F-0007 is about the Settings/Configuration model. ProjectItem replacement is discussed within F-0002 and F-0004 themselves, and in the IMPLEMENTATION_PLAN §4.2, but there is no dedicated finding document for it.

**Fix**: Either create `F-0009-projectitem-replacement.md` or update the next-steps references to point to the plan §4.2 instead.

---

## 7. Source Generator Investigation: Conflicting MSBuild Properties

**Severity**: Low (investigation detail)

| Document | Suggested Property |
|---|---|
| Q-0001 investigation item 3 | `additionalProperties["CompilerGeneratedFilesOutputPath"]` |
| R-0001 mitigation item 3 | `GenerateDocumentationFile` property |

These reference different MSBuild properties for the same concern (source generator output in MSBuildWorkspace). Neither is definitively correct — both are speculative. The actual investigation (Phase 5) will determine the right approach.

**Fix**: Align or note both as candidates to test.

---

## 8. Long Path Handling: Contradictory Wording

**Severity**: Low (wording)

| Document | Claim |
|---|---|
| P-0001 item #19 | "🟨 partial ... .NET 10 handles long paths natively; Windows `\\?\` prefix unnecessary" |
| PR-0001 cross-platform table | "Long paths ... May need `\\?\` prefix (net10.0 handles this)" |

P-0001 says the prefix is "unnecessary". PR-0001 says "May need" but then qualifies with "(net10.0 handles this)". The PR-0001 wording is ambiguous — it could be read as "you may still need it" or "net10.0 handles it so you don't need it".

**Fix**: PR-0001 should say "No `\\?\` prefix needed (net10.0 handles long paths natively)" to match P-0001.

---

## 9. IMPLEMENTATION_PLAN §8.1: ShadowClass in Wrong Module

**Severity**: Medium (architecture)

The module diagram (lines 196–225) places `ShadowClass` inside `Typewriter.Generation`:
```
│  Typewriter.Generation     │
│  - ShadowClass             │
```

But the upstream file is `origin/src/Typewriter/TemplateEditor/Lexing/Roslyn/ShadowClass.cs` — it lives in the template editor/lexing layer, not the generation layer. The plan's Phase 6 correctly ports it as part of template execution. However, `ShadowClass` constructs a compilable C# class from extracted code blocks — this is compilation infrastructure, and placing it in `Typewriter.Generation` is a reasonable architectural choice for the CLI. But it should be explicitly noted as a deliberate rearchitecting rather than a direct port.

**Fix**: Minor — add a note in Phase 6 that ShadowClass is relocated from `TemplateEditor/Lexing/Roslyn/` to `Generation/`.

---

## 10. F-0005: Buildalyzer Microsoft.Extensions.Logging Version

**Severity**: Low (data accuracy)

F-0005 (line 47) states Buildalyzer uses `Microsoft.Extensions.Logging` **v10.0.0**. The embedded Buildalyzer targets .NET Framework 4.7.2 and is a fork — it's unlikely to reference v10.0.0 of this package. This version may have been read from a restored packages cache rather than the actual `.csproj` source.

**Fix**: Verify against the actual `Buildalyzer.csproj` PackageReference and correct if needed.

---

## Summary

| # | Issue | Severity | Type |
|---|---|---|---|
| 1 | Decision ID numbering vs AGENTS.md | Low | Naming convention |
| 2 | Prototype filename mismatch | Low | Naming convention |
| 3 | Metadata class count 17 vs 18 | Low | Accuracy |
| 4 | Watch mode "post-v1.0" vs "v2.0" | Low | Ambiguity |
| 5 | ShadowClass reuse vs adapt | Medium | Contradictory guidance |
| 6 | F-0004/F-0002 next steps → wrong finding | Low | Cross-reference error |
| 7 | Source generator MSBuild properties | Low | Investigation detail |
| 8 | Long path wording contradiction | Low | Wording |
| 9 | ShadowClass module placement | Medium | Architecture clarity |
| 10 | Buildalyzer logging version | Low | Data accuracy |

**No critical inconsistencies found.** The two medium-severity items (#5, #9) involve ShadowClass — its reusability claim and module placement should be clarified before implementation begins.
