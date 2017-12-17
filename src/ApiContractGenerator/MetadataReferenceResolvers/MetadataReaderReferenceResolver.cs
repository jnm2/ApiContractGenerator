using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ApiContractGenerator.AssemblyReferenceResolvers;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.MetadataReferenceResolvers
{
    public sealed partial class MetadataReaderReferenceResolver : IMetadataReferenceResolver, IDisposable
    {
        private readonly Func<Stream> currentAssemblyStreamFactory;
        private AssemblyLazyLoader currentAssemblyLoader;
        private AssemblyName currentAssemblyMscorlibReference;
        private readonly IAssemblyReferenceResolver assemblyResolver;

        private readonly Dictionary<string, AssemblyLazyLoader> assemblyLoadersByFullName = new Dictionary<string, AssemblyLazyLoader>();

        public MetadataReaderReferenceResolver(Func<Stream> currentAssemblyStreamFactory, IAssemblyReferenceResolver assemblyResolver)
        {
            this.currentAssemblyStreamFactory = currentAssemblyStreamFactory ?? throw new ArgumentNullException(nameof(currentAssemblyStreamFactory));
            this.assemblyResolver = assemblyResolver ?? throw new ArgumentNullException(nameof(assemblyResolver));
        }

        public bool TryGetEnumInfo(MetadataTypeReference typeReference, out EnumInfo info)
        {
            if (TryGetCachedInfo(typeReference, out var cachedInfo) && cachedInfo.EnumInfo != null)
            {
                info = cachedInfo.EnumInfo.Value;
                return true;
            }
            info = default;
            return false;
        }

        public bool TryGetIsValueType(MetadataTypeReference typeReference, out bool isValueType)
        {
            if (TryGetCachedInfo(typeReference, out var cachedInfo))
            {
                isValueType = cachedInfo.IsValueType;
                return true;
            }
            isValueType = default;
            return false;
        }

        public bool TryGetIsDelegateType(MetadataTypeReference typeReference, out bool isDelegateType)
        {
            if (TryGetCachedInfo(typeReference, out var cachedInfo))
            {
                isDelegateType = cachedInfo.IsDelegateType;
                return true;
            }
            isDelegateType = default;
            return false;
        }

        private bool TryGetCachedInfo(MetadataTypeReference typeReference, out CachedInfo cachedInfo)
        {
            var (assemblyName, typeName) = NameSpec.FromMetadataTypeReference(typeReference);

            if (assemblyName == null)
            {
                if (currentAssemblyLoader == null)
                    currentAssemblyLoader = new AssemblyLazyLoader(currentAssemblyStreamFactory.Invoke());

                if (currentAssemblyLoader.TryGetInfo(typeName, out cachedInfo))
                    return true;

                // Attribute values containing enum type references serialize the string.
                // So far all mscorlib enum references I've seen include the assembly name,
                // but this is just in case.

                if (currentAssemblyMscorlibReference == null)
                    currentAssemblyMscorlibReference = GetMscorlibReference(currentAssemblyStreamFactory.Invoke());

                assemblyName = currentAssemblyMscorlibReference;
            }

            var fullName = assemblyName.FullName;
            if (!assemblyLoadersByFullName.TryGetValue(fullName, out var loader))
            {
                loader = assemblyResolver.TryGetAssemblyPath(assemblyName, out var path) ? new AssemblyLazyLoader(File.OpenRead(path)) : null;
                assemblyLoadersByFullName.Add(fullName, loader);
            }

            if (loader == null)
            {
                // We couldn't locate the assembly.
                cachedInfo = default;
                return false;
            }

            return loader.TryGetInfo(typeName, out cachedInfo);
        }

        private static AssemblyName GetMscorlibReference(Stream assemblyStream)
        {
            using (var peReader = new PEReader(assemblyStream))
            {
                var reader = peReader.GetMetadataReader();

                foreach (var handle in reader.AssemblyReferences)
                {
                    var reference = reader.GetAssemblyReference(handle);
                    if ("mscorlib".Equals(reader.GetString(reference.Name), StringComparison.OrdinalIgnoreCase))
                        return reference.GetAssemblyName(reader);
                }
            }

            throw new NotImplementedException();
        }

        public void Dispose()
        {
            currentAssemblyLoader?.Dispose();
            currentAssemblyLoader = null;

            foreach (var loader in assemblyLoadersByFullName.Values)
                loader?.Dispose();
            assemblyLoadersByFullName.Clear();
        }
    }
}
