using System;

namespace Typewriter.Tests.CodeModel.Support
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Interface | AttributeTargets.Class)]
    public sealed class AttributeInfoAttribute
        : Attribute
    {
        public AttributeInfoAttribute()
        {
        }

        public AttributeInfoAttribute(string parameter)
        {
            Parameter = parameter;
        }

        public AttributeInfoAttribute(int parameter)
        {
        }

        public AttributeInfoAttribute(params string[] parameters)
        {
        }

        public AttributeInfoAttribute(int parameter, params string[] parameters)
        {
        }

        public AttributeInfoAttribute(Type parameter)
        {
        }

        public string Parameter { get; set; }
    }
}