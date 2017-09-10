using System;
using System.Reflection.Metadata;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private struct GenericContext
        {
            public readonly GenericParameterTypeReference[] TypeParameters;
            public readonly GenericParameterTypeReference[] MethodParameters;

            public GenericContext(GenericParameterTypeReference[] typeParameters, GenericParameterTypeReference[] methodParameters)
            {
                TypeParameters = typeParameters;
                MethodParameters = methodParameters;
            }
        }

        public static GenericParameterTypeReference[] GetGenericParameters(MetadataReader reader, GenericParameterHandleCollection genericParameters)
        {
            if (genericParameters.Count == 0) return Array.Empty<GenericParameterTypeReference>();

            var r = new GenericParameterTypeReference[genericParameters.Count];

            foreach (var handle in genericParameters)
            {
                var genericParameter = reader.GetGenericParameter(handle);
                r[genericParameter.Index] = new GenericParameterTypeReference(reader.GetString(genericParameter.Name));
            }

            return r;
        }
    }
}
