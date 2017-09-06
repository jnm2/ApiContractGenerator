namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class GenericParameterTypeReference : MetadataTypeReference
    {
        public GenericParameterTypeReference(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Accept(this);
    }
}
