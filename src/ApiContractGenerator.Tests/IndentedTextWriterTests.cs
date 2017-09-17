using System;
using System.IO;
using NUnit.Framework;

namespace ApiContractGenerator.Tests
{
    public static class IndentedTextWriterTests
    {
        private const string Indent = "    ";

        [Test]
        public static void Initial_write_should_contain_indent()
        {
            var withIndent = new StringWriter();
            using (var indentedWriter = new IndentedTextWriter(withIndent))
            {
                indentedWriter.Indent();
                indentedWriter.Write("x");
            }

            Assert.That(withIndent.ToString(), Is.EqualTo(Indent + "x"));
        }

        [Test]
        public static void Indent_should_apply_to_newlines_within_value()
        {
            var withIndent = new StringWriter();
            using (var indentedWriter = new IndentedTextWriter(withIndent))
            {
                indentedWriter.Indent();
                indentedWriter.Write("x" + Environment.NewLine + "x");
            }

            Assert.That(withIndent.ToString(), Is.EqualTo(Indent + "x" + Environment.NewLine + Indent + "x"));
        }

        [Test]
        public static void Indent_should_not_apply_to_empty_lines_within_value()
        {
            var withIndent = new StringWriter();
            using (var indentedWriter = new IndentedTextWriter(withIndent))
            {
                indentedWriter.Indent();
                indentedWriter.Write("x" + Environment.NewLine + Environment.NewLine + "x");
            }

            Assert.That(withIndent.ToString(), Is.EqualTo(Indent + "x" + Environment.NewLine + Environment.NewLine + Indent + "x"));
        }

        [Test]
        public static void Empty_line_should_not_contain_indent()
        {
            var withIndent = new StringWriter();
            using (var indentedWriter = new IndentedTextWriter(withIndent))
            {
                indentedWriter.Indent();
                indentedWriter.WriteLine("x");
                indentedWriter.WriteLine();
                indentedWriter.Write("x");
            }

            var withoutIndent = new StringWriter();
            using (var indentedWriter = new IndentedTextWriter(withoutIndent))
            {
                indentedWriter.Indent();
                indentedWriter.WriteLine("x");
                indentedWriter.Unindent();
                indentedWriter.WriteLine();
                indentedWriter.Indent();
                indentedWriter.Write("x");
            }

            Assert.That(withIndent.ToString(), Is.EqualTo(withoutIndent.ToString()));
        }
    }
}
