using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class ReaderParameter : IMetadataParameter
        {
            private readonly MetadataReader reader;
            private readonly TypeReferenceTypeProvider typeProvider;
            private readonly Parameter definition;
            private readonly GenericContext genericContext;

            public ReaderParameter(MetadataReader reader, TypeReferenceTypeProvider typeProvider, Parameter definition, GenericContext genericContext, MetadataTypeReference parameterType)
            {
                this.reader = reader;
                this.typeProvider = typeProvider;
                this.definition = definition;
                this.genericContext = genericContext;
                ParameterType = parameterType;
            }

            private string name;
            public string Name => name ?? (name = reader.GetString(definition.Name));

            private IReadOnlyList<IMetadataAttribute> attributes;
            public IReadOnlyList<IMetadataAttribute> Attributes => attributes ?? (attributes =
                GetAttributes(reader, typeProvider, definition.GetCustomAttributes(), genericContext));

            public bool IsIn => (definition.Attributes & ParameterAttributes.In) != 0;
            public bool IsOut => (definition.Attributes & ParameterAttributes.Out) != 0;
            public bool IsOptional => (definition.Attributes & ParameterAttributes.Optional) != 0;

            public MetadataTypeReference ParameterType { get; }

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
