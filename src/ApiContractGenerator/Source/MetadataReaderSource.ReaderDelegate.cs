using System.Collections.Generic;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class ReaderDelegate : ReaderClassBase, IMetadataDelegate
        {
            public ReaderDelegate(MetadataReader reader, TypeDefinition definition) : base(reader, definition)
            {
            }

            private MethodDefinition? invokeMethod;
            private MethodDefinition InvokeMethod
            {
                get
                {
                    if (invokeMethod == null)
                    {
                        foreach (var handle in Definition.GetMethods())
                        {
                            var method = Reader.GetMethodDefinition(handle);
                            if (Reader.GetString(method.Name) == "Invoke")
                            {
                                invokeMethod = method;
                                break;
                            }
                        }
                    }
                    return invokeMethod.Value;
                }
            }

            private MethodSignature<MetadataTypeReference>? invokeMethodSignature;
            private MethodSignature<MetadataTypeReference> InvokeMethodSignature
            {
                get
                {
                    if (invokeMethodSignature == null)
                        invokeMethodSignature = InvokeMethod.DecodeSignature(TypeReferenceTypeProvider.Instance, GenericContext);
                    return invokeMethodSignature.Value;
                }
            }

            public MetadataTypeReference ReturnType => InvokeMethodSignature.ReturnType;

            private IReadOnlyList<IMetadataParameter> parameters;
            public IReadOnlyList<IMetadataParameter> Parameters
            {
                get
                {
                    if (parameters == null)
                    {
                        var handles = InvokeMethod.GetParameters();
                        var r = new IMetadataParameter[handles.Count];
                        var signature = InvokeMethodSignature;
                        foreach (var handle in handles)
                        {
                            var parameter = Reader.GetParameter(handle);
                            var parameterIndex = parameter.SequenceNumber - 1;
                            r[parameterIndex] = new ReaderParameter(Reader, parameter, GenericContext, signature.ParameterTypes[parameterIndex]);
                        }
                        parameters = r;
                    }
                    return parameters;
                }
            }
        }
    }
}
