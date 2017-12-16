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
            public ReaderDelegate(MetadataReader reader, TypeReferenceTypeProvider typeProvider, TypeDefinition definition) : base(reader, typeProvider, definition)
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
                        invokeMethodSignature = InvokeMethod.DecodeSignature(TypeProvider, GenericContext);
                    return invokeMethodSignature.Value;
                }
            }

            public MetadataTypeReference ReturnType => InvokeMethodSignature.ReturnType;

            private void ReadParameters()
            {
                (parameters, returnValueAttributes) = GetParameters(Reader, TypeProvider, GenericContext, InvokeMethodSignature, InvokeMethod.GetParameters());
            }

            private IReadOnlyList<IMetadataAttribute> returnValueAttributes;
            public IReadOnlyList<IMetadataAttribute> ReturnValueAttributes
            {
                get
                {
                    if (returnValueAttributes == null) ReadParameters();
                    return returnValueAttributes;
                }
            }

            private IReadOnlyList<IMetadataParameter> parameters;
            public IReadOnlyList<IMetadataParameter> Parameters
            {
                get
                {
                    if (parameters == null) ReadParameters();
                    return parameters;
                }
            }
        }
    }
}
