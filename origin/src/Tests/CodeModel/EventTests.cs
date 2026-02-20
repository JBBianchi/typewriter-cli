using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Xunit;

namespace Typewriter.Tests.CodeModel
{
    [Trait(nameof(CodeModel), "Events")]
    [Collection(MockedVS.Collection)]
    public class RoslynEventTests : EventTests
    {
        public RoslynEventTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class EventTests : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;

        protected EventTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\CodeModel\Support\EventInfo.cs");
        }

        [Fact]
        public void Expect_name_to_match_property_name()
        {
            var classInfo = _fileInfo.Classes.First();
            var enumInfo = classInfo.Events.First(p => string.Equals(p.Name, "DelegateEvent", System.StringComparison.OrdinalIgnoreCase));

            enumInfo.Name.ShouldEqual("DelegateEvent");
            enumInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.EventInfo.DelegateEvent");
            enumInfo.Parent.ShouldEqual(classInfo);
        }

        [Fact]
        public void Expect_to_find_doc_comment()
        {
            var classInfo = _fileInfo.Classes.First();
            var enumInfo = classInfo.Events.First(p => string.Equals(p.Name, "DelegateEvent", System.StringComparison.OrdinalIgnoreCase));
            enumInfo.DocComment.Summary.ShouldEqual("summary");
        }

        [Fact]
        public void Expect_to_find_attributes()
        {
            var classInfo = _fileInfo.Classes.First();
            var enumInfo = classInfo.Events.First(p => string.Equals(p.Name, "DelegateEvent", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = enumInfo.Attributes.First();

            enumInfo.Attributes.Count.ShouldEqual(1);
            attributeInfo.Name.ShouldEqual("AttributeInfo");
            attributeInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.AttributeInfoAttribute");
        }

        [Fact]
        public void Expect_generic_delegate_type_type_to_match_generic_argument()
        {
            var classInfo = _fileInfo.Classes.First();
            var eventInfo = classInfo.Events.First(e => string.Equals(e.Name, "GenericDelegateEvent", System.StringComparison.OrdinalIgnoreCase));
            var typeInfo = eventInfo.Type;

            typeInfo.Name.ShouldEqual("GenericDelegate<string>");

            typeInfo.TypeArguments.Count.ShouldEqual(1);
            typeInfo.TypeParameters.Count.ShouldEqual(1);

            typeInfo.TypeArguments.First().Name.ShouldEqual("string");
            if (IsRoslyn)
            {
                typeInfo.TypeParameters.First().Name.ShouldEqual("T");
            }
        }
    }
}