using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class ReaderMethod : IMetadataMethod
        {
            private readonly MetadataReader reader;
            private readonly MethodDefinition definition;
            private readonly GenericContext declaringTypeGenericContext;

            public ReaderMethod(MetadataReader reader, MethodDefinition definition, GenericContext declaringTypeGenericContext)
            {
                this.reader = reader;
                this.definition = definition;
                this.declaringTypeGenericContext = declaringTypeGenericContext;
            }

            private GenericContext? genericContext;
            private GenericContext GenericContext
            {
                get
                {
                    if (genericContext == null)
                        genericContext = new GenericContext(declaringTypeGenericContext.TypeParameters, GetGenericParameters(reader, definition.GetGenericParameters()));
                    return genericContext.Value;
                }
            }

            private string name;
            public string Name => name ?? (name = reader.GetString(definition.Name));

            public MetadataVisibility Visibility
            {
                get
                {
                    switch (definition.Attributes & MethodAttributes.MemberAccessMask)
                    {
                        case MethodAttributes.Public:
                            return MetadataVisibility.Public;
                        case MethodAttributes.Family:
                        case MethodAttributes.FamORAssem:
                            return MetadataVisibility.Protected;
                        default:
                            return 0;
                    }
                }
            }

            public IReadOnlyList<GenericParameterTypeReference> GenericTypeParameters => GenericContext.MethodParameters;

            public bool IsStatic => (definition.Attributes & MethodAttributes.Static) != 0;
            public bool IsAbstract => (definition.Attributes & MethodAttributes.Abstract) != 0;
            public bool IsVirtual => (definition.Attributes & MethodAttributes.Virtual) != 0;
            public bool IsFinal => (definition.Attributes & MethodAttributes.Final) != 0;
            public bool IsOverride => (definition.Attributes & (MethodAttributes.Virtual | MethodAttributes.NewSlot)) == MethodAttributes.Virtual;

            private MethodSignature<MetadataTypeReference>? signature;
            private MethodSignature<MetadataTypeReference> Signature
            {
                get
                {
                    if (signature == null)
                        signature = definition.DecodeSignature(SignatureTypeProvider.Instance, GenericContext);
                    return signature.Value;
                }
            }

            public MetadataTypeReference ReturnType => Signature.ReturnType;

            private IReadOnlyList<IMetadataParameter> parameters;
            public IReadOnlyList<IMetadataParameter> Parameters
            {
                get
                {
                    if (parameters == null)
                    {
                        var handles = definition.GetParameters();

                        var r = new IMetadataParameter[handles.Count];
                        var signature = Signature;
                        var maxSequenceNumber = 0;
                        foreach (var handle in handles)
                        {
                            var parameter = reader.GetParameter(handle);
                            if (parameter.SequenceNumber == 0) continue;
                            if (maxSequenceNumber < parameter.SequenceNumber) maxSequenceNumber = parameter.SequenceNumber;
                            var parameterIndex = parameter.SequenceNumber - 1;
                            r[parameterIndex] = new ReaderParameter(reader, parameter, signature.ParameterTypes[parameterIndex]);
                        }

                        if (maxSequenceNumber != r.Length)
                        {
                            // Account for skipping the return parameter
                            var correctedLength = new IMetadataParameter[maxSequenceNumber];
                            Array.Copy(r, correctedLength, correctedLength.Length);
                            parameters = correctedLength;
                        }
                        else
                        {
                            parameters = r;
                        }
                    }
                    return parameters;
                }
            }
        }
    }
}
