using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApiContractGenerator
{
    public sealed class IndentedTextWriter : TextWriter
    {
        private readonly TextWriter target;
        private int indent;
        private const string IndentChars = "    ";
        private char[] indentBuffer = Array.Empty<char>();
        private bool pendingIndent;

        public IndentedTextWriter(TextWriter target)
        {
            this.target = target;
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
            if (indentBuffer.Length < indent * IndentChars.Length)
            {
                indentBuffer = new char[indent * IndentChars.Length];
                for (var i = 0; i < indent; i++)
                    IndentChars.CopyTo(0, indentBuffer, i * IndentChars.Length, IndentChars.Length);
            }
            return indentBuffer;
        }
        private void WriteIndent()
        {
            target.Write(GetIndentBuffer(), 0, indent * IndentChars.Length);
        }
        private Task WriteIndentAsync()
        {
            return target.WriteAsync(GetIndentBuffer(), 0, indent * IndentChars.Length);
        }



        public override void Write(char value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

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

        public override void Write(bool value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

        public override void Write(char[] buffer)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(buffer, index, count);
        }

        public override void Write(decimal value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

        public override void Write(double value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

        public override void Write(int value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

        public override void Write(long value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

        public override void Write(object value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

        public override void Write(float value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

        public override void Write(string value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(format, arg0, arg1);
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(format, arg0, arg1, arg2);
        }

        public override void Write(string format, params object[] arg)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(format, arg);
        }

        public override void Write(uint value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

        public override void Write(ulong value)
        {
            if (pendingIndent)
            {
                WriteIndent();
                pendingIndent = false;
            }
            target.Write(value);
        }

        public override async Task WriteAsync(char value)
        {
            if (pendingIndent)
            {
                await WriteIndentAsync().ConfigureAwait(false);
                pendingIndent = false;
            }
            await target.WriteAsync(value).ConfigureAwait(false);
        }

        public override async Task WriteAsync(char[] buffer, int index, int count)
        {
            if (pendingIndent)
            {
                await WriteIndentAsync().ConfigureAwait(false);
                pendingIndent = false;
            }
            await target.WriteAsync(buffer, index, count).ConfigureAwait(false);
        }

        public override async Task WriteAsync(string value)
        {
            if (pendingIndent)
            {
                await WriteIndentAsync().ConfigureAwait(false);
                pendingIndent = false;
            }
            await target.WriteAsync(value).ConfigureAwait(false);
        }

        public override void WriteLine()
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine();
            pendingIndent = true;
        }

        public override void WriteLine(bool value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override void WriteLine(char value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override void WriteLine(char[] buffer)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(buffer);
            pendingIndent = true;
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(buffer, index, count);
            pendingIndent = true;
        }

        public override void WriteLine(decimal value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override void WriteLine(double value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override void WriteLine(int value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override void WriteLine(long value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override void WriteLine(object value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override void WriteLine(float value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override void WriteLine(string value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override void WriteLine(string format, object arg0)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(format, arg0);
            pendingIndent = true;
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(format, arg0, arg1);
            pendingIndent = true;
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(format, arg0, arg1, arg2);
            pendingIndent = true;
        }

        public override void WriteLine(string format, params object[] arg)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(format, arg);
            pendingIndent = true;
        }

        public override void WriteLine(uint value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override void WriteLine(ulong value)
        {
            if (pendingIndent) WriteIndent();
            target.WriteLine(value);
            pendingIndent = true;
        }

        public override async Task WriteLineAsync()
        {
            if (pendingIndent) await WriteIndentAsync().ConfigureAwait(false);
            await target.WriteLineAsync().ConfigureAwait(false);
            pendingIndent = true;
        }

        public override async Task WriteLineAsync(char value)
        {
            if (pendingIndent) await WriteIndentAsync().ConfigureAwait(false);
            await target.WriteLineAsync(value).ConfigureAwait(false);
            pendingIndent = true;
        }

        public override async Task WriteLineAsync(char[] buffer, int index, int count)
        {
            if (pendingIndent) await WriteIndentAsync().ConfigureAwait(false);
            await target.WriteLineAsync(buffer, index, count).ConfigureAwait(false);
            pendingIndent = true;
        }

        public override async Task WriteLineAsync(string value)
        {
            if (pendingIndent) await WriteIndentAsync().ConfigureAwait(false);
            await target.WriteLineAsync(value).ConfigureAwait(false);
            pendingIndent = true;
        }

        public override Encoding Encoding => target.Encoding;

        public override IFormatProvider FormatProvider => target.FormatProvider;

        public override string NewLine { get => target.NewLine; set => target.NewLine = value; }
    }
}
