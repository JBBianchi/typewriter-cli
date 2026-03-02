using Typewriter.Metadata;

namespace Typewriter.Metadata.Roslyn
{
    /// <summary>
    /// Minimal stub for <c>RoslynFileMetadata</c> used by the wrapper classes to
    /// support partial-rendering mode filtering.
    /// The full VS-coupled implementation (using <c>ThreadHelper.JoinableTaskFactory</c>
    /// and <c>Microsoft.CodeAnalysis.Document</c>) is deferred to M5.
    /// </summary>
    public class RoslynFileMetadata
    {
        public RoslynFileMetadata(Settings settings, string fullName)
        {
            Settings = settings;
            FullName = fullName;
        }

        public Settings Settings { get; }

        public string FullName { get; }
    }
}
