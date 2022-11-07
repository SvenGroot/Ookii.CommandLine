using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    internal static class StringExtensions
    {
        public static (string, string?) SplitOnce(this string value, char separator, int start = 0)
        {
            var index = value.IndexOf(separator, start);
            return value.SplitAt(index, start, 1);
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
    }
}
