using System.Collections.Generic;
using System.Linq;

namespace Ookii.CommandLine
{
    internal static class EnumerableHelper
    {
#if !NET6_0_OR_GREATER
        // Provide an implementation of Append for .Net Standard 2.0
        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
        {
            return source.Concat(Enumerable.Repeat(item, 1));
        }
#endif
    }
}
