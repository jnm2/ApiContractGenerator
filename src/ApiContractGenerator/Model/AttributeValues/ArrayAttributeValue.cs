using System;
using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Model.AttributeValues
{
    public sealed class ArrayAttributeValue : MetadataAttributeValue
    {
        public ArrayTypeReference ArrayType { get; }
        public IReadOnlyList<MetadataAttributeValue> Elements { get; }

        public ArrayAttributeValue(ArrayTypeReference arrayType, IReadOnlyList<MetadataAttributeValue> elements)
        {
            ArrayType = arrayType ?? throw new ArgumentNullException(nameof(arrayType));
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }
    }
}
