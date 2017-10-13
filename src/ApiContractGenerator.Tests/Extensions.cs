using System;
using System.IO;

namespace ApiContractGenerator.Tests
{
    internal static class Extensions
    {
        public static MemoryStream CreateReadOnlyView(this MemoryStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (!stream.TryGetBuffer(out var buffer))
                throw new ArgumentException("Cannot access buffer.", nameof(stream));

            return new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, writable: false);
        }
    }
}
