// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Provides options for the <see cref="CommandLineParser.Parse{T}(string[], ParseOptions)"/> method
    /// and the <see cref="CommandLineParser(Type, ParseOptions?)"/> constructor.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Several options can also be specified using the <see cref="ParseOptionsAttribute"/>
    ///   attribute on the type defining the arguments. If the option is set in both in the
    ///   attribute and here, the value from the <see cref="ParseOptions"/> class will override the
    ///   value from the <see cref="ParseOptionsAttribute"/> attribute.
    /// </para>
    /// </remarks>
    public class ParseOptions
    {
        private WriteUsageOptions? _usageOptions;
        private LocalizedStringProvider? _stringProvider;

        /// <summary>
        /// Gets or sets the culture used to convert command line argument values from their string representation to the argument type.
        /// </summary>
        /// <value>
        /// The culture used to convert command line argument values from their string representation to the argument type, or
        /// <see langword="null" /> to use <see cref="CultureInfo.InvariantCulture"/>. The default value is <see langword="null"/>
        /// </value>
        /// <seealso cref="CommandLineParser.Culture"/>
        public CultureInfo? Culture { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the command line argument parsing rules to use.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="ParsingMode"/> enumeration, or <see langword="null"/>
        /// to use the value from the <see cref="ParseOptionsAttribute"/> attribute, or if that
        /// attribute is not present, <see cref="ParsingMode.Default"/>. The default value is
        /// <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.Mode"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineParser.Mode"/>
        public ParsingMode? Mode { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates how names are created for arguments that don't have
        /// an explicit name.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="NameTransform"/> enumeration, or <see langword="null"/>
        /// to use the value from the <see cref="ParseOptionsAttribute"/> attribute, or if that
        /// attribute is not present, <see cref="NameTransform.None"/>. The default value is
        /// <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If an argument doesn't have the <see cref="CommandLineArgumentAttribute.ArgumentName"/>
        ///   property set (or doesn't have an <see cref="ArgumentNameAttribute"/> attribute for
        ///   constructor parameters), the argument name is determined by taking the name of the
        ///   property, constructor parameter, or method that defines it, and applying the specified
        ///   transform.
        /// </para>
        /// <para>
        ///   The name transform will also be applied to the names of the automatically added
        ///   help and version attributes.
        /// </para>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.NameTransform"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineParser.NameTransform"/>
        /// <seealso cref="ValueDescriptionTransform"/>
        /// <seealso cref="CommandOptions.CommandNameTransform"/>
        public NameTransform? NameTransform { get; set; }

        /// <summary>
        /// Gets or sets the argument name prefixes to use when parsing the arguments.
        /// </summary>
        /// <value>
        /// The named argument switches, or <see langword="null"/> to use the values from the
        /// <see cref="ParseOptionsAttribute"/> attribute, or if not set, the default prefixes for
        /// the current platform as returned by the <see cref="CommandLineParser.GetDefaultArgumentNamePrefixes"/>
        /// method. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If the parsing mode is set to <see cref="ParsingMode.LongShort"/>, either using the
        ///   <see cref="Mode"/> property or the <see cref="ParseOptionsAttribute"/> attribute,
        ///   this property sets the short argument name prefixes. Use the<see cref="LongArgumentNamePrefix"/>
        ///   property to set the argument prefix for long names.
        /// </para>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.ArgumentNamePrefixes"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineParser.ArgumentNamePrefixes"/>

        public IEnumerable<string>? ArgumentNamePrefixes { get; set; }

        /// <summary>
        /// Gets or sets the argument name prefix to use for long argument names.
        /// </summary>
        /// <value>
        /// The long argument prefix, or <see langword="null"/> to use the value from the
        /// <see cref="ParseOptionsAttribute"/> attribute, or if not set, the default prefix from
        /// the <see cref="CommandLineParser.DefaultLongArgumentNamePrefix"/> constant. The default
        /// value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property is only used if the if the parsing mode is set to <see cref="ParsingMode.LongShort"/>,
        ///   either using the <see cref="Mode"/> property or the <see cref="ParseOptionsAttribute"/>
        ///   attribute
        /// </para>
        /// <para>
        ///   Use the <see cref="ArgumentNamePrefixes"/> to specify the prefixes for short argument
        ///   names.
        /// </para>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.LongArgumentNamePrefix"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineParser.LongArgumentNamePrefix"/>
        public string? LongArgumentNamePrefix { get; set; }

        /// <summary>
        /// Gets or set the <see cref="IComparer{T}"/> to use to compare argument names.
        /// </summary>
        /// <value>
        /// The <see cref="IComparer{T}"/> to use to compare the names of named arguments, or
        /// <see langword="null"/> to use the one determined using the <see cref="ParseOptionsAttribute.CaseSensitive"/>
        /// property, or if the <see cref="ParseOptionsAttribute"/> is not present, <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.CaseSensitive"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineParser.ArgumentNameComparer"/>
        public IComparer<string>? ArgumentNameComparer { get; set; }

        /// <summary>
        /// Gets or sets the output <see cref="TextWriter"/> used to print usage information if
        /// argument parsing fails or is canceled.
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
        ///   <see langword="true"/> if it is allowed to supply non-multi-value arguments more than once;
        ///   <see langword="false"/> if it is not allowed, or <see langword="null" /> to use the
        ///   value from the <see cref="ParseOptionsAttribute"/> attribute, or if that attribute
        ///   is not present, <see langword="false"/>. The default value is <see langword="null"/>.
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
        ///   to use the value from the <see cref="ParseOptionsAttribute"/> attribute, or if that
        ///   is not present, the <see cref="CommandLineParser.DefaultNameValueSeparator"/>
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
        ///   Do not pick a white-space character as the separator. Doing this only works if the
        ///   whitespace character is part of the argument, which usually means it needs to be
        ///   quoted or escaped when invoking your application. Instead, use the
        ///   <see cref="AllowWhiteSpaceValueSeparator"/> property to control whether white space
        ///   is allowed as a separator.
        /// </note>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.NameValueSeparator"/> property.
        /// </para>
        /// </remarks>
        public char? NameValueSeparator { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates a help argument will be automatically added.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> to automatically create a help argument; <see langword="false"/>
        ///   to not create one, or <see langword="null" /> to use the value from the <see cref="ParseOptionsAttribute"/>
        ///   attribute, or if that is not present, <see langword="true"/>. The default value is
        ///   <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is <see langword="true"/>, the <see cref="CommandLineParser"/>
        ///   will automatically add an argument with the name "Help". If using <see cref="ParsingMode.LongShort"/>,
        ///   this argument will have the short name "?" and a short alias "h"; otherwise, it
        ///   will have the aliases "?" and "h". When supplied, this argument will cancel parsing
        ///   and cause usage help to be printed.
        /// </para>
        /// <para>
        ///   If you already have an argument conflicting with the names or aliases above, the
        ///   automatic help argument will not be created even if this property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   The name, aliases and description can be customized by using a custom <see cref="StringProvider"/>.
        /// </para>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.AutoHelpArgument"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="LocalizedStringProvider.AutomaticHelpName"/>
        /// <seealso cref="LocalizedStringProvider.AutomaticHelpDescription"/>
        /// <seealso cref="LocalizedStringProvider.AutomaticHelpShortName"/>
        public bool? AutoHelpArgument { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates a version argument will be automatically added.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> to automatically create a version argument; <see langword="false"/>
        ///   to not create one, or <see langword="null" /> to use the value from the
        ///   <see cref="ParseOptionsAttribute.AutoVersionArgument"/> property, or if the
        ///   <see cref="ParseOptionsAttribute"/> is not present, <see langword="true"/>.
        ///   The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is <see langword="true"/>, the <see cref="CommandLineParser"/>
        ///   will automatically add an argument with the name "Version". When supplied, this
        ///   argument will write version information to the console and cancel parsing, without
        ///   showing usage help.
        /// </para>
        /// <para>
        ///   If you already have an argument named "Version", the automatic version argument
        ///   will not be created even if this property is <see langword="true"/>.
        /// </para>
        /// <para>
        ///   The name and description can be customized by using a custom <see cref="StringProvider"/>.
        /// </para>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the value of the
        ///   <see cref="ParseOptionsAttribute.AutoVersionArgument"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="LocalizedStringProvider.AutomaticVersionName"/>
        /// <seealso cref="LocalizedStringProvider.AutomaticVersionDescription"/>
        public bool AutoVersionArgument { get; set; } = true;

        /// <summary>
        /// Gets or sets the color applied to error messages.
        /// </summary>
        /// <value>
        ///   The virtual terminal sequence for a color. The default value is
        ///   <see cref="TextFormat.ForegroundRed"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The color will only be used if the <see cref="UseErrorColor"/> property is
        ///   <see langword="true"/>; otherwise, it will be replaced with an empty string.
        /// </para>
        /// <para>
        ///   If the string contains anything other than virtual terminal sequences, those parts
        ///   will be included in the output, but only when the <see cref="UseErrorColor"/> property is
        ///   <see langword="true"/>.
        /// </para>
        /// <para>
        ///   After the error message, the value of the <see cref="WriteUsageOptions.ColorReset"/>
        ///   property will be written to undo the color change.
        /// </para>
        /// </remarks>
        public string ErrorColor { get; set; } = TextFormat.ForegroundRed;

        /// <summary>
        /// Gets or sets a value that indicates whether error messages should use color.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> to enable color output; <see langword="false"/> to disable
        ///   color output; or <see langword="null"/> to enable it if the error output supports it.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is <see langword="null"/> and the <see cref="Error"/> property is
        ///   <see langword="null"/>, the <see cref="CommandLineParser.Parse{T}(string[], int, ParseOptions?)"/>
        ///   method and the <see cref="CommandManager"/> class will determine if color is supported
        ///   using the <see cref="VirtualTerminal.EnableColor"/> method for the standard error stream.
        /// </para>
        /// <para>
        ///   If this property is set to <see langword="true"/> explicitly, virtual terminal
        ///   sequences may be included in the output even if it's not supported, which may lead to
        ///   garbage characters appearing in the output.
        /// </para>
        /// </remarks>
        public bool? UseErrorColor { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LocalizedStringProvider"/> implementation to use to get
        /// strings for error messages and usage help.
        /// </summary>
        /// <value>
        /// An instance of a class inheriting from the <see cref="LocalizedStringProvider"/> class.
        /// The default value is an instance of the <see cref="LocalizedStringProvider"/> class
        /// itself.
        /// </value>
        /// <remarks>
        /// <para>
        ///   Set this property if you want to customize or localize error messages or usage help
        ///   strings.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineParser.StringProvider"/>
        public LocalizedStringProvider StringProvider
        {
            get => _stringProvider ??= new LocalizedStringProvider();
            set => _stringProvider = value;
        }

        /// <summary>
        /// Gets or sets a value that indicates how usage is shown after a parsing error occurred.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="UsageHelpRequest"/> enumeration. The default value
        /// is <see cref="UsageHelpRequest.Full"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If the value of this property is not <see cref="UsageHelpRequest.Full"/>, the
        ///   <see cref="CommandLineParser.Parse{T}(string[], int, ParseOptions?)"/> method and
        ///   <see cref="CommandManager"/> class will write the message returned by the
        ///   <see cref="LocalizedStringProvider.MoreInfoOnError"/> method instead of usage help.
        /// </para>
        /// </remarks>
        public UsageHelpRequest ShowUsageOnError { get; set; }

        /// <summary>
        /// Gets or sets a dictionary containing default value descriptions for types.
        /// </summary>
        /// <value>
        /// A dictionary containing default value descriptions for types, or <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The value description is a short, typically one-word description that indicates the
        ///   type of value that the user should supply. It is not the long description used to
        ///   describe the purpose of the argument.
        /// </para>
        /// <para>
        ///   If an argument doesn't have the <see cref="CommandLineArgumentAttribute.ValueDescription"/>
        ///   property set or the <see cref="ValueDescriptionAttribute"/> attribute applied, the
        ///   value description will be determined by first checking this dictionary. If the type
        ///   of the argument isn't in the dictionary, the type name is used, applying the
        ///   transformation specified by the <see cref="ValueDescriptionTransform"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineArgument.ValueDescription"/>
        public IDictionary<Type, string>? DefaultValueDescriptions { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates how value descriptions derived from type names
        /// are transformed.
        /// </summary>
        /// <value>
        /// One of the members of the <see cref="NameTransform"/> enumeration, or <see langword="null"/>
        /// to use the value from the <see cref="ParseOptionsAttribute"/> attribute, or if that is
        /// not present, <see cref="NameTransform.None"/>. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property has no effect on explicit value description specified with the
        ///   <see cref="CommandLineArgument.ValueDescription"/> property, the <see cref="ValueDescriptionAttribute"/>
        ///   attribute, or the <see cref="ParseOptions.DefaultValueDescriptions"/> property.
        /// </para>
        /// <para>
        ///   If not <see langword="null"/>, this property overrides the <see cref="ParseOptionsAttribute.ValueDescriptionTransform"/>
        ///   property.
        /// </para>
        /// </remarks>
        public NameTransform? ValueDescriptionTransform { get; set; }

        /// <summary>
        /// Gets or sets the options to use to write usage information to <see cref="Out"/> when
        /// parsing the arguments fails or is canceled.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="WriteUsageOptions"/> attribute.
        /// </value>
        public WriteUsageOptions UsageOptions
        {
            get => _usageOptions ??= new WriteUsageOptions();
            set => _usageOptions = value;
        }

        internal OptionsRestorer? EnableOutputColor()
        {
            if (Out == null)
            {
                var support = UsageOptions.EnableColor();
                if (support != null)
                {
                    return new OptionsRestorer(this, support)
                    {
                        ResetUseColor = true,
                    };
                }
            }

            return null;
        }

        internal OptionsRestorer? EnableErrorColor()
        {
            if (Error == null && UseErrorColor == null)
            {
                var support = VirtualTerminal.EnableColor(StandardStream.Error);
                UseErrorColor = support.IsSupported;
                return new OptionsRestorer(this, support)
                {
                    ResetUseErrorColor = true,
                };
            }

            return null;
        }

    }
}
