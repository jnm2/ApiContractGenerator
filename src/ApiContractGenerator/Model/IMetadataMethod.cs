using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataMethod : IMetadataSymbol
    {
        MetadataVisibility Visibility { get; }
        bool IsStatic { get; }
        IReadOnlyList<GenericParameterTypeReference> GenericTypeParameters { get; }
        bool IsAbstract { get; }
        bool IsOverride { get; }
        bool IsFinal { get; }
        bool IsVirtual { get; }
        MetadataTypeReference ReturnType { get; }
        IReadOnlyList<IMetadataParameter> Parameters { get; }
    }
}