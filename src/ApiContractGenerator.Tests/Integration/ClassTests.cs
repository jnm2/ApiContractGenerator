using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class ClassTests : IntegrationTests
    {
        [Test]
        public static void Class_without_namespace()
        {
            Assert.That("public class Test { private Test() { } }", HasContract(
                "public class Test",
                "{",
                "}"));
        }

        [Test]
        public static void Internal_class_is_not_shown()
        {
            Assert.That("namespace Test { internal static class Test { } }", HasContract());
        }

        [Test]
        public static void Nested_protected_class_is_shown()
        {
            Assert.That("public static class Test { protected static class Nested { } }", HasContract(
                "public static class Test",
                "{",
                "    protected static class Nested",
                "    {",
                "    }",
                "}"));
        }

        [Test]
        public static void Nested_protected_internal_class_is_shown_as_protected()
        {
            Assert.That("public static class Test { protected internal static class Nested { } }", HasContract(
                "public static class Test",
                "{",
                "    protected static class Nested",
                "    {",
                "    }",
                "}"));
        }

        [Test]
        public static void Nested_public_class_is_shown()
        {
            Assert.That("public static class Test { public static class Nested { } }", HasContract(
                "public static class Test",
                "{",
                "    public static class Nested",
                "    {",
                "    }",
                "}"));
        }

        [Test]
        public static void Nested_private_class_is_not_shown()
        {
            Assert.That("public static class Test { private static class Nested { } }", HasContract(
                "public static class Test",
                "{",
                "}"));
        }

        [Test]
        public static void Nested_internal_class_is_not_shown()
        {
            Assert.That("public static class Test { internal static class Nested { } }", HasContract(
                "public static class Test",
                "{",
                "}"));
        }

        [Test]
        public static void Nested_namespaces_are_merged()
        {
            Assert.That("namespace Part1 { namespace Part2 { public static class Test { } } }", HasContract(
                "namespace Part1.Part2",
                "{",
                "    public static class Test",
                "    {",
                "    }",
                "}"));
        }

        [Test]
        public static void Static_modifier_is_shown()
        {
            Assert.That("public static class Test { }", HasContract(
                "public static class Test",
                "{",
                "}"));
        }

        [Test]
        public static void Sealed_modifier_is_shown()
        {
            Assert.That("public sealed class Test { private Test() { } }", HasContract(
                "public sealed class Test",
                "{",
                "}"));
        }

        [Test]
        public static void Abstract_modifier_is_shown()
        {
            Assert.That("public abstract class Test { private Test() { } }", HasContract(
                "public abstract class Test",
                "{",
                "}"));
        }

        [Test]
        public static void Single_generic_parameter()
        {
            Assert.That("public class Test<T> { private Test() { } }", HasContract(
                "public class Test<T>",
                "{",
                "}"));
        }

        [Test]
        public static void Multiple_generic_parameters()
        {
            Assert.That("public class Test<T1, T2> { private Test() { } }", HasContract(
                "public class Test<T1, T2>",
                "{",
                "}"));
        }

        [Test]
        public static void Nongeneric_nested_in_generic()
        {
            Assert.That("public class Test<T1> { private Test() { } public class Nested { private Nested() { } } }", HasContract(
                "public class Test<T1>",
                "{",
                "    public class Nested",
                "    {",
                "    }",
                "}"));
        }

        [Test]
        public static void Generic_nested_in_nongeneric()
        {
            Assert.That("public class Test { private Test() { } public class Nested<T1> { private Nested() { } } }", HasContract(
                "public class Test",
                "{",
                "    public class Nested<T1>",
                "    {",
                "    }",
                "}"));
        }

        [Test]
        public static void Generic_nested_in_generic()
        {
            Assert.That("public class Test<T1> { private Test() { } public class Nested<T2> { private Nested() { } } }", HasContract(
                "public class Test<T1>",
                "{",
                "    public class Nested<T2>",
                "    {",
                "    }",
                "}"));
        }

        [TestCase("class")]
        [TestCase("struct")]
        [TestCase("new()")]
        [TestCase("class, new()")]
        [TestCase("System.IDisposable")]
        [TestCase("class, System.IDisposable")]
        [TestCase("struct, System.IDisposable")]
        [TestCase("System.IDisposable, new()")]
        [TestCase("class, System.IDisposable, new()")]
        [TestCase("System.IDisposable, System.IEquatable<T>")]
        [TestCase("class, System.IDisposable, System.IEquatable<T>")]
        [TestCase("struct, System.IDisposable, System.IEquatable<T>")]
        [TestCase("System.IDisposable, System.IEquatable<T>, new()")]
        [TestCase("class, System.IDisposable, System.IEquatable<T>, new()")]
        public static void Generic_constraints(string constraint)
        {
            Assert.That("public class Test<T> where T : " + constraint + " { private Test() { } }", HasContract(
                "public class Test<T> where T : " + constraint,
                "{",
                "}"));
        }
    }
}
