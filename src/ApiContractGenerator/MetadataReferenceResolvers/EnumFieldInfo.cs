using System;
using System.Diagnostics;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.MetadataReferenceResolvers
{
    [DebuggerDisplay("{ToString(),nq}")]
    public struct EnumFieldInfo : IComparable<EnumFieldInfo>
    {
        public EnumFieldInfo(string name, IMetadataConstantValue value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Name { get; }
        public IMetadataConstantValue Value { get; }

        public override string ToString() => $"{Name} = {Value}";

        public int CompareTo(EnumFieldInfo other)
        {
            var byValue = MetadataConstantValueComparer.Instance.Compare(Value, other.Value);
            return byValue != 0 ? byValue : string.Compare(Name, other.Name, StringComparison.Ordinal);
        }
    }
}
