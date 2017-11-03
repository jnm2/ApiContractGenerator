using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class EnumTests : IntegrationTests
    {
        [Test]
        public static void Empty_enum()
        {
            Assert.That("public enum Test : int { }", HasContract(
                "public enum Test : int",
                "{",
                "}"));
        }
    }
}
