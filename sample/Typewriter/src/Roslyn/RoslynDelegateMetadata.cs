using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynDelegateMetadata : IDelegateMetadata
    {
        private readonly INamedTypeSymbol _symbol;
        private readonly IMethodSymbol _methodSymbol;

        public RoslynDelegateMetadata(INamedTypeSymbol symbol, Settings settings)
        {
            _symbol = symbol;
            _methodSymbol = symbol.DelegateInvokeMethod;
            Settings = settings;
        }

        public Settings Settings { get; }

        public string DocComment => _symbol.GetDocumentationCommentXml();

        public string Name => _symbol.Name;

        public string FullName => _symbol.GetFullName();

        public string AssemblyName => _symbol.ContainingAssembly?.Name;

        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes(), Settings);

        public ITypeMetadata Type => _methodSymbol == null ? null : RoslynTypeMetadata.FromTypeSymbol(_methodSymbol.ReturnType, Settings);

        public bool IsAbstract => false;

        public bool IsGeneric => _symbol.TypeParameters.Any();

        public IEnumerable<ITypeParameterMetadata> TypeParameters => RoslynTypeParameterMetadata.FromTypeParameterSymbols(_symbol.TypeParameters);

        public IEnumerable<IParameterMetadata> Parameters => _methodSymbol == null ? System.Array.Empty<IParameterMetadata>() : RoslynParameterMetadata.FromParameterSymbols(_methodSymbol.Parameters, Settings);

        public static IEnumerable<IDelegateMetadata> FromNamedTypeSymbols(IEnumerable<INamedTypeSymbol> symbols, Settings settings)
        {
            return symbols.Where(s => s.DeclaredAccessibility == Accessibility.Public).Select(s => new RoslynDelegateMetadata(s, settings));
        }
    }
}