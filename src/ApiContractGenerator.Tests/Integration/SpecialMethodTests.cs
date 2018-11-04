using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class SpecialMethodTests : IntegrationTests
    {
        [Test]
        public static void Finalizer_is_not_shown()
        {
            Assert.That("public class Test { private Test() { } ~Test() { } }", HasContract(
                "public class Test",
                "{",
                "}"));
        }
    }
}
