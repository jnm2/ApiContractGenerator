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
    }
}
