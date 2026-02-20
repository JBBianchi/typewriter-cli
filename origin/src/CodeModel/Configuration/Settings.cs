using System;
using Typewriter.CodeModel;
using Typewriter.VisualStudio;

namespace Typewriter.Configuration
{
    /// <summary>
    /// Provides settings for Typewriter Templates.
    /// </summary>
    public abstract class Settings
    {
        /// <summary>
        /// Gets or sets the file extension for output files.
        /// </summary>
        public string OutputExtension { get; set; } = ".ts";

        /// <summary>
        /// Gets or sets a filename factory for the template.
        /// The factory is called for each rendered file to determine the output filename (including extension).
        /// Example: file => file.Classes.First().FullName + ".ts".
        /// </summary>
        public Func<File, string> OutputFilenameFactory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how partial classes and interfaces are rendered.
        /// </summary>
        public PartialRenderingMode PartialRenderingMode { get; set; } = PartialRenderingMode.Partial;

        /// <summary>
        /// Gets full solution name from DTE.
        /// </summary>
        public abstract string SolutionFullName { get; }

        /// <summary>
        /// Gets or sets output directory to which generated files needs to be saved.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets value indicating if generated files should not be added to project.
        /// </summary>
        public bool SkipAddingGeneratedFilesToProject { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is single file mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is single file mode; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsSingleFileMode { get; }

        /// <summary>
        /// Gets the name of the single file.
        /// </summary>
        /// <value>
        /// The name of the single file.
        /// </value>
        public abstract string SingleFileName { get; }

        /// <summary>
        /// String literal character.
        /// Default ".
        /// </summary>
        public abstract char StringLiteralCharacter { get; }

        /// <summary>
        /// Strict null generation.
        /// In C# whenever some property is defined as nullable in
        /// default it is generated in TypeScript with `type | null` signature.
        /// This is in sync with https://github.com/microsoft/TypeScript/pull/7140
        ///
        /// When false only `type` is generated.
        /// </summary>
        public abstract bool StrictNullGeneration { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate UTF8 BOM in output files.
        /// </summary>
        public abstract bool Utf8BomGeneration { get; }

        /// <summary>
        /// Gets full path to the template file.
        /// </summary>
        public abstract string TemplatePath { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public abstract ILog Log { get; }

        /// <summary>
        /// Includes files in the specified project when rendering the template.
        /// </summary>
        /// <param name="projectName">Project name.</param>
        public abstract Settings IncludeProject(string projectName);

        /// <summary>
        /// Single File Mode - Template will be parsed in one file.
        /// Note: SingleFileMode ignores the filename factory(!).
        /// </summary>
        /// <param name="singleFilename">The single filename.</param>
        /// <returns><see cref="Settings"/>.</returns>
        public abstract Settings SingleFileMode(string singleFilename);

        /// <summary>
        /// Includes files in the current project when rendering the template.
        /// </summary>
        public abstract Settings IncludeCurrentProject();

        /// <summary>
        /// Includes files in all referenced projects when rendering the template.
        /// </summary>
        public abstract Settings IncludeReferencedProjects();

        /// <summary>
        /// Includes files in all projects in the solution when rendering the template.
        /// Note: Including all projects can have a large impact on performance in large solutions.
        /// </summary>
        public abstract Settings IncludeAllProjects();

        /// <summary>
        /// Use given string literal character in TypeScript.
        /// </summary>
        /// <param name="ch">Character used as string literal start finish mark.</param>
        public abstract Settings UseStringLiteralCharacter(char ch);

        /// <summary>
        /// Disable strict null generation.
        /// In C# whenever some property is defined as nullable in
        /// default it is generated in TypeScript with `type | null` signature.
        /// This is in sync with https://github.com/microsoft/TypeScript/pull/7140
        ///
        /// Using this method falls back to previous method of generating type name when only `type` is generated.
        /// </summary>
        /// <returns><see cref="Settings"/> implementation.</returns>
        public abstract Settings DisableStrictNullGeneration();

        /// <summary>
        /// Disable UTF8 BOM generation in generated files.
        /// </summary>
        /// <returns><see cref="Settings"/> implementation.</returns>
        public abstract Settings DisableUtf8BomGeneration();
    }
}
