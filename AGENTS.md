# Coding Agent Guide (Typewriter CLI Spin-off — .NET Core)

## Goal
Create a **spin-off** of `AdaskoTheBeAsT/Typewriter` that:
- **Does not depend on Visual Studio** (no VSIX runtime requirement).
- Runs as a **cross-platform CLI** suitable for **CI pipelines**.
- Is a **modern .NET (Core) project** (cross-platform; Windows-only behavior must be explicitly justified).
- Uses **MSBuild integration** to support:
  - **.sln**, **.slnx**, and **.csproj** inputs.
  - Multi-targeting, SDK-style projects, Directory.Build.props/targets, global.json, NuGet restore scenarios.
- Achieves **feature parity** with the original extension (unless explicitly documented as “parity gap” with rationale + mitigation).

## Hard Constraints (Non-negotiables)
- The upstream repo clone (“origin”) **must remain untouched**:
  - No commits, no file edits, no formatting changes.
  - All work occurs in the spin-off repo/folder.
- Your analysis must be **evidence-based** (paths + symbols) and captured in `.ai/`.
- Do **analysis + planning only**: do not implement the CLI yet.

---

## Deliverables
1. `.ai/` structured notes capturing:
   - Findings, decisions, open questions, prototypes/spikes, risks, parity matrix
2. `IMPLEMENTATION_PLAN.md` containing:
   - Visual Studio dependency map + replacement strategy
   - MSBuild loading design for sln/slnx/csproj
   - Feature parity matrix
   - Concrete architecture + phased implementation plan (milestones + tests)
   - CI strategy, risks, and open questions

---

## .NET Core / Modern .NET Requirements
The spin-off MUST be implemented as a modern .NET solution:
- Prefer **.NET 10** unless a lower LTS target is required (document the choice in decisions).
- Cross-platform first:
  - Works on Linux/macOS/Windows for CI usage.
  - If any feature is Windows-only due to upstream constraints, you MUST:
    - Mark it explicitly as a parity gap or conditional feature
    - Provide a mitigation strategy
- Packaging:
  - Primary target: `dotnet tool` installable CLI (`dotnet tool install -g ...`)
  - Consider secondary: self-contained binaries (optional, plan-dependent)

Document these in:
- `.ai/decisions/D-000X-target-framework.md`
- `.ai/decisions/D-000Y-packaging.md`

---

## Working Style
- Prefer **precise evidence** over interpretation:
  - Always record file paths + class/method identifiers.
- Keep `.ai/` notes **atomic** (one topic per file).
- When you make assumptions, record them and create a matching open question.

---

## Upstream Exploration Checklist (Required)
Perform a deep analysis of upstream focusing on:

1. **Entry points / orchestration**
   - VS extension init, commands, menus, events
2. **Generation pipeline**
   - Template discovery, evaluation, file writing
   - Incremental triggers (save/build/solution load)
3. **C# model extraction**
   - How types/symbols are obtained (Roslyn? VS CodeModel? MSBuild?)
4. **Configuration**
   - Settings sources and precedence rules
5. **I/O & output**
   - Output directory, overwrite rules, formatting
6. **Diagnostics**
   - Logging, warnings, error surface, exit behavior
7. **External dependencies**
   - NuGet packages, VS SDK assemblies, MSBuild APIs, Roslyn APIs

---

## Visual Studio Dependency Analysis (Required)
Build a concrete map of all dependencies on Visual Studio / Windows-only surfaces:

Examples:
- `Microsoft.VisualStudio.*` SDK packages
- `EnvDTE` / `DTE2`
- VS services (solution service, threading/JTF)
- VS build events / solution load events
- MEF composition in VS host
- Any COM usage or registry probing
- Any reliance on devenv-specific assumptions

For each dependency, classify:
- **Hard**: must be replaced for CLI
- **Soft**: optional UI enhancement (likely removed)
- **Accidental**: could be removed/refactored

For each, propose replacement:
- CLI arguments + config files
- `Microsoft.Build.*` + `Microsoft.Build.Locator` (MSBuildLocator)
- Roslyn `MSBuildWorkspace` or `Microsoft.Build.Graph.ProjectGraph` (or hybrid)
- Standard logging (console + structured verbosity)

Capture findings in `.ai/findings/F-*.md` and summarize replacements in the plan.

---

## MSBuild + Solution/Project Loading Requirements
The CLI must load inputs robustly:

### Inputs
- `path/to/solution.sln`
- `path/to/solution.slnx`
- `path/to/project.csproj`

### Must handle
- SDK selection via `global.json`
- `Directory.Build.props/targets`
- multi-targeting (`TargetFrameworks`)
- project references + project graph traversal
- restore scenarios (document behavior when assets are missing)
- CI-friendly determinism

### Strategy decision (mandatory)
You must decide and justify:
- Roslyn `MSBuildWorkspace`
- `Microsoft.Build.Graph.ProjectGraph`
- Hybrid (graph for traversal, workspace for semantic model)

You MUST write:
- A prototype note in `.ai/prototypes/PR-0001-msbuild-load-spike.md`
- A decision note in `.ai/decisions/D-0001-project-loading-strategy.md`

Include tradeoffs: symbol fidelity, performance, restore requirements, slnx support, cross-platform behavior.

---

## Feature Parity (Required)
Create and maintain a matrix:
- Upstream feature
- Where implemented upstream (file/symbol)
- CLI parity status:
  - ✅ identical
  - 🟨 partial (define gaps)
  - ❌ not planned (must justify)
- Implementation notes and tests per feature

Store it in:
- `.ai/parity/P-0001-feature-matrix.md`

Common parity targets (examples):
- Template discovery & execution
- Output path rules
- Configuration variables/macros
- Type filtering/selection rules
- Generics/attributes/nested/partial types handling
- Diagnostics quality
- Deterministic generation and caching (CLI equivalent)
- Performance considerations for large solutions

---

## CLI Product Requirements (Minimum)
Command:
- `typewriter-cli generate [options] <input>`

Options:
- `--solution <pathToSlnOrSlnx>` (optional)
- `--project <pathToCsproj>` (optional)
- `--output <dir>` (optional override)
- `--verbosity quiet|normal|detailed`
- `--fail-on-warnings` (optional)

Input:
- Glob e.g.: "**/*.tst"

Exit codes:
- `0` success
- `1` generation errors
- `2` input/args errors
- `3` build/restore/load errors

---

## `.ai/` Workspace Rules
You will maintain a structured `.ai/` directory at the spin-off repo root.

### Directory Structure
.ai/
  00_INDEX.md
  findings/
    F-0001-<short-title>.md
  decisions/
    D-0001-<short-title>.md
  questions/
    Q-0001-<short-title>.md
  parity/
    P-0001-feature-matrix.md
  prototypes/
    PR-0001-msbuild-load-spike.md
  risks/
    R-0001-<short-title>.md

### Atomic Note Template (mandatory)
Each note MUST include:
- ID, Title, Date (YYYY-MM-DD)
- Context
- Evidence (file paths, symbols, minimal excerpts)
- Conclusion
- Impact
- Next steps

### Index
`.ai/00_INDEX.md` must list:
- All findings/decisions/questions by ID with one-line summaries
- Current status (“inventory”, “dependency map”, “parity matrix”, “ready for plan”)

---

## Analysis Workflow (Step-by-step)
1. **Inventory upstream**
   - Identify projects, entry points, generation flow
2. **Map Visual Studio coupling**
   - Findings per dependency cluster, with replacements
3. **Understand model extraction**
   - How semantic types are obtained
4. **Map configuration + templates**
   - Settings, template discovery, output rules
5. **Build parity matrix**
   - Populate `.ai/parity/P-0001-feature-matrix.md` continuously
6. **Prototype loading strategies**
   - PR note + decision note (Workspace vs Graph vs hybrid)
7. **Define target CLI architecture**
   - Modules, boundaries, interfaces
8. **Write `IMPLEMENTATION_PLAN.md`**
   - Using required headings below
9. **Review for completeness**
   - Evidence coverage, risks, open questions linked

---

## Output: `IMPLEMENTATION_PLAN.md` Required Headings
You MUST produce `IMPLEMENTATION_PLAN.md` with the exact headings:

1. Overview
2. Goals and Non-goals
3. Upstream Architecture Summary
4. Visual Studio Dependency Map (with replacement plan)
5. MSBuild & Project Loading Design (sln, slnx, csproj)
6. CLI UX Spec (commands, flags, exit codes)
7. Feature Parity Matrix (link to `.ai/parity/`)
8. Target Architecture (modules, APIs, boundaries)
9. Implementation Phases (milestones + acceptance criteria)
10. Testing Strategy (unit/integration/golden tests)
11. CI/CD Plan (restore/build/generate verification)
12. Risk Register (top risks + mitigations)
13. Open Questions (must link to `.ai/questions/`)
14. Appendix (key references to upstream files/symbols)

---

## Quality Bar
A plan is acceptable only if:
- It is actionable without additional discovery
- It proposes concrete modules/classes/interfaces to create
- Each milestone has acceptance tests
- Parity gaps are explicit and justified
- Cross-platform behavior is addressed explicitly (since this is .NET Core / modern .NET)
