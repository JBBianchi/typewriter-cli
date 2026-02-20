using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynClassMetadata : IClassMetadata
    {
        private readonly INamedTypeSymbol _symbol;
        private readonly RoslynFileMetadata _file;
        private IReadOnlyCollection<ISymbol> _members;

        private RoslynClassMetadata(INamedTypeSymbol symbol, RoslynFileMetadata file, Settings settings)
        {
            _symbol = symbol;
            _file = file;
            Settings = settings;
        }

        public Settings Settings { get; }

        public string DocComment => _symbol.GetDocumentationCommentXml();

        public string Name => _symbol.Name;

        public string AssemblyName => _symbol.ContainingAssembly?.Name;

        public string FullName => _symbol.ToDisplayString();

        public bool IsAbstract => _symbol.IsAbstract;

        public bool IsGeneric => _symbol.TypeParameters.Any();

        public bool IsStatic => _symbol.IsStatic;

        public string Namespace => _symbol.GetNamespace();

        public ITypeMetadata Type => RoslynTypeMetadata.FromTypeSymbol(_symbol, Settings);

        public IEnumerable<IAttributeMetadata> Attributes => RoslynAttributeMetadata.FromAttributeData(_symbol.GetAttributes(), Settings);

        public IClassMetadata BaseClass => FromNamedTypeSymbol(_symbol.BaseType, Settings);

        public IClassMetadata ContainingClass => FromNamedTypeSymbol(_symbol.ContainingType, Settings);

        public IEnumerable<IConstantMetadata> Constants => RoslynConstantMetadata.FromFieldSymbols(Members.OfType<IFieldSymbol>(), Settings);

        public IEnumerable<IDelegateMetadata> Delegates => RoslynDelegateMetadata.FromNamedTypeSymbols(Members.OfType<INamedTypeSymbol>().Where(s => s.TypeKind == TypeKind.Delegate), Settings);

        public IEnumerable<IEventMetadata> Events => RoslynEventMetadata.FromEventSymbols(Members.OfType<IEventSymbol>(), Settings);

        public IEnumerable<IFieldMetadata> Fields => RoslynFieldMetadata.FromFieldSymbols(Members.OfType<IFieldSymbol>(), Settings);

        public IEnumerable<IInterfaceMetadata> Interfaces => RoslynInterfaceMetadata.FromNamedTypeSymbols(_symbol.Interfaces, null, Settings);

        public IEnumerable<IMethodMetadata> Methods => RoslynMethodMetadata.FromMethodSymbols(Members.OfType<IMethodSymbol>(), Settings);

        public IEnumerable<IPropertyMetadata> Properties => RoslynPropertyMetadata.FromPropertySymbol(Members.OfType<IPropertySymbol>(), Settings);

        public IEnumerable<IStaticReadOnlyFieldMetadata> StaticReadOnlyFields => RoslynStaticReadOnlyFieldMetadata.FromFieldSymbols(Members.OfType<IFieldSymbol>(), Settings);

        public IEnumerable<ITypeParameterMetadata> TypeParameters => RoslynTypeParameterMetadata.FromTypeParameterSymbols(_symbol.TypeParameters);

        public IEnumerable<ITypeMetadata> TypeArguments => RoslynTypeMetadata.FromTypeSymbols(_symbol.TypeArguments, Settings);

        public IEnumerable<IClassMetadata> NestedClasses => FromNamedTypeSymbols(Members.OfType<INamedTypeSymbol>().Where(s => s.TypeKind == TypeKind.Class), null, Settings);

        public IEnumerable<IEnumMetadata> NestedEnums => RoslynEnumMetadata.FromNamedTypeSymbols(Members.OfType<INamedTypeSymbol>().Where(s => s.TypeKind == TypeKind.Enum), Settings);

        public IEnumerable<IInterfaceMetadata> NestedInterfaces => RoslynInterfaceMetadata.FromNamedTypeSymbols(Members.OfType<INamedTypeSymbol>().Where(s => s.TypeKind == TypeKind.Interface), null, Settings);

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

        internal static IClassMetadata FromNamedTypeSymbol(INamedTypeSymbol symbol, Settings settings)
        {
            if (symbol == null)
            {
                return null;
            }

            if (symbol.DeclaredAccessibility != Accessibility.Public ||
                symbol.ToDisplayString().Equals("object", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new RoslynClassMetadata(symbol, null, settings);
        }

        internal static IEnumerable<IClassMetadata> FromNamedTypeSymbols(IEnumerable<INamedTypeSymbol> symbols, RoslynFileMetadata file, Settings settings)
        {
            return symbols
                .Where(
                    s => s.DeclaredAccessibility == Accessibility.Public &&
                         !s.ToDisplayString().Equals("object", StringComparison.OrdinalIgnoreCase))
                .Select(s => new RoslynClassMetadata(s, file, settings));
        }
    }
}
