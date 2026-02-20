# F-0006: Test Infrastructure Analysis

**Date**: 2026-02-19
**Status**: Complete

## Context
Analysis of upstream test patterns for CLI test strategy design.

## Evidence

### Test Framework
- **File**: `origin/src/Tests/Typewriter.Tests.csproj`
- xUnit v2.9.3, `Should` v1.1.20, `AwesomeAssertions` v9.3.0, `NSubstitute` v5.3.0
- VS Test Framework: `Microsoft.VisualStudio.Sdk.TestFramework` v17.11.66 — heavily VS-coupled

### Test Base Class
- **File**: `origin/src/Tests/TestInfrastructure/TestBase.cs`
- `TestBase : IAsyncLifetime`
- `InitializeAsync()`: creates `ExportProvider` from `MefHostingFixture`, gets `DTE`, creates `RoslynMetadataProviderStub`
- Helper: `GetFile(path)` → `MetadataProvider.GetFile()` → `new FileImpl(metadata, settings)`

### Test Categories
| Category | Files | Pattern |
|----------|-------|---------|
| CodeModel | `ClassTests`, `PropertyTests`, `TypeTests`, etc. | Load C# support file → parse → assert properties |
| Extensions | `WebApiTests`, `WebApiRouteTests` | Load controller → test extension methods |
| Render | `RenderTests` | Load `.tstemplate` + `.cs` → render → diff `.result` |
| Helpers | `CamelCaseTests` | Unit test of utility functions |

### Golden File Tests
- Template tests use `.result` files as expected output
- Pattern: `template.Render(file, out success)` → `output.ShouldEqual(expectedContent)`
- Test data in `origin/src/Tests/Render/` and `origin/src/Tests/CodeModel/Support/`

### VS Dependencies in Tests
- `MefHostingFixture` — requires VS MEF hosting
- `DTE` — from VS service provider
- `RoslynMetadataProviderStub` — uses `VisualStudioWorkspace` from ComponentModel
- `MessageFilter` — COM interop handler
- **All must be replaced** for CLI tests

## Conclusion
Test infrastructure is deeply VS-coupled and cannot be reused. However, **test data files** (support C# classes, `.tstemplate` files, `.result` files) are directly reusable as golden test fixtures. The CLI needs a new `CliTestBase` that uses `MSBuildWorkspace`.

## Impact
- Copy test data files (support classes, templates, expected outputs)
- Build new test base using MSBuildWorkspace for project loading
- Golden file pattern is ideal for CLI: load → generate → diff
- Consider using `Verify` library for snapshot testing

## Next Steps
- Design CLI test infrastructure in implementation plan
