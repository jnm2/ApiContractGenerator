using System;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model.AttributeValues
{
    public sealed class EnumAttributeValue : MetadataAttributeValue
    {
        public MetadataTypeReference EnumType { get; }
        public IMetadataConstantValue UnderlyingValue { get; }

        public EnumAttributeValue(MetadataTypeReference enumType, IMetadataConstantValue underlyingValue)
        {
            EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
            UnderlyingValue = underlyingValue ?? throw new ArgumentNullException(nameof(underlyingValue));
        }
    }
}
