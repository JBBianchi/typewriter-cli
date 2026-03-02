using Typewriter.Application.Diagnostics;

namespace Typewriter.Application.Loading;

public interface IInputResolver
{
    Task<ResolvedInput?> ResolveAsync(string projectPath, IDiagnosticReporter reporter, CancellationToken ct = default);
}
