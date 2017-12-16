using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class SignatureTests : IntegrationTests
    {
        [Test]
        public static void Ref_readonly_method_parameter_should_use_in()
        {
            Assert.That("public struct A { public void Test(in int x) { } }", HasContract(
                "public struct A",
                "{",
                "    public void Test(in int x);",
                "}"));
        }

        [Test]
        public static void Ref_readonly_delegate_parameter_should_use_in()
        {
            Assert.That("public delegate void Test(in int x);", HasContract(
                "public delegate void Test(in int x);"));
        }
    }
}
