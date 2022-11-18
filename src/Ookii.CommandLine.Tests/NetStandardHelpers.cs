#if !NET6_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.CommandLine.Tests
{
    internal static class KeyValuePair
    {
        public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }

    internal static class StringExtensions
    {
        private static readonly char[] _newLineChars = { '\r', '\n' };

        public static string ReplaceLineEndings(this string value, string ending = null)
        {
            ending ??= Environment.NewLine;
            var result = new StringBuilder();
            int pos = 0;
            while (pos < value.Length)
            {
                int index = value.IndexOfAny(_newLineChars, pos);
                if (index < 0)
                {
                    result.Append(value.Substring(pos));
                    break;
                }

                if (index > pos)
                {
                    result.Append(value.Substring(pos, index - pos));
                }

                result.Append(ending);
                if (value[index] == '\r' && index + 1 < value.Length && value[index + 1] == '\n')
                {
                    pos = index + 2;
                }
                else
                {
                    pos = index + 1;
                }
            }

            return result.ToString();
        }
    }
}

#endif