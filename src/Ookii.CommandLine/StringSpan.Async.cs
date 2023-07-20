// The async methods in this file are used to generate the normal, non-async versions using the
// Convert-SyncMethod.ps1 script.
using Ookii.CommandLine.Terminal;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ookii.CommandLine;


internal static partial class StringSpanExtensions
{

    public static async Task WriteToAsync(this ReadOnlyMemory<char> self, TextWriter writer, CancellationToken cancellationToken)
    {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        await writer.WriteAsync(self, cancellationToken);
#else
        await writer.WriteAsync(self.ToString());
#endif
    }

    public static async Task SplitAsync(this ReadOnlyMemory<char> self, bool newLinesOnly, AsyncCallback callback)
    {
        var separators = newLinesOnly ? _newLineSeparators : _segmentSeparators;
        var remaining = self;
        while (remaining.Span.Length > 0)
        {
            var separatorIndex = remaining.Span.IndexOfAny(separators);
            if (separatorIndex < 0)
            {
                await callback(StringSegmentType.Text, remaining);
                break;
            }

            if (separatorIndex > 0)
            {
                await callback(StringSegmentType.Text, remaining.Slice(0, separatorIndex));
                remaining = remaining.Slice(separatorIndex);
            }

            if (remaining.Span[0] == VirtualTerminal.Escape)
            {
                // This is a VT sequence.
                // Find the end of the sequence.
                StringSegmentType type = StringSegmentType.PartialFormattingUnknown;
                var end = VirtualTerminal.FindSequenceEnd(remaining.Slice(1).Span, ref type);
                if (end == -1)
                {
                    // No end? Should come in a following write.
                    await callback(type, remaining);
                    break;
                }

                // Add one for the escape character, and one to skip past the end.
                end += 2;
                await callback(StringSegmentType.Formatting, remaining.Slice(0, end));
                remaining = remaining.Slice(end);
            }
            else
            {
                ReadOnlyMemory<char> lineBreak;
                (lineBreak, remaining) = remaining.SkipLineBreak();

                if (remaining.Span.Length == 0 && lineBreak.Span.Length == 1 && lineBreak.Span[0] == '\r')
                {
                    // This could be the start of a Windows-style break, the remainder of
                    // which could follow in the next span.
                    await callback(StringSegmentType.PartialLineBreak, lineBreak);
                    break;
                }

                await callback(StringSegmentType.LineBreak, lineBreak);
            }
        }
    }
}
