using System;
using System.Diagnostics;

namespace ApiContractGenerator.Internal
{
    [DebuggerDisplay("{ToString()}")]
    public struct StringSpan
    {
        private readonly string value;
        private readonly int start;
        private readonly int length;

        private StringSpan(string value, int start, int length)
        {
            this.value = value;
            this.start = start;
            this.length = length;
        }

        public StringSpan Slice(int start)
        {
            if (start < 0 || start > length) throw new ArgumentOutOfRangeException(nameof(start), start, "Start must be greater than or equal to zero and less than or equal to the current length.");
            return new StringSpan(value, this.start + start, length - start);
        }

        public StringSpan Slice(int start, int length)
        {
            if (start < 0 || start > this.length) throw new ArgumentOutOfRangeException(nameof(start), start, "Start must be greater than or equal to zero and less than or equal to the current length.");
            if (length < 0 || start + length > this.length) throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be greater than or equal to zero and less than or equal to the current length minus the start.");
            return new StringSpan(value, this.start + start, length);
        }

        public static implicit operator StringSpan(string value) => new StringSpan(value, 0, value.Length);

        public static explicit operator string(StringSpan value) => value.ToString();

        public override string ToString() => value.Substring(start, length);

        public int IndexOf(char value)
        {
            return this.value.IndexOf(value, start, length);
        }

        public int LastIndexOf(char value)
        {
            return this.value.LastIndexOf(value, start + length - 1, length);
        }
    }

    public static class StringSpanExtensions
    {
        public static StringSpan Slice(this string value, int start)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return ((StringSpan)value).Slice(start);
        }

        public static StringSpan Slice(this string value, int start, int length)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return ((StringSpan)value).Slice(start, length);
        }
    }
}
