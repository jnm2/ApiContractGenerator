using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApiContractGenerator.Internal
{
    public sealed class IndentedTextWriter : TextWriter
    {
        private readonly TextWriter target;
        private int indent;
        private readonly string indentChars = "    ";
        private char[] indentBuffer = Array.Empty<char>();

        /// <summary>
        /// Indicates whether you're at the start of a new line and the indent has yet not been written
        /// </summary>
        private bool pendingIndent = true;

        public IndentedTextWriter(TextWriter target, string indent = "    ")
        {
            this.target = target;
            indentChars = indent ?? string.Empty;
        }

        public void Indent()
        {
            indent++;
        }

        public void Unindent()
        {
            if (indent == 0) throw new InvalidOperationException("More unindents than indents.");
            indent--;
        }

        private char[] GetIndentBuffer()
        {
            if (indentBuffer.Length < indent * indentChars.Length)
            {
                indentBuffer = new char[indent * indentChars.Length];
                for (var i = 0; i < indent; i++)
                    indentChars.CopyTo(0, indentBuffer, i * indentChars.Length, indentChars.Length);
            }
            return indentBuffer;
        }
        private void WriteIndent()
        {
            target.Write(GetIndentBuffer(), 0, indent * indentChars.Length);
        }
        private Task WriteIndentAsync()
        {
            return target.WriteAsync(GetIndentBuffer(), 0, indent * indentChars.Length);
        }

        private int nextNewLineMatchPosition;

        public override void Write(char value)
        {
            var newLine = NewLine;
            if (!string.IsNullOrEmpty(newLine) && value == newLine[nextNewLineMatchPosition])
            {
                nextNewLineMatchPosition++;
                if (nextNewLineMatchPosition == newLine.Length)
                {
                    nextNewLineMatchPosition = 0;
                    pendingIndent = true;
                }
            }
            else
            {
                nextNewLineMatchPosition = 0;
                if (pendingIndent)
                {
                    pendingIndent = false;
                    WriteIndent();
                }
            }

            target.Write(value);
        }

        public override Task WriteAsync(char value)
        {
            var newLine = NewLine;
            if (!string.IsNullOrEmpty(newLine) && value == newLine[nextNewLineMatchPosition])
            {
                nextNewLineMatchPosition++;
                if (nextNewLineMatchPosition == newLine.Length)
                {
                    nextNewLineMatchPosition = 0;
                    pendingIndent = true;
                }
            }
            else if (pendingIndent)
            {
                pendingIndent = false;
                return IndentThenWriteAsync(value);
            }

            return target.WriteAsync(value);
        }

        private async Task IndentThenWriteAsync(char value)
        {
            await WriteIndentAsync().ConfigureAwait(false);
            await WriteAsync(value).ConfigureAwait(false);
        }

        #region Performance optimization

        public override void WriteLine()
        {
            if (nextNewLineMatchPosition != 0)
            {
                base.WriteLine();
                return;
            }
            pendingIndent = true;
            target.WriteLine();
        }

        public override Task WriteLineAsync()
        {
            if (nextNewLineMatchPosition != 0) return base.WriteLineAsync();
            pendingIndent = true;
            return target.WriteLineAsync();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing) target.Dispose();
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            target.Flush();
        }

        public override Task FlushAsync()
        {
            return target.FlushAsync();
        }

        public override Encoding Encoding => target.Encoding;

        public override IFormatProvider FormatProvider => target.FormatProvider;

        public override string NewLine { get => target.NewLine; set => target.NewLine = value; }
    }
}
