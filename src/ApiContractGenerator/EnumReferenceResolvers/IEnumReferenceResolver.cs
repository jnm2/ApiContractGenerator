using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.EnumReferenceResolvers
{
    public interface IEnumReferenceResolver
    {
        bool TryGetEnumInfo(MetadataTypeReference typeReference, out EnumInfo info);
    }
}
