using System;
using System.Diagnostics;
using System.IO;
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using StringSpan = System.ReadOnlySpan<char>;
#endif

namespace Ookii.CommandLine
{
    internal partial class RingBuffer
    {
        private char[] _buffer;
        private int _bufferStart;
        private int _bufferEnd;

        public RingBuffer(int size)
        {
            _buffer = new char[size];
            _bufferStart = 0;
            _bufferEnd = 0;
        }

        public int Size => _bufferEnd >= _bufferStart ? _bufferEnd - _bufferStart : Capacity - _bufferStart + _bufferEnd;

        public int Capacity => _buffer.Length;

        public char this[int index]
        {
            get
            {
                index += _bufferStart;
                if (index >= _buffer.Length)
                {
                    index -= _buffer.Length;
                }

                if (index < _bufferStart && index >= _bufferEnd)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _buffer[index];
            }
        }

        public void CopyFrom(StringSpan span)
        {
            int size = Size;
            if (span.Length > Capacity - size)
            {
                Resize(size + span.Length);
            }

            var remaining = _buffer.Length - _bufferEnd;
            if (remaining < span.Length)
            {
                var (first, second) = span.Split(remaining);
                first.CopyTo(_buffer, _bufferEnd);
                second.CopyTo(_buffer, 0);
                _bufferEnd = second.Length;
            }
            else
            {
                span.CopyTo(_buffer, _bufferEnd);
                _bufferEnd += span.Length;
                Debug.Assert(_bufferEnd <= _buffer.Length);
            }
        }

        // This method is generated from WriteToAsync before the build.
        public partial void WriteTo(TextWriter writer, int length);

        public void Discard(int length)
        {
            var remaining = _buffer.Length - _bufferStart;
            if (remaining < length)
            {
                _bufferStart = length - remaining;
            }
            else
            {
                _bufferStart += length;
                Debug.Assert(_bufferStart <= _buffer.Length);
            }
        }

        public StringSpanTuple GetContents(int offset)
        {
            if (offset < 0 || offset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            int start = _bufferStart + offset;
            if (start >= _buffer.Length)
            {
                start -= _buffer.Length;
            }

            if (start > _bufferEnd)
            {
                return new(new StringSpan(_buffer, _bufferStart, _buffer.Length - _bufferStart), new StringSpan(_buffer, 0, _bufferEnd));
            }

            return new(new StringSpan(_buffer, start, _bufferEnd - start), default);
        }

        public void Peek(TextWriter writer, int offset, int length)
        {
            var (first, second) = GetContents(offset);
            first.Slice(0, length).WriteTo(writer);
            if (length > first.Length)
            {
                second.Slice(0, length - first.Length).WriteTo(writer);
            }
        }

        public int BreakLine(int offset, int length)
        {
            int size = Size;
            if (offset < 0 || offset > size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (offset + length > size)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            for (int i = offset + length - 1; i >= offset; i--)
            {
                if (char.IsWhiteSpace(this[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private void Resize(int capacityNeeded)
        {
            var newCapacity = 2 * _buffer.Length;

            // Check for overflow
            if (newCapacity < 0)
            {
                newCapacity = int.MaxValue;
            }

            if (capacityNeeded > newCapacity)
            {
                newCapacity = capacityNeeded;
            }

            var newBuffer = new char[newCapacity];
            int size = Size;
            if (_bufferStart > _bufferEnd)
            {
                int length = _buffer.Length - _bufferStart;
                Array.Copy(_buffer, _bufferStart, newBuffer, 0, length);
                Array.Copy(_buffer, 0, newBuffer, length, _bufferEnd);
            }
            else
            {
                Array.Copy(_buffer, _bufferStart, newBuffer, 0, _bufferEnd - _bufferStart);
            }

            _bufferStart = 0;
            _bufferEnd = size;
            _buffer = newBuffer;
        }
    }
}
