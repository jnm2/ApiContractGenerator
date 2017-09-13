using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataMethod : IMetadataSymbol
    {
        IReadOnlyList<IMetadataAttribute> Attributes { get; }
        MetadataVisibility Visibility { get; }
        bool IsStatic { get; }
        IReadOnlyList<IMetadataGenericTypeParameter> GenericTypeParameters { get; }
        bool IsAbstract { get; }
        bool IsOverride { get; }
        bool IsFinal { get; }
        bool IsVirtual { get; }
        MetadataTypeReference ReturnType { get; }
        IReadOnlyList<IMetadataParameter> Parameters { get; }
    }
}
