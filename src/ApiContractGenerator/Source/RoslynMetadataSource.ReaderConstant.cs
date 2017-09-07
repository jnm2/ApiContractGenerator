using System.Reflection.Metadata;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource
    {
        private sealed class ReaderConstantValue : IMetadataConstantValue
        {
            private readonly MetadataReader reader;
            private readonly ConstantHandle handle;

            public ReaderConstantValue(MetadataReader reader, ConstantHandle handle)
            {
                this.reader = reader;
                this.handle = handle;
            }

            private Constant? definition;

            private Constant Definition
            {
                get
                {
                    if (definition == null) definition = reader.GetConstant(handle);
                    return definition.Value;
                }
            }

            public ConstantTypeCode TypeCode => Definition.TypeCode;

            public bool GetValueAsBoolean() => reader.GetBlobReader(Definition.Value).ReadBoolean();

            public char GetValueAsChar() => reader.GetBlobReader(Definition.Value).ReadChar();

            public sbyte GetValueAsSByte() => reader.GetBlobReader(Definition.Value).ReadSByte();

            public byte GetValueAsByte() => reader.GetBlobReader(Definition.Value).ReadByte();

            public short GetValueAsInt16() => reader.GetBlobReader(Definition.Value).ReadInt16();

            public ushort GetValueAsUInt16() => reader.GetBlobReader(Definition.Value).ReadUInt16();

            public int GetValueAsInt32() => reader.GetBlobReader(Definition.Value).ReadInt32();

            public uint GetValueAsUInt32() => reader.GetBlobReader(Definition.Value).ReadUInt32();

            public long GetValueAsInt64() => reader.GetBlobReader(Definition.Value).ReadInt64();

            public ulong GetValueAsUInt64() => reader.GetBlobReader(Definition.Value).ReadUInt64();

            public float GetValueAsSingle() => reader.GetBlobReader(Definition.Value).ReadSingle();

            public double GetValueAsDouble() => reader.GetBlobReader(Definition.Value).ReadDouble();

            public string GetValueAsString()
            {
                var br = reader.GetBlobReader(Definition.Value);
                return br.ReadUTF16(br.Length);
            }
        }
    }
}
