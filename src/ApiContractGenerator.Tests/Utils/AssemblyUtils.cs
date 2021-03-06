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
        internal static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location);

        // Cached for performance
        private static readonly MetadataReference[] MetadataReferences =
        {
            CorlibReference,
            MetadataReference.CreateFromFile(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Diagnostics.DebuggerStepperBoundaryAttribute).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute).GetTypeInfo().Assembly.Location)
        };

        public static string EmittedAssemblyName { get; } = $"{typeof(AssemblyUtils).FullName}.{nameof(EmitCompilation)}";

        private static readonly Lazy<CSharpCompilation> BaseCSharpCompilation = new Lazy<CSharpCompilation>(() =>
            CSharpCompilation.Create(
                EmittedAssemblyName,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    allowUnsafe: true),
                references: MetadataReferences));

        private static readonly Lazy<VisualBasicCompilation> BaseVisualBasicCompilation = new Lazy<VisualBasicCompilation>(() =>
            VisualBasicCompilation.Create(
                EmittedAssemblyName,
                options: new VisualBasicCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release),
                references: MetadataReferences));

        public static void EmitCompilation(string sourceCode, Stream target, Microsoft.CodeAnalysis.CSharp.LanguageVersion languageVersion)
        {
            EmitCompilation(
                BaseCSharpCompilation.Value.AddSyntaxTrees(CSharpSyntaxTree.ParseText(sourceCode, new CSharpParseOptions(languageVersion))),
                target);
        }

        public static void EmitCompilation(string sourceCode, Stream target, Microsoft.CodeAnalysis.VisualBasic.LanguageVersion languageVersion)
        {
            EmitCompilation(
                BaseVisualBasicCompilation.Value.AddSyntaxTrees(VisualBasicSyntaxTree.ParseText(sourceCode, new VisualBasicParseOptions(languageVersion))),
                target);
        }

        public static void EmitCompilation(Compilation compilation, Stream target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var emitResult = compilation.Emit(target);

            if (!emitResult.Success)
            {
                throw new InvalidOperationException(
                    "Code does not compile: " + string.Concat(emitResult.Diagnostics.Select(d => Environment.NewLine + d.GetMessage())));
            }
        }

        public static string GenerateContract(ApiContractGenerator generator, MemoryStream peStream, IAssemblyReferenceResolver assemblyResolver = null)
        {
            using var writer = new StringWriter();
            return GenerateContract(generator, writer, peStream, assemblyResolver);
        }

        public static string GenerateContract(ApiContractGenerator generator, string sourceCode, Microsoft.CodeAnalysis.CSharp.LanguageVersion languageVersion, IAssemblyReferenceResolver assemblyResolver = null)
        {
            using var stream = new MemoryStream();
            EmitCompilation(sourceCode, stream, languageVersion);
            return GenerateContract(generator, stream, assemblyResolver);
        }

        public static string GenerateContract(ApiContractGenerator generator, string sourceCode, Microsoft.CodeAnalysis.VisualBasic.LanguageVersion languageVersion, IAssemblyReferenceResolver assemblyResolver = null)
        {
            using var stream = new MemoryStream();
            EmitCompilation(sourceCode, stream, languageVersion);
            return GenerateContract(generator, stream, assemblyResolver);
        }

        private static string GenerateContract(ApiContractGenerator generator, StringWriter writer, MemoryStream assemblyStream, IAssemblyReferenceResolver assemblyResolver = null)
        {
            assemblyStream.Seek(0, SeekOrigin.Begin);

            if (assemblyResolver == null) assemblyResolver = new CompositeAssemblyReferenceResolver(
                new GacAssemblyReferenceResolver(),
                new SameDirectoryAssemblyReferenceResolver(Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location)));

            using (var metadataReferenceResolver = new MetadataReaderReferenceResolver(() => assemblyStream.CreateReadOnlyView(), assemblyResolver))
            using (var source = new MetadataReaderSource(assemblyStream, metadataReferenceResolver))
                generator.Generate(source, metadataReferenceResolver, new CSharpTextFormatter(writer, metadataReferenceResolver));

            return writer.ToString();
        }
    }
}
