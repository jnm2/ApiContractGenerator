using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataDelegate : IMetadataType
    {
        MetadataTypeReference ReturnType { get; }
        IReadOnlyList<IMetadataAttribute> ReturnValueAttributes { get; }
        IReadOnlyList<IMetadataParameter> Parameters { get; }
    }
}
