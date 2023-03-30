#if NET7_0_OR_GREATER

using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion
{
    internal class SpanParsableConverter<T> : ArgumentConverter
        where T : ISpanParsable<T>
    {
        public override object? Convert(string value, CultureInfo culture) => T.Parse(value, culture);

        public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture) => T.Parse(value, culture);
    }
}

#endif
