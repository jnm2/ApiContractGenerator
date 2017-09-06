using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataField : IMetadataMember
    {
        bool IsLiteral { get; }
        bool IsInitOnly { get; }
        MetadataTypeReference FieldType { get; }
    }
}
