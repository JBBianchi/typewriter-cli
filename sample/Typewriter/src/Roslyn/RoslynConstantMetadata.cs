using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynConstantMetadata : RoslynFieldMetadata, IConstantMetadata
    {
        private readonly IFieldSymbol _symbol;

        private RoslynConstantMetadata(IFieldSymbol symbol, Settings settings)
            : base(symbol, settings)
        {
            _symbol = symbol;
        }

        public string Value => $"{_symbol.ConstantValue}";

        // ReSharper disable once ArrangeModifiersOrder
        public static new IEnumerable<IConstantMetadata> FromFieldSymbols(IEnumerable<IFieldSymbol> symbols, Settings settings)
        {
            return symbols.Where(s => s.DeclaredAccessibility == Accessibility.Public && s.IsConst).Select(s => new RoslynConstantMetadata(s, settings));
        }
    }
}