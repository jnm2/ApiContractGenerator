using System.Collections.Generic;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public interface IMetadataSource
    {
        IReadOnlyList<IMetadataNamespace> Namespaces { get; }
    }
}
