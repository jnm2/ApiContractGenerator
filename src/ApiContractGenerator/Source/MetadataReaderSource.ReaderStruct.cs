using System.Reflection.Metadata;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class ReaderStruct : ReaderClassBase, IMetadataStruct
        {
            public ReaderStruct(MetadataReader reader, TypeReferenceTypeProvider typeProvider, TypeDefinition definition) : base(reader, typeProvider, definition)
            {
            }
        }
    }
}
