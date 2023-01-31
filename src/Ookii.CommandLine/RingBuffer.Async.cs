using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    internal partial class RingBuffer
    {
        public async Task WriteToAsync(TextWriter writer, int length)
        {
            if (length > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            var remaining = _buffer.Length - _bufferStart;
            if (remaining < length)
            {
                await writer.WriteAsync(_buffer, _bufferStart, remaining);
                remaining = length - remaining;
                await writer.WriteAsync(_buffer, 0, remaining);
                _bufferStart = remaining;
            }
            else
            {
                await writer.WriteAsync(_buffer, _bufferStart, length);
                _bufferStart += length;
                Debug.Assert(_bufferStart <= _buffer.Length);
            }
        }
    }
}
