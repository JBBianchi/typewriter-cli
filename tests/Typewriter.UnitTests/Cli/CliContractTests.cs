using Typewriter.Application;
using Typewriter.Application.Diagnostics;
using Xunit;

namespace Typewriter.UnitTests.Cli;

public class CliContractTests
{
    private sealed class FakeDiagnosticReporter : IDiagnosticReporter
    {
        private int _warningCount;
        private int _errorCount;

        public FakeDiagnosticReporter(int warningCount = 0, int errorCount = 0)
        {
            _warningCount = warningCount;
            _errorCount = errorCount;
        }

        public void Report(DiagnosticMessage message)
        {
            if (message.Severity == DiagnosticSeverity.Warning) _warningCount++;
            else if (message.Severity == DiagnosticSeverity.Error) _errorCount++;
        }

        public int WarningCount => _warningCount;
        public int ErrorCount => _errorCount;
    }

    [Fact]
    public async Task Generate_InvalidArgs_Returns2()
    {
        var runner = new ApplicationRunner();
        var reporter = new FakeDiagnosticReporter();

        // Empty templates + no solution/project → exit code 2
        var options = GenerateCommandOptions.Merge(
            config:        null,
            templates:     [],
            solution:      null,
            project:       null,
            framework:     null,
            configuration: null,
            runtime:       null,
            restore:       false,
            output:        null,
            verbosity:     null,
            failOnWarnings: false);

        var exitCode = await runner.RunAsync(options, reporter);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task Generate_WarningsWithFailFlag_Returns1()
    {
        var runner = new ApplicationRunner();
        // Pre-seed the reporter with 1 warning to simulate a prior warning being reported.
        var reporter = new FakeDiagnosticReporter(warningCount: 1);

        var options = GenerateCommandOptions.Merge(
            config:        null,
            templates:     ["tmpl.tst"],
            solution:      "my.sln",
            project:       null,
            framework:     null,
            configuration: null,
            runtime:       null,
            restore:       false,
            output:        null,
            verbosity:     null,
            failOnWarnings: true);

        var exitCode = await runner.RunAsync(options, reporter);

        Assert.Equal(1, exitCode);
    }
}
