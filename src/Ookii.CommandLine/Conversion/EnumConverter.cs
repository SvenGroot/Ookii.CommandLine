using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// A converter for arguments with enumeration values.
/// </summary>
/// <remarks>
/// <para>
///   If conversion fails, this converter will provide an error message that includes all the
///   allowed values for the enumeration.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public class EnumConverter : ArgumentConverter
{
    private readonly Type _enumType;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumConverter"/> for the specified enumeration
    /// type.
    /// </summary>
    /// <param name="enumType">The enumeration type.</param>
    public EnumConverter(Type enumType)
    {
        _enumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
    }

    /// <inheritdoc/>
    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument)
    {
        try
        {
            return Enum.Parse(_enumType, value, true);
        }
        catch (ArgumentException ex)
        {
            throw CreateException(value, ex, argument);
        }
        catch (OverflowException ex)
        {
            throw CreateException(value, ex, argument);
        }
    }

#if NET6_0_OR_GREATER
    /// <inheritdoc/>
    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        try
        {
            return Enum.Parse(_enumType, value, true);
        }
        catch (ArgumentException ex)
        {
            throw CreateException(value.ToString(), ex, argument);
        }
        catch (OverflowException ex)
        {
            throw CreateException(value.ToString(), ex, argument);
        }
    }
#endif

    private Exception CreateException(string value, Exception inner, CommandLineArgument argument)
    {
        var message = argument.Parser.StringProvider.ValidateEnumValueFailed(argument.ArgumentName, _enumType, value, true);
        return new CommandLineArgumentException(message, argument.ArgumentName, CommandLineArgumentErrorCategory.ArgumentValueConversion, inner);
    }
}
