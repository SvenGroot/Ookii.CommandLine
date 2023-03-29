using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion
{
    public abstract class ArgumentConverter
    {
        public abstract object? Convert(string value, CultureInfo culture);

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public virtual object? Convert(ReadOnlySpan<char> value, CultureInfo culture)
        {
            return Convert(value.ToString(), culture);
        }
#endif
    }
}
