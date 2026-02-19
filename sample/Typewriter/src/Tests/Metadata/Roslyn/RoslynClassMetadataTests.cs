using System;
using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Typewriter.CodeModel.Configuration;
using Typewriter.Configuration;
using Typewriter.Tests.Metadata.Support;
using Typewriter.VisualStudio;
using Xunit;

namespace Typewriter.Tests.Metadata.Roslyn
{
    [Trait(nameof(CodeModel), "PartialClasses")]
    [Collection(MockedVS.Collection)]
    public class RoslynClassMetadataTests : TestInfrastructure.TestBase
    {
        public RoslynClassMetadataTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }

        private File GetFile(PartialRenderingMode partialRenderingMode)
        {
            var settings = new SettingsImpl(Log.Instance, null, string.Empty) { PartialRenderingMode = partialRenderingMode };
            return GetFile(@"Tests\Metadata\Support\GeneratedClass.cs", settings);
        }

        [Fact]
        public void Expect_all_properties_to_exist()
        {
            var fileInfo = GetFile(PartialRenderingMode.Partial);
            var classInfo = fileInfo.Classes.First();
            var properties = typeof(GeneratedClass).GetProperties();

            classInfo.Properties.All(op => properties.Any(cp => string.Equals(op.Name, cp.Name, StringComparison.OrdinalIgnoreCase)))
                .ShouldBeTrue();
        }

        [Fact]
        public void Expect_property_attributes_to_exist()
        {
            var fileInfo = GetFile(PartialRenderingMode.Partial);
            var classInfo = fileInfo.Classes.First();

            classInfo.Properties.Any(propertyInfo =>
                propertyInfo.Attributes.Any(a => string.Equals(a.Name, "Key", StringComparison.OrdinalIgnoreCase))
            ).ShouldBeTrue();

            var hasDisplayProperty = classInfo.Properties.Any(propertyInfo =>
                propertyInfo.Attributes
                    .Any(a => string.Equals(a.Name, "Display"
, StringComparison.OrdinalIgnoreCase) && a.Arguments.Any(arg =>
                            string.Equals(
                                arg.Value.ToString(),
                                "NewPropertyName",
                                StringComparison.InvariantCultureIgnoreCase
                            )
                        )
                    )
            );
            hasDisplayProperty.ShouldBeTrue();
        }
    }
}