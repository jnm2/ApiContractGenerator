namespace ApiContractGenerator.MetadataReferenceResolvers
{
    public sealed partial class MetadataReaderReferenceResolver
    {
        private struct CachedInfo
        {
            public readonly bool IsValueType;
            public readonly EnumInfo? EnumInfo;

            private CachedInfo(bool isValueType, EnumInfo? enumInfo)
            {
                IsValueType = isValueType;
                EnumInfo = enumInfo;
            }

            public static readonly CachedInfo Default = default;
            public static readonly CachedInfo ValueType = new CachedInfo(true, null);
            public static CachedInfo Enum(EnumInfo info) => new CachedInfo(true, info);
        }
    }
}
