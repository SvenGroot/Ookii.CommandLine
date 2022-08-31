// Copyright (c) Sven Groot (Ookii.org)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at https://github.com/SvenGroot/ookii.commandline. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides options for the <see cref="ShellCommand.CreateShellCommand(System.Reflection.Assembly,string,string[],int,CreateShellCommandOptions)"/> method.
    /// </summary>
    public sealed class ParseOptions
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
        /// Gets or sets the argument name prefixes to use when parsing the shell command's arguments.
        /// </summary>
        /// <value>
        /// The named argument switches, or <see langword="null"/> to indicate the default prefixes for
        /// the current platform must be used. The default value is <see langword="null"/>.
        /// </value>
        public IEnumerable<string>? ArgumentNamePrefixes { get; set; }

        /// <summary>
        /// Gets or set the <see cref="IComparer{T}"/> to use to compare argument names.
        /// </summary>
        /// <value>
        /// The <see cref="IComparer{T}"/> to use to compare the names of named arguments. The default value is <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </value>
        public IComparer<string> ArgumentNameComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Gets or sets the output <see cref="TextWriter"/> used to print usage information.
        /// </summary>
        /// <value>
        /// The <see cref="TextWriter"/> used to print usage information, or <see langword="null"/>
        /// to print to the standard output stream. The default value is <see langword="null"/>.
        /// </value>
        public TextWriter? Out { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TextWriter"/> used to print error information.
        /// </summary>
        /// <value>
        /// The <see cref="TextWriter"/> used to print error information, or <see langword="null"/>
        /// to print to the standard output stream. The default value is <see langword="null"/>.
        /// </value>
        public TextWriter? Error { get; set; }

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
        public bool AllowWhiteSpaceValueSeparator { get; set; } = true;

        /// <summary>
        /// Gets or sets the character used to separate the name and the value of an argument.
        /// </summary>
        /// <value>
        ///   The character used to separate the name and the value of an argument. The default value is the
        ///   <see cref="CommandLineParser.DefaultNameValueSeparator"/> constant, a colon (:).
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
        /// </remarks>
        public char NameValueSeparator { get; set; } = CommandLineParser.DefaultNameValueSeparator;

        /// <summary>
        /// Gets or sets the options to use when parsing the shell command fails.
        /// </summary>
        /// <value>
        /// The usage options.
        /// </value>
        public WriteUsageOptions UsageOptions { get; set; } = new WriteUsageOptions();
    }
}
