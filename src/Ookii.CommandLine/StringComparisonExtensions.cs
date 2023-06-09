using System;

namespace Ookii.CommandLine;

internal static class StringComparisonExtensions
{
    public static StringComparer GetComparer(this StringComparison comparison)
    {
#if NETSTANDARD2_0
        return comparison switch
        {
            StringComparison.CurrentCulture => StringComparer.CurrentCulture,
            StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
            StringComparison.InvariantCulture => StringComparer.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
            StringComparison.Ordinal => StringComparer.Ordinal,
            StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
            _ => throw new ArgumentException(Properties.Resources.InvalidStringComparison, nameof(comparison))
        };
#else
        return StringComparer.FromComparison(comparison);
#endif
    }
}
