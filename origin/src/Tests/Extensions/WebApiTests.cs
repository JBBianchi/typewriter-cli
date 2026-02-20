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
    public class RoslynWebApiExtensionsTests : WebApiExtensionsTests
    {
        public RoslynWebApiExtensionsTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
        }
    }

    public abstract class WebApiExtensionsTests : TestInfrastructure.TestBase
    {
        private readonly File _fileInfo;

        protected WebApiExtensionsTests(MefHostingFixture mefHostingFixture)
            : base(mefHostingFixture)
        {
            _fileInfo = GetFile(@"Tests\Extensions\Support\HttpMethodController.cs");
        }

        [Fact]
        public void Expect_httpmethod_to_match_convension_name()
        {
            var classInfo = _fileInfo.Classes.First();
            var getInfo = classInfo.Methods.First(p => string.Equals(p.Name, "Get", System.StringComparison.OrdinalIgnoreCase));
            var getAllMethod = classInfo.Methods.First(p => string.Equals(p.Name, "GetAll", System.StringComparison.OrdinalIgnoreCase));
            var listAllMethod = classInfo.Methods.First(p => string.Equals(p.Name, "ListAll", System.StringComparison.OrdinalIgnoreCase));

            getInfo.HttpMethod().ShouldEqual("get");
            getAllMethod.HttpMethod().ShouldEqual("get");
            listAllMethod.HttpMethod().ShouldEqual("post");
        }

        [Fact]
        public void Expect_httpmethod_to_match_http_attribute()
        {
            var classInfo = _fileInfo.Classes.First();
            var getMethod = classInfo.Methods.First(p => string.Equals(p.Name, "GetHttpAttibute", System.StringComparison.OrdinalIgnoreCase));

            getMethod.HttpMethod().ShouldEqual("post");
        }

        [Fact]
        public void Expect_httpmethod_to_match_acceptverbs_attribute()
        {
            var classInfo = _fileInfo.Classes.First();
            var getMethod = classInfo.Methods.First(p => string.Equals(p.Name, "GetAcceptVerbsAttribute", System.StringComparison.OrdinalIgnoreCase));
            var getMultipleMethod1 = classInfo.Methods.First(p => string.Equals(p.Name, "GetMultipleAcceptVerbsAttribute1", System.StringComparison.OrdinalIgnoreCase));
            var getMultipleMethod2 = classInfo.Methods.First(p => string.Equals(p.Name, "GetMultipleAcceptVerbsAttribute2", System.StringComparison.OrdinalIgnoreCase));

            getMethod.HttpMethod().ShouldEqual("head");
            getMultipleMethod1.HttpMethod().ShouldEqual("post");
            getMultipleMethod2.HttpMethod().ShouldEqual("head");
        }
    }
}