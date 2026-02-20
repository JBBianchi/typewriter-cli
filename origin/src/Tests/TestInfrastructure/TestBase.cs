using System;
using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Microsoft.VisualStudio.Shell;
using Typewriter.CodeModel.Configuration;
using Typewriter.CodeModel.Implementation;
using Typewriter.Configuration;
using Typewriter.Metadata.Providers;
using Typewriter.VisualStudio;
using Xunit;
using File = Typewriter.CodeModel.File;

namespace Typewriter.Tests.TestInfrastructure
{
    [Collection(MockedVS.Collection)]
    public class TestBase
        : IAsyncLifetime
    {
        public TestBase(MefHostingFixture mefHostingFixture)
        {
            MefHostingFixture = mefHostingFixture;
        }

        protected MefHostingFixture MefHostingFixture { get; }

        protected IAsyncServiceProvider AsyncServiceProvider { get; set; }

        protected DTE Dte { get; set; }

        protected IMetadataProvider MetadataProvider { get; set; }

        protected bool IsRoslyn => true;

        protected bool IsCodeDom => false;

        public virtual async Task InitializeAsync()
        {
            // Create the ExportProvider asynchronously
            var exportProvider = await MefHostingFixture.CreateExportProviderAsync();

            // Retrieve the IAsyncServiceProvider from the ExportProvider
            AsyncServiceProvider = exportProvider.GetExportedValue<IAsyncServiceProvider>();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Retrieve the DTE service asynchronously
            Dte = await AsyncServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;

            MetadataProvider = new RoslynMetadataProviderStub(Dte);

            MessageFilter.Register();
        }

        public virtual Task DisposeAsync()
        {
            MessageFilter.Revoke();
            return Task.CompletedTask;
        }

        protected string SolutionDirectory
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var fileName = Dte.Solution.FileName;
                return Path.Combine(new FileInfo(fileName).Directory!.FullName, "src");
            }
        }

        protected ProjectItem GetProjectItem(string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return Dte.Solution.FindProjectItem(Path.Combine(SolutionDirectory, path));
        }

        protected string GetFileContents(string path)
        {
            return System.IO.File.ReadAllText(Path.Combine(SolutionDirectory, path));
        }

        protected File GetFile(string path, Settings settings = null, Action<string[]> requestRender = null)
        {
            if (settings == null)
            {
                settings = new SettingsImpl(Log.Instance, null, string.Empty);
            }

            var metadata = MetadataProvider.GetFile(Path.Combine(SolutionDirectory, path), settings, requestRender);
            return new FileImpl(metadata, settings);
        }
    }
}