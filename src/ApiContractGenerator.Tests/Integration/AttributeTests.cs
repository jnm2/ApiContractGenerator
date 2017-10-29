using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class AttributeTests : IntegrationTests
    {
        [Test]
        public static void Attribute_argument_with_serialized_enum_type_name()
        {
            Assert.That("[Test(A.B.C.D.E.F)] public class TestAttribute : System.Attribute { public TestAttribute(object obj) { } }" +
                        "namespace A.B { public static class C { public static class D { public enum E { F } } } }", HasContract(
                "[Test(A.B.C.D.E.F)]",
                "public class TestAttribute : System.Attribute",
                "{",
                "    public TestAttribute(object obj);",
                "}",
                "namespace A.B",
                "{",
                "    public static class C",
                "    {",
                "        public static class D",
                "        {",
                "            public enum E : int",
                "            {",
                "                F = 0",
                "            }",
                "        }",
                "    }",
                "}"));
        }
    }
}
