# Risks Folder — Comparison

## 1. Scope

- **Folders compared**:
  - `.ai_CLAUDE/risks/` (4 files: R-0001 through R-0004)
  - `.ai_CODEX/risks/` (4 files: R-0001 through R-0004)
- **Claude's risks**:
  - R-0001: MSBuildWorkspace semantic model fidelity (High impact, Low likelihood)
  - R-0002: .slnx format stability (Medium impact, Medium likelihood)
  - R-0003: Template assembly loading cross-platform (High impact, Medium likelihood)
  - R-0004: NuGet restore state in CI (High impact, Medium likelihood)
- **Codex's risks**:
  - R-0001: Semantic fidelity regression (High impact, Medium likelihood)
  - R-0002: MSBuild load/restore determinism (High impact, Medium likelihood)
  - R-0003: Project mutation parity gap (Medium impact, Medium likelihood)
  - R-0004: Large solution performance (Medium impact, Medium likelihood)

## 2. Alignment Analysis

### Agreement: Semantic fidelity is a top risk
- Both agents identify Roslyn metadata fidelity as the highest-impact risk.
- Both propose golden file tests and regression suites as primary mitigation.
- **Technical strength**: Correct. Metadata correctness is the foundation of the tool's value.
- **Pertinence score**: 5/5
- **Justification**: If the semantic model is wrong, the output is wrong.

### Agreement: Restore/load determinism is critical
- Claude R-0004 and Codex R-0002 both address NuGet restore state and MSBuild loading reliability.
- Both propose pre-checks, clear error messages, `--restore` flag, and exit code mapping.
- **Technical strength**: Correct. CI environments have varying restore states.
- **Pertinence score**: 5/5
- **Justification**: CI reliability is a core project goal.

## 3. Divergence Analysis

### Divergence #1
- **Topic**: Risk assessment calibration (Semantic fidelity likelihood)
- **My position (Claude)**: Likelihood = Low. Rationale: MSBuildWorkspace uses the same Roslyn compiler pipeline.
- **Other agent's position (Codex)**: Likelihood = Medium. No explicit rationale for higher rating, but implies the workspace change introduces risk.
- **Technical evaluation**: Claude's "Low" likelihood is probably correct but overconfident. While the Roslyn compiler is the same, the workspace initialization path differs (MSBuildWorkspace vs. VisualStudioWorkspace), which could cause subtle differences in compilation options, preprocessor defines, or assembly resolution. Codex's "Medium" is more cautious and defensible.
- **Who is more correct and why**: Codex's more conservative rating is safer. In risk management, underestimating likelihood is worse than overestimating it.
- **Pertinence score for Codex**: 4/5
- **Pertinence score for Claude**: 3/5

### Divergence #2
- **Topic**: `.slnx` format stability as a dedicated risk
- **My position (Claude)**: R-0002 — dedicated risk for `.slnx` instability. Custom parser may break if format changes.
- **Other agent's position (Codex)**: Not raised as a separate risk. `.slnx` is treated as a first-class format supported by the SDK.
- **Technical evaluation**: Given Codex's spike evidence that `.slnx` is natively supported by ProjectGraph and is the *default* format for `dotnet new sln`, Claude's risk is overblown. The format is already stable enough for the SDK to default to it. Claude's risk was driven by the assumption that a custom parser was needed — but if native support exists, the risk disappears.
- **Who is more correct and why**: Codex is correct. The risk is moot when using native SDK support.
- **Pertinence score for Codex**: 5/5 (correct omission)
- **Pertinence score for Claude**: 2/5 (risk based on incorrect premise)

### Divergence #3
- **Topic**: Template assembly loading cross-platform
- **My position (Claude)**: R-0003 — dedicated risk for `Assembly.LoadFrom()` behavior changes in .NET 10. Proposes `AssemblyLoadContext` with isolated context and resolving handler.
- **Other agent's position (Codex)**: Not raised as a separate risk.
- **Technical evaluation**: This is a legitimate risk that Codex missed. Template `${ }` code blocks compile to assemblies that reference `Typewriter.CodeModel`. Loading these compiled assemblies cross-platform requires `AssemblyLoadContext` and proper probing paths. `Assembly.LoadFrom()` has different behavior in .NET Core/5+. This is a real implementation challenge.
- **Who is more correct and why**: Claude identified a genuine risk. Template assembly loading is non-trivial cross-platform.
- **Pertinence score for Codex**: 2/5 (missed a real risk)
- **Pertinence score for Claude**: 4/5

### Divergence #4
- **Topic**: Project mutation as a risk
- **My position (Claude)**: Not raised as a risk. Covered in parity matrix as ❌ not planned.
- **Other agent's position (Codex)**: R-0003 — dedicated risk for project mutation parity gap. Teams may rely on automatic project-file updates. Cross-referenced to Q-0002.
- **Technical evaluation**: Codex correctly identifies this as a risk, not just a scoping decision. Even if v1 doesn't implement project mutation, teams upgrading from the VSIX may expect it. The risk is user-facing friction, not technical failure.
- **Who is more correct and why**: Codex is more thoughtful about user impact. Claude's dismissal as "not planned" ignores migration friction.
- **Pertinence score for Codex**: 4/5
- **Pertinence score for Claude**: 2/5

### Divergence #5
- **Topic**: Large solution performance as a risk
- **My position (Claude)**: Not raised as a dedicated risk.
- **Other agent's position (Codex)**: R-0004 — dedicated risk for graph + semantic loading performance on large/multi-target solutions. Proposes caching, filters, profiling fixtures, CI thresholds.
- **Technical evaluation**: This is a valid risk. Loading and compiling large solutions (50+ projects) with full semantic models can be slow and memory-intensive. Codex cites upstream's own warning in `Settings.cs:117` about `IncludeAllProjects()` performance impact. Claude mentions performance in passing (Phase 7, 60-second threshold) but doesn't classify it as a risk.
- **Who is more correct and why**: Codex is more explicit. Performance should be a tracked risk with mitigations, not an afterthought in the implementation plan.
- **Pertinence score for Codex**: 4/5
- **Pertinence score for Claude**: 2/5

## 4. Blind Spot Detection

- **What Codex missed**:
  - Template assembly loading cross-platform risk. `Assembly.LoadFrom()` → `AssemblyLoadContext` migration is non-trivial and platform-dependent. Severity: 4/5 (high-impact if templates fail to load on Linux/macOS).

- **What Claude missed**:
  - `.slnx` risk is based on incorrect premise (need for custom parser). Severity: 2/5 (waste of analysis effort but not harmful).
  - Project mutation parity gap as a user-facing risk. Severity: 3/5.
  - Large solution performance as a tracked risk. Severity: 3/5.
  - Codex's more conservative likelihood ratings are generally sounder. Severity: 2/5.

- **Neither addressed**:
  - Concurrent MSBuild evaluation safety. If the CLI ever parallelizes project loading, MSBuild's static state can cause race conditions. Severity: 2/5 (unlikely in v1 but worth noting).
  - License/IP risk of porting upstream code. Neither agent assesses whether the upstream project's license permits the CLI fork. Severity: 1/5 (out of scope for technical analysis, but worth noting).

## 5. Overall File Verdict

- **Which analysis is stronger?**: **Mixed — each identified risks the other missed.**
- **Why**: Codex's risks are more user-facing and operationally relevant (performance, project mutation). Claude's template assembly loading risk is genuinely important and Codex missed it. However, Claude's `.slnx` risk is based on an incorrect premise, which weakens the overall set. Net assessment: Codex's risk selection is slightly better-calibrated.
- **Overall quality rating of Codex's work**: 4/5
- **Overall quality rating of Claude's work**: 3/5
- **Confidence level in my assessment**: Medium-High
