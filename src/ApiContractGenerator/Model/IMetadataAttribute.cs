using System.Collections.Generic;
using ApiContractGenerator.Model.AttributeValues;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataAttribute
    {
        MetadataTypeReference AttributeType { get; }
        IReadOnlyList<MetadataAttributeValue> FixedArguments { get; }
        IReadOnlyList<MetadataAttributeNamedArgument> NamedArguments { get; }
    }
}
