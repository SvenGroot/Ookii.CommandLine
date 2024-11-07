using Ookii.CommandLine.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine;

partial class LocalizedStringProvider
{
    /// <summary>
    /// Gets the default prefix for the usage syntax, used by the <see cref="UsageWriter"/> class.
    /// </summary>
    /// <returns>The string.</returns>
    public virtual string UsageSyntaxPrefix() => Resources.DefaultUsagePrefix;

    /// <summary>
    /// Gets the default suffix for the usage syntax when creating command list usage help, used by
    /// the <see cref="UsageWriter"/> class.
    /// </summary>
    /// <returns>The string.</returns>
    public virtual string CommandUsageSuffix() => Resources.DefaultCommandUsageSuffix;

    /// <summary>
    /// Gets the default suffix for the usage syntax to indicate more arguments are available if the
    /// syntax is abbreviated, used by the <see cref="UsageWriter"/> class.
    /// </summary>
    /// <returns>The string.</returns>
    public virtual string UsageAbbreviatedRemainingArguments() => Resources.DefaultAbbreviatedRemainingArguments;

    /// <summary>
    /// Gets the default header to print above the list of available commands, used by the
    /// <see cref="UsageWriter"/> class.
    /// </summary>
    /// <returns>The string.</returns>
    public virtual string UsageAvailableCommandsHeader() => Resources.DefaultAvailableCommandsHeader;

    /// <summary>
    /// Gets the text to use to display a default value in the usage help, used by the
    /// <see cref="UsageWriter"/> class.
    /// </summary>
    /// <param name="defaultValue">The argument's default value.</param>
    /// <param name="formatProvider">
    /// An object that provides culture-specific format information for the default value. This
    /// will be the value of the <see cref="CommandLineParser.Culture"/> property, to ensure the
    /// format used matches the format used when parsing arguments.
    /// </param>
    /// <returns>The string.</returns>
    public virtual string UsageDefaultValue(object defaultValue, IFormatProvider formatProvider)
        => string.Format(formatProvider, Resources.DefaultDefaultValueFormat, defaultValue);

    /// <summary>
    /// Gets a message telling the user how to get more detailed help, used by the
    /// <see cref="UsageWriter"/> class.
    /// </summary>
    /// <param name="executableName">
    /// The application's executable name, optionally including the command name.
    /// </param>
    /// <param name="helpArgumentName">The name of the help argument, including prefix.</param>
    /// <returns>The string.</returns>
    public virtual string UsageMoreInfoMessage(string executableName, string helpArgumentName)
        => Format(Resources.MoreInfoOnErrorFormat, executableName, helpArgumentName);

    /// <summary>
    /// Gets an instruction on how to get help on a command, used by the <see cref="UsageWriter"/>
    /// class.
    /// </summary>
    /// <param name="name">The application and command name.</param>
    /// <param name="argumentNamePrefix">The argument name prefix for the help argument.</param>
    /// <param name="argumentName">The help argument name.</param>
    /// <returns>The string.</returns>
    public virtual string UsageCommandHelpInstruction(string name, string argumentNamePrefix, string argumentName)
        => Format(Resources.CommandHelpInstructionFormat, name, argumentNamePrefix, argumentName);

    /// <summary>
    /// Gets the default header to print above the list of available commands, used by the
    /// <see cref="UsageWriter.WriteParserAmbiguousPrefixAliasUsageCore" qualifyHint="true"/>
    /// method.
    /// </summary>
    /// <returns>The string.</returns>
    public virtual string AmbiguousPrefixAliasMatchesHeader() => Resources.AmbiguousArgumentPrefixMatchesHeader;
}
