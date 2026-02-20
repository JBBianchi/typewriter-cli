/*using System;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Typewriter.CodeModel.Configuration;
using Typewriter.CodeModel.Implementation;
using Typewriter.Configuration;
using Typewriter.Metadata.Providers;
using Xunit;
using File = Typewriter.CodeModel.File;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Typewriter.Tests.TestInfrastructure
{
    public abstract class TestBase
    {
        protected readonly DTE Dte;
        protected readonly IMetadataProvider MetadataProvider;

        protected readonly bool IsRoslyn;
        protected readonly bool IsCodeDom;

        protected TestBase(ITestFixture fixture, GlobalServiceProvider sp)
        {
            //sp.Reset();
            Dte = fixture.Dte;
            MetadataProvider = fixture.Provider;

            IsRoslyn = fixture is RoslynFixture;
            IsCodeDom = false;
        }

        protected string SolutionDirectory
        {
            get
            {
                const int retryCount = 10;
                const int delayMs = 500; // half a second delay between retries
                var fileName = string.Empty;
                for (int i = 0; i < retryCount; i++)
                {
                    try
                    {
                        fileName = Dte.Solution.FileName;
                        return Path.Combine(new FileInfo(fileName).Directory?.FullName, "src");
                    }
                    catch (COMException ex) when ((uint)ex.HResult == 0x8001010A)
                    {
                        // RPC_E_SERVERCALL_RETRYLATER
                        System.Threading.Thread.Sleep(delayMs);
                    }
                }

                throw new InvalidOperationException("Unable to get the solution filename after multiple retries.");
            }
        }

        protected ProjectItem GetProjectItem(string path)
        {
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
                settings = new SettingsImpl(null);
            }

            var metadata = MetadataProvider.GetFile(Path.Combine(SolutionDirectory, path), settings, requestRender);
            return new FileImpl(metadata, settings);
        }
    }
}
*/