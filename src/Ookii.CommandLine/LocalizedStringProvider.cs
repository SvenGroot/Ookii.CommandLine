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
        private const string ArgumentSeparator = ", ";

        #region Error messages

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

        #endregion

        #region Usage syntax

        /// <summary>
        /// Gets a string used at the start of the usage syntax, similar to "Usage: executable".
        /// </summary>
        /// <param name="executableName">The value of <see cref="WriteUsageOptions.ExecutableName"/>.</param>
        /// <param name="color">
        ///   The value of <see cref="WriteUsageOptions.UsagePrefixColor"/>, or an empty string
        ///   if <see cref="WriteUsageOptions.UseColor"/> is <see langword="false"/>.
        /// </param>
        /// <param name="colorReset">
        ///   The value of <see cref="WriteUsageOptions.ColorReset"/>, or an empty string if
        ///   <see cref="WriteUsageOptions.UseColor"/> is <see langword="false"/>.
        /// </param>
        /// <returns>The string.</returns>
        public virtual string UsagePrefix(string executableName, string color, string colorReset)
            => $"{color}{Resources.DefaultUsagePrefix}{colorReset} {executableName}";

        /// <summary>
        /// Gets a string used at the start of the usage syntax for a <see cref="ShellCommand"/>,
        /// similar to "Usage: executable command_name".
        /// </summary>
        /// <param name="executableName">The value of <see cref="WriteUsageOptions.ExecutableName"/>.</param>
        /// <param name="commandName">The name of the shell command.</param>
        /// <param name="color">
        ///   The value of <see cref="WriteUsageOptions.UsagePrefixColor"/>, or an empty string
        ///   if <see cref="WriteUsageOptions.UseColor"/> is <see langword="false"/>.
        /// </param>
        /// <param name="colorReset">
        ///   The value of <see cref="WriteUsageOptions.ColorReset"/>, or an empty string if
        ///   <see cref="WriteUsageOptions.UseColor"/> is <see langword="false"/>.
        /// </param>
        /// <returns>The string.</returns>
        public virtual string CommandUsagePrefix(string executableName, string commandName, string color, string colorReset)
            => $"{color}{Resources.DefaultUsagePrefix}{colorReset} {executableName} {commandName}";

        /// <summary>
        /// Gets a formatted value description, similar to "&lt;value&gt;".
        /// </summary>
        /// <param name="valueDescription">The argument's value description.</param>
        /// <param name="useColor">The value of <see cref="WriteUsageOptions.UseColor"/>.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so the <paramref name="useColor"/> parameter is provided to allow you to
        ///   manually add colors if desired.
        /// </para>
        /// <para>
        ///   If you override the <see cref="ArgumentSyntax"/> method and the <see cref="ArgumentDescription"/>
        ///   method, this method will not be called.
        /// </para>
        /// </remarks>
        public virtual string ValueDescription(string valueDescription, bool useColor)
            => $"<{valueDescription}>";

        /// <summary>
        /// Gets a formatted argument name, similar to "-Name".
        /// </summary>
        /// <param name="argumentName">
        ///   The name of the argument.  This will either be long or the short name, depending on
        ///   <see cref="CommandLineParser.Mode"/> and the value of
        ///   <see cref="WriteUsageOptions.UseShortNamesForSyntax"/>.
        /// </param>
        /// <param name="prefix">
        ///   The argument name prefix. This will either be first element of the <see cref="CommandLineParser.ArgumentNamePrefixes"/>
        ///   property, or the value of the <see cref="CommandLineParser.LongArgumentNamePrefix"/>
        ///   property, depending on <see cref="CommandLineParser.Mode"/> and the value of
        ///   <see cref="WriteUsageOptions.UseShortNamesForSyntax"/>.
        /// </param>
        /// <param name="useColor">The value of <see cref="WriteUsageOptions.UseColor"/>.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so the <paramref name="useColor"/> parameter is provided to allow you to
        ///   manually add colors if desired.
        /// </para>
        /// <para>
        ///   If you override the <see cref="ArgumentSyntax"/> method and the <see cref="ArgumentDescription"/>
        ///   method, this method will not be called.
        /// </para>
        /// </remarks>
        public virtual string ArgumentName(string argumentName, string prefix, bool useColor) => prefix + argumentName;

        /// <summary>
        /// Gets a formatted optional argument name, similar to "[-Name]".
        /// </summary>
        /// <param name="argumentName">
        ///   The name of the argument.  This will either be long or the short name, depending on
        ///   <see cref="CommandLineParser.Mode"/> and the value of
        ///   <see cref="WriteUsageOptions.UseShortNamesForSyntax"/>.
        /// </param>
        /// <param name="prefix">
        ///   The argument name prefix. This will either be first element of the <see cref="CommandLineParser.ArgumentNamePrefixes"/>
        ///   property, or the value of the <see cref="CommandLineParser.LongArgumentNamePrefix"/>
        ///   property, depending on <see cref="CommandLineParser.Mode"/> and the value of
        ///   <see cref="WriteUsageOptions.UseShortNamesForSyntax"/>.
        /// </param>
        /// <param name="useColor">The value of <see cref="WriteUsageOptions.UseColor"/>.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so the <paramref name="useColor"/> parameter is provided to allow you to
        ///   manually add colors if desired.
        /// </para>
        /// <para>
        ///   If you override the <see cref="ArgumentSyntax"/> method, this method will not be
        ///   called.
        /// </para>
        /// </remarks>
        public virtual string OptionalArgumentName(string argumentName, string prefix, bool useColor)
            => $"[{ArgumentName(argumentName, prefix, useColor)}]";

        /// <summary>
        /// Gets a suffix to add to multi-value arguments, similar to "...".
        /// </summary>
        /// <param name="useColor">The value of <see cref="WriteUsageOptions.UseColor"/>.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so the <paramref name="useColor"/> parameter is provided to allow you to
        ///   manually add colors if desired.
        /// </para>
        /// <para>
        ///   If you override the <see cref="ArgumentSyntax"/> method, this method will not be
        ///   called.
        /// </para>
        /// </remarks>
        public virtual string MultiValueSuffix(bool useColor) => Resources.DefaultArraySuffix;

        /// <summary>
        /// Gets the usage syntax for an argument, similar to "-Argument &lt;Value&gt;".
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="options">The options for formatting usage help.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   The default implementation calls the <see cref="ArgumentName(string, string, bool)"/>,
        ///   <see cref="OptionalArgumentName(string, string, bool)"/>,
        ///   <see cref="ValueDescription(string, bool)"/> and <see cref="MultiValueSuffix(bool)"/>
        ///   methods, so you do not need to override this method if you only want to customize
        ///   those elements.
        /// </para>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so check the <see cref="WriteUsageOptions.UseColor"/> property to see if
        ///   manually adding colors is allowed.
        /// </para>
        /// <para>
        ///   If you override the <see cref="OptionalArgumentSyntax"/> method, this method will
        ///   not be called.
        /// </para>
        /// </remarks>
        public virtual string ArgumentSyntax(CommandLineArgument argument, WriteUsageOptions options)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string argumentName;
            if (argument.HasShortName && options.UseShortNamesForSyntax)
                argumentName = argument.ShortName.ToString();
            else
                argumentName = argument.ArgumentName;

            var prefix = argument.Parser.Mode != ParsingMode.LongShort || (argument.HasShortName && (options.UseShortNamesForSyntax || !argument.HasLongName))
                ? argument.Parser.ArgumentNamePrefixes[0]
                : argument.Parser.LongArgumentNamePrefix!;

            var separator = argument.Parser.AllowWhiteSpaceValueSeparator && options.UseWhiteSpaceValueSeparator
                ? ' '
                : argument.Parser.NameValueSeparator;

            bool useColor = options.UseColor ?? false;
            if (argument.Position == null)
                argumentName = ArgumentName(argumentName, prefix, useColor);
            else
                argumentName = OptionalArgumentName(argumentName, prefix, useColor);

            var result = argumentName;
            if (!argument.IsSwitch)
            {
                string argumentValue = ValueDescription(argument.ValueDescription, useColor);
                result = argumentName + separator + argumentValue;
            }

            if (argument.IsMultiValue)
                result += MultiValueSuffix(useColor);

            return result;
        }

        /// <summary>
        /// Gets the usage syntax for an optional argument, similar to "[-Argument &lt;Value&gt;]".
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="options">The options for formatting usage help.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   The default implementation calls the <see cref="ArgumentName(string, string, bool)"/>,
        ///   <see cref="OptionalArgumentName(string, string, bool)"/>,
        ///   <see cref="ValueDescription(string, bool)"/> and <see cref="MultiValueSuffix(bool)"/>
        ///   methods, so you do not need to override this method if you only want to customize
        ///   those elements.
        /// </para>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so check the <see cref="WriteUsageOptions.UseColor"/> property to see if
        ///   manually adding colors is allowed.
        /// </para>
        /// </remarks>
        public virtual string OptionalArgumentSyntax(CommandLineArgument argument, WriteUsageOptions options)
            => $"[{ArgumentSyntax(argument, options)}]";

        /// <summary>
        /// Gets a string to indicate there are more arguments when using abbreviated usage syntax,
        /// similar to "[arguments]".
        /// </summary>
        /// <param name="useColor">The value of <see cref="WriteUsageOptions.UseColor"/>.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so the <paramref name="useColor"/> parameter is provided to allow you to
        ///   manually add colors if desired.
        /// </para>
        /// <para>
        ///   This method is only used if <see cref="WriteUsageOptions.UseAbbreviatedSyntax"/> is
        ///   <see langword="true"/>.
        /// </para>
        /// </remarks>
        public virtual string AbbreviatedRemainingArguments(bool useColor) => Resources.DefaultAbbreviatedRemainingArguments;

        #endregion

        #region Usage descriptions

        /// <summary>
        /// Gets a formatted default, similar to " Default value: value".
        /// </summary>
        /// <param name="defaultValue">The argument's default value.</param>
        /// <param name="useColor">The value of <see cref="WriteUsageOptions.UseColor"/>.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <note>
        ///   The default implementation of <see cref="ArgumentDescription"/> expects the returned
        ///   value to start with a white-space character.
        /// </note>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so the <paramref name="useColor"/> parameter is provided to allow you to
        ///   manually add colors if desired.
        /// </para>
        /// <para>
        ///   If you override the <see cref="ArgumentDescription"/> method, this method will not be
        ///   called.
        /// </para>
        /// </remarks>
        public virtual string DefaultValue(object defaultValue, bool useColor)
            => Format(Resources.DefaultDefaultValueFormat, defaultValue);

        /// <summary>
        /// Gets a formatted default, similar to " Default value: value".
        /// </summary>
        /// <param name="aliases">
        ///   The argument's aliases, or long aliases if using <see cref="ParsingMode.LongShort"/>.
        /// </param>
        /// <param name="shortAliases">The argument's short aliases.</param>
        /// <param name="prefix">
        ///   The default argument name prefix, or the long argument name prefix is using
        ///   <see cref="ParsingMode.LongShort"/>.
        /// </param>
        /// <param name="shortPrefix">The short argument name prefix.</param>
        /// <param name="useColor">The value of <see cref="WriteUsageOptions.UseColor"/>.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <note>
        ///   The default implementation of <see cref="ArgumentDescription"/> expects the returned
        ///   value to start with a white-space character.
        /// </note>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so the <paramref name="useColor"/> parameter is provided to allow you to
        ///   manually add colors if desired.
        /// </para>
        /// <para>
        ///   If you override the <see cref="ArgumentDescription"/> method, this method will not be
        ///   called.
        /// </para>
        /// </remarks>
        public virtual string Aliases(IEnumerable<string>? aliases, IEnumerable<char>? shortAliases, string prefix, string shortPrefix, bool useColor)
        {
            if (shortAliases == null && aliases == null)
                return string.Empty;

            var result = new StringBuilder();
            var count = AppendAliases(result, shortPrefix, shortAliases, 0);
            count = AppendAliases(result, prefix, aliases, count);

            if (count == 0)
                return string.Empty;

            return $" ({result})";
        }

        /// <summary>
        /// Gets a formatted optional value description, similar to "[&lt;value&gt;]".
        /// </summary>
        /// <param name="valueDescription">The argument's value description.</param>
        /// <param name="useColor">The value of <see cref="WriteUsageOptions.UseColor"/>.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   This string is used for switch arguments in the argument description list.
        /// </para>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so the <paramref name="useColor"/> parameter is provided to allow you to
        ///   manually add colors if desired.
        /// </para>
        /// <para>
        ///   If you override the <see cref="ArgumentDescription"/> method, this method will not be called.
        /// </para>
        /// </remarks>
        public virtual string OptionalValueDescription(string valueDescription, bool useColor)
            => $"[{ValueDescription(valueDescription, useColor)}]";

        /// <summary>
        /// Gets the usage description for an argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="options">The options for formatting usage help.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   The default implementation calls the <see cref="ArgumentName(string, string, bool)"/>,
        ///   <see cref="ValueDescription(string, bool)"/>,
        ///   <see cref="OptionalValueDescription(string, bool)"/>, <see cref="DefaultValue(object, bool)"/>
        ///   and <see cref="Aliases(IEnumerable{string}?, IEnumerable{char}?, string, string, bool)"/>
        ///   methods, so you do not need to override this method if you only want to customize
        ///   those elements.
        /// </para>
        /// <para>
        ///   If you override this function, you may also need to change the <see cref="WriteUsageOptions.ArgumentDescriptionIndent"/>
        ///   or the <see cref="WriteUsageOptions.LongShortArgumentDescriptionIndent"/> property to
        ///   a value suitable for your description format.
        /// </para>
        /// </remarks>
        public virtual string ArgumentDescription(CommandLineArgument argument, WriteUsageOptions options)
        {
            bool useColor = options.UseColor ?? false;
            string colorStart = string.Empty;
            string colorEnd = string.Empty;
            if (useColor)
            {
                colorStart = options.ArgumentDescriptionColor;
                colorEnd = options.ColorReset;
            }

            string valueDescription = argument.IsSwitch
                ? OptionalValueDescription(argument.ValueDescription, useColor)
                : ValueDescription(argument.ValueDescription, useColor);

            string defaultValue = options.IncludeDefaultValueInDescription && argument.DefaultValue != null 
                ? DefaultValue(argument.DefaultValue, useColor)
                : string.Empty;

            var shortPrefix = argument.Parser.ArgumentNamePrefixes[0];
            var prefix = argument.Parser.LongArgumentNamePrefix ?? shortPrefix;
            string alias = options.IncludeAliasInDescription
                ? Aliases(argument.Aliases, argument.ShortAliases, prefix, shortPrefix, useColor)
                : string.Empty;

            if (argument.Parser.Mode == ParsingMode.LongShort)
            {
                var shortName = argument.HasShortName
                    ? ArgumentName(argument.ShortName.ToString(), shortPrefix, useColor)
                    : new string(' ', shortPrefix.Length + 3);

                var longName = argument.HasLongName ? ArgumentName(argument.ArgumentName, prefix, useColor) : string.Empty;
                var separator = argument.HasShortName && argument.HasLongName ? ArgumentSeparator : string.Empty;
                return $"    {colorStart}{shortName}{separator}{longName} {valueDescription}{alias}{colorEnd}{Environment.NewLine}{argument.Description}{defaultValue}{Environment.NewLine}";
            }
            else
            {
                var name = ArgumentName(argument.ArgumentName, prefix, useColor);
                return $"    {colorStart}{name} {valueDescription}{alias}{colorEnd}{Environment.NewLine}{argument.Description}{defaultValue}{Environment.NewLine}";
            }
        }

        #endregion

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

        private static string Format(string format, object? arg0)
            => string.Format(CultureInfo.CurrentCulture, format, arg0);

        private static string Format(string format, object? arg0, object? arg1)
            => string.Format(CultureInfo.CurrentCulture, format, arg0, arg1);

        private static string Format(string format, object? arg0, object? arg1, object? arg2)
            => string.Format(CultureInfo.CurrentCulture, format, arg0, arg1, arg2);

        private static string Format(string format, params object?[] args)
            => string.Format(CultureInfo.CurrentCulture, format, args);

        private static int AppendAliases<T>(StringBuilder builder, string prefix, IEnumerable<T>? aliases, int count)
        {
            if (aliases == null)
                return count;

            foreach (var alias in aliases)
            {
                if (count != 0)
                    builder.Append(ArgumentSeparator);

                builder.Append(prefix);
                builder.Append(alias);
                ++count;
            }

            return count;
        }
    }
}
