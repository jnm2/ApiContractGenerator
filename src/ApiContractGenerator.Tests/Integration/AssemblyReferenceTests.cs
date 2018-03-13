using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using ApiContractGenerator.AssemblyReferenceResolvers;
using ApiContractGenerator.Tests.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class AssemblyReferenceTests : IntegrationTests
    {
        [TestCase(AssemblyFlags.Retargetable)]
        [TestCase(AssemblyFlags.EnableJitCompileTracking)]
        [TestCase(AssemblyFlags.DisableJitCompileOptimizer)]
        public static void Assembly_reference_with_flag(AssemblyFlags flag)
        {
            var builder = BuildAssembly();
            var baseType = typeof(AttributeTargets);
            var baseTypeAssemblyName = baseType.Assembly.GetName();

            var assemblyReferenceHandle = builder.AddAssemblyReference(
                builder.GetOrAddString(baseTypeAssemblyName.Name),
                baseTypeAssemblyName.Version,
                builder.GetOrAddString(baseTypeAssemblyName.CultureName),
                builder.GetOrAddBlob((baseTypeAssemblyName.Flags & AssemblyNameFlags.PublicKey) != 0 ? baseTypeAssemblyName.GetPublicKey() : baseTypeAssemblyName.GetPublicKeyToken()),
                (AssemblyFlags)baseTypeAssemblyName.Flags | flag,
                hashValue: default);

            var typeReferenceHandle = builder.AddTypeReference(
                assemblyReferenceHandle,
                builder.GetOrAddString(baseType.Namespace),
                builder.GetOrAddString(baseType.Name));

            var fieldTypeBlob = new BlobBuilder();
            var fieldTypeEncoder = new BlobEncoder(fieldTypeBlob).FieldSignature();
            fieldTypeEncoder.Type(typeReferenceHandle, isValueType: false);

            var fieldHandle = builder.AddFieldDefinition(
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal,
                builder.GetOrAddString("TestField"),
                builder.GetOrAddBlob(fieldTypeBlob));

            builder.AddConstant(fieldHandle, null);

            builder.AddTypeDefinition(
                TypeAttributes.Public,
                @namespace: default,
                name: builder.GetOrAddString("TestClass"),
                baseType: default,
                fieldList: fieldHandle,
                methodList: default);

            Assert.That(builder, HasContract(
                "public class TestClass",
                "{",
                "    public const System.AttributeTargets TestField = default;",
                "}"));
        }

        [Test]
        public static void Type_forwarded_twice()
        {
            using (var temp = new TempDirectory())
            {
                var baseCompilation = CSharpCompilation.Create(null, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(AssemblyUtils.CorlibReference);

                var enumDeclaration = CSharpSyntaxTree.ParseText("public enum ForwardedType { SomeValue = 1 }");
                var typeForwardDeclaration = CSharpSyntaxTree.ParseText("[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(ForwardedType))]");

                var pathA = Path.Combine(temp, "a.dll");
                using (var fileA = File.Create(pathA))
                {
                    AssemblyUtils.EmitCompilation(
                        baseCompilation.WithAssemblyName("A").AddSyntaxTrees(enumDeclaration),
                        fileA);
                }

                var pathB = Path.Combine(temp, "b.dll");
                using (var fileB = File.Create(pathB))
                {
                    AssemblyUtils.EmitCompilation(
                        baseCompilation
                            .WithAssemblyName("B")
                            .AddReferences(MetadataReference.CreateFromFile(pathA))
                            .AddSyntaxTrees(typeForwardDeclaration),
                        fileB);
                }

                var originalB = baseCompilation
                    .WithAssemblyName("B")
                    .AddSyntaxTrees(enumDeclaration);

                var pathC = Path.Combine(temp, "c.dll");
                using (var fileC = File.Create(pathC))
                {
                    AssemblyUtils.EmitCompilation(
                        baseCompilation
                            .WithAssemblyName("C")
                            .AddReferences(originalB.ToMetadataReference())
                            .AddSyntaxTrees(typeForwardDeclaration),
                        fileC);
                }

                var originalC = baseCompilation
                    .WithAssemblyName("C")
                    .AddSyntaxTrees(enumDeclaration);

                Assert.That(baseCompilation
                        .AddReferences(originalC.ToMetadataReference())
                        .AddSyntaxTrees(CSharpSyntaxTree.ParseText("public static class Test { public const ForwardedType Value = ForwardedType.SomeValue; }")),
                    HasContract(
                        "public static class Test",
                        "{",
                        "    public const ForwardedType Value = ForwardedType.SomeValue;",
                        "}"
                    ).WithAssemblyResolver(new SameDirectoryAssemblyReferenceResolver(temp)));
            }
        }

        [Test]
        public static void Nested_type_forward()
        {
            using (var temp = new TempDirectory())
            {
                var baseCompilation = CSharpCompilation.Create(null, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(AssemblyUtils.CorlibReference);

                var enumDeclaration = CSharpSyntaxTree.ParseText("public static class SomeClass { public enum ForwardedType { SomeValue = 1 } }");
                var typeForwardDeclaration = CSharpSyntaxTree.ParseText("[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(SomeClass))]");

                var pathA = Path.Combine(temp, "a.dll");
                using (var fileA = File.Create(pathA))
                {
                    AssemblyUtils.EmitCompilation(
                        baseCompilation.WithAssemblyName("A").AddSyntaxTrees(enumDeclaration),
                        fileA);
                }

                var pathB = Path.Combine(temp, "b.dll");
                using (var fileB = File.Create(pathB))
                {
                    AssemblyUtils.EmitCompilation(
                        baseCompilation
                            .WithAssemblyName("B")
                            .AddReferences(MetadataReference.CreateFromFile(pathA))
                            .AddSyntaxTrees(typeForwardDeclaration),
                        fileB);
                }

                var originalB = baseCompilation
                    .WithAssemblyName("B")
                    .AddSyntaxTrees(enumDeclaration);

                Assert.That(baseCompilation
                    .AddReferences(originalB.ToMetadataReference())
                    .AddSyntaxTrees(CSharpSyntaxTree.ParseText("public static class Test { public const SomeClass.ForwardedType Value = SomeClass.ForwardedType.SomeValue; }")),
                    HasContract(
                        "public static class Test",
                        "{",
                        "    public const SomeClass.ForwardedType Value = SomeClass.ForwardedType.SomeValue;",
                        "}"
                    ).WithAssemblyResolver(new SameDirectoryAssemblyReferenceResolver(temp)));
            }
        }
    }
}
