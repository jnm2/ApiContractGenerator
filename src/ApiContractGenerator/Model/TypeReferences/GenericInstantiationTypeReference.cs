using System.Collections.Generic;

namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class GenericInstantiationTypeReference : MetadataTypeReference
    {
        public MetadataTypeReference TypeDefinition { get; }
        public IReadOnlyList<MetadataTypeReference> GenericTypeArguments { get; }

        public GenericInstantiationTypeReference(MetadataTypeReference typeDefinition, IReadOnlyList<MetadataTypeReference> genericTypeArguments)
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
