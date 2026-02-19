using System.Linq;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Should;
using Typewriter.CodeModel;
using Typewriter.Extensions.WebApi;
using Xunit;

namespace Typewriter.Tests.Extensions
{
    [Trait(nameof(Extensions), "WebApi")]
    [Collection(MockedVS.Collection)]
    public class WebApiRouteTestsClassWithNullableRouteTests : WebApiRouteTestsClassWithNullableRoute
    {
        public WebApiRouteTestsClassWithNullableRouteTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class WebApiRouteTestsClassWithNullableRoute : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;

        protected WebApiRouteTestsClassWithNullableRoute(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\Extensions\Support\RouteControllerWithNullableParts.cs");
        }

        [Fact]
        public void Expect_to_find_url_on_in_httpget_action_attribute_with_nullable_and_non_nullable_string()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(
                p => string.Equals(
                    p.Name,
                    "RouteInHttpAttributeWithNullableAndNonNullableString",
                    System.StringComparison.OrdinalIgnoreCase));

            var result = methodInfo.Url();
            result.ShouldEqual("api/RouteControllerWithNullableParts/${encodeURIComponent(name)}?filter=${encodeURIComponent(filter)}");
        }
    }
}