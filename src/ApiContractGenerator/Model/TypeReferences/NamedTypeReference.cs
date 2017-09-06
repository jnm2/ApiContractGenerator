namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class NamedTypeReference : MetadataTypeReference
    {
        public string Namespace { get; }
        public string Name { get; }

        public NamedTypeReference(string @namespace, string name)
        {
            Namespace = @namespace;
            Name = name;
        }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit((NamedTypeReference)this);
    }
}
