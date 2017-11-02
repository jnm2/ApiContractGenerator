using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class OptionalParameterTests : IntegrationTests
    {
        [TestCase("bool x = false")]
        [TestCase("bool? x = false")]
        [TestCase("bool? x = null")]
        [TestCase(@"char x = '\0'")]
        [TestCase(@"char? x = '\0'")]
        [TestCase(@"char? x = null")]
        [TestCase("byte x = 0")]
        [TestCase("byte? x = 0")]
        [TestCase("byte? x = null")]
        [TestCase("sbyte x = 0")]
        [TestCase("sbyte? x = 0")]
        [TestCase("sbyte? x = null")]
        [TestCase("short x = 0")]
        [TestCase("short? x = 0")]
        [TestCase("short? x = null")]
        [TestCase("ushort x = 0")]
        [TestCase("ushort? x = 0")]
        [TestCase("ushort? x = null")]
        [TestCase("int x = 0")]
        [TestCase("int? x = 0")]
        [TestCase("int? x = null")]
        [TestCase("uint x = 0")]
        [TestCase("uint? x = 0")]
        [TestCase("uint? x = null")]
        [TestCase("long x = 0")]
        [TestCase("long? x = 0")]
        [TestCase("long? x = null")]
        [TestCase("ulong x = 0")]
        [TestCase("ulong? x = 0")]
        [TestCase("ulong? x = null")]
        [TestCase("float x = 0")]
        [TestCase("float? x = 0")]
        [TestCase("float? x = null")]
        [TestCase("double x = 0")]
        [TestCase("double? x = 0")]
        [TestCase("double? x = null")]
        [TestCase("object x = null")]
        [TestCase("string x = null")]
        [TestCase("System.IntPtr x = default")]
        [TestCase("System.IntPtr? x = null")]
        [TestCase("System.UIntPtr x = default")]
        [TestCase("System.UIntPtr? x = null")]
        [TestCase("System.DayOfWeek x = System.DayOfWeek.Sunday")]
        [TestCase("System.DayOfWeek? x = System.DayOfWeek.Sunday")]
        [TestCase("System.DayOfWeek? x = null")]
        [TestCase("System.AttributeTargets x = 0")]
        [TestCase("System.AttributeTargets? x = 0")]
        [TestCase("System.AttributeTargets? x = null")]
        [TestCase("System.DateTime x = default")]
        [TestCase("System.DateTime? x = null")]
        [TestCase("System.Exception x = null")]
        [TestCase("System.Action x = null")]
        [TestCase("System.TypedReference x = default")]
        [TestCase("object[] x = null")]
        [TestCase("int* x = null")]
        public static void Default_value_is_type_default(string parameterDefinition)
        {
            Assert.That("public interface ITest { unsafe void Method(" + parameterDefinition + "); }", HasContract(
                "public interface ITest",
                "{",
                "    void Method(" + parameterDefinition + ");",
                "}"));
        }

        [Test]
        public static void Default_value_is_generic_type_default()
        {
            Assert.That("public interface ITest { void Method<T>(T x = default); }", HasContract(
                "public interface ITest",
                "{",
                "    void Method<T>(T x = default);",
                "}"));
        }

        [Test]
        public static void Default_value_is_generic_value_type_default()
        {
            Assert.That("public interface ITest { void Method<T>(T? x = null) where T : struct; }", HasContract(
                "public interface ITest",
                "{",
                "    void Method<T>(T? x = null) where T : struct;",
                "}"));
        }

        [Test]
        public static void Default_value_is_generic_reference_type_default()
        {
            Assert.That("public interface ITest { void Method<T>(T x = null) where T : class; }", HasContract(
                "public interface ITest",
                "{",
                "    void Method<T>(T x = null) where T : class;",
                "}"));
        }

        [TestCase("decimal x = 0", "[System.Runtime.CompilerServices.DecimalConstant(0, 0, 0, 0, 0)] System.Decimal x = default")]
        [TestCase("decimal x = 1", "[System.Runtime.CompilerServices.DecimalConstant(0, 0, 0, 0, 1)] System.Decimal x = default")]
        public static void Decimal_default_value_is_shown_as_non_constant(string source, string api)
        {
            Assert.That("public interface ITest { void Method(" + source + "); }", HasContract(
                "public interface ITest",
                "{",
                "    void Method(" + api + ");",
                "}"));
        }

        [TestCase("Optional x As System.DateTime = Nothing", "[System.Runtime.CompilerServices.DateTimeConstant(0)] System.DateTime x = default")]
        [TestCase("Optional x As System.DateTime = #1970/01/01#", "[System.Runtime.CompilerServices.DateTimeConstant(621_355_968_000_000_000)] System.DateTime x = default")]
        public static void DateTime_default_value_is_shown_as_non_constant(string source, string api)
        {
            Assert.That("Public Interface ITest : Sub Method(" + source + ") : End Interface", HasContract(
                "public interface ITest",
                "{",
                "    void Method(" + api + ");",
                "}").SourceIsVisualBasic);
        }
    }
}
