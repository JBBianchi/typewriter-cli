using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Typewriter.Tests.CodeModel.Support;
using Xunit;

namespace Typewriter.Tests.CodeModel
{
    [Trait(nameof(CodeModel), "Attributes")]
    [Collection(MockedVS.Collection)]
    public class RoslynAttributeTests : AttributeTests
    {
        public RoslynAttributeTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class AttributeTests : TestInfrastructure.TestBase
    {
        private readonly Class _classInfo;

        protected AttributeTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            var fileInfo = GetFile(@"Tests\CodeModel\Support\AttributeTestClass.cs");
            _classInfo = fileInfo.Classes.First(c => string.Equals(c.Name, nameof(AttributeTestClass), System.StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Expect_name_to_match_attribute_name()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "NoParameters", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            propertyInfo.Attributes.Count.ShouldEqual(1);
            attributeInfo.Name.ShouldEqual("AttributeInfo");
            attributeInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.AttributeInfoAttribute");
        }

        [Fact]
        public void Expect_attributes_with_no_parameters_to_have_an_empty_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "NoParameters", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            attributeInfo.Value.ShouldBeNull();
        }

        [Fact]
        public void Expect_attributes_with_string_parameter_to_have_a_string_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "StringParameter", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            attributeInfo.Value.ShouldEqual("parameter");
        }

        [Fact]
        public void Expect_attributes_with_int_parameter_to_have_an_integer_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "IntParameter", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            attributeInfo.Value.ShouldEqual("1");
        }

        [Fact]
        public void Expect_attributes_with_int_and_named_parameter_to_have_a_proper_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "IntAndNamedParameter", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            attributeInfo.Value.ShouldEqual("2, Parameter = \"parameter\"");
        }

        [Fact]
        public void Expect_attributes_with_params_parameter_to_have_a_proper_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "ParamsParameter", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            attributeInfo.Value.ShouldEqual("\"parameter1\", \"parameter2\"");
        }

        [Fact]
        public void Expect_attributes_with_string_and_params_parameter_to_have_a_proper_value()
        {
            var propertyInfo = _classInfo.Properties.First(p => string.Equals(p.Name, "IntAndParamsParameter", System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = propertyInfo.Attributes.First();

            attributeInfo.Value.ShouldEqual("1, \"parameter\"");
        }
    }
}