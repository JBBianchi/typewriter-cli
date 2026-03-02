namespace Typewriter.Metadata;

public interface IConstantMetadata : IFieldMetadata
{
    string Value { get; }
}
