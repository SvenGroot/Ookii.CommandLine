// Copyright (c) Sven Groot (Ookii.org)
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Terminal;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests
{
    [TestClass()]
    public class LineWrappingTextWriterTest
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
            using (TextWriter baseWriter = new StringWriter())
            using (LineWrappingTextWriter target = new LineWrappingTextWriter(baseWriter, maximumLineLength, disposeBaseWriter))
            {
                Assert.AreEqual(baseWriter, target.BaseWriter);
                Assert.AreEqual(maximumLineLength, target.MaximumLineLength);
                Assert.AreEqual(0, target.Indent);
                Assert.AreEqual(baseWriter.Encoding, target.Encoding);
                Assert.AreEqual(baseWriter.FormatProvider, target.FormatProvider);
                Assert.AreEqual(baseWriter.NewLine, target.NewLine);
            }
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
        public void TestSplitLineBreakNoMaximum()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter();
            writer.Indent = 4;
            writer.Write("Foo\r");
            writer.Write("Bar\r");
            writer.Write("\nBaz");
            string expected = "Foo\n    Bar\n    Baz".ReplaceLineEndings();
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
            writer.WriteLine("This is a test");
            writer.Write("Unfinished second line");
            writer.Flush(false);

            var expected = $"This is a test\nUnfinished second line".ReplaceLineEndings();
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
            return writer.BaseWriter.ToString();
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
            return writer.BaseWriter.ToString();
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

        #region Input and expected values

        private static readonly string _input = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
quam pellentesque nec
nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum consectetur.
Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat. Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae sapien pellentesque habitant morbi tristique senectus et netus.
Augue neque gravida in
fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae elementum curabitur.

01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789".ReplaceLineEndings();

        private static readonly string _expectedNoIndent = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus
nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
quam pellentesque nec
nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum consectetur.
Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat. Malesuada
fames ac turpis egestas maecenas pharetra. Fringilla est
ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae sapien
pellentesque habitant morbi tristique senectus et netus.
Augue neque gravida in
fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae
elementum curabitur.

01234567890123456789012345678901234567890123456789012345678901234567890123456789
01234567890123456789012345678901234567890123456789012345678901234567890123456789
0123456789012345678901234567890123456789
".ReplaceLineEndings();

        private static readonly string _expectedIndent = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
        incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique
        risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
        quam pellentesque nec
        nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum
        consectetur.
        Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat.
        Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
        ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae
        sapien pellentesque habitant morbi tristique senectus et netus.
        Augue neque gravida in
        fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae
        elementum curabitur.

01234567890123456789012345678901234567890123456789012345678901234567890123456789
        012345678901234567890123456789012345678901234567890123456789012345678901
        234567890123456789012345678901234567890123456789
".ReplaceLineEndings();

        private static readonly string _expectedIndentChanges = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
    incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique
    risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
    quam pellentesque nec
    nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum
    consectetur.
    Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat.
    Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
    ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae
    sapien pellentesque habitant morbi tristique senectus et netus.
    Augue neque gravida in
    fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae
    elementum curabitur.

01234567890123456789012345678901234567890123456789012345678901234567890123456789
    0123456789012345678901234567890123456789012345678901234567890123456789012345
    67890123456789012345678901234567890123456789
    Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing
        tristique risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
        quam pellentesque nec
        nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum
        consectetur.
        Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat.
        Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
        ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae
        sapien pellentesque habitant morbi tristique senectus et netus.
        Augue neque gravida in
        fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae
        elementum curabitur.

01234567890123456789012345678901234567890123456789012345678901234567890123456789
        012345678901234567890123456789012345678901234567890123456789012345678901
        234567890123456789012345678901234567890123456789
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
        incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique
        risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
        quam pellentesque nec
        nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum
        consectetur.
        Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat.
        Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
        ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae
        sapien pellentesque habitant morbi tristique senectus et netus.
        Augue neque gravida in
        fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae
        elementum curabitur.

01234567890123456789012345678901234567890123456789012345678901234567890123456789
        012345678901234567890123456789012345678901234567890123456789012345678901
        234567890123456789012345678901234567890123456789
".ReplaceLineEndings();

        private static readonly string _expectedIndentNoMaximum = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
        quam pellentesque nec
        nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum consectetur.
        Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat. Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
        ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae sapien pellentesque habitant morbi tristique senectus et netus.
        Augue neque gravida in
        fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae elementum curabitur.

01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789".ReplaceLineEndings();

        private static readonly string _inputFormatting = "\x1b[34mLorem \x1b[34mipsum \x1b[34mdolor \x1b[34msit \x1b[34mamet, \x1b[34mconsectetur \x1b[34madipiscing \x1b[34melit, \x1b]0;new title\x1b\\sed do \x1b]0;new title2\x0007eiusmod \x1b(Btempor\x1bH incididunt\nut labore et dolore magna aliqua. Donec\x1b[38;2;1;2;3m adipiscing tristique risus nec feugiat in fermentum.\x1b[0m".ReplaceLineEndings();

        private static readonly string _expectedFormatting = @"[34mLorem [34mipsum [34mdolor [34msit [34mamet, [34mconsectetur [34madipiscing [34melit, ]0;new title\sed do ]0;new title2eiusmod (BtemporH
        incididunt
        ut labore et dolore magna aliqua. Donec[38;2;1;2;3m adipiscing tristique risus nec
        feugiat in fermentum.[0m
".ReplaceLineEndings();

        private static readonly string _expectedFormattingCounted = @"[34mLorem [34mipsum [34mdolor [34msit [34mamet, [34mconsectetur
        [34madipiscing [34melit, ]0;new title\sed do ]0;new title2eiusmod
        (BtemporH incididunt
        ut labore et dolore magna aliqua. Donec[38;2;1;2;3m adipiscing
        tristique risus nec feugiat in fermentum.[0m
".ReplaceLineEndings();

        private const string _inputLongFormatting = "Lorem ipsum dolor sit amet, consectetur\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in fermentum.";

        private static readonly string _expectedLongFormatting = @"Lorem ipsum dolor sit amet, consectetur[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m adipiscing elit, sed do eiusmod tempor
        incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique
        risus nec feugiat in fermentum.
".ReplaceLineEndings();

        #endregion
    }
}
