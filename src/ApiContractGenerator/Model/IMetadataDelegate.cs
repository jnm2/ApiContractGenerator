using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataDelegate : IMetadataType
    {
        MetadataTypeReference ReturnType { get; }
    }
}
