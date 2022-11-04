// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Terminal;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Options for the <see cref="CommandLineParser.WriteUsage(System.IO.TextWriter, int, WriteUsageOptions)"/> method.
    /// </summary>
    public sealed class WriteUsageOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteUsageOptions"/> class.
        /// </summary>
        public WriteUsageOptions()
        {
            UseWhiteSpaceValueSeparator = true;
            // These indent values are suitable for the default format strings.
            Indent = 3;
            ArgumentDescriptionIndent = 8;
            IncludeApplicationDescription = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value of the <see cref="CommandLineParser.Description"/> property
        /// is written before the syntax.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the value of the <see cref="CommandLineParser.Description"/> property
        ///   is written before the syntax; otherwise, <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        public bool IncludeApplicationDescription { get; set; }

        /// <summary>
        /// Gets or sets a value that overrides the default application executable name used in the
        /// usage syntax.
        /// </summary>
        /// <value>
        /// The application executable name, or <see langword="null"/> to use the default value.
        /// </value>
        /// <seealso cref="CommandLineParser.GetExecutableName(bool)"/>
        public string? ExecutableName { get; set; }

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
        ///   This property is not used if the <see cref="ExecutableName"/> property is not
        ///   <see langword="null"/>.
        /// </para>
        /// </remarks>
        public bool IncludeExecutableExtension { get; set; }

        /// <summary>
        /// Gets or sets the color applied to the <see cref="LocalizedStringProvider.UsagePrefix"/>.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="TextFormat.ForegroundCyan"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The color will only be used if the <see cref="UseColor"/> property is
        ///   <see langword="true"/>; otherwise, it will be replaced with an empty string.
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
        ///   With the default value, only the "Usage:" portion of the string has color; the
        ///   application name does not.
        /// </para>
        /// </remarks>
        public string UsagePrefixColor { get; set; } = TextFormat.ForegroundCyan;

        /// <summary>
        /// Gets or sets the number of characters by which to indent all except the first line of the command line syntax of the usage help.
        /// </summary>
        /// <value>
        /// The number of characters by which to indent all except the first line of the command line usage. The default value is 3.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The command line syntax is a single line that consists of the usage prefix returned
        ///   by <see cref="LocalizedStringProvider.UsagePrefix"/> followed by the syntax of all
        ///   the arguments. This indentation is used when that line exceeds the maximum line
        ///   length.
        /// </para>
        /// <para>
        ///   This value is not used if the maximum line length of the <see cref="LineWrappingTextWriter"/> to which the usage
        ///   is being written is less than 30.
        /// </para>
        /// </remarks>
        public int Indent { get; set; }

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
        /// Gets or sets a value that indicates whether to use a shorter version of the usage
        /// syntax.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to abbreviate the syntax; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   Abbreviated usage syntax only lists the positional arguments explicitly. After that,
        ///   if there are any more arguments, it will just print the value of the <see cref="LocalizedStringProvider.AbbreviatedRemainingArguments(bool)"/>
        ///   method. The user will have to refer to the description list to see the remaining
        ///   possible arguments.
        /// </para>
        /// <para>
        ///   Use this if your application has a very large number of arguments.
        /// </para>
        /// </remarks>
        public bool UseAbbreviatedSyntax { get; set; }

        /// <summary>
        /// Gets or sets the number of characters by which to indent the all but the first line of
        /// argument descriptions if the <see cref="CommandLineParser.Mode"/> property is
        /// <see cref="ParsingMode.Default"/>.
        /// </summary>
        /// <value>
        /// The number of characters by which to indent the all but the first line of argument descriptions. The default value is 8.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property should be set to a value appropriate for the string returned by the
        ///   <see cref="LocalizedStringProvider.ArgumentDescription"/> method, if you have overridden
        ///   that method.
        /// </para>
        /// <para>
        ///   This value is not used if the maximum line length of the <see cref="LineWrappingTextWriter"/> to which the usage
        ///   is being written is less than 30.
        /// </para>
        /// </remarks>
        public int ArgumentDescriptionIndent { get; set; }

        /// <summary>
        /// Gets or sets the number of characters by which to indent the all but the first line of
        /// argument descriptions if the <see cref="CommandLineParser.Mode"/> property is
        /// <see cref="ParsingMode.LongShort"/>.
        /// </summary>
        /// <value>
        /// The number of characters by which to indent the all but the first line of argument
        /// descriptions. The default value is 12.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property is used in place of the <see cref="ArgumentDescriptionIndent"/> property
        ///   when the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>.
        /// </para>
        /// <para>
        ///   This property should be set to a value appropriate for the string returned by the
        ///   <see cref="LocalizedStringProvider.ArgumentDescription"/> method, if you have overridden
        ///   that method.
        /// </para>
        /// <para>
        ///   This value is not used if the maximum line length of the <see cref="LineWrappingTextWriter"/>
        ///   to which the usage is being written is less than 30.
        /// </para>
        /// </remarks>
        public int LongShortArgumentDescriptionIndent { get; set; } = 12;

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
        /// Gets or sets the color applied to the <see cref="LocalizedStringProvider.ArgumentDescription"/>.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="TextFormat.ForegroundGreen"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The color will only be used if the <see cref="UseColor"/> property is
        ///   <see langword="true"/>; otherwise, it will be replaced with an empty string.
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
        ///   With the default value, only the argument name, value description and aliases
        ///   portion of the string has color; the actual argument description does not.
        /// </para>
        /// </remarks>
        public string ArgumentDescriptionColor { get; set; } = TextFormat.ForegroundGreen;

        /// <summary>
        /// Gets or sets a value indicating whether white space, rather than a colon, is used to separate named arguments and their values
        /// in the command line syntax.
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
        public bool UseWhiteSpaceValueSeparator { get; set; }

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
        /// Gets or sets the sequence used to reset color applied a usage help element.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence used to reset color. The default value is
        ///   <see cref="TextFormat.Default"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property will only be used if the <see cref="UseColor"/> property is
        ///   <see langword="true"/>; otherwise, it will be replaced with an empty string.
        /// </para>
        /// <para>
        ///   If the string contains anything other than virtual terminal sequences, those parts
        ///   will be included in the output, but only when the <see cref="UseColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// </remarks>
        public string ColorReset { get; set; } = TextFormat.Default;

        /// <summary>
        /// Gets or sets a value that indicates whether the usage help should use color.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> to enable color output; <see langword="false"/> to disable
        ///   color output; or <see langword="null"/> to enable it if the output supports it.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is <see langword="null"/>, the <see cref="CommandLineParser.Parse{T}(string[], int, ParseOptions?)"/>
        ///   and <see cref="CommandLineParser.WriteUsageToConsole"/> methods, and the <see cref="CommandManager"/>
        ///   class will determine if color is supported using the <see cref="VirtualTerminal.EnableColor"/>
        ///   method for the standard error stream.
        /// </para>
        /// <para>
        ///   If this property is set to <see langword="true"/> explicitly, virtual terminal
        ///   sequences may be included in the output even if it's not supported, which may lead to
        ///   garbage characters appearing in the output.
        /// </para>
        /// </remarks>
        public bool? UseColor { get; set; }

        internal string GetExecutableName()
        {
            return ExecutableName ?? CommandLineParser.GetExecutableName(IncludeExecutableExtension);
        }

        internal string? CommandName { get; set; }

        internal VirtualTerminalSupport? EnableColor()
        {
            if (UseColor == null)
            {
                var support = VirtualTerminal.EnableColor(StandardStream.Output);
                UseColor = support.IsSupported;
                return support;
            }

            return null;
        }
    }
}
