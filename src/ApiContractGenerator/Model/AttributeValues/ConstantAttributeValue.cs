using System;

namespace ApiContractGenerator.Model.AttributeValues
{
    public sealed class ConstantAttributeValue : MetadataAttributeValue
    {
        public static ConstantAttributeValue Null { get; } = new ConstantAttributeValue(MetadataConstantValue.Null);

        public IMetadataConstantValue Value { get; }

        public ConstantAttributeValue(IMetadataConstantValue value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
