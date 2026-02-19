using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynMethodMetadata : IMethodMetadata
    {
        private readonly IMethodSymbol _symbol;

        public RoslynMethodMetadata(IMethodSymbol symbol, Settings settings)
        {
            _symbol = symbol;
            Settings = settings;
        }

        public Settings Settings { get; }

        public string DocComment => _symbol.GetDocumentationCommentXml();

        public string Name => _symbol.Name;

        public string FullName => _symbol.GetFullName();

        public string AssemblyName => _symbol.ContainingAssembly?.Name;

        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes(), Settings);

        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_symbol.ReturnType, Settings);

        public bool IsAbstract => _symbol.IsAbstract;

        public bool IsGeneric => _symbol.IsGenericMethod;

        public IEnumerable<ITypeParameterMetadata> TypeParameters => RoslynTypeParameterMetadata.FromTypeParameterSymbols(_symbol.TypeParameters);

        public IEnumerable<IParameterMetadata> Parameters => RoslynParameterMetadata.FromParameterSymbols(_symbol.Parameters, Settings);

        public static IEnumerable<IMethodMetadata> FromMethodSymbols(IEnumerable<IMethodSymbol> symbols, Settings settings)
        {
            return symbols
                .Where(
                    s => s.DeclaredAccessibility == Accessibility.Public && s.MethodKind == MethodKind.Ordinary &&
                         !s.IsStatic).Select(p => new RoslynMethodMetadata(p, settings));
        }
    }
}