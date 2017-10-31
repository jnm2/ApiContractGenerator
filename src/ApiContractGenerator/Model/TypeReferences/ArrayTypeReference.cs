namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class ArrayTypeReference : MetadataTypeReference
    {
        public ArrayTypeReference(MetadataTypeReference elementType, int rank)
        {
            ElementType = elementType;
            Rank = rank;
        }

        public MetadataTypeReference ElementType { get; }

        public int Rank { get; }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit(this);
    }
}
