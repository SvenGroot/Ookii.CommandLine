using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Properties;
using Ookii.CommandLine.Terminal;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Creates usage help for the <see cref="CommandLineParser"/> class and the <see cref="Commands.CommandManager"/>
    /// class.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   You can derive from this class to override the formatting of various aspects of the usage
    ///   help. Set the <see cref="ParseOptions.UsageWriter"/> property to specify a custom instance.
    /// </para>
    /// <para>
    ///   Depending on what methods you override, you can change small parts of the formatting, or
    ///   completely change how usage looks. Certain methods may not be called if you override the
    ///   methods that call them.
    /// </para>
    /// <para>
    ///   This class has a number of properties that customize the usage help for the base
    ///   implementation of this class. It is not guaranteed that a derived class will respect
    ///   these properties.
    /// </para>
    /// </remarks>
    public class UsageWriter
    {
        #region Nested types

        /// <summary>
        /// Indicates the type of operation in progress.
        /// </summary>
        /// <seealso cref="OperationInProgress"/>
        protected enum Operation
        {
            /// <summary>
            /// No operation is in progress.
            /// </summary>
            None,
            /// <summary>
            /// A call to <see cref="WriteParserUsage"/> is in progress.
            /// </summary>
            ParserUsage,
            /// <summary>
            /// A call to <see cref="WriteCommandListUsage"/> is in progress.
            /// </summary>
            CommandListUsage,
        }

        #endregion

        /// <summary>
        /// The default indentation for the usage syntax.
        /// </summary>
        /// <value>
        /// The default indentation, which is three characters.
        /// </value>
        /// <seealso cref="SyntaxIndent"/>
        public const int DefaultSyntaxIndent = 3;

        /// <summary>
        /// The default indentation for the argument descriptions for the <see cref="ParsingMode.Default"/>
        /// mode.
        /// </summary>
        /// <value>
        /// The default indentation, which is eight characters.
        /// </value>
        /// <seealso cref="ArgumentDescriptionIndent"/>
        public const int DefaultArgumentDescriptionIndent = 8;

        /// <summary>
        /// The default indentation for the application description.
        /// </summary>
        /// <value>
        /// The default indentation, which is zero.
        /// </value>
        /// <seealso cref="IncludeApplicationDescription"/>
        public const int DefaultApplicationDescriptionIndent = 0;

        /// <summary>
        /// Gets the default value for the <see cref="CommandDescriptionIndent"/> property.
        /// </summary>
        public const int DefaultCommandDescriptionIndent = 8;

        // Don't apply indentation to console output if the line width is less than this.
        private const int MinimumLineWidthForIndent = 30;

        private const char OptionalStart = '[';
        private const char OptionalEnd = ']';

        private LineWrappingTextWriter? _writer;
        private bool? _useColor;
        private CommandLineParser? _parser;
        private CommandManager? _commandManager;
        private string? _executableName;
        private string? _defaultExecutableName;
        private bool _includeExecutableExtension;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsageWriter"/> class.
        /// </summary>
        /// <param name="writer">
        /// A <see cref="LineWrappingTextWriter"/> instance to write usage to, or <see langword="null"/>
        /// to write to the standard output stream.
        /// </param>
        /// <param name="useColor">
        /// <see langword="true"/> to enable color output using virtual terminal sequences;
        /// <see langword="false"/> to disable it; or, <see langword="null"/> to automatically
        /// enable it if <paramref name="writer"/> is <see langword="null"/> using the
        /// <see cref="VirtualTerminal.EnableColor"/> method.
        /// </param>
        /// <remarks>
        /// <para>
        ///   If the <paramref name="writer"/> parameter is <see langword="null"/>, output is
        ///   written to a <see cref="LineWrappingTextWriter"/> for the standard output stream,
        ///   wrapping at the console's window width. If the stream is redirected, output may still
        ///   be wrapped, depending on the value returned by <see cref="Console.WindowWidth"/>.
        /// </para>
        /// </remarks>
        public UsageWriter(LineWrappingTextWriter? writer = null, bool? useColor = null)
        {
            _writer = writer;
            _useColor = useColor;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value of the <see cref="CommandLineParser.Description"/> property
        /// is written before the syntax.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the value of the <see cref="CommandLineParser.Description"/> property
        ///   is written before the syntax; otherwise, <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        public bool IncludeApplicationDescription { get; set; } = true;

        /// <summary>
        /// The indentation to use for the application description.
        /// </summary>
        /// <value>
        /// The indentation. The default value is the value of the <see cref="DefaultApplicationDescriptionIndent"/>
        /// constant.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property is only used if the <see cref="IncludeApplicationDescription"/> property
        ///   is <see langword="true"/>.
        /// </para>
        /// <para>
        ///   This also applies to the command description when showing usage help for a subcommand.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandManager"/>
        public int ApplicationDescriptionIndent { get; set; } = DefaultApplicationDescriptionIndent;

        /// <summary>
        /// Gets or sets a value that overrides the default application executable name used in the
        /// usage syntax.
        /// </summary>
        /// <value>
        /// The application executable name, or <see langword="null"/> to use the default value,
        /// determined by calling <see cref="CommandLineParser.GetExecutableName(bool)"/>.
        /// </value>
        /// <seealso cref="IncludeExecutableExtension"/>
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [AllowNull]
#endif
        public virtual string ExecutableName
        {
            get => _executableName ?? (_defaultExecutableName ??= CommandLineParser.GetExecutableName(IncludeExecutableExtension));
            set => _executableName = value;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the usage syntax should include the file
        /// name extension of the application's executable.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the extension should be included; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If the <see cref="ExecutableName"/> property is <see langword="null"/>, the executable
        ///   name is determined by calling <see cref="CommandLineParser.GetExecutableName(bool)"/>,
        ///   passing the value of this property as the argument.
        /// </para>
        /// <para>
        ///   This property is not used if the <see cref="ExecutableName"/> property is not
        ///   <see langword="null"/>.
        /// </para>
        /// </remarks>
        public bool IncludeExecutableExtension
        {
            get => _includeExecutableExtension;
            set
            {
                _includeExecutableExtension = value;
                _defaultExecutableName = null;
            }
        }

        /// <summary>
        /// Gets or sets the color applied by the <see cref="WriteUsageSyntaxPrefix"/> method.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="TextFormat.ForegroundCyan"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The color will only be used if the <see cref="UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   If the string contains anything other than virtual terminal sequences, those parts
        ///   will be included in the output, but only when the <see cref="UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   The portion of the string that has color will end with the value of the
        ///   <see cref="ColorReset"/> property.
        /// </para>
        /// <para>
        ///   With the base implementation, only the "Usage:" portion of the string has color; the
        ///   executable name does not.
        /// </para>
        /// </remarks>
        public string UsagePrefixColor { get; set; } = TextFormat.ForegroundCyan;

        /// <summary>
        /// Gets or sets the number of characters by which to indent all except the first line of the command line syntax of the usage help.
        /// </summary>
        /// <value>
        /// The number of characters by which to indent the usage syntax. The default value is the
        /// value of the <see cref="DefaultSyntaxIndent"/> constant.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The command line syntax is a single line that consists of the usage prefix written
        ///   by <see cref="WriteUsageSyntaxPrefix"/> followed by the syntax of all
        ///   the arguments. This indentation is used when that line exceeds the maximum line
        ///   length.
        /// </para>
        /// <para>
        ///   This value is not used if the maximum line length of the <see cref="LineWrappingTextWriter"/> to which the usage
        ///   is being written is less than 30.
        /// </para>
        /// </remarks>
        public int SyntaxIndent { get; set; } = DefaultSyntaxIndent;

        /// <summary>
        /// Gets or sets a value that indicates whether the usage syntax should use short names
        /// for arguments that have one.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to use short names for arguments that have one; otherwise,
        /// <see langword="false"/> to use an empty string. The default value is
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <note>
        ///   This property is only used when the <see cref="CommandLineParser.Mode"/> property is
        ///   <see cref="ParsingMode.LongShort"/>.
        /// </note>
        /// </remarks>
        public bool UseShortNamesForSyntax { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to list only positional arguments in the
        /// usage syntax.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to abbreviate the syntax; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   Abbreviated usage syntax only lists the positional arguments explicitly. After that,
        ///   if there are any more arguments, it will just print the value from the
        ///   <see cref="WriteAbbreviatedRemainingArguments"/> method. The user will have to refer
        ///   to the description list to see the remaining possible
        ///   arguments.
        /// </para>
        /// <para>
        ///   Use this if your application has a very large number of arguments.
        /// </para>
        /// </remarks>
        public bool UseAbbreviatedSyntax { get; set; }

        /// <summary>
        /// Gets or sets the number of characters by which to indent all but the first line of each
        /// argument's description, if the <see cref="CommandLineParser.Mode"/> property is
        /// <see cref="ParsingMode.Default"/>.
        /// </summary>
        /// <value>
        /// The number of characters by which to indent the argument descriptions. The default
        /// value is the value of the <see cref="DefaultArgumentDescriptionIndent"/> constant.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property is used by the <see cref="WriteArgumentDescriptions"/> method.
        /// </para>
        /// <para>
        ///   This value is not used if the maximum line length of the <see cref="LineWrappingTextWriter"/> to which the usage
        ///   is being written is less than 30.
        /// </para>
        /// </remarks>
        public int ArgumentDescriptionIndent { get; set; } = DefaultArgumentDescriptionIndent;

        /// <summary>
        /// Gets or sets a value that indicates which arguments should be included in the list of
        /// argument descriptions.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="DescriptionListFilterMode"/> enumeration. The default
        /// value is <see cref="DescriptionListFilterMode.Information"/>.
        /// </value>
        public DescriptionListFilterMode ArgumentDescriptionListFilter { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the order of the arguments in the list of argument
        /// descriptions.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="DescriptionListSortMode"/> enumeration. The default
        /// value is <see cref="DescriptionListSortMode.UsageOrder"/>.
        /// </value>
        public DescriptionListSortMode ArgumentDescriptionListOrder { get; set; }

        /// <summary>
        /// Gets or sets the color applied by the <see cref="WriteArgumentDescription(CommandLineArgument)"/> method.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="TextFormat.ForegroundGreen"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The color will only be used if the <see cref="UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   If the string contains anything other than virtual terminal sequences, those parts
        ///   will be included in the output, but only when the <see cref="UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   The portion of the string that has color will end with the value of the 
        ///   <see cref="ColorReset"/> property.
        /// </para>
        /// <para>
        ///   With the default format, only the argument name, value description and aliases
        ///   portion of the string has color; the actual argument description does not.
        /// </para>
        /// </remarks>
        public string ArgumentDescriptionColor { get; set; } = TextFormat.ForegroundGreen;

        /// <summary>
        /// Gets or sets a value indicating whether white space, rather than the first item of the
        /// <see cref="CommandLineParser.NameValueSeparators"/> property, is used to separate
        /// arguments and their values in the command line syntax.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the command line syntax uses a white space value separator; <see langword="false"/> if it uses a colon.
        ///   The default value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is <see langword="true"/>, an argument would be formatted in the command line syntax as "-name &lt;Value&gt;" (using
        ///   default formatting), with a white space character separating the argument name and value description. If this property is <see langword="false"/>,
        ///   it would be formatted as "-name:&lt;Value&gt;", using a colon as the separator.
        /// </para>
        /// <para>
        ///   The command line syntax will only use a white space character as the value separator if both the <see cref="CommandLineParser.AllowWhiteSpaceValueSeparator"/> property
        ///   and the <see cref="UseWhiteSpaceValueSeparator"/> property are true.
        /// </para>
        /// </remarks>
        public bool UseWhiteSpaceValueSeparator { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the alias or aliases of an argument should be included in the argument description..
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the alias(es) should be included in the description;
        /// otherwise, <see langword="false" />. The default value is <see langword="true" />.
        /// </value>
        /// <remarks>
        /// <para>
        ///   For arguments that do not have any aliases, this property has no effect.
        /// </para>
        /// </remarks>
        public bool IncludeAliasInDescription { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the default value of an argument should be included in the argument description.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the default value should be included in the description;
        /// otherwise, <see langword="false" />. The default value is <see langword="true" />.
        /// </value>
        /// <remarks>
        /// <para>
        ///   For arguments with a default value of <see langword="null"/>, this property has no effect.
        /// </para>
        /// </remarks>
        public bool IncludeDefaultValueInDescription { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Validation.ArgumentValidationAttribute"/>
        /// attributes of an argument should be included in the argument description.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the validator descriptions should be included in; otherwise,
        /// <see langword="false" />. The default value is <see langword="true" />.
        /// </value>
        /// <remarks>
        /// <para>
        ///   For arguments with no validators, or validators with no usage help, this property
        ///   has no effect.
        /// </para>
        /// </remarks>
        public bool IncludeValidatorsInDescription { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="WriteArgumentDescription(CommandLineArgument)"/>
        /// method will write a blank lines between arguments in the description list.
        /// </summary>
        /// <value>
        /// <see langword="true" /> to write a blank line; otherwise, <see langword="false" />. The
        /// default value is <see langword="true" />.
        /// </value>
        public bool BlankLineAfterDescription { get; set; } = true;

        /// <summary>
        /// Gets or sets the sequence used to reset color applied a usage help element.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence used to reset color. The default value is
        ///   <see cref="TextFormat.Default"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property will only be used if the <see cref="UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   If the string contains anything other than virtual terminal sequences, those parts
        ///   will be included in the output, but only when the <see cref="UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// </remarks>
        public string ColorReset { get; set; } = TextFormat.Default;

        /// <summary>
        /// Gets or sets the name of the subcommand.
        /// </summary>
        /// <value>
        /// The name of the subcommand, or <see langword="null"/> if the current parser is not for
        /// a subcommand.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property is set by the <see cref="CommandManager"/> class before writing usage
        ///   help for a subcommand.
        /// </para>
        /// </remarks>
        public string? CommandName { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the usage help should use color.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> to enable color output; otherwise, <see langword="false"/>.
        /// </value>
        protected bool UseColor => _useColor ?? false;

        /// <summary>
        /// Gets or sets the color applied by the base implementation of the <see cref="WriteCommandDescription(CommandInfo)"/>
        /// method.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="TextFormat.ForegroundGreen"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The color will only be used if the <see cref="UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   If the string contains anything other than virtual terminal sequences, those parts
        ///   will be included in the output, but only when the <see cref="UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   The portion of the string that has color will end with the <see cref="ColorReset"/>.
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
        ///   This value is used by the base implementation of the <see cref="WriteCommandDescription(CommandInfo)"/>
        ///   class, unless the <see cref="ShouldIndent"/> property is <see langword="false"/>.
        /// </para>
        /// </remarks>
        public int CommandDescriptionIndent { get; set; } = DefaultCommandDescriptionIndent;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="WriteCommandDescription(CommandInfo)"/>
        /// method will write a blank lines between commands in the command list.
        /// </summary>
        /// <value>
        /// <see langword="true" /> to write a blank line; otherwise, <see langword="false" />. The
        /// default value is <see langword="true" />.
        /// </value>
        public bool BlankLineAfterCommandDescription { get; set; } = true;

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
        ///   If set to <see langword="true"/>, the message is provided by the <see cref="WriteCommandHelpInstruction"/>
        ///   method. The default implementation of that method assumes that all commands have a
        ///   help argument, the same <see cref="ParsingMode"/>, and the same argument prefixes. For
        ///   that reason, showing this message is not enabled by default.
        /// </para>
        /// </remarks>
        public bool IncludeCommandHelpInstruction { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to show the application description before
        /// the command list in the usage help.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to show the description; otherwise, <see langword="false"/>. The
        /// default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The description to show is taken from the <see cref="AssemblyDescriptionAttribute"/>
        ///   of the first assembly passed to the <see cref="CommandManager"/> class. If the
        ///   assembly has no description, nothing is written.
        /// </para>
        /// <para>
        ///   If the <see cref="CommandOptions.ParentCommand"/> property is not <see langword="null"/>,
        ///   and the specified type has a <see cref="DescriptionAttribute"/>, that description is
        ///   used instead.
        /// </para>
        /// </remarks>
        public bool IncludeApplicationDescriptionBeforeCommandList { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to show a command's aliases as part of the
        /// command list usage help.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to show the command's aliases; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        public bool IncludeCommandAliasInCommandList { get; set; } = true;

        /// <summary>
        /// Gets the <see cref="LineWrappingTextWriter"/> to which the usage should be written.
        /// </summary>
        /// <value>
        /// The <see cref="LineWrappingTextWriter"/> passed to the <see cref="UsageWriter(LineWrappingTextWriter?, bool?)"/>
        /// constructor, or an instance created by the <see cref="LineWrappingTextWriter.ForConsoleOut"/>
        /// or <see cref="LineWrappingTextWriter.ForStringWriter(int, IFormatProvider?, bool)"/>
        /// function.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// No <see cref="LineWrappingTextWriter"/> was passed to the constructor, and a
        /// <see cref="WriteParserUsage"/> operation is not in progress.
        /// </exception>
        protected LineWrappingTextWriter Writer
            => _writer ?? throw new InvalidOperationException(Resources.UsageWriterPropertyNotAvailable);

        /// <summary>
        /// Gets the <see cref="CommandLineParser"/> that usage is being written for.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// A <see cref="WriteParserUsage"/> operation is not in progress.
        /// </exception>
        protected CommandLineParser Parser
            => _parser ?? throw new InvalidOperationException(Resources.UsageWriterPropertyNotAvailable);

        /// <summary>
        /// Gets the <see cref="CommandManager"/> that usage is being written for.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// A <see cref="WriteCommandListUsage"/> operation is not in progress.
        /// </exception>
        protected CommandManager CommandManager
            => _commandManager ?? throw new InvalidOperationException(Resources.UsageWriterPropertyNotAvailable);

        /// <summary>
        /// Indicates what operation is currently in progress.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="Operation"/> enumeration.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is not <see cref="Operation.ParserUsage"/>, the <see cref="Parser"/>
        ///   property will throw an exception.
        /// </para>
        /// <para>
        ///   If this property is not <see cref="Operation.CommandListUsage"/>, the <see cref="CommandManager"/>
        ///   property will throw an exception.
        /// </para>
        /// <para>
        ///   If this property is <see cref="Operation.None"/>, the <see cref="Writer"/>
        ///   property may throw an exception.
        /// </para>
        /// </remarks>
        protected Operation OperationInProgress
        {
            get
            {
                if (_parser != null)
                {
                    return Operation.ParserUsage;
                }
                else if (_commandManager != null)
                {
                    return Operation.CommandListUsage;
                }

                return Operation.None;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether indentation should be enabled in the output.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="Writer"/> property's maximum line length is
        /// unlimited or greater than 30; otherwise, <see langword="false"/>.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// No <see cref="LineWrappingTextWriter"/> was passed to the constructor, and a
        /// <see cref="WriteParserUsage"/> operation is not in progress.
        /// </exception>
        protected virtual bool ShouldIndent => Writer.MaximumLineLength is 0 or >= MinimumLineWidthForIndent;

        /// <summary>
        /// Gets the separator used for argument names, command names, and aliases.
        /// </summary>
        /// <value>
        /// The string ", ".
        /// </value>
        protected virtual string NameSeparator => ", ";

        /// <summary>
        /// Creates usage help for the specified parser.
        /// </summary>
        /// <param name="parser">The <see cref="CommandLineParser"/>.</param>
        /// <param name="request">The parts of usage to write.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="parser"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   If no writer was passed to the <see cref="UsageWriter(LineWrappingTextWriter?, bool?)"/>
        ///   constructor, this method will create a <see cref="LineWrappingTextWriter"/> for the
        ///   standard output stream. If color usage wasn't explicitly enabled, it will be enabled
        ///   if the output supports it according to <see cref="VirtualTerminal.EnableColor"/>.
        /// </para>
        /// <para>
        ///   This method calls the <see cref="WriteParserUsageCore"/> method to create the usage help
        ///   text.
        /// </para>
        /// </remarks>
        public void WriteParserUsage(CommandLineParser parser, UsageHelpRequest request = UsageHelpRequest.Full)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            WriteUsageInternal(request);
        }

        /// <summary>
        /// Creates usage help for the specified command manager.
        /// </summary>
        /// <param name="manager">The <see cref="Commands.CommandManager"/></param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="manager"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   The usage help will contain a list of all available commands.
        /// </para>
        /// <para>
        ///   If no writer was passed to the <see cref="UsageWriter(LineWrappingTextWriter?, bool?)"/>
        ///   constructor, this method will create a <see cref="LineWrappingTextWriter"/> for the
        ///   standard output stream. If color usage wasn't explicitly enabled, it will be enabled
        ///   if the output supports it according to <see cref="VirtualTerminal.EnableColor"/>.
        /// </para>
        /// <para>
        ///   This method calls the <see cref="WriteCommandListUsageCore"/> method to create the
        ///   usage help text.
        /// </para>
        /// </remarks>
        public void WriteCommandListUsage(CommandManager manager)
        {
            _commandManager = manager ?? throw new ArgumentNullException(nameof(manager));
            WriteUsageInternal();
        }

        /// <summary>
        /// Returns a string with usage help for the specified parser.
        /// </summary>
        /// <returns>A string containing the usage help.</returns>
        /// <param name="parser">The <see cref="CommandLineParser"/>.</param>
        /// <param name="request">The parts of usage to write.</param>
        /// <param name="maximumLineLength">
        /// The length at which to white-space wrap lines in the output, or 0 to disable wrapping.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="parser"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   This method ignores the writer passed to the <see cref="UsageWriter(LineWrappingTextWriter?, bool?)"/>
        ///   constructor, and will use the <see cref="LineWrappingTextWriter.ForStringWriter"/>
        ///   method instead, and returns the resulting string. If color support was not explicitly
        ///   enabled, it will be disabled.
        /// </para>
        /// <para>
        ///   This method calls the <see cref="WriteParserUsageCore"/> method to create the usage help
        ///   text.
        /// </para>
        /// </remarks>
        public string GetUsage(CommandLineParser parser, UsageHelpRequest request = UsageHelpRequest.Full, int maximumLineLength = 0)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            return GetUsageInternal(maximumLineLength, request);
        }

        /// <summary>
        /// Returns a string with usage help for the specified command manager.
        /// </summary>
        /// <returns>A string containing the usage help.</returns>
        /// <param name="manager">The <see cref="Commands.CommandManager"/></param>
        /// <param name="maximumLineLength">
        /// The length at which to white-space wrap lines in the output, or 0 to disable wrapping.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="manager"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   The usage help will contain a list of all available commands.
        /// </para>
        /// <para>
        ///   This method ignores the writer passed to the <see cref="UsageWriter(LineWrappingTextWriter?, bool?)"/>
        ///   constructor, and will use the <see cref="LineWrappingTextWriter.ForStringWriter"/>
        ///   method instead, and returns the resulting string. If color support was not explicitly
        ///   enabled, it will be disabled.
        /// </para>
        /// <para>
        ///   This method calls the <see cref="WriteCommandListUsageCore"/> method to create the
        ///   usage help text.
        /// </para>
        /// </remarks>
        public string GetCommandListUsage(CommandManager manager, int maximumLineLength = 0)
        {
            _commandManager = manager ?? throw new ArgumentNullException(nameof(manager));
            return GetUsageInternal(maximumLineLength);
        }

        #region CommandLineParser usage

        /// <summary>
        /// Creates the usage help for a <see cref="CommandLineParser"/> instance.
        /// </summary>
        /// <param name="request">The parts of usage to write.</param>
        /// <remarks>
        /// <para>
        ///   This is the primary method used to generate usage help for the <see cref="CommandLineParser"/>
        ///   class. It calls into the various other methods of this class, so overriding this
        ///   method should not typically be necessary unless you wish to deviate from the order
        ///   in which usage elements are written.
        /// </para>
        /// <para>
        ///   The base implementation writes the application description, followed by the usage
        ///   syntax, followed by the class validator help messages, followed by a list of argument
        ///   descriptions. Which elements are included exactly can be influenced by the
        ///   <paramref name="request"/> parameter and the properties of this class.
        /// </para>
        /// </remarks>
        protected virtual void WriteParserUsageCore(UsageHelpRequest request)
        {
            if (request == UsageHelpRequest.None)
            {
                WriteMoreInfoMessage();
                return;
            }

            if (request == UsageHelpRequest.Full && IncludeApplicationDescription && !string.IsNullOrEmpty(Parser.Description))
            {
                WriteApplicationDescription(Parser.Description);
            }

            WriteParserUsageSyntax();
            if (request == UsageHelpRequest.Full)
            {
                if (IncludeValidatorsInDescription)
                {
                    WriteClassValidators();
                }

                WriteArgumentDescriptions();
                Writer.Indent = 0;
            }
            else
            {
                Writer.Indent = 0;
                WriteMoreInfoMessage();
            }
        }

        /// <summary>
        /// Writes the application description, or command description in case of a subcommand.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <remarks>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageCore"/>
        ///   method if the command has a description and the <see cref="IncludeApplicationDescription"/>
        ///   property is <see langword="true"/>.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteCommandListUsageCore"/>
        ///   method if the assembly has a description and the <see cref="IncludeApplicationDescriptionBeforeCommandList"/>
        ///   property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        protected virtual void WriteApplicationDescription(string description)
        {
            SetIndent(ApplicationDescriptionIndent);
            WriteLine(description);
            WriteLine();
        }

        /// <summary>
        /// Writes the usage syntax for the application or subcommand.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageCore"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteParserUsageSyntax()
        {
            Writer.ResetIndent();
            SetIndent(SyntaxIndent);

            WriteUsageSyntaxPrefix();
            foreach (CommandLineArgument argument in GetArgumentsInUsageOrder())
            {
                if (argument.IsHidden)
                {
                    continue;
                }

                Write(" ");
                if (UseAbbreviatedSyntax && argument.Position == null)
                {
                    WriteAbbreviatedRemainingArguments();
                    break;
                }

                if (argument.IsRequired)
                {
                    WriteArgumentSyntax(argument);
                }
                else
                {
                    WriteOptionalArgumentSyntax(argument);
                }
            }

            WriteUsageSyntaxSuffix();
            WriteLine(); // End syntax line
            WriteLine(); // Blank line
        }

        /// <summary>
        /// Gets the arguments in the order they will be shown in the usage syntax.
        /// </summary>
        /// <returns>A list of all arguments in usage order.</returns>
        /// <remarks>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageSyntax"/>
        ///   method.
        /// </para>
        /// <para>
        ///   The base implementation first returns positional arguments in the specified order,
        ///   then required non-positional arguments in alphabetical order, then the remaining
        ///   arguments in alphabetical order.
        /// </para>
        /// </remarks>
        protected virtual IEnumerable<CommandLineArgument> GetArgumentsInUsageOrder() => Parser.Arguments;

        /// <summary>
        /// Write the prefix for the usage syntax, including the executable name and, for
        /// subcommands, the command name.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The base implementation returns a string like "Usage: executable" or "Usage:
        ///   executable command", using the color specified. If color is enabled, part of the
        ///   string will be colored using the <see cref="UsagePrefixColor"/> property.
        /// </para>
        /// <para>
        ///   An implementation of this method should typically include the value of the
        ///   <see cref="ExecutableName"/> property, and the value of the <see cref="CommandName"/>
        ///   property if it's not <see langword="null"/>.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageSyntax"/>
        ///   method and the <see cref="WriteCommandListUsageSyntax"/> method.
        /// </para>
        /// </remarks>
        protected virtual void WriteUsageSyntaxPrefix()
        {
            WriteColor(UsagePrefixColor);
            Write(Resources.DefaultUsagePrefix);
            ResetColor();
            Write(' ');
            Write(ExecutableName);
            if (CommandName != null)
            {
                Write(' ');
                Write(CommandName);
            }
        }

        /// <summary>
        /// Write the suffix for the usage syntax.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The base implementation does nothing for parser usage, and writes a string like
        ///   " &lt;command&gt; [arguments]" for command manager usage.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageSyntax"/>
        ///   method and the <see cref="WriteCommandListUsageSyntax"/> method.
        /// </para>
        /// </remarks>
        protected virtual void WriteUsageSyntaxSuffix()
        {
            if (OperationInProgress == Operation.CommandListUsage)
            {
                WriteLine(Resources.DefaultCommandUsageSuffix);
            }
        }

        /// <summary>
        /// Writes the syntax for a single optional argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation surrounds the result of the <see cref="WriteArgumentSyntax"/>
        ///   method in square brackets.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageSyntax"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteOptionalArgumentSyntax(CommandLineArgument argument)
        {
            Write(OptionalStart);
            WriteArgumentSyntax(argument);
            Write(OptionalEnd);
        }

        /// <summary>
        /// Writes the syntax for a single argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <remarks>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageSyntax"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteArgumentSyntax(CommandLineArgument argument)
        {
            string argumentName;
            if (argument.HasShortName && UseShortNamesForSyntax)
            {
                argumentName = argument.ShortName.ToString();
            }
            else
            {
                argumentName = argument.ArgumentName;
            }

            var prefix = argument.Parser.Mode != ParsingMode.LongShort || (argument.HasShortName && (UseShortNamesForSyntax || !argument.HasLongName))
                ? argument.Parser.ArgumentNamePrefixes[0]
                : argument.Parser.LongArgumentNamePrefix!;

            char? separator = argument.Parser.AllowWhiteSpaceValueSeparator && UseWhiteSpaceValueSeparator
                ? null
                : argument.Parser.NameValueSeparators[0];

            if (argument.Position == null)
            {
                WriteArgumentName(argumentName, prefix);
            }
            else
            {
                WritePositionalArgumentName(argumentName, prefix, separator);
            }

            if (!argument.IsSwitch)
            {
                // Otherwise, the separator was included in the argument name.
                if (argument.Position == null || separator == null)
                {
                    Write(separator ?? ' ');
                }

                WriteValueDescription(argument.ValueDescription);
            }

            if (argument.IsMultiValue)
            {
                WriteMultiValueSuffix();
            }
        }

        /// <summary>
        /// Writes the name of an argument.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="prefix">
        /// The argument name prefix; if using <see cref="ParsingMode.LongShort"/>, this may vary
        /// depending on whether the name is a short or long name.
        /// </param>
        /// <remarks>
        /// <para>
        ///   The default implementation returns the prefix followed by the name, e.g. "-Name".
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentSyntax"/>
        ///   method and the <see cref="WritePositionalArgumentName"/> method.
        /// </para>
        /// </remarks>
        protected virtual void WriteArgumentName(string argumentName, string prefix)
        {
            Write(prefix);
            Write(argumentName);
        }

        /// <summary>
        /// Writes the name of a positional argument.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="prefix">
        /// The argument name prefix; if using <see cref="ParsingMode.LongShort"/>, this may vary
        /// depending on whether the name is a short or long name.
        /// </param>
        /// <param name="separator">
        /// The argument name/value separator, or <see langword="null"/> if the <see cref="UseWhiteSpaceValueSeparator"/>
        /// property and the <see cref="CommandLineParser.AllowWhiteSpaceValueSeparator"/> property
        /// are both <see langword="true"/>.
        /// </param>
        /// <remarks>
        /// <para>
        ///   The default implementation surrounds the value written by the <see cref="WriteArgumentName"/>
        ///   method, as well as the <paramref name="separator"/> if not <see langword="null"/>,
        ///   with square brackets. For example, "[-Name]" or "[-Name:]", to indicate the name
        ///   itself is optional.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentSyntax"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WritePositionalArgumentName(string argumentName, string prefix, char? separator)
        {
            Write(OptionalStart);
            WriteArgumentName(argumentName, prefix);
            if (separator is char separatorValue)
            {
                Write(separatorValue);
            }

            Write(OptionalEnd);
        }

        /// <summary>
        /// Writes the value description of an argument.
        /// </summary>
        /// <param name="valueDescription">The value description.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation returns the value description surrounded by angle brackets.
        ///   For example, "&lt;String&gt;".
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentSyntax"/>
        ///   method for arguments that are not switch arguments.
        /// </para>
        /// </remarks>
        protected virtual void WriteValueDescription(string valueDescription)
            => Write($"<{valueDescription}>");

        /// <summary>
        /// Writes the string used to indicate there are more arguments if the usage syntax was
        /// abbreviated.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The default implementation returns a string like "[arguments]".
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageSyntax"/>
        ///   method if the <see cref="UseAbbreviatedSyntax"/> property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        protected virtual void WriteAbbreviatedRemainingArguments()
            => Write(Resources.DefaultAbbreviatedRemainingArguments);

        /// <summary>
        /// Writes a suffix that indicates an argument is a multi-value argument.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The default implementation returns a string like "...".
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentSyntax"/>
        ///   method for arguments that are multi-value arguments.
        /// </para>
        /// </remarks>
        protected virtual void WriteMultiValueSuffix()
            => Write(Resources.DefaultArraySuffix);

        /// <summary>
        /// Writes the help messages for any <see cref="ClassValidationAttribute"/> attributes
        /// applied to the arguments class.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The base implementation writes each message on its own line, followed by a blank line.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageCore"/>
        ///   method if the <see cref="IncludeValidatorsInDescription"/> property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        protected virtual void WriteClassValidators()
        {
            Writer.Indent = 0;
            bool hasHelp = false;
            foreach (var validator in Parser.Validators)
            {
                var help = validator.GetUsageHelp(Parser);
                if (!string.IsNullOrEmpty(help))
                {
                    hasHelp = true;
                    WriteLine(help);
                }
            }

            if (hasHelp)
            {
                WriteLine(); // Blank line.
            }
        }

        /// <summary>
        /// Writes the list of argument descriptions.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The default implementation gets the list of arguments using the <see cref="GetArgumentsInDescriptionOrder"/>
        ///   method, and calls the <see cref="WriteArgumentDescription(CommandLineArgument)"/> method for each one.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageCore"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteArgumentDescriptions()
        {
            if (ArgumentDescriptionListFilter == DescriptionListFilterMode.None)
            {
                return;
            }

            if (ShouldIndent)
            {
                // For long/short mode, increase the indentation by the size of the short argument.
                Writer.Indent = ArgumentDescriptionIndent;
                if (Parser.Mode == ParsingMode.LongShort)
                {
                    Writer.Indent += Parser.ArgumentNamePrefixes[0].Length + NameSeparator.Length + 1;
                }
            }

            var arguments = GetArgumentsInDescriptionOrder();
            bool first = true;
            foreach (var argument in arguments)
            {
                if (first)
                {
                    WriteArgumentDescriptionListHeader();
                    first = false;
                }

                WriteArgumentDescription(argument);
            }
        }

        /// <summary>
        /// Writes a header before the list of argument descriptions.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The base implementation does not write anything, as a header is not used in the
        ///   default format.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentDescriptions"/>
        ///   method before the first argument.
        /// </para>
        /// </remarks>
        protected virtual void WriteArgumentDescriptionListHeader()
        {
            // Intentionally blank.
        }

        /// <summary>
        /// Writes the description of a single argument.
        /// </summary>
        /// <param name="argument">The argument</param>
        /// <remarks>
        /// <para>
        ///   The base implementation calls the <see cref="WriteArgumentDescriptionHeader"/> method,
        ///   the <see cref="WriteArgumentDescriptionBody"/> method, and then adds an extra blank
        ///   line if the <see cref="BlankLineAfterDescription"/> property is <see langword="true"/>.
        /// </para>
        /// <para>
        ///   If color is enabled, the <see cref="ArgumentDescriptionColor"/> property is used for
        ///   the first line.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentDescriptions"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteArgumentDescription(CommandLineArgument argument)
        {
            WriteArgumentDescriptionHeader(argument);
            WriteArgumentDescriptionBody(argument);

            if (BlankLineAfterDescription)
            {
                WriteLine();
            }
        }

        /// <summary>
        /// Writes the header of an argument's description, which is usually the name and value
        /// description.
        /// </summary>
        /// <param name="argument">The argument</param>
        /// <remarks>
        /// <para>
        ///   The base implementation writes the name(s), value description, and alias(es), ending
        ///   with a new line. Which elements are included can be influenced using the properties of
        ///   this class.
        /// </para>
        /// <para>
        ///   If color is enabled, the <see cref="ArgumentDescriptionColor"/> property is used.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of <see cref="WriteArgumentDescription(CommandLineArgument)"/>.
        /// </para>
        /// </remarks>
        protected virtual void WriteArgumentDescriptionHeader(CommandLineArgument argument)
        {
            Writer.ResetIndent();
            var indent = ShouldIndent ? ArgumentDescriptionIndent : 0;
            WriteSpacing(indent / 2);

            var shortPrefix = argument.Parser.ArgumentNamePrefixes[0];
            var prefix = argument.Parser.LongArgumentNamePrefix ?? shortPrefix;

            WriteColor(ArgumentDescriptionColor);
            if (argument.Parser.Mode == ParsingMode.LongShort)
            {
                if (argument.HasShortName)
                {
                    WriteArgumentNameForDescription(argument.ShortName.ToString(), shortPrefix);
                    if (argument.HasLongName)
                    {
                        Write(NameSeparator);
                    }
                }
                else
                {
                    WriteSpacing(shortPrefix.Length + NameSeparator.Length + 1);
                }

                if (argument.HasLongName)
                {
                    WriteArgumentNameForDescription(argument.ArgumentName, prefix);
                }
            }
            else
            {
                WriteArgumentNameForDescription(argument.ArgumentName, prefix);
            }

            Write(' ');
            if (argument.IsSwitch)
            {
                WriteSwitchValueDescription(argument.ValueDescription);
            }
            else
            {
                WriteValueDescriptionForDescription(argument.ValueDescription);
            }

            if (IncludeAliasInDescription)
            {
                WriteAliases(argument.Aliases, argument.ShortAliases, prefix, shortPrefix);
            }

            ResetColor();
            WriteLine();
        }

        /// <summary>
        /// Writes the body of an argument description, which is usually the description itself
        /// with any supplemental information.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation writes the description text, argument validator messages, and
        ///   the default value, followed by two new lines. Which elements are included can be
        ///   influenced using the properties of this class.
        /// </para>
        /// </remarks>
        protected virtual void WriteArgumentDescriptionBody(CommandLineArgument argument)
        {
            bool hasDescription = !string.IsNullOrEmpty(argument.Description);
            if (hasDescription)
            {
                WriteArgumentDescription(argument.Description);
            }

            if (IncludeValidatorsInDescription)
            {
                WriteArgumentValidators(argument);
            }

            if (IncludeDefaultValueInDescription && argument.DefaultValue != null)
            {
                WriteDefaultValue(argument.DefaultValue);
            }

            WriteLine();
        }

        /// <summary>
        /// Writes the name or alias of an argument for use in the argument description list.
        /// </summary>
        /// <param name="argumentName">The argument name or alias.</param>
        /// <param name="prefix">
        /// The argument name prefix; if using <see cref="ParsingMode.LongShort"/>, this may vary
        /// depending on whether the name or alias is a short or long name or alias.
        /// </param>
        /// <remarks>
        /// <para>
        ///   The default implementation returns the prefix followed by the name.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentDescription(CommandLineArgument)"/>
        ///   method and the <see cref="WriteAlias"/> method.
        /// </para>
        /// </remarks>
        protected virtual void WriteArgumentNameForDescription(string argumentName, string prefix)
        {
            Write(prefix);
            Write(argumentName);
        }

        /// <summary>
        /// Writes the value description of an argument for use in the argument description list.
        /// </summary>
        /// <param name="valueDescription">The value description.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation returns the value description surrounded by angle brackets.
        ///   For example, "&lt;String&gt;".
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentDescription(CommandLineArgument)"/>
        ///   method and by the <see cref="WriteSwitchValueDescription"/> method..
        /// </para>
        /// </remarks>
        protected virtual void WriteValueDescriptionForDescription(string valueDescription)
            => Write($"<{valueDescription}>");

        /// <summary>
        /// Writes the value description of a switch argument for use in the argument description
        /// list.
        /// </summary>
        /// <param name="valueDescription">The value description.</param>
        /// <remarks>
        /// <para>
        ///   The default implementation surrounds the value written by the <see cref="WriteValueDescriptionForDescription"/>
        ///   method with angle brackets, to indicate that it is optional.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentDescription(CommandLineArgument)"/>
        ///   method for switch arguments.
        /// </para>
        /// </remarks>
        protected virtual void WriteSwitchValueDescription(string valueDescription)
        {
            Write(OptionalStart);
            WriteValueDescriptionForDescription(valueDescription);
            Write(OptionalEnd);
        }

        /// <summary>
        /// Writes the aliases of an argument for use in the argument description list.
        /// </summary>
        /// <param name="aliases">
        /// The aliases of an argument, or the long aliases for <see cref="ParsingMode.LongShort"/>
        /// mode, or <see langword="null"/> if the argument has no (long) aliases.
        /// </param>
        /// <param name="shortAliases">
        /// The short aliases of an argument, or <see langword="null"/> if the argument has no short
        /// aliases.
        /// </param>
        /// <param name="prefix">
        /// The argument name prefix to use for the <paramref name="aliases"/>.
        /// </param>
        /// <param name="shortPrefix">
        /// The argument name prefix to use for the <paramref name="shortAliases"/>.
        /// </param>
        /// <remarks>
        /// <para>
        ///   The base implementation writes a list of the short aliases, followed by the long
        ///   aliases, surrounded by parentheses, and preceded by a single space. For example,
        ///   " (-Alias1, -Alias2)" or " (-a, -b, --alias1, --alias2)".
        /// </para>
        /// <para>
        ///   If there are no aliases at all, it writes nothing.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentDescription(CommandLineArgument)"/>
        ///   method if the <see cref="IncludeAliasInDescription"/> property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        protected virtual void WriteAliases(IEnumerable<string>? aliases, IEnumerable<char>? shortAliases, string prefix, string shortPrefix)
        {
            if (shortAliases == null && aliases == null)
            {
                return;
            }

            var count = WriteAliasHelper(shortPrefix, shortAliases, 0);
            count = WriteAliasHelper(prefix, aliases, count);

            if (count > 0)
            {
                Write(")");
            }
        }

        /// <summary>
        /// Writes a single alias for use in the argument description list.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="prefix">
        /// The argument name prefix; if using <see cref="ParsingMode.LongShort"/>, this may vary
        /// depending on whether the alias is a short or long alias.
        /// </param>
        /// <remarks>
        /// <para>
        ///   The base implementation calls the <see cref="WriteArgumentNameForDescription"/> method.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteAliases"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteAlias(string alias, string prefix)
            => WriteArgumentNameForDescription(alias, prefix);

        /// <summary>
        /// Writes the actual argument description text.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation just writes the description text.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentDescription(CommandLineArgument)"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteArgumentDescription(string description)
        {
            Write(description);
        }

        /// <summary>
        /// Writes the help message of any <see cref="ArgumentValidationAttribute"/> attributes
        /// applied to the argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation writes each message separated by a space, and preceded by a
        ///   space.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentDescription(CommandLineArgument)"/>
        ///   method if the <see cref="IncludeValidatorsInDescription"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// </remarks>
        protected virtual void WriteArgumentValidators(CommandLineArgument argument)
        {
            foreach (var validator in argument.Validators)
            {
                var help = validator.GetUsageHelp(argument);
                if (!string.IsNullOrEmpty(help))
                {
                    Write(' ');
                    Write(help);
                }
            }
        }

        /// <summary>
        /// Writes the default value of an argument.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation writes a string like " Default value: value.", including the
        ///   leading space.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteArgumentDescription(CommandLineArgument)"/>
        ///   method if the <see cref="IncludeDefaultValueInDescription"/> property is
        ///   <see langword="true"/> and the <see cref="CommandLineArgument.DefaultValue"/> property
        ///   is not <see langword="null"/>.
        /// </para>
        /// </remarks>
        protected virtual void WriteDefaultValue(object defaultValue)
            => Write(Resources.DefaultDefaultValueFormat, defaultValue);

        /// <summary>
        /// Writes a message telling to user how to get more detailed help.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The default implementation writes a message like "Run 'executable -Help' for more
        ///   information." or "Run 'executable command -Help' for more information."
        /// </para>
        /// <para>
        ///   If the <see cref="CommandLineParser.HelpArgument"/> property returns <see langword="null"/>,
        ///   nothing is written.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteParserUsageCore"/>
        ///   method if the requested help is not <see cref="UsageHelpRequest.Full"/>.
        /// </para>
        /// </remarks>
        protected virtual void WriteMoreInfoMessage()
        {
            var arg = Parser.HelpArgument;
            if (arg != null)
            {
                var name = ExecutableName;
                if (CommandName != null)
                {
                    name += " " + CommandName;
                }

                WriteLine(Resources.MoreInfoOnErrorFormat, name, arg.ArgumentNameWithPrefix);
            }
        }

        /// <summary>
        /// Gets the parser's arguments filtered according to the <see cref="ArgumentDescriptionListFilter"/>
        /// property and sorted according to the <see cref="ArgumentDescriptionListOrder"/> property.
        /// </summary>
        /// <returns>A list of filtered and sorted arguments.</returns>
        /// <remarks>
        /// <para>
        ///   Arguments that are hidden are excluded from the list.
        /// </para>
        /// </remarks>
        protected virtual IEnumerable<CommandLineArgument> GetArgumentsInDescriptionOrder()
        {
            var arguments = Parser.Arguments.Where(argument => !argument.IsHidden && ArgumentDescriptionListFilter switch
            {
                DescriptionListFilterMode.Information => argument.HasInformation(this),
                DescriptionListFilterMode.Description => !string.IsNullOrEmpty(argument.Description),
                DescriptionListFilterMode.All => true,
                _ => false,
            });

            var comparer = Parser.ArgumentNameComparison.GetComparer();

            return ArgumentDescriptionListOrder switch
            {
                DescriptionListSortMode.Alphabetical => arguments.OrderBy(arg => arg.ArgumentName, comparer),
                DescriptionListSortMode.AlphabeticalDescending => arguments.OrderByDescending(arg => arg.ArgumentName, comparer),
                DescriptionListSortMode.AlphabeticalShortName =>
                    arguments.OrderBy(arg => arg.HasShortName ? arg.ShortName.ToString() : arg.ArgumentName, comparer),
                DescriptionListSortMode.AlphabeticalShortNameDescending =>
                    arguments.OrderByDescending(arg => arg.HasShortName ? arg.ShortName.ToString() : arg.ArgumentName, comparer),
                _ => arguments,
            };
        }

        #endregion

        #region Subcommand usage

        /// <summary>
        /// Creates the usage help for a <see cref="Commands.CommandManager"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   This is the primary method used to generate usage help for the <see cref="Commands.CommandManager"/>
        ///   class. It calls into the various other methods of this class, so overriding this
        ///   method should not typically be necessary unless you wish to deviate from the order
        ///   in which usage elements are written.
        /// </para>
        /// <para>
        ///   The base implementation writes the application description, followed by the list
        ///   of commands, followed by a message indicating how to get help on a command. Which
        ///   elements are included exactly can be influenced by the properties of this class.
        /// </para>
        /// </remarks>
        protected virtual void WriteCommandListUsageCore()
        {
            if (IncludeApplicationDescriptionBeforeCommandList)
            {
                var description = CommandManager.GetApplicationDescription();
                if (description != null)
                {
                    WriteApplicationDescription(description);
                }
            }

            SetIndent(SyntaxIndent);
            WriteCommandListUsageSyntax();
            Writer.ResetIndent();
            Writer.Indent = 0;
            WriteAvailableCommandsHeader();

            WriteCommandDescriptions();

            if (IncludeCommandHelpInstruction)
            {
                var prefix = CommandManager.Options.Mode == ParsingMode.LongShort
                    ? (CommandManager.Options.LongArgumentNamePrefixOrDefault)
                    : (CommandManager.Options.ArgumentNamePrefixes?.FirstOrDefault() ?? CommandLineParser.GetDefaultArgumentNamePrefixes()[0]);

                var transform = CommandManager.Options.ArgumentNameTransformOrDefault;
                var argumentName = transform.Apply(CommandManager.Options.StringProvider.AutomaticHelpName());

                Writer.Indent = 0;
                var name = ExecutableName;
                if (CommandName != null)
                {
                    name += " " + CommandName;
                }

                WriteCommandHelpInstruction(name, prefix, argumentName);
            }
        }

        /// <summary>
        /// Writes the usage syntax for an application using subcommands.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The base implementation calls <see cref="WriteUsageSyntaxPrefix"/> and <see cref="WriteUsageSyntaxSuffix"/>.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteCommandListUsageCore"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteCommandListUsageSyntax()
        {
            WriteUsageSyntaxPrefix();
            WriteUsageSyntaxSuffix();
            WriteLine();
        }

        /// <summary>
        /// Writes a header before the list of available commands.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The base implementation writes a string like "The following commands are available:"
        ///   followed by a blank line.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteCommandListUsageCore"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteAvailableCommandsHeader()
        {
            WriteLine(Resources.DefaultAvailableCommandsHeader);
            WriteLine();
        }

        /// <summary>
        /// Writes a list of available commands.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The base implementation calls <see cref="WriteCommandDescription(CommandInfo)"/> for all commands,
        ///   except hidden commands.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteCommandListUsageCore"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteCommandDescriptions()
        {
            SetIndent(CommandDescriptionIndent);
            foreach (var command in CommandManager.GetCommands())
            {
                if (command.IsHidden)
                {
                    continue;
                }

                WriteCommandDescription(command);
            }
        }

        /// <summary>
        /// Writes the description of a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation calls the <see cref="WriteCommandDescriptionHeader"/> method,
        ///   the <see cref="WriteCommandDescriptionBody"/> method, and then adds an extra blank
        ///   line if the <see cref="BlankLineAfterCommandDescription"/> property is <see langword="true"/>.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteCommandDescriptions"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteCommandDescription(CommandInfo command)
        {
            WriteCommandDescriptionHeader(command);
            WriteCommandDescriptionBody(command);

            if (BlankLineAfterCommandDescription)
            {
                WriteLine();
            }
        }

        /// <summary>
        /// Writes the header of a command's description, which is typically the name and alias(es)
        /// of the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation writes the command's name and alias(es), using the color from
        ///   the <see cref="CommandDescriptionColor"/> property if color is enabled, followed by a
        ///   newline.
        /// </para>
        /// </remarks>
        protected virtual void WriteCommandDescriptionHeader(CommandInfo command)
        {
            Writer.ResetIndent();
            var indent = ShouldIndent ? CommandDescriptionIndent : 0;
            WriteSpacing(indent / 2);
            WriteColor(CommandDescriptionColor);
            WriteCommandName(command.Name);
            if (IncludeCommandAliasInCommandList)
            {
                WriteCommandAliases(command.Aliases);
            }

            ResetColor();
            WriteLine();
        }

        /// <summary>
        /// Writes the body of a command's description, which is typically the description of the
        /// command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation writes the command's description, followed by a newline.
        /// </para>
        /// </remarks>
        protected virtual void WriteCommandDescriptionBody(CommandInfo command)
        {
            if (command.Description != null)
            {
                WriteCommandDescription(command.Description);
                WriteLine();
            }
        }

        /// <summary>
        /// Writes the name of a command.
        /// </summary>
        /// <param name="commandName">The command name.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation just writes the name.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteCommandDescription(CommandInfo)"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteCommandName(string commandName)
            => Write(commandName);

        /// <summary>
        /// Writes the aliases of a command.
        /// </summary>
        /// <param name="aliases">The aliases.</param>
        /// <remarks>
        /// <para>
        ///   The default implementation writes a comma-separated list of aliases, preceded by a
        ///   comma.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteCommandDescription(CommandInfo)"/>
        ///   method if the <see cref="IncludeCommandAliasInCommandList"/> property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        protected virtual void WriteCommandAliases(IEnumerable<string> aliases)
        {
            foreach (var alias in aliases)
            {
                Write(NameSeparator);
                Write(alias);
            }
        }

        /// <summary>
        /// Writes the description text of a command.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation just writes the description text.
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteCommandDescription(CommandInfo)"/>
        ///   method.
        /// </para>
        /// </remarks>
        protected virtual void WriteCommandDescription(string description)
            => Write(description);

        /// <summary>
        /// Writes an instruction on how to get help on a command.
        /// </summary>
        /// <param name="name">The application and command name.</param>
        /// <param name="argumentNamePrefix">The argument name prefix for a help argument.</param>
        /// <param name="argumentName">The automatic help argument name.</param>
        /// <remarks>
        /// <para>
        ///   The base implementation writes a string like "Run 'executable command -Help' for more
        ///   information on a command."
        /// </para>
        /// <para>
        ///   This method is called by the base implementation of the <see cref="WriteCommandListUsageCore"/>
        ///   method if the <see cref="IncludeCommandHelpInstruction"/> property is <see langword="true"/>.
        ///   If that property is <see langword="true"/>, it is assumed that every command has an
        ///   argument matching the automatic help argument's name.
        /// </para>
        /// </remarks>
        protected virtual void WriteCommandHelpInstruction(string name, string argumentNamePrefix, string argumentName)
        {
            WriteLine(Resources.CommandHelpInstructionFormat, name, argumentNamePrefix, argumentName);
        }

        #endregion

        /// <summary>
        /// Writes the specified amount of spaces to the <see cref="Writer"/>.
        /// </summary>
        /// <param name="count">The number of spaces.</param>
        protected virtual void WriteSpacing(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                Write(' ');
            }
        }

        /// <summary>
        /// Writes a string to the <see cref="Writer"/>.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <remarks>
        /// <para>
        ///   This method, along with <see cref="Write(char)"/>, is called for every write by the
        ///   base implementation. Override this method if you need to apply a transformation,
        ///   like HTML encoding, to all written text.
        /// </para>
        /// </remarks>
        protected virtual void Write(string? value) => Writer.Write(value);

        /// <summary>
        /// Writes a character to the <see cref="Writer"/>.
        /// </summary>
        /// <param name="value">The character to write.</param>
        /// <remarks>
        /// <para>
        ///   This method, along with <see cref="Write(char)"/>, is called for every write by the
        ///   base implementation. Override this method if you need to apply a transformation,
        ///   like HTML encoding, to all written text.
        /// </para>
        /// </remarks>
        protected virtual void Write(char value) => Writer.Write(value);

        /// <summary>
        /// Writes a new line to the <see cref="Writer"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   This method is called for every explicit new line added by the base implementation.
        ///   Override this method if you need to apply a transformation to all newlines.
        /// </para>
        /// <note>
        ///   This method does not get called for newlines embedded in strings like argument
        ///   descriptions. Those will be part of strings passed to the <see cref="Write(string)"/>
        ///   method.
        /// </note>
        /// </remarks>
        protected virtual void WriteLine() => Writer.WriteLine();


        /// <summary>
        /// Writes a string with virtual terminal sequences only if color is enabled.
        /// </summary>
        /// <param name="color">The string containing the color formatting.</param>
        /// <remarks>
        /// <para>
        ///   The <paramref name="color"/> should contain one or more virtual terminal sequences
        ///   from the <see cref="TextFormat"/> class, or another virtual terminal sequence. It
        ///   should not contain any other characters.
        /// </para>
        /// <para>
        ///   Nothing is written if the <see cref="UseColor"/> property is <see langword="false"/>.
        /// </para>
        /// </remarks>
        protected void WriteColor(string color)
        {
            if (UseColor)
            {
                Write(color);
            }
        }

        /// <summary>
        /// Returns the color to the previous value, if color is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   Writes the value of the <see cref="ColorReset"/> property if color is enabled.
        /// </para>
        /// <para>
        ///   Nothing is written if the <see cref="UseColor"/> property is <see langword="false"/>.
        /// </para>
        /// </remarks>
        protected void ResetColor() => WriteColor(ColorReset);

        /// <summary>
        /// Sets the indentation of the <see cref="Writer"/>, only if the <see cref="ShouldIndent"/>
        /// property returns <see langword="true"/>.
        /// </summary>
        /// <param name="indent">The number of characters to use for indentation.</param>
        protected void SetIndent(int indent)
        {
            if (ShouldIndent)
            {
                Writer.Indent = indent;
            }
        }

        internal string GetArgumentUsage(CommandLineArgument argument)
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(0);
            _writer = writer;
            _parser = argument.Parser;
            if (argument.IsRequired)
            {
                WriteArgumentSyntax(argument);
            }
            else
            {
                WriteOptionalArgumentSyntax(argument);
            }

            writer.Flush();
            return writer.BaseWriter.ToString()!;
        }

        private void WriteLine(string? value)
        {
            Write(value);
            WriteLine();
        }

        private void Write(string format, object? arg0) => Write(string.Format(Writer.FormatProvider, format, arg0));

        private void WriteLine(string format, object? arg0, object? arg1)
            => WriteLine(string.Format(Writer.FormatProvider, format, arg0, arg1));

        private void WriteLine(string format, object? arg0, object? arg1, object? arg2)
            => WriteLine(string.Format(Writer.FormatProvider, format, arg0, arg1, arg2));

        private VirtualTerminalSupport? EnableColor()
        {
            if (_useColor == null && _writer == null)
            {
                var support = VirtualTerminal.EnableColor(StandardStream.Output);
                _useColor = support.IsSupported;
                return support;
            }

            return null;
        }

        private int WriteAliasHelper<T>(string prefix, IEnumerable<T>? aliases, int count)
        {
            if (aliases == null)
            {
                return count;
            }

            foreach (var alias in aliases)
            {
                if (count == 0)
                {
                    Write(" (");
                }
                else
                {
                    Write(NameSeparator);
                }

                WriteAlias(alias!.ToString()!, prefix);
                ++count;
            }

            return count;
        }

        private void WriteUsageInternal(UsageHelpRequest request = UsageHelpRequest.Full)
        {
            bool restoreColor = _useColor == null;
            bool restoreWriter = _writer == null;
            try
            {
                using var support = EnableColor();
                using var writer = DisposableWrapper.Create(_writer, LineWrappingTextWriter.ForConsoleOut);
                _writer = writer.Inner;
                Writer.ResetIndent();
                Writer.Indent = 0;
                RunOperation(request);
            }
            finally
            {
                if (restoreColor)
                {
                    _useColor = null;
                }

                if (restoreWriter)
                {
                    _writer = null;
                }
            }
        }

        private string GetUsageInternal(int maximumLineLength = 0, UsageHelpRequest request = UsageHelpRequest.Full)
        {
            var originalWriter = _writer;
            try
            {
                using var writer = LineWrappingTextWriter.ForStringWriter(maximumLineLength);
                _writer = writer;
                RunOperation(request);
                writer.Flush();
                return writer.BaseWriter.ToString()!;
            }
            finally
            {
                _writer = originalWriter;
            }
        }

        private void RunOperation(UsageHelpRequest request)
        {
            try
            {
                if (_parser == null)
                {
                    WriteCommandListUsageCore();
                }
                else
                {
                    WriteParserUsageCore(request);
                }
            }
            finally
            {
                _parser = null;
                _commandManager = null;
            }
        }
    }
}
