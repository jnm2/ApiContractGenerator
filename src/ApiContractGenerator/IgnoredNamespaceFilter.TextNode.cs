using System;

namespace ApiContractGenerator
{
    partial class IgnoredNamespaceFilter
    {
        private sealed class TextNode
        {
            public TextNode(TextNode previous, string text)
            {
                Previous = previous;
                Text = text;
            }

            public TextNode Previous { get; }
            public string Text { get; }

            public static TextNode operator +(TextNode previous, string text) => new TextNode(previous, text);

            public static TextNode Root(string text) => new TextNode(null, text);

            public static string[] ToArray(TextNode node)
            {
                var count = 0;
                for (var current = node; current != null; current = current.Previous)
                    count++;

                if (count == 0) return Array.Empty<string>();

                var builder = new string[count];

                var i = count - 1;
                for (var current = node; current != null; current = current.Previous)
                {
                    builder[i] = current.Text;
                    i--;
                }

                return builder;
            }
        }
    }
}
