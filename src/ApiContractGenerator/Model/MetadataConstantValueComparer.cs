using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace ApiContractGenerator.Model
{
    public sealed class MetadataConstantValueComparer : IComparer<IMetadataConstantValue>
    {
        public static MetadataConstantValueComparer Instance { get; } = new MetadataConstantValueComparer();
        private MetadataConstantValueComparer() { }

        public int Compare(IMetadataConstantValue x, IMetadataConstantValue y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            if (x.TypeCode == ConstantTypeCode.NullReference)
                return y.TypeCode == ConstantTypeCode.NullReference ? 0 : -1;
            if (y.TypeCode == ConstantTypeCode.NullReference) return 1;

            if (x.TypeCode != y.TypeCode)
                throw new NotImplementedException("Comparing values of different primitive types");

            switch (x.TypeCode)
            {
                case ConstantTypeCode.Boolean:
                    return x.GetValueAsBoolean().CompareTo(y.GetValueAsBoolean());
                case ConstantTypeCode.Char:
                    return x.GetValueAsChar().CompareTo(y.GetValueAsChar());
                case ConstantTypeCode.SByte:
                    return x.GetValueAsSByte().CompareTo(y.GetValueAsSByte());
                case ConstantTypeCode.Byte:
                    return x.GetValueAsByte().CompareTo(y.GetValueAsByte());
                case ConstantTypeCode.Int16:
                    return x.GetValueAsInt16().CompareTo(y.GetValueAsInt16());
                case ConstantTypeCode.UInt16:
                    return x.GetValueAsUInt16().CompareTo(y.GetValueAsUInt16());
                case ConstantTypeCode.Int32:
                    return x.GetValueAsInt32().CompareTo(y.GetValueAsInt32());
                case ConstantTypeCode.UInt32:
                    return x.GetValueAsUInt32().CompareTo(y.GetValueAsUInt32());
                case ConstantTypeCode.Int64:
                    return x.GetValueAsInt64().CompareTo(y.GetValueAsInt64());
                case ConstantTypeCode.UInt64:
                    return x.GetValueAsUInt64().CompareTo(y.GetValueAsUInt64());
                case ConstantTypeCode.Single:
                    return x.GetValueAsSingle().CompareTo(y.GetValueAsSingle());
                case ConstantTypeCode.Double:
                    return x.GetValueAsDouble().CompareTo(y.GetValueAsDouble());
                case ConstantTypeCode.String:
                    return string.Compare(x.GetValueAsString(), y.GetValueAsString(), StringComparison.Ordinal);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
