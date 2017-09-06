using ApiContractGenerator.Source;

namespace ApiContractGenerator.Model
{
    public interface IMetadataClass : IMetadataType, IMetadataSource
    {
        bool IsStatic { get; }
        bool IsAbstract { get; }
        bool IsSealed { get; }
    }
}
