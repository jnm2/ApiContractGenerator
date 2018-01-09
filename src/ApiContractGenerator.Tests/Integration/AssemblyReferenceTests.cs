using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using ApiContractGenerator.Tests.Utils;
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
    }
}
