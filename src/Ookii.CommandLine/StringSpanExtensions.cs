using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using StringSpan = System.ReadOnlySpan<char>;
#endif

namespace Ookii.CommandLine
{
    // These methods are declared as extension methods so they can be used with StringSpan on
    // .Net Standard 2.0 and with ReadOnlySpan<char> on .Net Standard 2.1.
    internal static class StringSpanExtensions
    {
        public delegate void Callback(StringSegmentType type, StringSpan span);

        private static readonly char[] _segmentSeparators = { '\r', '\n', VirtualTerminal.Escape };
        private static readonly char[] _newLineSeparators = { '\r', '\n' };

        public static void Split(this StringSpan self, bool newLinesOnly, Callback callback)
        {
            var separators = newLinesOnly ? _newLineSeparators : _segmentSeparators;
            var remaining = self;
            while (remaining.Length > 0)
            {
                var separatorIndex = remaining.IndexOfAny(separators);
                if (separatorIndex < 0)
                {
                    callback(StringSegmentType.Text, remaining);
                    break;
                }

                if (separatorIndex > 0)
                {
                    callback(StringSegmentType.Text, remaining.Slice(0, separatorIndex));
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
                        callback(StringSegmentType.PartialFormatting, remaining);
                        break;
                    }

                    //end++;
                    callback(StringSegmentType.Formatting, remaining.Slice(0, end));
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
                        callback(StringSegmentType.PartialLineBreak, lineBreak);
                        break;
                    }

                    callback(StringSegmentType.LineBreak, lineBreak);
                }
            }
        }

        public static StringSpanTuple SkipLineBreak(this StringSpan self)
        {
            Debug.Assert(self[0] is '\r' or '\n');
            var split = self[0] == '\r' && self.Length > 1 && self[1] == '\n'
                ? 2
                : 1;

            return self.Split(split);
        }

        public static StringSpanTuple Split(this StringSpan self, int index)
            => new(self.Slice(0, index), self.Slice(index));

        // On .Net 6 StringSpanTuple is a ref struct so it can't be used with Nullable<T>, so use
        // an out param instead.
        public static bool BreakLine(this StringSpan self, int startIndex, bool force, out StringSpanTuple splits)
        {
            if (force)
            {
                splits = new(self.Slice(0, startIndex), self.Slice(startIndex));
                return true;
            }

            for (int index = startIndex; index >= 0; --index)
            {
                if (char.IsWhiteSpace(self[index]))
                {
                    splits = new(self.Slice(0, index), self.Slice(index + 1));
                    return true;
                }
            }

            splits = default;
            return false;
        }

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public static void CopyTo(this StringSpan self, char[] destination, int start)
        {
            self.CopyTo(destination.AsSpan(start));
        }

        public static void WriteTo(this StringSpan self, TextWriter writer)
        {
            writer.Write(self);
        }

#endif
    }
}
