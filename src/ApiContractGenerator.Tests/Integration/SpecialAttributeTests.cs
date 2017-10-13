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

        [Test]
        public static void ParamArrayAttribute_on_method()
        {
            Assert.That("public static class Class { public static void Method(params object[] p) { } }", HasContract(
                "public static class Class",
                "{",
                "    public static void Method(params object[] p);",
                "}"));
        }

        [Test]
        public static void ParamArrayAttribute_on_property()
        {
            Assert.That("public class Class { public int this[params object[] p] => 0; private Class() { } }", HasContract(
                "public class Class",
                "{",
                "    public int this[params object[] p] { get; }",
                "}"));
        }

        [Test]
        public static void ParamArrayAttribute_on_delegate()
        {
            Assert.That("public delegate void Method(params object[] p);", HasContract(
                "public delegate void Method(params object[] p);"));
        }

        [Test]
        public static void IteratorStateMachineAttribute_on_method()
        {
            Assert.That("public static class Class { public static System.Collections.Generic.IEnumerable<int> Method() { yield break; } }", HasContract(
                "public static class Class",
                "{",
                "    public static System.Collections.Generic.IEnumerable<int> Method();",
                "}"));
        }

        [Test]
        public static void IteratorStateMachineAttribute_on_property()
        {
            Assert.That("public static class Class { public static System.Collections.Generic.IEnumerable<int> Property { get { yield break; } } }", HasContract(
                "public static class Class",
                "{",
                "    public static System.Collections.Generic.IEnumerable<int> Property { get; }",
                "}"));
        }

        [Test]
        public static void AsyncStateMachineAttribute()
        {
            Assert.That("public static class Class { public static async void Method() { } }", HasContract(
                "public static class Class",
                "{",
                "    public static void Method();",
                "}"));
        }
    }
}
