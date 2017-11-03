using System;
using ApiContractGenerator.Internal;
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
        public static void Iterator_method_has_no_visible_attribute()
        {
            Assert.That("public static class Class { public static System.Collections.Generic.IEnumerable<int> Method() { yield break; } }", HasContract(
                "public static class Class",
                "{",
                "    public static System.Collections.Generic.IEnumerable<int> Method();",
                "}"));
        }

        [Test]
        public static void Iterator_property_has_no_visible_attribute()
        {
            Assert.That("public static class Class { public static System.Collections.Generic.IEnumerable<int> Property { get { yield break; } } }", HasContract(
                "public static class Class",
                "{",
                "    public static System.Collections.Generic.IEnumerable<int> Property { get; }",
                "}"));
        }

        [Test]
        public static void Async_method_has_no_visible_attribute()
        {
            Assert.That("public static class Class { public static async void Method() { } }", HasContract(
                "public static class Class",
                "{",
                "    public static void Method();",
                "}"));
        }

        [TestCase("[System.CodeDom.Compiler.GeneratedCode(null, null)]", AttributeTargets.All)]
        [TestCase("[System.Runtime.CompilerServices.CompilerGenerated]", AttributeTargets.All)]
        [TestCase("[System.Runtime.CompilerServices.IteratorStateMachine(null)]", AttributeTargets.Method)]
        [TestCase("[System.Runtime.CompilerServices.AsyncStateMachine(null)]", AttributeTargets.Method)]
        [TestCase("[System.Diagnostics.DebuggerNonUserCode]", AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
        [TestCase("[System.Diagnostics.DebuggerStepThrough]", AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method)]
        [TestCase("[System.Diagnostics.DebuggerHidden]", AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
        [TestCase("[System.Diagnostics.DebuggerStepperBoundary]", AttributeTargets.Constructor | AttributeTargets.Method)]
        [TestCase("[System.Diagnostics.CodeAnalysis.SuppressMessage(null, null)]", AttributeTargets.All)]
        [TestCase("[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]", AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
        public static void Attribute_is_ignored(string attribute, AttributeTargets targets)
        {
            if (targets.HasFlag(AttributeTargets.Class))
                Assert.That(attribute + " public static class Class { }", HasContract(
                "public static class Class",
                "{",
                "}"));

            if (targets.HasFlag(AttributeTargets.Constructor))
                Assert.That("public class Class { " + attribute + " public Class() { } }", HasContract(
                    "public class Class",
                    "{",
                    "    public Class();",
                    "}"));

            if (targets.HasFlag(AttributeTargets.Delegate))
                Assert.That(attribute + " public delegate void Delegate();", HasContract(
                    "public delegate void Delegate();"));

            if (targets.HasFlag(AttributeTargets.Enum))
                Assert.That(attribute + " public enum Enum { }", HasContract(
                    "public enum Enum : int",
                    "{",
                    "}"));

            if (targets.HasFlag(AttributeTargets.Event))
                Assert.That("public static class Class { " + attribute + " public static event System.EventHandler Event; }", HasContract(
                    "public static class Class",
                    "{",
                    "    public static event System.EventHandler Event;",
                    "}"));

            if (targets.HasFlag(AttributeTargets.Field))
                Assert.That("public static class Class { " + attribute + " public static int Field; }", HasContract(
                    "public static class Class",
                    "{",
                    "    public static int Field;",
                    "}"));

            if (targets.HasFlag(AttributeTargets.GenericParameter))
                Assert.That("public static class Class<" + attribute + " T> { }", HasContract(
                    "public static class Class<T>",
                    "{",
                    "}"));

            if (targets.HasFlag(AttributeTargets.Interface))
                Assert.That(attribute + " public interface IInterface { }", HasContract(
                    "public interface IInterface",
                    "{",
                    "}"));

            if (targets.HasFlag(AttributeTargets.Method))
                Assert.That("public static class Class { " + attribute + " public static void Method() { } }", HasContract(
                    "public static class Class",
                    "{",
                    "    public static void Method();",
                    "}"));

            if (targets.HasFlag(AttributeTargets.Module))
                Assert.That("[module: " + attribute.Slice(1), HasContract());

            if (targets.HasFlag(AttributeTargets.Parameter))
                Assert.That("public delegate void Delegate(" + attribute + " int parameter);", HasContract(
                    "public delegate void Delegate(int parameter);"));

            if (targets.HasFlag(AttributeTargets.Property))
                Assert.That("public static class Class { " + attribute + " public static int Property { get; } }", HasContract(
                    "public static class Class",
                    "{",
                    "    public static int Property { get; }",
                    "}"));

            if (targets.HasFlag(AttributeTargets.ReturnValue))
                Assert.That("[return: " + attribute.Slice(1) + " public delegate int Delegate();", HasContract(
                    "public delegate int Delegate();"));

            if (targets.HasFlag(AttributeTargets.Struct))
                Assert.That(attribute + " public struct Struct { }", HasContract(
                    "public struct Struct",
                    "{",
                    "}"));
        }
    }
}
