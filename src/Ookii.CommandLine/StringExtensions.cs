using System;

namespace Ookii.CommandLine
{
    internal static class StringExtensions
    {
        public static (ReadOnlyMemory<char>, ReadOnlyMemory<char>?) SplitOnce(this ReadOnlyMemory<char> value, char separator)
        {
            var index = value.Span.IndexOf(separator);
            return value.SplitAt(index, 1);
        }

        public static (ReadOnlyMemory<char>, ReadOnlyMemory<char>?) SplitFirstOfAny(this ReadOnlyMemory<char> value, ReadOnlySpan<char> separators)
        {
            var index = value.Span.IndexOfAny(separators);
            return value.SplitAt(index, 1);
        }

        public static StringSpanTuple SplitOnce(this ReadOnlySpan<char> value, ReadOnlySpan<char> separator, out bool hasSeparator)
        {
            var index = value.IndexOf(separator);
            return value.SplitAt(index, separator.Length, out hasSeparator);
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

        private static StringSpanTuple SplitAt(this ReadOnlySpan<char> value, int index, int skip, out bool hasSeparator)
        {
            if (index < 0)
            {
                hasSeparator = false;
                return new(value, default);
            }

            var before = value.Slice(0, index);
            var after = value.Slice(index + skip);
            hasSeparator = true;
            return new(before, after);
        }
    }
}
