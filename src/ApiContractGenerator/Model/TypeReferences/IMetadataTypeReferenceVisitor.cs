namespace ApiContractGenerator.Model.TypeReferences
{
    public interface IMetadataTypeReferenceVisitor<out T>
    {
        T Visit(ArrayTypeReference array);
        T Visit(PrimitiveTypeReference primitiveTypeReference);
        T Visit(NamespaceTypeReference namespaceTypeReference);
        T Accept(GenericParameterTypeReference genericParameterTypeReference);
        T Visit(GenericInstantiationTypeReference genericInstantiationTypeReference);
        T Visit(NestedTypeReference nestedTypeReference);
    }
}
