using ApiContractGenerator.Model.TypeReferences;
using ApiContractGenerator.Source;

namespace ApiContractGenerator.Model
{
    public interface IMetadataEnum : IMetadataType
    {
        MetadataTypeReference UnderlyingType { get; }
    }
}
