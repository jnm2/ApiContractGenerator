using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace ApiContractGenerator.EnumReferenceResolvers
{
    public struct EnumInfo
    {
        public EnumInfo(bool isFlags, PrimitiveTypeCode underlyingType, IReadOnlyList<EnumFieldInfo> sortedFields)
        {
            IsFlags = isFlags;
            UnderlyingType = underlyingType;
            SortedFields = sortedFields ?? throw new ArgumentNullException(nameof(sortedFields));
        }

        public bool IsFlags { get; }
        public PrimitiveTypeCode UnderlyingType { get; }
        public IReadOnlyList<EnumFieldInfo> SortedFields { get; }
    }
}
