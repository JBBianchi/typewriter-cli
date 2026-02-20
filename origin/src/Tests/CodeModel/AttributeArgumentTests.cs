using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Typewriter.Tests.CodeModel.Support;
using Xunit;

namespace Typewriter.Tests.CodeModel
{
    [Trait(nameof(CodeModel), "AttributeArguments")]
    [Collection(MockedVS.Collection)]
    public class RoslynAttributeArgumentTests : AttributeArgumentTests
    {
        public RoslynAttributeArgumentTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class AttributeArgumentTests : TestInfrastructure.TestBase
    {
        private readonly Class _classInfo;

        protected AttributeArgumentTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            var fileInfo = GetFile(@"Tests\CodeModel\Support\AttributeTestClass.cs");
            _classInfo = fileInfo.Classes.First(c => string.Equals(c.Name, nameof(AttributeTestClass), System.StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Expect_attributes_with_no_parameters_to_have_an_empty_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "NoParameters", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            attributeInfo.Arguments.ShouldBeEmpty();
        }

        [Fact]
        public void Expect_attributes_with_string_parameter_to_have_a_string_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "StringParameter", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            var attributeArgument = attributeInfo.Arguments.Single();
            attributeArgument.Value.ShouldEqual("parameter");
            attributeArgument.Type.OriginalName.ShouldEqual("string");
        }

        [Fact]
        public void Expect_attributes_with_int_parameter_to_have_an_integer_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "IntParameter", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            var attributeArgument = attributeInfo.Arguments.Single();
            attributeArgument.Value.ShouldEqual(1);
            attributeArgument.Type.OriginalName.ShouldEqual("int");
        }

        [Fact]
        public void Expect_attributes_with_int_and_named_parameter_to_have_a_proper_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "IntAndNamedParameter", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            var integerArgument = attributeInfo.Arguments.First();
            integerArgument.Value.ShouldEqual(2);
            integerArgument.Type.OriginalName.ShouldEqual("int");

            var stringArgument = attributeInfo.Arguments.Skip(1).First();
            stringArgument.Value.ShouldEqual("parameter");
            stringArgument.Type.OriginalName.ShouldEqual("string");
        }

        [Fact]
        public void Expect_attributes_with_params_parameter_to_have_a_proper_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "ParamsParameter", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            var stringArgument1 = attributeInfo.Arguments.First();
            stringArgument1.Value.ShouldEqual(new object[] { "parameter1", "parameter2" });
            stringArgument1.Type.OriginalName.ShouldEqual("string[]");
        }

        [Fact]
        public void Expect_attributes_with_string_and_params_parameter_to_have_a_proper_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "IntAndParamsParameter", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            var integerArgument = attributeInfo.Arguments.First();
            integerArgument.Value.ShouldEqual(1);
            integerArgument.Type.OriginalName.ShouldEqual("int");

            var stringArgument = attributeInfo.Arguments.Skip(1).First();
            stringArgument.Value.ShouldEqual(new object[] { "parameter" });
            stringArgument.Type.OriginalName.ShouldEqual("string[]");
        }

        [Fact]
        public void Expect_attributes_with_type_parameter_to_have_a_proper_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, nameof(Type), System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            var integerArgument = attributeInfo.Arguments.Single();
            integerArgument.TypeValue.Name.ShouldEqual(nameof(AttributeTestClass));
            integerArgument.Type.OriginalName.ShouldEqual(nameof(Type));
        }
    }
}