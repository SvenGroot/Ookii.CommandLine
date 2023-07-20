// The async methods in this file are used to generate the normal, non-async versions using the
// Convert-SyncMethod.ps1 script.
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ookii.CommandLine;

internal partial class RingBuffer
{
    public async Task WriteToAsync(TextWriter writer, int length, CancellationToken cancellationToken)
    {
        if (length > Size)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        var remaining = _buffer.Length - _bufferStart;
        if (remaining < length)
        {
            await WriteAsyncHelper(writer, _buffer, _bufferStart, remaining, cancellationToken);
            remaining = length - remaining;
            await WriteAsyncHelper(writer, _buffer, 0, remaining, cancellationToken);
            _bufferStart = remaining;
        }
        else
        {
            await WriteAsyncHelper(writer, _buffer, _bufferStart, length, cancellationToken);
            _bufferStart += length;
            Debug.Assert(_bufferStart <= _buffer.Length);
        }

        if (_bufferEnd != null && _bufferStart == _bufferEnd.Value)
        {
            _bufferEnd = null;
        }
    }

    private static async Task WriteAsyncHelper(TextWriter writer, char[] buffer, int index, int length, CancellationToken cancellationToken)
    {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        await writer.WriteAsync(buffer.AsMemory(index, length), cancellationToken);
#else
        await writer.WriteAsync(buffer, index, length);
#endif
    }
}
