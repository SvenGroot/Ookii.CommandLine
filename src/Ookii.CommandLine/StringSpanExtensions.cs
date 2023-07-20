using Ookii.CommandLine.Terminal;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Ookii.CommandLine;

// These methods are declared as extension methods so they can be used with ReadOnlySpan<char> on
// .Net Standard 2.0 and with ReadOnlySpan<char> on .Net Standard 2.1.
internal static partial class StringSpanExtensions
{
    public delegate void Callback(StringSegmentType type, ReadOnlySpan<char> span);
    public delegate Task AsyncCallback(StringSegmentType type, ReadOnlyMemory<char> span);
    public delegate bool SplitCallback(ReadOnlySpan<char> span);

    private static readonly char[] _segmentSeparators = { '\r', '\n', VirtualTerminal.Escape };
    private static readonly char[] _newLineSeparators = { '\r', '\n' };

    public static partial void Split(this ReadOnlySpan<char> self, bool newLinesOnly, Callback callback);

    public static StringSpanTuple SkipLineBreak(this ReadOnlySpan<char> self)
    {
        Debug.Assert(self[0] is '\r' or '\n');
        var split = self[0] == '\r' && self.Length > 1 && self[1] == '\n'
            ? 2
            : 1;

        return self.Split(split);
    }

    public static StringSpanTuple Split(this ReadOnlySpan<char> self, int index)
        => new(self.Slice(0, index), self.Slice(index));

    // On .Net 6 StringSpanTuple is a ref struct so it can't be used with Nullable<T>, so use
    // an out param instead.
    public static bool BreakLine(this ReadOnlySpan<char> self, int startIndex, BreakLineMode mode, out StringSpanTuple splits)
    {
        if (BreakLine(self, startIndex, mode) is var (end, start))
        {
            splits = new(self.Slice(0, end), self.Slice(start));
            return true;
        }

        splits = default;
        return false;
    }

    public static (ReadOnlyMemory<char>, ReadOnlyMemory<char>) SkipLineBreak(this ReadOnlyMemory<char> self)
    {
        Debug.Assert(self.Span[0] is '\r' or '\n');
        var split = self.Span[0] == '\r' && self.Span.Length > 1 && self.Span[1] == '\n'
            ? 2
            : 1;

        return self.Split(split);
    }

    public static (ReadOnlyMemory<char>, ReadOnlyMemory<char>) Split(this ReadOnlyMemory<char> self, int index)
        => new(self.Slice(0, index), self.Slice(index));

    public static bool BreakLine(this ReadOnlyMemory<char> self, int startIndex, BreakLineMode mode, out (ReadOnlyMemory<char>, ReadOnlyMemory<char>) splits)
    {
        if (BreakLine(self.Span, startIndex, mode) is var (end, start))
        {
            splits = new(self.Slice(0, end), self.Slice(start));
            return true;
        }

        splits = default;
        return false;
    }

    public static void CopyTo(this ReadOnlySpan<char> self, char[] destination, int start)
    {
        self.CopyTo(destination.AsSpan(start));
    }

    public static void Split(this ReadOnlySpan<char> self, ReadOnlySpan<char> separator, SplitCallback callback)
    {
        while (!self.IsEmpty)
        {
            var (first, remaining) = self.SplitOnce(separator, out bool _);
            if (!callback(first))
            {
                break;
            }

            self = remaining;
        }
    }

    public static partial void WriteTo(this ReadOnlySpan<char> self, TextWriter writer);

    private static (int, int)? BreakLine(ReadOnlySpan<char> span, int startIndex, BreakLineMode mode)
    {
        switch (mode)
        {
        case BreakLineMode.Force:
            return (startIndex, startIndex);

        case BreakLineMode.Backward:
            for (int index = startIndex; index >= 0; --index)
            {
                if (char.IsWhiteSpace(span[index]))
                {
                    return (index, index + 1);
                }
            }

            break;

        case BreakLineMode.Forward:
            for (int index = 0; index <= startIndex; ++index)
            {
                if (char.IsWhiteSpace(span[index]))
                {
                    return (index, index + 1);
                }
            }

            break;
        }

        return null;
    }
}
