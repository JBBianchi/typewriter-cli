using System.Collections.Generic;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Typewriter.Configuration;
using Typewriter.VisualStudio;

namespace Typewriter.CodeModel.Configuration
{
    public class SettingsImpl : Settings
    {
        private readonly ProjectItem _projectItem;

        private bool _isSingleFileMode;
        private string _singleFileName;
        private char _stringLiteralCharacter = '"';
        private List<string> _includedProjects;
        private bool _strictNullGeneration = true;
        private bool _utf8BomGeneration = true;
        private string _templatePath;

        public SettingsImpl(
            ILog log,
            ProjectItem projectItem,
            string templatePath)
        {
            Log = log;
            _projectItem = projectItem;
            _templatePath = templatePath;
        }

        public ICollection<string> IncludedProjects
        {
            get
            {
                if (_includedProjects == null)
                {
                    IncludeCurrentProject();
                    IncludeReferencedProjects();
                }

                return _includedProjects;
            }
        }

        public override string SolutionFullName
        {
            get
            {
                var fullName = string.Empty;
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    fullName = _projectItem.DTE.Solution.FullName;
                });
                return fullName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is single file mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is single file mode; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSingleFileMode => _isSingleFileMode;

        /// <summary>
        /// Gets the name of the single file.
        /// </summary>
        /// <value>
        /// The name of the single file.
        /// </value>
        public override string SingleFileName => _singleFileName;

        /// <summary>
        /// String literal character.
        /// Default ".
        /// </summary>
        public override char StringLiteralCharacter => _stringLiteralCharacter;

        /// <summary>
        /// Strict null generation.
        /// In C# whenever some property is defined as nullable in
        /// default it is generated in TypeScript with `type | null` signature.
        /// This is in sync with https://github.com/microsoft/TypeScript/pull/7140
        ///
        /// When false only `type` is generated.
        /// </summary>
        public override bool StrictNullGeneration => _strictNullGeneration;

        /// <summary>
        /// Gets or sets a value indicating whether to generate UTF8 BOM in output files.
        /// </summary>
        public override bool Utf8BomGeneration => _utf8BomGeneration;

        /// <summary>
        /// Gets full path to the template file.
        /// </summary>
        public override string TemplatePath => _templatePath;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public override ILog Log { get; }

        public override Settings IncludeProject(string projectName)
        {
            if (_includedProjects == null)
            {
                _includedProjects = new List<string>();
            }

            ProjectHelpers.AddProject(_projectItem, _includedProjects, projectName);
            return this;
        }

        public override Settings SingleFileMode(string singleFilename)
        {
            _isSingleFileMode = true;
            _singleFileName = singleFilename;

            return this;
        }

        public override Settings IncludeReferencedProjects()
        {
            if (_includedProjects == null)
            {
                _includedProjects = new List<string>();
            }

            ProjectHelpers.AddReferencedProjects(_includedProjects, _projectItem);
            return this;
        }

        public override Settings IncludeCurrentProject()
        {
            if (_includedProjects == null)
            {
                _includedProjects = new List<string>();
            }

            ProjectHelpers.AddCurrentProject(_includedProjects, _projectItem);
            return this;
        }

        public override Settings IncludeAllProjects()
        {
            if (_includedProjects == null)
            {
                _includedProjects = new List<string>();
            }

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                ProjectHelpers.AddAllProjects(_projectItem.DTE, _includedProjects);
            });
            return this;
        }

        /// <summary>
        /// Use given string literal character in TypeScript.
        /// </summary>
        /// <param name="ch">Character used as string literal start finish mark.</param>
        public override Settings UseStringLiteralCharacter(char ch)
        {
            _stringLiteralCharacter = ch;
            return this;
        }

        /// <summary>
        /// Disable strict null generation.
        /// In C# whenever some property is defined as nullable in
        /// default it is generated in TypeScript with `type | null` signature.
        /// This is in sync with https://github.com/microsoft/TypeScript/pull/7140
        ///
        /// Using this method falls back to previous method of generating type name when only `type` is generated.
        /// </summary>
        /// <returns><see cref="Settings"/> implementation.</returns>
        public override Settings DisableStrictNullGeneration()
        {
            _strictNullGeneration = false;
            return this;
        }

        /// <summary>
        /// Disable UTF8 BOM generation in generated files.
        /// </summary>
        /// <returns><see cref="Settings"/> implementation.</returns>
        public override Settings DisableUtf8BomGeneration()
        {
            _utf8BomGeneration = false;
            return this;
        }
    }
}
