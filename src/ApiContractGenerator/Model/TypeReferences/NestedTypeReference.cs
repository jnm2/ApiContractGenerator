namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class NestedTypeReference : MetadataTypeReference
    {
        public MetadataTypeReference DeclaringType { get; }
        public string Name { get; }

        public NestedTypeReference(MetadataTypeReference declaringType, string name)
        {
            DeclaringType = declaringType;
            Name = name;
        }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit(this);
    }
}
