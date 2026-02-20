using System;
using System.Linq;
using Typewriter.CodeModel;

namespace Typewriter.Extensions.WebApi
{
    /// <summary>
    /// Extension methods for extracting Web API parameters.
    /// </summary>
    public static class RequestDataExtensions
    {
        /// <summary>
        /// Creates an object literal containing the parameters that should be sent in the request body of a Web API request.
        /// If no parameters are required the literal "null" is returned.
        /// </summary>
        /// <param name="method"><see cref="Method"/>.</param>
        public static string RequestData(this Method method)
        {
            return RequestData(method, UrlExtensions.DefaultRoute);
        }

        /// <summary>
        /// Creates an object literal containing the parameters that should be sent in the request body of a Web API request.
        /// If no parameters are required the literal "null" is returned.
        /// </summary>
        /// <param name="method"><see cref="Method"/>.</param>
        /// <param name="route">Route.</param>
        public static string RequestData(this Method method, string route)
        {
            var url = method.Url(route);

            // CancellationToken will never be send from TypeScript, filter them out before generating RequestData
            var dataParameters = method.Parameters
                .Where(x => !x.Type.Name.Equals("CancellationToken", StringComparison.OrdinalIgnoreCase))
                .Where(p => !url.Contains($"${{{UrlExtensions.GetParameterValue(method, p.Name)}}}")).ToList();

            if (dataParameters.Count == 1)
            {
                return dataParameters[0].Name;
            }

            if (dataParameters.Count > 1)
            {
                return $"{{ {string.Join(", ", dataParameters.Select(p => $"{p.Name}: {p.Name}"))} }}";
            }

            return "null";
        }
    }
}
