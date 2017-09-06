namespace ApiContractGenerator.Model.TypeReferences
{
    public abstract class MetadataTypeReference
    {
        public abstract T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor);
    }
}
