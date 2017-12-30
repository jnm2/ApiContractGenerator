namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class TopLevelTypeReference : MetadataTypeReference
    {
        public MetadataAssemblyReference Assembly { get; }
        public string Namespace { get; }
        public string Name { get; }

        public TopLevelTypeReference(MetadataAssemblyReference assembly, string @namespace, string name)
        {
            Assembly = assembly;
            Namespace = @namespace;
            Name = name;
        }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit(this);
    }
}
