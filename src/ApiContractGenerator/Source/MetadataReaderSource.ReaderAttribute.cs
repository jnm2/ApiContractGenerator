using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
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

            private IReadOnlyList<IMetadataAttributeArgument> fixedArguments;
            public IReadOnlyList<IMetadataAttributeArgument> FixedArguments
            {
                get
                {
                    if (fixedArguments == null)
                        DecodeValue();
                    return fixedArguments;
                }
            }

            private IReadOnlyList<IMetadataAttributeNamedArgument> namedArguments;
            public IReadOnlyList<IMetadataAttributeNamedArgument> NamedArguments
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
                    fixedArguments = Array.Empty<IMetadataAttributeArgument>();
                }
                else
                {
                    var r = new IMetadataAttributeArgument[value.FixedArguments.Length];
                    for (var i = 0; i < r.Length; i++)
                    {
                        var arg = value.FixedArguments[i];
                        r[i] = new Argument(arg.Value, arg.Type);
                    }
                    fixedArguments = r;
                }

                if (value.NamedArguments.IsEmpty)
                {
                    namedArguments = Array.Empty<IMetadataAttributeNamedArgument>();
                }
                else
                {
                    var r = new IMetadataAttributeNamedArgument[value.FixedArguments.Length];
                    for (var i = 0; i < r.Length; i++)
                    {
                        var arg = value.NamedArguments[i];
                        r[i] = new NamedArgument(arg.Value, arg.Type, arg.Name);
                    }
                    namedArguments = r;
                }
            }

            private class Argument : IMetadataAttributeArgument
            {
                public Argument(object value, MetadataTypeReference valueType)
                {
                    Value = value;
                    ValueType = valueType;
                }

                public object Value { get; }
                public MetadataTypeReference ValueType { get; }
            }

            private sealed class NamedArgument : Argument, IMetadataAttributeNamedArgument
            {
                public NamedArgument(object value, MetadataTypeReference valueType, string name) : base(value, valueType)
                {
                    Name = name;
                }

                public string Name { get; }
            }
        }
    }
}
