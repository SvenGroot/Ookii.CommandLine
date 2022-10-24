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
        public IEqualityComparer<string> CommandNameComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Gets or sets the format string to use for the usage help if no command name was supplied or the command name was not recognized.
        /// </summary>
        /// <value>
        /// The format string to use for the usage if no command was specified or the command name was not recognized. The default value is "{0} &lt;command&gt; [args...]".
        /// </value>
        /// <remarks>
        /// <para>
        ///   This format string shoud have one placeholder, which is used for the value of the <see cref="WriteUsageOptions.UsagePrefixFormat"/> property.
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
        /// The format string used to format a command's name and description. The default value is "&#160;&#160;&#160;&#160;{0}\n{1}\n".
        /// </value>
        /// <remarks>
        /// <para>
        ///   This format string should have two placeholders, which are used for command's name and description respectively. If the
        ///   format ends in a line break, the commands will be separated by a blank line (this is the default).
        /// </para>
        /// <para>
        ///   If you change the description format, you should also change the value of the <see cref="CommandDescriptionIndent"/>
        ///   to an appropriate value. The default format uses an indentation of 8 characters.
        /// </para>
        /// </remarks>
        public string CommandDescriptionFormat
        {
            get { return _commandDescriptionFormat ?? Properties.Resources.DefaultCommandFormat; }
            set { _commandDescriptionFormat = value; }
        }


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
        
    }
}
