using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class IgnoredNamespaceTests : IntegrationTests
    {
        [Test]
        public static void Simple_ignore()
        {
            Assert.That("namespace A { public struct Test { } } namespace B { public struct Test { } }", HasContract(
                "namespace B",
                "{",
                "    public struct Test",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("A"));
        }
    }
}
