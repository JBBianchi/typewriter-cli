using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynAttributeArgumentMetadata : IAttributeArgumentMetadata
    {
        private readonly TypedConstant _typeConstant;

        public RoslynAttributeArgumentMetadata(TypedConstant typeConstant, Settings settings)
        {
            _typeConstant = typeConstant;
            Settings = settings;
        }

        public Settings Settings { get; }

        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_typeConstant.Type, Settings);

        public ITypeMetadata TypeValue => _typeConstant.Kind == TypedConstantKind.Type
            ? RoslynTypeMetadata.FromTypeSymbol((INamedTypeSymbol)_typeConstant.Value, Settings)
            : null;

        public object GetValue() => _typeConstant.Kind == TypedConstantKind.Array
            ? _typeConstant.Values.Select(prop => prop.Value).ToArray()
            : _typeConstant.Value;
    }
}