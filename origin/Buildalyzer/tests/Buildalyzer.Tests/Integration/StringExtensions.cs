using System;
using System.Collections.Generic;
using System.Text;

namespace Buildalyzer.Tests.Integration
{
#if NET472_OR_GREATER
    public static class StringExtensions
    {
        public static IEnumerable<string> TakeLast(this string[] values, int count)
        {
            if (values == null || values.Length == 0)
            {
                yield break;
            }

            for (int i = Math.Min(count, values.Length); i > 0; i--)
            {
                yield return values[values.Length - i];
            }
        }
    }
#endif
}
