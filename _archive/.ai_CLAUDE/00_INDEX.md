# .ai/ Index — Typewriter CLI Spin-off Analysis

**Status**: Ready for Plan
**Date**: 2026-02-19

---

## Findings
| ID | Title | Status |
|----|-------|--------|
| F-0001 | Repository Structure Inventory | Complete |
| F-0002 | Visual Studio Dependency Map | Complete |
| F-0003 | Roslyn Metadata Provider Architecture | Complete |
| F-0004 | Generation Pipeline Architecture | Complete |
| F-0005 | Buildalyzer Integration and Project Loading | Complete |
| F-0006 | Test Infrastructure Analysis | Complete |
| F-0007 | Configuration Model and Settings | Complete |
| F-0008 | Type Mapping, Extensions, and Helpers | Complete |

## Decisions
| ID | Title | Status |
|----|-------|--------|
| D-0001 | Target Framework Selection (.NET 10) | Decided |
| D-0002 | Packaging Strategy (dotnet tool) | Decided |
| D-0003 | Project Loading Strategy (MSBuildWorkspace hybrid) | Decided |

## Questions
| ID | Title | Status |
|----|-------|--------|
| Q-0001 | Source Generator Support in MSBuildWorkspace | Open |
| Q-0002 | Template Discovery Strategy for CLI | Resolved |
| Q-0003 | .NET Framework 4.7.2 API Compatibility During Port | Open |
| Q-0004 | Watch Mode for CLI | Deferred |

## Parity
| ID | Title | Status |
|----|-------|--------|
| P-0001 | Feature Parity Matrix (58 ✅ / 1 🟨 / 8 ❌ / 7 new) | Complete |

## Prototypes
| ID | Title | Status |
|----|-------|--------|
| PR-0001 | MSBuild Loading Spike (design-only) | Complete |

## Risks
| ID | Title | Status |
|----|-------|--------|
| R-0001 | MSBuildWorkspace Semantic Model Fidelity | Active |
| R-0002 | .slnx Format Stability | Active |
| R-0003 | Template Assembly Loading on Cross-platform | Active |
| R-0004 | NuGet Restore State in CI | Active |
