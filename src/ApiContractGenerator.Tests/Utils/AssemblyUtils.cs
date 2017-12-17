using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ApiContractGenerator.AssemblyReferenceResolvers;
using ApiContractGenerator.MetadataReferenceResolvers;
using ApiContractGenerator.Source;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace ApiContractGenerator.Tests.Utils
{
    public static class AssemblyUtils
    {
        // Cached for performance
        private static readonly MetadataReference[] MetadataReferences =
        {
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Diagnostics.DebuggerStepperBoundaryAttribute).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute).GetTypeInfo().Assembly.Location)
        };

        private static readonly Lazy<CSharpCompilation> BaseCSharpCompilation = new Lazy<CSharpCompilation>(() =>
            CSharpCompilation.Create(
                assemblyName: $"{typeof(AssemblyUtils).FullName}.{nameof(EmitCompilation)}",
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    allowUnsafe: true),
                references: MetadataReferences));

        private static readonly Lazy<VisualBasicCompilation> BaseVisualBasicCompilation = new Lazy<VisualBasicCompilation>(() =>
            VisualBasicCompilation.Create(
                assemblyName: $"{typeof(AssemblyUtils).FullName}.{nameof(EmitCompilation)}",
                options: new VisualBasicCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release),
                references: MetadataReferences));

        public static void EmitCompilation(string sourceCode, Stream target, Microsoft.CodeAnalysis.CSharp.LanguageVersion languageVersion)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var compilation = BaseCSharpCompilation.Value.AddSyntaxTrees(CSharpSyntaxTree.ParseText(sourceCode, new CSharpParseOptions(languageVersion)));

            var emitResult = compilation.Emit(target);

            if (!emitResult.Success)
            {
                throw new InvalidOperationException(
                    "Code does not compile: " + string.Concat(emitResult.Diagnostics.Select(d => Environment.NewLine + d.GetMessage())));
            }
        }

        public static void EmitCompilation(string sourceCode, Stream target, Microsoft.CodeAnalysis.VisualBasic.LanguageVersion languageVersion)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var compilation = BaseVisualBasicCompilation.Value.AddSyntaxTrees(VisualBasicSyntaxTree.ParseText(sourceCode, new VisualBasicParseOptions(languageVersion)));

            var emitResult = compilation.Emit(target);

            if (!emitResult.Success)
            {
                throw new InvalidOperationException(
                    "Code does not compile: " + string.Concat(emitResult.Diagnostics.Select(d => Environment.NewLine + d.GetMessage())));
            }
        }

        public static string GenerateContract(string sourceCode, ApiContractGenerator generator, Microsoft.CodeAnalysis.CSharp.LanguageVersion languageVersion)
        {
            using (var writer = new StringWriter())
            using (var stream = new MemoryStream())
            {
                EmitCompilation(sourceCode, stream, languageVersion);
                return GenerateContract(generator, writer, stream);
            }
        }

        public static string GenerateContract(string sourceCode, ApiContractGenerator generator, Microsoft.CodeAnalysis.VisualBasic.LanguageVersion languageVersion)
        {
            using (var writer = new StringWriter())
            using (var stream = new MemoryStream())
            {
                EmitCompilation(sourceCode, stream, languageVersion);
                return GenerateContract(generator, writer, stream);
            }
        }

        private static string GenerateContract(ApiContractGenerator generator, StringWriter writer, MemoryStream assemblyStream)
        {
            assemblyStream.Seek(0, SeekOrigin.Begin);

            var assemblyResolver = new CompositeAssemblyReferenceResolver(
                new GacAssemblyReferenceResolver(),
                new SameDirectoryAssemblyReferenceResolver(Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location)));

            using (var metadataReferenceResolver = new MetadataReaderReferenceResolver(() => assemblyStream.CreateReadOnlyView(), assemblyResolver))
            using (var source = new MetadataReaderSource(assemblyStream, metadataReferenceResolver))
                generator.Generate(source, metadataReferenceResolver, new CSharpTextFormatter(writer, metadataReferenceResolver));

            return writer.ToString();
        }
    }
}
