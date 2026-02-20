using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Typewriter.Configuration;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynStaticReadOnlyFieldMetadata : RoslynFieldMetadata, IStaticReadOnlyFieldMetadata
    {
        private readonly IFieldSymbol _symbol;

        private RoslynStaticReadOnlyFieldMetadata(IFieldSymbol symbol, Settings settings)
            : base(symbol, settings)
        {
            _symbol = symbol;
        }

        public string Value
        {
            get
            {
                var syntaxRef = _symbol.DeclaringSyntaxReferences.FirstOrDefault();
                var syntax = syntaxRef?.GetSyntax() as VariableDeclaratorSyntax;
                var initializer = syntax?.Initializer?.Value;
                if (initializer is LiteralExpressionSyntax literal)
                {
                    if (literal.IsKind(SyntaxKind.NullLiteralExpression))
                    {
                        return string.Empty;
                    }

                    return literal.Token.ValueText;
                }

                if (initializer == null)
                {
                    return
                        "The field might be initialized elsewhere (like in a static constructor) " +
                        "or not initialized at all. Typewriter is not able to process such static readonly fields";
                }

                return null;
            }
        }

        // ReSharper disable once ArrangeModifiersOrder
        public static new IEnumerable<IStaticReadOnlyFieldMetadata> FromFieldSymbols(IEnumerable<IFieldSymbol> symbols, Settings settings)
        {
            return symbols.Where(s => s.DeclaredAccessibility == Accessibility.Public && s.IsStatic && s.IsReadOnly)
                .Select(s => new RoslynStaticReadOnlyFieldMetadata(s, settings));
        }
    }
}