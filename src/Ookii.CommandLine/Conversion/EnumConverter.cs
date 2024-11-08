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
///   value or a number representing the underlying type of the enumeration. Comma-separated values
///   that will be combined using bitwise-or are also accepted, regardless of whether the
///   enumeration uses the <see cref="FlagsAttribute"/> attribute. When using a numeric value, the
///   value does not need to be one of the defined values of the enumeration.
/// </para>
/// <para>
///   Use the <see cref="ValidateEnumValueAttribute"/> attribute to alter these behaviors. Applying
///   that attribute will ensure that only values defined by the enumeration are allowed. The
///   <see cref="ValidateEnumValueAttribute.AllowCommaSeparatedValues" qualifyHint="true"/>
///   property can be used to control the use of multiple values, and the 
///   <see cref="ValidateEnumValueAttribute.AllowNumericValues" qualifyHint="true"/> property
///   controls the use of numbers instead of names. Set the
///   <see cref="ValidateEnumValueAttribute.CaseSensitive" qualifyHint="true"/> property to
///   <see langword="true"/> to enable case sensitive conversion.
/// </para>
/// <para>
///   If conversion fails, the converter will check the
///   <see cref="ValidateEnumValueAttribute.IncludeValuesInErrorMessage" qualifyHint="true"/>
///   property to see whether or not the enumeration's defined values should be listed in the
///   error message. If the argument does not have the <see cref="ValidateEnumValueAttribute"/>
///   attribute applied, the enumeration's values will be listed in the message.
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
    /// Converts a string memory region to the enumeration type.
    /// </summary>
    /// <param name="value">The <see cref="ReadOnlyMemory{T}"/> containing the string to convert.</param>
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
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="argument"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="CommandLineArgumentException">
    ///   The value was not valid for the enumeration type.
    /// </exception>
    public override object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(nameof(argument));
        }

        var attribute = argument.Validators.OfType<ValidateEnumValueAttribute>().FirstOrDefault();
        if (attribute != null && !attribute.ValidateBeforeConversion(argument, value.Span))
        {
            throw CreateException(value.ToString(), null, argument, attribute);
        }

        try
        {
#if NET6_0_OR_GREATER
            return Enum.Parse(EnumType, value.Span, !attribute?.CaseSensitive ?? true);
#else
            return Enum.Parse(EnumType, value.ToString(), !attribute?.CaseSensitive ?? true);
#endif
        }
        catch (ArgumentException ex)
        {
            throw CreateException(value.ToString(), ex, argument, attribute);
        }
        catch (OverflowException ex)
        {
            throw CreateException(value.ToString(), ex, argument, attribute);
        }
    }

    private CommandLineArgumentException CreateException(string value, Exception? inner, CommandLineArgument argument,
        ValidateEnumValueAttribute? attribute)
    {
        string message = attribute?.GetErrorMessage(argument, value)
            ?? argument.Parser.StringProvider.ValidateEnumValueFailed(argument.ArgumentName, EnumType, value, true);

        return new(message, argument.ArgumentName, CommandLineArgumentErrorCategory.ArgumentValueConversion, inner);
    }
}
