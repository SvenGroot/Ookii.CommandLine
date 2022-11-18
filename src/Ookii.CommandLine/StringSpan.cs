using Ookii.CommandLine.Terminal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    // This is a poor man's ReadOnlySpan<char> for .Net Standard 2.0.
    internal struct StringSpan : IEnumerable<char>
    {
        private static readonly char[] _segmentSeparators = { '\r', '\n', VirtualTerminal.Escape };
        private static readonly char[] _newLineSeparators = { '\r', '\n' };

        private readonly string? _stringValue;
        private readonly char[]? _charArrayValue;
        private readonly char _charValue;
        private readonly int _offset;
        private readonly int _length;

        public StringSpan(string value)
        {
            _stringValue = value;
            _charArrayValue = null;
            _charValue = default;
            _offset = 0;
            _length = value.Length;
        }

        public StringSpan(char[] value, int offset, int length)
        {
            _stringValue = null;
            _charArrayValue = value;
            _charValue = default;
            _offset = offset;
            _length = length;
        }

        public StringSpan(char value)
        {
            _stringValue = null;
            _charArrayValue = null;
            _charValue = value;
            _offset = 0;
            _length = 1;
        }

        private StringSpan(string? stringValue, char[]? charArrayValue, char charValue, int offset, int length)
        {
            _stringValue = stringValue;
            _charArrayValue = charArrayValue;
            _charValue = charValue;
            _offset = offset;
            _length = length;
        }

        public int Length => _length;

        public char this[int index]
        {
            get
            {
                ValidateIndex(index, nameof(index));
                if (_stringValue != null)
                {
                    return _stringValue[_offset + index];
                }
                else if (_charArrayValue != null)
                {
                    return _charArrayValue[_offset + index];
                }

                return _charValue;
            }
        }

        public StringSpan Slice(int start)
        {
            return Slice(start, Length - start);
        }

        public StringSpan Slice(int start, int length)
        {
            ValidateIndex(start, nameof(start));
            if (length > Length - start)
            {
                length = Length - start;
            }

            int newOffset = _offset + Math.Min(Length, start);
            return new StringSpan(_stringValue, _charArrayValue, _charValue, newOffset, length);
        }

        public int IndexOfAny(char[] separators)
        {
            int index;
            if (_stringValue != null)
            {
                index = _stringValue.IndexOfAny(separators, _offset, _length);
            }
            else if (_charArrayValue != null)
            {
                index = Array.FindIndex(_charArrayValue, _offset, _length, ch => separators.Contains(ch));
            }
            else
            {
                index = separators.Contains(_charValue) ? 0 : -1;
            }

            if (index >= 0)
            {
                index -= _offset;
            }

            return index;
        }

        public override string ToString()
        {
            if (_stringValue != null)
            {
                return _stringValue.Substring(_offset, _length);
            }
            else if (_charArrayValue != null)
            {
                return new string(_charArrayValue, _offset, _length);
            }
            else if (_length > 0)
            {
                return _charValue.ToString();
            }

            return string.Empty;
        }

        public void CopyTo(char[] destination, int start)
        {
            if (_stringValue != null)
            {
                _stringValue.CopyTo(_offset, destination, start, _length);
            }
            else if (_charArrayValue != null)
            {
                Array.Copy(_charArrayValue, _offset, destination, start, _length);
            }
            else if (_length > 0)
            {
                destination[start] = _charValue;
            }
        }

        public IEnumerable<(StringSegmentType, StringSpan)> Split(bool newLinesOnly)
        {
            var separators = newLinesOnly ? _newLineSeparators : _segmentSeparators;
            var remaining = this;
            while (remaining.Length > 0)
            {
                var separatorIndex = remaining.IndexOfAny(separators);
                if (separatorIndex < 0)
                {
                    yield return (StringSegmentType.Text, remaining);
                    break;
                }

                if (separatorIndex > 0)
                {
                    yield return (StringSegmentType.Text, remaining.Slice(0, separatorIndex));
                    remaining = remaining.Slice(separatorIndex);
                }

                if (remaining[0] == VirtualTerminal.Escape)
                {
                    // This is a VT sequence.
                    // Find the end of the sequence.
                    var end = VirtualTerminal.FindSequenceEnd(remaining.Slice(1));
                    if (end == -1)
                    {
                        // No end? Should come in a following write.
                        yield return (StringSegmentType.PartialFormatting, remaining);
                        break;
                    }

                    //end++;
                    yield return (StringSegmentType.Formatting, remaining.Slice(0, end));
                    remaining = remaining.Slice(end);
                }
                else
                {
                    StringSpan lineBreak;
                    (lineBreak, remaining) = remaining.SkipLineBreak();

                    if (remaining.Length == 0 && lineBreak.Length == 1 && lineBreak[0] == '\r')
                    {
                        // This could be the start of a Windows-style break, the remainder of
                        // which could follow in the next span.
                        yield return (StringSegmentType.PartialLineBreak, lineBreak);
                        break;
                    }

                    yield return (StringSegmentType.LineBreak, lineBreak);
                }
            }
        }

        public (StringSpan, StringSpan) SkipLineBreak()
        {
            Debug.Assert(this[0] is '\r' or '\n');
            var split = this[0] == '\r' && Length > 1 && this[1] == '\n'
                ? 2
                : 1;

            return Split(split);
        }

        public (StringSpan, StringSpan) Split(int index) => (Slice(0, index), Slice(index));

        public (StringSpan, StringSpan)? BreakLine(int startIndex, bool force)
        {
            if (force)
            {
                return (Slice(0, startIndex), Slice(startIndex));
            }

            for (int index = startIndex; index >= 0; --index)
            {
                if (char.IsWhiteSpace(this[index]))
                {
                    return (Slice(0, index), Slice(index + 1));
                }
            }

            return null;
        }

        public void WriteTo(TextWriter writer)
        {
            if (_stringValue != null)
            {
                writer.Write(_stringValue.Substring(_offset, _length));
            }
            else if (_charArrayValue != null)
            {
                writer.Write(_charArrayValue, _offset, _length);
            }
            else if (_length > 0)
            {
                writer.Write(_charValue);
            }
        }

        private void ValidateIndex(int index, string name)
        {
            if (index < 0 || index > Length)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        public IEnumerator<char> GetEnumerator()
        {
            if (_length == 0)
            {
                return Enumerable.Empty<char>().GetEnumerator();
            }

            IEnumerable<char> values;
            if (_stringValue != null)
            {
                values = _stringValue;
            }
            else if (_charArrayValue != null)
            {
                values = _charArrayValue;
            }
            else
            {
                return Enumerable.Repeat(_charValue, 1).GetEnumerator();
            }

            return values.Skip(_offset).Take(_length).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
