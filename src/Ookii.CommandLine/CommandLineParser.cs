// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Support;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ookii.CommandLine;

/// <summary>
/// Parses command line arguments defined by a class of the specified type.
/// </summary>
/// <remarks>
/// <para>
///   The <see cref="CommandLineParser"/> class can parse a set of command line arguments into
///   values. Which arguments are accepted is determined from the properties and methods of the
///   type passed to the <see cref="CommandLineParser(Type, ParseOptions)"/> constructor. The
///   result of a parsing operation is an instance of that type, created using the values that
///   were supplied on the command line.
/// </para>
/// <para>
///   The arguments type must have a constructor that has no parameter, or a single parameter
///   with the type <see cref="CommandLineParser"/>, which will be passed the instance of the
///   <see cref="CommandLineParser"/> class that was used to parse the arguments when the type
///   is instantiated.
/// </para>
/// <para>
///   A property defines a command line argument if it is <see langword="public"/>, not
///   <see langword="static"/>, and has the <see cref="CommandLineArgumentAttribute"/> attribute
///   defined. The properties of the argument are determined by the properties of the
///   <see cref="CommandLineArgumentAttribute"/> class.
/// </para>
/// <para>
///   A method defines a command line argument if it is <see langword="public"/>, <see langword="static"/>,
///   has the <see cref="CommandLineArgumentAttribute"/> attribute applied, and one of the
///   signatures shown in the documentation for the <see cref="CommandLineArgumentAttribute"/>
///   attribute.
/// </para>
/// <para>
///   To parse arguments, invoke the <see cref="Parse()"/> method or one of its overloads.
///   The static <see cref="Parse{T}(ParseOptions)"/> method is a helper that will
///   parse arguments and print error and usage information if required. Calling this method
///   will be sufficient for most use cases.
/// </para>
/// <para>
///   The derived type <see cref="CommandLineParser{T}"/> also provides strongly-typed instance
///   <see cref="CommandLineParser{T}.Parse()"/> methods, if you don't wish to use the static
///   method.
/// </para>
/// <para>
///   The <see cref="CommandLineParser"/> class can generate detailed usage help for the
///   defined arguments, which can be shown to the user to provide information about how to
///   invoke your application from the command line. This usage is shown automatically by the
///   <see cref="Parse{T}(ParseOptions?)"/> method and the <see cref="CommandManager"/> class,
///   or you can use the <see cref="WriteUsage"/> and <see cref="GetUsage"/> methods to generate
///   it manually.
/// </para>
/// <para>
///   The <see cref="CommandLineParser"/> class is for applications with a single (root) command.
///   If you wish to create an application with subcommands, use the <see cref="CommandManager"/>
///   class instead.
/// </para>
/// <para>
///   The <see cref="CommandLineParser"/> supports two sets of rules for how to parse arguments;
///   <see cref="ParsingMode.Default"/> mode and <see cref="ParsingMode.LongShort"/> mode. For
///   more details on these rules, please see
///   <see href="https://www.github.com/SvenGroot/ookii.commandline">the documentation on GitHub</see>.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
/// <seealso cref="CommandLineParser{T}"/>
/// <seealso cref="CommandManager"/>
/// <seealso href="https://www.github.com/SvenGroot/ookii.commandline">Usage documentation</seealso>
public class CommandLineParser
{
    #region Nested types

    private sealed class CommandLineArgumentComparer : IComparer<CommandLineArgument>
    {
        private readonly StringComparison _comparison;

        public CommandLineArgumentComparer(StringComparison comparison)
        {
            _comparison = comparison;
        }

        public int Compare(CommandLineArgument? x, CommandLineArgument? y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else if (y == null)
            {
                return 1;
            }

            // Positional arguments come before non-positional ones, and must be sorted by position
            if (x.Position != null)
            {
                if (y.Position != null)
                {
                    return x.Position.Value.CompareTo(y.Position.Value);
                }
                else
                {
                    return -1;
                }
            }
            else if (y.Position != null)
            {
                return 1;
            }

            // Non-positional required arguments come before optional arguments
            if (x.IsRequired)
            {
                if (!y.IsRequired)
                {
                    return -1;
                }
                // If both are required, sort by name
            }
            else if (y.IsRequired)
            {
                return 1;
            }

            // Sort the rest by name
            return string.Compare(x.ArgumentName, y.ArgumentName, _comparison);
        }
    }

    private sealed class MemoryComparer : IComparer<ReadOnlyMemory<char>>
    {
        private readonly StringComparison _comparison;

        public MemoryComparer(StringComparison comparison)
        {
            _comparison = comparison;
        }

        public int Compare(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) => x.Span.CompareTo(y.Span, _comparison);
    }

    private sealed class CharComparer : IComparer<char>
    {
        private readonly StringComparison _comparison;

        public CharComparer(StringComparison comparison)
        {
            _comparison = comparison;
        }

        public int Compare(char x, char y)
        {
            unsafe
            {
                // If anyone knows a better way to compare individual chars according to a
                // StringComparison, I'd be happy to hear it.
                var spanX = new ReadOnlySpan<char>(&x, 1);
                var spanY = new ReadOnlySpan<char>(&y, 1);
                return spanX.CompareTo(spanY, _comparison);
            }
        }
    }

    private struct PrefixInfo
    {
        public string Prefix { get; set; }
        public bool Short { get; set; }
    }

    #endregion

    private readonly ArgumentProvider _provider;
    private readonly ImmutableArray<CommandLineArgument> _arguments;
    private readonly SortedDictionary<ReadOnlyMemory<char>, CommandLineArgument> _argumentsByName;
    private readonly SortedDictionary<char, CommandLineArgument>? _argumentsByShortName;
    private readonly int _positionalArgumentCount;

    private readonly ParseOptions _parseOptions;
    private readonly ParsingMode _mode;
    private readonly PrefixInfo[] _sortedPrefixes;
    private readonly ImmutableArray<string> _argumentNamePrefixes;
    private readonly string? _longArgumentNamePrefix;
    private readonly ImmutableArray<char> _nameValueSeparators;

    private List<CommandLineArgument>? _requiredPropertyArguments;

    /// <summary>
    /// Gets the default prefix used for long argument names if <see cref="Mode"/> is
    /// <see cref="ParsingMode.LongShort"/>.
    /// </summary>
    /// <value>
    /// The default long argument name prefix, which is '--'.
    /// </value>
    /// <remarks>
    /// <para>
    /// This constant is used as the default value of the <see cref="LongArgumentNamePrefix"/>
    /// property.
    /// </para>
    /// </remarks>
    public const string DefaultLongArgumentNamePrefix = "--";

    /// <summary>
    /// Event raised when an argument is parsed from the command line.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   If the event handler sets the <see cref="CancelEventArgs.Cancel"/> property to <see langword="true"/>, command line processing will stop immediately,
    ///   and the <see cref="Parse(string[],int)"/> method will return <see langword="null"/>. The
    ///   <see cref="HelpRequested"/> property will be set to <see langword="true"/> automatically.
    /// </para>
    /// <para>
    ///   If the argument used <see cref="ArgumentKind.Method"/> and the argument's method
    ///   canceled parsing, the <see cref="CancelEventArgs.Cancel"/> property will already be
    ///   true when the event is raised. In this case, the <see cref="HelpRequested"/> property
    ///   will not automatically be set to <see langword="true"/>.
    /// </para>
    /// <para>
    ///   This event is invoked after the <see cref="CommandLineArgument.Value"/> and <see cref="CommandLineArgument.UsedArgumentName"/> properties have been set.
    /// </para>
    /// </remarks>
    public event EventHandler<ArgumentParsedEventArgs>? ArgumentParsed;

    /// <summary>
    /// Event raised when a non-multi-value argument is specified more than once.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Handling this event allows you to inspect the new value, and decide to keep the old
    ///   or new value. It also allows you to, for instance, print a warning for duplicate
    ///   arguments.
    /// </para>
    /// <para>
    ///   This even is only raised when the <see cref="AllowDuplicateArguments"/> property is
    ///   <see langword="true"/>.
    /// </para>
    /// </remarks>
    public event EventHandler<DuplicateArgumentEventArgs>? DuplicateArgument;

    internal const string UnreferencedCodeHelpUrl = "https://www.ookii.org/Link/CommandLineSourceGeneration";

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineParser"/> class using the
    /// specified arguments type and options.
    /// </summary>
    /// <param name="argumentsType">The <see cref="Type"/> of the class that defines the command line arguments.</param>
    /// <param name="options">
    ///   The options that control parsing behavior, or <see langword="null"/> to use the
    ///   default options.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="argumentsType"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    ///   The <see cref="CommandLineParser"/> cannot use <paramref name="argumentsType"/> as the command line arguments type,
    ///   because it violates one of the rules concerning argument names or positions, or has an argument type that cannot
    ///   be parsed.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This constructor uses reflection to determine the arguments defined by the type indicated
    ///   by <paramref name="argumentsType"/> at runtime, unless the type has the
    ///   <see cref="GeneratedParserAttribute"/> applied. In that case, you can also use the
    ///   generated static <c>CreateParser()</c> or <c>Parse()</c> methods on that type instead.
    /// </para>
    /// <para>
    ///   If the <paramref name="options"/> parameter is not <see langword="null"/>, the
    ///   instance passed in will be modified to reflect the options from the arguments class's
    ///   <see cref="ParseOptionsAttribute"/> attribute, if it has one.
    /// </para>
    /// <para>
    ///   Certain properties of the <see cref="ParseOptions"/> class can be changed after the
    ///   <see cref="CommandLineParser"/> class has been constructed, and still affect the
    ///   parsing behavior. See the <see cref="Options"/> property for details.
    /// </para>
    /// </remarks>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Argument information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute.", Url = UnreferencedCodeHelpUrl)]
#endif
    public CommandLineParser(Type argumentsType, ParseOptions? options = null)
        : this(GetArgumentProvider(argumentsType ?? throw new ArgumentNullException(nameof(argumentsType)), options), options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineParser"/> class using the
    /// specified arguments type and options.
    /// </summary>
    /// <param name="provider">
    /// The <see cref="ArgumentProvider"/> that defines the command line arguments.
    /// </param>
    /// <param name="options">
    ///   The options that control parsing behavior, or <see langword="null"/> to use the
    ///   default options.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="provider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    ///   The <see cref="CommandLineParser"/> cannot use <paramref name="provider"/> for the command
    ///   line arguments, because it violates one of the rules concerning argument names or
    ///   positions, or has an argument type that cannot
    ///   be parsed.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   If the <paramref name="options"/> parameter is not <see langword="null"/>, the
    ///   instance passed in will be modified to reflect the options from the arguments class's
    ///   <see cref="ParseOptionsAttribute"/> attribute, if it has one.
    /// </para>
    /// <para>
    ///   Certain properties of the <see cref="ParseOptions"/> class can be changed after the
    ///   <see cref="CommandLineParser"/> class has been constructed, and still affect the
    ///   parsing behavior. See the <see cref="Options"/> property for details.
    /// </para>
    /// </remarks>
    public CommandLineParser(ArgumentProvider provider, ParseOptions? options = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _parseOptions = options ?? new();
        var optionsAttribute = _provider.OptionsAttribute;
        if (optionsAttribute != null)
        {
            _parseOptions.Merge(optionsAttribute);
        }

        _mode = _parseOptions.ModeOrDefault;
        var comparison = _parseOptions.ArgumentNameComparisonOrDefault;
        ArgumentNameComparison = comparison;
        _argumentNamePrefixes = DetermineArgumentNamePrefixes(_parseOptions);
        _nameValueSeparators = DetermineNameValueSeparators(_parseOptions);
        var prefixInfos = _argumentNamePrefixes.Select(p => new PrefixInfo { Prefix = p, Short = true });
        if (_mode == ParsingMode.LongShort)
        {
            _longArgumentNamePrefix = _parseOptions.LongArgumentNamePrefixOrDefault;
            if (string.IsNullOrWhiteSpace(_longArgumentNamePrefix))
            {
                throw new ArgumentException(Properties.Resources.EmptyArgumentNamePrefix, nameof(options));
            }

            var longInfo = new PrefixInfo { Prefix = _longArgumentNamePrefix, Short = false };
            prefixInfos = prefixInfos.Append(longInfo);
            _argumentsByShortName = new(new CharComparer(comparison));
        }

        _sortedPrefixes = prefixInfos.OrderByDescending(info => info.Prefix.Length).ToArray();
        _argumentsByName = new(new MemoryComparer(comparison));

        var builder = ImmutableArray.CreateBuilder<CommandLineArgument>();
        _positionalArgumentCount = DetermineMemberArguments(builder);
        DetermineAutomaticArguments(builder);
        // Sort the member arguments in usage order (positional first, then required
        // non-positional arguments, then the rest by name.
        builder.Sort(new CommandLineArgumentComparer(comparison));
        if (builder.Count == builder.Capacity)
        {
            _arguments = builder.MoveToImmutable();
        }
        else
        {
            _arguments = builder.ToImmutable();
        }

        VerifyPositionalArgumentRules();
    }

    /// <summary>
    /// Gets the command line argument parsing rules used by the parser.
    /// </summary>
    /// <value>
    /// The <see cref="Ookii.CommandLine.ParsingMode"/> for this parser. The default is
    /// <see cref="ParsingMode.Default"/>.
    /// </value>
    /// <seealso cref="ParseOptionsAttribute.Mode"/>
    /// <seealso cref="ParseOptions.Mode"/>
    public ParsingMode Mode => _mode;

    /// <summary>
    /// Gets the argument name prefixes used by this instance.
    /// </summary>
    /// <value>
    /// A list of argument name prefixes.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The argument name prefixes are used to distinguish argument names from positional argument values in a command line.
    /// </para>
    /// <para>
    ///   These prefixes will be used for short argument names if the <see cref="Mode"/>
    ///   property is <see cref="ParsingMode.LongShort"/>. Use <see cref="LongArgumentNamePrefix"/>
    ///   to get the prefix for long argument names.
    /// </para>
    /// </remarks>
    /// <seealso cref="ParseOptionsAttribute.ArgumentNamePrefixes"/>
    /// <seealso cref="ParseOptions.ArgumentNamePrefixes"/>
    public ImmutableArray<string> ArgumentNamePrefixes => _argumentNamePrefixes;

    /// <summary>
    /// Gets the prefix to use for long argument names.
    /// </summary>
    /// <value>
    /// The prefix for long argument names, or <see langword="null"/> if <see cref="Mode"/>
    /// is not <see cref="ParsingMode.LongShort"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The long argument prefix is only used if <see cref="Mode"/> property is
    ///   <see cref="ParsingMode.LongShort"/>. See <see cref="ArgumentNamePrefixes"/> to
    ///   get the prefixes for short argument names.
    /// </para>
    /// </remarks>
    /// <seealso cref="ParseOptionsAttribute.LongArgumentNamePrefix"/>
    /// <seealso cref="ParseOptions.LongArgumentNamePrefix"/>
    public string? LongArgumentNamePrefix => _longArgumentNamePrefix;

    /// <summary>
    /// Gets the type that was used to define the arguments.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> that was used to define the arguments.
    /// </value>
    public Type ArgumentsType => _provider.ArgumentsType;

    /// <summary>
    /// Gets the friendly name of the application.
    /// </summary>
    /// <value>
    /// The friendly name of the application.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The friendly name is determined by checking for the <see cref="ApplicationFriendlyNameAttribute"/>
    ///   attribute first on the arguments type, then on the arguments type's assembly. If
    ///   neither exists, the <see cref="AssemblyTitleAttribute"/> is used. If that is not present
    ///   either, the assembly's name is used.
    /// </para>
    /// <para>
    ///   This name is only used in the output of the automatically created "-Version"
    ///   attribute.
    /// </para>
    /// </remarks>
    public string ApplicationFriendlyName => _provider.ApplicationFriendlyName;

    /// <summary>
    /// Gets a description that is used when generating usage information.
    /// </summary>
    /// <value>
    /// The description of the command line application. The default value is an empty string ("").
    /// </value>
    /// <remarks>
    /// <para>
    ///   This description will be added to the usage returned by the <see cref="WriteUsage"/>
    ///   method. This description can be set by applying the <see cref="DescriptionAttribute"/>
    ///   to the command line arguments type.
    /// </para>
    /// </remarks>
    public string Description => _provider.Description;

    /// <summary>
    /// Gets the options used by this instance.
    /// </summary>
    /// <value>
    /// An instance of the <see cref="ParseOptions"/> class.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If you change the value of the <see cref="ParseOptions.Culture"/>, <see cref="ParseOptions.DuplicateArguments"/>,
    ///   <see cref="ParseOptions.AllowWhiteSpaceValueSeparator"/>, <see cref="StringProvider"/> or
    ///   <see cref="UsageWriter"/> property, this will affect the behavior of this instance. The
    ///   other properties of the <see cref="ParseOptions"/> class are only used when the
    ///   <see cref="CommandLineParser"/> class in constructed, so changing them afterwards will
    ///   have no effect.
    /// </para>
    /// </remarks>
    public ParseOptions Options => _parseOptions;

    /// <summary>
    /// Gets the culture used to convert command line argument values from their string representation to the argument type.
    /// </summary>
    /// <value>
    /// The culture used to convert command line argument values from their string representation to the argument type. The default value
    /// is <see cref="CultureInfo.InvariantCulture"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Use the <see cref="ParseOptions"/> class to change this value.
    /// </para>
    /// </remarks>
    /// <seealso cref="ParseOptions.Culture"/>
    public CultureInfo Culture => _parseOptions.CultureOrDefault;

    /// <summary>
    /// Gets a value indicating whether duplicate arguments are allowed.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if it is allowed to supply non-multi-value arguments more than once; otherwise, <see langword="false"/>.
    ///   The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="AllowDuplicateArguments"/> property is <see langword="false"/>, a <see cref="CommandLineArgumentException"/> is thrown by the <see cref="Parse(string[],int)"/>
    ///   method if an argument's value is supplied more than once.
    /// </para>
    /// <para>
    ///   If the <see cref="AllowDuplicateArguments"/> property is <see langword="true"/>, the last value supplied for the argument is used if it is supplied multiple times.
    /// </para>
    /// <para>
    ///   The <see cref="AllowDuplicateArguments"/> property has no effect on multi-value or
    ///   dictionary arguments, which can always be supplied multiple times.
    /// </para>
    /// <para>
    ///   Use the <see cref="ParseOptions"/> or <see cref="ParseOptionsAttribute "/> class to
    ///   change this value.
    /// </para>
    /// </remarks>
    /// <see cref="ParseOptionsAttribute.DuplicateArguments"/>
    /// <see cref="ParseOptions.DuplicateArguments"/>
    public bool AllowDuplicateArguments => _parseOptions.DuplicateArgumentsOrDefault != ErrorMode.Error;

    /// <summary>
    /// Gets value indicating whether the value of an argument may be in a separate
    /// argument from its name.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if names and values can be in separate arguments; <see langword="false"/> if the characters
    ///   specified in the <see cref="NameValueSeparators"/> property must be used. The default
    ///   value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="AllowWhiteSpaceValueSeparator"/> property is <see langword="true"/>,
    ///   the value of an argument can be separated from its name either by using the characters
    ///   specified in the <see cref="NameValueSeparators"/> property or by using white space (i.e.
    ///   by having a second argument that has the value). Given a named argument named "Sample",
    ///   the command lines <c>-Sample:value</c> and <c>-Sample value</c>
    ///   are both valid and will assign the value "value" to the argument.
    /// </para>
    /// <para>
    ///   If the <see cref="AllowWhiteSpaceValueSeparator"/> property is <see langword="false"/>, only the characters
    ///   specified in the <see cref="NameValueSeparators"/> property are allowed to separate the value from the name.
    ///   The command line <c>-Sample:value</c> still assigns the value "value" to the argument, but for the command line "-Sample value" the argument 
    ///   is considered not to have a value (which is only valid if <see cref="CommandLineArgument.IsSwitch"/> is <see langword="true"/>), and
    ///   "value" is considered to be the value for the next positional argument.
    /// </para>
    /// <para>
    ///   For switch arguments (the <see cref="CommandLineArgument.IsSwitch"/> property is <see langword="true"/>),
    ///   only the characters specified in the <see cref="NameValueSeparators"/> property are allowed
    ///   to specify an explicit value regardless of the value of the <see cref="AllowWhiteSpaceValueSeparator"/>
    ///   property. Given a switch argument named "Switch"  the command line <c>-Switch false</c>
    ///   is interpreted to mean that the value of "Switch" is <see langword="true"/> and the value of the
    ///   next positional argument is "false", even if the <see cref="AllowWhiteSpaceValueSeparator"/>
    ///   property is <see langword="true"/>.
    /// </para>
    /// <para>
    ///   Use the <see cref="ParseOptions"/> or <see cref="ParseOptionsAttribute "/> class to
    ///   change this value.
    /// </para>
    /// </remarks>
    /// <seealso cref="ParseOptionsAttribute.AllowWhiteSpaceValueSeparator"/>
    /// <seealso cref="ParseOptions.AllowWhiteSpaceValueSeparator"/>
    public bool AllowWhiteSpaceValueSeparator => _parseOptions.AllowWhiteSpaceValueSeparatorOrDefault;

    /// <summary>
    /// Gets the characters used to separate the name and the value of an argument.
    /// </summary>
    /// <value>
    ///   The characters used to separate the name and the value of an argument.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Use the <see cref="ParseOptions"/> or <see cref="ParseOptionsAttribute "/> class to
    ///   change this value.
    /// </para>
    /// </remarks>
    /// <seealso cref="ParseOptionsAttribute.NameValueSeparators"/>
    /// <seealso cref="ParseOptions.NameValueSeparators"/>
    public ImmutableArray<char> NameValueSeparators => _nameValueSeparators;

    /// <summary>
    /// Gets or sets a value that indicates whether usage help should be displayed if the <see cref="Parse(string[], int)"/>
    /// method returned <see langword="null"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if usage help should be displayed; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Check this property after calling the <see cref="Parse(string[], int)"/> method
    ///   to see if usage help should be displayed.
    /// </para>
    /// <para>
    ///   This property will be <see langword="true"/> if the <see cref="Parse(string[], int)"/>
    ///   method threw a <see cref="CommandLineArgumentException"/>, if an argument used
    ///   <see cref="CommandLineArgumentAttribute.CancelParsing"/>, if parsing was canceled
    ///   using the <see cref="ArgumentParsed"/> event.
    /// </para>
    /// <para>
    ///   If an argument that is defined by a method (<see cref="ArgumentKind.Method"/>) cancels
    ///   parsing by returning <see langword="false"/> from the method, this property is <em>not</em>
    ///   automatically set to <see langword="true"/>. Instead, the method should explicitly
    ///   set the <see cref="HelpRequested"/> property if it wants usage help to be displayed.
    /// </para>
    /// <code>
    /// [CommandLineArgument]
    /// public static bool MethodArgument(CommandLineParser parser)
    /// {
    ///     parser.HelpRequested = true;
    ///     return false;
    /// }
    /// </code>
    /// <para>
    ///   The <see cref="HelpRequested"/> property will always be <see langword="false"/> if
    ///   <see cref="Parse(string[], int)"/> did not throw and returned a non-null value.
    /// </para>
    /// </remarks>
    public bool HelpRequested { get; set; }

    /// <summary>
    /// Gets the <see cref="LocalizedStringProvider"/> implementation used to get strings for
    /// error messages and usage help.
    /// </summary>
    /// <value>
    /// An instance of a class inheriting from the <see cref="LocalizedStringProvider"/> class.
    /// </value>
    /// <seealso cref="ParseOptions.StringProvider"/>
    public LocalizedStringProvider StringProvider => _parseOptions.StringProvider;

    /// <summary>
    /// Gets the class validators for the arguments class.
    /// </summary>
    /// <value>
    /// A list of <see cref="ClassValidationAttribute"/> instances.
    /// </value>
    public IEnumerable<ClassValidationAttribute> Validators
        => ArgumentsType.GetCustomAttributes<ClassValidationAttribute>();

    /// <summary>
    /// Gets the string comparer used for argument names.
    /// </summary>
    /// <value>
    /// One of the members of the <see cref="StringComparison"/> enumeration.
    /// </value>
    /// <seealso cref="ParseOptionsAttribute.CaseSensitive"/>
    /// <seealso cref="ParseOptions.ArgumentNameComparison"/>
    public StringComparison ArgumentNameComparison { get; }

    /// <summary>
    /// Gets the arguments supported by this <see cref="CommandLineParser"/> instance.
    /// </summary>
    /// <value>
    /// A list of all the arguments.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The <see cref="Arguments"/> property can be used to retrieve additional information about the arguments, including their name, description,
    ///   and default value. Their current value can also be retrieved this way, in addition to using the arguments type directly.
    /// </para>
    /// </remarks>
    public ImmutableArray<CommandLineArgument> Arguments => _arguments;

    /// <summary>
    /// Gets the automatic help argument or an argument with the same name, if there is one.
    /// </summary>
    /// <value>
    /// A <see cref="CommandLineArgument"/> instance, or <see langword="null"/> if there is no
    /// argument using the name of the automatic help argument.
    /// </value>
    public CommandLineArgument? HelpArgument { get; private set; }

    /// <summary>
    /// Gets the result of the last call to the <see cref="Parse(string[], int)"/> method.
    /// </summary>
    /// <value>
    /// An instance of the <see cref="CommandLine.ParseResult"/> class.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Use this property to get the name of the argument that canceled parsing, or to get
    ///   error information if the <see cref="ParseWithErrorHandling()"/> method returns
    ///   <see langword="null"/>.
    /// </para>
    /// </remarks>
    public ParseResult ParseResult { get; private set; }

    /// <summary>
    /// Gets the kind of provider that was used to determine the available arguments.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="Support.ProviderKind"/> enumeration.
    /// </value>
    public ProviderKind ProviderKind => _provider.Kind;

    internal IComparer<char>? ShortArgumentNameComparer => _argumentsByShortName?.Comparer;


    /// <summary>
    /// Gets the name of the executable used to invoke the application.
    /// </summary>
    /// <param name="includeExtension">
    ///   <see langword="true"/> to include the file name extension in the result; otherwise,
    ///   <see langword="false"/>.
    /// </param>
    /// <returns>
    /// The file name of the application's executable, with or without extension.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   To determine the executable name, this method first checks the <see cref="Environment.ProcessPath"/>
    ///   property (if using .Net 6.0 or later). If using the .Net Standard package, or if
    ///   <see cref="Environment.ProcessPath"/> returns "dotnet", it checks the first item in
    ///   the array returned by <see cref="Environment.GetCommandLineArgs"/>, and finally falls
    ///   back to the file name of the entry point assembly.
    /// </para>
    /// <para>
    ///   The return value of this function is used as the default executable name to show in
    ///   the usage syntax when generating usage help, unless overridden by the <see cref="UsageWriter.ExecutableName"/>
    ///   property.
    /// </para>
    /// </remarks>
    /// <seealso cref="UsageWriter.IncludeExecutableExtension"/>
    public static string GetExecutableName(bool includeExtension = false)
    {
        string? path = null;
        string? nameWithoutExtension = null;
#if NET6_0_OR_GREATER
        // Prefer this because it actually returns the exe name, not the dll.
        path = Environment.ProcessPath;

        // Fall back if this returned the dotnet executable.
        nameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        if (nameWithoutExtension == "dotnet")
        {
            path = null;
            nameWithoutExtension = null;
        }
#endif
        path ??= Environment.GetCommandLineArgs().FirstOrDefault() ?? Assembly.GetEntryAssembly()?.Location;
        if (path == null)
        {
            path = string.Empty;
        }
        else if (includeExtension)
        {
            path = Path.GetFileName(path);
        }
        else
        {
            path = nameWithoutExtension ?? Path.GetFileNameWithoutExtension(path);
        }

        return path;
    }

    /// <summary>
    /// Writes command line usage help to the specified <see cref="TextWriter"/> using the specified options.
    /// </summary>
    /// <param name="usageWriter">
    ///   The <see cref="UsageWriter"/> to use to create the usage. If <see langword="null"/>,
    ///   the value from the <see cref="ParseOptions.UsageWriter"/> property in the
    ///   <see cref="Options"/> property is sued.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     The usage help consists of first the <see cref="Description"/>, followed by the usage syntax, followed by a description of all the arguments.
    ///   </para>
    ///   <para>
    ///     You can add descriptions to the usage text by applying the <see cref="DescriptionAttribute"/> attribute to your command line arguments type,
    ///     and the constructor parameters and properties defining command line arguments.
    ///   </para>
    ///   <para>
    ///     Color is applied to the output only if the <see cref="UsageWriter"/> instance
    ///     has enabled it.
    ///   </para>
    /// </remarks>
    /// <seealso cref="GetUsage"/>
    public void WriteUsage(UsageWriter? usageWriter = null)
    {
        usageWriter ??= _parseOptions.UsageWriter;
        usageWriter.WriteParserUsage(this);
    }

    /// <summary>
    /// Gets a string containing command line usage help.
    /// </summary>
    /// <param name="maximumLineLength">
    ///   The maximum line length of lines in the usage text. A value less than 1 or larger
    ///   than 65536 is interpreted as infinite line length.
    /// </param>
    /// <param name="usageWriter">
    ///   The <see cref="UsageWriter"/> to use to create the usage. If <see langword="null"/>,
    ///   the value from the <see cref="ParseOptions.UsageWriter"/> property in the
    ///   <see cref="Options"/> property is sued.
    /// </param>
    /// <returns>
    ///   A string containing usage help for the command line options defined by the type
    ///   specified by <see cref="ArgumentsType"/>.
    /// </returns>
    /// <remarks>
    ///   <inheritdoc cref="WriteUsage"/>
    /// </remarks>
    public string GetUsage(UsageWriter? usageWriter = null, int maximumLineLength = 0)
    {
        usageWriter ??= _parseOptions.UsageWriter;
        return usageWriter.GetUsage(this, maximumLineLength: maximumLineLength);
    }

    /// <summary>
    /// Parses the arguments returned by the <see cref="Environment.GetCommandLineArgs"/>
    /// method.
    /// </summary>
    /// <returns>
    ///   An instance of the type specified by the <see cref="ArgumentsType"/> property, or
    ///   <see langword="null"/> if argument parsing was canceled by the <see cref="ArgumentParsed"/>
    ///   event handler, the <see cref="CommandLineArgumentAttribute.CancelParsing"/> property,
    ///   or a method argument that returned <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   If the return value is <see langword="null"/>, check the <see cref="HelpRequested"/>
    ///   property to see if usage help should be displayed.
    /// </para>
    /// </remarks>
    /// <exception cref="CommandLineArgumentException">
    ///   An error occurred parsing the command line. Check the <see cref="CommandLineArgumentException.Category"/>
    ///   property for the exact reason for the error.
    /// </exception>
    public object? Parse()
    {
        // GetCommandLineArgs include the executable, so skip it.
        return Parse(Environment.GetCommandLineArgs(), 1);
    }

    /// <inheritdoc cref="Parse()" />
    /// <summary>
    /// Parses the specified command line arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public object? Parse(ReadOnlyMemory<string> args)
    {
        int index = -1;
        try
        {
            HelpRequested = false;
            return ParseCore(args, ref index);
        }
        catch (CommandLineArgumentException ex)
        {
            HelpRequested = true;
            ParseResult = ParseResult.FromException(ex, args.Slice(index));
            throw;
        }
    }

    /// <inheritdoc cref="Parse()" />
    /// <summary>
    /// Parses the specified command line arguments, starting at the specified index.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <param name="index">The index of the first argument to parse.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="args"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="index"/> does not fall within the bounds of <paramref name="args"/>.
    /// </exception>
    public object? Parse(string[] args, int index = 0)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(index));
        }

        if (index < 0 || index > args.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return Parse(args.AsMemory(index));
    }

    /// <summary>
    /// Parses the arguments returned by the <see cref="Environment.GetCommandLineArgs"/>
    /// method, and displays error messages and usage help if required.
    /// </summary>
    /// <returns>
    ///   An instance of the type specified by the <see cref="ArgumentsType"/> property, or
    ///   <see langword="null"/> if an error occurred, or argument parsing was canceled by the
    ///   <see cref="CommandLineArgumentAttribute.CancelParsing"/> property or a method argument
    ///   that returned <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   If an error occurs or parsing is canceled, it prints errors to the <see
    ///   cref="ParseOptions.Error"/> stream, and usage help to the <see cref="UsageWriter"/> if
    ///   the <see cref="HelpRequested"/> property is <see langword="true"/>. It then returns
    ///   <see langword="null"/>.
    /// </para>
    /// <para>
    ///   If the return value is <see langword="null"/>, check the <see cref="ParseResult"/>
    ///   property for more information about whether an error occurred or parsing was
    ///   canceled.
    /// </para>
    /// <para>
    ///   This method will never throw a <see cref="CommandLineArgumentException"/> exception.
    /// </para>
    /// </remarks>
    public object? ParseWithErrorHandling()
    {
        // GetCommandLineArgs include the executable, so skip it.
        return ParseWithErrorHandling(Environment.GetCommandLineArgs(), 1);
    }

    /// <inheritdoc cref="ParseWithErrorHandling()" />
    /// <summary>
    /// Parses the specified command line arguments, starting at the specified index, and
    /// displays error messages and usage help if required.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <param name="index">The index of the first argument to parse.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="args"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="index"/> does not fall within the bounds of <paramref name="args"/>.
    /// </exception>
    public object? ParseWithErrorHandling(string[] args, int index = 0)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(index));
        }

        if (index < 0 || index > args.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return ParseWithErrorHandling(args.AsMemory(index));
    }

    /// <inheritdoc cref="ParseWithErrorHandling()" />
    /// <summary>
    /// Parses the specified command line arguments, and displays error messages and usage help if
    /// required.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public object? ParseWithErrorHandling(ReadOnlyMemory<string> args)
    {
        EventHandler<DuplicateArgumentEventArgs>? handler = null;
        if (_parseOptions.DuplicateArgumentsOrDefault == ErrorMode.Warning)
        {
            handler = (sender, e) =>
            {
                var warning = StringProvider.DuplicateArgumentWarning(e.Argument.ArgumentName);
                WriteError(_parseOptions, warning, _parseOptions.WarningColor);
            };

            DuplicateArgument += handler;
        }

        var helpMode = UsageHelpRequest.Full;
        object? result = null;
        try
        {
            result = Parse(args);
        }
        catch (CommandLineArgumentException ex)
        {
            WriteError(_parseOptions, ex.Message, _parseOptions.ErrorColor, true);
            helpMode = _parseOptions.ShowUsageOnError;
        }
        finally
        {
            if (handler != null)
            {
                DuplicateArgument -= handler;
            }
        }

        if (HelpRequested)
        {
            _parseOptions.UsageWriter.WriteParserUsage(this, helpMode);
        }

        return result;
    }

    /// <summary>
    /// Parses the arguments returned by the <see cref="Environment.GetCommandLineArgs"/>
    /// method using the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type defining the command line arguments.</typeparam>
    /// <param name="options">
    ///   The options that control parsing behavior and usage help formatting. If
    ///   <see langword="null" />, the default options are used.
    /// </param>
    /// <returns>
    ///   An instance of the type <typeparamref name="T"/>, or <see langword="null"/> if an
    ///   error occurred, or argument parsing was canceled by the <see cref="CommandLineArgumentAttribute.CancelParsing"/>
    ///   property or a method argument that returned <see langword="false"/>.
    /// </returns>
    /// <exception cref="CommandLineArgumentException">
    ///   <inheritdoc cref="Parse()"/>
    /// </exception>
    /// <exception cref="NotSupportedException">
    ///   The <see cref="CommandLineParser"/> cannot use <typeparamref name="T"/> as the command
    ///   line arguments type, because it violates one of the rules concerning argument names or
    ///   positions, or has an argument type that cannot be parsed.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This is a convenience function that instantiates a <see cref="CommandLineParser"/>,
    ///   calls the <see cref="Parse()"/> method, and returns the result. If an error occurs
    ///   or parsing is canceled, it prints errors to the <see cref="ParseOptions.Error"/>
    ///   stream, and usage help to the <see cref="UsageWriter"/> if the <see cref="HelpRequested"/>
    ///   property is <see langword="true"/>. It then returns <see langword="null"/>.
    /// </para>
    /// <para>
    ///   If the <see cref="ParseOptions.Error"/> parameter is <see langword="null"/>, output is
    ///   written to a <see cref="LineWrappingTextWriter"/> for the standard error stream,
    ///   wrapping at the console's window width. If the stream is redirected, output may still
    ///   be wrapped, depending on the value returned by <see cref="Console.WindowWidth"/>.
    /// </para>
    /// <para>
    ///   Color is applied to the output depending on the value of the <see cref="UsageWriter.UseColor"/>
    ///   property, the <see cref="ParseOptions.UseErrorColor"/> property, and the capabilities
    ///   of the console.
    /// </para>
    /// <para>
    ///   If you want more control over the parsing process, including custom error/usage output
    ///   or handling the <see cref="ArgumentParsed"/> event, you should manually create an
    ///   instance of the <see cref="CommandLineParser{T}"/> class and call its <see cref="CommandLineParser{T}.Parse()"/>
    ///   method.
    /// </para>
    /// <para>
    ///   This method uses reflection to determine the arguments defined by the type <typeparamref name="T"/>
    ///   at runtime, unless the type has the <see cref="GeneratedParserAttribute"/> applied. In
    ///   that case, you can also use the generated static <c>CreateParser()</c> or <c>Parse()</c>
    ///   methods on that type instead.
    /// </para>
    /// </remarks>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Argument information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute.", Url = UnreferencedCodeHelpUrl)]
#endif
    public static T? Parse<T>(ParseOptions? options = null)
        where T : class
    {
        // GetCommandLineArgs include the executable, so skip it.
        return Parse<T>(Environment.GetCommandLineArgs(), 1, options);
    }

    /// <summary>
    /// Parses the specified command line arguments, starting at the specified index, using the
    /// type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type defining the command line arguments.</typeparam>
    /// <param name="args">The command line arguments.</param>
    /// <param name="index">The index of the first argument to parse.</param>
    /// <param name="options">
    ///   The options that control parsing behavior and usage help formatting. If
    ///   <see langword="null" />, the default options are used.
    /// </param>
    /// <returns>
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="args"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="index"/> does not fall within the bounds of <paramref name="args"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </exception>
    /// <exception cref="NotSupportedException">
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </exception>
    /// <exception cref="CommandLineArgumentException">
    ///   <inheritdoc cref="Parse()"/>
    /// </exception>
    /// <remarks>
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </remarks>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Argument information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute.", Url = UnreferencedCodeHelpUrl)]
#endif
    public static T? Parse<T>(string[] args, int index, ParseOptions? options = null)
        where T : class
    {
        options ??= new();
        var parser = new CommandLineParser(typeof(T), options);
        return (T?)parser.ParseWithErrorHandling(args, index);
    }

    /// <summary>
    /// Parses the specified command line arguments using the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type defining the command line arguments.</typeparam>
    /// <param name="args">The command line arguments.</param>
    /// <param name="options">
    ///   The options that control parsing behavior and usage help formatting. If
    ///   <see langword="null" />, the default options are used.
    /// </param>
    /// <returns>
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="args"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="CommandLineArgumentException">
    ///   <inheritdoc cref="Parse()"/>
    /// </exception>
    /// <exception cref="NotSupportedException">
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </exception>
    /// <remarks>
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </remarks>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Argument information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute.", Url = UnreferencedCodeHelpUrl)]
#endif
    public static T? Parse<T>(string[] args, ParseOptions? options = null)
        where T : class
    {
        return Parse<T>(args, 0, options);
    }

    /// <summary>
    /// Gets a command line argument by name or alias.
    /// </summary>
    /// <param name="name">The name or alias of the argument.</param>
    /// <returns>The <see cref="CommandLineArgument"/> instance containing information about
    /// the argument, or <see langword="null" /> if the argument was not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    ///   If the <see cref="Mode"/> property is <see cref="ParsingMode.LongShort"/>, this uses
    ///   the long name and long aliases of the argument.
    /// </remarks>
    public CommandLineArgument? GetArgument(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (_argumentsByName.TryGetValue(name.AsMemory(), out var argument))
        {
            return argument;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets a command line argument by short name.
    /// </summary>
    /// <param name="shortName">The short name of the argument.</param>
    /// <returns>The <see cref="CommandLineArgument"/> instance containing information about
    /// the argument, or <see langword="null" /> if the argument was not found.</returns>
    /// <remarks>
    /// <para>
    ///   If <see cref="Mode"/> is not <see cref="ParsingMode.LongShort"/>, this
    ///   method always returns <see langword="null"/>
    /// </para>
    /// </remarks>
    public CommandLineArgument? GetShortArgument(char shortName)
    {
        if (_argumentsByShortName != null && _argumentsByShortName.TryGetValue(shortName, out var argument))
        {
            return argument;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the default argument name prefixes for the current platform.
    /// </summary>
    /// <returns>
    /// An array containing the default prefixes for the current platform.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   The default prefixes for each platform are:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Platform</term>
    ///     <description>Prefixes</description>
    ///   </listheader>
    ///   <item>
    ///     <term>Windows</term>
    ///     <description>'-' and '/'</description>
    ///   </item>
    ///   <item>
    ///     <term>Other</term>
    ///     <description>'-'</description>
    ///   </item>
    /// </list>
    /// <para>
    ///   If the <see cref="Mode"/> property is <see cref="ParsingMode.LongShort"/>, these
    ///   prefixes will be used for short argument names. The <see cref="DefaultLongArgumentNamePrefix"/>
    ///   constant is the default prefix for long argument names regardless of platform.
    /// </para>
    /// </remarks>
    /// <seealso cref="ArgumentNamePrefixes"/>
    /// <seealso cref="ParseOptionsAttribute.ArgumentNamePrefixes"/>
    /// <seealso cref="ParseOptions.ArgumentNamePrefixes"/>
    public static ImmutableArray<string> GetDefaultArgumentNamePrefixes()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ImmutableArray.Create("-", "/")
            : ImmutableArray.Create("-");
    }

    /// <summary>
    /// Gets the default character used to separate the name and the value of an argument.
    /// </summary>
    /// <returns>
    /// The default characters used to separate the name and the value of an argument, which are
    /// ':' and '='.
    /// </returns>
    /// <remarks>
    /// The return value of this method is used as the default value of the <see cref="NameValueSeparators"/> property.
    /// </remarks>
    /// <seealso cref="AllowWhiteSpaceValueSeparator"/>
    public static ImmutableArray<char> GetDefaultNameValueSeparators() => ImmutableArray.Create(':', '=');

    /// <summary>
    /// Raises the <see cref="ArgumentParsed"/> event.
    /// </summary>
    /// <param name="e">The data for the event.</param>
    protected virtual void OnArgumentParsed(ArgumentParsedEventArgs e)
    {
        ArgumentParsed?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="DuplicateArgument"/> event.
    /// </summary>
    /// <param name="e">The data for the event.</param>
    protected virtual void OnDuplicateArgument(DuplicateArgumentEventArgs e)
    {
        DuplicateArgument?.Invoke(this, e);
    }

    internal static bool ShouldIndent(LineWrappingTextWriter writer)
    {
        return writer.MaximumLineLength is 0 or >= 30;
    }

    internal static void WriteError(ParseOptions options, string message, string color, bool blankLine = false)
    {
        using var errorVtSupport = options.EnableErrorColor();
        try
        {
            using var error = DisposableWrapper.Create(options.Error, LineWrappingTextWriter.ForConsoleError);
            if (options.UseErrorColor ?? false)
            {
                error.Inner.Write(color);
            }

            error.Inner.Write(message);
            if (options.UseErrorColor ?? false)
            {
                error.Inner.Write(options.UsageWriter.ColorReset);
            }

            error.Inner.WriteLine();
            if (blankLine)
            {
                error.Inner.WriteLine();
            }
        }
        finally
        {
            // Reset UseErrorColor if it was changed.
            if (errorVtSupport != null)
            {
                options.UseErrorColor = null;
            }
        }
    }

    private static ImmutableArray<string> DetermineArgumentNamePrefixes(ParseOptions options)
    {
        if (options.ArgumentNamePrefixes == null)
        {
            return GetDefaultArgumentNamePrefixes();
        }
        else
        {
            var result = options.ArgumentNamePrefixes.ToImmutableArray();
            if (result.Length == 0)
            {
                throw new ArgumentException(Properties.Resources.EmptyArgumentNamePrefixes, nameof(options));
            }

            if (result.Any(prefix => string.IsNullOrWhiteSpace(prefix)))
            {
                throw new ArgumentException(Properties.Resources.EmptyArgumentNamePrefix, nameof(options));
            }

            return result;
        }
    }

    private static ImmutableArray<char> DetermineNameValueSeparators(ParseOptions options)
    {
        if (options.NameValueSeparators == null)
        {
            return GetDefaultNameValueSeparators();
        }
        else
        {
            var result = options.NameValueSeparators.ToImmutableArray();
            if (result.Length == 0)
            {
                throw new ArgumentException(Properties.Resources.EmptyNameValueSeparators, nameof(options));
            }

            return result;
        }
    }

    private int DetermineMemberArguments(ImmutableArray<CommandLineArgument>.Builder builder)
    {
        int additionalPositionalArgumentCount = 0;
        foreach (var argument in _provider.GetArguments(this))
        {
            AddNamedArgument(argument, builder);
            if (argument.Position != null)
            {
                ++additionalPositionalArgumentCount;
            }
        }

        return additionalPositionalArgumentCount;
    }

    private void DetermineAutomaticArguments(ImmutableArray<CommandLineArgument>.Builder builder)
    {
        bool autoHelp = Options.AutoHelpArgumentOrDefault;
        if (autoHelp)
        {
            var (argument, created) = CommandLineArgument.CreateAutomaticHelp(this);

            if (created)
            {
                AddNamedArgument(argument, builder);
            }

            HelpArgument = argument;
        }

        bool autoVersion = Options.AutoVersionArgumentOrDefault;
        if (autoVersion && !_provider.IsCommand)
        {
            var argument = CommandLineArgument.CreateAutomaticVersion(this);

            if (argument != null)
            {
                AddNamedArgument(argument, builder);
            }
        }
    }

    private void AddNamedArgument(CommandLineArgument argument, ImmutableArray<CommandLineArgument>.Builder builder)
    {
        if (_nameValueSeparators.Any(separator => argument.ArgumentName.Contains(separator)))
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.ArgumentNameContainsSeparatorFormat, argument.ArgumentName));
        }

        if (argument.HasLongName)
        {
            _argumentsByName.Add(argument.ArgumentName.AsMemory(), argument);
            foreach (string alias in argument.Aliases)
            {
                _argumentsByName.Add(alias.AsMemory(), argument);
            }
        }

        if (_argumentsByShortName != null && argument.HasShortName)
        {
            _argumentsByShortName.Add(argument.ShortName, argument);
            foreach (var alias in argument.ShortAliases)
            {
                _argumentsByShortName.Add(alias, argument);
            }
        }

        // The generated provider needs values for arguments that use a required property to be
        // supplied to the CreateInstance method in the exact order they were originally returned,
        // so a separate list is maintained for that. The reflection provider doesn't need these
        // values at all.
        if (_provider.Kind != ProviderKind.Reflection && argument.IsRequiredProperty)
        {
            _requiredPropertyArguments ??= new();
            _requiredPropertyArguments.Add(argument);
        }

        builder.Add(argument);
    }

    private void VerifyPositionalArgumentRules()
    {
        bool hasOptionalArgument = false;
        bool hasArrayArgument = false;

        for (int x = 0; x < _positionalArgumentCount; ++x)
        {
            CommandLineArgument argument = _arguments[x];

            if (hasArrayArgument)
            {
                throw new NotSupportedException(Properties.Resources.ArrayNotLastArgument);
            }

            if (argument.IsRequired && hasOptionalArgument)
            {
                throw new NotSupportedException(Properties.Resources.InvalidOptionalArgumentOrder);
            }

            if (!argument.IsRequired)
            {
                hasOptionalArgument = true;
            }

            if (argument.IsMultiValue)
            {
                hasArrayArgument = true;
            }

            argument.Position = x;
        }
    }

    private object? ParseCore(ReadOnlyMemory<string> args, ref int x)
    {
        // Reset all arguments to their default value.
        foreach (CommandLineArgument argument in _arguments)
        {
            argument.Reset();
        }

        HelpRequested = false;
        int positionalArgumentIndex = 0;

        var cancelParsing = CancelMode.None;
        CommandLineArgument? lastArgument = null;
        for (x = 0; x < args.Length; ++x)
        {
            string arg = args.Span[x];
            var argumentNamePrefix = CheckArgumentNamePrefix(arg);
            if (argumentNamePrefix != null)
            {
                // If white space was the value separator, this function returns the index of argument containing the value for the named argument.
                // It returns -1 if parsing was canceled by the ArgumentParsed event handler or the CancelParsing property.
                (cancelParsing, x, lastArgument) = ParseNamedArgument(args.Span, x, argumentNamePrefix.Value);
                if (cancelParsing != CancelMode.None)
                {
                    break;
                }
            }
            else
            {
                // If this is a multi-value argument is must be the last argument.
                if (positionalArgumentIndex < _positionalArgumentCount && !_arguments[positionalArgumentIndex].IsMultiValue)
                {
                    // Skip named positional arguments that have already been specified by name.
                    while (positionalArgumentIndex < _positionalArgumentCount && !_arguments[positionalArgumentIndex].IsMultiValue && _arguments[positionalArgumentIndex].HasValue)
                    {
                        ++positionalArgumentIndex;
                    }
                }

                if (positionalArgumentIndex >= _positionalArgumentCount)
                {
                    throw StringProvider.CreateException(CommandLineArgumentErrorCategory.TooManyArguments);
                }

                lastArgument = _arguments[positionalArgumentIndex];
                cancelParsing = ParseArgumentValue(lastArgument, arg, arg.AsMemory());
                if (cancelParsing != CancelMode.None)
                {
                    break;
                }
            }
        }

        if (cancelParsing == CancelMode.Abort)
        {
            ParseResult = ParseResult.FromCanceled(lastArgument!.ArgumentName, args.Slice(x + 1));
            return null;
        }

        // Check required arguments and post-parsing validation. This is done in usage order.
        foreach (CommandLineArgument argument in _arguments)
        {
            argument.ValidateAfterParsing();
        }

        // Run class validators.
        _provider.RunValidators(this);

        object commandLineArguments;
        try
        {
            object?[]? requiredPropertyValues = null;
            if (_requiredPropertyArguments != null)
            {
                requiredPropertyValues = new object?[_requiredPropertyArguments.Count];
                for (int i = 0; i < requiredPropertyValues.Length; ++i)
                {
                    requiredPropertyValues[i] = _requiredPropertyArguments[i].Value;
                }
            }

            commandLineArguments = _provider.CreateInstance(this, requiredPropertyValues);
        }
        catch (TargetInvocationException ex)
        {
            throw StringProvider.CreateException(CommandLineArgumentErrorCategory.CreateArgumentsTypeError, ex.InnerException);
        }
        catch (Exception ex)
        {
            throw StringProvider.CreateException(CommandLineArgumentErrorCategory.CreateArgumentsTypeError, ex);
        }

        foreach (CommandLineArgument argument in _arguments)
        {
            // Apply property argument values (this does nothing for constructor or method arguments).
            argument.ApplyPropertyValue(commandLineArguments);
        }

        ParseResult = cancelParsing == CancelMode.None
            ? ParseResult.FromSuccess()
            : ParseResult.FromSuccess(lastArgument!.ArgumentName, args.Slice(x + 1));

        // Reset to false in case it was set by a method argument that didn't cancel parsing.
        HelpRequested = false;
        return commandLineArguments;
    }

    private CancelMode ParseArgumentValue(CommandLineArgument argument, string? stringValue, ReadOnlyMemory<char>? memoryValue)
    {
        if (argument.HasValue && !argument.IsMultiValue)
        {
            if (!AllowDuplicateArguments)
            {
                throw StringProvider.CreateException(CommandLineArgumentErrorCategory.DuplicateArgument, argument);
            }

            var duplicateEventArgs = stringValue == null
                ? new DuplicateArgumentEventArgs(argument, memoryValue.HasValue, memoryValue ?? default)
                : new DuplicateArgumentEventArgs(argument, stringValue);

            OnDuplicateArgument(duplicateEventArgs);
            if (duplicateEventArgs.KeepOldValue)
            {
                return CancelMode.None;
            }
        }

        var cancelParsing = argument.SetValue(Culture, memoryValue.HasValue, stringValue, (memoryValue ?? default).Span);
        var e = new ArgumentParsedEventArgs(argument)
        {
            CancelParsing = cancelParsing
        };

        if (argument.CancelParsing != CancelMode.None)
        {
            e.CancelParsing = argument.CancelParsing;
        }

        OnArgumentParsed(e);

        if (e.CancelParsing != CancelMode.None)
        {
            // Automatically request help only if the cancellation was due to the
            // CommandLineArgumentAttribute.CancelParsing property.
            if (argument.CancelParsing == CancelMode.Abort)
            {
                HelpRequested = true;
            }
        }

        return e.CancelParsing;
    }

    private (CancelMode, int, CommandLineArgument?) ParseNamedArgument(ReadOnlySpan<string> args, int index, PrefixInfo prefix)
    {
        var (argumentName, argumentValue) = args[index].AsMemory(prefix.Prefix.Length).SplitFirstOfAny(_nameValueSeparators.AsSpan());

        CancelMode cancelParsing;
        CommandLineArgument? argument = null;
        if (_argumentsByShortName != null && prefix.Short)
        {
            if (argumentName.Length == 1)
            {
                argument = GetShortArgumentOrThrow(argumentName.Span[0]);
            }
            else
            {
                CommandLineArgument? lastArgument;
                (cancelParsing, lastArgument) = ParseShortArgument(argumentName.Span, argumentValue);
                return (cancelParsing, index, lastArgument);
            }
        }

        if (argument == null && !_argumentsByName.TryGetValue(argumentName, out argument))
        {
            if (Options.AutoPrefixAliasesOrDefault)
            {
                argument = GetArgumentByNamePrefix(argumentName.Span);
            }

            if (argument == null)
            {
                throw StringProvider.CreateException(CommandLineArgumentErrorCategory.UnknownArgument, argumentName.ToString());
            }
        }

        argument.SetUsedArgumentName(argumentName);
        if (!argumentValue.HasValue && !argument.IsSwitch && AllowWhiteSpaceValueSeparator)
        {
            string? argumentValueString = null;

            // No separator was present but a value is required. We take the next argument as
            // its value. For multi-value arguments that can consume multiple values, we keep
            // going until we hit another argument name.
            while (index + 1 < args.Length && CheckArgumentNamePrefix(args[index + 1]) == null)
            {
                ++index;
                argumentValueString = args[index];

                cancelParsing = ParseArgumentValue(argument, argumentValueString, argumentValueString.AsMemory());
                if (cancelParsing != CancelMode.None)
                {
                    return (cancelParsing, index, argument);
                }

                if (!argument.AllowMultiValueWhiteSpaceSeparator)
                {
                    break;
                }
            }

            if (argumentValueString != null)
            {
                return (CancelMode.None, index, argument);
            }
        }

        // ParseArgumentValue returns true if parsing was canceled by the ArgumentParsed event handler
        // or the CancelParsing property.
        cancelParsing = ParseArgumentValue(argument, null, argumentValue);
        return (cancelParsing, index, argument);
    }

    private CommandLineArgument? GetArgumentByNamePrefix(ReadOnlySpan<char> prefix)
    {
        CommandLineArgument? foundArgument = null;
        foreach (var argument in _arguments)
        {
            // Skip arguments without a long name.
            if (Mode == ParsingMode.LongShort && !argument.HasLongName)
            {
                continue;
            }

            var matches = argument.ArgumentName.AsSpan().StartsWith(prefix, ArgumentNameComparison);
            if (!matches)
            {
                foreach (var alias in argument.Aliases)
                {
                    if (alias.AsSpan().StartsWith(prefix, ArgumentNameComparison))
                    {
                        matches = true;
                        break;
                    }
                }
            }

            if (matches)
            {
                if (foundArgument != null)
                {
                    // Prefix is not unique.
                    return null;
                }

                foundArgument = argument;
            }
        }

        return foundArgument;
    }

    private (CancelMode, CommandLineArgument?) ParseShortArgument(ReadOnlySpan<char> name, ReadOnlyMemory<char>? value)
    {
        CommandLineArgument? arg = null;
        foreach (var ch in name)
        {
            arg = GetShortArgumentOrThrow(ch);
            if (!arg.IsSwitch)
            {
                throw StringProvider.CreateException(CommandLineArgumentErrorCategory.CombinedShortNameNonSwitch, name.ToString());
            }

            var cancelParsing = ParseArgumentValue(arg, null, value);
            if (cancelParsing != CancelMode.None)
            {
                return (cancelParsing, arg);
            }
        }

        return (CancelMode.None, arg);
    }

    private CommandLineArgument GetShortArgumentOrThrow(char shortName)
    {
        if (_argumentsByShortName!.TryGetValue(shortName, out CommandLineArgument? argument))
        {
            return argument;
        }

        throw StringProvider.CreateException(CommandLineArgumentErrorCategory.UnknownArgument, shortName.ToString());
    }

    private PrefixInfo? CheckArgumentNamePrefix(string argument)
    {
        // Even if '-' is the argument name prefix, we consider an argument starting with dash followed by a digit as a value, because it could be a negative number.
        if (argument.Length >= 2 && argument[0] == '-' && char.IsDigit(argument, 1))
        {
            return null;
        }

        foreach (var prefix in _sortedPrefixes)
        {
            if (argument.StartsWith(prefix.Prefix, StringComparison.Ordinal))
            {
                return prefix;
            }
        }

        return null;
    }

#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Argument information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute.", Url = UnreferencedCodeHelpUrl)]
#endif
    private static ArgumentProvider GetArgumentProvider(Type type, ParseOptions? options)
    {
        // Try to use the generated provider if it exists.
        var forceReflection = options?.ForceReflection ?? ParseOptions.ForceReflectionDefault;
        if (!forceReflection && Attribute.IsDefined(type, typeof(GeneratedParserAttribute)))
        {
            var providerType = type.GetNestedType("OokiiCommandLineArgumentProvider", BindingFlags.NonPublic);
            if (providerType != null && typeof(ArgumentProvider).IsAssignableFrom(providerType))
            {
                return (ArgumentProvider)Activator.CreateInstance(providerType)!;
            }
        }

        return new ReflectionArgumentProvider(type);
    }
}
