using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataEnum : IMetadataType
    {
        MetadataTypeReference UnderlyingType { get; }
    }
}
