using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynEnumValueMetadata : IEnumValueMetadata
    {
        private static readonly Int64Converter _converter = new Int64Converter();

        private readonly IFieldSymbol _symbol;

        private RoslynEnumValueMetadata(IFieldSymbol symbol, Settings settings)
        {
            _symbol = symbol;
            Settings = settings;
        }

        public Settings Settings { get; }

        public string DocComment => _symbol.GetDocumentationCommentXml();

        public string Name => _symbol.Name;

        public string FullName => _symbol.ToDisplayString();

        public string AssemblyName => _symbol.ContainingAssembly?.Name;

        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes(), Settings);

        public long Value => (long?)_converter.ConvertFromString(_symbol.ConstantValue.ToString().Trim('\'')) ?? -1;

        internal static IEnumerable<IEnumValueMetadata> FromFieldSymbols(IEnumerable<IFieldSymbol> symbols, Settings settings)
        {
            return symbols.Select(s => new RoslynEnumValueMetadata(s, settings));
        }
    }
}