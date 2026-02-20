# D-0003: Project Loading Strategy — Comparison

## 1. Scope

- **Files compared**:
  - `.ai_CLAUDE/decisions/D-0003-project-loading-strategy.md`
  - `.ai_CODEX/decisions/D-0003-project-loading-strategy.md`
- **Claude's intent**: Decide on MSBuildWorkspace hybrid with custom `.slnx` parser. Focus on direct `Document` → `SemanticModel` path. Explicitly rejects Buildalyzer.
- **Codex's intent**: Decide on hybrid `ProjectGraph` + Roslyn semantic loading. Two-phase strategy: graph for traversal/ordering, Roslyn for symbol fidelity.

## 2. Alignment Analysis

### Agreement: Hybrid loading approach
- Both agents reject a single-technology solution and adopt a hybrid.
- Both recognize the need for both project graph traversal AND Roslyn semantic models.
- **Technical strength**: Strong. Neither MSBuildWorkspace alone nor ProjectGraph alone covers all requirements.
- **Pertinence score**: 5/5
- **Justification**: The problem genuinely requires two capabilities: deterministic project graph handling and high-fidelity symbol extraction.

### Agreement: Buildalyzer is not the primary path
- Both agents conclude Buildalyzer should not be the primary loader.
- Claude explicitly rejects it (lossy intermediate step). Codex relegates it to optional/investigational.
- **Technical strength**: Correct. Buildalyzer is not used in upstream runtime and adds unnecessary indirection.
- **Pertinence score**: 5/5
- **Justification**: Evidence-backed (both cite `rg` search confirming no runtime usage).

### Agreement: Restore detection is critical
- Both identify NuGet restore state as a hard prerequisite for loading.
- Both propose `obj/project.assets.json` checking and `--restore` flag.
- **Technical strength**: Correct. MSBuild project loading without restored packages produces compilation failures.
- **Pertinence score**: 5/5
- **Justification**: Confirmed by both agents' spike/evidence work.

## 3. Divergence Analysis

### Divergence #1 — THE CRITICAL DIVERGENCE
- **Topic**: MSBuildWorkspace-centric vs. ProjectGraph-centric
- **My position (Claude)**: Primary loader is `MSBuildWorkspace` (via `Microsoft.Build.Locator`). Custom `.slnx` parser supplements it. No explicit `ProjectGraph` usage.
- **Other agent's position (Codex)**: Primary traversal is `ProjectGraph` (from `Microsoft.Build.Graph`). Roslyn semantic loading is a separate second phase per project. `.slnx` loaded through ProjectGraph natively (confirmed by spike).
- **Technical evaluation**: This is the most significant divergence in the entire analysis.
  - **MSBuildWorkspace advantages**: Direct `Document` → `SemanticModel` pipeline. One API for loading and symbol extraction. Simpler code.
  - **ProjectGraph advantages**: Deterministic traversal order. Explicit multi-target handling (graph nodes per TFM). Better `.slnx` support (native, not custom parser). Separation of concerns (traversal vs. compilation). More control over global properties per project.
  - **Key technical fact**: Codex's spike *actually ran code* that confirmed `ProjectGraph` loads `.slnx` natively. Claude's spike is design-only and proposed a custom `.slnx` XML parser that may be unnecessary.
  - **MSBuildWorkspace weakness**: It does NOT natively support `.slnx` as of the tested SDK version. Claude acknowledges this and proposes a ~50 line custom parser. This is a fragile workaround.
  - **ProjectGraph weakness**: It provides evaluation results but not Roslyn `SemanticModel` directly. A second phase is needed (Codex acknowledges this).
- **Who is more correct and why**: **Codex is more correct.** The two-phase approach (ProjectGraph for traversal + Roslyn for semantics) is architecturally cleaner and avoids the `.slnx` custom parser hack. The spike evidence backs this up. MSBuildWorkspace's lack of `.slnx` support is a real gap that Claude's custom parser only partially addresses.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 3/5

### Divergence #2
- **Topic**: `.slnx` handling
- **My position (Claude)**: Custom XML parser (~50 lines) to extract `<Project Path="..."/>` elements, then feed to MSBuildWorkspace.
- **Other agent's position (Codex)**: ProjectGraph loads `.slnx` natively (confirmed by spike).
- **Technical evaluation**: If ProjectGraph handles `.slnx` natively, writing a custom parser is unnecessary complexity. Claude's approach works but is fragile (format changes could break it). Codex's approach leverages built-in SDK support.
- **Who is more correct and why**: Codex is clearly correct. Native `.slnx` support through ProjectGraph eliminates the need for custom parsing. Claude's R-0002 risk about `.slnx` format stability becomes less relevant if the SDK handles it natively.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 2/5

### Divergence #3
- **Topic**: Multi-targeting control
- **My position (Claude)**: Set `TargetFramework` as MSBuildWorkspace global property. Default to first TFM.
- **Other agent's position (Codex)**: Set `TargetFramework` as ProjectGraph global property. Graph nodes expand per TFM; constrain with `--framework`. Also supports `--runtime <RID>` and `--configuration`.
- **Technical evaluation**: Codex's approach is more complete. `--runtime` and explicit `--configuration` are valuable for CI determinism. Claude's approach is functional but narrower.
- **Who is more correct and why**: Codex is more thorough. The additional flags (`--runtime`, `--configuration`) address real CI scenarios.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 3/5

### Divergence #4
- **Topic**: Evidence quality (design-only vs. running code)
- **My position (Claude)**: Design-only spike — code snippets showing proposed approach, not executed.
- **Other agent's position (Codex)**: Actual running spike (`GraphProbe/Program.cs`) with build logs, real `.slnx` and `.sln` files, `global.json`, and multi-target projects.
- **Technical evaluation**: Running code is categorically stronger evidence than design documents. Codex's spike *proves* the approach works. Claude's spike *hypothesizes* it will work.
- **Who is more correct and why**: Codex's evidence is stronger by a wide margin. A running prototype eliminates speculation.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 2/5

## 4. Blind Spot Detection

- **What Codex missed**: Explicit naming of `MSBuildLocator.RegisterDefaults()` as the SDK resolution mechanism. Though Codex uses explicit MSBuild environment variables in the spike, the final architecture should use MSBuildLocator for cleaner SDK detection. Severity: 3/5 (the spike's manual environment setup may not translate well to production code).
- **What Claude missed**:
  - That ProjectGraph natively supports `.slnx`, making the custom parser unnecessary. Severity: 4/5 (significant — this is wasted work and fragile code).
  - `--runtime <RID>` flag for evaluation consistency. Severity: 2/5 (nice-to-have for CI).
  - That the spike should have been executed, not just designed. Severity: 3/5 (design-only spikes leave uncertainty).
- **Neither addressed**:
  - How to handle `Directory.Build.props` and `Directory.Build.targets` in the loading pipeline (both mentioned but neither detailed the interaction). Severity: 3/5 (can cause subtle evaluation differences).
  - How the two phases (graph + semantic) connect concretely — what data is passed from phase 1 to phase 2? Severity: 3/5 (architectural gap).

## 5. Overall File Verdict

- **Which analysis is stronger?**: **Codex — significantly.**
- **Why**: The ProjectGraph + Roslyn hybrid is architecturally superior to MSBuildWorkspace + custom `.slnx` parser. Codex's running spike provides concrete evidence. The two-phase separation of concerns is cleaner. Native `.slnx` support eliminates fragile custom parsing. The additional CLI flags (`--runtime`, `--configuration`) show deeper CI awareness.
- **Overall quality rating of Codex's work**: 5/5
- **Overall quality rating of Claude's work**: 3/5
- **Confidence level in my assessment**: High — the spike evidence is conclusive.
