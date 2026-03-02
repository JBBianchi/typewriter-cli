namespace Typewriter.Metadata;

public interface INamedItem
{
    string Name { get; }

    string FullName { get; }

    string AssemblyName { get; }
}
