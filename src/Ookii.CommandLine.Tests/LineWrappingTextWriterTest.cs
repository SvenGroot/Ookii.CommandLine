// Copyright (c) Sven Groot (Ookii.org)
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Terminal;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests
{
    [TestClass()]
    public partial class LineWrappingTextWriterTest
    {
        [TestMethod()]
        public void TestWriteCharArray()
        {
            const int maxLength = 80;

            Assert.AreEqual(_expectedNoIndent, WriteCharArray(_input.ToCharArray(), maxLength, _input.Length));
            // write it again, in pieces exactly as long as the max line length
            Assert.AreEqual(_expectedNoIndent, WriteCharArray(_input.ToCharArray(), maxLength, maxLength));
            // And again, in pieces less than the max line length
            Assert.AreEqual(_expectedNoIndent, WriteCharArray(_input.ToCharArray(), maxLength, 50));
        }

        [TestMethod()]
        public void TestWriteString()
        {
            const int maxLength = 80;

            Assert.AreEqual(_expectedNoIndent, WriteString(_input, maxLength, _input.Length));
            // Write it again, in pieces exactly as long as the max line length.
            Assert.AreEqual(_expectedNoIndent, WriteString(_input, maxLength, maxLength));
            // And again, in pieces less than the max line length.
            Assert.AreEqual(_expectedNoIndent, WriteString(_input, maxLength, 50));
        }

        [TestMethod()]
        public async Task TestWriteStringAsync()
        {
            const int maxLength = 80;

            Assert.AreEqual(_expectedNoIndent, await WriteStringAsync(_input, maxLength, _input.Length));
            // Write it again, in pieces exactly as long as the max line length.
            Assert.AreEqual(_expectedNoIndent, await WriteStringAsync(_input, maxLength, maxLength));
            // And again, in pieces less than the max line length.
            Assert.AreEqual(_expectedNoIndent, await WriteStringAsync(_input, maxLength, 50));
        }


        [TestMethod()]
        public void TestWriteStringNoMaximum()
        {
            const int maxLength = 0;

            Assert.AreEqual(_input, WriteString(_input, maxLength, _input.Length));
            // Write it again, in pieces.
            Assert.AreEqual(_input, WriteString(_input, maxLength, 80));
        }

        [TestMethod()]
        public void TestWriteCharArrayNoMaximum()
        {
            const int maxLength = 0;

            Assert.AreEqual(_input, WriteCharArray(_input.ToCharArray(), maxLength, _input.Length));
            // Write it again, in pieces.
            Assert.AreEqual(_input, WriteCharArray(_input.ToCharArray(), maxLength, 80));
        }


        [TestMethod()]
        public void TestWriteUnixLineEnding()
        {
            const int maxLength = 80;
            var input = _input.ReplaceLineEndings("\n");
            Assert.AreEqual(_expectedNoIndent, WriteString(input, maxLength, input.Length));

            using var writer = LineWrappingTextWriter.ForStringWriter(maxLength);
            writer.NewLine = "\n";
            var expected = _expectedNoIndent.ReplaceLineEndings("\n");
            Assert.AreEqual(expected, WriteString(writer, input, input.Length));
        }

        [TestMethod()]
        public void TestWriteWindowsLineEnding()
        {
            const int maxLength = 80;
            var input = _input.ReplaceLineEndings("\r\n");
            Assert.AreEqual(_expectedNoIndent, WriteString(input, maxLength, input.Length));

            using var writer = LineWrappingTextWriter.ForStringWriter(maxLength);
            writer.NewLine = "\r\n";
            var expected = _expectedNoIndent.ReplaceLineEndings("\r\n");
            Assert.AreEqual(expected, WriteString(writer, input, input.Length));
        }

        [TestMethod()]
        public void TestIndentString()
        {
            const int maxLength = 80;
            const int indent = 8;

            Assert.AreEqual(_expectedIndent, WriteString(_input, maxLength, _input.Length, indent));
            // Write it again, in pieces exactly as long as the max line length.
            Assert.AreEqual(_expectedIndent, WriteString(_input, maxLength, maxLength, indent));
            // And again, in pieces less than the max line length.
            Assert.AreEqual(_expectedIndent, WriteString(_input, maxLength, 50, indent));
        }

        [TestMethod()]
        public void TestIndentCharArray()
        {
            const int maxLength = 80;
            const int indent = 8;

            Assert.AreEqual(_expectedIndent, WriteCharArray(_input.ToCharArray(), maxLength, _input.Length, indent));
            // Write it again, in pieces exactly as long as the max line length.
            Assert.AreEqual(_expectedIndent, WriteCharArray(_input.ToCharArray(), maxLength, maxLength, indent));
            // And again, in pieces less than the max line length.
            Assert.AreEqual(_expectedIndent, WriteCharArray(_input.ToCharArray(), maxLength, 50, indent));
        }

        [TestMethod()]
        public void TestIndentChanges()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(80);
            writer.Indent = 4;
            writer.WriteLine(_input);
            writer.Indent = 8;
            writer.Write(_input.Trim());
            // Should add a new line.
            writer.ResetIndent();
            writer.WriteLine(_input.Trim());
            // Should not add a new line.
            writer.ResetIndent();
            writer.Flush();

            Assert.AreEqual(_expectedIndentChanges, writer.BaseWriter.ToString());
        }

        [TestMethod()]
        public async Task TestIndentChangesAsync()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(80);
            writer.Indent = 4;
            await writer.WriteLineAsync(_input);
            writer.Indent = 8;
            await writer.WriteLineAsync(_input.Trim());
            // Should add a new line.
            await writer.ResetIndentAsync();
            await writer.WriteLineAsync(_input.Trim());
            // Should not add a new line.
            await writer.ResetIndentAsync();
            await writer.FlushAsync();

            Assert.AreEqual(_expectedIndentChanges, writer.BaseWriter.ToString());
        }

        [TestMethod()]
        public void TestIndentStringNoMaximum()
        {
            const int maxLength = 0;
            const int indent = 8;

            Assert.AreEqual(_expectedIndentNoMaximum, WriteString(_input, maxLength, _input.Length, indent));
            // Write it again, in pieces.
            Assert.AreEqual(_expectedIndentNoMaximum, WriteString(_input, maxLength, 80, indent));
        }

        [TestMethod()]
        public void TestIndentCharArrayNoMaximum()
        {
            const int maxLength = 0;
            const int indent = 8;

            Assert.AreEqual(_expectedIndentNoMaximum, WriteCharArray(_input.ToCharArray(), maxLength, _input.Length, indent));
            // Write it again, in pieces.
            Assert.AreEqual(_expectedIndentNoMaximum, WriteCharArray(_input.ToCharArray(), maxLength, 80, indent));
        }

        /// <summary>
        ///A test for LineWrappingTextWriter Constructor
        ///</summary>
        [TestMethod()]
        public void TestConstructor()
        {
            int maximumLineLength = 85;
            bool disposeBaseWriter = true;
            using TextWriter baseWriter = new StringWriter();
            using LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter);
            Assert.AreEqual(baseWriter, target.BaseWriter);
            Assert.AreEqual(maximumLineLength, target.MaximumLineLength);
            Assert.AreEqual(0, target.Indent);
            Assert.AreEqual(baseWriter.Encoding, target.Encoding);
            Assert.AreEqual(baseWriter.FormatProvider, target.FormatProvider);
            Assert.AreEqual(baseWriter.NewLine, target.NewLine);
            Assert.AreEqual(WrappingMode.Enabled, target.Wrapping);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTestBaseWriterNull()
        {
            new LineWrappingTextWriter(null, 0, false);
        }

        [TestMethod()]
        public void TestDisposeBaseWriterTrue()
        {
            using (TextWriter baseWriter = new StringWriter())
            {
                using (LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, 80, true))
                {
                    target.Write("test");
                }

                try
                {
                    baseWriter.Write("foo");
                    Assert.Fail("base writer not disposed");
                }
                catch (ObjectDisposedException)
                {
                }

                Assert.AreEqual("test\n".ReplaceLineEndings(), baseWriter.ToString());
            }
        }

        [TestMethod]
        public void TestDisposeBaseWriterFalse()
        {
            using (TextWriter baseWriter = new StringWriter())
            {
                using (LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, 80, false))
                {
                    target.Write("test");
                }

                // This will throw if the base writer was disposed.
                baseWriter.Write("foo");

                Assert.AreEqual("test\nfoo".ReplaceLineEndings(), baseWriter.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestIndentTooSmall()
        {
            using (LineWrappingTextWriter target = LineWrappingTextWriter.ForStringWriter(80))
            {
                target.Indent = -1;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestIndentTooLarge()
        {
            using (LineWrappingTextWriter target = LineWrappingTextWriter.ForStringWriter(80))
            {
                target.Indent = target.MaximumLineLength;
            }
        }

        [TestMethod]
        public void TestSkipFormatting()
        {
            Assert.AreEqual(_expectedFormatting, WriteString(_inputFormatting, 80, _inputFormatting.Length, 8));
            Assert.AreEqual(_expectedLongFormatting, WriteString(_inputLongFormatting, 80, _inputLongFormatting.Length, 8));
            Assert.AreEqual(_expectedLongFormatting, WriteString(_inputLongFormatting, 80, 80, 8));
            Assert.AreEqual(_expectedLongFormatting, WriteString(_inputLongFormatting, 80, 50, 8));
            Assert.AreEqual(_expectedLongFormatting, WriteChars(_inputLongFormatting.ToCharArray(), 80, 8));
        }

        [TestMethod]
        public void TestSkipFormattingNoMaximum()
        {
            Assert.AreEqual(_inputFormatting.ReplaceLineEndings(), WriteString(_inputFormatting, 0, _inputFormatting.Length, 0));
        }

        [TestMethod]
        public void TestCountFormatting()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(80, null, true);
            writer.Indent = 8;
            Assert.AreEqual(_expectedFormattingCounted, WriteString(writer, _inputFormatting, _inputFormatting.Length));
        }

        [TestMethod]
        public void TestSplitFormatting()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(14);
            writer.Write("Hello \x1b[38;2");
            writer.Write(";1;2");
            writer.Write(";3mWorld and stuff Bye\r");
            writer.Write("\nEveryone");
            writer.Flush();
            string expected = "Hello \x1b[38;2;1;2;3mWorld\nand stuff Bye\nEveryone\n".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestSplitLineBreak()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(80);
            writer.Indent = 4;
            writer.Write("Foo\r");
            writer.Write("Bar\r");
            writer.Write("\nBaz\r");
            writer.Write("\rOne\r");
            writer.Write("\r\nTwo\r\n");
            string expected = "Foo\n    Bar\n    Baz\n\nOne\n\nTwo\n".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestSplitLineBreakNoMaximum()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter();
            writer.Indent = 4;
            writer.Write("Foo\r");
            writer.Write("Bar\r");
            writer.Write("\nBaz\r");
            writer.Write("\rOne\r");
            writer.Write("\r\nTwo\r\n");
            string expected = "Foo\n    Bar\n    Baz\n\nOne\n\nTwo\n".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestWriteChar()
        {
            Assert.AreEqual(_expectedIndent, WriteChars(_input.ToCharArray(), 80, 8));
        }

        [TestMethod]
        public void TestWriteCharFormatting()
        {
            Assert.AreEqual(_expectedFormatting, WriteChars(_inputFormatting.ToCharArray(), 80, 8));
        }

        [TestMethod]
        public void TestFlush()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(40);
            writer.Write(TextFormat.ForegroundBlue);
            writer.WriteLine("This is a test");
            writer.Write(TextFormat.Default);
            writer.Flush();

            var expected = $"{TextFormat.ForegroundBlue}This is a test\n{TextFormat.Default}\n".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestFlushNoNewLine()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(40);
            writer.Indent = 4;
            writer.WriteLine("This is a test");
            writer.Write("Unfinished second line");
            writer.Flush(false);

            var expected = "This is a test\n    Unfinished second line".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.BaseWriter.ToString());

            writer.Write("more text");
            writer.Flush(false);
            expected = "This is a test\n    Unfinished second linemore text".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.BaseWriter.ToString());
            writer.WriteLine();
            writer.WriteLine("Another line");
            writer.WriteLine("And another");
            expected = "This is a test\n    Unfinished second linemore text\nAnother line\n    And another\n".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestResetIndent()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(40);
            writer.Write(TextFormat.ForegroundBlue);
            writer.WriteLine("This is a test");
            writer.Write(TextFormat.Default);
            writer.ResetIndent();
            writer.WriteLine("Hello");

            var expected = $"{TextFormat.ForegroundBlue}This is a test\n{TextFormat.Default}Hello\n".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestToString()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(40);
            writer.WriteLine("This is a test");
            writer.Write("Unfinished second\x1b[34m line\x1b[0m");
            var expected = "This is a test\nUnfinished second\x1b[34m line\x1b[0m".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.ToString());
            expected = "This is a test\n".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.BaseWriter.ToString());

            using var writer2 = LineWrappingTextWriter.ForConsoleOut();
            Assert.AreEqual(typeof(LineWrappingTextWriter).FullName, writer2.ToString());
        }

        [TestMethod]
        public void TestWrappingMode()
        {
            {
                using var writer = LineWrappingTextWriter.ForStringWriter(80);
                writer.Indent = 4;
                writer.WriteLine(_inputWrappingMode);
                writer.Wrapping = WrappingMode.Disabled;
                writer.WriteLine(_inputWrappingMode);
                writer.Wrapping = WrappingMode.Enabled;
                writer.WriteLine(_inputWrappingMode);
                Assert.AreEqual(_expectedWrappingMode, writer.ToString());
            }

            // Make sure the buffer is cleared if not empty.
            {
                using var writer = LineWrappingTextWriter.ForStringWriter(80);
                writer.Indent = 4;
                writer.Write(_inputWrappingMode);
                writer.Wrapping = WrappingMode.Disabled;
                writer.Write(_inputWrappingMode);
                writer.Wrapping = WrappingMode.Enabled;
                writer.Write(_inputWrappingMode);
                Assert.AreEqual(_expectedWrappingModeWrite, writer.ToString());
            }

            // Test EnabledNoForce
            {
                using var writer = LineWrappingTextWriter.ForStringWriter(80);
                writer.Indent = 4;
                writer.Wrapping = WrappingMode.EnabledNoForce;
                writer.Write(_inputWrappingMode);
                writer.Write(_inputWrappingMode);
                writer.Wrapping = WrappingMode.Enabled;
                writer.Write(_inputWrappingMode);
                Assert.AreEqual(_expectedWrappingModeNoForce, writer.ToString());
            }

            // Should be false and unchangeable if no maximum length.
            {
                using var writer = LineWrappingTextWriter.ForStringWriter();
                Assert.AreEqual(WrappingMode.Disabled, writer.Wrapping);
                writer.Wrapping = WrappingMode.Enabled;
                Assert.AreEqual(WrappingMode.Disabled, writer.Wrapping);
            }
        }

        [TestMethod]
        public void TestExactLineLength()
        {
            // This tests for a situation where a line is the exact length of the ring buffer,
            // but the buffer start is not zero. This can only happen if countFormatting is true
            // otherwise the buffer is made larger than the line length to begin with.
            using var writer = LineWrappingTextWriter.ForStringWriter(40, null, true);
            writer.WriteLine("test");
            writer.Write("1234 1234 1234 1234 1234 1234 1234 12345");
            writer.Write("1234 1234 1234 1234 1234 1234 1234 12345");
            var expected = "test\n1234 1234 1234 1234 1234 1234 1234\n123451234 1234 1234 1234 1234 1234 1234\n12345".ReplaceLineEndings();
            Assert.AreEqual(expected, writer.ToString());
        }

        private static string WriteString(string value, int maxLength, int segmentSize, int indent = 0)
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(maxLength);
            writer.Indent = indent;
            return WriteString(writer, value, segmentSize);
        }

        private static string WriteString(LineWrappingTextWriter writer, string value, int segmentSize)
        {
            for (int i = 0; i < value.Length; i += segmentSize)
            {
                // Ignore the suggestion to use AsSpan, we want to call the string overload.
                writer.Write(value.Substring(i, Math.Min(value.Length - i, segmentSize)));
            }

            writer.Flush();
            return writer.ToString();
        }

        private static async Task<string> WriteStringAsync(string value, int maxLength, int segmentSize, int indent = 0)
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(maxLength);
            writer.Indent = indent;
            return await WriteStringAsync(writer, value, segmentSize);
        }

        private static async Task<string> WriteStringAsync(LineWrappingTextWriter writer, string value, int segmentSize)
        {
            for (int i = 0; i < value.Length; i += segmentSize)
            {
                // Ignore the suggestion to use AsSpan, we want to call the string overload.
                await writer.WriteAsync(value.Substring(i, Math.Min(value.Length - i, segmentSize)));
            }

            await writer.FlushAsync();
            return writer.ToString();
        }


        private static string WriteCharArray(char[] value, int maxLength, int segmentSize, int indent = 0)
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(maxLength);
            writer.Indent = indent;
            for (int i = 0; i < value.Length; i += segmentSize)
            {
                writer.Write(value, i, Math.Min(value.Length - i, segmentSize));
            }

            writer.Flush();
            return writer.BaseWriter.ToString();
        }

        private static string WriteChars(char[] value, int maxLength, int indent = 0)
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(maxLength);
            writer.Indent = indent;
            foreach (var ch in value)
            {
                writer.Write(ch);
            }

            writer.Flush();
            return writer.BaseWriter.ToString();
        }
    }
}
