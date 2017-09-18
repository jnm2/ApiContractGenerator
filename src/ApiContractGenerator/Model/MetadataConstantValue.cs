using System;
using System.Reflection.Metadata;

namespace ApiContractGenerator.Model
{
    public static class MetadataConstantValue
    {
        public static IMetadataConstantValue Null { get; } = new ObjectAsConstantValue(null);

        public static IMetadataConstantValue FromObject(object value)
        {
            return value == null ? Null : new ObjectAsConstantValue(value);
        }

        private sealed class ObjectAsConstantValue : IMetadataConstantValue
        {
            private readonly object value;

            public ConstantTypeCode TypeCode { get; }

            public ObjectAsConstantValue(object value)
            {
                TypeCode = GetTypeCode(value);
                this.value = value;
            }

            private static ConstantTypeCode GetTypeCode(object value)
            {
                switch (value)
                {
                    case null:
                        return ConstantTypeCode.NullReference;
                    case bool _:
                        return ConstantTypeCode.Boolean;
                    case char _:
                        return ConstantTypeCode.Char;
                    case sbyte _:
                        return ConstantTypeCode.SByte;
                    case byte _:
                        return ConstantTypeCode.Byte;
                    case short _:
                        return ConstantTypeCode.Int16;
                    case ushort _:
                        return ConstantTypeCode.UInt16;
                    case int _:
                        return ConstantTypeCode.Int32;
                    case uint _:
                        return ConstantTypeCode.UInt32;
                    case long _:
                        return ConstantTypeCode.Int64;
                    case ulong _:
                        return ConstantTypeCode.UInt64;
                    case float _:
                        return ConstantTypeCode.Single;
                    case double _:
                        return ConstantTypeCode.Double;
                    case string _:
                        return ConstantTypeCode.String;
                    default:
                        throw new NotImplementedException();
                }
            }

            public bool GetValueAsBoolean() => (bool)value;

            public char GetValueAsChar() => (char)value;

            public sbyte GetValueAsSByte() => (sbyte)value;

            public byte GetValueAsByte() => (byte)value;

            public short GetValueAsInt16() => (short)value;

            public ushort GetValueAsUInt16() => (ushort)value;

            public int GetValueAsInt32() => (int)value;

            public uint GetValueAsUInt32() => (uint)value;

            public long GetValueAsInt64() => (long)value;

            public ulong GetValueAsUInt64() => (ulong)value;

            public float GetValueAsSingle() => (float)value;

            public double GetValueAsDouble() => (double)value;

            public string GetValueAsString() => (string)value;
        }
    }
}
