using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Typewriter.Metadata.Roslyn
{
    public static class Extensions
    {
        public static string GetName(this ISymbol symbol)
        {
            return symbol is IArrayTypeSymbol array ? $"{array.ElementType}[]" : symbol.Name;
        }

        public static string GetFullName(this ISymbol symbol)
        {
            if (symbol is ITypeParameterSymbol)
            {
                return symbol.Name;
            }

            if (symbol is IDynamicTypeSymbol)
            {
                return symbol.Name;
            }

            var name = (symbol is INamedTypeSymbol type) ? GetFullTypeName(type) : symbol.Name;

            var namespaceSymbol = symbol.ContainingSymbol as INamespaceSymbol;
            if (namespaceSymbol?.IsGlobalNamespace == true)
            {
                return name;
            }

            if (symbol is IArrayTypeSymbol array)
            {
                return "System.Collections.Generic.ICollection<" + GetFullName(array.ElementType) + ">";
            }

            return GetFullName(symbol.ContainingSymbol) + "." + name;
        }

        public static string GetNamespace(this ISymbol symbol)
        {
            if (string.IsNullOrEmpty(symbol.ContainingNamespace?.Name))
            {
                return null;
            }

            var restOfResult = GetNamespace(symbol.ContainingNamespace);
            var result = symbol.ContainingNamespace.Name;

            if (restOfResult != null)
            {
                result = restOfResult + '.' + result;
            }

            return result;
        }

        public static string GetFullTypeName(this INamedTypeSymbol type)
        {
            var sb = new StringBuilder(type.Name);

            if (type.Name.Equals("Nullable", StringComparison.OrdinalIgnoreCase) &&
                type.ContainingNamespace.Name.Equals("System", StringComparison.OrdinalIgnoreCase) &&
                type.TypeArguments.First() is INamedTypeSymbol typeSymbol)
            {
                return GetFullTypeName(typeSymbol) + "?";
            }

            if (type.TypeArguments.Any())
            {
                sb.Append("<");
                sb.Append(string.Join(
                    ", ",
                    type.TypeArguments.Select(
                        t => !(t is INamedTypeSymbol typeSymbol2) ? t.Name : GetFullName(typeSymbol2))));
                sb.Append(">");
            }
            else if (type.TypeParameters.Any())
            {
                sb.Append("<");
                sb.Append(string.Join(", ", type.TypeParameters.Select(t => t.Name)));
                sb.Append(">");
            }

            return sb.ToString();
        }
    }
}
