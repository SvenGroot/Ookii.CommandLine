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

namespace Ookii.CommandLine;

/// <summary>
/// Creates usage help for the <see cref="CommandLineParser"/> class and the <see cref="Commands.CommandManager"/>
/// class.
/// </summary>
/// <remarks>
/// <para>
///   You can derive from this class to override the formatting of various aspects of the usage
///   help. Set the <see cref="ParseOptions.UsageWriter" qualifyHint="true"/> property to specify a custom instance.
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
/// <threadsafety static="true" instance="false"/>
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
    /// The default indentation for the argument descriptions.
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

    private readonly LineWrappingTextWriter? _customWriter;
    private LineWrappingTextWriter? _writer;
    private readonly TriState _useColor;
    private bool _autoColor;
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
    /// <see cref="TriState.True" qualifyHint="true"/> to enable color output using virtual terminal
    /// sequences; <see cref="TriState.False" qualifyHint="true"/> to disable it; or,
    /// <see cref="TriState.Auto" qualifyHint="true"/> to automatically enable it if
    /// <paramref name="writer"/> is <see langword="null"/> using the
    /// <see cref="VirtualTerminal.EnableColor" qualifyHint="true"/> method.
    /// </param>
    /// <remarks>
    /// <para>
    ///   If the <paramref name="writer"/> parameter is <see langword="null"/>, output is
    ///   written to a <see cref="LineWrappingTextWriter"/> for the standard output stream,
    ///   wrapping at the console's window width. If the stream is redirected, output may still
    ///   be wrapped, depending on the value returned by <see cref="Console.WindowWidth" qualifyHint="true"/>.
    /// </para>
    /// </remarks>
    public UsageWriter(LineWrappingTextWriter? writer = null, TriState useColor = TriState.Auto)
    {
        _customWriter = writer;
        _useColor = useColor;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the value of the <see cref="CommandLineParser.Description" qualifyHint="true"/> property
    /// is written before the syntax.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if the value of the <see cref="CommandLineParser.Description" qualifyHint="true"/> property
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
    /// Gets or sets the application executable name used in the usage help.
    /// </summary>
    /// <value>
    /// The application executable name.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Set this property to <see langword="null"/> to use the default value, determined by
    ///   calling the <see cref="CommandLineParser.GetExecutableName(bool)" qualifyHint="true"/>
    ///   method.
    /// </para>
    /// </remarks>
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
    ///   name is determined by calling <see cref="CommandLineParser.GetExecutableName(bool)" qualifyHint="true"/>,
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
    ///   <see cref="TextFormat.ForegroundCyan" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The color will only be used if the <see cref="UseColor"/> property is
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
    public TextFormat UsagePrefixColor { get; set; } = TextFormat.ForegroundCyan;

    /// <summary>
    /// Gets or sets the number of characters by which to indent all except the first line of the
    /// command line syntax of the usage help.
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
    ///   This value is used by the base implementation of the <see cref="WriteParserUsageSyntax"/>
    ///   class, unless the <see cref="ShouldIndent"/> property is <see langword="false"/>.
    /// </para>
    /// </remarks>
    public int SyntaxIndent { get; set; } = DefaultSyntaxIndent;

    /// <summary>
    /// Gets or sets a value that indicates whether the usage syntax should use short names
    /// for arguments that have one.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to use short names for arguments that have one; otherwise,
    /// <see langword="false"/> to use the long name. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <note>
    ///   This property is only used when the <see cref="CommandLineParser.Mode" qualifyHint="true"/>
    ///   property is <see cref="ParsingMode.LongShort" qualifyHint="true"/>.
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
    /// argument's description, if the <see cref="CommandLineParser.Mode" qualifyHint="true"/> property is
    /// <see cref="ParsingMode.Default" qualifyHint="true"/>.
    /// </summary>
    /// <value>
    /// The number of characters by which to indent the argument descriptions. The default
    /// value is the value of the <see cref="DefaultArgumentDescriptionIndent"/> constant.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This value is used by the base implementation of the <see cref="WriteArgumentDescriptionHeader"/>
    ///   method, unless the <see cref="ShouldIndent"/> property is <see langword="false"/>.
    /// </para>
    /// </remarks>
    public int ArgumentDescriptionIndent { get; set; } = DefaultArgumentDescriptionIndent;

    /// <summary>
    /// Gets or sets a value that indicates which arguments should be included in the list of
    /// argument descriptions.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="DescriptionListFilterMode"/> enumeration. The default
    /// value is <see cref="DescriptionListFilterMode.Information" qualifyHint="true"/>.
    /// </value>
    public DescriptionListFilterMode ArgumentDescriptionListFilter { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates the order of the arguments in the list of argument
    /// descriptions.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="DescriptionListSortMode"/> enumeration. The default
    /// value is <see cref="DescriptionListSortMode.UsageOrder" qualifyHint="true"/>.
    /// </value>
    public DescriptionListSortMode ArgumentDescriptionListOrder { get; set; }

    /// <summary>
    /// Gets or sets the color applied by the <see cref="WriteArgumentDescription(CommandLineArgument)"/> method.
    /// </summary>
    /// <value>
    ///   The virtual terminal sequence for a color. The default value is
    ///   <see cref="TextFormat.ForegroundGreen" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The color will only be used if the <see cref="UseColor"/> property is
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
    public TextFormat ArgumentDescriptionColor { get; set; } = TextFormat.ForegroundGreen;

    /// <summary>
    /// Gets or sets the color used for category headers by the
    /// <see cref="WriteArgumentCategoryHeader"/> method.
    /// </summary>
    /// <value>
    ///   The virtual terminal sequence for a color. The default value is
    ///   <see cref="TextFormat.ForegroundCyan" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The color will only be used if the <see cref="UseColor"/> property is
    ///   <see langword="true"/>.
    /// </para>
    /// <para>
    ///   The portion of the string that has color will end with the value of the 
    ///   <see cref="ColorReset"/> property.
    /// </para>
    /// </remarks>
    public TextFormat ArgumentCategoryColor { get; set; } = TextFormat.ForegroundCyan;

    /// <summary>
    /// Gets or sets a value indicating whether white space, rather than the first element of the
    /// <see cref="CommandLineParser.NameValueSeparators" qualifyHint="true"/> property, is used to
    /// separate arguments and their values in the command line syntax.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if the command line syntax uses a white space value separator;
    ///   <see langword="false"/> if it uses the first element of the <see cref="CommandLineParser.NameValueSeparators" qualifyHint="true"/>
    ///   property. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If this property is <see langword="true"/>, an argument would be formatted in the command
    ///   line syntax as "-Name &lt;Value&gt;" (using default formatting), with a white space
    ///   character separating the argument name and value description. If this property is
    ///   <see langword="false"/>, it would be formatted as "-Name:&lt;Value&gt;", using a colon as the
    ///   separator (when using the default separators).
    /// </para>
    /// <para>
    ///   The command line syntax will only use a white space character as the value separator if
    ///   both the <see cref="CommandLineParser.AllowWhiteSpaceValueSeparator" qualifyHint="true"/>
    ///   property and the <see cref="UseWhiteSpaceValueSeparator"/> property are
    ///   <see langword="true"/>.
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
    /// <para>
    ///   To exclude the default value for a particular argument only, use the
    ///   <see cref="CommandLineArgumentAttribute.IncludeDefaultInUsageHelp" qualifyHint="true"/>
    ///   property.
    /// </para>
    /// </remarks>
    public bool IncludeDefaultValueInDescription { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="ArgumentValidationAttribute"/>
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
    /// <para>
    ///   For validators derived from the <see cref="ArgumentValidationWithHelpAttribute"/> class,
    ///   you can use the <see cref="ArgumentValidationWithHelpAttribute.IncludeInUsageHelp"/>
    ///   property to exclude the help text for individual validators.
    /// </para>
    /// </remarks>
    public bool IncludeValidatorsInDescription { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="WriteArgumentDescription(CommandLineArgument)"/>
    /// method will write a blank line between arguments in the description list.
    /// </summary>
    /// <value>
    /// <see langword="true" /> to write a blank line; otherwise, <see langword="false" />. The
    /// default value is <see langword="true" />.
    /// </value>
    public bool BlankLineAfterDescription { get; set; } = true;

    /// <summary>
    /// Gets or sets the virtual terminal sequence used to undo a color change that was applied
    /// to a usage help element.
    /// </summary>
    /// <value>
    ///   The virtual terminal sequence used to reset color. The default value is
    ///   <see cref="TextFormat.Default" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property will only be used if the <see cref="UseColor"/> property is
    ///   <see langword="true"/>.
    /// </para>
    /// </remarks>
    public TextFormat ColorReset { get; set; } = TextFormat.Default;

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
    /// <para>
    ///   When nested subcommands are used with the <see cref="ParentCommand"/> class, this may be
    ///   several subcommand names separated by spaces.
    /// </para>
    /// </remarks>
    public string? CommandName { get; set; }

    /// <summary>
    /// Gets or sets a value which indicates whether a line after an empty line should have
    /// indentation.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a line after an empty line should be indented; otherwise,
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   By default, the <see cref="UsageWriter"/> class will start lines that follow an empty line
    ///   at the beginning of the line, regardless of the value of the <see cref="SyntaxIndent"/>,
    ///   <see cref="ArgumentDescriptionIndent"/>, or <see cref="CommandDescriptionIndent"/>
    ///   property. Set this property to <see langword="true"/> to apply indentation even to lines
    ///   following an empty line.
    /// </para>
    /// <para>
    ///   This can be useful if you have argument descriptions that contain blank lines when
    ///   argument descriptions are indented, such as in the default format.
    /// </para>
    /// </remarks>
    public bool IndentAfterEmptyLine { get; set; }

    /// <summary>
    /// Gets a value that indicates whether the usage help should use color.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> to use color output; otherwise, <see langword="false"/>.
    /// </value>
    protected bool UseColor => _useColor.ToBoolean(_autoColor);

    /// <summary>
    /// Gets or sets the color applied by the base implementation of the <see cref="WriteCommandDescription(CommandInfo)"/>
    /// method.
    /// </summary>
    /// <value>
    ///   The virtual terminal sequence for a color. The default value is
    ///   <see cref="TextFormat.ForegroundGreen" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The color will only be used if the <see cref="UseColor"/> property is
    ///   <see langword="true"/>.
    /// </para>
    /// <para>
    ///   The portion of the string that has color will end with the <see cref="ColorReset"/>.
    /// </para>
    /// <para>
    ///   With the default implementation, only the command name portion of the string has color;
    ///   the application name does not.
    /// </para>
    /// </remarks>
    public TextFormat CommandDescriptionColor { get; set; } = TextFormat.ForegroundGreen;

    /// <summary>
    /// Gets or sets the number of characters by which to indent the all but the first line of command descriptions.
    /// </summary>
    /// <value>
    /// The number of characters by which to indent the all but the first line of command descriptions. The default value is 8.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This value is used by the base implementation of the <see cref="WriteCommandDescriptionHeader"/>
    ///   method, unless the <see cref="ShouldIndent"/> property is <see langword="false"/>.
    /// </para>
    /// </remarks>
    public int CommandDescriptionIndent { get; set; } = DefaultCommandDescriptionIndent;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="WriteCommandDescription(CommandInfo)"/>
    /// method will write a blank line between commands in the command list.
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
    /// <see cref="TriState.Auto" qualifyHint="true"/> to show the instruction if all commands have
    /// the default help argument; <see cref="TriState.True" qualifyHint="true"/> to always show the
    /// instruction; otherwise, <see cref="TriState.False" qualifyHint="true"/>. The default value
    /// is <see cref="TriState.Auto" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If this property is <see cref="TriState.Auto" qualifyHint="true"/>, the instruction will
    ///   be shown under the following conditions:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       The <see cref="ParseOptions.AutoHelpArgument" qualifyHint="true"/> property is
    ///       <see langword="null"/> or <see langword="true"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       For every command with a <see cref="ParseOptionsAttribute"/> attribute, the
    ///       <see cref="ParseOptionsAttribute.AutoHelpArgument" qualifyHint="true"/> property is
    ///       <see langword="true"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       No command uses the <see cref="ICommandWithCustomParsing"/> interface (this includes
    ///       commands that derive from the <see cref="ParentCommand"/> class).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       No command specifies custom values for the <see cref="ParseOptionsAttribute.ArgumentNamePrefixes" qualifyHint="true"/>
    ///       and <see cref="ParseOptionsAttribute.LongArgumentNamePrefix" qualifyHint="true"/>
    ///       properties.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Every command uses the same values for the <see cref="ParseOptionsAttribute.ArgumentNameTransform" qualifyHint="true"/>
    ///       and <see cref="ParseOptionsAttribute.Mode" qualifyHint="true"/> properties.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    ///   If set to <see cref="TriState.True" qualifyHint="true"/>, the message is shown even if not
    ///   all commands meet these conditions. You can use this to show the message when you know
    ///   it's valid despite this (e.g. you have a command using <see cref="ICommandWithCustomParsing"/>
    ///   which implements its own help argument that matches the other commands).
    /// </para>
    /// <para>
    ///   To customize the message, override the <see cref="WriteCommandHelpInstruction"/> method.
    /// </para>
    /// </remarks>
    public TriState IncludeCommandHelpInstruction { get; set; }

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
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is not <see langword="null"/>,
    ///   and the specified type has a <see cref="DescriptionAttribute"/>, that description is
    ///   used instead.
    /// </para>
    /// <para>
    ///   To use a custom description, set this property to <see langword="true"/>, and override
    ///   the <see cref="WriteApplicationDescription"/> method.
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
    /// The <see cref="LineWrappingTextWriter"/> passed to the <see cref="UsageWriter(LineWrappingTextWriter?, TriState)"/>
    /// constructor, or an instance created by the <see cref="LineWrappingTextWriter.ForConsoleOut" qualifyHint="true"/>
    /// or <see cref="LineWrappingTextWriter.ForStringWriter(int, IFormatProvider?, bool)" qualifyHint="true"/>
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
    /// <value>
    /// An instance of the <see cref="CommandLineParser"/> class.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// A <see cref="WriteParserUsage"/> operation is not in progress.
    /// </exception>
    protected CommandLineParser Parser
        => _parser ?? throw new InvalidOperationException(Resources.UsageWriterPropertyNotAvailable);

    /// <summary>
    /// Gets the <see cref="Commands.CommandManager"/> that usage is being written for.
    /// </summary>
    /// <value>
    /// An instance of the <see cref="Commands.CommandManager"/> class.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// A <see cref="WriteCommandListUsage"/> operation is not in progress.
    /// </exception>
    protected CommandManager CommandManager
        => _commandManager ?? throw new InvalidOperationException(Resources.UsageWriterPropertyNotAvailable);

    /// <summary>
    /// Gets the <see cref="LocalizedStringProvider"/> implementation used to get strings for
    /// error messages and usage help.
    /// </summary>
    /// <value>
    /// An instance of a class inheriting from the <see cref="LocalizedStringProvider"/> class.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// A <see cref="WriteCommandListUsage"/> operation is not in progress.
    /// </exception>
    protected LocalizedStringProvider StringProvider
        => _parser?.StringProvider ?? _commandManager?.Options.StringProvider 
            ?? throw new InvalidOperationException(Resources.UsageWriterPropertyNotAvailable);

    /// <summary>
    /// Indicates what operation is currently in progress.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="Operation"/> enumeration.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If this property is not <see cref="Operation.ParserUsage" qualifyHint="true"/>, the <see cref="Parser"/>
    ///   property will throw an exception.
    /// </para>
    /// <para>
    ///   If this property is not <see cref="Operation.CommandListUsage" qualifyHint="true"/>, the <see cref="CommandManager"/>
    ///   property will throw an exception.
    /// </para>
    /// <para>
    ///   If this property is <see cref="Operation.None" qualifyHint="true"/>, the <see cref="Writer"/>
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
    /// Gets the separator used between multiple consecutive argument names, command names, and
    /// aliases in the usage help.
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
    ///   If no writer was passed to the <see cref="UsageWriter(LineWrappingTextWriter?, TriState)"/>
    ///   constructor, this method will create a <see cref="LineWrappingTextWriter"/> for the
    ///   standard output stream. If color usage wasn't explicitly enabled, it will be enabled
    ///   if the output supports it according to <see cref="VirtualTerminal.EnableColor" qualifyHint="true"/>.
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
    /// Creates usage help for when the user used an argument name that was a prefix alias for
    /// multiple arguments.
    /// </summary>
    /// <param name="parser">The <see cref="CommandLineParser"/>.</param>
    /// <param name="possibleMatches">The list of possible argument names or aliases.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parser"/> or <paramref name="possibleMatches"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   If no writer was passed to the <see cref="UsageWriter(LineWrappingTextWriter?, TriState)"/>
    ///   constructor, this method will create a <see cref="LineWrappingTextWriter"/> for the
    ///   standard output stream. If color usage wasn't explicitly enabled, it will be enabled
    ///   if the output supports it according to <see cref="VirtualTerminal.EnableColor" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   This method calls the <see cref="WriteParserAmbiguousPrefixAliasUsageCore"/> method to
    ///   create the usage help text.
    /// </para>
    /// </remarks>
    public void WriteParserAmbiguousPrefixAliasUsage(CommandLineParser parser, IEnumerable<string> possibleMatches)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        WriteUsageInternal(possibleMatches: possibleMatches ?? throw new ArgumentNullException(nameof(possibleMatches)));
    }

    /// <summary>
    /// Creates usage help for the specified command manager.
    /// </summary>
    /// <param name="manager">The <see cref="Commands.CommandManager" qualifyHint="true"/></param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="manager"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   The usage help will contain a list of all available commands.
    /// </para>
    /// <para>
    ///   If no writer was passed to the <see cref="UsageWriter(LineWrappingTextWriter?, TriState)"/>
    ///   constructor, this method will create a <see cref="LineWrappingTextWriter"/> for the
    ///   standard output stream. If color usage wasn't explicitly enabled, it will be enabled
    ///   if the output supports it according to <see cref="VirtualTerminal.EnableColor" qualifyHint="true"/>.
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
    /// Creates usage help for when the user used a command name that was a prefix alias for
    /// multiple commands.
    /// </summary>
    /// <param name="manager">The <see cref="Commands.CommandManager" qualifyHint="true"/></param>
    /// <param name="possibleMatches">The list of possible argument names or aliases.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="manager"/> or <paramref name="possibleMatches"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   If no writer was passed to the <see cref="UsageWriter(LineWrappingTextWriter?, TriState)"/>
    ///   constructor, this method will create a <see cref="LineWrappingTextWriter"/> for the
    ///   standard output stream. If color usage wasn't explicitly enabled, it will be enabled
    ///   if the output supports it according to <see cref="VirtualTerminal.EnableColor" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   This method calls the <see cref="WriteCommandAmbiguousPrefixAliasUsageCore"/> method to
    ///   create the usage help text.
    /// </para>
    /// </remarks>
    public void WriteCommandAmbiguousPrefixAliasUsage(CommandManager manager, IEnumerable<string> possibleMatches)
    {
        _commandManager = manager ?? throw new ArgumentNullException(nameof(manager));
        WriteUsageInternal(possibleMatches: possibleMatches ?? throw new ArgumentNullException(nameof(possibleMatches)));
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
    ///   This method ignores the writer passed to the <see cref="UsageWriter(LineWrappingTextWriter?, TriState)"/>
    ///   constructor, and will use the <see cref="LineWrappingTextWriter.ForStringWriter" qualifyHint="true"/>
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
    /// Returns a string with usage help for when the user used an argument name that was a prefix
    /// alias for multiple arguments.
    /// </summary>
    /// <returns>A string containing the usage help.</returns>
    /// <param name="parser">The <see cref="CommandLineParser"/>.</param>
    /// <param name="possibleMatches">The list of possible argument names or aliases.</param>
    /// <param name="maximumLineLength">
    /// The length at which to white-space wrap lines in the output, or 0 to disable wrapping.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parser"/> or <paramref name="possibleMatches"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   If no writer was passed to the <see cref="UsageWriter(LineWrappingTextWriter?, TriState)"/>
    ///   constructor, this method will create a <see cref="LineWrappingTextWriter"/> for the
    ///   standard output stream. If color usage wasn't explicitly enabled, it will be enabled
    ///   if the output supports it according to <see cref="VirtualTerminal.EnableColor" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   This method calls the <see cref="WriteParserAmbiguousPrefixAliasUsageCore"/> method to
    ///   create the usage help text.
    /// </para>
    /// </remarks>
    public string GetParserAmbiguousPrefixAliasUsage(CommandLineParser parser, IEnumerable<string> possibleMatches,
        int maximumLineLength = 0)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        return GetUsageInternal(maximumLineLength,
            possibleMatches: possibleMatches ?? throw new ArgumentNullException(nameof(possibleMatches)));
    }


    /// <summary>
    /// Returns a string with usage help for the specified command manager.
    /// </summary>
    /// <returns>A string containing the usage help.</returns>
    /// <param name="manager">The <see cref="Commands.CommandManager"/>.</param>
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
    ///   This method ignores the writer passed to the <see cref="UsageWriter(LineWrappingTextWriter?, TriState)"/>
    ///   constructor, and will use the <see cref="LineWrappingTextWriter.ForStringWriter" qualifyHint="true"/>
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
            WriteParserUsageFooter();
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
    /// <para>
    ///   Arguments that are hidden are excluded from the list.
    /// </para>
    /// </remarks>
    protected virtual IEnumerable<CommandLineArgument> GetArgumentsInUsageOrder() => Parser.Arguments.Where(a => !a.IsHidden);

    /// <summary>
    /// Write the prefix for the usage syntax, including the executable name and, for
    /// subcommands, the command name.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The base implementation returns a string like "Usage: executable" or "Usage: executable
    ///   command". If color is enabled, part of the string will be colored using the
    ///   <see cref="UsagePrefixColor"/> property.
    /// </para>
    /// <para>
    ///   An implementation of this method should typically include the value of the
    ///   <see cref="ExecutableName"/> property, and the value of the <see cref="CommandName"/>
    ///   property if it is not <see langword="null"/>.
    /// </para>
    /// <para>
    ///   This method is called by the base implementation of the <see cref="WriteParserUsageSyntax"/>
    ///   method and the <see cref="WriteCommandListUsageSyntax"/> method.
    /// </para>
    /// </remarks>
    protected virtual void WriteUsageSyntaxPrefix()
    {
        WriteColor(UsagePrefixColor);
        Write(StringProvider.UsageSyntaxPrefix());
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
            WriteLine(StringProvider.CommandUsageSuffix());
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

        if (argument.MultiValueInfo != null)
        {
            WriteMultiValueSuffix();
        }
    }

    /// <summary>
    /// Writes the name of an argument.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <param name="prefix">
    /// The argument name prefix; if using <see cref="ParsingMode.LongShort" qualifyHint="true"/>, this may vary
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
    /// The argument name prefix; if using <see cref="ParsingMode.LongShort" qualifyHint="true"/>, this may vary
    /// depending on whether the name is a short or long name.
    /// </param>
    /// <param name="separator">
    /// The argument name/value separator, or <see langword="null"/> if the <see cref="UseWhiteSpaceValueSeparator"/>
    /// property and the <see cref="CommandLineParser.AllowWhiteSpaceValueSeparator" qualifyHint="true"/> property
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
        => Write(StringProvider.UsageAbbreviatedRemainingArguments());

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

        Enum? previousCategory = null;
        var arguments = GetArgumentsInDescriptionOrder();
        bool first = true;
        foreach (var argument in arguments)
        {
            if (first)
            {
                WriteArgumentDescriptionListHeader();
                first = false;
            }

            if (!object.Equals(argument.Category, previousCategory))
            {
                // The default implementation for GetArgumentsInDescriptionOrder should group all
                // null categories together, but if overridden that might not be the case.
                if (argument.Category is Enum category)
                {
                    WriteArgumentCategoryHeader(category);
                }

                previousCategory = argument.Category;
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
    /// Writes a header for a category of arguments.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <remarks>
    /// <para>
    ///   The base implementation applies the <see cref="ArgumentCategoryColor"/>, and calls the
    ///   <see cref="WriteArgumentCategory"/> method to write the category description.
    /// </para>
    /// <para>
    ///   This method is called by the base implementation of the <see cref="WriteArgumentDescriptions"/>
    ///   method once before the first argument in each category. It will not be called if none
    ///   of the arguments have a category.
    /// </para>
    /// </remarks>
    protected virtual void WriteArgumentCategoryHeader(Enum category)
    {
        Writer.ResetIndent();
        WriteColor(ArgumentCategoryColor);
        WriteArgumentCategory(category);
        ResetColor();
        WriteLine();
        WriteLine();
    }

    /// <summary>
    /// Writes the description of a category of arguments.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <remarks>
    /// <para>
    ///   This method is called by the base implementation of the <see cref="WriteArgumentCategoryHeader"/>
    ///   method.
    /// </para>
    /// <para>
    ///   You can override this method if you wish to change how the category description is
    ///   determined for a category. If you want to change the formatting of the category header,
    ///   override the <see cref="WriteArgumentCategoryHeader"/> method instead.
    /// </para>
    /// </remarks>
    protected virtual void WriteArgumentCategory(Enum category)
        => Writer.Write(Parser.GetCategoryDescription(category));


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
    ///   This method is called by the base implementation of the
    ///   <see cref="WriteArgumentDescription(CommandLineArgument)"/> method.
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

        if (IncludeDefaultValueInDescription && argument.IncludeDefaultInUsageHelp && argument.DefaultValue != null)
        {
            var defaultValue = argument.DefaultValue;
            if (argument.DefaultValueFormat != null)
            {
                // Use the parser's culture so the format matches the format the user should use
                // for values.
                defaultValue = string.Format(argument.Parser.Culture, argument.DefaultValueFormat, defaultValue);
            }

            WriteDefaultValue(defaultValue);
        }

        WriteLine();
    }

    /// <summary>
    /// Writes the name or alias of an argument for use in the argument description list.
    /// </summary>
    /// <param name="argumentName">The argument name or alias.</param>
    /// <param name="prefix">
    /// The argument name prefix; if using <see cref="ParsingMode.LongShort" qualifyHint="true"/>, this may vary
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
    ///   This method is called by the base implementation of the <see cref="WriteArgumentDescriptionHeader"/>
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
    ///   method with square brackets, to indicate that it is optional.
    /// </para>
    /// <para>
    ///   This method is called by the base implementation of the <see cref="WriteArgumentDescriptionHeader"/>
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
    /// The aliases of an argument, or the long aliases for <see cref="ParsingMode.LongShort" qualifyHint="true"/>
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
    protected virtual void WriteAliases(IEnumerable<AliasAttribute>? aliases, IEnumerable<ShortAliasAttribute>? shortAliases,
        string prefix, string shortPrefix)
    {
        var hasAlias = false;
        if (shortAliases != null)
        {
            foreach (var alias in shortAliases)
            {
                if (alias.IsHidden)
                {
                    continue;
                }

                if (hasAlias)
                {
                    Write(NameSeparator);
                }
                else
                {
                    Write(" (");
                    hasAlias = true;
                }

                WriteAlias(alias.Alias.ToString(), shortPrefix);
            }
        }

        if (aliases != null)
        {
            foreach (var alias in aliases)
            {
                if (alias.IsHidden)
                {
                    continue;
                }

                if (hasAlias)
                {
                    Write(NameSeparator);
                }
                else
                {
                    Write(" (");
                    hasAlias = true;
                }

                WriteAlias(alias.Alias, prefix);
            }
        }

        if (hasAlias)
        {
            Write(")");
        }
    }

    /// <summary>
    /// Writes a single alias for use in the argument description list.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="prefix">
    /// The argument name prefix; if using <see cref="ParsingMode.LongShort" qualifyHint="true"/>, this may vary
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
    ///   This method is called by the base implementation of the <see cref="WriteArgumentDescriptionBody"/>
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
    ///   This method is called by the base implementation of the <see cref="WriteArgumentDescriptionBody"/>
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
    ///   This method is called by the base implementation of the <see cref="WriteArgumentDescriptionBody"/>
    ///   method if the <see cref="IncludeDefaultValueInDescription"/> property is
    ///   <see langword="true"/> and the <see cref="CommandLineArgument.DefaultValue" qualifyHint="true"/> property
    ///   is not <see langword="null"/>.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandLineArgumentAttribute.DefaultValueFormat" qualifyHint="true"/>
    ///   property for the argument is not <see langword="null"/>, then the base implementation of
    ///   the <see cref="WriteArgumentDescriptionBody"/> method will use the formatted string,
    ///   rather than the original default value, for the <paramref name="defaultValue"/>
    ///   parameter.
    /// </para>
    /// <para>
    ///   The default implementation formats the argument using the culture specified by the
    ///   <see cref="CommandLineParser.Culture" qualifyHint="true"/> property, rather than the
    ///   culture used by the output <see cref="Writer"/>, so that the displayed format will match
    ///   the format the user should use for argument values.
    /// </para>
    /// </remarks>
    protected virtual void WriteDefaultValue(object defaultValue)
        => Write(StringProvider.UsageDefaultValue(defaultValue, Parser.Culture));

    /// <summary>
    /// Writes a message telling to user how to get more detailed help.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The default implementation writes a message like "Run 'executable -Help' for more
    ///   information." or "Run 'executable command -Help' for more information."
    /// </para>
    /// <para>
    ///   If the <see cref="CommandLineParser.HelpArgument" qualifyHint="true"/> property returns <see langword="null"/>,
    ///   nothing is written.
    /// </para>
    /// <para>
    ///   This method is called by the base implementation of the <see cref="WriteParserUsageCore"/>
    ///   method if the requested help is not <see cref="UsageHelpRequest.Full" qualifyHint="true"/>,
    ///   and by the <see cref="WriteParserAmbiguousPrefixAliasUsageCore"/> method.
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

            WriteLine(StringProvider.UsageMoreInfoMessage(name, arg.ArgumentNameWithPrefix));
        }
    }

    /// <summary>
    /// Writes a footer under the usage help.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This method is called by the base implementation of <see cref="WriteParserUsageCore"/>
    ///   only if the requested help is <see cref="UsageHelpRequest.Full" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   The base implementation writes the value of the <see cref="CommandLineParser.UsageFooter" qualifyHint="true"/>
    ///   property, if it is not an empty string. This value can be set using the
    ///   <see cref="UsageFooterAttribute"/> attribute.
    /// </para>
    /// </remarks>
    protected virtual void WriteParserUsageFooter()
    {
        if (!string.IsNullOrEmpty(Parser.UsageFooter))
        {
            WriteLine(Parser.UsageFooter);
            WriteLine();
        }
    }

    /// <summary>
    /// Writes a list of possible matches when the user used an argument name that was a prefix
    /// alias for multiple arguments.
    /// </summary>
    /// <param name="possibleMatches">The list of possible argument names or aliases.</param>
    /// <remarks>
    /// <para>
    ///   The default implementation writes a list of possible matches, preceded by a header.
    ///   The argument names will be written using the <see cref="ArgumentDescriptionColor"/> and
    ///   by calling the <see cref="WriteArgumentNameForDescription"/> method. Finally, it calls
    ///   the <see cref="WriteMoreInfoMessage"/> method.
    /// </para>
    /// </remarks>
    protected virtual void WriteParserAmbiguousPrefixAliasUsageCore(IEnumerable<string> possibleMatches)
    {
        Writer.WriteLine(StringProvider.AmbiguousArgumentPrefixAliasMatchesHeader());
        var prefix = Parser.LongArgumentNamePrefix ?? Parser.ArgumentNamePrefixes[0];
        SetIndent(2);
        foreach (var match in possibleMatches)
        {
            WriteColor(ArgumentDescriptionColor);
            WriteArgumentNameForDescription(match, prefix);
            ResetColor();
            WriteLine();
        }

        WriteLine();
        WriteMoreInfoMessage();
    }

    /// <summary>
    /// Gets the parser's arguments filtered according to the <see cref="ArgumentDescriptionListFilter"/>
    /// property and sorted by category and according to the <see cref="ArgumentDescriptionListOrder"/>
    /// property.
    /// </summary>
    /// <returns>A list of filtered and sorted arguments.</returns>
    /// <remarks>
    /// <para>
    ///   If any of the arguments use the <see cref="CommandLineArgumentAttribute.Category" qualifyHint="true"/>
    ///   property, the arguments are sorted by category based on the category's enumeration values,
    ///   and sorted according to the <see cref="ArgumentDescriptionListOrder"/> property within
    ///   each category. Arguments that have no category set are returned before any arguments that
    ///   do have a category.
    /// </para>
    /// <para>
    ///   Arguments that are hidden are excluded from the list, even if
    ///   <see cref="DescriptionListFilterMode.All" qualifyHint="true"/> is used.
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
        }).OrderBy(argument => argument.Category);

        var comparer = Parser.ArgumentNameComparison.GetComparer();

        return ArgumentDescriptionListOrder switch
        {
            DescriptionListSortMode.Alphabetical => arguments.ThenBy(arg => arg.ArgumentName, comparer),
            DescriptionListSortMode.AlphabeticalDescending => arguments.ThenByDescending(arg => arg.ArgumentName, comparer),
            DescriptionListSortMode.AlphabeticalShortName =>
                arguments.ThenBy(arg => arg.HasShortName ? arg.ShortName.ToString() : arg.ArgumentName, comparer),
            DescriptionListSortMode.AlphabeticalShortNameDescending =>
                arguments.ThenByDescending(arg => arg.HasShortName ? arg.ShortName.ToString() : arg.ArgumentName, comparer),
            _ => arguments,
        };
    }

    #endregion

    #region Subcommand usage

    /// <summary>
    /// Creates the usage help for a <see cref="Commands.CommandManager" qualifyHint="true"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This is the primary method used to generate usage help for the <see cref="Commands.CommandManager" qualifyHint="true"/>
    ///   class. It calls into the various other methods of this class, so overriding this
    ///   method should not typically be necessary unless you wish to deviate from the order
    ///   in which usage elements are written.
    /// </para>
    /// <para>
    ///   The base implementation writes the application description, followed by the list
    ///   of commands, followed by a footer, which may include a message indicating how to get help
    ///   on a command. Which elements are included exactly can be influenced by the properties of
    ///   this class.
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
        Writer.Indent = 0;
        WriteCommandListUsageFooter();
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
        WriteLine(StringProvider.UsageAvailableCommandsHeader());
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
    ///   method and <see cref="WriteCommandAmbiguousPrefixAliasUsageCore"/> method.
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
    protected virtual void WriteCommandAliases(IEnumerable<AliasAttribute> aliases)
    {
        foreach (var alias in aliases)
        {
            if (!alias.IsHidden)
            {
                Write(NameSeparator);
                Write(alias.Alias);
            }
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
    ///   This method is called by the base implementation of the <see cref="WriteCommandDescriptionBody"/>
    ///   method.
    /// </para>
    /// </remarks>
    protected virtual void WriteCommandDescription(string description)
        => Write(description);

    /// <summary>
    /// Writes a footer underneath the command list usage.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The base implementation calls the <see cref="WriteCommandHelpInstruction"/> method if the
    ///   help instruction is explicitly or automatically enabled.
    /// </para>
    /// <para>
    ///   This method is called by the base implementation of the <see cref="WriteCommandListUsageCore"/>
    ///   method.
    /// </para>
    /// </remarks>
    protected virtual void WriteCommandListUsageFooter()
    {
        if (CheckShowCommandHelpInstruction())
        {
            var prefix = CommandManager.Options.Mode == ParsingMode.LongShort
                ? (CommandManager.Options.LongArgumentNamePrefixOrDefault)
                : (CommandManager.Options.ArgumentNamePrefixes?.FirstOrDefault() ?? CommandLineParser.GetDefaultArgumentNamePrefixes()[0]);

            var transform = CommandManager.Options.ArgumentNameTransformOrDefault;
            var argumentName = transform.Apply(CommandManager.Options.StringProvider.AutomaticHelpName());
            var name = ExecutableName;
            if (CommandName != null)
            {
                name += " " + CommandName;
            }

            WriteCommandHelpInstruction(name, prefix, argumentName);
        }
    }

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
    ///   This method is called by the base implementation of the <see cref="WriteCommandListUsageFooter"/>
    ///   method if the <see cref="IncludeCommandHelpInstruction"/> property is <see langword="true"/>,
    ///   or if it is <see langword="null"/> and all commands meet the requirements.
    /// </para>
    /// </remarks>
    protected virtual void WriteCommandHelpInstruction(string name, string argumentNamePrefix, string argumentName)
    {
        WriteLine(StringProvider.UsageCommandHelpInstruction(name, argumentNamePrefix, argumentName));
    }

    /// <summary>
    /// Writes a list of possible matches when the user used a command name that was a prefix
    /// alias for multiple commands.
    /// </summary>
    /// <param name="possibleMatches">The list of possible command names or aliases.</param>
    /// <remarks>
    /// <para>
    ///   The default implementation writes a list of possible matches, preceded by a header.
    ///   The command names will be written using the <see cref="CommandDescriptionColor"/> and
    ///   by calling the <see cref="WriteCommandName"/> method.
    /// </para>
    /// </remarks>
    protected virtual void WriteCommandAmbiguousPrefixAliasUsageCore(IEnumerable<string> possibleMatches)
    {
        Writer.WriteLine(StringProvider.AmbiguousCommandPrefixAliasMatchesHeader());
        SetIndent(2);
        foreach (var match in possibleMatches)
        {
            WriteColor(CommandDescriptionColor);
            WriteCommandName(match);
            ResetColor();
            WriteLine();
        }

        WriteLine();
        WriteCommandMoreInfoMessage();
    }

    /// <summary>
    /// Writes a message telling to user how to get more detailed help about available commands.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The default implementation writes a message like "Run 'executable' without arguments for
    ///   more information."
    /// </para>
    /// <para>
    ///   This method is called by the base implementation of the
    ///   <see cref="WriteCommandAmbiguousPrefixAliasUsageCore"/>.
    /// </para>
    /// </remarks>
    protected virtual void WriteCommandMoreInfoMessage()
    {
        var name = ExecutableName;
        if (CommandName != null)
        {
            name += " " + CommandName;
        }

        WriteLine(StringProvider.UsageCommandMoreInfoMessage(name));
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
    ///   This method, along with the <see cref="Write(char)"/> method, is called for every write by
    ///   the base implementation. Override this method if you need to apply a transformation, like
    ///   HTML encoding, to all written text.
    /// </para>
    /// </remarks>
    protected virtual void Write(string? value) => Writer.Write(value);

    /// <summary>
    /// Writes a character to the <see cref="Writer"/>.
    /// </summary>
    /// <param name="value">The character to write.</param>
    /// <remarks>
    /// <para>
    ///   This method, along with the <see cref="Write(string)"/> method, is called for every write
    ///   by the base implementation. Override this method if you need to apply a transformation,
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
    /// Writes a string to the <see cref="Writer"/>, followed by a line break.
    /// </summary>
    /// <param name="value">The string to write.</param>
    protected void WriteLine(string? value)
    {
        Write(value);
        WriteLine();
    }

    /// <summary>
    /// Writes a string with virtual terminal sequences only if color is enabled.
    /// </summary>
    /// <param name="color">The color formatting.</param>
    /// <remarks>
    /// <para>
    ///   Nothing is written if the <see cref="UseColor"/> property is <see langword="false"/>.
    /// </para>
    /// </remarks>
    protected void WriteColor(TextFormat color)
    {
        if (UseColor)
        {
            Write(color.Value);
        }
    }

    /// <summary>
    /// Returns the output color to the value before modifications, if color is enabled.
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

    private VirtualTerminalSupport? EnableColor()
    {
        if (_useColor == TriState.Auto)
        {
            VirtualTerminalSupport? support = null;
            if (_customWriter == null)
            {
                support = VirtualTerminal.EnableColor(StandardStream.Output);
            }
            else if (_customWriter.GetStandardStream() is StandardStream stream)
            {
                support = VirtualTerminal.EnableColor(stream);
            }

            _autoColor = support?.IsSupported ?? false;
            return support;
        }

        return null;
    }

    private void WriteUsageInternal(UsageHelpRequest request = UsageHelpRequest.Full, IEnumerable<string>? possibleMatches = null)
    {
        using var support = EnableColor();
        using var writer = DisposableWrapper.Create(_customWriter, LineWrappingTextWriter.ForConsoleOut);
        _writer = writer.Inner;
        Writer.ResetIndent();
        Writer.Indent = 0;
        RunOperation(request, possibleMatches);
    }

    private string GetUsageInternal(int maximumLineLength = 0, UsageHelpRequest request = UsageHelpRequest.Full, IEnumerable<string>? possibleMatches = null)
    {
        using var writer = LineWrappingTextWriter.ForStringWriter(maximumLineLength);
        _writer = writer;
        RunOperation(request, possibleMatches);
        writer.Flush();
        return writer.BaseWriter.ToString()!;
    }

    private void RunOperation(UsageHelpRequest request, IEnumerable<string>? possibleMatches)
    {
        try
        {
            Writer.IndentAfterEmptyLine = IndentAfterEmptyLine;
            if (_parser == null)
            {
                if (possibleMatches == null)
                {
                    WriteCommandListUsageCore();
                }
                else
                {
                    WriteCommandAmbiguousPrefixAliasUsageCore(possibleMatches);
                }
            }
            else
            {
                if (possibleMatches == null)
                {
                    WriteParserUsageCore(request);
                }
                else
                {
                    WriteParserAmbiguousPrefixAliasUsageCore(possibleMatches);
                }
            }
        }
        finally
        {
            _parser = null;
            _commandManager = null;
            _writer = null;
            _autoColor = false;
        }
    }

    private bool CheckShowCommandHelpInstruction()
    {
        if (IncludeCommandHelpInstruction == TriState.True)
        {
            return true;
        }

        if (IncludeCommandHelpInstruction == TriState.False)
        {
            return false;
        }

        // If not forced disabled/enabled, check requirements.
        if (CommandManager.Options.AutoHelpArgument == false)
        {
            return false;
        }

        // Options specified in ParseOptions override the ParseOptionsAttribute so those won't
        // need to be checked.
        var globalMode = CommandManager.Options.Mode != null;
        var globalNameTransform = CommandManager.Options.ArgumentNameTransform != null;
        var globalPrefixes = CommandManager.Options.ArgumentNamePrefixes != null;
        var globalLongPrefix = CommandManager.Options.LongArgumentNamePrefix != null;
        ParsingMode actualMode = default;
        ParsingMode? requiredMode = null;
        NameTransform? requiredNameTransform = null;
        bool first = true;
        foreach (var command in CommandManager.GetCommands())
        {
            if (command.UseCustomArgumentParsing)
            {
                return false;
            }

            var options = command.CommandType.GetCustomAttribute<ParseOptionsAttribute>() ?? new();
            if (first)
            {
                requiredMode ??= options.Mode;
                requiredNameTransform ??= options.ArgumentNameTransform;
                actualMode = CommandManager.Options.Mode ?? options.Mode;
                first = false;
            }

            if ((!globalMode && requiredMode != options.Mode) ||
                (!globalNameTransform && requiredNameTransform != options.ArgumentNameTransform) ||
                (!globalPrefixes && options.ArgumentNamePrefixes != null) ||
                (actualMode == ParsingMode.LongShort && !globalLongPrefix && options.LongArgumentNamePrefix != null))
            {
                return false;
            }
        }

        return true;
    }
}
