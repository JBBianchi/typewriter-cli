using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Typewriter.Generation;

/// <summary>
/// Compiles template code from a <see cref="ShadowClass"/> into a loadable assembly type.
/// Adapted from upstream <c>Typewriter.Generation.Compiler</c> with VS coupling removed:
/// <list type="bullet">
///   <item><c>EnvDTE.ProjectItem</c> replaced with <c>string templateFilePath</c>.</item>
///   <item><c>Assembly.LoadFrom</c> replaced with <see cref="TemplateAssemblyLoadContext"/>.</item>
///   <item><c>ErrorList</c> / <c>Log</c> (VS OutputWindow) removed; diagnostics are surfaced
///         via the exception message on compilation failure.</item>
/// </list>
/// Roslyn <see cref="Microsoft.CodeAnalysis.CSharp.CSharpCompilation"/> logic is preserved
/// in <see cref="ShadowClass.Compile(string)"/>.
/// </summary>
internal static class Compiler
{
    private static readonly string TempDirectory = Path.Combine(Path.GetTempPath(), "Typewriter");

    /// <summary>
    /// Compiles the shadow class and returns the generated <c>__Typewriter.Template</c> type,
    /// loaded in an isolated <see cref="TemplateAssemblyLoadContext"/>.
    /// </summary>
    /// <param name="templateFilePath">Absolute path to the <c>.tst</c> template file.</param>
    /// <param name="shadowClass">The shadow class containing parsed template code.</param>
    /// <returns>The compiled template <see cref="Type"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when compilation produces errors.</exception>
    public static Type Compile(string templateFilePath, ShadowClass shadowClass)
    {
        if (!Directory.Exists(TempDirectory))
        {
            Directory.CreateDirectory(TempDirectory);
        }

        // Copy referenced assemblies to the temp directory so the isolated load context
        // can resolve them at runtime.
        foreach (var assembly in shadowClass.ReferencedAssemblies)
        {
            var asmSourcePath = assembly.Location;
            if (string.IsNullOrEmpty(asmSourcePath))
            {
                continue;
            }

            var asmDestPath = Path.Combine(TempDirectory, Path.GetFileName(asmSourcePath));

            var sourceAssemblyName = AssemblyName.GetAssemblyName(asmSourcePath);

            // Skip the copy if the destination already has the same version.
            if (File.Exists(asmDestPath))
            {
                var destAssemblyName = AssemblyName.GetAssemblyName(asmDestPath);
                if (sourceAssemblyName.Version is not null
                    && destAssemblyName.Version is not null
                    && sourceAssemblyName.Version.CompareTo(destAssemblyName.Version) == 0)
                {
                    continue;
                }
            }

            try
            {
                File.Copy(asmSourcePath, asmDestPath, overwrite: true);
            }
            catch (IOException)
            {
                // File may be in use by another template compilation; non-fatal.
            }
        }

        var fileName = Path.GetRandomFileName();
        var path = Path.Combine(TempDirectory, fileName);

        var result = shadowClass.Compile(path);

        var errors = result.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error
                     || d.Severity == DiagnosticSeverity.Warning);

        var hasErrors = false;
        var errorMessages = new List<string>();

        foreach (var diagnostic in errors)
        {
            var message = diagnostic.GetMessage();
            message = message.Replace("__Typewriter.", string.Empty);
            message = message.Replace("publicstatic", string.Empty);

            if (diagnostic.Severity == DiagnosticSeverity.Error || diagnostic.IsWarningAsError)
            {
                hasErrors = true;
                errorMessages.Add($"error {diagnostic.Id}: {message}");
            }
        }

        if (result.Success && !hasErrors)
        {
            var assemblyDir = Path.GetDirectoryName(path)!;
            var loadContext = new TemplateAssemblyLoadContext(assemblyDir);
            var assembly = loadContext.LoadFromAssemblyPath(path);
            var type = assembly.GetType("__Typewriter.Template");

            return type ?? throw new InvalidOperationException(
                $"Compiled assembly does not contain the expected type '__Typewriter.Template'. Template: {templateFilePath}");
        }

        throw new InvalidOperationException(
            $"Failed to compile template '{templateFilePath}'. Errors:{Environment.NewLine}"
            + string.Join(Environment.NewLine, errorMessages));
    }
}
