// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using StringSpan = System.ReadOnlySpan<char>;
using StringMemory = System.ReadOnlyMemory<char>;
#endif

namespace Ookii.CommandLine
{
    /// <summary>
    /// Implements a <see cref="TextWriter"/> that writes text to another <see cref="TextWriter"/>,
    /// white-space wrapping lines at the specified maximum line length, and supporting indentation.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   If the <see cref="MaximumLineLength"/> property is not zero, the <see cref="LineWrappingTextWriter"/>
    ///   will buffer the data written to it until an explicit new line is present in the text, or
    ///   until the length of the buffered data exceeds the value of the <see cref="MaximumLineLength"/>
    ///   property.
    /// </para>
    /// <para>
    ///   If the length of the buffered data exceeds the value of the <see cref="MaximumLineLength"/>
    ///   property, the <see cref="LineWrappingTextWriter"/> will attempt to find a white-space
    ///   character to break the line at. If such a white-space character is found, everything
    ///   before that character is output to the <see cref="BaseWriter"/>, followed by a line ending,
    ///   and everything after that character is kept in the buffer. The white-space character
    ///   itself is not written to the output.
    /// </para>
    /// <para>
    ///   If no suitable place to break the line could be found, the line is broken at the maximum
    ///   line length. This may occur in the middle of a word.
    /// </para>
    /// <para>
    ///   After a line break (either one that was caused by wrapping or one that was part of the
    ///   text), the next line is indented by the number of characters specified by the <see cref="Indent"/>
    ///   property. The length of the indentation counts towards the maximum line length.
    /// </para>
    /// <para>
    ///   When the <see cref="Flush()"/> or <see cref="FlushAsync()"/> method is called, the current
    ///   contents of the buffer are written to the <see cref="BaseWriter"/>, followed by a new
    ///   line, unless the buffer is empty. If the buffer contains only indentation, it is
    ///   considered empty and no new line is written. Calling <see cref="Flush()"/> has the same
    ///   effect as writing a new line to the <see cref="LineWrappingTextWriter"/> if the buffer is
    ///   not empty. The <see cref="LineWrappingTextWriter"/> is flushed when the <see cref="Dispose"/>
    ///   or <see cref="DisposeAsync"/> method is called.
    /// </para>
    /// <para>
    ///   The <see cref="ResetIndent"/> or <see cref="ResetIndentAsync"/> method can be used to move
    ///   the output position back to the beginning of the line. If the buffer is not empty, is
    ///   first flushed and indentation is reset to zero on the next line. After the next line
    ///   break, indentation will again be set to the value of the <see cref="Indent"/> property.
    /// </para>
    /// <para>
    ///   If there is no maximum line length, output is written directly to the <see cref="BaseWriter"/>
    ///   and buffering does not occur. Indentation is still inserted as appropriate.
    /// </para>
    /// <para>
    ///   The <see cref="Flush()"/>, <see cref="FlushAsync()"/>, <see cref="Dispose"/> and
    ///   <see cref="DisposeAsync"/> methods will not write an additional new line if the
    ///   <see cref="MaximumLineLength"/> property is zero.
    /// </para>
    /// </remarks>
    public partial class LineWrappingTextWriter : TextWriter
    {
        #region Nested types

        [DebuggerDisplay("Type = {Type}, ContentLength = {ContentLength}, Length = {Length}")]
        private struct Segment
        {
            public Segment(StringSegmentType type, int length)
            {
                Type = type;
                Length = length;
            }

            public StringSegmentType Type { get; set; }
            public int Length { get; set; }

            public int ContentLength => IsContent(Type) ? Length : 0;

            public static bool IsContent(StringSegmentType type)
                => type is not (StringSegmentType.Formatting or StringSegmentType.PartialFormatting or StringSegmentType.PartialLineBreak);

        }

        private partial class LineBuffer
        {
            private struct AsyncBreakLineResult
            {
                public bool Success { get; set; }
                public StringMemory Remaining { get; set; }
            }

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            private ref struct BreakLineResult
#else
        private struct BreakLineResult
#endif
            {
                public bool Success { get; set; }
                public StringSpan Remaining { get; set; }
            }

            private readonly RingBuffer _buffer;
            private readonly List<Segment> _segments = new();

            public LineBuffer(int capacity)
            {
                _buffer = new(capacity);
            }

            public int ContentLength { get; private set; }

            public bool IsContentEmpty => ContentLength == 0;

            public bool IsEmpty => _segments.Count == 0;

            public int Indentation { get; set; }

            public int LineLength => ContentLength + Indentation;

            public void Append(StringSpan span, StringSegmentType type)
            {
                Debug.Assert(type != StringSegmentType.LineBreak);

                // If we got here, we know the line length is not overflowing, so copy everything
                // except partial linebreaks into the buffer.
                if (type != StringSegmentType.PartialLineBreak)
                {
                    _buffer.CopyFrom(span);
                }

                if (LastSegment is Segment last)
                {
                    if (last.Type == type)
                    {
                        last.Length += span.Length;
                        _segments[_segments.Count - 1] = last;
                        if (Segment.IsContent(type))
                        {
                            ContentLength += span.Length;
                        }

                        return;
                    }
                    else if (last.Type == StringSegmentType.PartialFormatting)
                    {
                        if (type == StringSegmentType.Text)
                        {
                            FindPartialFormattingEnd(last, span.Length);
                            return;
                        }
                        else
                        {
                            // If this is not a text segment, we never found the end of the formatting,
                            // so just treat everything up to now as formatting.
                            last.Type = StringSegmentType.Formatting;
                            _segments[_segments.Count - 1] = last;
                        }
                    }
                }

                var segment = new Segment(type, span.Length);
                _segments.Add(segment);
                var contentLength = segment.ContentLength;
                ContentLength += contentLength;
            }

            public Segment? LastSegment => _segments.Count > 0 ? _segments[_segments.Count - 1] : null;

            public bool HasPartialFormatting => LastSegment is Segment last && last.Type == StringSegmentType.PartialFormatting;

            public partial void FlushTo(TextWriter writer, int indent, bool insertNewLine);

            public partial void WriteLineTo(TextWriter writer, int indent);

            public void Peek(TextWriter writer)
            {
                WriteIndent(writer, Indentation);
                int offset = 0;
                foreach (var segment in _segments)
                {
                    switch (segment.Type)
                    {
                    case StringSegmentType.PartialLineBreak:
                    case StringSegmentType.LineBreak:
                        writer.WriteLine();
                        break;

                    default:
                        _buffer.Peek(writer, offset, segment.Length);
                        offset += segment.Length;
                        break;
                    }
                }
            }

            public bool CheckAndRemovePartialLineBreak()
            {
                if (LastSegment is Segment last && last.Type == StringSegmentType.PartialLineBreak)
                {
                    _segments.RemoveAt(_segments.Count - 1);
                    return true;
                }

                return false;
            }

            private partial void WriteTo(TextWriter writer, int indent, bool insertNewLine);

            private void FindPartialFormattingEnd(Segment lastSegment, int newLength)
            {
                var offset = _segments.Take(_segments.Count - 1).Sum(s => s.Length);
                var (first, second) = _buffer.GetContents(offset);
                int index = VirtualTerminal.FindSequenceEnd(first.Slice(1), second);
                if (index < 0)
                {
                    // No ending found, concatenate this to the last segment.
                    lastSegment.Length += newLength;
                    _segments[_segments.Count - 1] = lastSegment;
                }
                else
                {
                    // Concatenate the rest of the formatting.
                    var remaining = newLength - (index - lastSegment.Length);
                    lastSegment.Length = index;
                    lastSegment.Type = StringSegmentType.Formatting;
                    _segments[_segments.Count - 1] = lastSegment;
                    if (remaining > 0)
                    {
                        _segments.Add(new Segment(StringSegmentType.Text, remaining));
                        ContentLength += remaining;
                    }
                }
            }

            private partial void WriteSegments(TextWriter writer, IEnumerable<Segment> segments);

            public partial StringSpan BreakLine(TextWriter writer, StringSpan newSegment, int maxLength, int indent);

            private partial BreakLineResult BreakLine(TextWriter writer, StringSpan newSegment, int maxLength, int indent, bool force);

            public void ClearCurrentLine(int indent, bool clearSegments = true)
            {
                if (clearSegments)
                {
                    _segments.Clear();
                }

                if (!IsContentEmpty)
                {
                    Indentation = indent;
                }
                else
                {
                    Indentation = 0;
                }

                ContentLength = 0;
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
        private bool _hasPartialLineBreak;
        private Task _asyncWriteTask = Task.CompletedTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineWrappingTextWriter"/> class.
        /// </summary>
        /// <param name="baseWriter">The <see cref="TextWriter"/> to which to write the wrapped output.</param>
        /// <param name="maximumLineLength">The maximum length of a line, in characters; a value of less than 1 or larger than 65536 means there is no maximum line length.</param>
        /// <param name="disposeBaseWriter">If set to <see langword="true"/> the <paramref name="baseWriter"/> will be disposed when the <see cref="LineWrappingTextWriter"/> is disposed.</param>
        /// <param name="countFormatting">
        ///   If set to <see langword="false"/>, virtual terminal sequences used to format the text
        ///   will not be counted as part of the line length, and will therefore not affect where
        ///   the text is wrapped. The default value is <see langword="false"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="baseWriter"/> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   The largest <paramref name="maximumLineLength"/> value supported is 65535. Above that, line length is considered to be unbounded. This is done
        ///   to avoid having to buffer large amounts of data to support these long line lengths.
        /// </para>
        /// <para>
        ///   If you want to write to the console, use <see cref="Console.Out"/> or <see cref="Console.Error"/> as the <paramref name="baseWriter"/> and
        ///   specify <see cref="Console.WindowWidth"/> - 1 as the <paramref name="maximumLineLength"/> and <see langword="false"/> for <paramref name="disposeBaseWriter"/>. If you don't
        ///   subtract one from the window width, additional empty lines can be printed if a line is exactly the width of the console. You can easily create a <see cref="LineWrappingTextWriter"/>
        ///   that writes to the console by using the <see cref="ForConsoleOut"/> and <see cref="ForConsoleError"/> methods.
        /// </para>
        /// </remarks>
        public LineWrappingTextWriter(TextWriter baseWriter, int maximumLineLength, bool disposeBaseWriter = true, bool countFormatting = false)
            : base(baseWriter?.FormatProvider)
        {
            _baseWriter = baseWriter ?? throw new ArgumentNullException(nameof(baseWriter));
            base.NewLine = baseWriter.NewLine;
            // We interpret anything larger than 65535 to mean infinite length to avoid buffering that much.
            _maximumLineLength = (maximumLineLength is < 1 or > ushort.MaxValue) ? 0 : maximumLineLength;
            _disposeBaseWriter = disposeBaseWriter;
            _countFormatting = countFormatting;
            if (_maximumLineLength > 0)
            {
                // Add some slack for formatting characters.
                _lineBuffer = new(countFormatting ? _maximumLineLength : _maximumLineLength * 2);
            }
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
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
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
                if (value < 0 || (_maximumLineLength > 0 && value >= _maximumLineLength))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Properties.Resources.IndentOutOfRange);
                }

                _indent = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="LineWrappingTextWriter"/> that writes to the standard output stream,
        /// using <see cref="Console.WindowWidth"/> as the maximum line length.
        /// </summary>
        /// <returns>A <see cref="LineWrappingTextWriter"/> that writes to the standard output stream.</returns>
        public static LineWrappingTextWriter ForConsoleOut()
        {
            return new LineWrappingTextWriter(Console.Out, GetLineLengthForConsole(), false);
        }

        /// <summary>
        /// Gets a <see cref="LineWrappingTextWriter"/> that writes to the standard error stream,
        /// using <see cref="Console.WindowWidth"/> as the maximum line length.
        /// </summary>
        /// <returns>A <see cref="LineWrappingTextWriter"/> that writes to the standard error stream.</returns>
        public static LineWrappingTextWriter ForConsoleError()
        {
            return new LineWrappingTextWriter(Console.Error, GetLineLengthForConsole(), false);
        }

        /// <summary>
        /// Gets a <see cref="LineWrappingTextWriter"/> that writes to a <see cref="StringWriter"/>.
        /// </summary>
        /// <param name="maximumLineLength">
        /// The maximum length of a line, in characters, or 0 to use no maximum.
        /// </param>
        /// <param name="formatProvider">An <see cref="IFormatProvider"/> that controls formatting.</param>
        /// <param name="countFormatting">
        ///   If set to <see langword="false"/>, virtual terminal sequences used to format the text
        ///   will not be counted as part of the line length, and will therefore not affect where
        ///   the text is wrapped. The default value is <see langword="false"/>.
        /// </param>
        /// <returns>A <see cref="LineWrappingTextWriter"/> that writes to a <see cref="StringWriter"/>.</returns>
        /// <remarks>
        ///   To retrieve the resulting string, first call <see cref="Flush()"/>, then use the <see
        ///   cref="StringWriter.ToString"/> method of the <see cref="BaseWriter"/>.
        /// </remarks>
        public static LineWrappingTextWriter ForStringWriter(int maximumLineLength = 0, IFormatProvider? formatProvider = null, bool countFormatting = false)
        {
            return new LineWrappingTextWriter(new StringWriter(formatProvider), maximumLineLength, true, countFormatting);
        }

        /// <inheritdoc/>
        public override void Write(char value)
        {
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            unsafe
            {
                WriteCore(new StringSpan(&value, 1));
            }
#else
            WriteCore(new StringSpan(value));
#endif
        }

        /// <inheritdoc/>
        public override void Write(string? value)
        {
            if (value != null)
            {
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                WriteCore(value);
#else
                WriteCore(new StringSpan(value));
#endif
            }
        }

        /// <inheritdoc/>
        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), Properties.Resources.ValueMustBeNonNegative);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), Properties.Resources.ValueMustBeNonNegative);
            }

            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException(Properties.Resources.IndexCountOutOfRange);
            }

            WriteCore(new StringSpan(buffer, index, count));
        }

        /// <inheritdoc/>
        public override Task WriteAsync(char value)
        {
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            // Array creation is unavoidable here because ReadOnlyMemory can't use a pointer.
            var task = WriteCoreAsync(new[] { value });
#else
            var task = WriteCoreAsync(new StringMemory(value));
#endif
            _asyncWriteTask = task;
            return task;
        }

        /// <inheritdoc/>
        public override Task WriteAsync(string? value)
        {
            if (value == null)
            {
                return Task.CompletedTask;
            }

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            // Array creation is unavoidable here because ReadOnlyMemory can't use a pointer.
            var task = WriteCoreAsync(value.AsMemory());
#else
            var task = WriteCoreAsync(new StringMemory(value));
#endif
            _asyncWriteTask = task;
            return task;
        }

        /// <inheritdoc/>
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), Properties.Resources.ValueMustBeNonNegative);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), Properties.Resources.ValueMustBeNonNegative);
            }

            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException(Properties.Resources.IndexCountOutOfRange);
            }

            var task = WriteCoreAsync(new StringMemory(buffer, index, count));
            _asyncWriteTask = task;
            return task;
        }

        /// <inheritdoc/>
        public override async Task WriteLineAsync() => await WriteAsync(CoreNewLine);

        /// <inheritdoc/>
        public override async Task WriteLineAsync(char value)
        {
            await WriteAsync(value);
            await WriteLineAsync();
        }

        /// <inheritdoc/>
        public override async Task WriteLineAsync(char[] buffer, int index, int count)
        {
            await WriteAsync(buffer, index, count);
            await WriteLineAsync();
        }

        /// <inheritdoc/>
        public override async Task WriteLineAsync(string? value)
        {
            if (value != null)
            {
                await WriteAsync(value);
            }

            await WriteLineAsync();
        }

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<char> buffer) => WriteCore(buffer);

        /// <inheritdoc/>
        public override void WriteLine(ReadOnlySpan<char> buffer)
        {
            Write(buffer);
            WriteLine();
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            await FlushAsync();
            await base.DisposeAsync();
            if (_disposeBaseWriter)
            {
                await _baseWriter.DisposeAsync();
            }
        }

        /// <inheritdoc/>
        public override Task WriteAsync(StringMemory buffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            // TODO: Use cancellation token if possible.
            _asyncWriteTask = WriteCoreAsync(buffer);
            return _asyncWriteTask;
        }

        /// <inheritdoc/>
        public override async Task WriteLineAsync(StringMemory buffer, CancellationToken cancellationToken = default)
        {
            await WriteAsync(buffer, cancellationToken);
            await WriteAsync(CoreNewLine.AsMemory(), cancellationToken);
        }

#endif

        /// <inheritdoc/>
        public override void Flush() => Flush(true);

        /// <inheritdoc/>
        public override Task FlushAsync() => FlushAsync(true);

        /// <summary>
        /// Clears all buffers for this <see cref="TextWriter"/> and causes any buffered data to be
        /// written to the underlying writer, optionally inserting an additional new line.
        /// </summary>
        /// <param name="insertNewLine">
        /// Insert an additional new line if the line buffer is not empty. This has no effect if
        /// the line buffer is empty or the <see cref="MaximumLineLength"/> property is zero.
        /// </param>
        /// <remarks>
        /// <para>
        ///   If <paramref name="insertNewLine"/> is set to <see langword="false"/>, the
        ///   <see cref="LineWrappingTextWriter"/> class will not know the length of the flushed
        ///   line, and therefore the current line may not be correctly wrapped if more text is
        ///   written to the <see cref="LineWrappingTextWriter"/>.
        /// </para>
        /// <para>
        ///   For this reason, it's recommended to only set <paramref name="insertNewLine"/> to
        ///   <see langword="false"/> if you are done writing to this instance.
        /// </para>
        /// <para>
        ///   The <see cref="Flush()"/> method is equivalent to calling this method with
        ///   <paramref name="insertNewLine"/> set to <see langword="true"/>.
        /// </para>
        /// </remarks>
        public void Flush(bool insertNewLine) => FlushCore(insertNewLine);

        /// <summary>
        /// Clears all buffers for this <see cref="TextWriter"/> and causes any buffered data to be
        /// written to the underlying writer, optionally inserting an additional new line.
        /// </summary>
        /// <param name="insertNewLine">
        /// Insert an additional new line if the line buffer is not empty. This has no effect if
        /// the line buffer is empty or the <see cref="MaximumLineLength"/> property is zero.
        /// </param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        /// <remarks>
        /// <para>
        ///   If <paramref name="insertNewLine"/> is set to <see langword="false"/>, the
        ///   <see cref="LineWrappingTextWriter"/> class will not know the length of the flushed
        ///   line, and therefore the current line may not be correctly wrapped if more text is
        ///   written to the <see cref="LineWrappingTextWriter"/>.
        /// </para>
        /// <para>
        ///   For this reason, it's recommended to only set <paramref name="insertNewLine"/> to
        ///   <see langword="false"/> if you are done writing to this instance.
        /// </para>
        /// <para>
        ///   The <see cref="FlushAsync()"/> method is equivalent to calling this method with
        ///   <paramref name="insertNewLine"/> set to <see langword="true"/>.
        /// </para>
        /// </remarks>
        public Task FlushAsync(bool insertNewLine)
        {
            var task = FlushCoreAsync(insertNewLine);
            _asyncWriteTask = task;
            return task;
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
        public void ResetIndent() => ResetIndentCore();

        /// <summary>
        /// Restarts writing on the beginning of the line, without indenting that line.
        /// </summary>
        /// <returns>
        ///   A task that represents the asynchronous reset operation.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   The <see cref="ResetIndentAsync"/> method will reset the output position to the beginning of the current line.
        ///   It does not modify the <see cref="Indent"/> property, so the text will be indented again the next time
        ///   a line break is written to the output.
        /// </para>
        /// <para>
        ///   If the current line buffer is not empty, it will be flushed to the <see cref="BaseWriter"/>, followed by a new line
        ///   before the indentation is reset. If the current line buffer is empty (a line containing only indentation is considered empty),
        ///   the output position is simply reset to the beginning of the line without writing anything to the base writer.
        /// </para>
        /// </remarks>
        public Task ResetIndentAsync()
        {
            var task = ResetIndentCoreAsync();
            _asyncWriteTask = task;
            return task;
        }

        /// <summary>
        /// Returns a string representation of the current <see cref="LineWrappingTextWriter"/>
        /// instance.
        /// </summary>
        /// <returns>
        /// If the <see cref="BaseWriter"/> property is an instance of the <see cref="StringWriter"/>
        /// class, the text written to this <see cref="LineWrappingTextWriter"/> so far; otherwise,
        /// the type name.</returns>
        /// <remarks>
        /// <para>
        ///   If the <see cref="BaseWriter"/> property is an instance of the <see cref="StringWriter"/>
        ///   class, this method will return all text written to this <see cref="LineWrappingTextWriter"/>
        ///   instance, including text that hasn't been flushed to the underlying <see cref="StringWriter"/>
        ///   yet. It does this without flushing the buffer.
        /// </para>
        /// </remarks>
        public override string? ToString()
        {
            if (_baseWriter is not StringWriter)
            {
                return base.ToString();
            }

            if (_lineBuffer?.IsEmpty ?? true)
            {
                return _baseWriter.ToString();
            }

            using var tempWriter = new StringWriter(FormatProvider) { NewLine = NewLine };
            tempWriter.Write(_baseWriter.ToString());
            _lineBuffer.Peek(tempWriter);
            return tempWriter.ToString();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
            if (disposing && _disposeBaseWriter)
            {
                _baseWriter.Dispose();
            }
        }

        private partial void WriteNoMaximum(StringSpan buffer);

        private partial void WriteLineBreakDirect();

        private partial void WriteIndentDirectIfNeeded();

        private static partial void WriteIndent(TextWriter writer, int indent);

        private partial void WriteCore(StringSpan buffer);

        private partial void FlushCore(bool insertNewLine);

        private partial void ResetIndentCore();

        private static int GetLineLengthForConsole()
        {
            try
            {
                return Console.WindowWidth - 1;
            }
            catch (IOException)
            {
                return 0;
            }
        }

        private void ThrowIfWriteInProgress()
        {
            if (!_asyncWriteTask.IsCompleted)
            {
                throw new InvalidOperationException(Properties.Resources.AsyncWriteInProgress);
            }
        }
    }
}
