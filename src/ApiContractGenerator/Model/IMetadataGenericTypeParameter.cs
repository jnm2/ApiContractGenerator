using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataGenericTypeParameter : IMetadataSymbol
    {
        bool IsCovariant { get; }
        bool IsContravariant { get; }
        bool HasReferenceTypeConstraint { get; }
        bool HasNotNullableValueTypeConstraint { get; }
        bool HasDefaultConstructorConstraint { get; }
        IReadOnlyList<MetadataTypeReference> TypeConstraints { get; }
    }
}
