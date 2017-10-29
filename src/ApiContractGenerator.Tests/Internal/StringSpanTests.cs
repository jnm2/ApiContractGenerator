using ApiContractGenerator.Internal;
using NUnit.Framework;

namespace ApiContractGenerator.Tests.Internal
{
    public static class StringSpanTests
    {
        [Test]
        public static void IndexOf_applies_offset_to_return([Range(0, 5)] int beforeSlice)
        {
            var span = (new string('.', beforeSlice) + '!').Slice(beforeSlice);
            Assert.That(span.IndexOf('!'), Is.EqualTo(0));
        }

        [Test]
        public static void LastIndexOf_applies_offset_to_return([Range(0, 5)] int beforeSlice)
        {
            var span = (new string('.', beforeSlice) + '!').Slice(beforeSlice);
            Assert.That(span.LastIndexOf('!'), Is.EqualTo(0));
        }
    }
}
