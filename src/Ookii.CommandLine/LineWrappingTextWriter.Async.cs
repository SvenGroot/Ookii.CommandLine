// The async methods in this file are used to generate the normal, non-async versions using the
// Convert-SyncMethod.ps1 script.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using StringMemory = System.ReadOnlyMemory<char>;
#endif

namespace Ookii.CommandLine
{
    public partial class LineWrappingTextWriter
    {

        private partial class LineBuffer
        {
            public async Task FlushToAsync(TextWriter writer, int indent, bool insertNewLine)
            {
                // Don't use IsContentEmpty because we also want to write if there's only VT sequences.
                if (_segments.Count != 0)
                {
                    await WriteToAsync(writer, indent, insertNewLine);
                }
            }

            public async Task WriteLineToAsync(TextWriter writer, int indent)
            {
                await WriteToAsync(writer, indent, true);
            }

            private async Task WriteToAsync(TextWriter writer, int indent, bool insertNewLine)
            {
                // Don't use IsContentEmpty because we also want to write if there's only VT sequences.
                if (_segments.Count != 0)
                {
                    await WriteSegmentsAsync(writer, _segments);
                }

                if (insertNewLine)
                {
                    await writer.WriteLineAsync();
                }

                ClearCurrentLine(indent);
            }

            private async Task WriteSegmentsAsync(TextWriter writer, IEnumerable<Segment> segments)
            {
                await WriteIndentAsync(writer, Indentation);
                foreach (var segment in segments)
                {
                    switch (segment.Type)
                    {
                    case StringSegmentType.PartialLineBreak:
                    case StringSegmentType.LineBreak:
                        await writer.WriteLineAsync();
                        break;

                    default:
                        await _buffer.WriteToAsync(writer, segment.Length);
                        break;
                    }
                }
            }

            public async Task<StringMemory> BreakLineAsync(TextWriter writer, StringMemory newSegment, int maxLength, int indent)
            {
                var result = await BreakLineAsync(writer, newSegment, maxLength, indent, false);
                if (!result.Success)
                {
                    // Guaranteed to succeed with force=true.
                    result = await BreakLineAsync(writer, newSegment, maxLength, indent, true);
                    Debug.Assert(result.Success);
                }

                return result.Remaining;
            }

            private async Task<AsyncBreakLineResult> BreakLineAsync(TextWriter writer, StringMemory newSegment, int maxLength, int indent, bool force)
            {
                Debug.Assert(LineLength <= maxLength || newSegment.Span.Length == 0);

                if (newSegment.BreakLine(maxLength - LineLength, force, out var splits))
                {
                    var (before, after) = splits;
                    await WriteSegmentsAsync(writer, _segments);
                    await before.WriteToAsync(writer);
                    await writer.WriteLineAsync();
                    ClearCurrentLine(indent);
                    Indentation = indent;
                    return new() { Success = true, Remaining = after };
                }

                if (IsContentEmpty)
                {
                    return new() { Success = false };
                }

                int offset = 0;
                int contentOffset = 0;
                foreach (var segment in _segments)
                {
                    offset += segment.Length;
                    contentOffset += segment.ContentLength;
                }

                for (int segmentIndex = _segments.Count - 1; segmentIndex >= 0; segmentIndex--)
                {
                    var segment = _segments[segmentIndex];
                    offset -= segment.Length;
                    contentOffset -= segment.ContentLength;
                    if (segment.Type != StringSegmentType.Text || contentOffset > maxLength)
                    {
                        continue;
                    }

                    int breakIndex = _buffer.BreakLine(offset, Math.Min(segment.Length, maxLength - contentOffset));
                    if (breakIndex >= 0)
                    {
                        await WriteSegmentsAsync(writer, _segments.Take(segmentIndex));
                        breakIndex -= offset;
                        await _buffer.WriteToAsync(writer, breakIndex);
                        _buffer.Discard(1);
                        await writer.WriteLineAsync();
                        if (breakIndex + 1 < segment.Length)
                        {
                            _segments.RemoveRange(0, segmentIndex);
                            segment.Length -= breakIndex + 1;
                            _segments[0] = segment;
                        }
                        else
                        {
                            _segments.RemoveRange(0, segmentIndex + 1);
                        }

                        ContentLength = _segments.Sum(s => s.ContentLength);
                        Indentation = indent;
                        return new() { Success = true, Remaining = newSegment };
                    }
                }

                return new() { Success = false };
            }
        }

        private async Task FlushCoreAsync(bool insertNewLine)
        {
            ThrowIfWriteInProgress();
            if (_lineBuffer != null)
            {
                await _lineBuffer.FlushToAsync(_baseWriter, insertNewLine ? _indent : 0, insertNewLine);
            }

            await _baseWriter.FlushAsync();
        }

        private async Task ResetIndentCoreAsync()
        {
            if (_lineBuffer != null)
            {
                if (!_lineBuffer.IsContentEmpty)
                {
                    await _lineBuffer.FlushToAsync(_baseWriter, 0, true);
                }
                else
                {
                    // Leave non-content segments in the buffer.
                    _lineBuffer.ClearCurrentLine(0, false);
                }
            }
            else
            {
                if (!_noWrappingState.IndentNextWrite && _noWrappingState.CurrentLineLength > 0)
                {
                    await _baseWriter.WriteLineAsync();
                }

                _noWrappingState.IndentNextWrite = false;
            }
        }

        private async Task WriteNoMaximumAsync(StringMemory buffer)
        {
            Debug.Assert(!EnableWrapping);

            await buffer.SplitAsync(true, async (type, span) =>
            {
                switch (type)
                {
                case StringSegmentType.PartialLineBreak:
                    // If we already had a partial line break, write it now.
                    if (_noWrappingState.HasPartialLineBreak)
                    {
                        await WriteLineBreakDirectAsync();
                    }
                    else
                    {
                        _noWrappingState.HasPartialLineBreak = true;
                    }

                    break;

                case StringSegmentType.LineBreak:
                    // Write an extra line break if there was a partial one and this one isn't the
                    // end of that line break.
                    if (_noWrappingState.HasPartialLineBreak)
                    {
                        _noWrappingState.HasPartialLineBreak = false;
                        if (span.Span.Length != 1 || span.Span[0] != '\n')
                        {
                            await WriteLineBreakDirectAsync();
                        }
                    }

                    await WriteLineBreakDirectAsync();
                    break;

                default:
                    // If we had a partial line break, write it now.
                    if (_noWrappingState.HasPartialLineBreak)
                    {
                        await WriteLineBreakDirectAsync();
                        _noWrappingState.HasPartialLineBreak = false;
                    }

                    await WriteIndentDirectIfNeededAsync();
                    await span.WriteToAsync(_baseWriter);
                    _noWrappingState.CurrentLineLength += span.Span.Length;
                    break;
                }
            });
        }

        private async Task WriteLineBreakDirectAsync()
        {
            await _baseWriter.WriteLineAsync();
            _noWrappingState.IndentNextWrite = _noWrappingState.CurrentLineLength != 0;
            _noWrappingState.CurrentLineLength = 0;
        }

        private async Task WriteIndentDirectIfNeededAsync()
        {
            // Write the indentation if necessary.
            if (_noWrappingState.IndentNextWrite)
            {
                await WriteIndentAsync(_baseWriter, _indent);
                _noWrappingState.IndentNextWrite = false;
            }
        }

        private static async Task WriteIndentAsync(TextWriter writer, int indent)
        {
            for (int x = 0; x < indent; ++x)
            {
                await writer.WriteAsync(IndentChar);
            }
        }

        private async Task WriteCoreAsync(StringMemory buffer)
        {
            ThrowIfWriteInProgress();
            if (!EnableWrapping)
            {
                await WriteNoMaximumAsync(buffer);
                return;
            }

            await buffer.SplitAsync(_countFormatting, async (type, span) =>
            {
                // _lineBuffer is guaranteed not null by EnableWrapping but the attribute for that
                // only exists in .Net 6.0.
                bool hadPartialLineBreak = _lineBuffer!.CheckAndRemovePartialLineBreak();
                if (hadPartialLineBreak)
                {
                    await _lineBuffer.WriteLineToAsync(_baseWriter, _indent);
                }

                if (type == StringSegmentType.LineBreak)
                {
                    // Check if this is just the end of a partial line break. If it is, it was
                    // already written above.
                    if (!hadPartialLineBreak || span.Span.Length > 1 || (span.Span.Length == 1 && span.Span[0] != '\n'))
                    {
                        await _lineBuffer.WriteLineToAsync(_baseWriter, _indent);
                    }
                }
                else
                {
                    var remaining = span;
                    if (type == StringSegmentType.Text && !_lineBuffer.HasPartialFormatting)
                    {
                        while (_lineBuffer.LineLength + remaining.Span.Length > _maximumLineLength)
                        {
                            remaining = await _lineBuffer.BreakLineAsync(_baseWriter, remaining, _maximumLineLength, _indent);
                        }
                    }

                    if (remaining.Span.Length > 0)
                    {
                        _lineBuffer.Append(remaining.Span, type);
                        if (_lineBuffer.LineLength > _maximumLineLength)
                        {
                            // This can happen if we couldn't break above because partial formatting
                            // had to be resolved.
                            await _lineBuffer.BreakLineAsync(_baseWriter, default, _maximumLineLength, _indent);
                        }
                    }
                }
            });
        }
    }
}
