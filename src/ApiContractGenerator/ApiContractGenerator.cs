using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using ApiContractGenerator.MetadataReferenceResolvers;
using ApiContractGenerator.Source;
using Microsoft.CodeAnalysis;

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

        public string GenerateApiDocument(ImmutableArray<TargetFrameworkAssembly> buildsOfProject)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GenerateApiDocumentAsync(Project project, CancellationToken cancellationToken)
        {
            var buildsOfProject = await TargetFrameworkAssembly.FromProjectAsync(project, cancellationToken).ConfigureAwait(false);

            return GenerateApiDocument(buildsOfProject);
        }
    }
}
