using Typewriter.Application.Diagnostics;

namespace Typewriter.Application.Loading;

public interface IRestoreService
{
    Task<bool> CheckAssetsAsync(string projectPath, CancellationToken ct = default);
    Task<bool> RestoreAsync(string projectPath, IDiagnosticReporter reporter, CancellationToken ct = default);
}
