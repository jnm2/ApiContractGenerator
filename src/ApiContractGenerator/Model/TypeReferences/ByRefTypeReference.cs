namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class ByRefTypeReference : MetadataTypeReference
    {
        public MetadataTypeReference ElementType { get; }

        public ByRefTypeReference(MetadataTypeReference elementType)
        {
            ElementType = elementType;
        }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit(this);
    }
}
