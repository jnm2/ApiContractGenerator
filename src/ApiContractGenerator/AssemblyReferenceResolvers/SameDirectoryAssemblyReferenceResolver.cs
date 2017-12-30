using System;
using System.IO;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.AssemblyReferenceResolvers
{
    public sealed class SameDirectoryAssemblyReferenceResolver : IAssemblyReferenceResolver
    {
        private readonly string baseDirectory;

        public SameDirectoryAssemblyReferenceResolver(string baseDirectory)
        {
            this.baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
        }

        public bool TryGetAssemblyPath(MetadataAssemblyReference assemblyReference, out string path)
        {
            if (assemblyReference != null)
            {
                var baseDirectoryPath = Path.Combine(baseDirectory, assemblyReference.Name + ".dll");

                if (File.Exists(baseDirectoryPath))
                {
                    path = baseDirectoryPath;
                    return true;
                }
            }

            path = null;
            return false;
        }
    }
}
