using System;

namespace Ookii.CommandLine;

internal static class StringExtensions
{
    public static (ReadOnlyMemory<char>, ReadOnlyMemory<char>?) SplitFirstOfAny(this ReadOnlyMemory<char> value, ReadOnlySpan<char> separators)
    {
        var index = value.Span.IndexOfAny(separators);
        return value.SplitAt(index, 1);
    }

    private static (ReadOnlyMemory<char>, ReadOnlyMemory<char>?) SplitAt(this ReadOnlyMemory<char> value, int index, int skip)
    {
        if (index < 0)
        {
            return (value, null);
        }

        var before = value.Slice(0, index);
        var after = value.Slice(index + skip);
        return (before, after);
    }
}
