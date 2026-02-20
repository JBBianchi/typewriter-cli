# P-0001: Feature Parity Matrix — Comparison

## 1. Scope

- **Files compared**:
  - `.ai_CLAUDE/parity/P-0001-feature-matrix.md`
  - `.ai_CODEX/parity/P-0001-feature-matrix.md`
- **Claude's intent**: Comprehensive 77-feature parity matrix (58 ✅, 1 🟨, 8 ❌, 7 new). Organized by functional categories with upstream file references and CLI notes.
- **Codex's intent**: Feature parity matrix with ~28 rows covering major functional areas. Each row references specific upstream file:line. Uses ✅/🟨/❌ status with notes and cross-references to open questions.

## 2. Alignment Analysis

### Agreement: Core generation features have full parity
- Both agree that template parsing, code blocks, runtime expansion, filters, rendering, output writing, type mapping, and metadata extraction are ✅ identical parity.
- **Technical strength**: Correct. These are the VS-independent core that can be ported.
- **Pertinence score**: 5/5
- **Justification**: The generation engine is the product's core value. Full parity here is non-negotiable.

### Agreement: VS IDE features are out of scope
- Both classify IntelliSense, syntax highlighting, context menus, and template editor features as ❌ not planned.
- **Technical strength**: Correct. These have no CLI equivalent and no generation parity impact.
- **Pertinence score**: 5/5
- **Justification**: Appropriate scoping for a CLI tool.

### Agreement: Watch mode deferred
- Both classify render-on-save / file tracking as 🟨 partial, with watch mode deferred to post-v1.
- **Technical strength**: Correct scoping decision. Batch mode is simpler and CI-appropriate.
- **Pertinence score**: 4/5
- **Justification**: Sound prioritization.

## 3. Divergence Analysis

### Divergence #1
- **Topic**: Granularity and feature count
- **My position (Claude)**: 77 features enumerated individually. Granular enough to list individual type mappings, each Settings method, each filter type, each CodeModel property.
- **Other agent's position (Codex)**: ~28 rows covering major functional areas. Each row represents a feature group (e.g., "Class/interface/enum/delegate/event/field/property metadata" is one row).
- **Technical evaluation**: Claude's higher granularity provides more specific parity tracking but may create false precision — listing 77 "features" when many are sub-features of the same system. Codex's grouping is more realistic for tracking but risks hiding individual gaps within a group.
- **Who is more correct and why**: Different trade-offs. Claude's approach is better as a parity checklist for testing. Codex's is better as a planning document. Neither is wrong.
- **Pertinence score for Codex**: 4/5
- **Pertinence score for Claude**: 4/5

### Divergence #2
- **Topic**: File:line evidence
- **My position (Claude)**: Feature matrix includes upstream file references but not always with line numbers.
- **Other agent's position (Codex)**: Every row includes specific `file:line` reference (e.g., `origin/src/Typewriter/Generation/Template.cs:152`).
- **Technical evaluation**: Codex's approach is more traceable and verifiable. During implementation, developers can jump directly to the relevant upstream code.
- **Who is more correct and why**: Codex's evidence quality is superior.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 3/5

### Divergence #3
- **Topic**: Cross-referencing open questions
- **My position (Claude)**: Features are listed with parity status but don't systematically link to open questions or risks.
- **Other agent's position (Codex)**: Several 🟨 partial features explicitly cross-reference open questions (Q-0001 for IncludeProject ambiguity, Q-0002 for project mutation, Q-0003 for watch mode, Q-0004 for requestRender).
- **Technical evaluation**: Codex's cross-referencing is significantly better. It connects the parity gaps to the specific decision points that will resolve them. This is more actionable.
- **Who is more correct and why**: Codex is more thorough in connecting parity to open questions.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 3/5

### Divergence #4
- **Topic**: New CLI-only features
- **My position (Claude)**: Explicitly lists 7 new features (`.slnx` support, `--framework`, `--restore`, `--output`, exit codes, cross-platform, glob input).
- **Other agent's position (Codex)**: Does not enumerate new CLI features as separate parity rows. Focuses on upstream parity only.
- **Technical evaluation**: Claude's inclusion of new features gives a more complete picture of the CLI's value proposition. However, a parity matrix should primarily track upstream equivalence — new features belong in the CLI spec.
- **Who is more correct and why**: This is a matter of document purpose. For parity tracking, Codex's focus is correct. For stakeholder communication, Claude's inclusion is useful.
- **Pertinence score for Codex**: 4/5
- **Pertinence score for Claude**: 4/5

### Divergence #5
- **Topic**: Partial parity nuance
- **My position (Claude)**: Only 1 feature marked 🟨 partial (long paths). Most VS features are ❌ not planned.
- **Other agent's position (Codex)**: More features marked 🟨 partial: IncludeProject by name, diagnostics, context menu commands, render-on-save, project mutation, source mapping, long paths, output/status bar. This reflects genuine uncertainty where the CLI provides *different* functionality (not absent, but transformed).
- **Technical evaluation**: Codex's more nuanced use of 🟨 is more honest. Features like "VS output window → console diagnostics" are genuinely partial parity — the *capability* exists but the *mechanism* is different. Claude's classification lumps these into ❌ or ✅, which loses the nuance.
- **Who is more correct and why**: Codex's classification is more accurate. A 🟨 for "replaced by different mechanism" is more honest than ❌ for "not planned" when the underlying capability does exist.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 3/5

## 4. Blind Spot Detection

- **What Codex missed**:
  - No enumeration of new CLI-only features. Stakeholders benefit from seeing what the CLI adds beyond parity. Severity: 2/5.
  - No individual feature granularity for type mapping rules. Severity: 2/5.

- **What Claude missed**:
  - The `requestRender` callback for combined partial mode is not tracked in the parity matrix. Codex links this to Q-0004. Severity: 3/5 (this is a real parity gap that could cause incomplete generation).
  - IncludeProject name ambiguity is not flagged as a parity concern. Codex links this to Q-0001. Severity: 3/5.
  - Over-confident parity classification — marking 58 features as ✅ identical when some (like diagnostics) are transformed, not identical. Severity: 2/5.

- **Neither addressed**:
  - Parity testing strategy — how will each ✅ feature be verified? The matrix lists features but neither agent defines what "identical" means operationally (byte-for-byte output? Same types resolved? Same error messages?). Severity: 3/5.

## 5. Overall File Verdict

- **Which analysis is stronger?**: **Codex — moderately.**
- **Why**: Better evidence traceability (file:line), more honest 🟨 classification for transformed features, and superior cross-referencing to open questions. Claude's higher feature count provides more granular tracking but with less nuance.
- **Overall quality rating of Codex's work**: 4/5
- **Overall quality rating of Claude's work**: 3/5
- **Confidence level in my assessment**: Medium-High
