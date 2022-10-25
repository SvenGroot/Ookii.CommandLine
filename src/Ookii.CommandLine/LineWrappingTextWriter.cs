// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Implements a <see cref="TextWriter"/> that writes text to another <see cref="TextWriter"/>, wrapping
    /// lines at word boundaries at a specific maximum line length.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The <see cref="LineWrappingTextWriter"/> will buffer the data written to it until an explicit new line is present in the text, or until
    ///   the length of the buffered data exceeds the value of the <see cref="MaximumLineLength"/> property.
    /// </para>
    /// <para>
    ///   If the length of the buffered data exceeds the value of the <see cref="MaximumLineLength"/> property, the
    ///   <see cref="LineWrappingTextWriter"/> will attempt to find a white space character to break the line at. If such a white space character
    ///   is found, everything before that character is output to the <see cref="BaseWriter"/>, and everything after that character is kept
    ///   in the buffer. The white space character itself is not written to the output.
    /// </para>
    /// <para>
    ///   If no suitable place to break the line could be found, the line is broken at the maximum line length. This may occur in the middle
    ///   of a word.
    /// </para>
    /// <para>
    ///   After a line break (either one that was caused by wrapping or one that was part of the text), the next line is indented by the
    ///   number of characters specified by the <see cref="Indent"/> property. The length of the indentation counts towards the maximum line length.
    /// </para>
    /// <para>
    ///   When the <see cref="Flush"/> method is called, the current contents of the buffer are written to the <see cref="BaseWriter"/>, followed
    ///   by a new line, unless the buffer is empty. If the buffer contains only indentation, it is considered empty and no new line is written.
    ///   Calling <see cref="Flush"/> has the same effect as writing a new line to the <see cref="LineWrappingTextWriter"/> if the buffer is not empty.
    ///   The <see cref="LineWrappingTextWriter"/> is flushed when the <see cref="Dispose"/> method is called.
    /// </para>
    /// <para>
    ///   The <see cref="ResetIndent"/> property can be used to move the output position back to the beginning of the line. If the buffer is
    ///   not empty, is is first flushed and indentation is reset to zero on the next line. After the next line break, indentation will again
    ///   be set to the value of the <see cref="Indent"/> property.
    /// </para>
    /// </remarks>
    public class LineWrappingTextWriter : TextWriter
    {
        #region Nested types

        private enum SegmentType
        {
            Text,
            Formatting,
            LineBreak,
        }

        [DebuggerDisplay("Type = {Type}, Indent = {Indent}, {Content}")]
        private struct Segment
        {
            public Segment(SegmentType segmentType, StringBuffer? content = null, int indent = 0)
            {
                Type = segmentType;
                Content = content;
                Indent = indent;
            }

            public StringBuffer? Content { get; set; }
            public SegmentType Type { get; set; }
            public int Indent { get; set; }

            public int ContentLength => 
                Type switch
                {
                    SegmentType.Formatting => 0,
                    _ => (Content?.Length ?? 0) + Indent,
                };


            public void WriteTo(TextWriter writer)
            {
                if (Indent > 0)
                    WriteIndent(writer, Indent);

                if (Content != null)
                    Content.Value.WriteTo(writer);
            }
        }

        private struct StringBuffer
        {
            // Wish I could use ReadOnlySpan...
            private readonly ArraySegment<char> _value;

            private static readonly char[] _segmentSeparators = { '\r', '\n', VirtualTerminal.Escape };

            public StringBuffer(char[] value)
                : this(new ArraySegment<char>(value))
            { 
            }

            public StringBuffer(char[] value, int offset, int length)
                : this(new ArraySegment<char>(value, offset, length))
            { 
            }

            public StringBuffer(ArraySegment<char> value)
            {
                _value = value;
            }

            public StringBuffer(string value)
                : this(value.ToCharArray())
            {
            }

            public IEnumerable<char> Characters => _value;

            public char this[int index] => _value.Array![index + _value.Offset];

            public int Length => _value.Count;

            public int SkipLineBreak(int index)
            {
                Debug.Assert(this[index] == '\r' || this[index] == '\n');
                if (this[index] == '\r' && index + 1 < Length && this[index + 1] == '\n')
                    return index + 2; // Windows line ending
                else
                    return index + 1;
            }

            private int IndexOfAny(char[] separators, int start)
            {
                int index = Array.FindIndex(_value.Array!, _value.Offset + start, _value.Count - start, ch => separators.Contains(ch));
                if (index >= 0)
                    index -= _value.Offset;

                return index;
            }

            public IEnumerable<Segment> Split(bool newLinesOnly)
            {
                int pos = 0;
                int segmentStart = pos;
                while (pos < Length)
                {
                    var separatorIndex = IndexOfAny(_segmentSeparators, pos);
                    if (separatorIndex < 0)
                    {
                        yield return new Segment(SegmentType.Text, this.Slice(segmentStart));
                        break;
                    }

                    int end;
                    if (this[separatorIndex] == VirtualTerminal.Escape)
                    {
                        if (newLinesOnly)
                        {
                            pos = separatorIndex + 1;
                            continue;
                        }

                        end = VirtualTerminal.FindSequenceEnd(Characters.Skip(separatorIndex + 1));
                        end += separatorIndex + 1;

                        if (separatorIndex > segmentStart)
                            yield return new Segment(SegmentType.Text, this.Slice(segmentStart, separatorIndex - segmentStart));

                        yield return new Segment(SegmentType.Formatting, this.Slice(separatorIndex, end - separatorIndex));
                    }
                    else
                    {
                        end = SkipLineBreak(separatorIndex);
                        if (separatorIndex > segmentStart)
                            yield return new Segment(SegmentType.Text, this.Slice(segmentStart, separatorIndex - segmentStart));

                        // TODO: Partial line breaks.
                        yield return new Segment(SegmentType.LineBreak);
                    }

                    pos = end;
                    segmentStart = pos;
                }
            }

            public StringBuffer Slice(int offset)
            {
                return Slice(offset, Length - offset);
            }

            public StringBuffer Slice(int offset, int count)
            {
                int newOffset = _value.Offset + Math.Min(Length, offset);
                if (newOffset + count > _value.Offset + Length)
                    count = Length - newOffset;

                return new StringBuffer(_value.Array!, newOffset, count);
            }

            public void WriteTo(TextWriter writer)
            {
                writer.Write(_value.Array!, _value.Offset, _value.Count);
            }

            public (StringBuffer, StringBuffer, int)? BreakLine(int startIndex, bool force)
            {
                if (force)
                    return (Slice(0, startIndex), Slice(startIndex), startIndex);

                int count = startIndex + 1;
                startIndex += _value.Offset;

                int breakPoint = Array.FindLastIndex(_value.Array!, startIndex, count, ch => char.IsWhiteSpace(ch));
                if (breakPoint < 0)
                    return null;

                breakPoint -= _value.Offset;
                return (Slice(0, breakPoint), Slice(breakPoint + 1), breakPoint);
            }

            public override string? ToString()
            {
                return new string(_value.Array!, _value.Offset, _value.Count);
            }
        }

        private class LineBuffer
        {
            private readonly List<Segment> _segments = new();

            public int ContentLength { get; private set; }

            public bool IsEmpty { get; private set; } = true;


            public void Append(Segment segment)
            {
                Debug.Assert(segment.Type != SegmentType.LineBreak);
                _segments.Add(segment);
                ContentLength += segment.ContentLength;
                IsEmpty = false;
            }

            public void FlushTo(TextWriter writer, int indent)
            {
                if (!IsEmpty)
                {
                    WriteLineTo(writer, indent);
                }
            }

            public void WriteLineTo(TextWriter writer, int indent)
            {
                if (!IsEmpty)
                {
                    WriteSegments(writer, _segments);
                }

                writer.WriteLine();
                ClearCurrentLine(indent);
            }

            private static void WriteSegments(TextWriter writer, IEnumerable<Segment> segments)
            {
                foreach (var segment in segments)
                {
                    segment.WriteTo(writer);
                }
            }

            public bool BreakLine(TextWriter writer, int maxLength, int indent, bool force)
            {
                Debug.Assert(!IsEmpty);
                int index = maxLength;
                int segmentIndex;
                int currentLength = ContentLength;
                (StringBuffer, StringBuffer, int)? splits = null;
                for (segmentIndex = _segments.Count - 1; segmentIndex >= 0; segmentIndex--)
                {
                    var segment = _segments[segmentIndex];
                    if (segment.Type != SegmentType.Text || segment.Content == null)
                        continue;

                    currentLength -= segment.ContentLength;
                    if (index < currentLength)
                        continue;

                    splits = segment.Content.Value.BreakLine(index - currentLength, force);
                    if (splits != null)
                        break;

                    index = currentLength - 1;
                }

                if (splits == null)
                    return false;

                var (lineEnd, lineStart, breakPoint) = splits.Value;
                WriteSegments(writer, _segments.Take(segmentIndex));
                lineEnd.WriteTo(writer);
                writer.WriteLine();
                ContentLength -= (breakPoint + 1);

                _segments.RemoveRange(0, segmentIndex + 1);
                if (lineStart.Length > 0)
                {
                    _segments.Insert(0, new Segment(SegmentType.Text, lineStart));
                }
                else
                {
                    IsEmpty = _segments.Count == 0;
                }

                if (indent > 0)
                {
                    _segments.Insert(0, new Segment(SegmentType.Text, null, indent));
                    ContentLength += indent;
                }

                ContentLength = _segments.Sum(s => s.ContentLength);

                return true;
            }

            public void ClearCurrentLine(int indent)
            {
                _segments.Clear();
                if (!IsEmpty && indent > 0)
                {
                    // Line needs to be indented, so fill the indent length with white space.
                    _segments.Add(new Segment(SegmentType.Text, null, indent));
                    ContentLength = indent;
                }
                else
                {
                    ContentLength = 0;
                }

                // This line contains no content, only (possibly) indentation.
                IsEmpty = true;
            }
        }

        #endregion

        private const char IndentChar = ' ';

        private readonly TextWriter _baseWriter;
        private readonly LineBuffer? _lineBuffer;
        private readonly bool _disposeBaseWriter;
        private readonly int _maximumLineLength;
        private readonly bool _countFormatting;
        private int _indent;

        // Used for indenting when there is no maximum line length.
        private bool _indentNextWrite;
        private int _currentLineLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineWrappingTextWriter"/> class.
        /// </summary>
        /// <param name="baseWriter">The <see cref="TextWriter"/> to which to write the wrapped output.</param>
        /// <param name="maximumLineLength">The maximum length of a line, in characters; a value of less than 1 or larger than 65536 means there is no maximum line length.</param>
        /// <param name="disposeBaseWriter">If set to <see langword="true"/> the <paramref name="baseWriter"/> will be disposed when the <see cref="LineWrappingTextWriter"/> is disposed.</param>
        /// <param name="countFormatting">
        ///   If set to <see langword="false"/>, virtual terminal sequences used to format the text
        ///   will not be counted as part of the line length, and will therefore not affect where
        ///   the text is wrapped. The default value is <see langword="true"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="baseWriter"/> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   The largest <paramref name="maximumLineLength"/> value supported is 65535. Above that, line length is considered to be unbounded. This is done
        ///   to avoid having to allocate very large buffers to support these long line lengths.
        /// </para>
        /// <para>
        ///   If you want to write to the console, use <see cref="Console.Out"/> or <see cref="Console.Error"/> as the <paramref name="baseWriter"/> and
        ///   specify <see cref="Console.WindowWidth"/> - 1 as the <paramref name="maximumLineLength"/> and <see langword="false"/> for <paramref name="disposeBaseWriter"/>. If you don't
        ///   subtract one from the window width, additional empty lines can be printed if a line is exactly the width of the console. You can easily create a <see cref="LineWrappingTextWriter"/>
        ///   that writes to the console by using the <see cref="ForConsoleOut"/> and <see cref="ForConsoleError"/> methods.
        /// </para>
        /// <para>
        ///   When the console output is redirected to a file, Microsoft .Net will still report the console's actual window width, but on Mono
        ///   the value of <see cref="Console.WindowWidth"/> will be 0. In that case, the <see cref="LineWrappingTextWriter"/> will use no
        ///   line limit.
        /// </para>
        /// </remarks>
        public LineWrappingTextWriter(TextWriter baseWriter, int maximumLineLength, bool disposeBaseWriter = true, bool countFormatting = false)
            : base(baseWriter?.FormatProvider)
        {
            _baseWriter = baseWriter ?? throw new ArgumentNullException(nameof(baseWriter));
            base.NewLine = baseWriter.NewLine;
            // We interpret anything larger than 65535 to mean infinite length to avoid allocating a buffer that size.
            _maximumLineLength = (maximumLineLength < 1 || maximumLineLength > ushort.MaxValue) ? 0 : maximumLineLength;
            _disposeBaseWriter = disposeBaseWriter;
            _countFormatting = countFormatting;
            if (_maximumLineLength > 0)
                _lineBuffer = new();
        }


        /// <summary>
        /// Gets the <see cref="TextWriter"/> that this <see cref="LineWrappingTextWriter"/> is writing to.
        /// </summary>
        /// <value>
        /// The <see cref="TextWriter"/> that this <see cref="LineWrappingTextWriter"/> is writing to.
        /// </value>
        public TextWriter BaseWriter
        {
            get { return _baseWriter; }
        }

        /// <inheritdoc/>
        public override Encoding Encoding 
        {
            get { return _baseWriter.Encoding; }
        }

        /// <inheritdoc/>
#if NET6_0_OR_GREATER
        [AllowNull]
#endif
        public override string NewLine
        { 
            get => _baseWriter.NewLine;
            set
            {
                base.NewLine = value;
                _baseWriter.NewLine = value;
            }
        }

        /// <summary>
        /// Gets the maximum length of a line in the output.
        /// </summary>
        /// <value>
        /// The maximum length of a line, or zero if the line length is not limited.
        /// </value>
        public int MaximumLineLength
        {
            get { return _maximumLineLength; }
        }

        /// <summary>
        /// Gets or sets the amount of characters to indent all but the first line.
        /// </summary>
        /// <value>
        /// The amount of characters to indent all but the first line of text.
        /// </value>
        /// <remarks>
        /// <para>
        ///   Whenever a line break is encountered (either because of wrapping or because a line break was written to the
        ///   <see cref="LineWrappingTextWriter"/>, the next line is indented by the number of characters specified
        ///   by the <see cref="Indent"/> property.
        /// </para>
        /// <para>
        ///   The output position can be reset to the start of the line after a line break by calling <see cref="ResetIndent"/>.
        /// </para>
        /// </remarks>
        public int Indent
        {
            get { return _indent; }
            set 
            {
                if( value < 0 || (_maximumLineLength > 0 && value >= _maximumLineLength) )
                    throw new ArgumentOutOfRangeException(nameof(value), Properties.Resources.IndentOutOfRange);
                _indent = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="LineWrappingTextWriter"/> that writes to the standard output stream.
        /// </summary>
        /// <returns>A <see cref="LineWrappingTextWriter"/> that writes to the standard output stream.</returns>
        public static LineWrappingTextWriter ForConsoleOut()
        {
            return new LineWrappingTextWriter(Console.Out, GetLineLengthForConsole(), false);
        }

        /// <summary>
        /// Gets a <see cref="LineWrappingTextWriter"/> that writes to the standard error stream.
        /// </summary>
        /// <returns>A <see cref="LineWrappingTextWriter"/> that writes to the standard error stream.</returns>
        public static LineWrappingTextWriter ForConsoleError()
        {
            return new LineWrappingTextWriter(Console.Error, GetLineLengthForConsole(), false);
        }

        /// <summary>
        /// Gets a <see cref="LineWrappingTextWriter"/> that writes to a <see cref="StringWriter"/> using the specified format provider.
        /// </summary>
        /// <param name="maximumLineLength">The maximum length of a line, in characters.</param>
        /// <param name="formatProvider">An <see cref="IFormatProvider"/> that controls formatting.</param>
        /// <param name="countFormatting">
        ///   If set to <see langword="false"/>, virtual terminal sequences used to format the text
        ///   will not be counted as part of the line length, and will therefore not affect where
        ///   the text is wrapped. The default value is <see langword="true"/>.
        /// </param>
        /// <returns>A <see cref="LineWrappingTextWriter"/> that writes to a <see cref="StringWriter"/>.</returns>
        /// <remarks>
        ///   To retrieve the resulting string, first call <see cref="Flush"/>, then use the <see cref="StringWriter.ToString"/> method
        ///   of the <see cref="BaseWriter"/>.
        /// </remarks>
        public static LineWrappingTextWriter ForStringWriter(int maximumLineLength, IFormatProvider? formatProvider = null, bool countFormatting = false)
        {
            return new LineWrappingTextWriter(new StringWriter(formatProvider), maximumLineLength, true, countFormatting);
        }

        /// <inheritdoc/>
        public override void Write(char value)
        {
            if( _maximumLineLength > 0 )
            {
                // This is not exactly optimal but it will do.
                Write(new[] { value }, 0, 1);
            }
            else
                _baseWriter.Write(value);
        }

        /// <inheritdoc/>
        public override void Write(string? value)
        {
            if( value != null )
            {
                WriteCore(new StringBuffer(value));
            }
        }

        /// <inheritdoc/>
        public override void Write(char[] buffer, int index, int count)
        {
            if( buffer == null )
                throw new ArgumentNullException(nameof(buffer));
            if( index < 0 )
                throw new ArgumentOutOfRangeException(nameof(index), Properties.Resources.ValueMustBeNonNegative);
            if( count < 0 )
                throw new ArgumentOutOfRangeException(nameof(count), Properties.Resources.ValueMustBeNonNegative);
            if( (buffer.Length - index) < count )
                throw new ArgumentException(Properties.Resources.IndexCountOutOfRange);

            // The array must be cloned because we'll store references to segments of it, which
            // would break if the caller changes the contents.
            WriteCore(new StringBuffer((char[])buffer.Clone(), index, count));
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            _lineBuffer?.FlushTo(_baseWriter, _indent);
            base.Flush();
            _baseWriter.Flush();
        }

        /// <summary>
        /// Restarts writing on the beginning of the line, without indenting that line.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The <see cref="ResetIndent"/> method will reset the output position to the beginning of the current line.
        ///   It does not modify the <see cref="Indent"/> property, so the text will be indented again the next time
        ///   a line break is written to the output.
        /// </para>
        /// <para>
        ///   If the current line buffer is not empty, it will be flushed to the <see cref="BaseWriter"/>, followed by a new line
        ///   before the indentation is reset. If the current line buffer is empty (a line containing only indentation is considered empty),
        ///   the output position is simply reset to the beginning of the line without writing anything to the base writer.
        /// </para>
        /// </remarks>
        public void ResetIndent()
        {
            if (_lineBuffer != null)
            {
                if (!_lineBuffer.IsEmpty)
                    _lineBuffer.FlushTo(_baseWriter, 0);
                else
                    _lineBuffer.ClearCurrentLine(0);
            }
            else
            {
                if (!_indentNextWrite && _currentLineLength > 0)
                    _baseWriter.WriteLine();

                _indentNextWrite = false;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
            if( disposing && _disposeBaseWriter )
            {
                _baseWriter.Dispose();
            }
        }

        private void WriteNoMaximum(StringBuffer buffer)
        {
            Debug.Assert(_maximumLineLength == 0);

            foreach (var segment in buffer.Split(true))
            {
                if (segment.Type == SegmentType.LineBreak)
                {
                    _baseWriter.WriteLine();
                    _indentNextWrite = _currentLineLength != 0;
                    _currentLineLength = 0;
                }
                else
                {
                    WriteIndentDirectIfNeeded();
                    segment.Content!.Value.WriteTo(_baseWriter);
                    _currentLineLength += segment.Content!.Value.Length;
                }
            }
        }

        private void WriteIndentDirectIfNeeded()
        {
            // Write the indentation if necessary.
            if (_indentNextWrite)
            {
                WriteIndent(_baseWriter, _indent);
                _indentNextWrite = false;
            }
        }

        private static void WriteIndent(TextWriter writer, int indent)
        {
            for (int x = 0; x < indent; ++x)
                writer.Write(IndentChar);
        }

        private void WriteCore(StringBuffer buffer)
        {
            if (_lineBuffer == null)
            {
                WriteNoMaximum(buffer);
                return;
            }

            foreach (var segment in buffer.Split(_countFormatting))
            {
                if (segment.Type == SegmentType.LineBreak)
                {
                    _lineBuffer.WriteLineTo(_baseWriter, _indent);
                }
                else
                {
                    _lineBuffer.Append(segment);
                    while (_lineBuffer.ContentLength > _maximumLineLength)
                    {
                        if (!_lineBuffer.BreakLine(_baseWriter, _maximumLineLength, _indent, false))
                            _lineBuffer.BreakLine(_baseWriter, _maximumLineLength, _indent, true);
                    }
                }
            }
        }

        private static int GetLineLengthForConsole()
        {
            try
            {
                return Console.WindowWidth - 1;
            }
            catch( IOException )
            {
                return 0;
            }
        }
    }
}
