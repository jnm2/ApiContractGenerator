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

        [Test]
        public static void DefaultMemberAttribute_on_class()
        {
            Assert.That("public class Class { public int this[int x] => x; private Class() { } }", HasContract(
                "public class Class",
                "{",
                "    public int this[int x] { get; }",
                "}"));
        }

        [Test]
        public static void DefaultMemberAttribute_on_struct()
        {
            Assert.That("public struct Struct { public int this[int x] => x; }", HasContract(
                "public struct Struct",
                "{",
                "    public int this[int x] { get; }",
                "}"));
        }
    }
}
