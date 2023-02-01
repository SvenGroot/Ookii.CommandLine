using Ookii.CommandLine.Terminal;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using StringSpan = System.ReadOnlySpan<char>;
using StringMemory = System.ReadOnlyMemory<char>;
#endif

namespace Ookii.CommandLine
{
    // These methods are declared as extension methods so they can be used with StringSpan on
    // .Net Standard 2.0 and with ReadOnlySpan<char> on .Net Standard 2.1.
    internal static partial class StringSpanExtensions
    {
        public delegate void Callback(StringSegmentType type, StringSpan span);
        public delegate Task AsyncCallback(StringSegmentType type, StringMemory span);

        private static readonly char[] _segmentSeparators = { '\r', '\n', VirtualTerminal.Escape };
        private static readonly char[] _newLineSeparators = { '\r', '\n' };

        public static partial void Split(this StringSpan self, bool newLinesOnly, Callback callback);

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
            if (BreakLine(self, startIndex, force) is var (end, start))
            {
                splits = new(self.Slice(0, end), self.Slice(start));
                return true;
            }

            splits = default;
            return false;
        }

        public static (StringMemory, StringMemory) SkipLineBreak(this StringMemory self)
        {
            Debug.Assert(self.Span[0] is '\r' or '\n');
            var split = self.Span[0] == '\r' && self.Span.Length > 1 && self.Span[1] == '\n'
                ? 2
                : 1;

            return self.Split(split);
        }

        public static (StringMemory, StringMemory) Split(this StringMemory self, int index)
            => new(self.Slice(0, index), self.Slice(index));

        public static bool BreakLine(this StringMemory self, int startIndex, bool force, out (StringMemory, StringMemory) splits)
        {
            if (BreakLine(self.Span, startIndex, force) is var (end, start))
            {
                splits = new(self.Slice(0, end), self.Slice(start));
                return true;
            }

            splits = default;
            return false;
        }

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER

        public static void CopyTo(this StringSpan self, char[] destination, int start)
        {
            self.CopyTo(destination.AsSpan(start));
        }

        public static partial void WriteTo(this StringSpan self, TextWriter writer);

#else

        public static StringSpan AsSpan(this string self)
        {
            return new StringSpan(self);
        }

        public static StringMemory AsMemory(this string self)
        {
            return new StringMemory(self);
        }

#endif

        private static (int, int)? BreakLine(StringSpan span, int startIndex, bool force)
        {
            if (force)
            {
                return (startIndex, startIndex);
            }

            for (int index = startIndex; index >= 0; --index)
            {
                if (char.IsWhiteSpace(span[index]))
                {
                    return (index, index + 1);
                }
            }

            return null;
        }
    }
}
