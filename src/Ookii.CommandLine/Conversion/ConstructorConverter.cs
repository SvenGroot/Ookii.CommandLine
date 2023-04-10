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

    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument)
    {
        try
        {
            return _type.CreateInstance(value);
        }
        catch (CommandLineArgumentException ex)
        {
            // Patch the exception with the argument name.
            throw new CommandLineArgumentException(ex.Message, argument.ArgumentName, ex.Category, ex.InnerException);
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
