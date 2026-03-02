using System.Collections.Generic;

namespace Typewriter.Metadata;

public interface IEventMetadata : INamedItem
{
    string DocComment { get; }

    IEnumerable<IAttributeMetadata> Attributes { get; }

    ITypeMetadata Type { get; }
}
