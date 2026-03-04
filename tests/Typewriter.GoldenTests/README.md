# Golden Tests

Golden tests verify that the Typewriter generation pipeline produces stable, expected output. Each test runs the full pipeline against a fixture project and compares the generated `.ts` files against committed baselines.

## Directory structure

```
tests/
├── baselines/           # Committed baseline .ts files (expected output)
│   ├── simple/
│   ├── multi-project/
│   ├── multi-target/
│   ├── source-generators/
│   └── complex-types/
├── fixtures/            # Input projects and .tst templates
│   ├── simple/
│   ├── multi-project/
│   ├── multi-target/
│   ├── source-generators/
│   └── complex-types/
└── Typewriter.GoldenTests/
    ├── Infrastructure/  # GoldenTestBase, CapturingOutputWriter, TestDiagnosticReporter
    ├── GoldenTest_*.cs  # Per-fixture test classes
    └── ParityMatrix.md  # Parity tracking for upstream feature coverage
```

## Running golden tests

Run all golden tests:

```bash
dotnet test tests/Typewriter.GoldenTests/ -c Release
```

Run a specific fixture test:

```bash
dotnet test tests/Typewriter.GoldenTests/ -c Release --filter "FullyQualifiedName~GoldenTest_Simple"
```

## Updating baselines

Update baselines **only** after an intentional change to generation behavior. Do not update baselines to make a failing test pass without understanding why the output changed.

### When to update

- You changed template rendering logic in `Typewriter.Generation`.
- You changed metadata extraction in `Typewriter.Metadata.Roslyn`.
- You changed output path policy or collision naming in `Typewriter.Generation.Output`.
- You added a new fixture or modified an existing fixture's `.tst` templates or source files.

### How to update

Use the `Verify.UpdateSnapshots` flag to accept new output as the baseline:

```bash
dotnet test tests/Typewriter.GoldenTests/ -c Release -- Verify.UpdateSnapshots=1
```

Alternatively, you can update baselines manually:

1. Build the solution: `dotnet build -c Release`
2. Run the CLI against each fixture to regenerate output:
   ```bash
   dotnet run --project src/Typewriter.Cli/ -c Release -- generate \
     "tests/fixtures/simple/SimpleProject/*.tst" \
     --project tests/fixtures/simple/SimpleProject/SimpleProject.csproj \
     --output tests/baselines/simple/ --restore
   ```
3. Copy the generated `.ts` files into the corresponding `tests/baselines/<fixture>/` directory.
4. Verify the updated baselines pass: `dotnet test tests/Typewriter.GoldenTests/ -c Release`
5. Commit the updated baselines in the same PR as the behavior change.

### Review checklist

Before committing updated baselines:

- [ ] Every diff is explained by your behavior change
- [ ] No unrelated files changed
- [ ] All golden tests pass with the new baselines
- [ ] The PR description explains why baselines changed

## Reviewing baseline diffs in CI

The CI workflow (`.github/workflows/ci.yml`) runs golden tests as part of the `Test` step on all three platform targets (Windows, Ubuntu, macOS). When a golden test fails:

1. Open the failing CI run from the PR checks.
2. Expand the **Test** step in the build log.
3. The assertion message shows a content diff between the expected baseline and actual generated output, including both `--- Expected (baseline) ---` and `--- Actual (generated) ---` sections.
4. Use the diff to determine whether the change is intentional or a regression.

## How golden tests work

Each `GoldenTest_*.cs` class extends `GoldenTestBase`, which provides:

- **`RunGenerationAsync`** — runs the full `ApplicationRunner` pipeline (resolve → restore → graph → workspace → render) with a `CapturingOutputWriter` that stores generated output in memory instead of writing to disk.
- **`AssertMatchesBaselines`** — compares every captured output file against the corresponding file in `tests/baselines/<fixture>/`. Fails on content mismatch, missing baselines, or unexpected extra files. Line endings are normalized to LF for cross-platform consistency.

See `Infrastructure/GoldenTestBase.cs` for the full implementation.
