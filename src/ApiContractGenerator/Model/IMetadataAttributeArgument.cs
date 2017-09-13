using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataAttributeArgument
    {
        object Value { get; }
        MetadataTypeReference ValueType { get; }
    }
}