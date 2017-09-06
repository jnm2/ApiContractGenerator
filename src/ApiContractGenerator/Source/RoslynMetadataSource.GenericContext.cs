using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource
    {
        private sealed class GenericContext
        {
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
        }
    }
}
