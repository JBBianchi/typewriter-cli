using System.Text.RegularExpressions;
using Typewriter.Application.Diagnostics;
using Xunit;

namespace Typewriter.UnitTests.Diagnostics;

public class DiagnosticFormatTests
{
    [Fact]
    public void MsBuildStyleMessage_IsParseable()
    {
        var message = new DiagnosticMessage(
            DiagnosticSeverity.Error,
            DiagnosticCode.TW1001,
            "Invalid template pattern",
            "template.tst",
            1,
            5);

        var formatted = MsBuildDiagnosticReporter.Format(message);

        var regex = new Regex(@"^.+\(\d+,\d+\): (error|warning) TW\d{4}: .+$");
        Assert.True(regex.IsMatch(formatted), $"Output '{formatted}' does not match MSBuild format");
    }

    [Theory]
    [InlineData(DiagnosticSeverity.Error,   "error")]
    [InlineData(DiagnosticSeverity.Warning, "warning")]
    [InlineData(DiagnosticSeverity.Info,    "info")]
    [InlineData(DiagnosticSeverity.Debug,   "debug")]
    public void Format_IncludesSeverityLabel(DiagnosticSeverity severity, string expectedLabel)
    {
        var message = new DiagnosticMessage(severity, "TW9001", "test", "f.tst", 1, 1);
        var formatted = MsBuildDiagnosticReporter.Format(message);
        Assert.Contains(expectedLabel, formatted);
    }

    [Fact]
    public void Format_NoLocation_OmitsParens()
    {
        var message = new DiagnosticMessage(DiagnosticSeverity.Error, DiagnosticCode.TW9001, "boom");
        var formatted = MsBuildDiagnosticReporter.Format(message);
        Assert.Equal("error TW9001: boom", formatted);
    }

    [Fact]
    public void Report_IncrementsErrorCount()
    {
        var reporter = new MsBuildDiagnosticReporter();
        reporter.Report(new DiagnosticMessage(DiagnosticSeverity.Error, DiagnosticCode.TW9001, "e"));
        Assert.Equal(1, reporter.ErrorCount);
        Assert.Equal(0, reporter.WarningCount);
    }

    [Fact]
    public void Report_IncrementsWarningCount()
    {
        var reporter = new MsBuildDiagnosticReporter();
        reporter.Report(new DiagnosticMessage(DiagnosticSeverity.Warning, DiagnosticCode.TW1001, "w"));
        Assert.Equal(0, reporter.ErrorCount);
        Assert.Equal(1, reporter.WarningCount);
    }
}
