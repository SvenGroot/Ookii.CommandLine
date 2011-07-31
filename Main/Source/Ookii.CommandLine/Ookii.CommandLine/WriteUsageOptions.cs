// $Id: WriteUsageOptions.cs 31 2011-06-26 11:10:04Z sgroot $
//
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

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteUsageOptions"/> class.
        /// </summary>
        public WriteUsageOptions()
        {
            UseWhiteSpaceValueSeparator = true;
            Indent = 3;
            ArgumentDescriptionIndent = 16;
        }

        /// <summary>
        /// Gets or sets the prefix to use on the first line of the usage text; typically this contains the executable name.
        /// </summary>
        /// <value>
        /// The prefix to use on the first line of the usage text; typically this contains the executable name. The default value
        /// is "Usage: " followed by the file name of the application's entry point assembly.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The usage prefix is written before the command line syntax of the usage help, and is followed by a single-line list
        ///   of all the arguments.
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
        /// The format string to use for required positional arguments; the default value is "&lt;{0}&gt;".
        /// </value>
        /// <remarks>
        /// <para>
        ///   The value description of an argument is used in the command line syntax in the usage help. For example,
        ///   the usage for an argument might look like "/sample &lt;String&gt;". In this example, "String" is the
        ///   value description, and that it is surrounded by angle brackets is the default value of the <see cref="ValueDescriptionFormat"/>
        ///   property.
        /// </para>
        /// <para>
        ///   This format string should have one placeholder, which is used for the value description of the argument.
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
        ///   optional parameter would be formatted as "[/sample &lt;String&gt;]". The format string is also used for positional arguments,
        ///   in which case the argument name is optional. For example, an optional positional argument would be formatted as "[[/sample] &lt;String&gt;]"
        ///   using the default value.
        /// </para>
        /// <para>
        ///   This format string should have one placeholder, which is used for the entire argument or the argument name.
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
        ///   be formatted as "[/sample &lt;String&gt;...]"
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
        /// The format string to use for the description of an argument; the default value is "{0,13} : {1}\n" (note that it contains a line break).
        /// </value>
        /// <remarks>
        /// <para>
        ///   This format string is used for the detailed descriptions of the arguments, which is written after
        ///   the command line syntax. For example, using the default format a command description would be written as "       sample : description\n".
        /// </para>
        /// <para>
        ///   The <see cref="ArgumentDescriptionIndent"/> property should be set to something appropriate for this format. For example, in
        ///   the default format, the description starts at an index of 16 characters, so subsequent lines should be indented by 16 characters.
        /// </para>
        /// <para>
        ///   This format string should have two placeholders, which are used for the command name and its description. If the format string ends in
        ///   a line break, the command descriptions will be separated by a blank line (this is the default).
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
        /// The number of characters by which to indent the all but the first line of argument descriptions. The default value is 16.
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
        ///   If this property is <see langword="true"/>, an argument would be formatted in the command line syntax as "/name &lt;Value&gt;" (using
        ///   default formatting), with a white space character separating the argument name and value description. If this property is <see langword="false"/>,
        ///   it would be formatted as "/name:&lt;Value&gt;", using a colon as the separator.
        /// </para>
        /// <para>
        ///   The command line syntax will only use a white space character as the value separator if both the <see cref="CommandLineParser.AllowWhiteSpaceValueSeparator"/> property
        ///   and the <see cref="UseWhiteSpaceValueSeparator"/> property are true.
        /// </para>
        /// </remarks>
        public bool UseWhiteSpaceValueSeparator { get; set; }
    }
}
