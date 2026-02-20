using System;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;
using Typewriter.Metadata.Providers;
using Typewriter.Metadata.Roslyn;

namespace Typewriter.Tests.TestInfrastructure
{
    public sealed class RoslynMetadataProviderStub
        : IMetadataProvider,
            IDisposable
    {
        private readonly Microsoft.CodeAnalysis.Workspace _workspace;

        public RoslynMetadataProviderStub(DTE dte)
        {
            //var solutionPath = dte.Solution.FullName;
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            var msBuildWorkspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

            // https://github.com/dotnet/roslyn/issues/73854#issuecomment-2148628705
            //var msBuildWorkspace = MSBuildWorkspace.Create();
            //msBuildWorkspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();
            _workspace = msBuildWorkspace;
        }

        public IFileMetadata GetFile(string path, Settings settings, Action<string[]> requestRender)
        {
            var document = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(path).FirstOrDefault();
            if (document != null)
            {
                var doc = _workspace.CurrentSolution.GetDocument(document);
                return new RoslynFileMetadata(doc!, settings, requestRender);
            }

            return null;
        }

        public void Dispose()
        {
            _workspace?.Dispose();
        }
    }
}
