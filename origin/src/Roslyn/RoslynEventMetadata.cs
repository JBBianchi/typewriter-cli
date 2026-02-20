using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynEventMetadata : IEventMetadata
    {
        private readonly IEventSymbol _symbol;

        public RoslynEventMetadata(IEventSymbol symbol, Settings settings)
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

        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_symbol.Type, Settings);

        public static IEnumerable<IEventMetadata> FromEventSymbols(IEnumerable<IEventSymbol> symbols, Settings settings)
        {
            return symbols.Where(s => s.DeclaredAccessibility == Accessibility.Public && !s.IsStatic)
                .Select(s => new RoslynEventMetadata(s, settings));
        }
    }
}
