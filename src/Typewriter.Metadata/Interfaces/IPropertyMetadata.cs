namespace Typewriter.Metadata;

public interface IPropertyMetadata : IFieldMetadata
{
    bool IsAbstract { get; }

    bool IsVirtual { get; }

    bool HasGetter { get; }

    bool HasSetter { get; }
}
