using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class TypeReferenceTests : IntegrationTests
    {
        [Test]
        public static void Nested_closed_generics()
        {
            Assert.That("public static class A<T1, T2> { public static class B { public class C<T3> { public C(A<T3, object>.B.C<int> x) { } } } }", HasContract(
                "public static class A<T1, T2>",
                "{",
                "    public static class B",
                "    {",
                "        public class C<T3>",
                "        {",
                "            public C(A<T3, object>.B.C<int> x);",
                "        }",
                "    }",
                "}"));
        }

        [TestCase("void")]
        [TestCase("bool")]
        [TestCase("char")]
        [TestCase("byte")]
        [TestCase("sbyte")]
        [TestCase("short")]
        [TestCase("ushort")]
        [TestCase("int")]
        [TestCase("uint")]
        [TestCase("long")]
        [TestCase("ulong")]
        [TestCase("float")]
        [TestCase("double")]
        [TestCase("decimal")]
        [TestCase("string")]
        [TestCase("object")]
        public static void Built_in_data_types(string keyword)
        {
            Assert.That("public delegate " + keyword + " X();", HasContract(
                "public delegate " + keyword + " X();"));
        }
    }
}
