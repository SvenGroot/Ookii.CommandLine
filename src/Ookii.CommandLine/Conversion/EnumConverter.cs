using Ookii.CommandLine.Validation;
using System;
using System.Globalization;
using System.Linq;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// A converter for arguments with enumeration values.
/// </summary>
/// <remarks>
/// <para>
///   This converter performs a case insensitive conversion, and accepts the name of an enumeration
///   value, or its underlying value. In the latter case, the value does not need to be one of the
///   defined values of the enumeration; use the <see cref="Validation.ValidateEnumValueAttribute"/>
///   attribute to ensure only defined enumeration values can be used.
/// </para>
/// <para>
///   A comma-separated list of values is also accepted, which will be combined using a bitwise-or
///   operation. This is accepted regardless of whether the enumeration uses the <see cref="FlagsAttribute"/>
///   attribute.
/// </para>
/// <para>
///   If conversion fails, the error message will check the
///   <see cref="ValidateEnumValueAttribute.IncludeValuesInErrorMessage" qualifyHint="true"/>
///   property to see whether or not the enumeration's defined values should be listed in the
///   error message. If the argument does not have the <see cref="ValidateEnumValueAttribute"/>
///   attribute applied, the values will be listed.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public class EnumConverter : ArgumentConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumConverter"/> for the specified enumeration
    /// type.
    /// </summary>
    /// <param name="enumType">The enumeration type.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="enumType"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="enumType"/> is not an enumeration type.
    /// </exception>
    public EnumConverter(Type enumType)
    {
        EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
        if (!EnumType.IsEnum)
        {
            throw new ArgumentException(
                string.Format(CultureInfo.CurrentCulture, Properties.Resources.TypeIsNotEnumFormat, EnumType.FullName),
                nameof(enumType));
        }
    }

    /// <summary>
    /// Gets the enumeration type that this converter converts to.
    /// </summary>
    /// <value>
    /// The enumeration type.
    /// </value>
    public Type EnumType { get; }

    /// <summary>
    /// Converts a string to the enumeration type.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <param name="argument">
    /// The <see cref="CommandLineArgument"/> that will use the converted value.
    /// </param>
    /// <returns>An object representing the converted value.</returns>
    /// <remarks>
    /// <para>
    ///   This method performs the conversion using the <see cref="Enum.Parse(Type, string, bool)" qualifyHint="true"/>
    ///   method.
    /// </para>
    /// </remarks>
    /// <exception cref="CommandLineArgumentException">
    ///   The value was not valid for the enumeration type.
    /// </exception>
    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument)
    {
        try
        {
            return Enum.Parse(EnumType, value, true);
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
    /// <summary>
    /// Converts a string span to the enumeration type.
    /// </summary>
    /// <param name="value">The <see cref="ReadOnlySpan{T}"/> containing the string to convert.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <param name="argument">
    /// The <see cref="CommandLineArgument"/> that will use the converted value.
    /// </param>
    /// <returns>An object representing the converted value.</returns>
    /// <remarks>
    /// <para>
    ///   This method performs the conversion using the <see cref="Enum.Parse(Type, ReadOnlySpan{char}, bool)" qualifyHint="true"/>
    ///   method.
    /// </para>
    /// </remarks>
    /// <exception cref="CommandLineArgumentException">
    ///   The value was not valid for the enumeration type.
    /// </exception>
    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        try
        {
            return Enum.Parse(EnumType, value, true);
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

    private CommandLineArgumentException CreateException(string value, Exception inner, CommandLineArgument argument)
    {
        var attribute = argument.Validators.OfType<ValidateEnumValueAttribute>().FirstOrDefault();
        var includeValues = attribute?.IncludeValuesInErrorMessage ?? true;
        var message = argument.Parser.StringProvider.ValidateEnumValueFailed(argument.ArgumentName, EnumType, value, includeValues);
        return new(message, argument.ArgumentName,CommandLineArgumentErrorCategory.ArgumentValueConversion, inner);
    }
}
