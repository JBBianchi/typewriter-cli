using System;

namespace Typewriter.Tests.Support
{
    internal class RouteAttribute : Attribute
    {
        private string _v;

        public RouteAttribute(string v)
        {
            this._v = v;
        }

        public string Name { get; set; }
    }
}