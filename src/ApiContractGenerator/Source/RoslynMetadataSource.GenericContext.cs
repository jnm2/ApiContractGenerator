using System.Collections.Generic;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource
    {
        private struct GenericContext
        {
            public readonly GenericParameterTypeReference[] TypeParameters;

            public GenericContext(GenericParameterTypeReference[] typeParameters)
            {
                TypeParameters = typeParameters;
            }

            public GenericContext ChildContext(GenericParameterTypeReference[] childParameters)
            {
                if (TypeParameters == null) return new GenericContext(childParameters);

                var r = new GenericParameterTypeReference[TypeParameters.Length + childParameters.Length];
                TypeParameters.CopyTo(r, 0);
                childParameters.CopyTo(r, TypeParameters.Length);
                return new GenericContext(r);
            }
        }
    }
}
