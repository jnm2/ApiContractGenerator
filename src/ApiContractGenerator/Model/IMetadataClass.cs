namespace ApiContractGenerator.Model
{
    public interface IMetadataClass : IMetadataType
    {
        bool IsStatic { get; }
        bool IsAbstract { get; }
        bool IsSealed { get; }
    }
}
