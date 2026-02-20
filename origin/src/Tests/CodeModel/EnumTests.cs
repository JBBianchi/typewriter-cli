using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Xunit;

namespace Typewriter.Tests.CodeModel
{
    [Trait(nameof(CodeModel), "Enums")]
    [Collection(MockedVS.Collection)]
    public class RoslynEnumTests : EnumTests
    {
        public RoslynEnumTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class EnumTests : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;

        protected EnumTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\CodeModel\Support\EnumInfo.cs");
        }

        [Fact]
        public void Expect_name_to_match_enum_name()
        {
            var enumInfo = _fileInfo.Enums.First();

            enumInfo.Name.ShouldEqual("EnumInfo");
            enumInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.EnumInfo");
            enumInfo.Namespace.ShouldEqual("Typewriter.Tests.CodeModel.Support");
            enumInfo.Parent.ShouldEqual(_fileInfo);
        }

        [Fact]
        public void Expect_to_find_doc_comment()
        {
            var enumInfo = _fileInfo.Enums.First();
            enumInfo.DocComment.Summary.ShouldEqual("summary");
        }

        [Fact]
        public void Expect_to_find_attributes()
        {
            var enumInfo = _fileInfo.Enums.First();
            var attributeInfo = enumInfo.Attributes.First();

            enumInfo.Attributes.Count.ShouldEqual(1);
            attributeInfo.Name.ShouldEqual("AttributeInfo");
            attributeInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.AttributeInfoAttribute");
        }

        [Fact]
        public void Expect_to_find_value_attributes()
        {
            var enumInfo = _fileInfo.Enums.First();
            var valueInfo = enumInfo.Values.First();
            var attributeInfo = valueInfo.Attributes.First();

            enumInfo.Attributes.Count.ShouldEqual(1);
            attributeInfo.Name.ShouldEqual("AttributeInfo");
            attributeInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.AttributeInfoAttribute");
        }

        [Fact]
        public void Expect_unspecified_values_to_count_from_zero()
        {
            var enumInfo = _fileInfo.Enums.First();
            var firstValue = enumInfo.Values.First(v => string.Equals(v.Name, "ValueA", System.StringComparison.OrdinalIgnoreCase));
            var secondValue = enumInfo.Values.First(v => string.Equals(v.Name, "ValueB", System.StringComparison.OrdinalIgnoreCase));

            firstValue.Value.ShouldEqual(0);
            secondValue.Value.ShouldEqual(1);
        }

        [Fact]
        public void Expect_values_after_a_specified_values_to_count_from_the_prevoius_value()
        {
            var enumInfo = _fileInfo.Enums.First();
            var thirdValue = enumInfo.Values.First(v => string.Equals(v.Name, "ValueC", System.StringComparison.OrdinalIgnoreCase));
            var fourthValue = enumInfo.Values.First(v => string.Equals(v.Name, "ValueD", System.StringComparison.OrdinalIgnoreCase));

            thirdValue.Value.ShouldEqual(5);
            fourthValue.Value.ShouldEqual(6);
        }

        [Fact]
        public void Expect_values_specified_with_a_char_to_be_converted_to_their_integer_value()
        {
            var enumInfo = _fileInfo.Enums.First();
            var fifthValue = enumInfo.Values.First(v => string.Equals(v.Name, "ValueE", System.StringComparison.OrdinalIgnoreCase));
            var sixthValue = enumInfo.Values.First(v => string.Equals(v.Name, "ValueF", System.StringComparison.OrdinalIgnoreCase));

            fifthValue.Value.ShouldEqual('A'); // A = 65
            fifthValue.Value.ShouldEqual(65);
            sixthValue.Value.ShouldEqual(66);
        }

        [Fact]
        public void Expect_enums_marked_with_flags_attribute_to_be_flags()
        {
            var flagsEnumInfo = _fileInfo.Enums.First(e => string.Equals(e.Name, "FlagsEnumInfo", System.StringComparison.OrdinalIgnoreCase));
            flagsEnumInfo.IsFlags.ShouldBeTrue();
        }

        [Fact]
        public void Expect_enums_not_marked_with_flags_attribute_to_be_flags()
        {
            var enumInfo = _fileInfo.Enums.First();
            enumInfo.IsFlags.ShouldBeFalse();
        }

        [Fact]
        public void Expect_hexadecimal_values_converted_to_their_integer_value()
        {
            var hexEnumInfo = _fileInfo.Enums.First(e => string.Equals(e.Name, "HexEnumInfo", System.StringComparison.OrdinalIgnoreCase));
            var firstValue = hexEnumInfo.Values.First(v => string.Equals(v.Name, "ValueA", System.StringComparison.OrdinalIgnoreCase));
            var thirdValue = hexEnumInfo.Values.First(v => string.Equals(v.Name, "ValueC", System.StringComparison.OrdinalIgnoreCase));

            firstValue.Value.ShouldEqual(1);
            firstValue.Value.ShouldEqual(0x01);

            thirdValue.Value.ShouldEqual(3);
            thirdValue.Value.ShouldEqual(0x03);
        }

        [Fact]
        public void Expect_to_find_containing_class_on_nested_enum()
        {
            var classInfo = _fileInfo.Classes.First();
            var nestedEnumInfo = classInfo.NestedEnums.First();
            var containingClassInfo = nestedEnumInfo.ContainingClass;

            containingClassInfo.Name.ShouldEqual("EnumContiningClassInfo");
        }

        [Fact]
        public void Expect_not_to_find_containing_class_on_top_level_enum()
        {
            var enumInfo = _fileInfo.Enums.First();
            var containingClassInfo = enumInfo.ContainingClass;

            containingClassInfo.ShouldBeNull();
        }
    }
}
