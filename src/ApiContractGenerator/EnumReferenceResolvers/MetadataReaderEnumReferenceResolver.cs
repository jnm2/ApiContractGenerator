using System;
using System.Collections.Generic;
using System.Reflection;
using ApiContractGenerator.AssemblyReferenceResolvers;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.EnumReferenceResolvers
{
    public sealed partial class MetadataReaderEnumReferenceResolver : IEnumReferenceResolver, IDisposable
    {
        private readonly IAssemblyReferenceResolver assemblyResolver;
        private readonly Dictionary<string, AssemblyLazyLoader> assemblyLoadersByFullName = new Dictionary<string, AssemblyLazyLoader>();

        public MetadataReaderEnumReferenceResolver(IAssemblyReferenceResolver assemblyResolver)
        {
            this.assemblyResolver = assemblyResolver;
        }

        public bool TryGetEnumInfo(MetadataTypeReference typeReference, out EnumInfo info)
        {
            var (assemblyName, typeName) = NameSpec.FromMetadataTypeReference(typeReference);

            if (assemblyName == null) assemblyName = new AssemblyName("mscorlib");

            var fullName = assemblyName.FullName;
            if (!assemblyLoadersByFullName.TryGetValue(fullName, out var loader))
            {
                loader = assemblyResolver.TryGetAssemblyPath(assemblyName, out var path) ? new AssemblyLazyLoader(path) : null;
                assemblyLoadersByFullName.Add(fullName, loader);
            }

            if (loader == null)
            {
                // We couldn't locate the assembly.
                info = default(EnumInfo);
                return false;
            }

            return loader.TryGetEnumInfo(typeName, out info);
        }

        public void Dispose()
        {
            foreach (var loader in assemblyLoadersByFullName.Values)
                loader?.Dispose();
            assemblyLoadersByFullName.Clear();
        }
    }
}
