namespace Typewriter.Application.Diagnostics;

/// <summary>
/// Writes MSBuild-format diagnostics to stderr and tracks warning/error counts.
/// Format: {file}({line},{col}): {severity} {code}: {message}
/// </summary>
public sealed class MsBuildDiagnosticReporter : IDiagnosticReporter
{
    private int _warningCount;
    private int _errorCount;

    public int WarningCount => _warningCount;
    public int ErrorCount => _errorCount;

    public void Report(DiagnosticMessage message)
    {
        if (message.Severity == DiagnosticSeverity.Error)
            _errorCount++;
        else if (message.Severity == DiagnosticSeverity.Warning)
            _warningCount++;

        Console.Error.WriteLine(Format(message));
    }

    public static string Format(DiagnosticMessage message)
    {
        var severity = message.Severity switch
        {
            DiagnosticSeverity.Error   => "error",
            DiagnosticSeverity.Warning => "warning",
            DiagnosticSeverity.Info    => "info",
            DiagnosticSeverity.Debug   => "debug",
            _                          => "info",
        };

        var location = message.File is null ? string.Empty
            : message.Line is null   ? $"{message.File}: "
            : message.Column is null ? $"{message.File}({message.Line}): "
            : $"{message.File}({message.Line},{message.Column}): ";

        return $"{location}{severity} {message.Code}: {message.Message}";
    }
}
