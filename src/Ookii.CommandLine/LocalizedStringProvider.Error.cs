using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ookii.CommandLine;

public partial class LocalizedStringProvider
{
    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.Unspecified" qualifyHint="true"/>.
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
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.ArgumentValueConversion" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <param name="argumentValue">The value of the argument.</param>
    /// <param name="valueDescription">The value description of the argument.</param>
    /// <returns>The error message.</returns>
    public virtual string ArgumentValueConversionError(string argumentName, string? argumentValue, string valueDescription)
        => Format(Resources.ArgumentConversionErrorFormat, argumentValue, argumentName, valueDescription);

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.UnknownArgument" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <returns>The error message.</returns>
    public virtual string UnknownArgument(string argumentName) => Format(Resources.UnknownArgumentFormat, argumentName);

    /// <summary>
    /// Gets the error message displayed when the user tries to invoke an unknown command.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <returns>The error message.</returns>
    public virtual string UnknownCommand(string commandName) => Format(Resources.UnknownCommandFormat, commandName);

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.MissingNamedArgumentValue" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <returns>The error message.</returns>
    public virtual string MissingNamedArgumentValue(string argumentName)
        => Format(Resources.MissingValueForNamedArgumentFormat, argumentName);

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.DuplicateArgument" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <returns>The error message.</returns>
    public virtual string DuplicateArgument(string argumentName) => Format(Resources.DuplicateArgumentFormat, argumentName);

    /// <summary>
    /// Gets the warning message used if the <see cref="ParseOptionsAttribute.DuplicateArguments" qualifyHint="true"/>
    /// or <see cref="ParseOptions.DuplicateArguments" qualifyHint="true"/> property is <see cref="ErrorMode.Warning" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <returns>The warning message.</returns>
    public virtual string DuplicateArgumentWarning(string argumentName) => Format(Resources.DuplicateArgumentWarningFormat, argumentName);

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.TooManyArguments" qualifyHint="true"/>.
    /// </summary>
    /// <returns>The error message.</returns>
    public virtual string TooManyArguments() => Resources.TooManyArguments;

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.MissingRequiredArgument" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <returns>The error message.</returns>
    public virtual string MissingRequiredArgument(string argumentName)
        => Format(Resources.MissingRequiredArgumentFormat, argumentName);

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.InvalidDictionaryValue" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <param name="argumentValue">The value of the argument.</param>
    /// <param name="message">The error message of the exception that caused this error.</param>
    /// <returns>The error message.</returns>
    public virtual string InvalidDictionaryValue(string argumentName, string? argumentValue, string? message)
        => Format(Resources.InvalidDictionaryValueFormat, argumentName, argumentValue, message);

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.CreateArgumentsTypeError" qualifyHint="true"/>.
    /// </summary>
    /// <param name="message">The error message from instantiating the type.</param>
    /// <returns>The error message.</returns>
    public virtual string CreateArgumentsTypeError(string? message)
        => Format(Resources.CreateArgumentsTypeErrorFormat, message);

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.ApplyValueError" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <param name="message">The error message from setting the value.</param>
    /// <returns>The error message.</returns>
    public virtual string ApplyValueError(string argumentName, string? message)
        => Format(Resources.SetValueErrorFormat, argumentName, message);

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.NullArgumentValue" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <returns>The error message.</returns>
    public virtual string NullArgumentValue(string argumentName) => Format(Resources.NullArgumentValueFormat, argumentName);

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.CombinedShortNameNonSwitch" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The names of the combined short arguments.</param>
    /// <returns>The error message.</returns>
    public virtual string CombinedShortNameNonSwitch(string argumentName)
        => Format(Resources.CombinedShortNameNonSwitchFormat, argumentName);

    /// <summary>
    /// Gets the error message for <see cref="CommandLineArgumentErrorCategory.AmbiguousPrefixAlias" qualifyHint="true"/>.
    /// </summary>
    /// <param name="argumentName">The argument name that is the ambiguous prefix.</param>
    /// <param name="prefix">
    /// The argument name prefix to use with <paramref name="possibleMatches"/>. This is either the
    /// long name prefix or the first regular prefix.
    /// </param>
    /// <param name="possibleMatches">A list of argument names and aliases that the prefix could match.</param>
    /// <returns>The error message.</returns>
    public virtual string AmbiguousArgumentPrefixAlias(string argumentName, string prefix, IEnumerable<string> possibleMatches)
        => Format(Resources.AmbiguousArgumentPrefixExceptionMessageFormat, argumentName,
            string.Join(ArgumentSeparator, possibleMatches.Select(m => prefix + m)));

    /// <summary>
    /// Gets the error message for an ambiguous prefix alias, without the possible matches.
    /// </summary>
    /// <param name="argumentName">The argument name that is the ambiguous prefix.</param>
    /// <returns>The error message.</returns>
    public virtual string AmbiguousArgumentPrefixAliasErrorOnly(string argumentName)
        => Format(Resources.AmbiguousArgumentPrefixErrorOnlyFormat, argumentName);

    /// <summary>
    /// Gets the error message for an ambiguous prefix alias of a subcommand.
    /// </summary>
    /// <param name="argumentName">The command name that is the ambiguous prefix.</param>
    /// <returns>The error message.</returns>
    public virtual string AmbiguousCommandPrefixAlias(string argumentName)
        => Format(Resources.AmbiguousCommandPrefixFormat, argumentName);

    /// <summary>
    /// Gets the error message used if the <see cref="KeyValuePairConverter{TKey, TValue}"/>
    /// is unable to find the key/value pair separator in the argument value.
    /// </summary>
    /// <param name="separator">The key/value pair separator.</param>
    /// <returns>The error message.</returns>
    public virtual string MissingKeyValuePairSeparator(string separator)
        => Format(Resources.NoKeyValuePairSeparatorFormat, separator);

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
        Debug.Assert(category != CommandLineArgumentErrorCategory.AmbiguousPrefixAlias);

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
