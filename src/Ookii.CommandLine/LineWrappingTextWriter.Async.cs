// The async methods in this file are used to generate the normal, non-async versions using the
// Convert-SyncMethod.ps1 script.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ookii.CommandLine;

public partial class LineWrappingTextWriter
{
    private partial class LineBuffer
    {
        public async Task FlushToAsync(TextWriter writer, int indent, bool insertNewLine, CancellationToken cancellationToken)
        {
            // Don't use IsContentEmpty because we also want to write if there's only VT sequences.
            if (_segments.Count != 0)
            {
                await WriteToAsync(writer, indent, insertNewLine, cancellationToken);
            }
        }

        public async Task WriteLineToAsync(TextWriter writer, int indent, CancellationToken cancellationToken)
        {
            await WriteToAsync(writer, indent, true, cancellationToken);
        }

        private async Task WriteToAsync(TextWriter writer, int indent, bool insertNewLine, CancellationToken cancellationToken)
        {
            // Don't use IsContentEmpty because we also want to write if there's only VT sequences.
            if (_segments.Count != 0)
            {
                await WriteSegmentsAsync(writer, _segments, cancellationToken);
            }

            if (insertNewLine)
            {
                await WriteBlankLineAsync(writer, cancellationToken);
            }

            ClearCurrentLine(indent);
        }

        private async Task WriteSegmentsAsync(TextWriter writer, IEnumerable<Segment> segments, CancellationToken cancellationToken)
        {
            await WriteIndentAsync(writer, Indentation);
            foreach (var segment in segments)
            {
                switch (segment.Type)
                {
                case StringSegmentType.PartialLineBreak:
                case StringSegmentType.LineBreak:
                    await WriteBlankLineAsync(writer, cancellationToken);
                    break;

                default:
                    await _buffer.WriteToAsync(writer, segment.Length, cancellationToken);
                    break;
                }
            }
        }

        public async Task<AsyncBreakLineResult> BreakLineAsync(TextWriter writer, ReadOnlyMemory<char> newSegment, int maxLength, int indent, WrappingMode mode, CancellationToken cancellationToken)
        {
            Debug.Assert(mode != WrappingMode.Disabled);
            var forceMode = _hasOverflow ? BreakLineMode.Forward : BreakLineMode.Backward;
            var result = await BreakLineAsync(writer, newSegment, maxLength, indent, forceMode, cancellationToken);
            if (!result.Success && forceMode != BreakLineMode.Forward)
            {
                forceMode = mode == WrappingMode.EnabledNoForce ? BreakLineMode.Forward : BreakLineMode.Force;
                result = await BreakLineAsync(writer, newSegment, maxLength, indent, forceMode, cancellationToken);
            }

            _hasOverflow = !result.Success && mode == WrappingMode.EnabledNoForce;
            return result;
        }

        private async Task<AsyncBreakLineResult> BreakLineAsync(TextWriter writer, ReadOnlyMemory<char> newSegment, int maxLength, int indent, BreakLineMode mode, CancellationToken cancellationToken)
        {
            if (mode == BreakLineMode.Forward)
            {
                maxLength = Math.Max(maxLength, LineLength + newSegment.Span.Length - 1);
            }

            // Line length can be over the max length if the previous place a line was split
            // plus the indentation is more than the line length.
            if (LineLength <= maxLength &&
                newSegment.Span.Length != 0 &&
                newSegment.BreakLine(maxLength - LineLength, mode, out var splits))
            {
                var (before, after) = splits;
                await WriteSegmentsAsync(writer, _segments, cancellationToken);
                await before.WriteToAsync(writer, cancellationToken);
                await writer.WriteLineAsync();
                ClearCurrentLine(indent);
                Indentation = indent;
                return new() { Success = true, Remaining = after };
            }

            // If forward mode is being used, we know there are no usable breaks in the buffer
            // because the line would've been broken there before the segment was put in the
            // buffer.
            if (IsContentEmpty || mode == BreakLineMode.Forward)
            {
                return new() { Success = false };
            }

            int offset = 0;
            int contentOffset = Indentation;
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

                int breakIndex = mode == BreakLineMode.Force
                    ? Math.Min(segment.Length, maxLength - contentOffset)
                    : _buffer.BreakLine(offset, Math.Min(segment.Length, maxLength - contentOffset));

                if (breakIndex >= 0)
                {
                    await WriteSegmentsAsync(writer, _segments.Take(segmentIndex), cancellationToken);
                    breakIndex -= offset;
                    await _buffer.WriteToAsync(writer, breakIndex, cancellationToken);
                    await writer.WriteLineAsync();
                    if (mode != BreakLineMode.Force)
                    {
                        _buffer.Discard(1);
                        breakIndex += 1;
                    }

                    if (breakIndex < segment.Length)
                    {
                        _segments.RemoveRange(0, segmentIndex);
                        segment.Length -= breakIndex;
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

    private async Task FlushCoreAsync(bool insertNewLine, CancellationToken cancellationToken)
    {
        ThrowIfWriteInProgress();
        if (_lineBuffer != null)
        {
            await _lineBuffer.FlushToAsync(_baseWriter, insertNewLine ? _indent : 0, insertNewLine, cancellationToken);
        }

        await _baseWriter.FlushAsync();
    }

    private async Task ResetIndentCoreAsync(CancellationToken cancellationToken)
    {
        if (_lineBuffer != null)
        {
            if (!_lineBuffer.IsContentEmpty)
            {
                await _lineBuffer.FlushToAsync(_baseWriter, 0, true, cancellationToken);
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

    private async Task WriteNoMaximumAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken)
    {
        Debug.Assert(Wrapping == WrappingMode.Disabled);

        await buffer.SplitAsync(true, async (type, span) =>
        {
            switch (type)
            {
            case StringSegmentType.PartialLineBreak:
                // If we already had a partial line break, write it now.
                if (_noWrappingState.HasPartialLineBreak)
                {
                    await WriteLineBreakDirectAsync(cancellationToken);
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
                        await WriteLineBreakDirectAsync(cancellationToken);
                    }
                }

                await WriteLineBreakDirectAsync(cancellationToken);
                break;

            default:
                // If we had a partial line break, write it now.
                if (_noWrappingState.HasPartialLineBreak)
                {
                    await WriteLineBreakDirectAsync(cancellationToken);
                    _noWrappingState.HasPartialLineBreak = false;
                }

                await WriteIndentDirectIfNeededAsync();
                await span.WriteToAsync(_baseWriter, cancellationToken);
                _noWrappingState.CurrentLineLength += span.Span.Length;
                break;
            }
        });
    }

    private async Task WriteLineBreakDirectAsync(CancellationToken cancellationToken)
    {
        await WriteBlankLineAsync(_baseWriter, cancellationToken);
        _noWrappingState.IndentNextWrite = IndentAfterEmptyLine || _noWrappingState.CurrentLineLength != 0;
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

    private async Task WriteCoreAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfWriteInProgress();
        if (Wrapping == WrappingMode.Disabled)
        {
            await WriteNoMaximumAsync(buffer, cancellationToken);
            return;
        }

        await buffer.SplitAsync(_countFormatting, async (type, span) =>
        {
            // _lineBuffer is guaranteed not null by EnableWrapping but the attribute for that
            // only exists in .Net 6.0.
            bool hadPartialLineBreak = _lineBuffer!.CheckAndRemovePartialLineBreak();
            if (hadPartialLineBreak)
            {
                await _lineBuffer.WriteLineToAsync(_baseWriter, _indent, cancellationToken);
            }

            if (type == StringSegmentType.LineBreak)
            {
                // Check if this is just the end of a partial line break. If it is, it was
                // already written above.
                if (!hadPartialLineBreak || span.Span.Length > 1 || (span.Span.Length == 1 && span.Span[0] != '\n'))
                {
                    await _lineBuffer.WriteLineToAsync(_baseWriter, _indent, cancellationToken);
                }
            }
            else
            {
                var remaining = span;
                if (type == StringSegmentType.Text)
                {
                    remaining = _lineBuffer.FindPartialFormattingEnd(remaining);
                    while (_lineBuffer.LineLength + remaining.Span.Length > _maximumLineLength)
                    {
                        var result = await _lineBuffer.BreakLineAsync(_baseWriter, remaining, _maximumLineLength, _indent, _wrapping, cancellationToken);
                        if (!result.Success)
                        {
                            break;
                        }

                        remaining = result.Remaining;
                    }
                }

                if (remaining.Span.Length > 0)
                {
                    _lineBuffer.Append(remaining.Span, type);
                    Debug.Assert(_lineBuffer.LineLength <= _maximumLineLength || Wrapping == WrappingMode.EnabledNoForce);
                }
            }
        });
    }
    private static async Task WriteBlankLineAsync(TextWriter writer, CancellationToken cancellationToken)
    {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        await writer.WriteLineAsync(ReadOnlyMemory<char>.Empty, cancellationToken);
#else
        await writer.WriteLineAsync();
#endif
    }
}
