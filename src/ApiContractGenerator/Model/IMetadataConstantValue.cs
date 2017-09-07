using System.Reflection.Metadata;

namespace ApiContractGenerator.Model
{
    public interface IMetadataConstantValue
    {
        ConstantTypeCode TypeCode { get; }
        bool GetValueAsBoolean();
        char GetValueAsChar();
        sbyte GetValueAsSByte();
        byte GetValueAsByte();
        short GetValueAsInt16();
        ushort GetValueAsUInt16();
        int GetValueAsInt32();
        uint GetValueAsUInt32();
        long GetValueAsInt64();
        ulong GetValueAsUInt64();
        float GetValueAsSingle();
        double GetValueAsDouble();
        string GetValueAsString();
    }
}
