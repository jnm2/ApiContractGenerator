using System;
using System.Text;
using ApiContractGenerator.Tests.Utils;
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
            private readonly string expected;
            private bool isVisualBasic;

            public ContractConstraint SourceIsVisualBasic
            {
                get
                {
                    isVisualBasic = true;
                    return this;
                }
            }

            public ContractConstraint(string expected)
            {
                this.expected = expected;
            }

            public override ConstraintResult ApplyTo<TActual>(TActual actual)
            {
                if (!((object)actual is string sourceCode))
                    throw new ArgumentException("Expected source code as string.", nameof(actual));

                var actualContract = isVisualBasic
                    ? AssemblyUtils.GenerateContract(sourceCode, Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.Latest)
                    : AssemblyUtils.GenerateContract(sourceCode, Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest);

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
    }
}
