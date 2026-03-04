using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Metadata;

namespace Typewriter.Metadata.Roslyn
{
    /// <summary>
    /// Roslyn-based implementation of <see cref="IMetadataProvider"/> that resolves
    /// file metadata from pre-loaded <see cref="Project"/>/<see cref="Compilation"/> pairs
    /// produced by the workspace loading pipeline.
    /// </summary>
    public class RoslynMetadataProvider : IMetadataProvider
    {
        private readonly IReadOnlyList<(Project Project, Compilation Compilation)> _entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynMetadataProvider"/> class.
        /// </summary>
        /// <param name="entries">
        /// The loaded project/compilation pairs, typically obtained from
        /// <c>WorkspaceLoadResult.Entries</c>.
        /// </param>
        public RoslynMetadataProvider(IReadOnlyList<(Project Project, Compilation Compilation)> entries)
        {
            _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        /// <inheritdoc />
        public IFileMetadata GetFile(string path, Settings settings, Action<string[]> requestRender)
        {
            foreach (var (project, compilation) in _entries)
            {
                var documentId = project.Solution.GetDocumentIdsWithFilePath(path).FirstOrDefault();
                if (documentId != null)
                {
                    var document = project.Solution.GetDocument(documentId);
                    if (document != null)
                    {
                        return new RoslynFileMetadata(document, compilation, settings, requestRender);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Enumerates all C# documents across every loaded project, yielding an
        /// <see cref="IFileMetadata"/> instance for each document.
        /// </summary>
        /// <param name="settings">Template settings controlling rendering behavior.</param>
        /// <param name="requestRender">
        /// Callback invoked when partial-rendering mode detects that a symbol's primary
        /// location is in a different file.
        /// </param>
        /// <returns>
        /// A lazily evaluated sequence of <see cref="IFileMetadata"/> instances for
        /// downstream template execution.
        /// </returns>
        public IEnumerable<IFileMetadata> GetFiles(Settings settings, Action<string[]> requestRender)
        {
            foreach (var (project, compilation) in _entries)
            {
                foreach (var document in project.Documents)
                {
                    if (document.SourceCodeKind == SourceCodeKind.Regular &&
                        document.FilePath?.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        yield return new RoslynFileMetadata(document, compilation, settings, requestRender);
                    }
                }
            }
        }
    }
}
