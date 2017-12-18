namespace ApiContractGenerator.Model.TypeReferences
{
    public interface IMetadataTypeReferenceVisitor<out T>
    {
        T Visit(ArrayTypeReference array);
        T Visit(PrimitiveTypeReference primitiveTypeReference);
        T Visit(TopLevelTypeReference topLevelTypeReference);
        T Visit(GenericParameterTypeReference genericParameterTypeReference);
        T Visit(GenericInstantiationTypeReference genericInstantiationTypeReference);
        T Visit(ByRefTypeReference byRefTypeReference);
        T Visit(NestedTypeReference nestedTypeReference);
        T Visit(PointerTypeReference pointerTypeReference);
    }
}
