namespace Typewriter.Application.Diagnostics;

public record DiagnosticMessage(
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    string? File = null,
    int? Line = null,
    int? Column = null);
