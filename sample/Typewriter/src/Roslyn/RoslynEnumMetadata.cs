using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynEnumMetadata : IEnumMetadata
    {
        private readonly INamedTypeSymbol _symbol;

        public RoslynEnumMetadata(INamedTypeSymbol symbol, Settings settings)
        {
            _symbol = symbol;
            Settings = settings;
        }

        public Settings Settings { get; }

        public string DocComment => _symbol.GetDocumentationCommentXml();

        public string Name => _symbol.Name;

        public string FullName => _symbol.ToDisplayString();

        public string AssemblyName => _symbol.ContainingAssembly?.Name;

        public string Namespace => _symbol.GetNamespace();

        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_symbol, Settings);

        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes(), Settings);

        public IClassMetadata ContainingClass => RoslynClassMetadata.FromNamedTypeSymbol(_symbol.ContainingType, Settings);

        public IEnumerable<IEnumValueMetadata> Values => RoslynEnumValueMetadata.FromFieldSymbols(_symbol.GetMembers().OfType<IFieldSymbol>(), Settings);

        internal static IEnumerable<IEnumMetadata> FromNamedTypeSymbols(IEnumerable<INamedTypeSymbol> symbols, Settings settings)
        {
            return symbols.Where(s => s.DeclaredAccessibility == Accessibility.Public).Select(s => new RoslynEnumMetadata(s, settings));
        }
    }
}