using System.Diagnostics;
using Typewriter.Application.Diagnostics;
using Typewriter.Application.Loading;

namespace Typewriter.Loading.MSBuild;

public sealed class RestoreService : IRestoreService
{
    public Task<bool> CheckAssetsAsync(string projectPath, CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(projectPath)!;
        var assetsFile = Path.Combine(dir, "obj", "project.assets.json");
        return Task.FromResult(File.Exists(assetsFile));
    }

    public async Task<bool> RestoreAsync(string projectPath, IDiagnosticReporter reporter, CancellationToken ct = default)
    {
        // -nodeReuse:false prevents MSBuild worker nodes from staying alive after restore.
        // Worker nodes that survive inherit the redirected stderr pipe handle, which causes
        // StandardError.ReadToEndAsync to hang indefinitely waiting for EOF.
        var psi = new ProcessStartInfo("dotnet", $"restore \"{projectPath}\" -nodeReuse:false")
        {
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        var stderrTask = process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            reporter.Report(new DiagnosticMessage(
                DiagnosticSeverity.Error,
                DiagnosticCode.TW2001,
                $"dotnet restore failed:{(string.IsNullOrWhiteSpace(stderr) ? string.Empty : $"\n{stderr.TrimEnd()}")}"));
            return false;
        }

        return true;
    }
}
