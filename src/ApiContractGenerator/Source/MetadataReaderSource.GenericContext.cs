using System;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private struct GenericContext
        {
            public readonly IMetadataGenericTypeParameter[] TypeParameters;
            public readonly IMetadataGenericTypeParameter[] MethodParameters;

            private GenericContext(IMetadataGenericTypeParameter[] typeParameters, IMetadataGenericTypeParameter[] methodParameters)
            {
                // ReaderGenericTypeParameter's constructor relies on the arrays we pass in being the arrays we expose.
                // See note in FillArray.
                TypeParameters = typeParameters;
                MethodParameters = methodParameters;
            }

            private static void FillArray(IMetadataGenericTypeParameter[] array, GenericContext genericContext, MetadataReader reader, GenericParameterHandleCollection genericParameters)
            {
                foreach (var handle in genericParameters)
                {
                    var genericParameter = reader.GetGenericParameter(handle);

                    // It's okay to pass in genericContext as we fill the array.
                    // It's coupling to an implementation detail in the same class.
                    array[genericParameter.Index] = new ReaderGenericTypeParameter(reader, genericParameter, genericContext);
                }
            }

            public static GenericContext FromType(MetadataReader reader, TypeDefinition definition)
            {
                var empty = Array.Empty<IMetadataGenericTypeParameter>();
                var genericParameters = definition.GetGenericParameters();
                if (genericParameters.Count == 0) return new GenericContext(empty, empty);

                var r = new IMetadataGenericTypeParameter[genericParameters.Count];
                var genericContext = new GenericContext(r, empty);
                FillArray(r, genericContext, reader, genericParameters);

                return genericContext;
            }

            public static GenericContext FromMethod(MetadataReader reader, GenericContext declaringTypeGenericContext, MethodDefinition definition)
            {
                var genericParameters = definition.GetGenericParameters();
                if (genericParameters.Count == 0) return declaringTypeGenericContext;

                var r = new IMetadataGenericTypeParameter[genericParameters.Count];
                var genericContext = new GenericContext(declaringTypeGenericContext.TypeParameters, r);
                FillArray(r, genericContext, reader, genericParameters);

                return genericContext;
            }
        }
    }
}
