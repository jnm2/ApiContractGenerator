using System;
using ApiContractGenerator.Tests.Utils;
using NUnit.Framework.Constraints;

namespace ApiContractGenerator.Tests.Integration
{
    public abstract class IntegrationTests
    {
        public static Constraint HasContract(params string[] lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            return new ContractConstraint(string.Join(Environment.NewLine, lines) + Environment.NewLine);
        }

        private sealed class ContractConstraint : Constraint
        {
            private readonly string expected;

            public ContractConstraint(string expected)
            {
                this.expected = expected;
            }

            public override ConstraintResult ApplyTo<TActual>(TActual actual)
            {
                if (!((object)actual is string sourceCode))
                    throw new ArgumentException("Expected source code as string.", nameof(actual));

                return new ContractConstraintResult(this, AssemblyUtils.GenerateContract(sourceCode), expected);
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
