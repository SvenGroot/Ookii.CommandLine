using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

internal class ConstructorConverter : ArgumentConverter
{
    private readonly Type _type;

    public ConstructorConverter(Type type)
    {
        _type = type;
    }

    public override object? Convert(string value, CultureInfo culture)
    {
        try
        {
            return _type.CreateInstance(value);
        }
        catch (CommandLineArgumentException)
        {
            throw;
        }
        catch (FormatException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Since we don't know what the constructor will throw, we'll wrap anything in a
            // FormatException.
            throw new FormatException(ex.Message, ex);
        }
    }
}
