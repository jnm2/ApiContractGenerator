using System.Collections.Generic;

namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class GenericInstantiationTypeReference : MetadataTypeReference
    {
        public NamespaceTypeReference TypeDefinition { get; }
        public IReadOnlyList<MetadataTypeReference> GenericTypeArguments { get; }

        public GenericInstantiationTypeReference(NamespaceTypeReference typeDefinition, IReadOnlyList<MetadataTypeReference> genericTypeArguments)
        {
            TypeDefinition = typeDefinition;
            GenericTypeArguments = genericTypeArguments;
        }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
