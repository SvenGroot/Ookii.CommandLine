using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

internal class EnumConverter : ArgumentConverter
{
    private readonly Type _enumType;

    public EnumConverter(Type enumType)
    {
        _enumType = enumType;
    }

    public override object? Convert(string value, CultureInfo culture)
    {
        try
        {
            return Enum.Parse(_enumType, value, true);
        }
        catch (ArgumentException ex)
        {
            throw new FormatException(ex.Message, ex);
        }
        catch (OverflowException ex)
        {
            throw new FormatException(ex.Message, ex);
        }
    }

#if NET6_0_OR_GREATER
    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture)
    {
        try
        {
            return Enum.Parse(_enumType, value, true);
        }
        catch (ArgumentException ex)
        {
            throw new FormatException(ex.Message, ex);
        }
        catch (OverflowException ex)
        {
            throw new FormatException(ex.Message, ex);
        }
    }
#endif
}
