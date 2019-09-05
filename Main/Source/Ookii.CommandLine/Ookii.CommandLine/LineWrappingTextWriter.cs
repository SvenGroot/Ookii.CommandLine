// Copyright (c) Sven Groot (Ookii.org)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at http://ookiicommandline.codeplex.com. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;
using System.IO;
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

        // We want to use the same code for either Write(char[]) and Write(string), without allocating a new char[] or string for either, so we provide this helper to access one or the other
        private struct StringBuffer
        {
            private readonly string _stringValue;
            private readonly char[] _charArrayValue;

            public StringBuffer(string stringValue)
            {
                _stringValue = stringValue;
                _charArrayValue = null;
            }

            public StringBuffer(char[] charArrayValue)
            {
                _stringValue = null;
                _charArrayValue = charArrayValue;
            }

            public char this[int index]
            {
                get { return _stringValue == null ? _charArrayValue[index] : _stringValue[index]; }
            }

            public int IndexOfLineBreak(int index, int count)
            {
                if( _stringValue == null )
                {
                    int end = index + count;
                    for( int x = index; x < end; ++x )
                    {
                        if( _charArrayValue[x] == '\r' || _charArrayValue[x] == '\n' )
                            return x;
                    }
                    return -1;
                }
                else
                    return _stringValue.IndexOfAny(_lineBreakCharacters, index, count);
            }

            public void WriteLine(TextWriter writer, char[] tempStorage, int index, int count)
            {
                if( _stringValue == null )
                    writer.WriteLine(_charArrayValue, index, count);
                else
                {
                    // Use temp storage to avoid allocating a new string with substring
                    _stringValue.CopyTo(index, tempStorage, 0, count);
                    writer.WriteLine(tempStorage, 0, count);
                }
            }

            public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
            {
                if( _stringValue == null )
                    Array.Copy(_charArrayValue, sourceIndex, destination, destinationIndex, count);
                else
                    _stringValue.CopyTo(sourceIndex, destination, destinationIndex, count);
            }
        }

        #endregion

        private readonly TextWriter _baseWriter;
        private readonly bool _disposeBaseWriter;
        private readonly int _maximumLineLength;
        private readonly char[] _currentLine;
        private int _currentLineLength;
        private bool _isLineEmpty = true;
        private static readonly char[] _lineBreakCharacters = { '\r', '\n' };
        private int _indent;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineWrappingTextWriter"/> class.
        /// </summary>
        /// <param name="baseWriter">The <see cref="TextWriter"/> to which to write the wrapped output.</param>
        /// <param name="maximumLineLength">The maximum length of a line, in characters; a value of less than 1 or larger than 65536 means there is no maximum line length.</param>
        /// <param name="disposeBaseWriter">If set to <see langword="true"/> the <paramref name="baseWriter"/> will be disposed when the <see cref="LineWrappingTextWriter"/> is disposed.</param>
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
        public LineWrappingTextWriter(TextWriter baseWriter, int maximumLineLength, bool disposeBaseWriter)
            : base(baseWriter == null ? null : baseWriter.FormatProvider)
        {
            if( baseWriter == null )
                throw new ArgumentNullException("baseWriter");

            _baseWriter = baseWriter;
            base.NewLine = baseWriter.NewLine;
            // We interpret anything larger than 65535 to mean infinite length to avoid allocating a buffer that size.
            _maximumLineLength = (maximumLineLength < 1 || maximumLineLength > UInt16.MaxValue) ? 0 : maximumLineLength;
            _disposeBaseWriter = disposeBaseWriter;
            if( _maximumLineLength > 0 )
                _currentLine = new char[maximumLineLength];
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

        /// <summary>
        /// Returns the <see cref="System.Text.Encoding"/> in which the output is written.
        /// </summary>
        /// <value>
        ///   The <see cref="System.Text.Encoding"/> in which the output is written.
        /// </value>
        public override Encoding Encoding 
        {
            get { return _baseWriter.Encoding; }
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
                    throw new ArgumentOutOfRangeException("value", Properties.Resources.IndentOutOfRange);
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
        /// <returns>A <see cref="LineWrappingTextWriter"/> that writes to a <see cref="StringWriter"/>.</returns>
        /// <remarks>
        ///   To retrieve the resulting string, first call <see cref="Flush"/>, then use the <see cref="StringWriter.ToString"/> method
        ///   of the <see cref="BaseWriter"/>.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static LineWrappingTextWriter ForStringWriter(int maximumLineLength, IFormatProvider formatProvider)
        {
            return new LineWrappingTextWriter(new StringWriter(formatProvider), maximumLineLength, true);
        }

        /// <summary>
        /// Gets a <see cref="LineWrappingTextWriter"/> that writes to a <see cref="StringWriter"/>.
        /// </summary>
        /// <param name="maximumLineLength">The maximum length of a line, in characters.</param>
        /// <returns>A <see cref="LineWrappingTextWriter"/> that writes to a <see cref="StringWriter"/>.</returns>
        /// <remarks>
        ///   To retrieve the resulting string, first call <see cref="Flush"/>, then use the <see cref="StringWriter.ToString"/> method
        ///   of the <see cref="BaseWriter"/>.
        /// </remarks>
        public static LineWrappingTextWriter ForStringWriter(int maximumLineLength)
        {
            return ForStringWriter(maximumLineLength, null);
        }

        /// <summary>
        /// Writes a character to the text stream.
        /// </summary>
        /// <param name="value">The character to write to the text stream.</param>
        /// <exception cref="ObjectDisposedException">
        ///   The <see cref="LineWrappingTextWriter"/> is closed.
        /// </exception>
        /// <exception cref="IOException">
        ///   An I/O error occurs.
        /// </exception>
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

        /// <summary>
        /// Writes a string to the text stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <exception cref="ObjectDisposedException">
        ///   The <see cref="LineWrappingTextWriter"/> is closed.
        /// </exception>
        /// <exception cref="IOException">
        ///   An I/O error occurs.
        /// </exception>
        public override void Write(string value)
        {
            if( _maximumLineLength > 0 )
            {
                if( value != null )
                {
                    WriteCore(new StringBuffer(value), 0, value.Length);
                }
            }
            else
                _baseWriter.Write(value);
        }

        /// <summary>
        /// Writes a subarray of characters to the text stream.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <param name="index">Starting index in the buffer.</param>
        /// <param name="count">The number of characters to write.</param>
        /// <exception cref="ArgumentException">
        ///   The buffer length minus <paramref name="index"/> is less than <paramref name="count"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   The <paramref name="buffer"/> parameter is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="index"/> or <paramref name="count"/> is negative.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   The <see cref="LineWrappingTextWriter"/> is closed.
        /// </exception>
        /// <exception cref="IOException">
        ///   An I/O error occurs.
        /// </exception>
        public override void Write(char[] buffer, int index, int count)
        {
            if( buffer == null )
                throw new ArgumentNullException("buffer");
            if( index < 0 )
                throw new ArgumentOutOfRangeException("index", Properties.Resources.ValueMustBeNonNegative);
            if( count < 0 )
                throw new ArgumentOutOfRangeException("count", Properties.Resources.ValueMustBeNonNegative);
            if( (buffer.Length - index) < count )
                throw new ArgumentException(Properties.Resources.IndexCountOutOfRange);

            if( _maximumLineLength > 0 )
                WriteCore(new StringBuffer(buffer), index, count);
            else
                _baseWriter.Write(buffer, index, count);
        }

        /// <summary>
        /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            if( _maximumLineLength > 0 && (_currentLineLength > _indent || !_isLineEmpty) )
            {
                _baseWriter.WriteLine(_currentLine, 0, _currentLineLength);
                ClearCurrentLine(_currentLineLength);
            }
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
            if( _maximumLineLength > 0 )
            {
                if( _currentLineLength > _indent || !_isLineEmpty )
                {
                    _baseWriter.WriteLine(_currentLine, 0, _currentLineLength);
                }
                _currentLineLength = 0;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="LineWrappingTextWriter"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
            if( disposing && _disposeBaseWriter )
            {
                _baseWriter.Dispose();
            }
        }

        private void WriteCore(StringBuffer buffer, int index, int count)
        {
            // Arguments have already been checked by caller.
            int pos = index;
            int end = index + count;
            while( pos < end )
            {
                int lineEnd = buffer.IndexOfLineBreak(pos, end - pos);
                if( lineEnd < 0 )
                    lineEnd = end; // No hard line break found

                do
                {
                    int lineLength = lineEnd - pos;
                    if( _currentLineLength + lineLength > _maximumLineLength ) // If line doesn't fit, we need to find a place to wrap it
                        pos = BreakLine(buffer, pos);
                    else if( lineEnd < end ) // Line does fit, and we found a hard line break before the end of the string
                    {
                        _baseWriter.Write(_currentLine, 0, _currentLineLength);
                        buffer.WriteLine(_baseWriter, _currentLine, pos, lineLength);
                        pos = lineEnd;
                        ClearCurrentLine(_currentLineLength + lineLength);
                    }
                    else // The entire remainder of the string fits into the buffer
                    {
                        buffer.CopyTo(pos, _currentLine, _currentLineLength, lineLength);
                        _isLineEmpty = false;
                        _currentLineLength += lineLength;
                        pos = lineEnd;
                    }
                } while( pos < lineEnd );

                if( lineEnd < end )
                {
                    if( buffer[lineEnd] == '\r' && lineEnd + 1 < end && buffer[lineEnd + 1] == '\n' )
                        pos = lineEnd + 2; // Windows line ending
                    else
                        pos = lineEnd + 1;
                }
            }
        }

        private int BreakLine(StringBuffer buffer, int start)
        {
            // Because this function should only be called if the buffer's first line break + _currentLineLength is longer than _maximumLineLength
            // this function cannot overrun the length of buffer
            int index;
            for( index = _maximumLineLength - 1; index >= _indent; --index )
            {
                char ch = index < _currentLineLength ? _currentLine[index] : buffer[start + index - _currentLineLength];
                if( Char.IsWhiteSpace(ch) )
                {
                    break;
                }
            }
            if( index < _indent )
                index = _maximumLineLength; // No nice place to wrap found

            _baseWriter.Write(_currentLine, 0, Math.Min(index, _currentLineLength));
            if( index < _currentLineLength )
            {
                // We wrapped inside the current line, so we need to copy the remainder of that line to the beginning of the current line buffer
                int newLineLength = _currentLineLength - (index + 1);
                Array.Copy(_currentLine, index + 1, _currentLine, _indent, newLineLength);
                for( int x = 0; x < _indent; ++x )
                    _currentLine[x] = ' ';
                _currentLineLength = newLineLength + _indent;
                _baseWriter.WriteLine();
                // We didn't process any characters from the string, so return start.
                return start;
            }
            else
            {
                // Our wrap position was inside the string buffer. We already wrote the entire current line, now write the string buffer up until that wrap position.
                int count = index - _currentLineLength;
                buffer.WriteLine(_baseWriter, _currentLine, start, count);
                ClearCurrentLine(_currentLineLength + count);
                // If we found a white space character to wrap on, skip it.
                return index < _maximumLineLength ? start + count + 1 : start + count;
            }
        }

        private void ClearCurrentLine(int lastLineLength)
        {
            if( lastLineLength > 0 && _indent > 0 )
            {
                // Line needs to be indented, so fill the indent length with white space.
                for( int x = 0; x < _indent; ++x )
                    _currentLine[x] = ' ';
                _currentLineLength = _indent;
            }
            else
                _currentLineLength = 0; // No indent necessary, just reset to 0.
            _isLineEmpty = true; // This line contains no content, only indentation.
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
