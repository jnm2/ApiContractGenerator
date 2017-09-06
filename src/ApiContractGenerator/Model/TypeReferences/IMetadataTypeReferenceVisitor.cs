namespace ApiContractGenerator.Model.TypeReferences
{
    public interface IMetadataTypeReferenceVisitor<out T>
    {
        T Visit(ArrayTypeReference array);
        T Visit(PrimitiveTypeReference primitiveTypeReference);
        T Visit(NamedTypeReference namedTypeReference);
        T Accept(GenericParameterTypeReference genericParameterTypeReference);
    }
}
