using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Xunit;

namespace Typewriter.Tests.CodeModel
{
    [Trait(nameof(CodeModel), "Constants")]
    [Collection(MockedVS.Collection)]
    public class RoslynConstantTests : ConstantTests
    {
        public RoslynConstantTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class ConstantTests : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;

        protected ConstantTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\CodeModel\Support\ConstantInfo.cs");
        }

        [Fact]
        public void Expect_name_to_match_cosntant_name()
        {
            var classInfo = _fileInfo.Classes.First();
            var constantInfo = classInfo.Constants.First();

            constantInfo.Name.ShouldEqual("StringField");
            constantInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.ConstantInfo.StringField");
            constantInfo.Parent.ShouldEqual(classInfo);
        }

        [Fact]
        public void Expect_to_find_doc_comment()
        {
            var classInfo = _fileInfo.Classes.First();
            var constantInfo = classInfo.Constants.First();
            constantInfo.DocComment.Summary.ShouldEqual("summary");
        }

        [Fact]
        public void Expect_value_not_empty()
        {
            var classInfo = _fileInfo.Classes.First();
            var constantInfo = classInfo.Constants.First();

            constantInfo.Value.ShouldEqual("test\"quotes\"");
        }

        [Fact]
        public void Expect_value_to_be_empty()
        {
            var classInfo = _fileInfo.Classes.First();
            var constantInfo = classInfo.Constants.Skip(1).First();

            constantInfo.Value.ShouldEqual(string.Empty);
        }

        [Fact]
        public void Expect_value_to_be_number()
        {
            var classInfo = _fileInfo.Classes.First();
            var constantInfo = classInfo.Constants.Skip(2).First();

            constantInfo.Value.ShouldEqual("123");
        }
    }
}