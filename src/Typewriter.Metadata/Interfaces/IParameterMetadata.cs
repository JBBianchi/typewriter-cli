using System.Collections.Generic;

namespace Typewriter.Metadata;

public interface IParameterMetadata : INamedItem
{
    bool HasDefaultValue { get; }

    string DefaultValue { get; }

    IEnumerable<IAttributeMetadata> Attributes { get; }

    ITypeMetadata Type { get; }
}
