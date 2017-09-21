using System.Diagnostics;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.EnumReferenceResolvers
{
    [DebuggerDisplay("{ToString(),nq}")]
    public struct EnumFieldInfo
    {
        public EnumFieldInfo(string name, IMetadataConstantValue value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public IMetadataConstantValue Value { get; }

        public override string ToString() => $"{Name} = {Value}";
    }
}
