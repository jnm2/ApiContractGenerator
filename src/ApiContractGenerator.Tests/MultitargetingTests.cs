using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace ApiContractGenerator.Tests
{
    public static class MultitargetingTests
    {
        [Test, Ignore("WIP")]
        public static void Roslyn_assembly_symbols_to_API_document()
        {
            AssertApiDocument(@"
#if NET48
public class A { }
#elif NET47
public class A { }
#else
public class B { }
#endif",
                @"
#if NET47 || NET48
public class A { }
#else
public class B { }
#endif");
        }

        private static void AssertApiDocument(string singleDocumentProject, string expectedApiDocument)
        {
            var actualApiDocument = new ApiContractGenerator()
                .GenerateApiDocument(CreateMultitargetingAssembly(singleDocumentProject));

            Assert.That(actualApiDocument, Is.EqualTo(expectedApiDocument));
        }

        private static ImmutableArray<TargetFrameworkAssembly> CreateMultitargetingAssembly(string singleDocument)
        {
            var syntaxRoot = CSharpSyntaxTree.ParseText(singleDocument).GetRoot();

            var preprocessorSymbols = ImmutableHashSet.CreateBuilder<string>();
            DetectPreprocessorSymbols(syntaxRoot, preprocessorSymbols);

            var projectWithoutSymbols = new AdhocWorkspace()
                .AddProject("Project", LanguageNames.CSharp)
                .AddDocument("Document.cs", syntaxRoot)
                .Project;

            return (
                from symbol in preprocessorSymbols
                let symbols = symbol == string.Empty ? ImmutableArray<string>.Empty : ImmutableArray.Create(symbol)
                let project = projectWithoutSymbols.WithParseOptions(new CSharpParseOptions(preprocessorSymbols: symbols))
                let assembly = project.GetCompilationAsync().GetAwaiter().GetResult().Assembly
                select new TargetFrameworkAssembly(assembly, symbols)
            ).ToImmutableArray();
        }

        private static void DetectPreprocessorSymbols(SyntaxNode syntaxRoot, ImmutableHashSet<string>.Builder builder)
        {
            foreach (var trivia in syntaxRoot.DescendantTrivia())
            {
                switch (trivia.Kind())
                {
                    case SyntaxKind.IfDirectiveTrivia:
                        DetectPreprocessorSymbols(((IfDirectiveTriviaSyntax)trivia.GetStructure()).Condition, builder);
                        break;
                    case SyntaxKind.ElifDirectiveTrivia:
                        DetectPreprocessorSymbols(((ElifDirectiveTriviaSyntax)trivia.GetStructure()).Condition, builder);
                        break;
                    case SyntaxKind.ElseDirectiveTrivia:
                        builder.Add(string.Empty);
                        break;
                }
            }
        }

        private static void DetectPreprocessorSymbols(ExpressionSyntax preprocessorCondition, ImmutableHashSet<string>.Builder builder)
        {
            new IdentifierNameWalker(builder).Visit(preprocessorCondition);
        }

        private sealed class IdentifierNameWalker : CSharpSyntaxWalker
        {
            private readonly ImmutableHashSet<string>.Builder builder;

            public IdentifierNameWalker(ImmutableHashSet<string>.Builder builder)
            {
                this.builder = builder;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                builder.Add(node.Identifier.ValueText);
            }
        }
    }
}
