using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataEvent : IMetadataSymbol
    {
        MetadataTypeReference HandlerType { get; }
        IMetadataMethod AddAccessor { get; }
        IMetadataMethod RemoveAccessor { get; }
        IMetadataMethod RaiseAccessor { get; }
    }
}
