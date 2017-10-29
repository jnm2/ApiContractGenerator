using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class OptionalParameterTests : IntegrationTests
    {
        [TestCase("bool x = false")]
        [TestCase(@"char x = '\0'")]
        [TestCase("byte x = 0")]
        [TestCase("sbyte x = 0")]
        [TestCase("short x = 0")]
        [TestCase("ushort x = 0")]
        [TestCase("int x = 0")]
        [TestCase("uint x = 0")]
        [TestCase("long x = 0")]
        [TestCase("ulong x = 0")]
        [TestCase("float x = 0")]
        [TestCase("double x = 0")]
        [TestCase("object x = null")]
        [TestCase("string x = null")]
        [TestCase("System.IntPtr x = default")]
        [TestCase("System.UIntPtr x = default")]
        [TestCase("System.DayOfWeek x = System.DayOfWeek.Sunday")]
        [TestCase("System.AttributeTargets x = 0")]
        [TestCase("System.DateTime x = default")]
        [TestCase("System.Exception x = null")]
        [TestCase("System.Action x = null")]
        public static void Default_value_is_type_default(string parameterDefinition)
        {
            Assert.That("public interface ITest { void Method(" + parameterDefinition + "); }", HasContract(
                "public interface ITest",
                "{",
                "    void Method(" + parameterDefinition + ");",
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
        [TestCase("Optional x As System.DateTime = #1970/01/01#", "[System.Runtime.CompilerServices.DateTimeConstant(621355968000000000)] System.DateTime x = default")]
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
