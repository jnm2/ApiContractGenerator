using System.IO;
using ApiContractGenerator.Model;
using NUnit.Framework;

namespace ApiContractGenerator.Tests
{
    public static class CSharpTextFormatterTests
    {
        [TestCase((short)1000, "1000", TestName = "{m}((short)1000)")]
        [TestCase((short)10_000, "10_000", TestName = "{m}((short)10_000)")]
        [TestCase((short)-1000, "-1000", TestName = "{m}((short)-1000)")]
        [TestCase((short)-10_000, "-10_000", TestName = "{m}((short)-10_000)")]
        [TestCase((ushort)1000, "1000", TestName = "{m}((ushort)1000)")]
        [TestCase((ushort)10_000, "10_000", TestName = "{m}((ushort)10_000)")]
        [TestCase((int)1000, "1000", TestName = "{m}((int)1000)")]
        [TestCase((int)10_000, "10_000", TestName = "{m}((int)10_000)")]
        [TestCase((int)100_000, "100_000", TestName = "{m}((int)100_000)")]
        [TestCase((int)1_000_000, "1_000_000", TestName = "{m}((int)1_000_000)")]
        [TestCase((int)-1000, "-1000", TestName = "{m}((int)-1000)")]
        [TestCase((int)-10_000, "-10_000", TestName = "{m}((int)-10_000)")]
        [TestCase((int)-100_000, "-100_000", TestName = "{m}((int)-100_000)")]
        [TestCase((int)-1_000_000, "-1_000_000", TestName = "{m}((int)-1_000_000)")]
        [TestCase((uint)1000, "1000", TestName = "{m}((uint)1000)")]
        [TestCase((uint)10_000, "10_000", TestName = "{m}((uint)10_000)")]
        [TestCase((uint)100_000, "100_000", TestName = "{m}((uint)100_000)")]
        [TestCase((uint)1_000_000, "1_000_000", TestName = "{m}((uint)1_000_000)")]
        [TestCase((long)1000, "1000", TestName = "{m}((long)1000)")]
        [TestCase((long)10_000, "10_000", TestName = "{m}((long)10_000)")]
        [TestCase((long)100_000, "100_000", TestName = "{m}((long)100_000)")]
        [TestCase((long)1_000_000, "1_000_000", TestName = "{m}((long)1_000_000)")]
        [TestCase((long)-1000, "-1000", TestName = "{m}((long)-1000)")]
        [TestCase((long)-10_000, "-10_000", TestName = "{m}((long)-10_000)")]
        [TestCase((long)-100_000, "-100_000", TestName = "{m}((long)-100_000)")]
        [TestCase((long)-1_000_000, "-1_000_000", TestName = "{m}((long)-1_000_000)")]
        [TestCase((ulong)1000, "1000", TestName = "{m}((ulong)1000)")]
        [TestCase((ulong)10_000, "10_000", TestName = "{m}((ulong)10_000)")]
        [TestCase((ulong)100_000, "100_000", TestName = "{m}((ulong)100_000)")]
        [TestCase((ulong)1_000_000, "1_000_000", TestName = "{m}((ulong)1_000_000)")]
        [TestCase((float)1000, "1000", TestName = "{m}((float)1000)")]
        [TestCase((float)10_000, "10_000", TestName = "{m}((float)10_000)")]
        [TestCase((float)100_000, "100_000", TestName = "{m}((float)100_000)")]
        [TestCase((float)1_000_000, "1_000_000", TestName = "{m}((float)1_000_000)")]
        [TestCase((float)-1000, "-1000", TestName = "{m}((float)-1000)")]
        [TestCase((float)-10_000, "-10_000", TestName = "{m}((float)-10_000)")]
        [TestCase((float)-100_000, "-100_000", TestName = "{m}((float)-100_000)")]
        [TestCase((float)-1_000_000, "-1_000_000", TestName = "{m}((float)-1_000_000)")]
        [TestCase((float)1000.1, "1000.1", TestName = "{m}((float)1000.1)")]
        [TestCase((float)10_000.1, "10_000.1", TestName = "{m}((float)10_000.1)")]
        [TestCase((float)100_000.1, "100_000.1", TestName = "{m}((float)100_000.1)")]
        [TestCase((float)-1000.1, "-1000.1", TestName = "{m}((float)-1000.1)")]
        [TestCase((float)-10_000.1, "-10_000.1", TestName = "{m}((float)-10_000.1)")]
        [TestCase((float)-100_000.1, "-100_000.1", TestName = "{m}((float)-100_000.1)")]
        [TestCase((double)1000, "1000", TestName = "{m}((double)1000)")]
        [TestCase((double)10_000, "10_000", TestName = "{m}((double)10_000)")]
        [TestCase((double)100_000, "100_000", TestName = "{m}((double)100_000)")]
        [TestCase((double)1_000_000, "1_000_000", TestName = "{m}((double)1_000_000)")]
        [TestCase((double)-1000, "-1000", TestName = "{m}((double)-1000)")]
        [TestCase((double)-10_000, "-10_000", TestName = "{m}((double)-10_000)")]
        [TestCase((double)-100_000, "-100_000", TestName = "{m}((double)-100_000)")]
        [TestCase((double)-1_000_000, "-1_000_000", TestName = "{m}((double)-1_000_000)")]
        [TestCase((double)1000.1, "1000.1", TestName = "{m}((double)1000.1)")]
        [TestCase((double)10_000.1, "10_000.1", TestName = "{m}((double)10_000.1)")]
        [TestCase((double)100_000.1, "100_000.1", TestName = "{m}((double)100_000.1)")]
        [TestCase((double)1_000_000.1, "1_000_000.1", TestName = "{m}((double)1_000_000.1)")]
        [TestCase((double)-1000.1, "-1000.1", TestName = "{m}((double)-1000.1)")]
        [TestCase((double)-10_000.1, "-10_000.1", TestName = "{m}((double)-10_000.1)")]
        [TestCase((double)-100_000.1, "-100_000.1", TestName = "{m}((double)-100_000.1)")]
        [TestCase((double)-1_000_000.1, "-1_000_000.1", TestName = "{m}((double)-1_000_000.1)")]
        public static void Decimal_literals_use_digit_separator_when_at_least_five_digits(object value, string expected)
        {
            using var result = new StringWriter();
            new CSharpTextFormatter(result, null).WriteConstantPrimitive(MetadataConstantValue.FromObject(value));
            Assert.That(result.ToString(), Is.EqualTo(expected));
        }
    }
}
