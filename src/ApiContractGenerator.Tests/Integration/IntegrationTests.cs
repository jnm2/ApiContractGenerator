using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using ApiContractGenerator.AssemblyReferenceResolvers;
using ApiContractGenerator.Tests.Utils;
using Microsoft.CodeAnalysis;
using NUnit.Framework.Constraints;

namespace ApiContractGenerator.Tests.Integration
{
    public abstract class IntegrationTests
    {
        public static ContractConstraint HasContract(params string[] lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));

            var newlineLength = Environment.NewLine.Length;
            var bufferSize = 0;
            foreach (var line in lines)
                bufferSize += line.Length + newlineLength;

            var expected = new StringBuilder(bufferSize);
            foreach (var line in lines)
                expected.AppendLine(line);

            return new ContractConstraint(expected.ToString());
        }

        public sealed class ContractConstraint : Constraint
        {
            private readonly ApiContractGenerator generator = new ApiContractGenerator { WriteAssemblyMetadata = false };
            private readonly string expected;
            private bool isVisualBasic;
            private IAssemblyReferenceResolver assemblyResolver;

            public ContractConstraint SourceIsVisualBasic
            {
                get
                {
                    isVisualBasic = true;
                    return this;
                }
            }

            public ContractConstraint IncludeAssemblyMetadata
            {
                get
                {
                    generator.WriteAssemblyMetadata = true;
                    return this;
                }
            }

            public ContractConstraint WithIgnoredNamespace(string @namespace)
            {
                generator.IgnoredNamespaces.Add(@namespace);
                return this;
            }

            public ContractConstraint WithAssemblyResolver(IAssemblyReferenceResolver assemblyResolver)
            {
                this.assemblyResolver = assemblyResolver;
                return this;
            }

            public ContractConstraint(string expected)
            {
                this.expected = expected;
            }

            public override ConstraintResult ApplyTo<TActual>(TActual actual)
            {
                string actualContract;
                switch (actual)
                {
                    case string sourceCode:
                        actualContract = isVisualBasic
                            ? AssemblyUtils.GenerateContract(generator, sourceCode, Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.Latest, assemblyResolver)
                            : AssemblyUtils.GenerateContract(generator, sourceCode, Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest, assemblyResolver);
                        break;

                    case Compilation compilation:
                        using (var stream = new MemoryStream())
                        {
                            AssemblyUtils.EmitCompilation(compilation, stream);
                            stream.Seek(0, SeekOrigin.Begin);
                            actualContract = AssemblyUtils.GenerateContract(generator, stream, assemblyResolver);
                        }
                        break;

                    case MetadataBuilder metadataBuilder:
                        var blobBuilder = new BlobBuilder();

                        var peBuilder = new ManagedPEBuilder(
                            PEHeaderBuilder.CreateLibraryHeader(),
                            new MetadataRootBuilder(metadataBuilder),
                            ilStream: new BlobBuilder());

                        peBuilder.Serialize(blobBuilder);

                        using (var stream = new MemoryStream(blobBuilder.ToArray(), writable: false))
                            actualContract = AssemblyUtils.GenerateContract(generator, stream, assemblyResolver);
                        break;

                    default:
                        throw new ArgumentException("Expected source code as string or library IL as MetadataBuilder.", nameof(actual));
                }

                return new ContractConstraintResult(this, actualContract, expected);
            }

            private sealed class ContractConstraintResult : ConstraintResult
            {
                private readonly string expected;

                public ContractConstraintResult(IConstraint constraint, string actual, string expected) : base(constraint, actual, actual == expected)
                {
                    this.expected = expected;
                }

                public override void WriteMessageTo(MessageWriter writer)
                {
                    writer.Write("Expected: ");
                    writer.WriteLine(expected.Replace(Environment.NewLine, "↵"));
                    writer.Write("But was:  ");
                    writer.WriteLine(((string)ActualValue).Replace(Environment.NewLine, "↵"));
                }
            }
        }

        protected static MetadataBuilder BuildAssembly()
        {
            var builder = new MetadataBuilder();

            var assemblyNameHandle = builder.GetOrAddString(AssemblyUtils.EmittedAssemblyName);

            builder.AddAssembly(
                assemblyNameHandle,
                version: new Version(),
                culture: default,
                publicKey: default,
                flags: default,
                hashAlgorithm: AssemblyHashAlgorithm.None);

            builder.AddModule(
                generation: 0,
                moduleName: assemblyNameHandle,
                mvid: default,
                encId: default,
                encBaseId: default);

            return builder;
        }
    }
}
