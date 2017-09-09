using System;
using System.Reflection.Metadata;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class GenericContext
        {
            public static readonly GenericContext Empty = new GenericContext(null, Array.Empty<GenericParameterTypeReference>());

            private readonly GenericContext parentContext;
            private readonly int parentTotalParameterCount;
            public readonly GenericParameterTypeReference[] TypeParameters;

            public GenericContext(GenericContext parentContext, GenericParameterTypeReference[] typeParameters)
            {
                if (parentContext != null)
                {
                    this.parentContext = parentContext;
                    parentTotalParameterCount = parentContext.parentTotalParameterCount + this.parentContext.TypeParameters.Length;
                }
                TypeParameters = typeParameters;
            }

            public GenericParameterTypeReference this[int absoluteIndex] =>
                absoluteIndex < parentTotalParameterCount
                    ? parentContext[absoluteIndex]
                    : TypeParameters[absoluteIndex - parentTotalParameterCount];

            public static GenericContext Create(MetadataReader reader, GenericContext parentContext, GenericParameterHandleCollection genericParameters)
            {
                if (genericParameters.Count == 0) return parentContext;

                var childParameters = new GenericParameterTypeReference[genericParameters.Count];
                foreach (var handle in genericParameters)
                {
                    var genericParameter = reader.GetGenericParameter(handle);
                    childParameters[genericParameter.Index] = new GenericParameterTypeReference(reader.GetString(genericParameter.Name));
                }
                return new GenericContext(parentContext, childParameters);
            }
        }
    }
}
