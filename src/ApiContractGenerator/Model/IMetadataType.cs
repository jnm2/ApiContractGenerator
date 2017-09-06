namespace ApiContractGenerator.Model
{
    public interface IMetadataType : IMetadataSymbol
    {
        MetadataVisibility Visibility { get; }
    }
}
