using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

// Boolean doesn't support ISpanParsable<T>, so special-case it.
internal class BooleanConverter : ArgumentConverter
{
    public static readonly BooleanConverter Instance = new();

    public override object? Convert(string value, CultureInfo culture) => bool.Parse(value);

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture) => bool.Parse(value);
#endif
}
