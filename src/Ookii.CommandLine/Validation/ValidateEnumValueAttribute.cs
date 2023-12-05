using Ookii.CommandLine.Conversion;
using System;
using System.Globalization;

namespace Ookii.CommandLine.Validation;

/// <summary>
/// Validates whether the value of an enumeration type is one of the defined values for that
/// type.
/// </summary>
/// <remarks>
/// <para>
///   The <see cref="EnumConverter"/> used to convert values for arguments with enumeration types
///   allows conversion using the string representation of the underlying value, as well as the
///   name. While names are checked against the members, any underlying value can be converted to an
///   enumeration, regardless of whether it's a defined value for the enumeration.
/// </para>
/// <para>
///   For example, using the <see cref="DayOfWeek"/> enumeration, converting a string value of
///   "9" would result in a value of <c>(DayOfWeek)9</c>, even though there is no enumeration
///   member with that value.
/// </para>
/// <para>
///   This validator makes sure that the result of conversion is a valid value for the
///   enumeration, by using the <see cref="Enum.IsDefined(Type, object)" qualifyHint="true"/> method.
/// </para>
/// <para>
///   In addition, this validator provides usage help listing all the possible values. If the
///   enumeration has a lot of values, you may wish to turn this off by setting the
///   <see cref="ArgumentValidationWithHelpAttribute.IncludeInUsageHelp" qualifyHint="true"/> property to
///   <see langword="false"/>. Similarly, you can avoid listing all the values in the error
///   message by setting the <see cref="IncludeValuesInErrorMessage"/> property to
///   <see langword="false"/>.
/// </para>
/// <para>
///   It is an error to use this validator on an argument whose type is not an enumeration.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public class ValidateEnumValueAttribute : ArgumentValidationWithHelpAttribute
{
    /// <summary>
    /// Gets or sets a value that indicates whether values that do not match one of the
    /// enumeration's defined values are allowed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if values that are not defined by the enumeration are allowed;
    /// otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Non-defined values can be provided using the underlying numeric type of the enumeration.
    ///   If this property is <see langword="true"/>, this validator will not check whether a value
    ///   provided in such a way actually is actually one of the enumeration's defined values.
    /// </para>
    /// <para>
    ///   Setting this to <see langword="true"/> essentially makes this validator do nothing. It
    ///   is useful if you want to use it solely to list defined values in the usage help, or if
    ///   you want to use one of the other properties that affect the <see cref="EnumConverter"/>
    ///   without also checking for defined values.
    /// </para>
    /// </remarks>
    public bool AllowNonDefinedValues { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the possible values of the enumeration
    /// should be included in the error message if validation fails.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to include the values; otherwise, <see langword="false"/>. The
    /// default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property is used when validation fails, and is also checked by the
    ///   <see cref="EnumConverter"/> class, which is the default converter for enumeration types,
    ///   when conversion fails due to an invalid string value.
    /// </para>
    /// </remarks>
    public bool IncludeValuesInErrorMessage { get; set; } = true;

    /// <summary>
    /// Gets or sets a value that indicates whether enumeration value conversion is case sensitive.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if conversion is case sensitive; otherwise, <see langword="false"/>.
    /// The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property is not used by the <see cref="ValidateEnumValueAttribute"/> class itself,
    ///   but by the <see cref="EnumConverter"/> class. Therefore, this property may not work if
    ///   a custom argument converter is used, unless that custom converter also checks this
    ///   property.
    /// </para>
    /// </remarks>
    public bool CaseSensitive { get; set; }

    /// <inheritdoc/>
    /// <summary>Determines if the argument's value is defined.</summary>
    /// <exception cref="NotSupportedException">
    /// <paramref name="argument"/> is not an argument with an enumeration type.
    /// </exception>
    public override bool IsValid(CommandLineArgument argument, object? value)
    {
        if (!argument.ElementType.IsEnum)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.ArgumentNotEnumFormat, argument.ArgumentName));
        }

        if (AllowNonDefinedValues)
        {
            return true;
        }

        return value == null || argument.ElementType.IsEnumDefined(value);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    ///   Use a custom <see cref="LocalizedStringProvider"/> class that overrides the
    ///   <see cref="LocalizedStringProvider.ValidateEnumValueUsageHelp" qualifyHint="true"/> method
    ///   to customize this message.
    /// </para>
    /// </remarks>
    protected override string GetUsageHelpCore(CommandLineArgument argument)
        => argument.Parser.StringProvider.ValidateEnumValueUsageHelp(argument.ElementType);

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    ///   Use a custom <see cref="LocalizedStringProvider"/> class that overrides the
    ///   <see cref="LocalizedStringProvider.ValidateEnumValueFailed" qualifyHint="true"/> method
    ///   to customize this message.
    /// </para>
    /// </remarks>
    public override string GetErrorMessage(CommandLineArgument argument, object? value)
        => argument.Parser.StringProvider.ValidateEnumValueFailed(argument.ArgumentName, argument.ElementType, value,
                IncludeValuesInErrorMessage);
}
