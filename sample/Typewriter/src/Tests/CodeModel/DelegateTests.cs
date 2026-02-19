using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Xunit;

namespace Typewriter.Tests.CodeModel
{
    [Trait(nameof(CodeModel), "Delegates")]
    [Collection(MockedVS.Collection)]
    public class RoslynDelegateTests : DelegateTests
    {
        public RoslynDelegateTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class DelegateTests : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;

        protected DelegateTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\CodeModel\Support\DelegateInfo.cs");
        }

        [Fact]
        public void Expect_name_to_match_delegate_name()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, nameof(Delegate), System.StringComparison.OrdinalIgnoreCase));

            delegateInfo.Name.ShouldEqual(nameof(Delegate));
            delegateInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.DelegateInfo.Delegate");
            delegateInfo.Parent.ShouldEqual(classInfo);
        }

        [Fact]
        public void Expect_to_find_doc_comment()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, nameof(Delegate), System.StringComparison.OrdinalIgnoreCase));
            delegateInfo.DocComment.Summary.ShouldEqual("summary");
            delegateInfo.DocComment.Parameters.First().Name.ShouldEqual("parameter");
            delegateInfo.DocComment.Parameters.First().Description.ShouldEqual("param");
        }

        [Fact]
        public void Expect_to_find_attributes()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, nameof(Delegate), System.StringComparison.OrdinalIgnoreCase));
            var attributeInfo = delegateInfo.Attributes.First();

            delegateInfo.Attributes.Count.ShouldEqual(1);
            attributeInfo.Name.ShouldEqual("AttributeInfo");
            attributeInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.AttributeInfoAttribute");
        }

        [Fact]
        public void Expect_to_find_parameters()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, nameof(Delegate), System.StringComparison.OrdinalIgnoreCase));
            var parameterInfo = delegateInfo.Parameters.First();

            delegateInfo.Parameters.Count.ShouldEqual(1);
            parameterInfo.Name.ShouldEqual("parameter");
        }

        [Fact]
        public void Expect_to_find_parameter_attributes()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, nameof(Delegate), System.StringComparison.OrdinalIgnoreCase));
            var parameterInfo = delegateInfo.Parameters.First();
            var attributeInfo = parameterInfo.Attributes.First();

            parameterInfo.Attributes.Count.ShouldEqual(1);
            attributeInfo.Name.ShouldEqual("AttributeInfo");
            attributeInfo.FullName.ShouldEqual("Typewriter.Tests.CodeModel.Support.AttributeInfoAttribute");
        }

        [Fact]
        public void Expect_void_delegates_to_return_void()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, nameof(Delegate), System.StringComparison.OrdinalIgnoreCase));

            delegateInfo.Type.FullName.ShouldEqual("System.Void");
            delegateInfo.Type.Name.ShouldEqual("void");
            delegateInfo.Type.OriginalName.ShouldEqual("Void");
            delegateInfo.Type.IsEnum.ShouldBeFalse("IsEnum");
            delegateInfo.Type.IsEnumerable.ShouldBeFalse("IsEnumerable");
            delegateInfo.Type.IsGeneric.ShouldBeFalse("IsGeneric");
            delegateInfo.Type.IsNullable.ShouldBeFalse("IsNullable");
            delegateInfo.Type.IsTask.ShouldBeFalse("IsTask");
            delegateInfo.Type.IsPrimitive.ShouldBeFalse("IsPrimitive");
            delegateInfo.Type.IsDictionary.ShouldBeFalse("IsDictionary");
        }

        [Fact]
        public void Expect_generic_delegates_to_handle_generic_type_arguments()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, "Generic", System.StringComparison.OrdinalIgnoreCase));
            var genericTypeInfo = delegateInfo.TypeParameters.First();
            var parameterTypeInfo = delegateInfo.Parameters.First().Type;

            delegateInfo.IsGeneric.ShouldBeTrue("IsGeneric");
            delegateInfo.TypeParameters.Count.ShouldEqual(1);

            delegateInfo.Type.Name.ShouldEqual("T");
            delegateInfo.Type.FullName.ShouldEqual("T");
            genericTypeInfo.Name.ShouldEqual("T");
            parameterTypeInfo.Name.ShouldEqual("T");
            parameterTypeInfo.FullName.ShouldEqual("T");
        }

        [Fact]
        public void Expect_delegates_to_handle_generic_type_arguments_from_class()
        {
            var classInfo = _fileInfo.Classes.First(c => string.Equals(c.Name, "GenericDelegateInfo", System.StringComparison.OrdinalIgnoreCase));
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, nameof(Delegate), System.StringComparison.OrdinalIgnoreCase));
            var parameterTypeInfo = delegateInfo.Parameters.First().Type;

            delegateInfo.IsGeneric.ShouldBeFalse("IsGeneric");
            delegateInfo.TypeParameters.Count.ShouldEqual(0);

            delegateInfo.Type.Name.ShouldEqual("T");
            delegateInfo.Type.FullName.ShouldEqual("T");
            parameterTypeInfo.Name.ShouldEqual("T");
            parameterTypeInfo.FullName.ShouldEqual("T");
        }

        [Fact]
        public void Expect_generic_delegates_to_handle_generic_type_arguments_from_class_and_delegate()
        {
            var classInfo = _fileInfo.Classes.First(c => string.Equals(c.Name, "GenericDelegateInfo", System.StringComparison.OrdinalIgnoreCase));
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, "Generic", System.StringComparison.OrdinalIgnoreCase));
            var firstParameterTypeInfo = delegateInfo.Parameters.First(p => string.Equals(p.Name, "parameter1", System.StringComparison.OrdinalIgnoreCase)).Type;
            var secondParameterTypeInfo = delegateInfo.Parameters.First(p => string.Equals(p.Name, "parameter2", System.StringComparison.OrdinalIgnoreCase)).Type;

            delegateInfo.IsGeneric.ShouldBeTrue("IsGeneric");
            delegateInfo.TypeParameters.Count.ShouldEqual(1);

            delegateInfo.Type.Name.ShouldEqual("T1");
            delegateInfo.Type.FullName.ShouldEqual("T1");
            firstParameterTypeInfo.Name.ShouldEqual("T1");
            firstParameterTypeInfo.FullName.ShouldEqual("T1");
            secondParameterTypeInfo.Name.ShouldEqual("T");
            secondParameterTypeInfo.FullName.ShouldEqual("T");
        }

        [Fact]
        public void Expect_task_delegates_to_return_void()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, "Task", System.StringComparison.OrdinalIgnoreCase));

            delegateInfo.Type.FullName.ShouldEqual("System.Void");
            delegateInfo.Type.Name.ShouldEqual("void");
            delegateInfo.Type.OriginalName.ShouldEqual("Void");
            delegateInfo.Type.IsEnum.ShouldBeFalse("IsEnum");
            delegateInfo.Type.IsEnumerable.ShouldBeFalse("IsEnumerable");
            delegateInfo.Type.IsGeneric.ShouldBeFalse("IsGeneric");
            delegateInfo.Type.IsNullable.ShouldBeFalse("IsNullable");
            delegateInfo.Type.IsTask.ShouldBeTrue("IsTask");
            delegateInfo.Type.IsPrimitive.ShouldBeFalse("IsPrimitive");
            delegateInfo.Type.IsDictionary.ShouldBeFalse("IsDictionary");
        }

        [Fact]
        public void Expect_task_string_delegates_to_return_string()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, "TaskString", System.StringComparison.OrdinalIgnoreCase));

            delegateInfo.Type.FullName.ShouldEqual("System.String");
            delegateInfo.Type.Name.ShouldEqual("string");
            delegateInfo.Type.OriginalName.ShouldEqual("string");
            delegateInfo.Type.IsEnum.ShouldBeFalse("IsEnum");
            delegateInfo.Type.IsEnumerable.ShouldBeFalse("IsEnumerable");
            delegateInfo.Type.IsGeneric.ShouldBeFalse("IsGeneric");
            delegateInfo.Type.IsNullable.ShouldBeFalse("IsNullable");
            delegateInfo.Type.IsTask.ShouldBeTrue("IsTask");
            delegateInfo.Type.IsPrimitive.ShouldBeTrue("IsPrimitive");
            delegateInfo.Type.IsDictionary.ShouldBeFalse("IsDictionary");
        }

        [Fact]
        public void Expect_task_nullable_int_delegates_to_return_int()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, "TaskNullableInt", System.StringComparison.OrdinalIgnoreCase));

            delegateInfo.Type.FullName.ShouldEqual("System.Int32?");
            delegateInfo.Type.Name.ShouldEqual("number | null");
            delegateInfo.Type.OriginalName.ShouldEqual("int?");
            delegateInfo.Type.IsEnum.ShouldBeFalse("IsEnum");
            delegateInfo.Type.IsEnumerable.ShouldBeFalse("IsEnumerable");
            delegateInfo.Type.IsGeneric.ShouldBeFalse("IsGeneric");
            delegateInfo.Type.IsNullable.ShouldBeTrue("IsNullable");
            delegateInfo.Type.IsTask.ShouldBeTrue("IsTask");
            delegateInfo.Type.IsPrimitive.ShouldBeTrue("IsPrimitive");
            delegateInfo.Type.IsDictionary.ShouldBeFalse("IsDictionary");
        }

        [Fact]
        public void Expect_task_Dictionary_delegates_to_return_dictionary()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, "TaskDictionary", System.StringComparison.OrdinalIgnoreCase));

            delegateInfo.Type.FullName.ShouldEqual("System.Collections.Generic.Dictionary<System.String, System.String>");
            delegateInfo.Type.Name.ShouldEqual("Record<string, string>");
            delegateInfo.Type.OriginalName.ShouldEqual("Dictionary");
            delegateInfo.Type.IsEnum.ShouldBeFalse("IsEnum");
            delegateInfo.Type.IsEnumerable.ShouldBeTrue("IsEnumerable");
            delegateInfo.Type.IsGeneric.ShouldBeTrue("IsGeneric");
            delegateInfo.Type.IsNullable.ShouldBeFalse("IsNullable");
            delegateInfo.Type.IsTask.ShouldBeTrue("IsTask");
            delegateInfo.Type.IsPrimitive.ShouldBeTrue("IsPrimitive");
            delegateInfo.Type.IsDictionary.ShouldBeTrue("IsDictionary");
        }

        [Fact]
        public void Expect_task_IDictionary_delegates_to_return_idictionary()
        {
            var classInfo = _fileInfo.Classes.First();
            var delegateInfo = classInfo.Delegates.First(p => string.Equals(p.Name, "TaskIDictionary", System.StringComparison.OrdinalIgnoreCase));

            delegateInfo.Type.FullName.ShouldEqual("System.Collections.Generic.IDictionary<System.String, System.String>");
            delegateInfo.Type.Name.ShouldEqual("Record<string, string>");
            delegateInfo.Type.OriginalName.ShouldEqual("IDictionary");
            delegateInfo.Type.IsEnum.ShouldBeFalse("IsEnum");
            delegateInfo.Type.IsEnumerable.ShouldBeTrue("IsEnumerable");
            delegateInfo.Type.IsGeneric.ShouldBeTrue("IsGeneric");
            delegateInfo.Type.IsNullable.ShouldBeFalse("IsNullable");
            delegateInfo.Type.IsTask.ShouldBeTrue("IsTask");
            delegateInfo.Type.IsPrimitive.ShouldBeTrue("IsPrimitive");
            delegateInfo.Type.IsDictionary.ShouldBeTrue("IsDictionary");
        }
    }
}