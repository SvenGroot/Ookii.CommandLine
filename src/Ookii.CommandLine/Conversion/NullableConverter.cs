using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

internal class NullableConverter : ArgumentConverter
{
    private readonly ArgumentConverter _baseConverter;

    public NullableConverter(ArgumentConverter baseConverter)
    {
        _baseConverter = baseConverter;
    }

    public override object? Convert(string value, CultureInfo culture)
    {
        if (value.Length == 0)
        {
            return null;
        }

        return _baseConverter.Convert(value, culture);
    }

    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture)
    {
        if (value.Length == 0)
        {
            return null;
        }

        return _baseConverter.Convert(value, culture);
    }
}
