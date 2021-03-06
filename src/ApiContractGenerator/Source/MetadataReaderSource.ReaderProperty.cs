using System.Collections.Generic;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class ReaderProperty : IMetadataProperty
        {
            private readonly MetadataReader reader;
            private readonly TypeReferenceTypeProvider typeProvider;
            private readonly PropertyDefinition definition;
            private readonly GenericContext genericContext;

            public ReaderProperty(MetadataReader reader, TypeReferenceTypeProvider typeProvider, PropertyDefinition definition, GenericContext genericContext, IMetadataMethod getAccessor, IMetadataMethod setAccessor)
            {
                this.reader = reader;
                this.typeProvider = typeProvider;
                this.definition = definition;
                this.genericContext = genericContext;
                GetAccessor = getAccessor;
                SetAccessor = setAccessor;
            }

            private string name;
            public string Name => name ?? (name = reader.GetString(definition.Name));

            private IReadOnlyList<IMetadataAttribute> attributes;
            public IReadOnlyList<IMetadataAttribute> Attributes => attributes ?? (attributes =
                GetAttributes(reader, typeProvider, definition.GetCustomAttributes(), genericContext));

            private MethodSignature<MetadataTypeReference>? signature;
            private MethodSignature<MetadataTypeReference> Signature
            {
                get
                {
                    if (signature == null)
                        signature = definition.DecodeSignature(typeProvider, genericContext);
                    return signature.Value;
                }
            }

            public MetadataTypeReference PropertyType => Signature.ReturnType;

            public IReadOnlyList<MetadataTypeReference> ParameterTypes => Signature.ParameterTypes;

            public IMetadataMethod GetAccessor { get; }
            public IMetadataMethod SetAccessor { get; }

            private ReaderConstantValue defaultValue;
            private bool isDefaultValueValid;
            public IMetadataConstantValue DefaultValue
            {
                get
                {
                    if (!isDefaultValueValid)
                    {
                        var defaultValueHandle = definition.GetDefaultValue();
                        if (!defaultValueHandle.IsNil) defaultValue = new ReaderConstantValue(reader, defaultValueHandle);
                        isDefaultValueValid = true;
                    }
                    return defaultValue;
                }
            }
        }
    }
}
