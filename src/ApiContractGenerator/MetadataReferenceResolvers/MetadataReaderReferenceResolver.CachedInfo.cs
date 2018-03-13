namespace ApiContractGenerator.MetadataReferenceResolvers
{
    public sealed partial class MetadataReaderReferenceResolver
    {
        private readonly struct CachedInfo
        {
            public readonly bool IsValueType;
            public readonly bool IsDelegateType;
            public readonly EnumInfo? EnumInfo;

            private CachedInfo(bool isValueType, bool isDelegateType, EnumInfo? enumInfo)
            {
                IsValueType = isValueType;
                EnumInfo = enumInfo;
                IsDelegateType = isDelegateType;
            }

            public static readonly CachedInfo Default = default;
            public static readonly CachedInfo ValueType = new CachedInfo(true, false, null);
            public static readonly CachedInfo Delegate = new CachedInfo(false, true, null);

            public static CachedInfo Enum(EnumInfo info) => new CachedInfo(true, false, info);
        }
    }
}
