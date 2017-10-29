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
    }
}
