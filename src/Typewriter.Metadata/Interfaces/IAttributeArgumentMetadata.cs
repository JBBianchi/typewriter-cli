namespace Typewriter.Metadata;

public interface IAttributeArgumentMetadata
{
    ITypeMetadata Type { get; }

    ITypeMetadata TypeValue { get; }

    object GetValue();
}
