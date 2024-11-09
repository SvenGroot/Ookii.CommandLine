using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace Ookii.CommandLine;

/// <summary>
/// Provides options that control parsing behavior.
/// </summary>
/// <remarks>
/// <para>
///   Several options can also be specified using the <see cref="ParseOptionsAttribute"/>
///   attribute on the type defining the arguments. If the option is set in both in the
///   attribute and here, the value from the <see cref="ParseOptions"/> class will override the
///   value from the <see cref="ParseOptionsAttribute"/> attribute.
/// </para>
/// </remarks>
/// <seealso cref="CommandLineParser.Parse{T}(Ookii.CommandLine.ParseOptions?)" qualifyHint="true"/>
/// <seealso cref="CommandLineParser{T}.CommandLineParser(Ookii.CommandLine.ParseOptions?)"/>
/// <seealso cref="IParser{TSelf}.Parse(Ookii.CommandLine.ParseOptions?)"/>
/// <seealso cref="IParserProvider{TSelf}.CreateParser(Ookii.CommandLine.ParseOptions?)"/>
/// <seealso cref="CommandOptions"/>
/// <threadsafety static="true" instance="false"/>
public class ParseOptions
{
    private CultureInfo? _culture;
    private UsageWriter? _usageWriter;
    private LocalizedStringProvider? _stringProvider;

    /// <summary>
    /// Gets or sets the culture used to convert command line argument values from their string
    /// representation to the argument type.
    /// </summary>
    /// <value>
    /// The culture used to convert command line argument values. The default value is
    /// <see cref="CultureInfo.InvariantCulture" qualifyHint="true"/>.
    /// </value>
    /// <seealso cref="CommandLineParser.Culture" qualifyHint="true"/>
#if NET6_0_OR_GREATER
    [AllowNull]
#endif
    public CultureInfo Culture
    {
        get => _culture ?? CultureInfo.InvariantCulture;
        set => _culture = value;
    }

    /// <summary>
    /// Gets or sets a value that indicates the command line argument parsing rules to use.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="ParsingMode"/> enumeration, or <see langword="null"/>
    /// to use the value from the <see cref="ParseOptionsAttribute"/> attribute, or if that
    /// attribute is not present, <see cref="ParsingMode.Default" qualifyHint="true"/>. The default value is
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.Mode" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.Mode" qualifyHint="true"/>
    public ParsingMode? Mode { get; set; }

    /// <summary>
    /// Gets a value that indicates the command line argument parsing rules to use.
    /// </summary>
    /// <value>
    /// The value of the <see cref="Mode"/> property, or <see cref="ParsingMode.Default" qualifyHint="true"/>
    /// if that property is <see langword="null"/>.
    /// </value>
    public ParsingMode ModeOrDefault => Mode ?? ParsingMode.Default;

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
    ///   <see cref="Mode"/> property to <see cref="ParsingMode.LongShort" qualifyHint="true"/>, the
    ///   <see cref="ArgumentNameComparison"/> property to <see cref="StringComparison.InvariantCulture" qualifyHint="true"/>,
    ///   the <see cref="ArgumentNameTransform"/> property to <see cref="NameTransform.DashCase" qualifyHint="true"/>,
    ///   and the <see cref="ValueDescriptionTransform"/> property to <see cref="NameTransform.DashCase" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   This property will only return <see langword="true"/> if the above properties are the
    ///   indicated values, except that <see cref="ArgumentNameComparison"/> can be any
    ///   case-sensitive comparison. It will return <see langword="false"/> for any other
    ///   combination of values, not just the ones indicated below.
    /// </para>
    /// <para>
    ///   Setting this property to <see langword="false"/> is equivalent to setting the
    ///   <see cref="Mode"/> property to <see cref="ParsingMode.Default" qualifyHint="true"/>, the
    ///   <see cref="ArgumentNameComparison"/> property to <see cref="StringComparison.OrdinalIgnoreCase" qualifyHint="true"/>,
    ///   the <see cref="ArgumentNameTransform"/> property to <see cref="NameTransform.None" qualifyHint="true"/>,
    ///   and the <see cref="ValueDescriptionTransform"/> property to <see cref="NameTransform.None" qualifyHint="true"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandOptions.IsPosix" qualifyHint="true"/>
    /// <seealso cref="ParseOptionsAttribute.IsPosix" qualifyHint="true"/>
    public virtual bool IsPosix
    {
        get => Mode == ParsingMode.LongShort && ArgumentNameComparisonOrDefault.IsCaseSensitive() &&
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
    /// attribute is not present, <see cref="NameTransform.None" qualifyHint="true"/>. The default value is
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If an argument doesn't have the <see cref="CommandLineArgumentAttribute.ArgumentName" qualifyHint="true"/>
    ///   property set, the argument name is determined by taking the name of the property or
    ///   method that defines it, and applying the specified transformation.
    /// </para>
    /// <para>
    ///   The name transform will also be applied to the names of the automatically added
    ///   help and version attributes.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.ArgumentNameTransform" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="NameTransform"/>
    /// <seealso cref="ValueDescriptionTransform"/>
    /// <seealso cref="CommandOptions.CommandNameTransform" qualifyHint="true"/>
    public NameTransform? ArgumentNameTransform { get; set; }

    /// <summary>
    /// Gets a value that indicates how names are created for arguments that don't have an explicit
    /// name.
    /// </summary>
    /// <value>
    /// The value of the <see cref="ArgumentNameTransform"/> property, or <see cref="NameTransform.None" qualifyHint="true"/>
    /// if that property is <see langword="null"/>.
    /// </value>
    public NameTransform ArgumentNameTransformOrDefault => ArgumentNameTransform ?? NameTransform.None;

    /// <summary>
    /// Gets or sets the argument name prefixes to use when parsing the arguments.
    /// </summary>
    /// <value>
    /// The named argument switches, or <see langword="null"/> to use the values from the
    /// <see cref="ParseOptionsAttribute"/> attribute, or if not set, the default prefixes for
    /// the current platform as returned by the <see cref="CommandLineParser.GetDefaultArgumentNamePrefixes" qualifyHint="true"/>
    /// method. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the parsing mode is set to <see cref="ParsingMode.LongShort" qualifyHint="true"/>,
    ///   either using the <see cref="Mode"/> property or the <see cref="ParseOptionsAttribute.ArgumentNamePrefixes" qualifyHint="true"/>
    ///   property, this property sets the short argument name prefixes. Use the <see cref="LongArgumentNamePrefix"/>
    ///   property to set the argument prefix for long names.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.ArgumentNamePrefixes" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.ArgumentNamePrefixes" qualifyHint="true"/>

    public IEnumerable<string>? ArgumentNamePrefixes { get; set; }

    /// <summary>
    /// Gets the argument name prefixes to use when parsing the arguments.
    /// </summary>
    /// <value>
    /// The value of the <see cref="ArgumentNamePrefixes"/> property, or the return value of the
    /// <see cref="CommandLineParser.GetDefaultArgumentNamePrefixes()" qualifyHint="true"/> method if that property
    /// is <see langword="null"/>
    /// </value>
    public IEnumerable<string> ArgumentNamePrefixesOrDefault => ArgumentNamePrefixes ?? CommandLineParser.GetDefaultArgumentNamePrefixes();

    /// <summary>
    /// Gets or sets the argument name prefix to use for long argument names.
    /// </summary>
    /// <value>
    /// The long argument prefix, or <see langword="null"/> to use the value from the
    /// <see cref="ParseOptionsAttribute"/> attribute, or if not set, the default prefix from
    /// the <see cref="CommandLineParser.DefaultLongArgumentNamePrefix" qualifyHint="true"/> constant. The default
    /// value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property is only used if the <see cref="Mode"/> property or the
    ///   <see cref="ParseOptionsAttribute.Mode" qualifyHint="true"/> is 
    ///   <see cref="ParsingMode.LongShort" qualifyHint="true"/>, or if the <see cref="PrefixTermination"/>
    ///   or <see cref="ParseOptionsAttribute.PrefixTermination" qualifyHint="true"/> property is not
    ///   <see cref="PrefixTerminationMode.None" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   Use the <see cref="ArgumentNamePrefixes"/> to specify the prefixes for short argument
    ///   names.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.LongArgumentNamePrefix" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.LongArgumentNamePrefix" qualifyHint="true"/>
    public string? LongArgumentNamePrefix { get; set; }

    /// <summary>
    /// Gets the argument name prefix to use for long argument names.
    /// </summary>
    /// <value>
    /// The value of the <see cref="ArgumentNamePrefixes"/> property, or the value of the
    /// <see cref="CommandLineParser.DefaultLongArgumentNamePrefix" qualifyHint="true"/> constant if that property
    /// is <see langword="null"/>
    /// </value>
    public string LongArgumentNamePrefixOrDefault => LongArgumentNamePrefix ?? CommandLineParser.DefaultLongArgumentNamePrefix;

    /// <summary>
    /// Gets or set the type of string comparison to use for argument names.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="StringComparison"/> enumeration, or
    /// <see langword="null"/> to use the one determined using the
    /// <see cref="ParseOptionsAttribute.CaseSensitive" qualifyHint="true"/> property, or if the
    /// <see cref="ParseOptionsAttribute"/> is not present,
    /// <see cref="StringComparison.OrdinalIgnoreCase" qualifyHint="true"/>. The default value is
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.CaseSensitive" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.ArgumentNameComparison" qualifyHint="true"/>
    public StringComparison? ArgumentNameComparison { get; set; }

    /// <summary>
    /// Gets the type of string comparison to use for argument names.
    /// </summary>
    /// <value>
    /// The value of the <see cref="ArgumentNameComparison"/> property, or <see cref="StringComparison.OrdinalIgnoreCase" qualifyHint="true"/>
    /// if that property is <see langword="null"/>.
    /// </value>
    public StringComparison ArgumentNameComparisonOrDefault => ArgumentNameComparison ?? StringComparison.OrdinalIgnoreCase;


    /// <summary>
    /// Gets or sets the <see cref="TextWriter"/> used to print error information if argument
    /// parsing fails.
    /// </summary>
    /// <value>
    /// The <see cref="TextWriter"/> used to print error information, or <see langword="null"/>
    /// to print to a <see cref="LineWrappingTextWriter"/> for the standard error stream 
    /// (<see cref="Console.Error" qualifyHint="true"/>). The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Only the parsing methods that automatically handle errors will use this property.
    /// </para>
    /// <para>
    ///   If argument parsing is successful, nothing will be written.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.Parse{T}(ParseOptions?)"/>
    /// <seealso cref="CommandLineParser{T}.ParseWithErrorHandling()"/>
    /// <seealso cref="IParser{TSelf}.Parse(ParseOptions?)"/>
    public TextWriter? Error { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether duplicate arguments are allowed.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="ErrorMode"/> enumeration, or <see langword="null"/>
    /// to use the value from the <see cref="ParseOptionsAttribute"/> attribute, or if that
    /// attribute is not present, <see cref="ErrorMode.Error" qualifyHint="true"/>. The default value is
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If set to <see cref="ErrorMode.Error" qualifyHint="true"/>, supplying a non-multi-value argument more
    ///   than once will cause an exception. If set to <see cref="ErrorMode.Allow" qualifyHint="true"/>, the
    ///   last value supplied will be used.
    /// </para>
    /// <para>
    ///   If set to <see cref="ErrorMode.Warning" qualifyHint="true"/>, the <see cref="CommandLineParser{T}.ParseWithErrorHandling()" qualifyHint="true"/>
    ///   method, the static <see cref="CommandLineParser.Parse{T}(ParseOptions?)" qualifyHint="true"/>
    ///   method, the generated <see cref="IParser{TSelf}.Parse(ParseOptions?)" qualifyHint="true"/>
    ///   method and the <see cref="CommandManager"/> class will print a warning to the stream
    ///   indicated by the <see cref="Error"/> property when a duplicate argument is found. If you
    ///   are not using these methods, <see cref="ErrorMode.Warning" qualifyHint="true"/> is
    ///   identical to <see cref="ErrorMode.Allow" qualifyHint="true"/>, and no warning is
    ///   displayed.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.DuplicateArguments" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.AllowDuplicateArguments" qualifyHint="true"/>
    public ErrorMode? DuplicateArguments { get; set; }

    /// <summary>
    /// Gets a value indicating whether duplicate arguments are allowed.
    /// </summary>
    /// <value>
    /// The value of the <see cref="DuplicateArguments"/> property, or <see cref="ErrorMode.Error" qualifyHint="true"/>
    /// if that property is <see langword="null"/>.
    /// </value>
    public ErrorMode DuplicateArgumentsOrDefault => DuplicateArguments ?? ErrorMode.Error;

    /// <summary>
    /// Gets or sets a value indicating whether the value of arguments may be separated from the name by white space.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if white space is allowed to separate an argument name and its
    ///   value; <see langword="false"/> if only the <see cref="NameValueSeparators"/> are allowed,
    ///   or <see langword="null" /> to use the value from the <see cref="ParseOptionsAttribute.AllowWhiteSpaceValueSeparator" qualifyHint="true"/>
    ///   property, or if the <see cref="ParseOptionsAttribute"/> is not present, the default
    ///   option which is <see langword="true"/>. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.AllowWhiteSpaceValueSeparator" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.AllowWhiteSpaceValueSeparator" qualifyHint="true"/>
    public bool? AllowWhiteSpaceValueSeparator { get; set; }

    /// <summary>
    /// Gets a value indicating whether the value of arguments may be separated from the name by
    /// white space.
    /// </summary>
    /// <value>
    /// The value of the <see cref="AllowWhiteSpaceValueSeparator"/> property, or <see langword="true"/>
    /// if that property is <see langword="null"/>.
    /// </value>
    public bool AllowWhiteSpaceValueSeparatorOrDefault => AllowWhiteSpaceValueSeparator ?? true;

    /// <summary>
    /// Gets or sets the characters used to separate the name and the value of an argument.
    /// </summary>
    /// <value>
    ///   The character used to separate the name and the value of an argument, or <see langword="null"/>
    ///   to use the value from the <see cref="ParseOptionsAttribute"/> attribute, or if that
    ///   is not present, the values returned by the <see cref="CommandLineParser.GetDefaultNameValueSeparators" qualifyHint="true"/>
    ///   method, which are a colon (:) and an equals sign (=). The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   These characters are used to separate the name and the value if both are provided as
    ///   a single argument to the application, e.g. <c>-sample:value</c> or <c>-sample=value</c>
    ///   if the default value is used.
    /// </para>
    /// <note>
    ///   The characters chosen here cannot be used in the name of any parameter. Therefore,
    ///   it's usually best to choose a non-alphanumeric value such as the colon or equals sign.
    ///   The characters can appear in argument values (e.g. <c>-sample:foo:bar</c> is fine, in\
    ///   which case the value is "foo:bar").
    /// </note>
    /// <note>
    ///   Do not pick a white-space character as the separator. Doing this only works if the
    ///   white-space character is part of the argument token, which usually means it needs to be
    ///   quoted or escaped when invoking your application. Instead, use the
    ///   <see cref="AllowWhiteSpaceValueSeparator"/> property to control whether white space
    ///   is allowed as a separator.
    /// </note>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.NameValueSeparators" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    public IEnumerable<char>? NameValueSeparators { get; set; }

    /// <summary>
    /// Gets the characters used to separate the name and the value of an argument.
    /// </summary>
    /// <value>
    /// The value of the <see cref="NameValueSeparators"/> property, or the return value of the
    /// <see cref="CommandLineParser.GetDefaultNameValueSeparators" qualifyHint="true"/> method if that property is
    /// <see langword="null"/>.
    /// </value>
    public IEnumerable<char> NameValueSeparatorsOrDefault => NameValueSeparators ?? CommandLineParser.GetDefaultNameValueSeparators();

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
    ///   will automatically add an argument with the name "Help". If using <see cref="ParsingMode.LongShort" qualifyHint="true"/>,
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
    ///   <see cref="ParseOptionsAttribute.AutoHelpArgument" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="LocalizedStringProvider.AutomaticHelpName" qualifyHint="true"/>
    /// <seealso cref="LocalizedStringProvider.AutomaticHelpDescription" qualifyHint="true"/>
    /// <seealso cref="LocalizedStringProvider.AutomaticHelpShortName" qualifyHint="true"/>
    public bool? AutoHelpArgument { get; set; }

    /// <summary>
    /// Gets a value that indicates a help argument will be automatically added.
    /// </summary>
    /// <value>
    /// The value of the <see cref="AutoHelpArgument"/> property, or <see langword="true"/>
    /// if that property is <see langword="null"/>.
    /// </value>
    public bool AutoHelpArgumentOrDefault => AutoHelpArgument ?? true;

    /// <summary>
    /// Gets or sets a value that indicates a version argument will be automatically added.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> to automatically create a version argument; <see langword="false"/>
    ///   to not create one, or <see langword="null" /> to use the value from the
    ///   <see cref="ParseOptionsAttribute.AutoVersionArgument" qualifyHint="true"/> property, or if the
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
    ///   <see cref="ParseOptionsAttribute.AutoVersionArgument" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="LocalizedStringProvider.AutomaticVersionName" qualifyHint="true"/>
    /// <seealso cref="LocalizedStringProvider.AutomaticVersionDescription" qualifyHint="true"/>
    public bool? AutoVersionArgument { get; set; }

    /// <summary>
    /// Gets a value that indicates a version argument will be automatically added.
    /// </summary>
    /// <value>
    /// The value of the <see cref="AutoVersionArgument"/> property, or <see langword="true"/>
    /// if that property is <see langword="null"/>.
    /// </value>
    public bool AutoVersionArgumentOrDefault => AutoVersionArgument ?? true;


    /// <summary>
    /// Gets or sets a value that indicates whether unique prefixes of an argument are automatically
    /// used as aliases.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> to automatically use unique prefixes of an argument as aliases for
    ///   that argument; <see langword="false"/> to not have automatic prefixes; otherwise,
    ///   <see langword="null" /> to use the value from the <see cref="ParseOptionsAttribute.AutoPrefixAliases" qualifyHint="true"/>
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
    ///   When using <see cref="ParsingMode.LongShort" qualifyHint="true"/>, this only applies to long names. Explicit
    ///   aliases set with the <see cref="AliasAttribute"/> take precedence over automatic aliases.
    ///   Automatic prefix aliases are not shown in the usage help.
    /// </para>
    /// <para>
    ///   This behavior is enabled unless explicitly disabled here or using the
    ///   <see cref="ParseOptionsAttribute.AutoPrefixAliases" qualifyHint="true"/> property.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the value of the
    ///   <see cref="ParseOptionsAttribute.AutoPrefixAliases" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    public bool? AutoPrefixAliases { get; set; }

    /// <summary>
    /// Gets a value that indicates whether unique prefixes of an argument are automatically used as
    /// aliases.
    /// </summary>
    /// <value>
    /// The value of the <see cref="AutoPrefixAliases"/> property, or <see langword="true"/>
    /// if that property is <see langword="null"/>.
    /// </value>
    public bool AutoPrefixAliasesOrDefault => AutoPrefixAliases ?? true;

    /// <summary>
    /// Gets or sets the color applied to error messages.
    /// </summary>
    /// <value>
    ///   The virtual terminal sequence for a color. The default value is
    ///   <see cref="TextFormat.ForegroundRed" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Only the parsing methods that automatically handle errors will use this property.
    /// </para>
    /// <para>
    ///   The color will only be used if the <see cref="UseErrorColor"/> property is
    ///   <see langword="true"/>; otherwise, it will be replaced with an empty string.
    /// </para>
    /// <para>
    ///   After the error message, the value of the <see cref="UsageWriter.ColorReset" qualifyHint="true"/>
    ///   property will be written to undo the color change.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.Parse{T}(ParseOptions?)"/>
    /// <seealso cref="CommandLineParser{T}.ParseWithErrorHandling()"/>
    /// <seealso cref="IParser{TSelf}.Parse(ParseOptions?)"/>
    public TextFormat ErrorColor { get; set; } = TextFormat.ForegroundRed;

    /// <summary>
    /// Gets or sets the color applied to warning messages.
    /// </summary>
    /// <value>
    ///   The virtual terminal sequence for a color. The default value is
    ///   <see cref="TextFormat.ForegroundYellow" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Only the parsing methods that automatically handle errors will use this property.
    /// </para>
    /// <para>
    ///   The color will only be used if the <see cref="UseErrorColor"/> property is
    ///   <see langword="true"/>; otherwise, it will be replaced with an empty string.
    /// </para>
    /// <para>
    ///   This color is used for the warning emitted if the <see cref="DuplicateArguments"/>
    ///   property is <see cref="ErrorMode.Warning" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   After the warning message, the value of the <see cref="UsageWriter.ColorReset" qualifyHint="true"/>
    ///   property will be written to undo the color change.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.Parse{T}(ParseOptions?)"/>
    /// <seealso cref="CommandLineParser{T}.ParseWithErrorHandling()"/>
    /// <seealso cref="IParser{TSelf}.Parse(ParseOptions?)"/>
    public TextFormat WarningColor { get; set; } = TextFormat.ForegroundYellow;

    /// <summary>
    /// Gets or sets a value that indicates whether error messages should use color.
    /// </summary>
    /// <value>
    ///   <see cref="TriState.True" qualifyHint="true"/> to enable color output;
    ///   <see cref="TriState.False" qualifyHint="true"/> to disable color output; or
    ///   <see cref="TriState.Auto" qualifyHint="true"/> to enable it if the error output supports
    ///   it. The default value is <see cref="TriState.Auto" qualifyHint="true"/>
    /// </value>
    /// <remarks>
    /// <para>
    ///   Only the parsing methods that automatically handle errors will use this property.
    /// </para>
    /// <para>
    ///   If this property is <see cref="TriState.Auto" qualifyHint="true"/> and the
    ///   <see cref="Error"/> property is <see langword="null"/>, color will be used if the standard
    ///   error stream supports it, as determined by the <see cref="VirtualTerminal.EnableColor"
    ///   qualifyHint="true"/> method.
    /// </para>
    /// <para>
    ///   If this property is set to <see langword="true"/> explicitly, virtual terminal
    ///   sequences may be included in the output even if it's not supported, which may lead to
    ///   garbage characters appearing in the output.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.Parse{T}(ParseOptions?)"/>
    /// <seealso cref="CommandLineParser{T}.ParseWithErrorHandling()"/>
    /// <seealso cref="IParser{TSelf}.Parse(ParseOptions?)"/>
    public TriState UseErrorColor { get; set; }

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
    /// <seealso cref="CommandLineParser.StringProvider" qualifyHint="true"/>
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    [AllowNull]
#endif
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
    /// is <see cref="UsageHelpRequest.SyntaxOnly" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Only the parsing methods that automatically handle errors will use this property.
    /// </para>
    /// <para>
    ///   If the value of this property is not <see cref="UsageHelpRequest.Full" qualifyHint="true"/>,
    ///   the message returned by the <see cref="UsageWriter.WriteMoreInfoMessage" qualifyHint="true"/>
    ///   method is written instead of the omitted parts of the usage help.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.Parse{T}(ParseOptions?)"/>
    /// <seealso cref="CommandLineParser{T}.ParseWithErrorHandling()"/>
    /// <seealso cref="IParser{TSelf}.Parse(ParseOptions?)"/>
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
    /// <seealso cref="CommandLineArgument.ValueDescription" qualifyHint="true"/>
    public IDictionary<Type, string>? DefaultValueDescriptions { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates how value descriptions derived from type names
    /// are transformed.
    /// </summary>
    /// <value>
    /// One of the members of the <see cref="ArgumentNameTransform"/> enumeration, or <see langword="null"/>
    /// to use the value from the <see cref="ParseOptionsAttribute"/> attribute, or if that is
    /// not present, <see cref="NameTransform.None" qualifyHint="true"/>. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property has no effect on explicit value description specified with the
    ///   <see cref="ValueDescriptionAttribute"/> attribute or the <see cref="DefaultValueDescriptions"/>
    ///   property.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the <see cref="ParseOptionsAttribute.ValueDescriptionTransform" qualifyHint="true"/>
    ///   property.
    /// </para>
    /// </remarks>
    public NameTransform? ValueDescriptionTransform { get; set; }

    /// <summary>
    /// Gets a value that indicates how value descriptions derived from type names are transformed.
    /// </summary>
    /// <value>
    /// The value of the <see cref="ValueDescriptionTransform"/> property, or <see cref="NameTransform.None" qualifyHint="true"/>
    /// if that property is <see langword="null"/>.
    /// </value>
    public NameTransform ValueDescriptionTransformOrDefault => ValueDescriptionTransform ?? NameTransform.None;

    /// <summary>
    /// Gets or sets the behavior when an argument is encountered that consists of only the long
    /// argument prefix ("--" by default) by itself, not followed by a name.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="PrefixTerminationMode"/> enumeration, or
    /// <see langword="null"/> to use the value from the <see cref="ParseOptionsAttribute"/>
    /// attribute, or if that is not present, <see cref="PrefixTerminationMode.None" qualifyHint="true"/>.
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Use this property to allow the use of the long argument prefix by itself to either treat
    ///   all remaining values as positional argument values, even when they start with an argument
    ///   prefix, or to cancel parsing with <see cref="CancelMode.Success" qualifyHint="true"/> so
    ///   the remaining values can be inspected using the
    ///   <see cref="ParseResult.RemainingArguments" qualifyHint="true"/> property. This follows
    ///   typical POSIX argument parsing conventions.
    /// </para>
    /// <para>
    ///   The value of the <see cref="LongArgumentNamePrefix"/> property is used to identify this
    ///   special argument, even if the parsing mode is not
    ///   <see cref="ParsingMode.LongShort" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   If not <see langword="null"/>, this property overrides the
    ///   <see cref="ParseOptionsAttribute.PrefixTermination" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    public PrefixTerminationMode? PrefixTermination {  get; set; }

    /// <summary>
    /// Gets the behavior when an argument is encountered that consists of only the long argument
    /// prefix ("--" by default) by itself, not followed by a name.
    /// </summary>
    /// <value>
    /// The value of the <see cref="PrefixTermination"/> property, or <see cref="PrefixTerminationMode.None" qualifyHint="true"/>
    /// if that property is <see langword="null"/>.
    /// </value>
    public PrefixTerminationMode PrefixTerminationOrDefault => PrefixTermination ?? PrefixTerminationMode.None;

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
    ///   of the static <see cref="CommandLineParser.Parse{T}(ParseOptions?)" qualifyHint="true"/>
    ///   methods. If you use the generated static <see cref="IParser{TSelf}"/> or
    ///   <see cref="IParserProvider{TSelf}"/> interface methods on the command line arguments type,
    ///   the generated parser is used regardless of the value of this property.
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
    /// An instance of a class inheriting from the <see cref="UsageWriter"/> class.
    /// The default value is an instance of the <see cref="UsageWriter"/> class
    /// itself.
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
        NameValueSeparators ??= attribute.NameValueSeparators;
        AutoHelpArgument ??= attribute.AutoHelpArgument;
        AutoVersionArgument ??= attribute.AutoVersionArgument;
        AutoPrefixAliases ??= attribute.AutoPrefixAliases;
        ValueDescriptionTransform ??= attribute.ValueDescriptionTransform;
        PrefixTermination ??= attribute.PrefixTermination;
    }

    internal VirtualTerminalSupport EnableErrorColor()
    {
        // If colors are forced on or off; don't change terminal mode but return the explicit
        // support value.
        if (UseErrorColor == TriState.True)
        {
            return new VirtualTerminalSupport(true);
        }

        if (UseErrorColor == TriState.False)
        {
            return new VirtualTerminalSupport(false);
        }

        // Enable for stderr if no custom error writer.
        if (Error == null)
        {
            return VirtualTerminal.EnableColor(StandardStream.Error);
        }

        // Try to enable it for the std stream associated with the custom writer.
        if (Error.GetStandardStream() is StandardStream stream)
        {
            return VirtualTerminal.EnableColor(stream);
        }

        // No std stream, no automatic color.
        return new VirtualTerminalSupport(false);
    }
}
