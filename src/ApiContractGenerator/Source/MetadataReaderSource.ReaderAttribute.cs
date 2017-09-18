using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.AttributeValues;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class ReaderAttribute : IMetadataAttribute
        {
            private readonly MetadataReader reader;
            private readonly CustomAttributeHandle handle;
            private readonly GenericContext genericContext;

            public ReaderAttribute(MetadataReader reader, CustomAttributeHandle handle, GenericContext genericContext)
            {
                this.reader = reader;
                this.handle = handle;
                this.genericContext = genericContext;
            }

            private CustomAttribute? definition;
            private CustomAttribute Definition
            {
                get
                {
                    if (definition == null)
                        definition = reader.GetCustomAttribute(handle);
                    return definition.Value;
                }
            }

            private MetadataTypeReference attributeType;
            public MetadataTypeReference AttributeType
            {
                get
                {
                    if (attributeType == null)
                    {
                        var constructorHandle = Definition.Constructor;
                        switch (constructorHandle.Kind)
                        {
                            case HandleKind.MemberReference:
                                var memberReference = reader.GetMemberReference((MemberReferenceHandle)constructorHandle);
                                attributeType = GetTypeFromEntityHandle(reader, genericContext, memberReference.Parent);
                                break;
                            case HandleKind.MethodDefinition:
                                var constructorDefinition = reader.GetMethodDefinition((MethodDefinitionHandle)constructorHandle);
                                attributeType = GetTypeFromTypeDefinitionHandle(reader, constructorDefinition.GetDeclaringType());
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    return attributeType;
                }
            }

            private IReadOnlyList<MetadataAttributeValue> fixedArguments;
            public IReadOnlyList<MetadataAttributeValue> FixedArguments
            {
                get
                {
                    if (fixedArguments == null)
                        DecodeValue();
                    return fixedArguments;
                }
            }

            private IReadOnlyList<MetadataAttributeNamedArgument> namedArguments;
            public IReadOnlyList<MetadataAttributeNamedArgument> NamedArguments
            {
                get
                {
                    if (namedArguments == null)
                        DecodeValue();
                    return namedArguments;
                }
            }

            private void DecodeValue()
            {
                var value = Definition.DecodeValue(TypeReferenceTypeProvider.Instance);

                if (value.FixedArguments.IsEmpty)
                {
                    fixedArguments = Array.Empty<MetadataAttributeValue>();
                }
                else
                {
                    var r = new MetadataAttributeValue[value.FixedArguments.Length];
                    for (var i = 0; i < r.Length; i++)
                    {
                        var arg = value.FixedArguments[i];
                        r[i] = DecodeValue(arg.Type, arg.Value);
                    }
                    fixedArguments = r;
                }

                if (value.NamedArguments.IsEmpty)
                {
                    namedArguments = Array.Empty<MetadataAttributeNamedArgument>();
                }
                else
                {
                    var r = new MetadataAttributeNamedArgument[value.NamedArguments.Length];
                    for (var i = 0; i < r.Length; i++)
                    {
                        var arg = value.NamedArguments[i];
                        r[i] = new MetadataAttributeNamedArgument(arg.Name, DecodeValue(arg.Type, arg.Value));
                    }
                    namedArguments = r;
                }
            }

            private MetadataAttributeValue DecodeValue(MetadataTypeReference type, object value)
            {
                if (value == null)
                {
                    return ConstantAttributeValue.Null;
                }
                if (type is PrimitiveTypeReference)
                {
                    return new ConstantAttributeValue(MetadataConstantValue.FromObject(value));
                }
                if (value is MetadataTypeReference typeValue)
                {
                    return new TypeAttributeValue(typeValue);
                }
                if (type is ArrayTypeReference arrayType)
                {
                    var items = (ImmutableArray<CustomAttributeTypedArgument<MetadataTypeReference>>)value;
                    var values = new MetadataAttributeValue[items.Length];

                    for (var i = 0; i < values.Length; i++)
                    {
                        var item = items[i];
                        values[i] = DecodeValue(item.Type, item.Value);
                    }

                    return new ArrayAttributeValue(arrayType, values);
                }

                // The only other thing DecodeValue can give us is an enum
                return new EnumAttributeValue(type, MetadataConstantValue.FromObject(value));
            }
        }
    }
}
