using System.Collections.Generic;

namespace ApiContractGenerator.Model
{
    public interface IMetadataNamespace : IMetadataSymbol
    {
        IReadOnlyList<IMetadataType> Types { get; }
    }
}
