namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class ArrayTypeReference : MetadataTypeReference
    {
        public ArrayTypeReference(MetadataTypeReference elementType, int dimensions)
        {
            ElementType = elementType;
            Dimensions = dimensions;
        }

        public MetadataTypeReference ElementType { get; }

        public int Dimensions { get; }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit(this);
    }
}
