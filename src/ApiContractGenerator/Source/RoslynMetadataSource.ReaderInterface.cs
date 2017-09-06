using System.Reflection.Metadata;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource
    {
        private sealed class ReaderInterface : ReaderClassBase, IMetadataInterface
        {
            public ReaderInterface(MetadataReader reader, TypeDefinition definition, GenericContext parentGenericContext) : base(reader, definition, parentGenericContext)
            {
            }
        }
    }
}
