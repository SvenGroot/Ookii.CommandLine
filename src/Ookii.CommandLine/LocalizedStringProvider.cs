using Ookii.CommandLine.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides localized strings for error messages and usage help.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Inherit from this class and override its members to provide customized or localized
    ///   strings. You can specify the implementation to use using <see cref="ParseOptions.StringProvider"/>.
    /// </para>
    /// <note>
    ///   This only lets you customize error messages for the <see cref="CommandLineArgumentException"/>
    ///   class. Other exceptions thrown by this library, such as for invalid argument definitions,
    ///   constitute bugs and should not occur in a correct program, and should therefore not be
    ///   shown to the user.
    /// </note>
    /// </remarks>
    public class LocalizedStringProvider
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
        public virtual string ArgumentValueConversionError(string argumentName, string? argumentValue, string valueDescription) =>
            Format(Resources.ArgumentConversionErrorFormat, argumentValue, argumentName, valueDescription);

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
        public virtual string MissingNamedArgumentValue(string argumentName) =>
            Format(Resources.MissingValueForNamedArgumentFormat, argumentName);

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
        public virtual string MissingRequiredArgument(string argumentName) =>
            Format(Resources.MissingRequiredArgumentFormat, argumentName);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.InvalidDictionaryValue"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="argumentValue">The value of the argument.</param>
        /// <param name="message">The error message of the conversion.</param>
        /// <returns>The error message.</returns>
        public virtual string InvalidDictionaryValue(string argumentName, string? argumentValue, string? message) =>
            Format(Resources.InvalidDictionaryValueFormat, argumentName, argumentValue, message);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.CreateArgumentsTypeError"/>.
        /// </summary>
        /// <param name="message">The error message of the conversion.</param>
        /// <returns>The error message.</returns>
        public virtual string CreateArgumentsTypeError(string? message) =>
            Format(Resources.CreateArgumentsTypeErrorFormat, message);

        /// <summary>
        /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.ApplyValueError"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="message">The error message of the conversion.</param>
        /// <returns>The error message.</returns>
        public virtual string ApplyValueError(string argumentName, string? message) =>
            Format(Resources.SetValueErrorFormat, argumentName, message);

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
        public virtual string CombinedShortNameNonSwitch(string argumentName) =>
            Format(Resources.CombinedShortNameNonSwitchFormat, argumentName);

        /// <summary>
        /// Gets the error message used if the <see cref="KeyValuePairConverter{TKey, TValue}"/>
        /// is unable to find the key/value pair separator in the argument value.
        /// </summary>
        /// <param name="separator">The key/value pair separator.</param>
        /// <returns>The error message.</returns>
        public virtual string MissingKeyValuePairSeparator(string separator) =>
            Format(Resources.NoKeyValuePairSeparatorFormat, separator);

        private CommandLineArgumentException CreateException(CommandLineArgumentErrorCategory category, Exception? inner, CommandLineArgument? argument = null, string? argumentName = null, string? value = null)
        {
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

        internal CommandLineArgumentException CreateException(CommandLineArgumentErrorCategory category, Exception? inner, CommandLineArgument argument, string? value = null) =>
            CreateException(category, inner, argument, argument.ArgumentName, value);

        internal CommandLineArgumentException CreateException(CommandLineArgumentErrorCategory category, Exception? inner, string? argumentName = null, string? value = null) =>
            CreateException(category, inner, null, argumentName, value);

        internal CommandLineArgumentException CreateException(CommandLineArgumentErrorCategory category, CommandLineArgument argument, string? value = null) =>
            CreateException(category, null, argument, value);

        internal CommandLineArgumentException CreateException(CommandLineArgumentErrorCategory category, string? argumentName = null, string? value = null) =>
            CreateException(category, null, argumentName, value);

        private static string Format(string format, object? arg0) =>
            string.Format(CultureInfo.CurrentCulture, format, arg0);

        private static string Format(string format, object? arg0, object? arg1) =>
            string.Format(CultureInfo.CurrentCulture, format, arg0, arg1);

        private static string Format(string format, object? arg0, object? arg1, object? arg2) =>
            string.Format(CultureInfo.CurrentCulture, format, arg0, arg1, arg2);

        private static string Format(string format, params object?[] args) =>
            string.Format(CultureInfo.CurrentCulture, format, args);
    }
}
