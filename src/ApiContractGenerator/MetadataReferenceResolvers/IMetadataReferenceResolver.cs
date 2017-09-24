using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.MetadataReferenceResolvers
{
    public interface IMetadataReferenceResolver
    {
        bool TryGetEnumInfo(MetadataTypeReference typeReference, out EnumInfo info);
        bool TryGetIsValueType(MetadataTypeReference typeReference, out bool isValueType);
    }
}
