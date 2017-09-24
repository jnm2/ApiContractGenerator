using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataParameter : IMetadataSymbol
    {
        IReadOnlyList<IMetadataAttribute> Attributes { get; }
        bool IsIn { get; }
        bool IsOut { get; }
        bool IsOptional { get; }
        MetadataTypeReference ParameterType { get; }
        IMetadataConstantValue DefaultValue { get; }
    }
}
