namespace Typewriter.Metadata.Interfaces
{
    public interface IStaticReadOnlyFieldMetadata : IFieldMetadata
    {
        string Value { get; }
    }
}