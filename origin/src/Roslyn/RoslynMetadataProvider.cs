using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;
using Typewriter.Metadata.Providers;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynMetadataProvider : IMetadataProvider
    {
        private readonly Workspace _workspace;

        public RoslynMetadataProvider()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var componentModel = ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            _workspace = componentModel?.GetService<VisualStudioWorkspace>();
        }

        public IFileMetadata GetFile(string path, Settings settings, Action<string[]> requestRender)
        {
            var document = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(path).FirstOrDefault();
            if (document != null)
            {
                return new RoslynFileMetadata(_workspace.CurrentSolution.GetDocument(document), settings, requestRender);
            }

            return null;
        }
    }
}
