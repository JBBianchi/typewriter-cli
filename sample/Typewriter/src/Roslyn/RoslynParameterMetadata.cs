using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynParameterMetadata : IParameterMetadata
    {
        private readonly IParameterSymbol _symbol;

        private RoslynParameterMetadata(IParameterSymbol symbol, Settings settings)
        {
            _symbol = symbol;
            Settings = settings;
        }

        public Settings Settings { get; }

        public string Name => _symbol.Name;

        public string FullName => _symbol.ToDisplayString();

        public string AssemblyName => _symbol.ContainingAssembly?.Name;

        public bool HasDefaultValue => _symbol.HasExplicitDefaultValue;

        public string DefaultValue => GetDefaultValue();

        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes(), Settings);

        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_symbol.Type, Settings);

        public static IEnumerable<IParameterMetadata> FromParameterSymbols(IEnumerable<IParameterSymbol> symbols, Settings settings)
        {
            return symbols.Select(s => new RoslynParameterMetadata(s, settings));
        }

        private string GetDefaultValue()
        {
            if (!_symbol.HasExplicitDefaultValue)
            {
                return null;
            }

            if (_symbol.ExplicitDefaultValue == null)
            {
                return "null";
            }

            if (_symbol.ExplicitDefaultValue is string stringValue)
            {
                return $"\"{stringValue.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
            }

            if (_symbol.ExplicitDefaultValue is bool v)
            {
                return v ? "true" : "false";
            }

            return _symbol.ExplicitDefaultValue.ToString();
        }
    }
}