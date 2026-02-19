using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Xunit;

namespace Typewriter.Tests.CodeModel
{
    [Trait(nameof(CodeModel), "Interfaces")]
    [Collection(MockedVS.Collection)]
    public class RoslynInterfaceTests : InterfaceTests
    {
        public RoslynInterfaceTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class InterfaceTests : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;

        protected InterfaceTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\CodeModel\Support\IInterfaceInfo.cs");
        }

        [Fact]
        public void Expect_name_to_match_interface_name()
        {
            var interfaceInfo = _fileInfo.Interfaces.First();

            interfaceInfo.Name.ShouldEqual("IInterfaceInfo");
            interfaceInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.IInterfaceInfo");
            interfaceInfo.Namespace.ShouldEqual("Typewriter.Tests.CodeModel.Support");
            interfaceInfo.Parent.ShouldEqual(_fileInfo);
        }

        [Fact]
        public void Expect_to_find_doc_comment()
        {
            var interfaceInfo = _fileInfo.Interfaces.First();
            interfaceInfo.DocComment.Summary.ShouldEqual("summary");
        }

        [Fact]
        public void Expect_to_find_attributes()
        {
            var interfaceInfo = _fileInfo.Interfaces.First();
            var attributeInfo = interfaceInfo.Attributes.First();

            interfaceInfo.Attributes.Count.ShouldEqual(1);
            attributeInfo.Name.ShouldEqual("AttributeInfo");
            attributeInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.AttributeInfoAttribute");
        }

        [Fact]
        public void Expect_non_generic_interface_not_to_be_generic()
        {
            var interfaceInfo = _fileInfo.Interfaces.First();

            interfaceInfo.IsGeneric.ShouldBeFalse();
            interfaceInfo.TypeParameters.Count.ShouldEqual(0);
        }

        [Fact]
        public void Expect_generic_interface_to_be_generic()
        {
            var interfaceInfo = _fileInfo.Interfaces.First(i => string.Equals(i.Name, "IGenericInterface", System.StringComparison.OrdinalIgnoreCase));
            var genericTypeArgument = interfaceInfo.TypeParameters.First();

            interfaceInfo.IsGeneric.ShouldBeTrue();
            interfaceInfo.TypeParameters.Count.ShouldEqual(1);
            genericTypeArgument.Name.ShouldEqual("T");
        }

        [Fact]
        public void Expect_to_find_public_events()
        {
            var interfaceInfo = _fileInfo.Interfaces.First();
            var delegateInfo = interfaceInfo.Events.First();

            interfaceInfo.Events.Count.ShouldEqual(1);
            delegateInfo.Name.ShouldEqual("PublicEvent");
        }

        [Fact]
        public void Expect_to_find_interfaces()
        {
            var interfaceInfo = _fileInfo.Interfaces.First();
            var implementedInterfaceInfo = interfaceInfo.Interfaces.First();
            var propertyInfo = implementedInterfaceInfo.Properties.First();

            interfaceInfo.Interfaces.Count.ShouldEqual(1);
            implementedInterfaceInfo.Name.ShouldEqual("IBaseInterfaceInfo");

            implementedInterfaceInfo.Properties.Count.ShouldEqual(1);
            propertyInfo.Name.ShouldEqual("PublicBaseProperty");
        }

        [Fact]
        public void Expect_to_find_methods()
        {
            var interfaceInfo = _fileInfo.Interfaces.First();
            var methodInfo = interfaceInfo.Methods.First();

            interfaceInfo.Methods.Count.ShouldEqual(1);
            methodInfo.Name.ShouldEqual("PublicMethod");
        }

        [Fact]
        public void Expect_to_find_properties()
        {
            var interfaceInfo = _fileInfo.Interfaces.First();
            var propertyInfo = interfaceInfo.Properties.First();

            interfaceInfo.Properties.Count.ShouldEqual(1);
            propertyInfo.Name.ShouldEqual("PublicProperty");
        }

        [Fact]
        public void Expect_to_find_containing_class_on_nested_interface()
        {
            var classInfo = _fileInfo.Classes.First();
            var nestedInterfaceInfo = classInfo.NestedInterfaces.First();
            var containingClassInfo = nestedInterfaceInfo.ContainingClass;

            containingClassInfo.Name.ShouldEqual("InterfaceContiningClassInfo");
        }

        [Fact]
        public void Expect_not_to_find_containing_class_on_top_level_interface()
        {
            var interfaceInfo = _fileInfo.Interfaces.First();
            var containingClassInfo = interfaceInfo.ContainingClass;

            containingClassInfo.ShouldBeNull();
        }

        [Fact]
        public void Expect_inherited_generic_interface_to_have_type_arguments()
        {
            var interfaceInfo = _fileInfo.Interfaces.First(m => string.Equals(m.Name, "IInheritGenericInterfaceInfo", System.StringComparison.OrdinalIgnoreCase));
            var genericTypeArgument = interfaceInfo.Interfaces.First().TypeArguments.First();

            interfaceInfo.Interfaces.First().IsGeneric.ShouldBeTrue();
            interfaceInfo.Interfaces.First().TypeArguments.Count.ShouldEqual(1);

            genericTypeArgument.Name.ShouldEqual("string");

            if (IsRoslyn)
            {
                var genericTypeParameter = interfaceInfo.Interfaces.First().TypeParameters.First();
                interfaceInfo.Interfaces.First().TypeParameters.Count.ShouldEqual(1);
                genericTypeParameter.Name.ShouldEqual("T");
            }
        }
    }
}