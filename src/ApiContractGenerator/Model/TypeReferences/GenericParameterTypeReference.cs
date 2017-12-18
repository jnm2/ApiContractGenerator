namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class GenericParameterTypeReference : MetadataTypeReference
    {
        public GenericParameterTypeReference(IMetadataGenericTypeParameter typeParameter)
        {
            TypeParameter = typeParameter;
        }

        public IMetadataGenericTypeParameter TypeParameter { get; }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit(this);
    }
}
