# Implementation Plan: Typewriter CLI Spin-off

## 1. Overview
- Problem statement:
- Summary of approach:
- Expected output artifacts (binary, nuget tool, docker image, etc.):

## 2. Goals and Non-goals
### Goals
- (bullet list)

### Non-goals
- (bullet list)

## 3. Upstream Architecture Summary
- Repository layout:
- Key projects/modules:
- Generation pipeline:
- Configuration model:
- How upstream loads solution/projects:
- Notes and references to `.ai/findings/*`:

## 4. Visual Studio Dependency Map (with replacement plan)
### 4.1 Dependency Inventory
| Dependency Surface | Upstream location (file/symbol) | Why used | Classification (Hard/Soft/Accidental) |
|---|---|---|---|

### 4.2 Replacement Strategy
- VS command/UI → CLI commands/options
- VS services/threading → plain .NET hosting + async rules
- VS solution/project APIs → MSBuild loading approach
- VS logging panes → structured console logging

### 4.3 Removed / Optional Behaviors
- (anything purely UI-driven, with CLI equivalents if needed)

## 5. MSBuild & Project Loading Design (sln, slnx, csproj)
### 5.1 Inputs
- Supported inputs:
- Discovery rules:
- Multiple solutions / repo root behavior:

### 5.2 Loading Strategy Decision
- Chosen approach:
- Rationale:
- Tradeoffs:
- Evidence links to `.ai/decisions/*` and `.ai/prototypes/*`:

### 5.3 Restore & SDK Resolution
- global.json handling:
- MSBuildLocator configuration:
- Restore strategy (auto, optional `--restore`, error messaging):
- Handling offline/CI environments:

### 5.4 Multi-targeting & Configurations
- How TargetFramework(s) and Configuration are selected:
- How the tool iterates frameworks (if applicable):
- Project graph traversal rules:

## 6. CLI UX Spec (commands, flags, exit codes)
### 6.1 Commands
- `typewriter-cli generate ...`
- (any others like `list`, `validate`, `watch` if included)

### 6.2 Flags
- Full flag list with defaults

### 6.3 Exit Codes & Error Contract
- Deterministic mapping of error types to exit codes
- Example outputs

## 7. Feature Parity Matrix
- Link to `.ai/parity/P-0001-feature-matrix.md`
- Summary of any intentional gaps + mitigations

## 8. Target Architecture (modules, APIs, boundaries)
### 8.1 High-level module diagram (text)
- `Typewriter.Cli`
- `Typewriter.Core`
- `Typewriter.MSBuild`
- `Typewriter.Generation`
- `Typewriter.Configuration`
- (etc.)

### 8.2 Key Interfaces
- IProjectLoader
- IModelExtractor
- ITemplateEngine
- IOutputWriter
- ILogger abstraction

### 8.3 Data Flow
- Input → load → compile/model → template eval → write outputs

## 9. Implementation Phases (milestones + acceptance criteria)
> Each phase must include: scope, tasks, acceptance tests, and “done when” criteria.

### Phase 0 — Repo bootstrap
- Tasks:
- Acceptance:

### Phase 1 — CLI skeleton + logging + config parsing
- Tasks:
- Acceptance:

### Phase 2 — MSBuild loading (csproj)
- Tasks:
- Acceptance:

### Phase 3 — Solution loading (.sln)
- Tasks:
- Acceptance:

### Phase 4 — Solution loading (.slnx)
- Tasks:
- Acceptance:

### Phase 5 — Semantic model extraction parity
- Tasks:
- Acceptance:

### Phase 6 — Template execution parity
- Tasks:
- Acceptance:

### Phase 7 — Golden tests + sample repos + perf
- Tasks:
- Acceptance:

### Phase 8 — Packaging (dotnet tool) + CI pipeline integration
- Tasks:
- Acceptance:

## 10. Testing Strategy (unit/integration/golden tests)
- Unit tests:
- Integration tests (real project fixtures):
- Golden file tests (expected TS outputs):
- Cross-platform matrix:

## 11. CI/CD Plan (restore/build/generate verification)
- GitHub Actions (or equivalent) workflow outline:
- Caching strategy:
- Artifacts/publishing:

## 12. Risk Register (top risks + mitigations)
| Risk | Impact | Likelihood | Mitigation | Owner |
|---|---:|---:|---|---|

## 13. Open Questions
- Link each item to `.ai/questions/Q-*.md`

## 14. Appendix
- Key upstream references (file paths, symbols)
- Any important prototypes/spikes and conclusions
