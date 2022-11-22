using Ookii.CommandLine.Properties;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ookii.CommandLine
{
    public partial class LocalizedStringProvider
    {
        private const string ArgumentSeparator = ", ";

        /// <summary>
        /// Gets a formatted list of validator help messages.
        /// </summary>
        /// <param name="argument">The command line argument.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <note>
        ///   The default implementation of <see cref="UsageWriter"/> expects the returned
        ///   value to start with a white-space character.
        /// </note>
        /// <para>
        ///   If you override the <see cref="UsageWriter"/> method, this method will not be called.
        /// </para>
        /// </remarks>
        public virtual string ValidatorDescriptions(CommandLineArgument argument)
        {
            var messages = argument.Validators
                .Select(v => v.GetUsageHelp(argument))
                .Where(h => !string.IsNullOrEmpty(h));

            var result = string.Join(" ", messages);
            if (result.Length > 0)
            {
                result = " " + result;
            }

            return result;
        }

        /// <summary>
        /// Gets the usage help for the <see cref="ValidateCountAttribute"/> class.
        /// </summary>
        /// <param name="attribute">The attribute instance.</param>
        /// <returns>The string.</returns>
        public virtual string ValidateCountUsageHelp(ValidateCountAttribute attribute)
        {
            if (attribute.Minimum <= 0)
            {
                return Format(Resources.ValidateCountUsageHelpMaxFormat, attribute.Maximum);
            }
            else if (attribute.Maximum == int.MaxValue)
            {
                return Format(Resources.ValidateCountUsageHelpMinFormat, attribute.Minimum);
            }

            return Format(Resources.ValidateCountUsageHelpBothFormat, attribute.Minimum, attribute.Maximum);
        }

        /// <summary>
        /// Gets the usage help for the <see cref="ValidateNotEmptyAttribute"/> class.
        /// </summary>
        /// <returns>The string.</returns>
        public virtual string ValidateNotEmptyUsageHelp()
            => Resources.ValidateNotEmptyUsageHelp;

        /// <summary>
        /// Gets the usage help for the <see cref="ValidateNotWhiteSpaceAttribute"/> class.
        /// </summary>
        /// <returns>The string.</returns>
        public virtual string ValidateNotWhiteSpaceUsageHelp()
            => Resources.ValidateNotWhiteSpaceUsageHelp;

        /// <summary>
        /// Gets the usage help for the <see cref="ValidateRangeAttribute"/> class.
        /// </summary>
        /// <param name="attribute">The attribute instance.</param>
        /// <returns>The string.</returns>
        public virtual string ValidateRangeUsageHelp(ValidateRangeAttribute attribute)
        {
            if (attribute.Minimum == null)
            {
                return Format(Resources.ValidateRangeUsageHelpMaxFormat, attribute.Maximum);
            }
            else if (attribute.Maximum == null)
            {
                return Format(Resources.ValidateRangeUsageHelpMinFormat, attribute.Minimum);
            }

            return Format(Resources.ValidateRangeUsageHelpBothFormat, attribute.Minimum, attribute.Maximum);
        }

        /// <summary>
        /// Gets the usage help for the <see cref="ValidateStringLengthAttribute"/> class.
        /// </summary>
        /// <param name="attribute">The attribute instance.</param>
        /// <returns>The string.</returns>
        public virtual string ValidateStringLengthUsageHelp(ValidateStringLengthAttribute attribute)
        {
            if (attribute.Minimum <= 0)
            {
                return Format(Resources.ValidateStringLengthUsageHelpMaxFormat, attribute.Maximum);
            }
            else if (attribute.Maximum == int.MaxValue)
            {
                return Format(Resources.ValidateStringLengthUsageHelpMinFormat, attribute.Minimum);
            }

            return Format(Resources.ValidateStringLengthUsageHelpBothFormat, attribute.Minimum, attribute.Maximum);
        }

        /// <summary>
        /// Gets the usage help for the <see cref="ValidateEnumValueAttribute"/> class.
        /// </summary>
        /// <param name="enumType">The enumeration type.</param>
        /// <returns>The string.</returns>
        public virtual string ValidateEnumValueUsageHelp(Type enumType)
            => Format(Resources.ValidateEnumValueUsageHelpFormat, string.Join(ArgumentSeparator, Enum.GetNames(enumType)));


        /// <summary>
        /// Gets the usage help for the <see cref="ProhibitsAttribute"/> class.
        /// </summary>
        /// <param name="arguments">The prohibited arguments.</param>
        /// <returns>The string.</returns>
        public virtual string ProhibitsUsageHelp(IEnumerable<CommandLineArgument> arguments)
            => Format(Resources.ValidateProhibitsUsageHelpFormat,
                   string.Join(ArgumentSeparator, arguments.Select(a => a.ArgumentNameWithPrefix)));

        /// <summary>
        /// Gets the usage help for the <see cref="RequiresAttribute"/> class.
        /// </summary>
        /// <param name="arguments">The required arguments.</param>
        /// <returns>The string.</returns>
        public virtual string RequiresUsageHelp(IEnumerable<CommandLineArgument> arguments)
            => Format(Resources.ValidateRequiresUsageHelpFormat,
                   string.Join(ArgumentSeparator, arguments.Select(a => a.ArgumentNameWithPrefix)));

        /// <summary>
        /// Gets an error message used if the <see cref="RequiresAnyAttribute"/> fails validation.
        /// </summary>
        /// <param name="arguments">The names of the arguments.</param>
        /// <returns>The error message.</returns>
        public virtual string RequiresAnyUsageHelp(IEnumerable<CommandLineArgument> arguments)
        {
            // This deliberately reuses the error messge.
            return Format(Resources.ValidateRequiresAnyFailedFormat,
                string.Join(ArgumentSeparator, arguments.Select(a => a.ArgumentNameWithPrefix)));
        }

        /// <summary>
        /// Gets a generic error message for the base implementation of <see cref="ArgumentValidationAttribute.GetErrorMessage"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidationFailed(string argumentName)
            => Format(Resources.ValidationFailedFormat, argumentName);

        /// <summary>
        /// Gets a generic error message for the base implementation of <see cref="ClassValidationAttribute.GetErrorMessage"/>.
        /// </summary>
        /// <returns>The error message.</returns>
        public virtual string ClassValidationFailed() => Resources.ClassValidationFailed;

        /// <summary>
        /// Gets an error message used if the <see cref="ValidateRangeAttribute"/> fails validation.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="attribute">The <see cref="ValidateRangeAttribute"/>.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidateRangeFailed(string argumentName, ValidateRangeAttribute attribute)
        {
            if (attribute.Maximum == null)
            {
                return Format(Resources.ValidateRangeFailedMinFormat, argumentName, attribute.Minimum);
            }
            else if (attribute.Minimum == null)
            {
                return Format(Resources.ValidateRangeFailedMaxFormat, argumentName, attribute.Maximum);
            }
            else
            {
                return Format(Resources.ValidateRangeFailedBothFormat, argumentName, attribute.Minimum, attribute.Maximum);
            }
        }

        /// <summary>
        /// Gets an error message used if the <see cref="ValidateNotEmptyAttribute"/> fails
        /// validation because the string was empty.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The error message.</returns>
        /// <remarks>
        /// <para>
        ///   If <see cref="ValidateNotEmptyAttribute"/> failed because the value was
        ///   <see langword="null"/>, the <see cref="NullArgumentValue"/> method is called instead.
        /// </para>
        /// </remarks>
        public virtual string ValidateNotEmptyFailed(string argumentName)
            => Format(Resources.ValidateNotEmptyFailedFormat, argumentName);

        /// <summary>
        /// Gets an error message used if the <see cref="ValidateNotWhiteSpaceAttribute"/> fails
        /// validation because the string was empty.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The error message.</returns>
        /// <remarks>
        /// <para>
        ///   If <see cref="ValidateNotWhiteSpaceAttribute"/> failed because the value was
        ///   <see langword="null"/>, the <see cref="NullArgumentValue"/> method is called instead.
        /// </para>
        /// </remarks>
        public virtual string ValidateNotWhiteSpaceFailed(string argumentName)
            => Format(Resources.ValidateNotWhiteSpaceFailedFormat, argumentName);

        /// <summary>
        /// Gets an error message used if the <see cref="ValidateStringLengthAttribute"/> fails validation.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="attribute">The <see cref="ValidateStringLengthAttribute"/>.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidateStringLengthFailed(string argumentName, ValidateStringLengthAttribute attribute)
        {
            if (attribute.Maximum == int.MaxValue)
            {
                return Format(Resources.ValidateStringLengthMinFormat, argumentName, attribute.Minimum);
            }
            else if (attribute.Minimum <= 0)
            {
                return Format(Resources.ValidateStringLengthMaxFormat, argumentName, attribute.Maximum);
            }
            else
            {
                return Format(Resources.ValidateStringLengthBothFormat, argumentName, attribute.Minimum, attribute.Maximum);
            }
        }

        /// <summary>
        /// Gets an error message used if the <see cref="ValidateCountAttribute"/> fails validation.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="attribute">The <see cref="ValidateCountAttribute"/>.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidateCountFailed(string argumentName, ValidateCountAttribute attribute)
        {
            if (attribute.Maximum == int.MaxValue)
            {
                return Format(Resources.ValidateCountMinFormat, argumentName, attribute.Minimum);
            }
            else if (attribute.Minimum <= 0)
            {
                return Format(Resources.ValidateCountMaxFormat, argumentName, attribute.Maximum);
            }
            else
            {
                return Format(Resources.ValidateCountBothFormat, argumentName, attribute.Minimum, attribute.Maximum);
            }
        }

        /// <summary>
        /// Gets an error message used if the <see cref="ValidateEnumValueAttribute"/> fails validation.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="enumType">The type of the enumeration.</param>
        /// <param name="value">The value of the argument.</param>
        /// <param name="includeValues">
        ///   <see langword="true"/> to include the possible values of the enumeration in the error
        ///   message; otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>The error message.</returns>
        public virtual string ValidateEnumValueFailed(string argumentName, Type enumType, object? value, bool includeValues)
        {
            return includeValues
                ? Format(Resources.ValidateEnumValueFailedWithValuesFormat, argumentName, string.Join(ArgumentSeparator,
                    Enum.GetNames(enumType)))
                : Format(Resources.ValidateEnumValueFailedFormat, value, argumentName);
        }

        /// <summary>
        /// Gets an error message used if the <see cref="RequiresAttribute"/> fails validation.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="dependencies">The names of the required arguments.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidateRequiresFailed(string argumentName, IEnumerable<CommandLineArgument> dependencies)
            => Format(Resources.ValidateRequiresFailedFormat, argumentName,
                   string.Join(ArgumentSeparator, dependencies.Select(a => a.ArgumentNameWithPrefix)));

        /// <summary>
        /// Gets an error message used if the <see cref="ProhibitsAttribute"/> fails validation.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="prohibitedArguments">The names of the prohibited arguments.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidateProhibitsFailed(string argumentName, IEnumerable<CommandLineArgument> prohibitedArguments)
            => Format(Resources.ValidateProhibitsFailedFormat, argumentName,
                   string.Join(ArgumentSeparator, prohibitedArguments.Select(a => a.ArgumentNameWithPrefix)));

        /// <summary>
        /// Gets an error message used if the <see cref="RequiresAnyAttribute"/> fails validation.
        /// </summary>
        /// <param name="arguments">The names of the arguments.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidateRequiresAnyFailed(IEnumerable<CommandLineArgument> arguments)
            => Format(Resources.ValidateRequiresAnyFailedFormat,
                   string.Join(ArgumentSeparator, arguments.Select(a => a.ArgumentNameWithPrefix)));
    }
}
