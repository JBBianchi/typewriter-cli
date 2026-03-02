namespace Typewriter.Application.Diagnostics;

public interface IDiagnosticReporter
{
    void Report(DiagnosticMessage message);
    int WarningCount { get; }
    int ErrorCount { get; }
}
