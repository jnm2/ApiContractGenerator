using System;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model.AttributeValues
{
    public sealed class TypeAttributeValue : MetadataAttributeValue
    {
        public MetadataTypeReference Value { get; }

        public TypeAttributeValue(MetadataTypeReference value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
