using System;

namespace Typewriter.Tests.Support
{
    internal class RoutePrefixAttribute : Attribute
    {
        private string _v;

        public RoutePrefixAttribute(string v)
        {
            this._v = v;
        }
    }
}