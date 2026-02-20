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
    public class RoslynWebApiRouteExtensionsTests : WebApiRouteExtensionsTests
    {
        public RoslynWebApiRouteExtensionsTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class WebApiRouteExtensionsTests : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;
        private readonly File _routeLessControllerInfo;

        protected WebApiRouteExtensionsTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\Extensions\Support\RouteController.cs");
            _routeLessControllerInfo = GetFile(@"Tests\Extensions\Support\RouteLessController.cs");
        }

        [Fact]
        public void Expect_to_route_on_routless_Controller_with_methodattribute()
        {
            var classInfo = _routeLessControllerInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "Test", System.StringComparison.OrdinalIgnoreCase));

            methodInfo.Url().ShouldEqual("api/RouteLess/${id}");
        }

        [Fact]
        public void Expect_to_route_on_routless_Controller_without_methodattribute()
        {
            var classInfo = _routeLessControllerInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "Test2", System.StringComparison.OrdinalIgnoreCase));

            methodInfo.Url().ShouldEqual("api/RouteLess/${id}");
        }

        [Fact]
        public void Expect_to_route_on_routless_Controller_without_methodattribute_and_inputparam()
        {
            var classInfo = _routeLessControllerInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "Test3", System.StringComparison.OrdinalIgnoreCase));

            methodInfo.Url().ShouldEqual("api/RouteLess/");
        }

        [Fact]
        public void Expect_to_route_on_routless_Controller_with_methodattribute_and_inputparam_custom_route()
        {
            var classInfo = _routeLessControllerInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "Test", System.StringComparison.OrdinalIgnoreCase));

            methodInfo.Url("api/{controller}/{action}/{id?}").ShouldEqual("api/RouteLess/test/${id}");
        }

        [Fact]
        public void Expect_to_find_parameters_on_wildcard_route_url()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "WildcardRoute", System.StringComparison.OrdinalIgnoreCase));

            methodInfo.Url().ShouldEqual("api/${encodeURIComponent(key)}");
        }

        [Fact]
        public void Expect_to_find_url_on_named_route()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "NamedRoute", System.StringComparison.OrdinalIgnoreCase));

            methodInfo.Url().ShouldEqual("api/${id}");
        }

        [Fact]
        public void Expect_to_find_url_on_httpget_route()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "HttpGetRoute", System.StringComparison.OrdinalIgnoreCase));

            methodInfo.Url().ShouldEqual("api/${id}");
        }

        [Fact]
        public void Expect_request_data_to_ignore_route_parameters()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "NamedRoute", System.StringComparison.OrdinalIgnoreCase));

            methodInfo.RequestData().ShouldEqual("null");
        }

        [Fact]
        public void Expect_to_find_url_on_action_without_route_attribute_and_id()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "NoRouteWithId", System.StringComparison.OrdinalIgnoreCase));

            var result = methodInfo.Url();
            result.ShouldEqual("api/Route/${id}");
        }

        [Fact]
        public void Expect_to_find_url_on_action_without_route_attribute()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "NoRoute", System.StringComparison.OrdinalIgnoreCase));

            var result = methodInfo.Url();
            result.ShouldEqual("api/Route/");
        }
    }
}