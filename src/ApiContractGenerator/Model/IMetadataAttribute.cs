using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataAttribute
    {
        MetadataTypeReference AttributeType { get; }
        IReadOnlyList<IMetadataAttributeArgument> FixedArguments { get; }
        IReadOnlyList<IMetadataAttributeNamedArgument> NamedArguments { get; }
    }
}
