using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Xunit;
using File = Typewriter.CodeModel.File;

namespace Typewriter.Tests.CodeModel
{
    [Trait(nameof(CodeModel), "Files")]
    [Collection(MockedVS.Collection)]
    public class RoslynFileTests : FileTests
    {
        public RoslynFileTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class FileTests : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;

        protected FileTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\CodeModel\Support\FileInfo.cs");
        }

        [Fact]
        public void Expect_name_to_match_filename()
        {
            _fileInfo.Name.ShouldEqual("FileInfo.cs");
            _fileInfo.FullName.ShouldEqual(Path.Combine(SolutionDirectory, @"Tests\CodeModel\Support\FileInfo.cs"));
        }

        [Fact]
        public void Expect_to_find_public_classes()
        {
            _fileInfo.Classes.Count.ShouldEqual(2);

            var classInfo1 = _fileInfo.Classes.First();
            classInfo1.Name.ShouldEqual(nameof(PublicClassNoNamespace));

            var classInfo2 = _fileInfo.Classes.Last();
            classInfo2.Name.ShouldEqual("PublicClass");
        }

        [Fact]
        public void Expect_to_find_public_delegates()
        {
            _fileInfo.Delegates.Count.ShouldEqual(2);

            var delegateInfo1 = _fileInfo.Delegates.First();
            delegateInfo1.Name.ShouldEqual(nameof(PublicDelegateNoNamespace));

            var delegateInfo2 = _fileInfo.Delegates.Last();
            delegateInfo2.Name.ShouldEqual("PublicDelegate");
        }

        [Fact]
        public void Expect_to_find_public_enums()
        {
            _fileInfo.Enums.Count.ShouldEqual(2);

            var enumInfo1 = _fileInfo.Enums.First();
            enumInfo1.Name.ShouldEqual(nameof(PublicEnumNoNamespace));

            var enumInfo2 = _fileInfo.Enums.Last();
            enumInfo2.Name.ShouldEqual("PublicEnum");
        }

        [Fact]
        public void Expect_to_find_public_interfaces()
        {
            _fileInfo.Interfaces.Count.ShouldEqual(2);

            var interfaceInfo1 = _fileInfo.Interfaces.First();
            interfaceInfo1.Name.ShouldEqual(nameof(IPublicInterfaceNoNamespace));

            var interfaceInfo2 = _fileInfo.Interfaces.Last();
            interfaceInfo2.Name.ShouldEqual("IPublicInterface");
        }
    }
}