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

        public static (string, string?) SplitOnce(this string value, string separator, int start = 0)
        {
            var index = value.IndexOf(separator);
            return value.SplitAt(index, start, separator.Length);
        }

        private static (string, string?) SplitAt(this string value, int index, int start, int skip)
        {
            if (index < 0)
            {
                return (value.Substring(start), null);
            }

            var before = value.Substring(start, index - start);
            var after = value.Substring(index + skip);
            return (before, after);
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
}
