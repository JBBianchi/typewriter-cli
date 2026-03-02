using System.Collections.Generic;

namespace Typewriter.Metadata;

public interface IAttributeMetadata : INamedItem
{
    string Value { get; }

    IEnumerable<IAttributeArgumentMetadata> Arguments { get; }

    ITypeMetadata Type { get; }
}
