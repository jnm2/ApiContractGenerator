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
        private readonly string currentAssemblyPath;
        private AssemblyLazyLoader currentAssemblyLoader;
        private AssemblyName currentAssemblyMscorlibReference;
        private readonly IAssemblyReferenceResolver assemblyResolver;

        private readonly Dictionary<string, AssemblyLazyLoader> assemblyLoadersByFullName = new Dictionary<string, AssemblyLazyLoader>();

        public MetadataReaderReferenceResolver(string currentAssemblyPath, IAssemblyReferenceResolver assemblyResolver)
        {
            this.currentAssemblyPath = currentAssemblyPath ?? throw new ArgumentNullException(nameof(currentAssemblyPath));
            this.assemblyResolver = assemblyResolver ?? throw new ArgumentNullException(nameof(assemblyResolver));
        }

        public bool TryGetEnumInfo(MetadataTypeReference typeReference, out EnumInfo info)
        {
            var (assemblyName, typeName) = NameSpec.FromMetadataTypeReference(typeReference);

            if (assemblyName == null)
            {
                if (currentAssemblyLoader == null)
                    currentAssemblyLoader = new AssemblyLazyLoader(currentAssemblyPath);

                if (currentAssemblyLoader.TryGetEnumInfo(typeName, out info))
                    return true;

                // Attribute values containing enum type references serialize the string.
                // So far all mscorlib enum references I've seen include the assembly name,
                // but this is just in case.

                if (currentAssemblyMscorlibReference == null)
                    currentAssemblyMscorlibReference = GetMscorlibReference(currentAssemblyPath);

                assemblyName = currentAssemblyMscorlibReference;
            }

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

        private static AssemblyName GetMscorlibReference(string assemblyPath)
        {
            using (var peReader = new PEReader(File.OpenRead(assemblyPath)))
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
