using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class IgnoredNamespaceTests : IntegrationTests
    {
        [Test]
        public static void Simple_ignore()
        {
            Assert.That("namespace A { public struct Test { } } namespace B { public struct Test { } }", HasContract(
                "namespace B",
                "{",
                "    public struct Test",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("A"));
        }

        [Test]
        public static void Ignore_should_only_include_nested()
        {
            Assert.That("namespace A { public struct Test { } namespace B { public struct Test { } } }", HasContract(
                "namespace A",
                "{",
                "    public struct Test",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("A.B"));
        }

        [Test]
        public static void Ignore_should_not_match_nested()
        {
            Assert.That("namespace A.B { public struct Test { } }", HasContract(
                "namespace A.B",
                "{",
                "    public struct Test",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("B"));
        }

        [Test]
        public static void Ignore_should_include_nested()
        {
            Assert.That("namespace A { public struct Test { } namespace B { public struct Test { } } }", HasContract().WithIgnoredNamespace("A"));
        }

        [Test]
        public static void Nonignored_method_should_unignore_return_type()
        {
            Assert.That("public struct Nonignored { public Ignored.ShouldUnignore Foo() => default; } namespace Ignored { public struct ShouldUnignore { } }", HasContract(
                "public struct Nonignored",
                "{",
                "    public Ignored.ShouldUnignore Foo();",
                "}",
                "namespace Ignored",
                "{",
                "    // Warning; type cannot be ignored because it is referenced by:",
                "    //  - Nonignored.Foo return type",
                "    public struct ShouldUnignore",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("Ignored"));
        }

        [Test]
        public static void Nonignored_delegate_should_unignore_return_type()
        {
            Assert.That("public delegate Ignored.ShouldUnignore Foo(); namespace Ignored { public struct ShouldUnignore { } }", HasContract(
                "public delegate Ignored.ShouldUnignore Foo();",
                "namespace Ignored",
                "{",
                "    // Warning; type cannot be ignored because it is referenced by:",
                "    //  - Foo return type",
                "    public struct ShouldUnignore",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("Ignored"));
        }

        [TestCase("Ignored.ShouldUnignore p")]
        [TestCase("ref Ignored.ShouldUnignore p")]
        [TestCase("in Ignored.ShouldUnignore p")]
        [TestCase("Ignored.ShouldUnignore p = default")]
        public static void Nonignored_method_should_not_unignore_non_out_parameter_types(string parameterList)
        {
            Assert.That("public struct Nonignored { public void Foo(" + parameterList + ") { } } namespace Ignored { public struct ShouldUnignore { } }", HasContract(
                "public struct Nonignored",
                "{",
                "    public void Foo(" + parameterList + ");",
                "}").WithIgnoredNamespace("Ignored"));
        }

        [TestCase("Ignored.ShouldUnignore p")]
        [TestCase("ref Ignored.ShouldUnignore p")]
        [TestCase("in Ignored.ShouldUnignore p")]
        [TestCase("Ignored.ShouldUnignore p = default")]
        public static void Nonignored_delegate_should_unignore_non_out_parameter_types(string parameterList)
        {
            Assert.That("public unsafe delegate void Foo(" + parameterList + "); namespace Ignored { public struct ShouldUnignore { } }", HasContract(
                "public delegate void Foo(" + parameterList + ");",
                "namespace Ignored",
                "{",
                "    // Warning; type cannot be ignored because it is referenced by:",
                "    //  - Foo parameter p",
                "    public struct ShouldUnignore",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("Ignored"));
        }

        [TestCase("out Ignored.ShouldUnignore p")]
        [TestCase("out Ignored.ShouldUnignore[] p")]
        [TestCase("out Ignored.ShouldUnignore* p")]
        [TestCase("out System.Collections.Generic.List<Ignored.ShouldUnignore> p")]
        public static void Nonignored_method_should_unignore_out_parameter_types(string parameterList)
        {
            Assert.That("public struct Nonignored { public unsafe void Foo(" + parameterList + ") { p = default; } } namespace Ignored { public struct ShouldUnignore { } }", HasContract(
                "public struct Nonignored",
                "{",
                "    public void Foo(" + parameterList + ");",
                "}",
                "namespace Ignored",
                "{",
                "    // Warning; type cannot be ignored because it is referenced by:",
                "    //  - Nonignored.Foo parameter p",
                "    public struct ShouldUnignore",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("Ignored"));
        }

        [TestCase("out Ignored.ShouldUnignore p")]
        [TestCase("out Ignored.ShouldUnignore[] p")]
        [TestCase("out Ignored.ShouldUnignore* p")]
        [TestCase("out System.Collections.Generic.List<Ignored.ShouldUnignore> p")]
        public static void Nonignored_delegate_should_unignore_out_parameter_types(string parameterList)
        {
            Assert.That("public unsafe delegate void Foo(" + parameterList + "); namespace Ignored { public struct ShouldUnignore { } }", HasContract(
                "public delegate void Foo(" + parameterList + ");",
                "namespace Ignored",
                "{",
                "    // Warning; type cannot be ignored because it is referenced by:",
                "    //  - Foo parameter p",
                "    public struct ShouldUnignore",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("Ignored"));
        }

        [Test]
        public static void Nonignored_method_should_unignore_delegate_byval_parameter_types()
        {
            Assert.That("public struct Nonignored { public unsafe void Foo(System.Action<Ignored.ShouldUnignore> a) { } } namespace Ignored { public struct ShouldUnignore { } }", HasContract(
                "public struct Nonignored",
                "{",
                "    public void Foo(System.Action<Ignored.ShouldUnignore> a);",
                "}",
                "namespace Ignored",
                "{",
                "    // Warning; type cannot be ignored because it is referenced by:",
                "    //  - Nonignored.Foo parameter a",
                "    public struct ShouldUnignore",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("Ignored"));
        }

        [Test]
        public static void Nonignored_delegate_should_unignore_delegate_byval_parameter_types()
        {
            Assert.That("public delegate void Foo(System.Action<Ignored.ShouldUnignore> a); namespace Ignored { public struct ShouldUnignore { } }", HasContract(
                "public delegate void Foo(System.Action<Ignored.ShouldUnignore> a);",
                "namespace Ignored",
                "{",
                "    // Warning; type cannot be ignored because it is referenced by:",
                "    //  - Foo parameter a",
                "    public struct ShouldUnignore",
                "    {",
                "    }",
                "}").WithIgnoredNamespace("Ignored"));
        }

        [Test]
        public static void Unignored_nested_should_expose_only_name_of_containing_type()
        {
            Assert.That("public delegate Ignored.Containing.ShouldUnignore Foo(); namespace Ignored { public struct Containing { public void Hide() { } public struct ShouldUnignore { } } }", HasContract(
                "public delegate Ignored.Containing.ShouldUnignore Foo();",
                "namespace Ignored",
                "{",
                "    public struct Containing",
                "    {",
                "        // Warning; type cannot be ignored because it is referenced by:",
                "        //  - Foo return type",
                "        public struct ShouldUnignore",
                "        {",
                "        }",
                "    }",
                "}").WithIgnoredNamespace("Ignored"));
        }
    }
}
