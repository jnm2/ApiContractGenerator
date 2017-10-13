using System;
using System.IO;
using ApiContractGenerator.AssemblyReferenceResolvers;
using ApiContractGenerator.MetadataReferenceResolvers;
using ApiContractGenerator.Source;

namespace ApiContractGenerator.Console
{
    using Console = System.Console;

    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: <assembly path> <output path>");
                return 1;
            }

            try
            {
                Main(args[0], args[1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }

        public static void Main(string assemblyPath, string outputPath)
        {
            var generator = new ApiContractGenerator
            {
                IgnoredNamespaces = { "NUnit.Framework.Internal" }
            };

            var assemblyResolver = new CompositeAssemblyReferenceResolver(
                new GacAssemblyReferenceResolver(),
                new SameDirectoryAssemblyReferenceResolver(Path.GetDirectoryName(assemblyPath)),
                new SameDirectoryAssemblyReferenceResolver(typeof(object).Assembly.Location));

            using (var metadataReferenceResolver = new MetadataReaderReferenceResolver(() => File.OpenRead(assemblyPath), assemblyResolver))
            using (var source = new MetadataReaderSource(File.OpenRead(assemblyPath), metadataReferenceResolver))
            using (var outputFile = File.CreateText(outputPath))
                generator.Generate(source, new CSharpTextFormatter(outputFile, metadataReferenceResolver));
        }
    }
}
