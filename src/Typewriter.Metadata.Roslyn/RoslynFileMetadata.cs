using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Typewriter.Configuration;
using Typewriter.Metadata;

namespace Typewriter.Metadata.Roslyn
{
    /// <summary>
    /// Roslyn-based implementation of <see cref="IFileMetadata"/> that extracts
    /// type-level metadata from a single C# document using the Roslyn semantic model.
    /// </summary>
    public class RoslynFileMetadata : IFileMetadata
    {
        private readonly Action<string[]> _requestRender;
        private readonly Document _document;
        private readonly SyntaxNode _root;
        private readonly SemanticModel _semanticModel;

        /// <summary>
        /// Initializes a new instance for a given Roslyn <see cref="Document"/> and its parent <see cref="Compilation"/>.
        /// </summary>
        /// <param name="document">The Roslyn document representing a C# source file.</param>
        /// <param name="compilation">The compilation that contains the document's syntax tree.</param>
        /// <param name="settings">Template settings controlling rendering behavior.</param>
        /// <param name="requestRender">
        /// Callback invoked when partial-rendering mode detects that a symbol's primary location
        /// is in a different file, requesting that file to be re-rendered.
        /// </param>
        public RoslynFileMetadata(Document document, Compilation compilation, Settings settings, Action<string[]> requestRender)
        {
            _document = document;
            _requestRender = requestRender;
            Settings = settings;

            _root = document.GetSyntaxRootAsync().GetAwaiter().GetResult();
            _semanticModel = compilation.GetSemanticModel(_root.SyntaxTree);
        }

        /// <summary>
        /// Initializes a minimal instance used by metadata wrapper classes for
        /// partial-rendering mode filtering without a backing document.
        /// </summary>
        internal RoslynFileMetadata(Settings settings, string fullName)
        {
            Settings = settings;
            _fullName = fullName;
        }

        private readonly string _fullName;

        /// <inheritdoc />
        public Settings Settings { get; }

        /// <inheritdoc />
        public string Name => _document?.Name;

        /// <inheritdoc />
        public string FullName => _document?.FilePath ?? _fullName;

        /// <inheritdoc />
        public IEnumerable<IClassMetadata> Classes => RoslynClassMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<ClassDeclarationSyntax>(), this, Settings);

        /// <inheritdoc />
        public IEnumerable<IDelegateMetadata> Delegates => RoslynDelegateMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<DelegateDeclarationSyntax>(), Settings);

        /// <inheritdoc />
        public IEnumerable<IEnumMetadata> Enums => RoslynEnumMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<EnumDeclarationSyntax>(), Settings);

        /// <inheritdoc />
        public IEnumerable<IInterfaceMetadata> Interfaces => RoslynInterfaceMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<InterfaceDeclarationSyntax>(), this, Settings);

        /// <inheritdoc />
        public IEnumerable<IRecordMetadata> Records => RoslynRecordMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<RecordDeclarationSyntax>(), this, Settings);

        private IEnumerable<INamedTypeSymbol> GetNamespaceChildNodes<T>()
            where T : SyntaxNode
        {
            if (_root == null || _semanticModel == null)
            {
                return Enumerable.Empty<INamedTypeSymbol>();
            }

#pragma warning disable RS1039 // This call to 'SemanticModel.GetDeclaredSymbol()' will always return 'null'
            var symbols = _root.ChildNodes().OfType<T>().Concat(
                _root.ChildNodes().OfType<NamespaceDeclarationSyntax>().SelectMany(n => n.ChildNodes().OfType<T>())).Concat(
                    _root.ChildNodes().OfType<FileScopedNamespaceDeclarationSyntax>().SelectMany(n => n.ChildNodes().OfType<T>()))
                .Select(c => _semanticModel.GetDeclaredSymbol(c) as INamedTypeSymbol);
#pragma warning restore RS1039 // This call to 'SemanticModel.GetDeclaredSymbol()' will always return 'null'

            if (Settings.PartialRenderingMode == PartialRenderingMode.Combined)
            {
                return symbols.Where(s =>
                {
                    var locationToRender = s?.Locations.Select(l => l.SourceTree?.FilePath)
                        .OrderBy(f => f, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
                    if (string.Equals(locationToRender, FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        if (locationToRender != null)
                        {
                            _requestRender?.Invoke(new[] { locationToRender });
                        }

                        return false;
                    }
                }).ToList();
            }

            return symbols;
        }
    }
}
