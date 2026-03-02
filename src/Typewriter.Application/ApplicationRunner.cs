using Typewriter.Application.Diagnostics;

namespace Typewriter.Application;

/// <summary>
/// Orchestrates the generate pipeline. M2 stub — validates inputs only; generation logic added in M3+.
/// </summary>
public sealed class ApplicationRunner
{
    /// <summary>
    /// Validates inputs and returns an exit code.
    /// </summary>
    /// <returns>
    /// 0 — success;
    /// 1 — warnings elevated to errors (<see cref="GenerateCommandOptions.FailOnWarnings"/> is true and warnings exist);
    /// 2 — argument/input errors (empty templates, missing solution/project);
    /// 3 — SDK/restore/load/build errors (reserved; not triggered in M2 stub).
    /// </returns>
    public Task<int> RunAsync(
        GenerateCommandOptions options,
        IDiagnosticReporter reporter,
        CancellationToken cancellationToken = default)
    {
        if (options.Templates == null || options.Templates.Count == 0)
            return Task.FromResult(2);

        if (string.IsNullOrWhiteSpace(options.Solution) && string.IsNullOrWhiteSpace(options.Project))
        {
            reporter.Report(new DiagnosticMessage(
                DiagnosticSeverity.Error,
                DiagnosticCode.TW1002,
                "Either --solution or --project must be provided."));
            return Task.FromResult(2);
        }

        if (options.FailOnWarnings && reporter.WarningCount > 0)
            return Task.FromResult(1);

        return Task.FromResult(0);
    }
}
