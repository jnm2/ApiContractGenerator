using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class SpecialAttributeTests : IntegrationTests
    {
        [Test]
        public static void ExtensionAttribute()
        {
            Assert.That("public static class Class { public static void Method(this object p) { } }", HasContract(
                "public static class Class",
                "{",
                "    public static void Method(this object p);",
                "}"));
        }
    }
}
