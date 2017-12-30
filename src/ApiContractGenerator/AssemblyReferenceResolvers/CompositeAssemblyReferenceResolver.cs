using System;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.AssemblyReferenceResolvers
{
    public sealed class CompositeAssemblyReferenceResolver : IAssemblyReferenceResolver
    {
        private readonly IAssemblyReferenceResolver[] resolvers;

        public CompositeAssemblyReferenceResolver(params IAssemblyReferenceResolver[] resolvers)
        {
            this.resolvers = resolvers ?? throw new ArgumentNullException(nameof(resolvers));
        }

        public bool TryGetAssemblyPath(MetadataAssemblyReference assemblyReference, out string path)
        {
            foreach (var resolver in resolvers)
                if (resolver.TryGetAssemblyPath(assemblyReference, out path))
                    return true;

            path = null;
            return false;
        }
    }
}
