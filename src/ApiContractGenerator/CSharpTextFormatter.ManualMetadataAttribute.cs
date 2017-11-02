using System;
using System.Collections.Generic;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.AttributeValues;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator
{
    public sealed partial class CSharpTextFormatter
    {
        private sealed class ManualMetadataAttribute : IMetadataAttribute
        {
            public ManualMetadataAttribute(MetadataTypeReference attributeType)
            {
                AttributeType = attributeType ?? throw new ArgumentNullException(nameof(attributeType));
            }

            public MetadataTypeReference AttributeType { get; }
            public IReadOnlyList<MetadataAttributeValue> FixedArguments => Array.Empty<MetadataAttributeValue>();
            public IReadOnlyList<MetadataAttributeNamedArgument> NamedArguments => Array.Empty<MetadataAttributeNamedArgument>();
        }
    }
}
