using Typewriter.Application.Diagnostics;

namespace Typewriter.Application.Loading;

public interface IMsBuildLocatorService
{
    void EnsureRegistered(IDiagnosticReporter reporter);
}
