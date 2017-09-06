namespace ApiContractGenerator.Model
{
    public interface IMetadataMember : IMetadataSymbol
    {
        MetadataVisibility Visibility { get; }
        bool IsStatic { get; }
    }
}
