using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model
{
    public interface IMetadataType : IMetadataSymbol
    {
        IReadOnlyList<IMetadataAttribute> Attributes { get; }
        MetadataVisibility Visibility { get; }
        IReadOnlyList<IMetadataGenericTypeParameter> GenericTypeParameters { get; }
        MetadataTypeReference BaseType { get; }
        IReadOnlyList<MetadataTypeReference> InterfaceImplementations { get; }
        IReadOnlyList<IMetadataField> Fields { get; }
        IReadOnlyList<IMetadataProperty> Properties { get; }
        IReadOnlyList<IMetadataEvent> Events { get; }
        IReadOnlyList<IMetadataMethod> Methods { get; }
        IReadOnlyList<IMetadataType> NestedTypes { get; }
    }
}
