using System;
using ApiContractGenerator.Model.AttributeValues;

namespace ApiContractGenerator.Model
{
    public readonly struct MetadataAttributeNamedArgument
    {
        public MetadataAttributeNamedArgument(string name, MetadataAttributeValue value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Name { get; }
        public MetadataAttributeValue Value { get; }
    }
}
