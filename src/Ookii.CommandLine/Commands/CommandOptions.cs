// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;

namespace Ookii.CommandLine.Commands
{
    /// <summary>
    /// Provides options for the <see cref="CommandManager"/> class.
    /// </summary>
    public class CommandOptions : ParseOptions
    {
        /// <summary>
        /// Gets the default value for the <see cref="CommandDescriptionIndent"/> property.
        /// </summary>
        public const int DefaultCommandDescriptionIndent = 8;

        /// <summary>
        /// Gets or sets the <see cref="IEqualityComparer{T}"/> used to compare command names.
        /// </summary>
        /// <value>
        /// The <see cref="IEqualityComparer{T}"/> used to compare command names. The default value is <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </value>
        public IComparer<string> CommandNameComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Gets or sets a value that indicates how names are created for commands that don't have
        /// an explicit name.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="NameTransform"/> enumeration. The default value
        /// is <see cref="NameTransform.None"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If a command hasn't set an explicit name using the <see cref="CommandAttribute"/>
        ///   attribute, the name is derived from the type name of the command, applying the
        ///   specified transformation.
        /// </para>
        /// <para>
        ///   If this property is not <see cref="NameTransform.None"/>, the value specified by the
        ///   <see cref="StripCommandNameSuffix"/> property will be removed from the end of the
        ///   type name before applying the transformation.
        /// </para>
        /// <para>
        ///   This transformation is also used for the name of the automatic version command if
        ///   the <see cref="AutoVersionCommand"/> property is <see langword="true"/>.
        /// </para>
        /// <para>
        ///   This transformation is not used for commands that have an explicit name.
        /// </para>
        /// </remarks>
        public NameTransform CommandNameTransform { get; set; }

        /// <summary>
        /// Gets or sets a value that will be removed from the end of a command name during name
        /// transformation.
        /// </summary>
        /// <value>
        /// The suffix to remove, or <see langword="null"/> to not remove any suffix. The default
        /// value is "Command".
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property is only used if the <see cref="CommandNameTransform"/> property is not 
        ///   <see cref="NameTransform.None"/>, and is never used for commands with an explicit
        ///   name.
        /// </para>
        /// <para>
        ///   For example, if you have a subcommand class named "CreateFileCommand" and you use
        ///   <see cref="NameTransform.DashCase"/> and the default value of "Command" for this
        ///   property, the name of the command will be "create-file" without having to explicitly
        ///   specify it.
        /// </para>
        /// <para>
        ///   The suffix is case sensitive.
        /// </para>
        /// </remarks>
        public string? StripCommandNameSuffix { get; set; } = "Command";

        /// <summary>
        /// Gets or sets the color applied to the <see cref="LocalizedStringProvider.CommandDescription"/>.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="TextFormat.ForegroundGreen"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The color will only be used if the <see cref="WriteUsageOptions.UseColor"/> property is
        ///   <see langword="true"/>; otherwise, it will be replaced with an empty string.
        /// </para>
        /// <para>
        ///   If the string contains anything other than virtual terminal sequences, those parts
        ///   will be included in the output, but only when the <see cref="WriteUsageOptions.UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   The portion of the string that has color will end with the <see cref="WriteUsageOptions.ColorReset"/>.
        /// </para>
        /// <para>
        ///   With the default value, only the command name portion of the string has color; the
        ///   application name does not.
        /// </para>
        /// </remarks>
        public string CommandDescriptionColor { get; set; } = TextFormat.ForegroundGreen;

        /// <summary>
        /// Gets or sets the number of characters by which to indent the all but the first line of command descriptions.
        /// </summary>
        /// <value>
        /// The number of characters by which to indent the all but the first line of command descriptions. The default value is 8.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This value should be adjusted to match the return value specified by the
        ///   <see cref="LocalizedStringProvider.CommandDescription"/> property.
        /// </para>
        /// <para>
        ///   This value is not used if <see cref="ParseOptions.Out"/> is not a <see cref="LineWrappingTextWriter"/>, or the
        ///   maximum line length is less than 30.
        /// </para>
        /// </remarks>
        public int CommandDescriptionIndent { get; set; } = DefaultCommandDescriptionIndent;

        /// <summary>
        /// Gets or sets a value that indicates whether a version command should automatically be
        /// created.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to automatically create a version command; otherwise,
        /// <see langword="false"/>. The default is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is true, a command named "version" will be automatically added to
        ///   the list of available commands, unless a command with that name already exists.
        ///   When invoked, the command will show version information for the application, based
        ///   on the entry point assembly.
        /// </para>
        /// </remarks>
        public bool AutoVersionCommand { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether a message is shown at the bottom of the
        /// command list that instructs the user how to get help for individual commands.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to show the instruction; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If set to <see langword="true"/>, the message is provided by <see cref="LocalizedStringProvider.CommandHelpInstruction"/>.
        ///   The default implementation of that method assumes that all commands have a help
        ///   argument, the same <see cref="ParsingMode"/>, and the same argument prefixes. For
        ///   that reason, showing this message is not enabled by default.
        /// </para>
        /// </remarks>
        public bool ShowCommandHelpInstruction { get; set; }

        internal string AutoVersionCommandName()
        {
            return CommandNameTransform.Apply(StringProvider.AutomaticVersionCommandName());
        }

    }
}
