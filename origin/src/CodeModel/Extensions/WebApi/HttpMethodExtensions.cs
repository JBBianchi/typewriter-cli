using System;
using System.Linq;
using Typewriter.CodeModel;

namespace Typewriter.Extensions.WebApi
{
    /// <summary>
    /// Extension methods for extracting Web API http method.
    /// </summary>
    public static class HttpMethodExtensions
    {
        private static readonly string[] _validVerbs = { "get", "post", "put", "delete", "patch", "head", "options" };

        /// <summary>
        /// Returns the http method from a Web API action.
        /// The http method is extracted from Http* or AcceptVerbs attribute or by naming convention if no attributes are specified.
        /// </summary>
        /// <param name="method"><see cref="Method"/>.</param>
        public static string HttpMethod(this Method method)
        {
            // HTTP method can be specified with an attribute: AcceptVerbs, HttpDelete, HttpGet, HttpHead, HttpOptions, HttpPatch, HttpPost, or HttpPut.
            // Otherwise, if the name of the controller method starts with "Get", "Post", "Put", "Delete", "Head", "Options", or "Patch",
            // then by convention the action supports that HTTP method.
            // If none of the above, the method supports POST.
            var httpAttributes = method.Attributes.Where(a => a.Name.StartsWith("Http", StringComparison.OrdinalIgnoreCase));
            var acceptAttribute = method.Attributes.FirstOrDefault(a => a.Name.Equals("AcceptVerbs", StringComparison.OrdinalIgnoreCase));

            var verbs = httpAttributes.Select(a => a.Name.Remove(0, 4).ToLowerInvariant()).ToList();
            if (acceptAttribute != null)
            {
                verbs.AddRange(acceptAttribute.Value.Split(',').Select(v => v.Trim().Trim('"').ToLowerInvariant()));
            }

            // Prefer POST if multiple verbs are specified
            if (verbs.Contains("post", StringComparer.OrdinalIgnoreCase))
            {
                return "post";
            }

            if (verbs.Any())
            {
                return verbs[0];
            }

            var methodName = method.Name.ToLowerInvariant();
            return Array.Find(_validVerbs, v => methodName.StartsWith(v, StringComparison.OrdinalIgnoreCase)) ?? "post";
        }
    }
}
