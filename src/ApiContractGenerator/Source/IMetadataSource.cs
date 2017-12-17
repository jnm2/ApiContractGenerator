using System.Collections.Generic;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public interface IMetadataSource
    {
        string AssemblyName { get; }
        byte[] PublicKey { get; }
        IReadOnlyList<IMetadataNamespace> Namespaces { get; }
    }
}
