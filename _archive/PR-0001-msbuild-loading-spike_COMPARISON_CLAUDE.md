# PR-0001: MSBuild Loading Spike — Comparison

## 1. Scope

- **Files compared**:
  - `.ai_CLAUDE/prototypes/PR-0001-msbuild-loading-spike.md` (design document only)
  - `.ai_CODEX/prototypes/PR-0001-msbuild-loading-spike.md` (design document + running code + test artifacts)
  - `.ai_CODEX/prototypes/spike-msbuild/` (actual spike implementation)
- **Claude's intent**: Document a proposed MSBuildWorkspace loading approach with code snippets for key scenarios (basic loading, multi-targeting, restore detection, `.slnx` parsing, cross-platform). Design-only, no executed code.
- **Codex's intent**: Build and execute a real spike (`GraphProbe`) that proves ProjectGraph loading of `.csproj`, `.sln`, `.slnx`, validates `global.json` behavior, tests multi-target expansion, and observes restore failure modes. Includes actual projects, solution files, build logs, and `global.json`.

## 2. Alignment Analysis

### Agreement: MSBuild-based loading is viable
- Both confirm that MSBuild APIs can load `.csproj`, `.sln`, and `.slnx` for the CLI's purposes.
- **Technical strength**: Correct and foundational.
- **Pertinence score**: 5/5
- **Justification**: This validates the entire project loading strategy.

### Agreement: global.json impacts SDK resolution
- Both identify that `global.json` SDK pinning directly affects loading success/failure.
- **Technical strength**: Correct. An invalid SDK pin causes hard failure.
- **Pertinence score**: 5/5
- **Justification**: Critical CI determinism concern.

### Agreement: Restore state is a prerequisite
- Both confirm that missing `obj/project.assets.json` (no restore) causes load/build failure.
- Both propose `--restore` flag and pre-check.
- **Technical strength**: Correct and actionable.
- **Pertinence score**: 5/5
- **Justification**: Essential for CI reliability.

### Agreement: Multi-targeting requires framework selection
- Both demonstrate that multi-target projects need explicit TFM selection.
- Claude: via MSBuildWorkspace global properties. Codex: via ProjectGraph global properties.
- **Technical strength**: Correct in both cases.
- **Pertinence score**: 5/5
- **Justification**: Multi-targeting is a common real-world scenario.

## 3. Divergence Analysis

### Divergence #1
- **Topic**: Design-only vs. executed spike
- **My position (Claude)**: Wrote code snippets illustrating the proposed approach. No actual project files, no execution, no build logs.
- **Other agent's position (Codex)**: Created actual projects (`SpikeLib`, `MultiLib`, `NoRestoreLib`), solution files (`.sln` and `.slnx`), `global.json`, a `GraphProbe` tool, and executed it. Build logs preserved as evidence.
- **Technical evaluation**: **This is a categorical difference.** Codex's spike is empirical evidence. Claude's spike is a thought experiment. In engineering, running code beats design documents.
  - Codex discovered real-world issues: MSB4276 workload resolver messages, specific `NETSDK1004` error text, `.slnx` default format in `dotnet new sln`.
  - Claude's design assumed behaviors that were never tested (e.g., custom `.slnx` parser viability).
- **Who is more correct and why**: Codex is significantly stronger. Empirical evidence eliminates guesswork.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 2/5

### Divergence #2
- **Topic**: Loading API choice (MSBuildWorkspace vs. ProjectGraph)
- **My position (Claude)**: Proposes `MSBuildWorkspace.Create()` → `OpenSolutionAsync()` as the primary API.
- **Other agent's position (Codex)**: Uses `ProjectGraph` as the primary traversal API, with explicit MSBuild environment configuration.
- **Technical evaluation**: Codex's choice of ProjectGraph for the spike was the correct one to validate. ProjectGraph provides deterministic traversal that MSBuildWorkspace's `OpenSolutionAsync` does not expose directly. The spike proved ProjectGraph works for all three input formats.
- **Who is more correct and why**: Codex chose the more informative API to spike. ProjectGraph validates both traversal AND input format support in one test.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 3/5

### Divergence #3
- **Topic**: `.slnx` handling
- **My position (Claude)**: Proposes custom XML parser for `.slnx` — `XDocument.Load()` → extract `<Project Path="..."/>`.
- **Other agent's position (Codex)**: Proves `.slnx` is natively loaded by `ProjectGraph` and `dotnet sln` commands. `.slnx` is now the *default* format for `dotnet new sln`.
- **Technical evaluation**: Codex's finding that `.slnx` is natively supported renders Claude's custom parser design unnecessary. This is a significant efficiency win — less code to write, less code to maintain, less fragility.
- **Who is more correct and why**: Codex is correct. The custom parser is unnecessary.
- **Pertinence score for Codex**: 5/5
- **Pertinence score for Claude**: 1/5

### Divergence #4
- **Topic**: Cross-platform considerations
- **My position (Claude)**: Discusses path separators, case sensitivity, `Path.Combine()`, temp directories.
- **Other agent's position (Codex)**: Focuses on MSBuild environment variables (`MSBuildSDKsPath`, `MSBuildExtensionsPath`, `MSBUILD_EXE_PATH`) and SDK resolver behavior.
- **Technical evaluation**: Both address different aspects of cross-platform concerns. Claude's path-level concerns are valid but lower-risk (.NET handles most of this). Codex's MSBuild environment concerns are more specific to the loading pipeline and higher-risk (incorrect MSBuild paths cause hard failures).
- **Who is more correct and why**: Codex addresses the higher-impact concern. MSBuild environment configuration is the real cross-platform challenge, not path separators.
- **Pertinence score for Codex**: 4/5
- **Pertinence score for Claude**: 3/5

### Divergence #5
- **Topic**: Known limitations documentation
- **My position (Claude)**: Lists analyzers, source generators, conditional compilation, design-time build as known limitations.
- **Other agent's position (Codex)**: Notes MSB4276 workload resolver issue observed in real execution. Notes `NETSDK1004` exact error message.
- **Technical evaluation**: Claude's limitations are speculative (reasonable but untested). Codex's are observed. Both are useful but in different ways.
- **Who is more correct and why**: Codex's observations are empirical. Claude's are theoretical. Both have value.
- **Pertinence score for Codex**: 4/5
- **Pertinence score for Claude**: 3/5

## 4. Blind Spot Detection

- **What Codex missed**:
  - No discussion of source generator support in the loading pipeline. Claude raises this in Q-0001. Severity: 3/5 (source generators are increasingly common and could affect template output).
  - The spike's manual MSBuild environment setup (explicit `MSBuildSDKsPath` etc.) may not generalize well to production code. The spike doesn't use `MSBuildLocator`, which is the standard mechanism. Severity: 3/5 (the spike proves the concept but the production implementation needs a different approach).

- **What Claude missed**:
  - That `.slnx` is natively supported and the custom parser is unnecessary. Severity: 4/5 (significant wasted design effort).
  - Real-world MSBuild environment issues (MSB4276). Severity: 2/5 (nice-to-know).
  - That the spike should have been executed. Severity: 4/5 (design-only spikes are insufficient for loading strategy validation).

- **Neither addressed**:
  - How `MSBuildLocator.RegisterDefaults()` interacts with the ProjectGraph API. ProjectGraph may not need MSBuildLocator if environment variables are set correctly, but the interaction needs clarification. Severity: 3/5.
  - Performance characteristics of loading large solutions. Neither spike tests at scale. Severity: 2/5.

## 5. Overall File Verdict

- **Which analysis is stronger?**: **Codex — overwhelmingly.**
- **Why**: Running code beats design documents. Codex proved the approach works empirically, discovered real issues, and demonstrated native `.slnx` support. Claude's design-only spike, while well-structured, contains an incorrect assumption (need for custom `.slnx` parser) that would have been caught by execution.
- **Overall quality rating of Codex's work**: 5/5
- **Overall quality rating of Claude's work**: 2/5
- **Confidence level in my assessment**: High — the evidence gap is conclusive.
