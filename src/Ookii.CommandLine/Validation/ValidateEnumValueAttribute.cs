using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Validates whether the value of an enumeration type is one of the defined values for that
    /// type.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The <see cref="EnumConverter"/> class, which is the default <see cref="TypeConverter"/>
    ///   for enumerations, allows conversion using the string representation of the underlying
    ///   value, as well as the name. While names are checked against the members, any underlying
    ///   value can be converted to an enumeration, regardless of whether it's a defined value for
    ///   the enumeration.
    /// </para>
    /// <para>
    ///   For example, using the <see cref="DayOfWeek"/> enumeration, converting a string value of
    ///   "9" would result in a value of <c>(DayOfWeek)9</c>, even though there is no enumeration
    ///   member with that value.
    /// </para>
    /// <para>
    ///   This validator makes sure that the result of conversion is a valid value for the
    ///   enumeration, by using the <see cref="Enum.IsDefined(Type, object)"/> method.
    /// </para>
    /// <para>
    ///   In addition, this validator provides usage help listing all the possible values. If the
    ///   enumeration has a lot of values, you may wish to turn this off by setting the
    ///   <see cref="ArgumentValidationWithHelpAttribute.IncludeInUsageHelp"/> property to
    ///   <see langword="false"/>. Similarly, you can avoid listing all the values in the error
    ///   message by setting the <see cref="IncludeValuesInErrorMessage"/> property to
    ///   <see langword="false"/>.
    /// </para>
    /// <para>
    ///   It is an error to use this validator on an argument whose type is not an enumeration.
    /// </para>
    /// </remarks>
    public class ValidateEnumValueAttribute : ArgumentValidationWithHelpAttribute
    {
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

            return value == null || argument.ElementType.IsEnumDefined(value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the possible values of the enumeration
        /// should be included in the error message if validation fails.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to include the error messages; otherwise, <see langword="false"/>.
        /// </value>
        public bool IncludeValuesInErrorMessage { get; set; }

        /// <inheritdoc/>
        protected override string GetUsageHelpCore(CommandLineArgument argument)
            => argument.Parser.StringProvider.ValidateEnumValueUsageHelp(argument.ElementType);

        /// <inheritdoc/>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
            => argument.Parser.StringProvider.ValidateEnumValueFailed(argument.ArgumentName, argument.ElementType, value,
                    IncludeValuesInErrorMessage);
    }
}
