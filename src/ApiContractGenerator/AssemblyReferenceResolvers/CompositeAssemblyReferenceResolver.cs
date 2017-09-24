using System;
using System.Reflection;

namespace ApiContractGenerator.AssemblyReferenceResolvers
{
    public sealed class CompositeAssemblyReferenceResolver : IAssemblyReferenceResolver
    {
        private readonly IAssemblyReferenceResolver[] resolvers;

        public CompositeAssemblyReferenceResolver(params IAssemblyReferenceResolver[] resolvers)
        {
            this.resolvers = resolvers ?? throw new ArgumentNullException(nameof(resolvers));
        }

        public bool TryGetAssemblyPath(AssemblyName assemblyName, out string path)
        {
            foreach (var resolver in resolvers)
                if (resolver.TryGetAssemblyPath(assemblyName, out path))
                    return true;

            path = null;
            return false;
        }
    }
}
