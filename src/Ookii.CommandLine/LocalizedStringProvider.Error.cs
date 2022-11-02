using Ookii.CommandLine.Properties;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    public partial class LocalizedStringProvider
    {
        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.Unspecified"/>.
        /// </summary>
        /// <returns>The error message.</returns>
        /// <remarks>
        /// <para>
        ///   Ookii.CommandLine never creates exceptions with this category, so this should not
        ///   normally be called.
        /// </para>
        /// </remarks>
        public virtual string UnspecifiedError() => Resources.UnspecifiedError;

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.ArgumentValueConversion"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="argumentValue">The value of the argument.</param>
        /// <param name="valueDescription">The value description of the argument.</param>
        /// <returns>The error message.</returns>
        public virtual string ArgumentValueConversionError(string argumentName, string? argumentValue, string valueDescription)
            => Format(Resources.ArgumentConversionErrorFormat, argumentValue, argumentName, valueDescription);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.UnknownArgument"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The error message.</returns>
        public virtual string UnknownArgument(string argumentName) => Format(Resources.UnknownArgumentFormat, argumentName);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.MissingNamedArgumentValue"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The error message.</returns>
        public virtual string MissingNamedArgumentValue(string argumentName)
            => Format(Resources.MissingValueForNamedArgumentFormat, argumentName);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.DuplicateArgument"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The error message.</returns>
        public virtual string DuplicateArgument(string argumentName) => Format(Resources.DuplicateArgumentFormat, argumentName);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.TooManyArguments"/>.
        /// </summary>
        /// <returns>The error message.</returns>
        public virtual string TooManyArguments() => Resources.TooManyArguments;

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.MissingRequiredArgument"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The error message.</returns>
        public virtual string MissingRequiredArgument(string argumentName)
            => Format(Resources.MissingRequiredArgumentFormat, argumentName);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.InvalidDictionaryValue"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="argumentValue">The value of the argument.</param>
        /// <param name="message">The error message of the conversion.</param>
        /// <returns>The error message.</returns>
        public virtual string InvalidDictionaryValue(string argumentName, string? argumentValue, string? message)
            => Format(Resources.InvalidDictionaryValueFormat, argumentName, argumentValue, message);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.CreateArgumentsTypeError"/>.
        /// </summary>
        /// <param name="message">The error message of the conversion.</param>
        /// <returns>The error message.</returns>
        public virtual string CreateArgumentsTypeError(string? message)
            => Format(Resources.CreateArgumentsTypeErrorFormat, message);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.ApplyValueError"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="message">The error message of the conversion.</param>
        /// <returns>The error message.</returns>
        public virtual string ApplyValueError(string argumentName, string? message)
            => Format(Resources.SetValueErrorFormat, argumentName, message);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.NullArgumentValue"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The error message.</returns>
        public virtual string NullArgumentValue(string argumentName) => Format(Resources.NullArgumentValueFormat, argumentName);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.CombinedShortNameNonSwitch"/>.
        /// </summary>
        /// <param name="argumentName">The names of the combined short arguments.</param>
        /// <returns>The error message.</returns>
        public virtual string CombinedShortNameNonSwitch(string argumentName)
            => Format(Resources.CombinedShortNameNonSwitchFormat, argumentName);

        /// <summary>
        /// Gets the error message used if the <see cref="KeyValuePairConverter{TKey, TValue}"/>
        /// is unable to find the key/value pair separator in the argument value.
        /// </summary>
        /// <param name="separator">The key/value pair separator.</param>
        /// <returns>The error message.</returns>
        public virtual string MissingKeyValuePairSeparator(string separator)
            => Format(Resources.NoKeyValuePairSeparatorFormat, separator);

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
                return Format(Resources.ValidateRangeFailedMinFormat, argumentName, attribute.Minimum);
            else if (attribute.Minimum == null)
                return Format(Resources.ValidateRangeFailedMaxFormat, argumentName, attribute.Maximum);
            else
                return Format(Resources.ValidateRangeFailedBothFormat, argumentName, attribute.Minimum, attribute.Maximum);
        }

        /// <summary>
        /// Gets an error message used if the <see cref="ValidateNotNullOrEmptyAttribute"/> fails
        /// validation because the string was empty.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The error message.</returns>
        /// <remarks>
        /// <para>
        ///   If <see cref="ValidateNotNullOrEmptyAttribute"/> failed because the value was
        ///   <see langword="null"/>, the <see cref="NullArgumentValue"/> method is called instead.
        /// </para>
        /// </remarks>
        public virtual string ValidateNotEmptyFailed(string argumentName)
            => Format(Resources.ValidateEmptyFailedFormat, argumentName);

        /// <summary>
        /// Gets an error message used if the <see cref="ValidateNotNullOrWhiteSpaceAttribute"/> fails
        /// validation because the string was empty.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The error message.</returns>
        /// <remarks>
        /// <para>
        ///   If <see cref="ValidateNotNullOrWhiteSpaceAttribute"/> failed because the value was
        ///   <see langword="null"/>, the <see cref="NullArgumentValue"/> method is called instead.
        /// </para>
        /// </remarks>
        public virtual string ValidateNotWhiteSpaceFailed(string argumentName)
            => Format(Resources.ValidateWhiteSpaceFailedFormat, argumentName);

        /// <summary>
        /// Gets an error message used if the <see cref="ValidateStringLengthAttribute"/> fails validation.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="attribute">The <see cref="ValidateStringLengthAttribute"/>.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidateStringLengthFailed(string argumentName, ValidateStringLengthAttribute attribute)
        {
            if (attribute.MaximumLength == int.MaxValue)
                return Format(Resources.ValidateRangeFailedMinFormat, argumentName, attribute.MinimumLength);
            else if (attribute.MinimumLength <= 0)
                return Format(Resources.ValidateRangeFailedMaxFormat, argumentName, attribute.MaximumLength);
            else
                return Format(Resources.ValidateRangeFailedBothFormat, argumentName, attribute.MinimumLength, attribute.MaximumLength);
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
                return Format(Resources.ValidateCountMinFormat, argumentName, attribute.Minimum);
            else if (attribute.Minimum <= 0)
                return Format(Resources.ValidateCountMaxFormat, argumentName, attribute.Maximum);
            else
                return Format(Resources.ValidateCountBothFormat, argumentName, attribute.Minimum, attribute.Maximum);
        }

        /// <summary>
        /// Gets an error message used if the <see cref="RequiresAttribute"/> fails validation.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="dependencies">The names of the required arguments.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidateRequiresFailed(string argumentName, IEnumerable<string> dependencies)
            => Format(Resources.ValidateRequiresFailedFormat, argumentName, string.Join(ArgumentSeparator, dependencies));

        /// <summary>
        /// Gets an error message used if the <see cref="ProhibitsAttribute"/> fails validation.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="prohibitedArguments">The names of the prohibited arguments.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidateProhibitsFailed(string argumentName, IEnumerable<string> prohibitedArguments)
            => Format(Resources.ValidateProhibitsFailedFormat, argumentName, string.Join(ArgumentSeparator, prohibitedArguments));

        /// <summary>
        /// Gets an error message used if the <see cref="RequiresAnyAttribute"/> fails validation.
        /// </summary>
        /// <param name="arguments">The names of the arguments.</param>
        /// <returns>The error message.</returns>
        public virtual string ValidateRequiresAnyFailed(IEnumerable<string> arguments)
            => Format(Resources.ValidateRequiresAnyFailedFormat, string.Join(ArgumentSeparator, arguments));

        internal CommandLineArgumentException CreateException(CommandLineArgumentErrorCategory category, Exception? inner, CommandLineArgument argument, string? value = null)
            => CreateException(category, inner, argument, argument.ArgumentName, value);

        internal CommandLineArgumentException CreateException(CommandLineArgumentErrorCategory category, Exception? inner, string? argumentName = null, string? value = null)
            => CreateException(category, inner, null, argumentName, value);

        internal CommandLineArgumentException CreateException(CommandLineArgumentErrorCategory category, CommandLineArgument argument, string? value = null)
            => CreateException(category, null, argument, value);

        internal CommandLineArgumentException CreateException(CommandLineArgumentErrorCategory category, string? argumentName = null, string? value = null)
            => CreateException(category, null, argumentName, value);

        private CommandLineArgumentException CreateException(CommandLineArgumentErrorCategory category, Exception? inner, CommandLineArgument? argument = null, string? argumentName = null, string? value = null)
        {
            // These are not created using the helper, because there is not one standard message.
            Debug.Assert(category != CommandLineArgumentErrorCategory.ValidationFailed);

            var message = category switch
            {
                CommandLineArgumentErrorCategory.MissingRequiredArgument => MissingRequiredArgument(argumentName!),
                CommandLineArgumentErrorCategory.ArgumentValueConversion => ArgumentValueConversionError(argumentName!, value, argument!.ValueDescription),
                CommandLineArgumentErrorCategory.UnknownArgument => UnknownArgument(argumentName!),
                CommandLineArgumentErrorCategory.MissingNamedArgumentValue => MissingNamedArgumentValue(argumentName!),
                CommandLineArgumentErrorCategory.DuplicateArgument => DuplicateArgument(argumentName!),
                CommandLineArgumentErrorCategory.TooManyArguments => TooManyArguments(),
                CommandLineArgumentErrorCategory.InvalidDictionaryValue => InvalidDictionaryValue(argumentName!, value, inner?.Message),
                CommandLineArgumentErrorCategory.CreateArgumentsTypeError => CreateArgumentsTypeError(inner?.Message),
                CommandLineArgumentErrorCategory.ApplyValueError => ApplyValueError(argumentName!, inner?.Message),
                CommandLineArgumentErrorCategory.NullArgumentValue => NullArgumentValue(argumentName!),
                CommandLineArgumentErrorCategory.CombinedShortNameNonSwitch => CombinedShortNameNonSwitch(argumentName!),
                _ => UnspecifiedError(),
            };

            return new CommandLineArgumentException(message, argumentName, category, inner);
        }
    }
}
