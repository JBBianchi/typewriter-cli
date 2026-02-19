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
    public class RoslynWebApiRouteClassRouteExtensionsTests : WebApiRouteClassRouteExtensionsTests
    {
        public RoslynWebApiRouteClassRouteExtensionsTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class WebApiRouteClassRouteExtensionsTests : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;
        private readonly File _inheritedFileInfo;

        protected WebApiRouteClassRouteExtensionsTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\Extensions\Support\RouteControllerWithDefaultRoute.cs");
            _inheritedFileInfo = GetFile(@"Tests\Extensions\Support\InheritedController.cs");
        }

        [Fact]
        public void Expect_to_find_parameters_on_wildcard_route_url()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "WildcardRoute", System.StringComparison.OrdinalIgnoreCase));
            var result = methodInfo.Url();
            result.ShouldEqual("api/RouteControllerWithDefaultRoute/${encodeURIComponent(key)}");
        }

        [Fact]
        public void Expect_to_find_url_on_named_route()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "NamedRoute", System.StringComparison.OrdinalIgnoreCase));
            var result = methodInfo.Url();
            result.ShouldEqual("api/RouteControllerWithDefaultRoute/${id}");
        }

        [Fact]
        public void Expect_to_find_url_on_route_in_http_attribute()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "RouteInHttpAttribute", System.StringComparison.OrdinalIgnoreCase));

            methodInfo.Url().ShouldEqual("api/RouteControllerWithDefaultRoute/${id}");
        }

        [Fact]
        public void Expect_to_find_url_on_subroute_in_http_attribute()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "SubRouteInHttpAttribute", System.StringComparison.OrdinalIgnoreCase));

            methodInfo.Url().ShouldEqual("api/RouteControllerWithDefaultRoute/sub/${id}");
        }

        [Fact]
        public void Expect_to_find_url_on_in_httpget_action_attribute()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "ActionTestInheritedClassController", System.StringComparison.OrdinalIgnoreCase));

            var result = methodInfo.Url();
            result.ShouldEqual("api/RouteControllerWithDefaultRoute/actionTestInheritedClassController");
        }

        [Fact]
        public void Expect_to_find_url_on_BaseController_HttpGet_Parameter()
        {
            var classInfo = _inheritedFileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "RoutePrefixFromBaseHttpGetWithParameter", System.StringComparison.OrdinalIgnoreCase));

            var result = methodInfo.Url();
            result.ShouldEqual("api/Inherited/inherited/${id}");
        }

        [Fact]
        public void Expect_to_find_url_on_in_httpget_action_withparameter()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "ActionTestInheritedClassControllerPostWithParameter", System.StringComparison.OrdinalIgnoreCase));

            var result = methodInfo.Url();
            result.ShouldEqual("api/RouteControllerWithDefaultRoute/actionTestInheritedClassControllerPostWithParameter/${id}");
        }

        [Fact]
        public void Expect_to_find_url_on_in_httppost_action_withparameter()
        {
            var classInfo = _fileInfo.Classes.First();
            var methodInfo = classInfo.Methods.First(p => string.Equals(p.Name, "ActionTestInheritedClassControllerPostWithParameter", System.StringComparison.OrdinalIgnoreCase));

            var result = methodInfo.Url();
            result.ShouldEqual("api/RouteControllerWithDefaultRoute/actionTestInheritedClassControllerPostWithParameter/${id}");
        }
    }
}