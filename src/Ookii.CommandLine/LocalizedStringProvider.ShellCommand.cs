using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Properties;
using System;

namespace Ookii.CommandLine
{
    public partial class LocalizedStringProvider
    {
        /// <summary>
        /// Gets the name of the version command created if the <see cref="CommandOptions.AutoVersionCommand"/>
        /// property is <see langword="true"/>.
        /// </summary>
        /// <returns>The string.</returns>
        public virtual string AutomaticVersionCommandName() => Resources.AutomaticVersionCommandName;

        /// <summary>
        /// Gets the description of the version command created if the <see cref="CommandOptions.AutoVersionCommand"/>
        /// property is <see langword="true"/>.
        /// </summary>
        /// <returns>The string.</returns>
        public virtual string AutomaticVersionCommandDescription() => Resources.AutomaticVersionDescription;

        /// <summary>
        /// Gets a the usage syntax for an application using subcommands when no command name
        /// was specified, similar to "Usage: executable &lt;command&gt; [arguments]".
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
        public virtual string RootCommandUsageSyntax(string executableName, string color, string colorReset)
            => UsagePrefix(executableName, color, colorReset) + Resources.DefaultCommandUsageSuffix;

        /// <summary>
        /// Gets the description of a subcommand, used when listing commands in the usage help.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="options">The options used to format the usage help.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   If you override this function, you may also need to change the <see cref="CommandOptions.CommandDescriptionIndent"/>
        ///   property to a value suitable for your description format.
        /// </para>
        /// </remarks>
        public virtual string CommandDescription(CommandInfo command, CommandOptions options)
        {
            bool useColor = options.UsageOptions.UseColor ?? false;
            string colorStart = string.Empty;
            string colorEnd = string.Empty;
            if (useColor)
            {
                colorStart = options.CommandDescriptionColor;
                colorEnd = options.UsageOptions.ColorReset;
            }

            return $"    {colorStart}{command.Name}{colorEnd}{Environment.NewLine}{command.Description ?? string.Empty}{Environment.NewLine}";
        }

        /// <summary>
        /// Gets header text to print before the list of commands in the usage help.
        /// </summary>
        /// <param name="useColor">The value of <see cref="WriteUsageOptions.UseColor"/>.</param>
        /// <returns>The string.</returns>
        /// <remarks>
        /// <para>
        ///   This string doesn't have any predefined colors in the <see cref="WriteUsageOptions"/>
        ///   class, so the <paramref name="useColor"/> parameter is provided to allow you to
        ///   manually add colors if desired.
        /// </para>
        /// </remarks>
        public virtual string AvailableCommandsHeader(bool useColor) => Resources.DefaultAvailableCommandsHeader;

        /// <summary>
        /// Gets a string used at the start of the usage syntax for a <see cref="ICommand"/>,
        /// similar to "Usage: executable command_name".
        /// </summary>
        /// <param name="executableName">The value of <see cref="WriteUsageOptions.ExecutableName"/>.</param>
        /// <param name="commandName">The name of the subcommand.</param>
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

    }
}
