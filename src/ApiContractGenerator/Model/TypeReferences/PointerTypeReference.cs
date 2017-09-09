namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class PointerTypeReference : MetadataTypeReference
    {
        public MetadataTypeReference ElementType { get; }

        public PointerTypeReference(MetadataTypeReference elementType)
        {
            ElementType = elementType;
        }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit(this);
    }
}
