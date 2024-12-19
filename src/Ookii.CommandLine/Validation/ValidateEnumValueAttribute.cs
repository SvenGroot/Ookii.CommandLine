using Ookii.CommandLine.Conversion;
using Ookii.Common;
using System;
using System.Globalization;
using System.Reflection;

namespace Ookii.CommandLine.Validation;

/// <summary>
/// Controls validation rules for arguments with enumeration values.
/// </summary>
/// <remarks>
/// <para>
///   Conversion from a string to an enumeration value can use either the name of the enumeration
///   member, the numeric value of the member, or a comma-separated list of values. By default,
///   however, the <see cref="EnumConverter"/> class only allows names of enumeration members,
///   and comma-separated values are only allowed if the enumeration has the
///   <see cref="FlagsAttribute"/> attribute.
/// </para>
/// <para>
///   Using the <see cref="ValidateEnumValueAttribute"/>, you can change these behaviors. You can
///   allow numeric values with the <see cref="AllowNumericValues"/> property, and you can force
///   whether comma-separated values are allowed with the <see cref="AllowCommaSeparatedValues"/>
///   property.
/// </para>
/// <para>
///   If numeric values are allowed, they are restricted to values that are defined in the
///   enumeration, unless the enumeration has the <see cref="FlagsAttribute"/> applied or you set
///   the <see cref="AllowNonDefinedValues"/> property to <see cref="TriState.True" qualifyHint="true"/>.
///   In that case, for example using the <see cref="DayOfWeek"/> enumeration, converting a string
///   value of "9" would result in a value of <c>(DayOfWeek)9</c>, even though there is no
///   enumeration member with that value.
/// </para>
/// <para>
///   In addition, this validator provides usage help listing all the possible values. If the
///   enumeration has a lot of values, you may wish to turn this off by setting the
///   <see cref="ArgumentValidationWithHelpAttribute.IncludeInUsageHelp" qualifyHint="true"/>
///   property to <see langword="false"/>. Similarly, you can avoid listing all the values in the
///   error message by setting the <see cref="IncludeValuesInErrorMessage"/> property to
///   <see langword="false"/>.
/// </para>
/// <para>
///   If this validator is used without changing any of its properties on an argument that uses the
///   default <see cref="EnumConverter"/>, its only effect is to list the values in the usage help.
///   The default behavior of the <see cref="EnumConverter"/> class is the same as the defaults of
///   this validator.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public class ValidateEnumValueAttribute : ArgumentValidationWithHelpAttribute
{
    internal static readonly ValidateEnumValueAttribute Default = new();

    /// <summary>
    /// Gets or sets a value that indicates whether values that do not match one of the
    /// enumeration's defined values are allowed.
    /// </summary>
    /// <value>
    /// <see cref="TriState.True" qualifyHint="true"/> if values that are not defined by the
    /// enumeration are allowed; <see cref="TriState.False" qualifyHint="true"/> if they are not
    /// allowed; <see cref="TriState.Auto" qualifyHint="true"/> to allow non-defined values only
    /// when the enumeration has the <see cref="FlagsAttribute"/> attribute applied. The default
    /// value is <see cref="TriState.Auto" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Non-defined values can be provided using the underlying numeric type of the enumeration,
    ///   or by using comma-separated values. If this property is <see cref="TriState.True"
    ///   qualifyHint="true"/>, or <see cref="TriState.Auto" qualifyHint="true"/> and the
    ///   enumeration has the <see cref="FlagsAttribute"/> attribute applied, this validator will
    ///   not check whether a value provided in such a way actually is actually one of the
    ///   enumeration's defined values.
    /// </para>
    /// </remarks>
    public TriState AllowNonDefinedValues { get; set; }

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
    ///   <see cref="EnumConverter"/> class when conversion fails due to an invalid string value.
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

    /// <summary>
    /// Gets or sets a value that indicates whether the value provided by the user can use commas
    /// to provide multiple values that will be combined with bitwise-or.
    /// </summary>
    /// <value>
    /// <see cref="TriState.True" qualifyHint="true"/> if comma-separated values are allowed;
    /// <see cref="TriState.False" qualifyHint="true"/> if they are not;
    /// <see cref="TriState.Auto" qualifyHint="true"/> to allow comma-separated values only if the
    /// enumeration has the <see cref="FlagsAttribute"/> applied. The default value is
    /// <see cref="TriState.Auto" qualifyHint="true"/>.
    /// </value>
    public TriState AllowCommaSeparatedValues { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the value provided by the user can the
    /// underlying numeric type of the enumeration.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if numeric values are allowed; otherwise, <see langword="false"/> to
    /// allow only value names. The default value is <see langword="false"/>.
    /// </value>
    public bool AllowNumericValues { get; set; }

    /// <inheritdoc/>
    /// <summary>Determines if the argument's value is defined.</summary>
    /// <exception cref="NotSupportedException">
    /// <paramref name="argument"/> is not an argument with an enumeration type.
    /// </exception>
    public override bool IsValidPostConversion(CommandLineArgument argument, object? value)
    {
        if (!argument.ElementType.IsEnum)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.ArgumentNotEnumFormat, argument.ArgumentName));
        }

        var allowNonDefinedValues = AllowNonDefinedValues switch
        {
            TriState.True => true,
            TriState.False => false,
            _ => argument.ElementType.GetCustomAttribute<FlagsAttribute>() != null
        };

        return allowNonDefinedValues || value == null || argument.ElementType.IsEnumDefined(value);
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

    /// <inheritdoc/>
    /// <summary>Determines if the argument's value contains commas or numbers if not allowed.</summary>
    public override bool IsValidPreConversion(CommandLineArgument argument, ReadOnlyMemory<char> value)
    {
        var allowCommaSeparatedValues = AllowCommaSeparatedValues switch
        {
            TriState.True => true,
            TriState.False => false,
            _ => argument.ElementType.GetCustomAttribute<FlagsAttribute>() != null
        };

        if (!allowCommaSeparatedValues && value.Span.IndexOf(',') >= 0)
        {
            return false;
        }

        if (!AllowNumericValues)
        {
            foreach (var segment in value.Span.Split(",".AsSpan()))
            {
                var trimmed = segment.TrimStart();
                if (trimmed.Length > 0 && char.IsDigit(trimmed[0]) ||
                    trimmed.StartsWith(argument.Parser.Culture.NumberFormat.NegativeSign.AsSpan()))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
