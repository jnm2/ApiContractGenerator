using System.Reflection.Metadata;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource
    {
        private sealed class ReaderStruct : ReaderClassBase, IMetadataStruct
        {
            public ReaderStruct(MetadataReader reader, TypeDefinition definition, GenericContext parentGenericContext) : base(reader, definition, parentGenericContext)
            {
            }
        }
    }
}
