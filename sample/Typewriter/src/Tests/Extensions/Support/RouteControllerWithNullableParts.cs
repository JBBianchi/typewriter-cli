#nullable enable
using Typewriter.Tests.Support;

namespace Typewriter.Tests.Extensions.Support
{
    [Route("api/[controller]")]
    public class RouteControllerWithNullableParts
    {
        [HttpGet("{name}?filter={filter}")]
        public string? RouteInHttpAttributeWithNullableAndNonNullableString(string name, string? filter)
        {
            return $"{name}{filter}";
        }
    }
}