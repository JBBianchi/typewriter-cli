# Questions Folder — Comparison

## 1. Scope

- **Folders compared**:
  - `.ai_CLAUDE/questions/` (4 files: Q-0001 through Q-0004)
  - `.ai_CODEX/questions/` (4 files: Q-0001 through Q-0004)
- **Claude's questions**:
  - Q-0001: Source generator support in MSBuildWorkspace (Open)
  - Q-0002: Template discovery strategy for CLI (Resolved: glob)
  - Q-0003: .NET Framework 4.7.2 API compatibility (Open)
  - Q-0004: Watch mode (Deferred)
- **Codex's questions**:
  - Q-0001: IncludeProject resolution policy (Open)
  - Q-0002: Project mutation parity scope (Open)
  - Q-0003: Watch mode vs. one-shot scope (Open)
  - Q-0004: Partial rendering requestRender equivalence (Open)

## 2. Alignment Analysis

### Agreement: Watch mode is deferred
- Both agents identify watch mode as a question and defer it to post-v1.
- Claude: Q-0004 (deferred). Codex: Q-0003 (open, but recommends deferral).
- **Technical strength**: Sound prioritization. Batch/one-shot mode is simpler and CI-appropriate.
- **Pertinence score**: 4/5
- **Justification**: Correct scoping for v1.

## 3. Divergence Analysis

### Divergence #1
- **Topic**: Question selection — what warrants an open question
- **My position (Claude)**: Questions focus on technical uncertainties: source generator support, API compatibility, template discovery, watch mode.
- **Other agent's position (Codex)**: Questions focus on *design decisions with parity implications*: IncludeProject ambiguity, project mutation scope, watch mode, requestRender callback.
- **Technical evaluation**: Codex's questions are architecturally more impactful. Each of Codex's questions represents a real design decision that affects user-facing behavior. Claude's Q-0002 (template discovery) is already resolved and Q-0003 (API compat) is low-risk by Claude's own admission — these are not high-impact open questions.
- **Who is more correct and why**: Codex's question selection is superior. Open questions should represent genuine design crossroads, not low-risk technical investigations.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 3/5

### Divergence #2
- **Topic**: IncludeProject resolution policy
- **My position (Claude)**: Not identified as an open question at all.
- **Other agent's position (Codex)**: Q-0001 — How should `Settings.IncludeProject(string)` resolve projects in CLI? Identifies that upstream uses DTE `project.Name` matching, which can be ambiguous in monorepos with duplicate names. Proposes alternatives: name-first with ambiguity error, full path matching, assembly name matching, or ordered fallback.
- **Technical evaluation**: This is a genuine and important design question. In a CLI context without DTE, project name resolution semantics change. Duplicate project names in large solutions are realistic. This directly affects which source files templates process — getting it wrong means wrong output.
- **Who is more correct and why**: **Codex identified a real gap that Claude missed entirely.** This is a legitimate blind spot in my analysis.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 1/5 (not identified)

### Divergence #3
- **Topic**: Project mutation parity scope
- **My position (Claude)**: Mentioned in the parity matrix as ❌ not planned, but not raised as an open question.
- **Other agent's position (Codex)**: Q-0002 — Should v1 include project-file mutation (adding generated files to `.csproj`, writing `CustomToolNamespace`)? Explicitly frames it as a scope decision.
- **Technical evaluation**: Codex correctly identifies this as a decision that needs explicit stakeholder input, not just a blanket "not planned." Some teams genuinely depend on generated files appearing in their project files. Dismissing it without discussion is premature.
- **Who is more correct and why**: Codex is more thorough. Even if the answer is "no," the question deserves explicit documentation.
- **Pertinence score for Codex**: 4/5
- **Pertinence score for Claude**: 2/5

### Divergence #4
- **Topic**: Partial rendering requestRender callback
- **My position (Claude)**: Not identified as an open question.
- **Other agent's position (Codex)**: Q-0004 — How should CLI handle `requestRender` callback behavior for combined partial mode? Identifies that `RoslynFileMetadata.cs:64` invokes `_requestRender` to trigger rendering of another file, and this event-driven behavior must be mapped to deterministic CLI batching.
- **Technical evaluation**: This is a subtle but important correctness concern. In combined partial rendering mode, the upstream VSIX can trigger re-rendering of related files via a callback. A CLI batch mode needs a different mechanism. Missing this could cause incomplete output for partial type scenarios.
- **Who is more correct and why**: **Codex identified a real technical gap.** This is a genuine correctness risk for partial type rendering.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 1/5 (not identified)

### Divergence #5
- **Topic**: Source generator support
- **My position (Claude)**: Q-0001 — Does MSBuildWorkspace include source-generator-produced files? Important for templates referencing generated types.
- **Other agent's position (Codex)**: Not raised as a dedicated question.
- **Technical evaluation**: This is a legitimate technical concern. Source generators are increasingly common (e.g., `System.Text.Json`, EF Core compiled models). If MSBuildWorkspace/ProjectGraph doesn't include generator output, templates referencing those types will produce incomplete results.
- **Who is more correct and why**: Claude identified a real risk that Codex missed. However, the practical likelihood of this being an issue is uncertain — it needs empirical testing, which is what Claude recommends.
- **Pertinence score for Codex**: 2/5 (not identified)
- **Pertinence score for Claude**: 4/5

### Divergence #6
- **Topic**: API compatibility (Claude Q-0003)
- **My position (Claude)**: Q-0003 — Which net472 APIs are unavailable on .NET 10? Lists Assembly.LoadFrom, AppDomain, Path, File encoding.
- **Other agent's position (Codex)**: Not raised as a question. Implicitly handled by porting during implementation.
- **Technical evaluation**: Claude admits this is "low risk" and "compilation will reveal issues." This is not a meaningful open question — it's a standard porting task that the compiler will flag. Elevating it to a question wastes attention.
- **Who is more correct and why**: Codex is correct not to raise this. The compiler is the answer.
- **Pertinence score for Codex**: 4/5 (correct omission)
- **Pertinence score for Claude**: 2/5 (unnecessary question)

## 4. Blind Spot Detection

- **What Codex missed**:
  - Source generator support in MSBuildWorkspace/ProjectGraph. This is a real technical risk that deserves investigation. Severity: 3/5.

- **What Claude missed**:
  - IncludeProject resolution policy for duplicate project names. Severity: 4/5 (directly affects output correctness in monorepos).
  - Partial rendering requestRender callback mapping to CLI batch mode. Severity: 4/5 (correctness risk for partial types).
  - Project mutation as a scoped decision (not just "not planned"). Severity: 2/5 (stakeholder communication issue).

- **Neither addressed**:
  - How `Settings.IncludeCurrentProject()` determines the "current project" in a CLI context where there is no "current project" in the VS sense. The template file's directory must be mapped to a project, but what if the template is outside all project directories? Severity: 3/5.
  - Template `#reference` assembly resolution in cross-platform contexts — what probing paths are used? Severity: 2/5.

## 5. Overall File Verdict

- **Which analysis is stronger?**: **Codex — clearly.**
- **Why**: Codex's questions identify genuine design crossroads with parity implications (IncludeProject ambiguity, requestRender mapping, project mutation scope). Claude's questions mix one legitimate concern (source generators) with two low-value items (API compat, already-resolved template discovery). The quality gap in question selection reveals a deeper gap in analysis thoroughness.
- **Overall quality rating of Codex's work**: 5/5
- **Overall quality rating of Claude's work**: 2/5
- **Confidence level in my assessment**: High
