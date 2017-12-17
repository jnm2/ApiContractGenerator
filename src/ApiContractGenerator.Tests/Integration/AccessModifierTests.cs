using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class AccessModifierTests : IntegrationTests
    {
        [TestCase("public", "public")]
        [TestCase("protected", "protected")]
        [TestCase("protected internal", "protected")]
        public static void Should_be_visible(string accessModifiers, string shown)
        {
            Assert.That("public class A { " + accessModifiers + " A() { } }", HasContract(
                "public class A",
                "{",
                "    " + shown + " A();",
                "}"));
        }
        [TestCase("private")]
        [TestCase("internal")]
        [TestCase("private protected")]
        public static void Should_be_invisible(string accessModifiers)
        {
            Assert.That("public class A { " + accessModifiers + " A() { } }", HasContract(
                "public class A",
                "{",
                "}"));
        }
    }
}
