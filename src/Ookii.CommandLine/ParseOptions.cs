using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace Ookii.CommandLine;

/// <summary>
/// Provides options for the <see cref="CommandLineParser.Parse{T}(string[], ParseOptions)"/>
/// method and the <see cref="CommandLineParser(Type, ParseOptions?)"/> constructor.
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
    private UsageWriter? _usageWriter;
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
    /// Gets or sets a value that indicates whether the options follow POSIX conventions.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the options follow POSIX conventions; otherwise,
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property is provided as a convenient way to set a number of related properties that
    ///   together indicate the parser is using POSIX conventions. POSIX conventions in this case
    ///   means that parsing uses long/short mode, argument names are case sensitive, and argument
    ///   names and value descriptions use dash case (e.g. "argument-name").
    /// </para>
    /// <para>
    ///   Setting this property to <see langword="true"/> is equivalent to setting the
    ///   <see cref="Mode"/> property to <see cref="ParsingMode.LongShort"/>, the
    ///   <see cref="ArgumentNameComparison"/> property to <see cref="StringComparison.InvariantCulture"/>,
    ///   the <see cref="ArgumentNameTransform"/> property to <see cref="NameTransform.DashCase"/>,
    ///   and the <see cref="ValueDescriptionTransform"/> property to <see cref="NameTransform.DashCase"/>.
    /// </para>
    /// <para>
    ///   This property will only return <see langword="true"/> if the above properties are the
    ///   indicated values, except that <see cref="ArgumentNameComparison"/> can be any
    ///   case-sensitive comparison. It will return <see langword="false"/> for any other
    ///   combination of values, not just the ones indicated below.
    /// </para>
    /// <para>
    ///   Setting this property to <see langword="false"/> is equivalent to setting the
    ///   <see cref="Mode"/> property to <see cref="ParsingMode.Default"/>, the
    ///   <see cref="ArgumentNameComparison"/> property to <see cref="StringComparison.OrdinalIgnoreCase"/>,
    ///   the <see cref="ArgumentNameTransform"/> property to <see cref="NameTransform.None"/>,
    ///   and the <see cref="ValueDescriptionTransform"/> property to <see cref="NameTransform.None"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandOptions.IsPosix"/>
    /// <seealso cref="ParseOptionsAttribute.IsPosix"/>
    public virtual bool IsPosix
    {
        get => Mode == ParsingMode.LongShort && (ArgumentNameComparison?.IsCaseSensitive() ?? false) &&
            ArgumentNameTransform == NameTransform.DashCase && ValueDescriptionTransform == NameTransform.DashCase;
        set
        {
            if (value)
            {
                Mode = ParsingMode.LongShort;
                ArgumentNameComparison = StringComparison.InvariantCulture;
                ArgumentNameTransform = NameTransform.DashCase;
                ValueDescriptionTransform = NameTransform.DashCase;
            }
            else
            {
                Mode = ParsingMode.Default;
                ArgumentNameComparison = StringComparison.OrdinalIgnoreCase;
                ArgumentNameTransform = NameTransform.None;
                ValueDescriptionTransform = NameTransform.None;
            }
        }
    }

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
    ///   property set, the argument name is determined by taking the name of the property, or
    ///   method that defines it, and applying the specified transform.
    /// </para>
    /// <para>
    ///   The name transform will also be applied to the names of the automatically added
    ///   help and version attributes.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.ArgumentNameTransform"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="NameTransform"/>
    /// <seealso cref="ValueDescriptionTransform"/>
    /// <seealso cref="CommandOptions.CommandNameTransform"/>
    public NameTransform? ArgumentNameTransform { get; set; }

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
    /// Gets or set the type of string comparison to use for argument names.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="StringComparison"/> enumeration, or
    /// <see langword="null"/> to use the one determined using the
    /// <see cref="ParseOptionsAttribute.CaseSensitive"/> property, or if the
    /// <see cref="ParseOptionsAttribute"/> is not present,
    /// <see cref="StringComparison.OrdinalIgnoreCase"/>. The default value is
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.CaseSensitive"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.ArgumentNameComparison"/>
    public StringComparison? ArgumentNameComparison { get; set; }

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
    /// One of the values of the <see cref="ErrorMode"/> enumeration, or <see langword="null"/>
    /// to use the value from the <see cref="ParseOptionsAttribute"/> attribute, or if that
    /// attribute is not present, <see cref="ErrorMode.Error"/>. The default value is
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If set to <see cref="ErrorMode.Error"/>, supplying a non-multi-value argument more
    ///   than once will cause an exception. If set to <see cref="ErrorMode.Allow"/>, the
    ///   last value supplied will be used.
    /// </para>
    /// <para>
    ///   If set to <see cref="ErrorMode.Warning"/>, the <see cref="CommandLineParser{T}.ParseWithErrorHandling()"/>
    ///   method, the static <see cref="CommandLineParser.Parse{T}(ParseOptions?)"/> method and
    ///   the <see cref="CommandManager"/> class will print a warning to the <see cref="Error"/>
    ///   stream when a duplicate argument is found. If you are not using these methods,
    ///   <see cref="ErrorMode.Warning"/> is identical to <see cref="ErrorMode.Allow"/> and no
    ///   warning is displayed.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.DuplicateArguments"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.AllowDuplicateArguments"/>
    public ErrorMode? DuplicateArguments { get; set; }

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
    public bool? AutoVersionArgument { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether unique prefixes of an argument are automatically
    /// used as aliases.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> to automatically use unique prefixes of an argument as aliases for
    ///   that argument; <see langword="false"/> to not have automatic prefixes; otherwise,
    ///   <see langword="null" /> to use the value from the <see cref="ParseOptionsAttribute.AutoPrefixAliases"/>
    ///   property, or if the <see cref="ParseOptionsAttribute"/> attribute is not present,
    ///   <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If this property is <see langword="true"/>, the <see cref="CommandLineParser"/> class
    ///   will consider any prefix that uniquely identifies an argument by its name or one of its
    ///   explicit aliases as an alias for that argument. For example, given two arguments "Port"
    ///   and "Protocol", "Po" and "Por" would be an alias for "Port", and "Pr" an alias for
    ///   "Protocol" (as well as "Pro", "Prot", "Proto", etc.). "P" would not be an alias because it
    ///   doesn't uniquely identify a single argument.
    /// </para>
    /// <para>
    ///   When using <see cref="ParsingMode.LongShort"/>, this only applies to long names. Explicit
    ///   aliases set with the <see cref="AliasAttribute"/> take precedence over automatic aliases.
    ///   Automatic prefix aliases are not shown in the usage help.
    /// </para>
    /// <para>
    ///   This behavior is enabled unless explicitly disabled here or using the
    ///   <see cref="ParseOptionsAttribute.AutoPrefixAliases"/> property.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.AutoPrefixAliases"/> property.
    /// </para>
    /// </remarks>
    public bool? AutoPrefixAliases { get; set; }

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
    ///   After the error message, the value of the <see cref="UsageWriter.ColorReset"/>
    ///   property will be written to undo the color change.
    /// </para>
    /// </remarks>
    public string ErrorColor { get; set; } = TextFormat.ForegroundRed;

    /// <summary>
    /// Gets or sets the color applied to warning messages.
    /// </summary>
    /// <value>
    ///   The virtual terminal sequence for a color. The default value is
    ///   <see cref="TextFormat.ForegroundYellow"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The color will only be used if the <see cref="UseErrorColor"/> property is
    ///   <see langword="true"/>; otherwise, it will be replaced with an empty string.
    /// </para>
    /// <para>
    ///   This color is used for the warning emitted if the <see cref="DuplicateArguments"/>
    ///   property is <see cref="ErrorMode.Warning"/>.
    /// </para>
    /// <para>
    ///   If the string contains anything other than virtual terminal sequences, those parts
    ///   will be included in the output, but only when the <see cref="UseErrorColor"/> property is
    ///   <see langword="true"/>.
    /// </para>
    /// <para>
    ///   After the warning message, the value of the <see cref="UsageWriter.ColorReset"/>
    ///   property will be written to undo the color change.
    /// </para>
    /// </remarks>
    public string WarningColor { get; set; } = TextFormat.ForegroundYellow;

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
    ///   <see langword="null"/>, the <see cref="CommandLineParser{T}.ParseWithErrorHandling()"/>
    ///   method, the <see cref="CommandLineParser.Parse{T}(string[], int, ParseOptions?)"/>
    ///   method and the <see cref="CommandManager"/> class will determine if color is supported
    ///   using the <see cref="VirtualTerminal.EnableColor"/> method for the standard error
    ///   stream.
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
    ///   <see cref="CommandLineParser{T}.ParseWithErrorHandling()"/> method, the
    ///   <see cref="CommandLineParser.Parse{T}(string[], int, ParseOptions?)"/> method and the
    ///   <see cref="CommandManager"/> class will write the message returned by the
    ///   <see cref="UsageWriter.WriteMoreInfoMessage"/> method instead of usage help.
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
    ///   If an argument doesn't have the <see cref="ValueDescriptionAttribute"/> attribute
    ///   applied, the value description will be determined by first checking this dictionary.
    ///   If the type of the argument isn't in the dictionary, the type name is used, applying
    ///   the transformation specified by the <see cref="ValueDescriptionTransform"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.ValueDescription"/>
    public IDictionary<Type, string>? DefaultValueDescriptions { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates how value descriptions derived from type names
    /// are transformed.
    /// </summary>
    /// <value>
    /// One of the members of the <see cref="ArgumentNameTransform"/> enumeration, or <see langword="null"/>
    /// to use the value from the <see cref="ParseOptionsAttribute"/> attribute, or if that is
    /// not present, <see cref="NameTransform.None"/>. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property has no effect on explicit value description specified with the
    ///   <see cref="CommandLineArgument.ValueDescription"/> property or the
    ///   <see cref="DefaultValueDescriptions"/> property.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the <see cref="ParseOptionsAttribute.ValueDescriptionTransform"/>
    ///   property.
    /// </para>
    /// </remarks>
    public NameTransform? ValueDescriptionTransform { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="CommandLineParser"/> class
    /// will use reflection even if the command line arguments type has the
    /// <see cref="GeneratedParserAttribute"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to force the use of reflection when the arguments class has the
    /// <see cref="GeneratedParserAttribute"/> attribute; otherwise, <see langword="false"/>. The
    /// default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property only applies when you manually construct an instance of the
    ///   <see cref="CommandLineParser"/> or <see cref="CommandLineParser{T}"/> class, or use one
    ///   of the static <see cref="CommandLineParser.Parse{T}(ParseOptions?)"/> methods. If you use
    ///   the generated static <c>CreateParser</c> and <c>Parse</c> methods on the command line
    ///   arguments type, the generated parser is used regardless of the value of this property.
    /// </para>
    /// </remarks>
    public bool ForceReflection { get; set; } = ForceReflectionDefault;

    // Used by the tests so we can get coverage of the default options path while not causing
    // exceptions.
    internal static bool ForceReflectionDefault { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="UsageWriter"/> to use to create usage help.
    /// </summary>
    /// <value>
    /// An instance of the <see cref="UsageWriter"/> class.
    /// </value>
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    [AllowNull]
#endif
    public UsageWriter UsageWriter
    {
        get => _usageWriter ??= new UsageWriter();
        set => _usageWriter = value;
    }

    /// <summary>
    /// Merges the options in this instance with the options from the <see cref="ParseOptionsAttribute"/>
    /// attribute.
    /// </summary>
    /// <param name="attribute">The <see cref="ParseOptionsAttribute"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="attribute"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   For all properties that have an equivalent in the <see cref="ParseOptionsAttribute"/>,
    ///   class, if the property in this instance is <see langword="null"/>, it will be set to
    ///   the value from the <see cref="ParseOptionsAttribute"/> class.
    /// </para>
    /// </remarks>
    public void Merge(ParseOptionsAttribute attribute)
    {
        if (attribute == null)
        {
            throw new ArgumentNullException(nameof(attribute));
        }

        Mode ??= attribute.Mode;
        ArgumentNameTransform ??= attribute.ArgumentNameTransform;
        ArgumentNamePrefixes ??= attribute.ArgumentNamePrefixes;
        LongArgumentNamePrefix ??= attribute.LongArgumentNamePrefix;
        ArgumentNameComparison ??= attribute.GetStringComparison();
        DuplicateArguments ??= attribute.DuplicateArguments;
        AllowWhiteSpaceValueSeparator ??= attribute.AllowWhiteSpaceValueSeparator;
        NameValueSeparator ??= attribute.NameValueSeparator;
        AutoHelpArgument ??= attribute.AutoHelpArgument;
        AutoVersionArgument ??= attribute.AutoVersionArgument;
        AutoPrefixAliases ??= attribute.AutoPrefixAliases;
        ValueDescriptionTransform ??= attribute.ValueDescriptionTransform;
    }

    internal VirtualTerminalSupport? EnableErrorColor()
    {
        if (Error == null && UseErrorColor == null)
        {
            var support = VirtualTerminal.EnableColor(StandardStream.Error);
            UseErrorColor = support.IsSupported;
            return support;
        }

        return null;
    }
}
