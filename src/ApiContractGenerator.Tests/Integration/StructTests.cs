using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class StructTests : IntegrationTests
    {
        [Test]
        public static void Ref_struct()
        {
            Assert.That("public ref struct Test { }", HasContract(
                "public ref struct Test",
                "{",
                "}"));
        }

        [Test]
        public static void Readonly_struct()
        {
            Assert.That("public readonly struct Test { }", HasContract(
                "public readonly struct Test",
                "{",
                "}"));
        }

        [Test]
        public static void Readonly_ref_struct()
        {
            Assert.That("public readonly ref struct Test { }", HasContract(
                "public readonly ref struct Test",
                "{",
                "}"));
        }

        [Test]
        public static void Struct_implementing_interface()
        {
            Assert.That("using System.Collections; public struct Test : IEnumerable { IEnumerator IEnumerable.GetEnumerator() => default; }", HasContract(
                "public struct Test : System.Collections.IEnumerable",
                "{",
                "}"));
        }
    }
}
