using Typewriter.Application;
using Typewriter.Application.Diagnostics;
using Typewriter.Generation.Output;
using Typewriter.Generation.Performance;
using Typewriter.Loading.MSBuild;
using Xunit;

namespace Typewriter.IntegrationTests;

/// <summary>
/// Integration tests that exercise the full <c>--dry-run</c> pipeline against a fixture project.
/// Verifies that the generate pipeline runs end-to-end without writing output files.
/// </summary>
public class DryRunIntegrationTests
{
    private sealed class CapturingReporter : IDiagnosticReporter
    {
        private readonly List<DiagnosticMessage> _messages = [];
        private int _warningCount;
        private int _errorCount;

        public void Report(DiagnosticMessage message)
        {
            _messages.Add(message);
            if (message.Severity == DiagnosticSeverity.Warning) _warningCount++;
            else if (message.Severity == DiagnosticSeverity.Error) _errorCount++;
        }

        public IReadOnlyList<DiagnosticMessage> Messages => _messages;
        public int WarningCount => _warningCount;
        public int ErrorCount => _errorCount;
    }

    private static string FixturePath(string relativePath) =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "tests", "fixtures",
            relativePath));

    /// <summary>
    /// Runs the full generate pipeline with <c>--dry-run</c> against the SimpleProject fixture.
    /// Asserts that: exit code is 0, no output files are written to disk, and dry-run
    /// diagnostics (<c>TW5001</c> per file, <c>TW5002</c> summary) are emitted.
    /// </summary>
    [Fact]
    public async Task DryRun_SimpleProject_FullPipeline_NoFilesWritten()
    {
        // Arrange
        var projectPath = FixturePath("simple/SimpleProject/SimpleProject.csproj");
        var templatePath = FixturePath("simple/SimpleProject/Interfaces.tst");

        var reporter = new CapturingReporter();

        // Wire real services (no mocks) — same pattern as CsprojIntegrationTests.
        var cache = new InvocationCache();
        var locator = new MsBuildLocatorService();
        var inputResolver = new InputResolver();
        var restoreService = new RestoreService();
        var solutionFallbackService = new SolutionFallbackService();
        var projectGraphService = new ProjectGraphService(locator, solutionFallbackService);
        var roslynWorkspaceService = new RoslynWorkspaceService(cache);
        var outputWriter = new OutputWriter();
        var outputPathPolicy = new OutputPathPolicy();

        locator.EnsureRegistered(reporter);

        // Ensure the fixture project is restored.
        if (!await restoreService.CheckAssetsAsync(projectPath))
        {
            var restored = await restoreService.RestoreAsync(projectPath, reporter);
            Assert.True(restored, "dotnet restore failed for SimpleProject fixture");
        }

        // Record any .ts files already present so we can detect new ones.
        var templateDir = Path.GetDirectoryName(templatePath)!;
        var preExistingTs = Directory.GetFiles(templateDir, "*.ts", SearchOption.AllDirectories)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var runner = new ApplicationRunner(
            inputResolver,
            restoreService,
            projectGraphService,
            roslynWorkspaceService,
            outputWriter,
            outputPathPolicy,
            cache);

        var options = GenerateCommandOptions.Merge(
            config: null,
            templates: [templatePath],
            solution: null,
            project: projectPath,
            framework: null,
            configuration: null,
            runtime: null,
            restore: false,
            output: null,
            verbosity: null,
            failOnWarnings: false,
            dryRun: true);

        // Act
        var exitCode = await runner.RunAsync(options, reporter);

        // Assert — exit code 0
        Assert.Equal(0, exitCode);

        // Assert — no new output files written to disk
        var postTs = Directory.GetFiles(templateDir, "*.ts", SearchOption.AllDirectories)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        postTs.ExceptWith(preExistingTs);
        Assert.Empty(postTs);

        // Assert — TW5001 per-file diagnostics emitted (at least one file would have been written)
        Assert.Contains(reporter.Messages, m => m.Code == DiagnosticCode.TW5001);

        // Assert — TW5002 summary diagnostic emitted
        Assert.Contains(reporter.Messages, m => m.Code == DiagnosticCode.TW5002);
    }
}
