#if !NET6_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER

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

        private void ValidateIndex(int index, string name)
        {
            if (index < 0 || index > Length)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }
}

#endif