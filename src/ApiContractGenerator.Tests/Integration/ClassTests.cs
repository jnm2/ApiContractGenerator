using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class ClassTests : IntegrationTests
    {
        [Test]
        public static void Class_without_namespace()
        {
            Assert.That("public static class Test { }", HasContract(
                "public static class Test",
                "{",
                "}"));
        }

        [Test]
        public static void Internal_class_is_not_shown()
        {
            Assert.That("namespace Test { internal static class Test { } }", HasContract());
        }
    }
}
