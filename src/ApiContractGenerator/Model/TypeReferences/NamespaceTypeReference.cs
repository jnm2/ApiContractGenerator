namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class NamespaceTypeReference : MetadataTypeReference
    {
        public string Namespace { get; }
        public string Name { get; }

        public NamespaceTypeReference(string @namespace, string name)
        {
            Namespace = @namespace;
            Name = name;
        }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit(this);
    }
}
