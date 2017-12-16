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

        [Test]
        public static void Attribute_argument_with_serialized_generic_enum_type_name()
        {
            Assert.That("[Test(A.B.C<int, object>.D<A.B.C<int, object>.D<int>.E>.E.F)] public class TestAttribute : System.Attribute { public TestAttribute(object obj) { } }" +
                        "namespace A.B { public static class C<T1, T2> { public static class D<T3> { public enum E { F } } } }", HasContract(
                "[Test(A.B.C<int, object>.D<A.B.C<int, object>.D<int>.E>.E.F)]",
                "public class TestAttribute : System.Attribute",
                "{",
                "    public TestAttribute(object obj);",
                "}",
                "namespace A.B",
                "{",
                "    public static class C<T1, T2>",
                "    {",
                "        public static class D<T3>",
                "        {",
                "            public enum E : int",
                "            {",
                "                F = 0",
                "            }",
                "        }",
                "    }",
                "}"));
        }

        [Test]
        public static void Attribute_argument_with_serialized_type_name()
        {
            Assert.That("[Test(typeof(A.B.C.D))] public class TestAttribute : System.Attribute { public TestAttribute(object obj) { } }" +
                        "namespace A.B { public static class C { public static class D { } } }", HasContract(
                "[Test(typeof(A.B.C.D))]",
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
                "        }",
                "    }",
                "}"));
        }

        [Test]
        public static void Attribute_argument_with_serialized_open_generic_type_name()
        {
            Assert.That("[Test(typeof(A.B.C<,>.D<>))] public class TestAttribute : System.Attribute { public TestAttribute(object obj) { } }" +
                        "namespace A.B { public static class C<T1, T2> { public static class D<T3> { } } }", HasContract(
                "[Test(typeof(A.B.C<,>.D<>))]",
                "public class TestAttribute : System.Attribute",
                "{",
                "    public TestAttribute(object obj);",
                "}",
                "namespace A.B",
                "{",
                "    public static class C<T1, T2>",
                "    {",
                "        public static class D<T3>",
                "        {",
                "        }",
                "    }",
                "}"));
        }

        [Test]
        public static void Attribute_argument_with_serialized_closed_generic_type_name()
        {
            Assert.That("[Test(typeof(A.B.C<int, object>.D<A.B.C<int, object>.D<int>>))] public class TestAttribute : System.Attribute { public TestAttribute(object obj) { } }" +
                        "namespace A.B { public static class C<T1, T2> { public class D<T3> { private D() { } } } }", HasContract(
                "[Test(typeof(A.B.C<int, object>.D<A.B.C<int, object>.D<int>>))]",
                "public class TestAttribute : System.Attribute",
                "{",
                "    public TestAttribute(object obj);",
                "}",
                "namespace A.B",
                "{",
                "    public static class C<T1, T2>",
                "    {",
                "        public class D<T3>",
                "        {",
                "        }",
                "    }",
                "}"));
        }

        [Test]
        public static void Attribute_argument_with_serialized_generic_type_name_closed_over_type_in_same_assembly()
        {
            Assert.That("[Test(typeof(TestGeneric<TestAttribute>))] public class TestAttribute : System.Attribute { public TestAttribute(object obj) { } }" +
                        "public class TestGeneric<T> { private TestGeneric() { } }", HasContract(
                "[Test(typeof(TestGeneric<TestAttribute>))]",
                "public class TestAttribute : System.Attribute",
                "{",
                "    public TestAttribute(object obj);",
                "}",
                "",
                "public class TestGeneric<T>",
                "{",
                "}"));
        }

        [TestCase("int[]")]
        [TestCase("int[,]")]
        [TestCase("int[][,]")]
        [TestCase("System.Action<int[]>[]")]
        public static void Attribute_argument_with_serialized_array_type_name(string typeName)
        {
            Assert.That("[Test(typeof(" + typeName + "))] public class TestAttribute : System.Attribute { public TestAttribute(object obj) { } }", HasContract(
                "[Test(typeof(" + typeName + "))]",
                "public class TestAttribute : System.Attribute",
                "{",
                "    public TestAttribute(object obj);",
                "}"));
        }

        [TestCase("int*")]
        [TestCase("int**")]
        [TestCase("System.IntPtr*")]
        [TestCase("TestAttribute.Nested*")]
        [TestCase("int**[][]")]
        public static void Attribute_argument_with_serialized_pointer_type_name(string typeName)
        {
            Assert.That("[Test(typeof(" + typeName + "))] public class TestAttribute : System.Attribute { public TestAttribute(object obj) { } public struct Nested { } }", HasContract(
                "[Test(typeof(" + typeName + "))]",
                "public class TestAttribute : System.Attribute",
                "{",
                "    public TestAttribute(object obj);",
                "",
                "    public struct Nested",
                "    {",
                "    }",
                "}"));
        }

        [TestCase("new object[0]")]
        [TestCase("new int[0]")]
        [TestCase("new int[] { 0 }")]
        [TestCase("new string[] { null, \"\" }")]
        public static void Attribute_array_argument(string argument)
        {
            Assert.That("[Test(" + argument + ")] public class TestAttribute : System.Attribute { public TestAttribute(object obj) { } }", HasContract(
                "[Test(" + argument + ")]",
                "public class TestAttribute : System.Attribute",
                "{",
                "    public TestAttribute(object obj);",
                "}"));
        }

        [Test]
        public static void Attribute_without_standard_suffix()
        {
            Assert.That("[Test] public class Test : System.Attribute { }", HasContract(
                "[Test]",
                "public class Test : System.Attribute",
                "{",
                "    public Test();",
                "}"));
        }

        [Test]
        public static void Method_return_value_attribute()
        {
            Assert.That("public struct A { [return: Test] public void Method() { } } public class Test : System.Attribute { }", HasContract(
                "public struct A",
                "{",
                "    [return: Test]",
                "    public void Method();",
                "}",
                "",
                "public class Test : System.Attribute",
                "{",
                "    public Test();",
                "}"));
        }

        [Test]
        public static void Delegate_return_value_attribute()
        {
            Assert.That("[return: Test] public delegate void Delegate(); public class Test : System.Attribute { }", HasContract(
                "[return: Test]",
                "public delegate void Delegate();",
                "",
                "public class Test : System.Attribute",
                "{",
                "    public Test();",
                "}"));
        }
    }
}
