// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Collections.Generic;
using System.IO;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides options for the <see cref="ShellCommand.CreateShellCommand(System.Reflection.Assembly,string,string[],int,CreateShellCommandOptions)"/>
    /// and <see cref="ShellCommand.RunShellCommand(System.Reflection.Assembly, string?, string[], int, CreateShellCommandOptions)"/> methods.
    /// </summary>
    public class CreateShellCommandOptions : ParseOptions
    {
        private string? _commandUsageFormat;
        private string? _commandDescriptionFormat;
        private string? _availableCommandsHeader;

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
        /// Gets or sets the format string to use for the usage help if no command name was supplied or the command name was not recognized.
        /// </summary>
        /// <value>
        /// The format string to use for the usage if no command was specified or the command name was not recognized. The default value is "{0} &lt;command&gt; [arguments]".
        /// </value>
        /// <remarks>
        /// <para>
        ///   This format string should have one placeholder.
        /// </para>
        /// </remarks>
        public string CommandUsageFormat
        {
            get { return _commandUsageFormat ?? Properties.Resources.DefaultCommandUsageFormat; }
            set { _commandUsageFormat = value; }
        }

        /// <summary>
        /// Gets or sets the format string used to format a command's name and description.
        /// </summary>
        /// <value>
        /// The format string used to format a command's name and description. The default value is "&#160;&#160;&#160;&#160;{2}{0}{3}\n{1}\n".
        /// </value>
        /// <remarks>
        /// <para>
        ///   If you change the description format, you should also change the value of the <see cref="CommandDescriptionIndent"/>
        ///   to an appropriate value. The default format uses an indentation of 8 characters.
        /// </para>
        /// <para>
        ///   This string can have the following placeholders:
        /// </para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Placeholder</term>
        ///     <description>Description</description>
        ///   </listheader>
        ///   <item>
        ///     <term>{0}</term>
        ///     <description>
        ///       The name of the command.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{1}</term>
        ///     <description>
        ///       The description of the command.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{2}</term>
        ///     <description>
        ///       If the <see cref="WriteUsageOptions.UseColor"/> property is <see langword="true"/>, the value of
        ///       the <see cref="CommandDescriptionColor"/> property; otherwise, an empty string.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{3}</term>
        ///     <description>
        ///       If the <see cref="WriteUsageOptions.UseColor"/> property is <see langword="false"/>, the value of
        ///       the <see cref="WriteUsageOptions.ColorReset"/> property; otherwise, an empty string.
        ///     </description>
        ///   </item>
        /// </list>
        /// </remarks>
        public string CommandDescriptionFormat
        {
            get { return _commandDescriptionFormat ?? Properties.Resources.DefaultCommandFormat; }
            set { _commandDescriptionFormat = value; }
        }

        /// <summary>
        /// Gets or sets the color applied to the <see cref="CommandDescriptionFormat"/>.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="VirtualTerminal.TextFormat.ForegroundGreen"/>.
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
        public string CommandDescriptionColor { get; set; } = VirtualTerminal.TextFormat.ForegroundGreen;

        /// <summary>
        /// Gets or sets the number of characters by which to indent the all but the first line of command descriptions.
        /// </summary>
        /// <value>
        /// The number of characters by which to indent the all but the first line of command descriptions. The default value is 8.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This value should be adjusted to match the formatting specified by the <see cref="CommandDescriptionFormat"/> property.
        /// </para>
        /// <para>
        ///   This value is not used if <see cref="ParseOptions.Out"/> is not a <see cref="LineWrappingTextWriter"/>, or the
        ///   maximum line length is less than 30.
        /// </para>
        /// </remarks>
        public int CommandDescriptionIndent { get; set; } = DefaultCommandDescriptionIndent;

        /// <summary>
        /// Gets or sets the header that is used when printing a list of commands if no command name was supplied or the command name was not recognized.
        /// </summary>
        /// <value>
        /// The header that is used when printing a list of commands if no command name was supplied or the command name was not recognized. The default value is "The following commands are available:".
        /// </value>
        public string AvailableCommandsHeader
        {
            get { return _availableCommandsHeader ?? Properties.Resources.DefaultAvailableCommandsHeader; }
            set { _availableCommandsHeader = value; }
        }

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

    }
}
