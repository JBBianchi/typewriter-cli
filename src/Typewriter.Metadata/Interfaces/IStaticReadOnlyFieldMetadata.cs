namespace Typewriter.Metadata;

public interface IStaticReadOnlyFieldMetadata : IFieldMetadata
{
    string Value { get; }
}
