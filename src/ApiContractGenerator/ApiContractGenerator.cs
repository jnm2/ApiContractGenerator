using System;
using System.Collections.Generic;
using ApiContractGenerator.MetadataReferenceResolvers;
using ApiContractGenerator.Source;

namespace ApiContractGenerator
{
    public sealed class ApiContractGenerator
    {
        public ISet<string> IgnoredNamespaces { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool WriteAssemblyMetadata { get; set; } = true;

        public void Generate(IMetadataSource source, IMetadataReferenceResolver metadataReferenceResolver, IMetadataWriter writer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.Write(new IgnoredNamespaceFilter(source, IgnoredNamespaces, metadataReferenceResolver), WriteAssemblyMetadata);
        }
    }
}
