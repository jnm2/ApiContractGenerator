using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource
    {
        private sealed class ReaderDelegate : ReaderClassBase, IMetadataDelegate
        {
            public ReaderDelegate(MetadataReader reader, TypeDefinition definition, GenericContext parentGenericContext) : base(reader, definition, parentGenericContext)
            {
            }

            private MethodSignature<MetadataTypeReference>? invokeMethodSignature;
            private MethodSignature<MetadataTypeReference> GetInvokeMethodSignature()
            {
                if (invokeMethodSignature == null)
                {
                    foreach (var handle in Definition.GetMethods())
                    {
                        var method = Reader.GetMethodDefinition(handle);
                        if (Reader.GetString(method.Name) == "Invoke")
                        {
                            invokeMethodSignature = method.DecodeSignature(SignatureTypeProvider.Instance, GenericContext);
                            break;
                        }
                    }
                }
                return invokeMethodSignature.Value;
            }

            public MetadataTypeReference ReturnType => GetInvokeMethodSignature().ReturnType;
        }
    }
}
