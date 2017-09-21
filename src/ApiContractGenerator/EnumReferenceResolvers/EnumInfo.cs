using System.Collections.Generic;
using System.Reflection.Metadata;

namespace ApiContractGenerator.EnumReferenceResolvers
{
    public struct EnumInfo
    {
        public EnumInfo(bool isFlags, PrimitiveTypeCode underlyingType, IReadOnlyList<EnumFieldInfo> fields)
        {
            IsFlags = isFlags;
            UnderlyingType = underlyingType;
            Fields = fields;
        }

        public bool IsFlags { get; }
        public PrimitiveTypeCode UnderlyingType { get; }
        public IReadOnlyList<EnumFieldInfo> Fields { get; }
    }
}
