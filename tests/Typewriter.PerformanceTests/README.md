# Performance Tests

Performance acceptance tests for the typewriter-cli pipeline. These tests run the
full end-to-end pipeline against the large-solution fixture (25 projects, 5 templates)
and assert time and memory budgets defined in AGENTS.md section 11.

## Why are they excluded from CI?

All tests in this project are tagged with `[Trait("Category", "Performance")]`.
The standard CI matrix (`ci.yml`) filters them out with `--filter "Category!=Performance"`
to keep PR feedback fast and deterministic across all three OS runners.

A dedicated `performance` job in `ci.yml` runs them on `ubuntu-latest` on pushes to
`main` and on manual `workflow_dispatch` triggers.

## Running performance tests locally

```bash
# Run only performance tests
dotnet test tests/Typewriter.PerformanceTests/ -c Release --filter "Category=Performance"

# Run all tests including performance
dotnet test Typewriter.Cli.slnx -c Release
```

## Running performance tests in CI

Trigger the workflow manually via the GitHub Actions UI or the CLI:

```bash
gh workflow run ci.yml
```

The `performance` job will execute on `ubuntu-latest`.

## Test inventory

| Test | Budget | What it measures |
|------|--------|------------------|
| `LargeSolution_CompletesUnderThreshold` | 60 s | End-to-end pipeline wall-clock time |
| `LargeSolution_PeakWorkingSet_UnderBudget` | 2 GB | Peak process working set after pipeline run |
