using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynInterfaceMetadata : IInterfaceMetadata
    {
        private readonly INamedTypeSymbol _symbol;
        private readonly RoslynFileMetadata _file;
        private IReadOnlyCollection<ISymbol> _members;

        public RoslynInterfaceMetadata(INamedTypeSymbol symbol, RoslynFileMetadata file, Settings settings)
        {
            _symbol = symbol;
            _file = file;
            Settings = settings;
        }

        public Settings Settings { get; }

        public string DocComment => _symbol.GetDocumentationCommentXml();

        public string Name => _symbol.Name;

        public string FullName => _symbol.ToDisplayString();

        public string AssemblyName => _symbol.ContainingAssembly?.Name;

        public bool IsGeneric => _symbol.TypeParameters.Any();

        public string Namespace => _symbol.GetNamespace();

        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_symbol, Settings);

        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes(), Settings);

        public IClassMetadata ContainingClass => RoslynClassMetadata.FromNamedTypeSymbol(_symbol.ContainingType, Settings);

        public IEnumerable<IEventMetadata> Events => RoslynEventMetadata.FromEventSymbols(Members.OfType<IEventSymbol>(), Settings);

        public IEnumerable<IInterfaceMetadata> Interfaces => FromNamedTypeSymbols(_symbol.Interfaces, null, Settings);

        public IEnumerable<IMethodMetadata> Methods => RoslynMethodMetadata.FromMethodSymbols(Members.OfType<IMethodSymbol>(), Settings);

        public IEnumerable<IPropertyMetadata> Properties => RoslynPropertyMetadata.FromPropertySymbol(Members.OfType<IPropertySymbol>(), Settings);

        public IEnumerable<ITypeParameterMetadata> TypeParameters => RoslynTypeParameterMetadata.FromTypeParameterSymbols(_symbol.TypeParameters);

        public IEnumerable<ITypeMetadata> TypeArguments => RoslynTypeMetadata.FromTypeSymbols(_symbol.TypeArguments, Settings);

        private IReadOnlyCollection<ISymbol> Members
        {
            get
            {
                if (_members == null)
                {
                    if (_file?.Settings.PartialRenderingMode == PartialRenderingMode.Partial && _symbol.Locations.Length > 1)
                    {
                        _members = _symbol.GetMembers().Where(m => m.Locations.Any(l => string.Equals(l.SourceTree.FilePath, _file.FullName, StringComparison.OrdinalIgnoreCase))).ToArray();
                    }
                    else
                    {
                        _members = _symbol.GetMembers();
                    }
                }

                return _members;
            }
        }

        public static IEnumerable<IInterfaceMetadata> FromNamedTypeSymbols(IEnumerable<INamedTypeSymbol> symbols, RoslynFileMetadata file, Settings settings)
        {
            return symbols.Where(s => s.DeclaredAccessibility == Accessibility.Public).Select(s => new RoslynInterfaceMetadata(s, file, settings));
        }
    }
}