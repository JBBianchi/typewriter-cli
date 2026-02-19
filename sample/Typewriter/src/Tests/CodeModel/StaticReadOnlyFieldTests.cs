using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Xunit;

namespace Typewriter.Tests.CodeModel
{
    [Trait(nameof(CodeModel), "StaticReadOnlyFields")]
    [Collection(MockedVS.Collection)]
    public class RoslynStaticReadOnlyFieldTests : StaticReadOnlyFieldTests
    {
        public RoslynStaticReadOnlyFieldTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class StaticReadOnlyFieldTests : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;

        protected StaticReadOnlyFieldTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\CodeModel\Support\StaticReadOnlyFieldInfo.cs");
        }

        [Fact]
        public void Expect_name_to_match_static_readonly_name()
        {
            var classInfo = _fileInfo.Classes.First();
            var staticReadOnlyFieldInfo = classInfo.StaticReadOnlyFields.First();

            staticReadOnlyFieldInfo.Name.ShouldEqual("StringField");
            staticReadOnlyFieldInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.StaticReadOnlyFieldInfo.StringField");
            staticReadOnlyFieldInfo.Parent.ShouldEqual(classInfo);
        }

        [Fact]
        public void Expect_to_find_doc_comment()
        {
            var classInfo = _fileInfo.Classes.First();
            var staticReadOnlyFieldInfo = classInfo.StaticReadOnlyFields.First();
            staticReadOnlyFieldInfo.DocComment.Summary.ShouldEqual("summary");
        }

        [Fact]
        public void Expect_value_not_empty()
        {
            var classInfo = _fileInfo.Classes.First();
            var staticReadOnlyFieldInfo = classInfo.StaticReadOnlyFields.First();

            staticReadOnlyFieldInfo.Value.ShouldEqual("test\"quotes\"");
        }

        [Fact]
        public void Expect_value_to_be_empty()
        {
            var classInfo = _fileInfo.Classes.First();
            var staticReadOnlyFieldInfo = classInfo.StaticReadOnlyFields.Skip(1).First();

            staticReadOnlyFieldInfo.Value.ShouldEqual(string.Empty);
        }

        [Fact]
        public void Expect_value_to_be_number()
        {
            var classInfo = _fileInfo.Classes.First();
            var staticReadOnlyFieldInfo = classInfo.StaticReadOnlyFields.Skip(2).First();

            staticReadOnlyFieldInfo.Value.ShouldEqual("123");
        }
    }
}