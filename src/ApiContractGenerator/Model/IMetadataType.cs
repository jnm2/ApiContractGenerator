using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataType : IMetadataSymbol
    {
        MetadataVisibility Visibility { get; }
        IReadOnlyList<GenericParameterTypeReference> GenericTypeParameters { get; }
        MetadataTypeReference BaseType { get; }
        IReadOnlyList<IMetadataField> Fields { get; }
        IReadOnlyList<IMetadataType> NestedTypes { get; }
    }
}
