using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataEvent : IMetadataSymbol
    {
        IReadOnlyList<IMetadataAttribute> Attributes { get; }
        MetadataTypeReference HandlerType { get; }
        IMetadataMethod AddAccessor { get; }
        IMetadataMethod RemoveAccessor { get; }
        IMetadataMethod RaiseAccessor { get; }
    }
}
