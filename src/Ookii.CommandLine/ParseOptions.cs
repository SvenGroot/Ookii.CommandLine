// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides options for the <see cref="CommandLineParser.Parse{T}(string[], ParseOptions)"/> method.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Several options can also be specified using the <see cref="ParseOptionsAttribute"/>
    ///   attribute on the type defining the arguments. If the option is set in both in the
    ///   attribute and here, the value from <see cref="ParseOptions"/> will override the value
    ///   from the <see cref="ParseOptionsAttribute"/> attribute.
    /// </para>
    /// </remarks>
    public class ParseOptions
    {
        /// <summary>
        /// Gets or sets the culture used to convert command line argument values from their string representation to the argument type.
        /// </summary>
        /// <value>
        /// The culture used to convert command line argument values from their string representation to the argument type, or
        /// <see langword="null" /> to use <see cref="CultureInfo.CurrentCulture"/>. The default value is <see langword="null"/>
        /// </value>
        public CultureInfo? Culture { get; set; }

        /// <summary>
        /// Gets or sets the argument name prefixes to use when parsing the arguments.
        /// </summary>
        /// <value>
        /// The named argument switches, or <see langword="null"/> to indicate the default prefixes for
        /// the current platform must be used. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.ArgumentNamePrefixes"/> property.
        /// </para>
        /// </remarks>
        public IEnumerable<string>? ArgumentNamePrefixes { get; set; }

        /// <summary>
        /// Gets or set the <see cref="IComparer{T}"/> to use to compare argument names.
        /// </summary>
        /// <value>
        /// The <see cref="IComparer{T}"/> to use to compare the names of named arguments. The default value is <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.CaseSensitive"/> property.
        /// </para>
        /// </remarks>
        public IComparer<string>? ArgumentNameComparer { get; set; }

        /// <summary>
        /// Gets or sets the output <see cref="TextWriter"/> used to print usage information if
        /// argument parsing fails or is cancelled.
        /// </summary>
        /// <remarks>
        /// If argument parsing is successful, nothing will be written.
        /// </remarks>
        /// <value>
        /// The <see cref="TextWriter"/> used to print usage information, or <see langword="null"/>
        /// to print to a <see cref="LineWrappingTextWriter"/> for the standard output stream
        /// (<see cref="Console.Out"/>). The default value is <see langword="null"/>.
        /// </value>
        public TextWriter? Out { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TextWriter"/> used to print error information if argument
        /// parsing fails.
        /// </summary>
        /// <remarks>
        /// If argument parsing is successful, nothing will be written.
        /// </remarks>
        /// <value>
        /// The <see cref="TextWriter"/> used to print error information, or <see langword="null"/>
        /// to print to a <see cref="LineWrappingTextWriter"/> for the standard error stream 
        /// (<see cref="Console.Error"/>). The default value is <see langword="null"/>.
        /// </value>
        public TextWriter? Error { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether duplicate arguments are allowed.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if it is allowed to supply non-array arguments more than once;
        ///   <see langword="false"/> if it is not allowed, or <see langword="null" /> to use the
        ///   value from the <see cref="ParseOptionsAttribute.AllowDuplicateArguments"/> property,
        ///   or if the <see cref="ParseOptionsAttribute"/> is not present, the default option
        ///   which is <see langword="false"/>. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.CaseSensitive"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineParser.AllowDuplicateArguments"/>
        public bool? AllowDuplicateArguments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value of arguments may be separated from the name by white space.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if white space is allowed to separate an argument name and its
        ///   value; <see langword="false"/> if only the <see cref="NameValueSeparator"/> is allowed,
        ///   or <see langword="null" /> to use the value from the <see cref="ParseOptionsAttribute.AllowWhiteSpaceValueSeparator"/>
        ///   property, or if the <see cref="ParseOptionsAttribute"/> is not present, the default
        ///   option which is <see langword="true"/>. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.AllowWhiteSpaceValueSeparator"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineParser.AllowWhiteSpaceValueSeparator"/>
        public bool? AllowWhiteSpaceValueSeparator { get; set; }

        /// <summary>
        /// Gets or sets the character used to separate the name and the value of an argument.
        /// </summary>
        /// <value>
        ///   The character used to separate the name and the value of an argument, or <see langword="null"/>
        ///   to use the value from the <see cref="ParseOptionsAttribute.NameValueSeparator" />
        ///   property, or if the <see cref="ParseOptionsAttribute"/> is not present, the default
        ///   separator which is the <see cref="CommandLineParser.DefaultNameValueSeparator"/>
        ///   constant, a colon (:). The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This character is used to separate the name and the value if both are provided as
        ///   a single argument to the application, e.g. <c>-sample:value</c> if the default value is used.
        /// </para>
        /// <note>
        ///   The character chosen here cannot be used in the name of any parameter. Therefore,
        ///   it's usually best to choose a non-alphanumeric value such as the colon or equals sign.
        ///   The character can appear in argument values (e.g. <c>-sample:foo:bar</c> is fine, in which
        ///   case the value is "foo:bar").
        /// </note>
        /// <note>
        ///   Do not pick a whitespace character as the separator. Doing this only works if the
        ///   whitespace character is part of the argument, which usually means it needs to be
        ///   quoted or escaped when invoking your application. Instead, use the
        ///   <see cref="AllowWhiteSpaceValueSeparator"/> property to control whether whitespace
        ///   is allowed as a separator.
        /// </note>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.NameValueSeparator"/> property.
        /// </para>
        /// </remarks>
        public char? NameValueSeparator { get; set; } = CommandLineParser.DefaultNameValueSeparator;

        /// <summary>
        /// Gets or sets the options to use to write usage information to <see cref="Out"/> when
        /// parsing the arguments fails or is cancelled.
        /// </summary>
        /// <value>
        /// The usage options.
        /// </value>
        public WriteUsageOptions UsageOptions { get; set; } = new WriteUsageOptions();
    }
}
