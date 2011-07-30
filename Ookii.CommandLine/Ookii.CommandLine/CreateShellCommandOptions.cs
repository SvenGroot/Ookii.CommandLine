// $Id: CreateShellCommandOptions.cs 30 2011-06-26 11:09:43Z sgroot $
//
using System;
using System.Collections.Generic;
using System.IO;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides options for the <see cref="ShellCommand.CreateShellCommand(System.Reflection.Assembly,string,string[],int,CreateShellCommandOptions)"/> method.
    /// </summary>
    public sealed class CreateShellCommandOptions
    {
        private string _commandUsageFormat;
        private string _commandDescriptionFormat;
        private string _availableCommandsHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateShellCommandOptions"/> class.
        /// </summary>
        public CreateShellCommandOptions()
        {
            AllowWhiteSpaceValueSeparator = true;
            UsageOptions = new WriteUsageOptions();
            CommandDescriptionIndent = 16;
            ArgumentNameComparer = StringComparer.OrdinalIgnoreCase;
            CommandNameComparer = StringComparer.OrdinalIgnoreCase;
        }

        /// <summary>
        /// Gets or sets the <see cref="IEqualityComparer{T}"/> used to compare command names.
        /// </summary>
        /// <value>
        /// The <see cref="IEqualityComparer{T}"/> used to compare command names. The default value is <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </value>
        public IEqualityComparer<string> CommandNameComparer { get; set; }

        /// <summary>
        /// Gets or sets the argument name prefixes to use when parsing the shell command's arguments.
        /// </summary>
        /// <value>
        /// The named argument switches, or <see langword="null"/> to indicate the default prefixes for
        /// the current platform must be used. The default value is <see langword="null"/>.
        /// </value>
        public IEnumerable<string> ArgumentNamePrefixes { get; set; }

        /// <summary>
        /// Gets or set the <see cref="IComparer{T}"/> to use to compare argument names.
        /// </summary>
        /// <value>
        /// The <see cref="IComparer{T}"/> to use to compare the names of named arguments. The default value is <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </value>
        public IComparer<string> ArgumentNameComparer { get; set; }

        /// <summary>
        /// Gets or sets the output <see cref="TextWriter"/> used to print usage information.
        /// </summary>
        /// <value>
        /// The <see cref="TextWriter"/> used to print usage information, or <see langword="null"/>
        /// to print to the standard output stream. The default value is <see langword="null"/>.
        /// </value>
        public TextWriter Out { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TextWriter"/> used to print error information.
        /// </summary>
        /// <value>
        /// The <see cref="TextWriter"/> used to print error information, or <see langword="null"/>
        /// to print to the standard output stream. The default value is <see langword="null"/>.
        /// </value>
        public TextWriter Error { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether duplicate arguments are allowed.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if it is allowed to supply non-array arguments more than once; otherwise, <see langword="false"/>.
        ///   The default value is <see langword="false"/>.
        /// </value>
        /// <seealso cref="CommandLineParser.AllowDuplicateArguments"/>
        public bool AllowDuplicateArguments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value of arguments may be separated from the name by white space.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if white space is allowed to separate an argument name and its value; <see langword="false"/> if only the colon (:) is allowed.
        ///   The default value is <see langword="true"/>.
        /// </value>
        /// <seealso cref="CommandLineParser.AllowWhiteSpaceValueSeparator"/>
        public bool AllowWhiteSpaceValueSeparator { get; set; }

        /// <summary>
        /// Gets or sets the options to use when parsing the shell command fails.
        /// </summary>
        /// <value>
        /// The usage options.
        /// </value>
        public WriteUsageOptions UsageOptions { get; set; }

        /// <summary>
        /// Gets or sets the format string to use for the usage help if no command name was supplied or the command name was not recognized.
        /// </summary>
        /// <value>
        /// The format string to use for the usage if no command was specified or the command name was not recognized. The default value is "{0} &lt;command&gt; [args...]".
        /// </value>
        /// <remarks>
        /// <para>
        ///   This format string shoud have one placeholder, which is used for the value of the <see cref="WriteUsageOptions.UsagePrefix"/> property.
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
        /// The format string used to format a command's name and description. The default value is "{0,13} : {1}".
        /// </value>
        /// <remarks>
        /// <para>
        ///   This format string should have two placeholders, which are used for command's name and description respectively.
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
        /// The number of characters by which to indent the all but the first line of command descriptions. The default value is 16.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This value should be adjusted to match the formatting specified by the <see cref="CommandDescriptionFormat"/> property.
        /// </para>
        /// <para>
        ///   This value is not used if the maximum line length of the <see cref="LineWrappingTextWriter"/> to which the usage
        ///   is being written is less than 30.
        /// </para>
        /// </remarks>
        public int CommandDescriptionIndent { get; set; }

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
