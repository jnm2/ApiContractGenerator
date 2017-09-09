using System.Reflection;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class ReaderClass : ReaderClassBase, IMetadataClass
        {
            public ReaderClass(MetadataReader reader, TypeDefinition definition, GenericContext parentGenericContext) : base(reader, definition, parentGenericContext)
            {
            }

            public bool IsStatic => (Definition.Attributes & (TypeAttributes.Abstract | TypeAttributes.Sealed)) == (TypeAttributes.Abstract | TypeAttributes.Sealed);

            public bool IsAbstract => (Definition.Attributes & (TypeAttributes.Abstract | TypeAttributes.Sealed)) == TypeAttributes.Abstract;

            public bool IsSealed => (Definition.Attributes & (TypeAttributes.Abstract | TypeAttributes.Sealed)) == TypeAttributes.Sealed;
        }
    }
}
