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

        [Test]
        public static void Ref_method_return()
        {
            Assert.That("public struct A { private static int x; public ref int Test() => ref x; }", HasContract(
                "public struct A",
                "{",
                "    public ref int Test();",
                "}"));
        }

        [Test]
        public static void Ref_delegate_return()
        {
            Assert.That("public delegate ref int Test();", HasContract(
                "public delegate ref int Test();"));
        }

        [Test]
        public static void Ref_readonly_method_return()
        {
            Assert.That("public struct A { private static int x; public ref readonly int Test() => ref x; }", HasContract(
                "public struct A",
                "{",
                "    public ref readonly int Test();",
                "}"));
        }

        [Test]
        public static void Ref_readonly_delegate_return()
        {
            Assert.That("public delegate ref readonly int Test();", HasContract(
                "public delegate ref readonly int Test();"));
        }
    }
}
