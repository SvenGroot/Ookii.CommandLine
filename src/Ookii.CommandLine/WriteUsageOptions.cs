// Copyright (c) Sven Groot (Ookii.org)
using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Options for the <see cref="CommandLineParser.WriteUsage(System.IO.TextWriter, int, WriteUsageOptions)"/> method.
    /// </summary>
    public sealed class WriteUsageOptions
    {
        private string? _usagePrefixFormat;
        private string? _valueDescriptionFormat;
        private string? _optionalArgumentFormat;
        private string? _arraySuffix;
        private string? _argumentDescriptionFormat;
        private string? _longShortArgumentDescriptionFormat;
        private string? _aliasFormat;
        private string? _aliasesFormat;
        private string? _defaultValueFormat;
        private string? _argumentNamesSeparator;
        private string? _abbreviatedRemainingArguments;

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
        /// Gets or sets the prefix to use for the argument syntax; typically this contains the executable name.
        /// </summary>
        /// <value>
        /// The prefix to use  for the argument syntax; typically this contains the executable name. The default value
        /// is "{0}Usage:{1} " followed by the file name of the application's entry point assembly.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The usage prefix is written before the command line syntax of the usage help, and is followed by the syntax
        ///   of the individual arguments.
        /// </para>
        /// <para>
        ///   Setting this property to <see langword="null"/> will revert it to its default value.
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
        ///       If the <see cref="UseColor"/> property is <see langword="true"/>, the value of
        ///       the <see cref="UsagePrefixColor"/> property; otherwise, an empty string.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{1}</term>
        ///     <description>
        ///       If the <see cref="UseColor"/> property is <see langword="false"/>, the value of
        ///       the <see cref="ColorReset"/> property; otherwise, an empty string.
        ///     </description>
        ///   </item>
        /// </list>
        /// </remarks>
        public string UsagePrefixFormat
        {
            get { return _usagePrefixFormat ?? CommandLineParser.DefaultUsagePrefixFormat; }
            set { _usagePrefixFormat = value; }
        }

        /// <summary>
        /// Gets or sets the color applied to the <see cref="UsagePrefixFormat"/>.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="VirtualTerminal.TextFormat.ForegroundCyan"/>.
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
        public string UsagePrefixColor { get; set; } = VirtualTerminal.TextFormat.ForegroundCyan;

        /// <summary>
        /// Gets or sets the format string to use for the value description of an argument.
        /// </summary>
        /// <value>
        /// The format string to use for the value description of an argument; the default value is "&lt;{0}&gt;".
        /// </value>
        /// <remarks>
        /// <para>
        ///   The value description of an argument is used in the command line syntax in the usage help. For example,
        ///   the usage for an argument might look like "-sample &lt;String&gt;". In this example, "String" is the
        ///   value description, and that it is surrounded by angle brackets is the default value of the <see cref="ValueDescriptionFormat"/>
        ///   property.
        /// </para>
        /// <para>
        ///   This format string should have one placeholder, which is used for the value description of the argument.
        /// </para>
        /// <para>
        ///   Setting this property to <see langword="null"/> will revert it to its default value.
        /// </para>
        /// </remarks>
        public string ValueDescriptionFormat
        {
            get { return _valueDescriptionFormat ?? Properties.Resources.DefaultValueDescriptionFormat; }
            set { _valueDescriptionFormat = value; }
        }

        /// <summary>
        /// Gets or sets the number of characters by which to indent all except the first line of the command line syntax of the usage help.
        /// </summary>
        /// <value>
        /// The number of characters by which to indent all except the first line of the command line usage. The default value is 3.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The command line syntax is a single line that consists of the usage prefix <see cref="UsagePrefixFormat"/> followed by the
        ///   syntax of all the arguments. This indentation is used when that line exceeds the maximum line length.
        /// </para>
        /// <para>
        ///   This value is not used if the maximum line length of the <see cref="LineWrappingTextWriter"/> to which the usage
        ///   is being written is less than 30.
        /// </para>
        /// </remarks>
        public int Indent { get; set; }

        /// <summary>
        /// Gets or sets the format string to use for optional arguments and optional argument names.
        /// </summary>
        /// <value>
        /// The format string to use for optional arguments; the default value is "[{0}]".
        /// </value>
        /// <remarks>
        /// <para>
        ///   This format string is used for optional parameters in the command line syntax. For example, using the default value, an
        ///   optional parameter would be formatted as "[-sample &lt;String&gt;]". The format string is also used for positional arguments,
        ///   in which case the argument name is optional. For example, an optional positional argument would be formatted as "[[-sample] &lt;String&gt;]"
        ///   using the default value.
        /// </para>
        /// <para>
        ///   This format string should have one placeholder, which is used for the entire argument or the argument name.
        /// </para>
        /// <para>
        ///   Setting this property to <see langword="null"/> will revert it to its default value.
        /// </para>
        /// </remarks>
        public string OptionalArgumentFormat
        {
            get { return _optionalArgumentFormat ?? Properties.Resources.DefaultOptionalArgumentFormat; }
            set { _optionalArgumentFormat = value; }
        }

        /// <summary>
        /// Gets or sets the suffix to append to the name of an array argument.
        /// </summary>
        /// <value>
        /// The suffix to append to the name of an array argument; the default value is "...".
        /// </value>
        /// <remarks>
        /// <para>
        ///   An argument that has an array type can be specified multiple times. This suffix is appended to the command line syntax for
        ///   the command to indicate that it can be repeated. For example, using the default options, an optional array argument would
        ///   be formatted as "[-sample &lt;String&gt;...]"
        /// </para>
        /// <para>
        ///   Setting this property to <see langword="null"/> will revert it to its default value.
        /// </para>
        /// </remarks>
        public string ArraySuffix
        {
            get { return _arraySuffix ?? Properties.Resources.DefaultArraySuffix; }
            set { _arraySuffix = value; }
        }

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
        ///   if there are any more arguments, it will just print the value of the <see cref="AbbreviatedRemainingArguments"/>
        ///   property. The user will have to refer to the description list to see the remaining
        ///   possible arguments.
        /// </para>
        /// <para>
        ///   Use this if your application has a very large number of arguments.
        /// </para>
        /// </remarks>
        public bool UseAbbreviatedSyntax { get; set; }

        /// <summary>
        /// Gets or sets the string used to indicate there are more arguments when the
        /// <see cref="UseAbbreviatedSyntax"/> property is <see langword="true"/>.
        /// </summary>
        /// <value>
        /// The string used to indicate there are more arguments. The default value is "[arguments]".
        /// </value>
        public string AbbreviatedRemainingArguments
        {
            get => _abbreviatedRemainingArguments ?? Properties.Resources.DefaultAbbreviatedRemainingArguments;
            set => _abbreviatedRemainingArguments = value;
        }

        /// <summary>
        /// Gets or sets the format string to use for the description of an argument if the
        /// <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.Default"/>.
        /// </summary>
        /// <value>
        /// The format string to use for the description of an argument; the default value is "&#160;&#160;&#160;&#160;{3}{0} {2}{5}\n{1}{4}\n" (note that it contains line breaks).
        /// </value>
        /// <remarks>
        /// <para>
        ///   This format string is used for the detailed descriptions of the arguments, which is written after
        ///   the command line syntax.
        /// </para>
        /// <para>
        ///   The <see cref="ArgumentDescriptionIndent"/> property should be set to something appropriate for this format. For example, in
        ///   the default format, the description is indented by 8 characters.
        /// </para>
        /// <para>
        ///   This format string can have the following placeholders (any can be omitted if desired):
        /// </para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Placeholder</term>
        ///     <description>Description</description>
        ///   </listheader>
        ///   <item>
        ///     <term>{0}</term>
        ///     <description>
        ///       The argument name, including the long argument prefix. If the
        ///       argument doesn't have a long name, this will be an empty string.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{1}</term>
        ///     <description>
        ///       The description of the argument.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{2}</term>
        ///     <description>
        ///       The value description, formatted according to <see cref="ValueDescriptionFormat"/>.
        ///       For a switch argument, also formatted according to <see cref="OptionalArgumentFormat"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{3}</term>
        ///     <description>
        ///       The primary argument name prefix.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{4}</term>
        ///     <description>
        ///       If the argument has a default value, the default value formatted according to the
        ///       <see cref="DefaultValueFormat"/> property. If the argument has no default value,
        ///       or the <see cref="IncludeDefaultValueInDescription"/> property is <see langword="false"/>,
        ///       an empty string.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{5}</term>
        ///     <description>
        ///       If the argument has aliases, a list of the short and long aliases, separated by
        ///       the value of the <see cref="ArgumentNamesSeparator"/> property, formatted
        ///       according to <see cref="AliasFormat"/> if there is one alias, or <see cref="AliasesFormat"/>
        ///       if there is more than one. If the argument has no aliases, or the <see cref="IncludeAliasInDescription"/>
        ///       property is <see langword="false"/>, an empty string.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{6}</term>
        ///     <description>
        ///       If the <see cref="UseColor"/> property is <see langword="true"/>, the value of
        ///       the <see cref="ArgumentDescriptionColor"/> property; otherwise, an empty string.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{7}</term>
        ///     <description>
        ///       If the <see cref="UseColor"/> property is <see langword="false"/>, the value of
        ///       the <see cref="ColorReset"/> property; otherwise, an empty string.
        ///     </description>
        ///   </item>
        /// </list>
        /// </remarks>
        public string ArgumentDescriptionFormat
        {
            get { return _argumentDescriptionFormat ?? Properties.Resources.DefaultArgumentDescriptionFormat; }
            set { _argumentDescriptionFormat = value; }
        }

        /// <summary>
        /// Gets or sets the format string to use for the description of an argument if the
        /// <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>.
        /// </summary>
        /// <value>
        /// The format string to use for the description of an argument; the default value is
        /// "&#160;&#160;&#160;&#160;{0}{1}{2} {3}{4}\n{5}{6}\n" (note that it contains line
        /// breaks, and widths that assume the primary short name prefix is a single character).
        /// </value>
        /// <remarks>
        /// <para>
        ///   This format string is used for the detailed descriptions of the arguments, which is
        ///   written after the command line syntax. This format is used in place of <see cref="ArgumentDescriptionFormat"/>
        ///   when the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>.
        /// </para>
        /// <para>
        ///   The <see cref="LongShortArgumentDescriptionIndent"/> property should be set to
        ///   something appropriate for this format. For example, in the default format, the
        ///   description is indented by 12 characters.
        /// </para>
        /// <para>
        ///   This format string can have the following placeholders (any can be omitted if desired):
        /// </para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Placeholder</term>
        ///     <description>Description</description>
        ///   </listheader>
        ///   <item>
        ///     <term>{0}</term>
        ///     <description>
        ///       The short argument name, including the primary short argument prefix. If the
        ///       argument doesn't have a short name, this will be an empty string, or if
        ///       the <see cref="PreserveShortNameSpacing"/> property is true, an amount of spaces
        ///       equal to the length of a short name with prefix plus the length of the value of
        ///       the <see cref="ArgumentNamesSeparator"/> property.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{1}</term>
        ///     <description>
        ///       If the argument has both a short and a long name, the value of the <see cref="ArgumentNamesSeparator"/>
        ///       property; otherwise, an empty string.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{2}</term>
        ///     <description>
        ///       The long argument name, including the long argument prefix. If the
        ///       argument doesn't have a long name, this will be an empty string.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{3}</term>
        ///     <description>
        ///       The value description, formatted according to <see cref="ValueDescriptionFormat"/>.
        ///       For a switch argument, also formatted according to <see cref="OptionalArgumentFormat"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{4}</term>
        ///     <description>
        ///       If the argument has aliases, a list of the short and long aliases, separated by
        ///       the value of the <see cref="ArgumentNamesSeparator"/> property, formatted
        ///       according to <see cref="AliasFormat"/> if there is one alias, or <see cref="AliasesFormat"/>
        ///       if there is more than one. If the argument has no aliases, or the <see cref="IncludeAliasInDescription"/>
        ///       property is <see langword="false"/>, an empty string.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{5}</term>
        ///     <description>
        ///       The description of the argument.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>{6}</term>
        ///     <description>
        ///       If the argument has a default value, the default value formatted according to the
        ///       <see cref="DefaultValueFormat"/> property. If the argument has no default value,
        ///       or the <see cref="IncludeDefaultValueInDescription"/> property is <see langword="false"/>,
        ///       an empty string.
        ///     </description>
        ///   </item>
        /// </list>
        /// </remarks>
        public string LongShortArgumentDescriptionFormat
        {
            get => _longShortArgumentDescriptionFormat ?? Properties.Resources.DefaultLongShortArgumentDescriptionFormat;
            set => _longShortArgumentDescriptionFormat = value;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to reserve space for a short name even if
        /// an argument doesn't have a short name.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to output a number of spaces equal to the length of a short name
        /// with separator, plus the length of the value of the <see cref="ArgumentNamesSeparator"/>
        /// property, for the short name placeholder in the <see cref="LongShortArgumentDescriptionFormat"/>
        /// property; otherwise, <see langword="false"/> to use an empty string. The default value
        /// is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <note>
        ///   This property is only used when the <see cref="CommandLineParser.Mode"/> property is
        ///   <see cref="ParsingMode.LongShort"/>.
        /// </note>
        /// </remarks>
        public bool PreserveShortNameSpacing { get; set; } = true;

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
        ///   This property should be set to a value appropriate for the format string specified by the <see cref="ArgumentDescriptionFormat"/> property.
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
        ///   This property is used in place of tne <see cref="ArgumentDescriptionIndent"/> property
        ///   when the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>.
        /// </para>
        /// <para>
        ///   This property should be set to a value appropriate for the format string specified
        ///   by the <see cref="LongShortArgumentDescriptionFormat"/> property.
        /// </para>
        /// <para>
        ///   This value is not used if the maximum line length of the <see cref="LineWrappingTextWriter"/>
        ///   to which the usage is being written is less than 30.
        /// </para>
        /// </remarks>
        public int LongShortArgumentDescriptionIndent { get; set; } = 12;

        /// <summary>
        /// Gets or sets a value that indicates which arguments should be included.
        /// </summary>
        /// <value>
        /// One of the <see cref="DescriptionListFilterMode"/> values. The default is
        /// <see cref="DescriptionListFilterMode.Information"/>.
        /// </value>
        public DescriptionListFilterMode ArgumentDescriptionListFilter { get; set; }

        /// <summary>
        /// Gets or sets the color applied to the <see cref="ArgumentDescriptionFormat"/>.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="VirtualTerminal.TextFormat.ForegroundGreen"/>.
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
        public string ArgumentDescriptionColor { get; set; } = VirtualTerminal.TextFormat.ForegroundGreen;

        /// <summary>
        /// Gets or sets a string used to separator argument names.
        /// </summary>
        /// <value>
        /// A string used to separator argument names. The default value is ", ".
        /// </value>
        /// <remarks>
        /// <para>
        ///   The value of this property is used to separate aliases in the <see cref="AliasesFormat"/>,
        ///   property and to separate the long and short argument name in the
        ///   <see cref="LongShortArgumentDescriptionFormat"/> property.
        /// </para>
        /// </remarks>
        public string ArgumentNamesSeparator
        {
            get => _argumentNamesSeparator ?? Properties.Resources.DefaultArgumentNamesSeparator;
            set => _argumentNamesSeparator = value;
        }

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
        ///   If this property is <see langword="true"/> and an argument has one or more aliases, the aliases will be formatted using
        ///   the <see cref="AliasFormat"/> property (if there's one alias) or the <see cref="AliasesFormat"/> property (for multiple
        ///   aliases), and then included in the description according to the <see cref="ArgumentDescriptionFormat"/> property.
        /// </para>
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
        /// <para>
        ///   If this property is <see langword="true"/> and an argument has a default value other than <see langword="null"/>, the default value will be formatted using
        ///   the <see cref="DefaultValueFormat"/> property, and then included in the description according to the <see cref="ArgumentDescriptionFormat"/> property.
        /// </para>
        /// <para>
        ///   For arguments with a default value of <see langword="null"/>, this property has no effect.
        /// </para>
        public bool IncludeDefaultValueInDescription { get; set; } = true;

        /// <summary>
        /// Gets or sets the format string to use to display the alias of an argument that only has one alias.
        /// </summary>
        /// <value>
        /// The format string for the alias of an argument; the default value is " ({0})" (note the leading space).
        /// </value>
        /// <remarks>
        /// <para>
        ///   The format specified by this property is used for an argument that has exactly one alias. Aliases
        ///   are only added to the description if the <see cref="IncludeAliasInDescription"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   This format string can have one placeholder, which will be set to the alias preceded by the primary argument name prefix.
        /// </para>
        /// <para>
        ///   To modify the placement of the alias in the description of an argument, use the <see cref="ArgumentDescriptionFormat"/> property.
        /// </para>
        /// <para>
        ///   Setting this property to <see langword="null"/> will revert it to its default value.
        /// </para>
        /// </remarks>
        public string AliasFormat
        {
            get { return _aliasFormat ?? Properties.Resources.DefaultAliasFormat; }
            set { _aliasFormat = value; }
        }

        /// <summary>
        /// Gets or sets the format string to use to display the alias of an argument that only has one alias.
        /// </summary>
        /// <value>
        /// The format string for the alias of an argument; the default value is " ({0})" (note the leading space).
        /// </value>
        /// <remarks>
        /// <para>
        ///   The format specified by this property is used for an argument that has more than one alias. Aliases
        ///   are only added to the description if the <see cref="IncludeAliasInDescription"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   This format string can have one placeholder, which will be set to a comma-separated list of the aliases, each
        ///   preceded by the primary argument name prefix.
        /// </para>
        /// <para>
        ///   To modify the placement of the alias in the description of an argument, use the <see cref="ArgumentDescriptionFormat"/> property.
        /// </para>
        /// <para>
        ///   Setting this property to <see langword="null"/> will revert it to its default value.
        /// </para>
        /// </remarks>
        public string AliasesFormat
        {
            get { return _aliasesFormat ?? Properties.Resources.DefaultAliasesFormat; }
            set { _aliasesFormat = value; }
        }

        /// <summary>
        /// Gets or sets the format string to use to display the alias of an argument that only has one alias.
        /// </summary>
        /// <value>
        /// The format string for the alias of an argument; the default value is " Default value: {0}." (note the leading space).
        /// </value>
        /// <remarks>
        /// <para>
        ///   The format specified by this property is used for an argument that has a default value other than <see langword="null"/>. Default values
        ///   are only added to the description if the <see cref="IncludeDefaultValueInDescription"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   This format string can have one placeholder, which will be set to the default value.
        /// </para>
        /// <para>
        ///   To modify the placement of the default value in the description of an argument, use the <see cref="ArgumentDescriptionFormat"/> property.
        /// </para>
        /// <para>
        ///   Setting this property to <see langword="null"/> will revert it to its default value.
        /// </para>
        /// </remarks>
        public string DefaultValueFormat
        {
            get { return _defaultValueFormat ?? Properties.Resources.DefaultDefaultValueFormat; }
            set { _defaultValueFormat = value; }
        }

        /// <summary>
        /// Gets or sets the sequence used to reset color applied a usage help element.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence used to reset color. The default value is
        ///   <see cref="VirtualTerminal.TextFormat.Default"/>.
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
        public string ColorReset { get; set; } = VirtualTerminal.TextFormat.Default;

        /// <summary>
        /// Gets or sets a value that indicates whether the usage help should use color.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> to enable color output; <see langword="false"/> to disable
        ///   color output; or <see langword="null"/> to enable it if the output supports it.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is <see langword="null"/>, the <see cref="CommandLineParser.Parse{T}(string[], int, ParseOptions?)"/>,
        ///   <see cref="CommandLineParser.WriteUsageToConsole"/>, and <see cref="ShellCommand.CreateShellCommand(System.Reflection.Assembly, string[], int, CreateShellCommandOptions?)"/>
        ///   methods will enable color support if the output is not redirected, supports virtual
        ///   terminal sequences, and there is no environment variable named "NO_COLOR".
        /// </para>
        /// <para>
        ///   If this property is set to <see langword="true"/> explicitly, virtual terminal
        ///   sequences may be included in the output even if it's not supported, which may lead to
        ///   garbage characters appearing in the output.
        /// </para>
        /// </remarks>
        public bool? UseColor { get; set; }
    }
}
