using System.IO;
using System.Reflection;
using ApiContractGenerator.AssemblyReferenceResolvers;
using ApiContractGenerator.MetadataReferenceResolvers;
using ApiContractGenerator.Source;
using Microsoft.Build.Framework;

namespace ApiContractGenerator.MSBuild
{
    public sealed class GenerateApiContract : ITask
    {
        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

        [Required]
        public ITaskItem[] Assemblies { get; set; }

        public string[] IgnoredNamespaces { get; set; }

        public bool Execute()
        {
            if (Assemblies == null || Assemblies.Length == 0) return true;

            var generator = new ApiContractGenerator();
            if (IgnoredNamespaces != null) generator.IgnoredNamespaces.UnionWith(IgnoredNamespaces);

            foreach (var assembly in Assemblies)
            {
                var assemblyPath = assembly.GetMetadata("ResolvedAssemblyPath");
                var outputPath = assembly.GetMetadata("ResolvedOutputPath");
                if (!string.IsNullOrEmpty(outputPath)) Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                var assemblyResolver = new CompositeAssemblyReferenceResolver(
                    new GacAssemblyReferenceResolver(),
                    new SameDirectoryAssemblyReferenceResolver(Path.GetDirectoryName(assemblyPath)),
                    new SameDirectoryAssemblyReferenceResolver(Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location)));

                using (var metadataReferenceResolver = new MetadataReaderReferenceResolver(() => File.OpenRead(assemblyPath), assemblyResolver))
                using (var source = new MetadataReaderSource(File.OpenRead(assemblyPath), metadataReferenceResolver))
                using (var outputFile = File.CreateText(outputPath))
                    generator.Generate(source, metadataReferenceResolver, new CSharpTextFormatter(outputFile, metadataReferenceResolver));
            }

            return true;
        }
    }
}
