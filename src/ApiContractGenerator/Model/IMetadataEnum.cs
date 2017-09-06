using ApiContractGenerator.Model.TypeReferences;
using ApiContractGenerator.Source;

namespace ApiContractGenerator.Model
{
    public interface IMetadataEnum : IMetadataType, IMetadataSource
    {
        MetadataTypeReference UnderlyingType { get; }
    }
}
