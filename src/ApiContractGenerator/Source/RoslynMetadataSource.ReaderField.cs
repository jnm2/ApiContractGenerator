using System.Reflection;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource
    {
        private sealed class ReaderField : IMetadataField
        {
            private readonly MetadataReader reader;
            private readonly FieldDefinition definition;
            private readonly GenericContext genericContext;

            public ReaderField(MetadataReader reader, FieldDefinition definition, GenericContext genericContext)
            {
                this.reader = reader;
                this.definition = definition;
                this.genericContext = genericContext;
            }

            private string name;
            public string Name => name ?? (name = reader.GetString(definition.Name));

            public MetadataVisibility Visibility
            {
                get
                {
                    switch (definition.Attributes & FieldAttributes.FieldAccessMask)
                    {
                        case FieldAttributes.Public:
                            return MetadataVisibility.Public;
                        case FieldAttributes.Family:
                        case FieldAttributes.FamORAssem:
                            return MetadataVisibility.Protected;
                        default:
                            return 0;
                    }
                }
            }

            public bool IsStatic => (definition.Attributes & FieldAttributes.Static) != 0;
            public bool IsLiteral => (definition.Attributes & FieldAttributes.Literal) != 0;
            public bool IsInitOnly => (definition.Attributes & FieldAttributes.InitOnly) != 0;

            private MetadataTypeReference fieldType;
            public MetadataTypeReference FieldType =>
                fieldType ?? (fieldType = definition.DecodeSignature(SignatureTypeProvider.Instance, genericContext));
        }
    }
}
