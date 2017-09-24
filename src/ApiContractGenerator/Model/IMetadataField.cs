using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataField : IMetadataSymbol
    {
        IReadOnlyList<IMetadataAttribute> Attributes { get; }
        MetadataVisibility Visibility { get; }
        bool IsStatic { get; }
        bool IsLiteral { get; }
        bool IsInitOnly { get; }
        MetadataTypeReference FieldType { get; }
        IMetadataConstantValue DefaultValue { get; }
    }
}
