// Copyright (c) Sven Groot (Ookii.org) 2012
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at https://github.com/SvenGroot/ookii.commandline. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Options for the <see cref="CommandLineParser.WriteUsage(System.IO.TextWriter, int, WriteUsageOptions)"/> method.
    /// </summary>
    public sealed class WriteUsageOptions
    {
        private string _usagePrefix;
        private string _valueDescriptionFormat;
        private string _optionalArgumentFormat;
        private string _arraySuffix;
        private string _argumentDescriptionFormat;
        private string _aliasFormat;
        private string _aliasesFormat;
        private string _defaultValueFormat;

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
            IncludeAliasInCommandLine = false;
            IncludeEnumValueListInCommandLine = false;
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
        /// is "Usage: " followed by the file name of the application's entry point assembly.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The usage prefix is written before the command line syntax of the usage help, and is followed by the syntax
        ///   of the individual arguments.
        /// </para>
        /// <para>
        ///   Setting this property to <see langword="null"/> will revert it to its default value.
        /// </para>
        /// </remarks>
        public string UsagePrefix
        {
            get { return _usagePrefix ?? CommandLineParser.DefaultUsagePrefix; }
            set { _usagePrefix = value; }
        }

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
        ///   The command line syntax is a single line that consists of the usage prefix <see cref="UsagePrefix"/> followed by the
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
        /// Gets or sets the format string to use for the description of an argument.
        /// </summary>
        /// <value>
        /// The format string to use for the description of an argument; the default value is "&#160;&#160;&#160;&#160;{3}{0} {2}\n{1}{4}{5}\n" (note that it contains line breaks).
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
        ///   This format string can have four placeholders, which are used for the argument name, the description, the value description
        ///   (formatted according to the <see cref="ValueDescriptionFormat"/>), the primary argument name prefix, the default value, and
        ///   the aliases. If the format string ends in a line break, the command descriptions will be separated by a blank line (this is the default).
        /// </para>
        /// <para>
        ///   Placeholder {4} will be replaced with the default value of an argument, formatted according to the <see cref="DefaultValueFormat"/> property.
        ///   If the <see cref="IncludeDefaultValueInDescription"/> property is set to <see langword="false"/> or the property's default value is
        ///   <see langword="null"/>, this placeholder will be set to an empty string ("").
        /// </para>
        /// <para>
        ///   Placeholder {5} will be replaced with the aliases of an argument, formatted according to the <see cref="AliasFormat"/> or <see cref="AliasesFormat"/> property.
        ///   If the <see cref="IncludeAliasInDescription"/> property is set to <see langword="false"/> or the property has no aliases, this placeholder will be set
        ///   to an empty string ("").
        /// </para>
        /// <para>
        ///   Setting this property to <see langword="null"/> will revert it to its default value.
        /// </para>
        /// </remarks>
        public string ArgumentDescriptionFormat
        {
            get { return _argumentDescriptionFormat ?? Properties.Resources.DefaultArgumentDescriptionFormat; }
            set { _argumentDescriptionFormat = value; }
        }

        /// <summary>
        /// Gets or sets the number of characters by which to indent the all but the first line of argument descriptions.
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
        /// <see langword="true" /> if the alias(es) should be included in the description; otherwise, <see langword="false" />.
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
        public bool IncludeAliasInDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the alias or aliases of an argument should be included in the command line description
        /// </summary>
        /// <value>
        ///   <c>true</c> if the alias or aliases of an argument should be included in the command line description; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeAliasInCommandLine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include enum value list in command line usage text.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enum value list is to be included in command line; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeEnumValueListInCommandLine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the default value of an argument should be included in the argument description.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the default value should be included in the description; otherwise, <see langword="false" />.
        /// </value>
        /// <para>
        ///   If this property is <see langword="true"/> and an argument has a default value other than <see langword="null"/>, the default value will be formatted using
        ///   the <see cref="DefaultValueFormat"/> property, and then included in the description according to the <see cref="ArgumentDescriptionFormat"/> property.
        /// </para>
        /// <para>
        ///   For arguments with a default value of <see langword="null"/>, this property has no effect.
        /// </para>
        public bool IncludeDefaultValueInDescription { get; set; }

        /// <summary>
        /// Gets or sets the format string to use to display the alias of an argument that only has one alias.
        /// </summary>
        /// <value>
        /// The format string for the alias of an argument; the default value is " Alias: {0}." (note the leading space).
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
        /// The format string for the alias of an argument; the default value is " Aliases: {0}." (note the leading space).
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
    }
}
