using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataProperty : IMetadataSymbol
    {
        IReadOnlyList<IMetadataAttribute> Attributes { get; }
        MetadataTypeReference PropertyType { get; }
        IReadOnlyList<MetadataTypeReference> ParameterTypes { get; }
        IMetadataMethod GetAccessor { get; }
        IMetadataMethod SetAccessor { get; }
        IMetadataConstantValue DefaultValue { get; }
    }
}
