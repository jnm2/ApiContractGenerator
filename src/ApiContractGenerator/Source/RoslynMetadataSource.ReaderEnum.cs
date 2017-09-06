using System.Reflection;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource
    {
        private sealed class ReaderEnum : ReaderClassBase, IMetadataEnum
        {
            public ReaderEnum(MetadataReader reader, TypeDefinition definition, GenericContext parentGenericContext) : base(reader, definition, parentGenericContext)
            {
            }

            private void ParseFields()
            {
                foreach (var handle in Definition.GetFields())
                {
                    var field = Reader.GetFieldDefinition(handle);
                    if ((field.Attributes & FieldAttributes.Static) != 0)
                    {
                    }
                    else
                    {
                        underlyingType = field.DecodeSignature(SignatureTypeProvider.Instance, GenericContext);
                    }
                }
            }

            private MetadataTypeReference underlyingType;
            public MetadataTypeReference UnderlyingType
            {
                get
                {
                    if (underlyingType == null) ParseFields();
                    return underlyingType;
                }
            }
        }
    }
}
