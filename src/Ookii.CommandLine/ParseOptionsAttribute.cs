using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides options that alter parsing behavior for the class that the attribute is applied
    /// to.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Options can be provided in several ways; you can change the properties of the
    ///   <see cref="CommandLineParser"/> class, you can use the <see cref="ParseOptions"/> class,
    ///   or you can use the <see cref="ParseOptionsAttribute"/> attribute.
    /// </para>
    /// <para>
    ///   This attribute allows you to define your preferred parsing behavior declaritively, with
    ///   the class that provides the arguments. Apply this attribute to the class to set the
    ///   properties.
    /// </para>
    /// <para>
    ///   If you also use the <see cref="ParseOptions"/> class with one of the static <see cref="CommandLineParser.Parse{T}(string[], ParseOptions)"/>
    ///   functions, any options provided there will override the options set in this attribute.
    /// </para>
    /// <para>
    ///   If you wish to use the default options, you do not need to apply this attribute to your
    ///   class at all.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class ParseOptionsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the prefixes that can be used to specify an argument name on the command
        /// line.
        /// </summary>
        /// <value>
        /// An array of prefixes, or <see langword="null"/> to use the value of
        /// <see cref="CommandLineParser.GetDefaultArgumentNamePrefixes()"/>. The default value is
        /// <see langword="null"/>
        /// </value>
        /// <remarks>
        /// <para>
        ///   This value can be overridden by the <see cref="ParseOptions.ArgumentNamePrefixes"/>
        ///   property, or the <see cref="CommandLineParser.CommandLineParser(Type, IEnumerable{string}?, IComparer{string}?)"/>
        ///   constructor.
        /// </para>
        /// </remarks>
        public string[]? ArgumentNamePrefixes { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether argument names are treated as case
        /// sensitive.
        /// </summary>
        /// <value>
        /// <see langword="true" /> to indicate that argument names must match case exactly when
        /// specified, or <see langword="false"/> to indicate the case does not need to match.
        /// The default value is <see langword="false"/>
        /// </value>
        /// <remarks>
        /// <para>
        ///   When <see langword="true" />, the <see cref="CommandLineParser"/> will use
        ///   <see cref="StringComparer.Ordinal"/> for command line argument comparisons; otherwise,
        ///   it will use <see cref="StringComparer.OrdinalIgnoreCase" />.
        /// </para>
        /// <para>
        ///   This value can be overridden by the <see cref="ParseOptions.ArgumentNameComparer"/>
        ///   property, or the <see cref="CommandLineParser.CommandLineParser(Type, IEnumerable{string}?, IComparer{string}?)"/>
        ///   constructor.
        /// </para>
        /// </remarks>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether duplicate arguments are allowed.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if it is allowed to supply non-array arguments more than once;
        ///   otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This value can be overridden by the <see cref="ParseOptions.AllowDuplicateArguments"/>
        ///   property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineParser.AllowDuplicateArguments"/>
        public bool AllowDuplicateArguments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value of arguments may be separated from
        /// the name by white space.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if white space is allowed to separate an argument name and its
        ///   value; <see langword="false"/> if only the colon (:) is allowed. The default value is
        ///   <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This value can be overridden by the <see cref="ParseOptions.AllowWhiteSpaceValueSeparator"/>
        ///   property.
        /// </para>
        /// </remarks>
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
        /// <para>
        ///   This value can be overridden by the <see cref="ParseOptions.NameValueSeparator"/>
        ///   property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineParser.NameValueSeparator"/>
        public char NameValueSeparator { get; set; } = CommandLineParser.DefaultNameValueSeparator;

        /// <summary>
        /// Gets the string comparer to use, according to the value of the <see cref="CaseSensitive"/>
        /// property.
        /// </summary>
        /// <returns>
        /// <see cref="StringComparer.Ordinal"/> if <see cref="CaseSensitive"/> is  <see langword="true" />;
        /// otherwise, <see cref="StringComparer.OrdinalIgnoreCase"/>
        /// </returns>
        public IComparer<string> GetStringComparer()
        {
            if (CaseSensitive)
                return StringComparer.Ordinal;
            else
                return StringComparer.OrdinalIgnoreCase;
        }
    }
}
