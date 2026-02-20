using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Xunit;

namespace Typewriter.Tests.CodeModel
{
    [Trait(nameof(CodeModel), "Classes")]
    [Collection(MockedVS.Collection)]
    public class RoslynClassTests : ClassTests
    {
        public RoslynClassTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class ClassTests : TestInfrastructure.TestBase
    {
        private File _fileInfo;

        protected ClassTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _fileInfo = GetFile(@"Tests\CodeModel\Support\ClassInfo.cs");
        }

        [Fact]
        public void Expect_name_to_match_class_name()
        {
            var classInfo = _fileInfo.Classes.First();

            classInfo.Name.ShouldEqual("ClassInfo");
            classInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.ClassInfo");
            classInfo.Namespace.ShouldEqual("Typewriter.Tests.CodeModel.Support");
            classInfo.Parent.ShouldEqual(_fileInfo);
        }

        [Fact]
        public void Expect_to_find_doc_comment()
        {
            var classInfo = _fileInfo.Classes.First();
            classInfo.DocComment.Summary.ShouldEqual("summary");
        }

        [Fact]
        public void Expect_to_find_attributes()
        {
            var classInfo = _fileInfo.Classes.First();
            var attributeInfo = classInfo.Attributes.First();

            classInfo.Attributes.Count.ShouldEqual(1);
            attributeInfo.Name.ShouldEqual("AttributeInfo");
            attributeInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.AttributeInfoAttribute");
        }

        [Fact]
        public void Expect_to_find_base_class()
        {
            var classInfo = _fileInfo.Classes.First();
            var baseClassInfo = classInfo.BaseClass;
            var propertyInfo = baseClassInfo.Properties.First();

            baseClassInfo.Name.ShouldEqual("BaseClassInfo");

            baseClassInfo.Properties.Count.ShouldEqual(1);
            propertyInfo.Name.ShouldEqual("PublicBaseProperty");
        }

        [Fact]
        public void Expect_not_to_find_object_base_class()
        {
            var classInfo = _fileInfo.Classes.First(c => string.Equals(c.Name, "BaseClassInfo", System.StringComparison.OrdinalIgnoreCase));

            classInfo.BaseClass.ShouldBeNull();
        }

        [Fact]
        public void Expect_to_find_interfaces()
        {
            var classInfo = _fileInfo.Classes.First();
            var interfaceInfo = classInfo.Interfaces.First();
            var propertyInfo = interfaceInfo.Properties.First();

            classInfo.Interfaces.Count.ShouldEqual(1);
            interfaceInfo.Name.ShouldEqual("IInterfaceInfo");

            interfaceInfo.Properties.Count.ShouldEqual(1);
            propertyInfo.Name.ShouldEqual("PublicProperty");
        }

        [Fact]
        public void Expect_non_generic_class_not_to_be_generic()
        {
            var classInfo = _fileInfo.Classes.First();

            classInfo.IsGeneric.ShouldBeFalse();
            classInfo.TypeParameters.Count.ShouldEqual(0);
        }

        [Fact]
        public void Expect_generic_class_to_be_generic()
        {
            var classInfo = _fileInfo.Classes.First(i => string.Equals(i.Name, "GenericClassInfo", System.StringComparison.OrdinalIgnoreCase));
            var genericTypeArgument = classInfo.TypeParameters.First();

            classInfo.IsGeneric.ShouldBeTrue();
            classInfo.TypeParameters.Count.ShouldEqual(1);
            genericTypeArgument.Name.ShouldEqual("T");
        }

        [Fact]
        public void Expect_to_find_public_constants()
        {
            var classInfo = _fileInfo.Classes.First();
            var constantInfo = classInfo.Constants.First();

            classInfo.Constants.Count.ShouldEqual(1);
            constantInfo.Name.ShouldEqual("PublicConstant");
        }

        [Fact]
        public void Expect_to_find_public_delegates()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First();

            classInfo.Delegates.Count.ShouldEqual(1);
            delegateInfo.Name.ShouldEqual("PublicDelegate");
        }

        [Fact]
        public void Expect_to_find_public_events()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Events.First();

            classInfo.Events.Count.ShouldEqual(1);
            delegateInfo.Name.ShouldEqual("PublicEvent");
        }

        [Fact]
        public void Expect_to_find_public_fields()
        {
            var classInfo = _fileInfo.Classes.First();
            var fieldInfo = classInfo.Fields.First();

            classInfo.Fields.Count.ShouldEqual(1);
            fieldInfo.Name.ShouldEqual("PublicField");
        }

        [Fact]
        public void Expect_to_find_public_methods()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First();

            classInfo.Methods.Count.ShouldEqual(1);
            methodInfo.Name.ShouldEqual("PublicMethod");
        }

        [Fact]
        public void Expect_to_find_public_properties()
        {
            var classInfo = _fileInfo.Classes.First();
            var propertyInfo = classInfo.Properties.First();

            classInfo.Properties.Count.ShouldEqual(1);
            propertyInfo.Name.ShouldEqual("PublicProperty");
        }

        [Fact]
        public void Expect_to_find_nested_public_classes()
        {
            var classInfo = _fileInfo.Classes.First();
            var nestedClassInfo = classInfo.NestedClasses.First();
            var propertyInfo = nestedClassInfo.Properties.First();

            classInfo.NestedClasses.Count.ShouldEqual(1);
            nestedClassInfo.Name.ShouldEqual("NestedClassInfo");

            nestedClassInfo.Properties.Count.ShouldEqual(1);
            propertyInfo.Name.ShouldEqual("PublicNestedProperty");
        }

        [Fact]
        public void Expect_to_find_nested_public_enums()
        {
            var classInfo = _fileInfo.Classes.First();
            var nestedEnumInfo = classInfo.NestedEnums.First();
            var valueInfo = nestedEnumInfo.Values.First();

            classInfo.NestedEnums.Count.ShouldEqual(1);
            nestedEnumInfo.Name.ShouldEqual("NestedEnumInfo");

            nestedEnumInfo.Values.Count.ShouldEqual(1);
            valueInfo.Name.ShouldEqual("NestedValue");
        }

        [Fact]
        public void Expect_to_find_nested_public_interfaces()
        {
            var classInfo = _fileInfo.Classes.First();
            var nestedInterfaceInfo = classInfo.NestedInterfaces.First();
            var propertyInfo = nestedInterfaceInfo.Properties.First();

            classInfo.NestedInterfaces.Count.ShouldEqual(1);
            nestedInterfaceInfo.Name.ShouldEqual("INestedInterfaceInfo");

            nestedInterfaceInfo.Properties.Count.ShouldEqual(1);
            propertyInfo.Name.ShouldEqual("PublicNestedProperty");
        }

        [Fact]
        public void Expect_to_find_containing_class_on_nested_class()
        {
            var classInfo = _fileInfo.Classes.First();
            var nestedClassInfo = classInfo.NestedClasses.First();
            var containingClassInfo = nestedClassInfo.ContainingClass;

            containingClassInfo.Name.ShouldEqual("ClassInfo");
        }

        [Fact]
        public void Expect_not_to_find_containing_class_on_top_level_class()
        {
            var classInfo = _fileInfo.Classes.First();
            var containingClassInfo = classInfo.ContainingClass;

            containingClassInfo.ShouldBeNull();
        }

        [Fact]
        public void Expect_generic_baseclass_to_have_type_arguments()
        {
            var classInfo = _fileInfo.Classes.First(m => string.Equals(m.Name, "InheritGenericClassInfo", System.StringComparison.OrdinalIgnoreCase));
            var genericTypeArgument = classInfo.BaseClass.TypeArguments.First();

            classInfo.BaseClass.IsGeneric.ShouldBeTrue();
            classInfo.BaseClass.TypeArguments.Count.ShouldEqual(1);

            genericTypeArgument.Name.ShouldEqual("string");

            if (IsRoslyn)
            {
                var genericTypeParameter = classInfo.BaseClass.TypeParameters.First();
                classInfo.BaseClass.TypeParameters.Count.ShouldEqual(1);
                genericTypeParameter.Name.ShouldEqual("T");
            }
        }
    }
}