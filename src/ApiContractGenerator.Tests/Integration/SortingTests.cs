using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class SortingTests : IntegrationTests
    {
        [Test]
        public static void Top_level_types_should_be_sorted_ascending_by_generic_arity()
        {
            Assert.That("public struct A<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> { }public struct A<T1, T2> { }  public struct A { }", HasContract(
                "public struct A",
                "{",
                "}",
                "",
                "public struct A<T1, T2>",
                "{",
                "}",
                "",
                "public struct A<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>",
                "{",
                "}"));
        }

        [Test]
        public static void Nested_types_should_be_sorted_ascending_by_generic_arity()
        {
            Assert.That("public static class C { public struct A<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> { } public struct A<T1, T2> { } public struct A { } }", HasContract(
                "public static class C",
                "{",
                "    public struct A",
                "    {",
                "    }",
                "",
                "    public struct A<T1, T2>",
                "    {",
                "    }",
                "",
                "    public struct A<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>",
                "    {",
                "    }",
                "}"));
        }

        [Test]
        public static void Methods_should_be_sorted_ascending_by_generic_arity()
        {
            Assert.That("public struct S { public void A<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() { } public void A<T1, T2>() { } public void A() { } }", HasContract(
                "public struct S",
                "{",
                "    public void A();",
                "",
                "    public void A<T1, T2>();",
                "",
                "    public void A<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();",
                "}"));
        }
    }
}
