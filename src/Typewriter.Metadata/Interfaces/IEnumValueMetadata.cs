using System.Collections.Generic;

namespace Typewriter.Metadata;

public interface IEnumValueMetadata : INamedItem
{
    string DocComment { get; }

    IEnumerable<IAttributeMetadata> Attributes { get; }

    long Value { get; }
}
