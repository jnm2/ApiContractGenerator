using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ApiContractGenerator.AssemblyReferenceResolvers;
using ApiContractGenerator.MetadataReferenceResolvers;
using ApiContractGenerator.Source;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ApiContractGenerator.Tests.Utils
{
    public static class AssemblyUtils
    {
        public static void EmitCompilation(string sourceCode, Stream target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var compilation = CSharpCompilation.Create(
                assemblyName: $"{typeof(AssemblyUtils).FullName}.{nameof(EmitCompilation)}",
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    allowUnsafe: true),
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(sourceCode) },
                references: new[] { MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location) });

            var emitResult = compilation.Emit(target);

            if (!emitResult.Success)
            {
                throw new InvalidOperationException(
                    "Code does not compile: " + string.Concat(emitResult.Diagnostics.Select(d => Environment.NewLine + d.GetMessage())));
            }
        }

        public static string GenerateContract(string sourceCode)
        {
            return GenerateContract(sourceCode, new ApiContractGenerator());
        }

        public static string GenerateContract(string sourceCode, ApiContractGenerator generator)
        {
            using (var writer = new StringWriter())
            {
                using (var stream = new MemoryStream())
                {
                    EmitCompilation(sourceCode, stream);

                    stream.Seek(0, SeekOrigin.Begin);

                    using (var metadataReferenceResolver = new MetadataReaderReferenceResolver(() => stream.CreateReadOnlyView(), new GacAssemblyReferenceResolver()))
                    using (var source = new MetadataReaderSource(stream, metadataReferenceResolver))
                        generator.Generate(source, new CSharpTextFormatter(writer, metadataReferenceResolver));
                }

                return writer.ToString();
            }
        }
    }
}
