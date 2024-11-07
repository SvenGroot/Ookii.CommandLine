using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Support;
using Ookii.CommandLine.Terminal;
using Ookii.CommandLine.Validation;
using Ookii.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
/// Parses command line arguments defined by a type's properties and methods.
/// </summary>
/// <remarks>
/// <para>
///   The <see cref="CommandLineParser"/> class parses command line arguments into named,
///   strongly-typed values. The accepted arguments are defined by the properties and methods of the
///   type passed to the <see cref="CommandLineParser(Type, ParseOptions)"/> constructor. The result
///   of a parsing operation is an instance of that type, created using the values that were
///   supplied on the command line.
/// </para>
/// <para>
///   The arguments type must have a constructor that has no parameters, or a single parameter
///   with the type <see cref="CommandLineParser"/>, which will receive the instance of the
///   <see cref="CommandLineParser"/> class that was used to parse the arguments.
/// </para>
/// <para>
///   A property defines a command line argument if it is <see langword="public"/>, not
///   <see langword="static"/>, and has the <see cref="CommandLineArgumentAttribute"/> attribute
///   applied. The <see cref="CommandLineArgumentAttribute"/> attribute has properties to
///   determine the behavior of the argument, such as whether it's required or positional.
/// </para>
/// <para>
///   A method defines a command line argument if it is <see langword="public"/>, <see langword="static"/>,
///   has the <see cref="CommandLineArgumentAttribute"/> attribute applied, and one of the
///   signatures shown in the documentation for the <see cref="CommandLineArgumentAttribute"/>
///   attribute.
/// </para>
/// <para>
///   To parse arguments, invoke the <see cref="Parse()"/> method or one of its overloads, or use
///   <see cref="ParseWithErrorHandling()"/> or one of its overloads to automatically handle
///   errors and print usage help when requested.
/// </para>
/// <para>
///   The static <see cref="Parse{T}(ParseOptions)"/> method is a helper that create a
///   <see cref="CommandLineParser"/> instance, and parse arguments with error handling in a single
///   call. If using source generation with the <see cref="GeneratedParserAttribute"/> attribute,
///   you can also use the generated <see cref="IParser{TSelf}.Parse(Ookii.CommandLine.ParseOptions?)" qualifyHint="true"/>
///   method.
/// </para>
/// <para>
///   The derived type <see cref="CommandLineParser{T}"/> provides strongly-typed instance <see
///   cref="CommandLineParser{T}.Parse()"/> and <see cref="CommandLineParser{T}.ParseWithErrorHandling()" qualifyHint="true"/>
///   methods, if you don't wish to use the static methods.
/// </para>
/// <para>
///   The <see cref="CommandLineParser"/> class is for applications with a single (root) command.
///   If you wish to create an application with subcommands, use the <see cref="CommandManager"/>
///   class instead.
/// </para>
/// <para>
///   The <see cref="CommandLineParser"/> supports two sets of rules for how to parse arguments;
///   <see cref="ParsingMode.Default" qualifyHint="true"/> mode and <see cref="ParsingMode.LongShort" qualifyHint="true"/> mode. For
///   more details on these rules, please see
///   <see href="https://www.github.com/SvenGroot/Ookii.CommandLine">the documentation on GitHub</see>.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
/// <seealso cref="CommandLineParser{T}"/>
/// <seealso cref="CommandManager"/>
/// <seealso href="https://www.github.com/SvenGroot/Ookii.CommandLine">Usage documentation</seealso>
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

    private struct ParseState
    {
        public CommandLineParser Parser;

        public ReadOnlyMemory<string> Arguments;

        public int Index;

        public int PositionalArgumentIndex;

        public bool PositionalOnly;

        public CancelMode CancelParsing;

        public CommandLineArgument? Argument;

        public ReadOnlyMemory<char> ArgumentName;

        public ReadOnlyMemory<char>? ArgumentValue;

        public bool IsUnknown;

        public bool IsSpecifiedByPosition;

        public ImmutableArray<string>.Builder? PossibleMatches;

        public readonly CommandLineArgument? PositionalArgument
            => PositionalArgumentIndex < Parser._positionalArgumentCount ? Parser.Arguments[PositionalArgumentIndex] : null;

        public readonly string RealArgumentName => Argument?.ArgumentName ?? ArgumentName.ToString();

        public readonly ReadOnlyMemory<string> RemainingArguments => Arguments.Slice(Index + 1);

        public void ResetForNextArgument()
        {
            Argument = null;
            ArgumentName = default;
            ArgumentValue = null;
            IsUnknown = false;
            IsSpecifiedByPosition = false;
            PossibleMatches?.Clear();
        }
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
    /// Gets the default prefix used for long argument names if the <see cref="Mode"/>
    /// property is <see cref="ParsingMode.LongShort" qualifyHint="true"/>.
    /// </summary>
    /// <value>
    /// The default long argument name prefix, which is '--'.
    /// </value>
    /// <remarks>
    /// <para>
    /// This constant is used as the default value of the <see cref="LongArgumentNamePrefix"/>
    /// property if no custom value was specified using the <see cref="ParseOptions.LongArgumentNamePrefix" qualifyHint="true"/>
    /// property of the <see cref="ParseOptionsAttribute.LongArgumentNamePrefix" qualifyHint="true"/>
    /// property.
    /// </para>
    /// </remarks>
    public const string DefaultLongArgumentNamePrefix = "--";

    /// <summary>
    /// Event raised when an argument is parsed from the command line.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Set the <see cref="ArgumentParsedEventArgs.CancelParsing" qualifyHint="true"/> property in
    ///   the event handler to cancel parsing at the current argument. To have usage help shown
    ///   by the parse methods that do this automatically, you must set the <see cref="HelpRequested"/>
    ///   property to <see langword="true"/> explicitly in the event handler.
    /// </para>
    /// <para>
    ///   The <see cref="ArgumentParsedEventArgs.CancelParsing" qualifyHint="true"/> property is
    ///   initialized to the value of the <see cref="CommandLineArgumentAttribute.CancelParsing" qualifyHint="true"/>
    ///   property, or the method return value of an argument using <see cref="ArgumentKind.Method" qualifyHint="true"/>.
    ///   Reset the value to <see cref="CancelMode.None" qualifyHint="true"/> to continue parsing
    ///   anyway.
    /// </para>
    /// <para>
    ///   This event is invoked after the <see cref="CommandLineArgument.Value" qualifyHint="true"/>
    ///   and <see cref="CommandLineArgument.UsedArgumentName" qualifyHint="true"/> properties have
    ///   been set.
    /// </para>
    /// </remarks>
    public event EventHandler<ArgumentParsedEventArgs>? ArgumentParsed;

    /// <summary>
    /// Event raised when an argument that is not multi-value is specified more than once.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Handling this event allows you to inspect the new value, and decide to keep the old
    ///   or new value. It also allows you to, for instance, print a warning for duplicate
    ///   arguments.
    /// </para>
    /// <para>
    ///   This event is only raised when the <see cref="AllowDuplicateArguments"/> property is
    ///   <see langword="true"/>.
    /// </para>
    /// </remarks>
    public event EventHandler<DuplicateArgumentEventArgs>? DuplicateArgument;

    /// <summary>
    /// Event raised when an unknown argument name or a positional value with no matching argument
    /// is used.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Specifying an argument with an unknown name, or too many positional arguments, is normally
    ///   an error. By handling this event and setting the
    ///   <see cref="UnknownArgumentEventArgs.Ignore" qualifyHint="true"/> property to
    ///   <see langword="true"/>, you can instead continue parsing the remainder of the command
    ///   line, ignoring the unknown argument.
    /// </para>
    /// <para>
    ///   You can also cancel parsing instead using the
    ///   <see cref="UnknownArgumentEventArgs.CancelParsing" qualifyHint="true"/> property.
    /// </para>
    /// <para>
    ///   If an unknown argument name is encountered and is followed by a value separated by
    ///   whitespace, that value will be treated as the next positional argument value. It is not
    ///   considered to be a value for the unknown argument.
    /// </para>
    /// </remarks>
    public event EventHandler<UnknownArgumentEventArgs>? UnknownArgument;

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
    /// <note>
    ///   Instead of this constructor, it's recommended to use the <see cref="CommandLineParser{T}"/>
    ///   class instead.
    /// </note>
    /// <para>
    ///   This constructor uses reflection to determine the arguments defined by the type indicated
    ///   by <paramref name="argumentsType"/> at runtime, unless the type has the
    ///   <see cref="GeneratedParserAttribute"/> applied. For a type using that attribute, you can
    ///   also use the generated static <see cref="IParserProvider{TSelf}.CreateParser" qualifyHint="true"/> or 
    ///   <see cref="IParser{TSelf}.Parse(ParseOptions?)" qualifyHint="true"/> methods on the arguments class instead.
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Consider using the GeneratedParserAttribute.")]
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
    ///   positions, or has an argument type that cannot be parsed.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This constructor supports source generation, and should not typically be used directly
    ///   by application code.
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
    /// <seealso cref="GeneratedParserAttribute"/>
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
        if (_mode == ParsingMode.LongShort || _parseOptions.PrefixTerminationOrDefault != PrefixTerminationMode.None)
        {
            _longArgumentNamePrefix = _parseOptions.LongArgumentNamePrefixOrDefault;
            if (string.IsNullOrWhiteSpace(_longArgumentNamePrefix))
            {
                throw new ArgumentException(Properties.Resources.EmptyArgumentNamePrefix, nameof(options));
            }

            if (_mode == ParsingMode.LongShort)
            {
                var longInfo = new PrefixInfo { Prefix = _longArgumentNamePrefix, Short = false };
                prefixInfos = prefixInfos.Append(longInfo);
                _argumentsByShortName = new(new CharComparer(comparison));
            }
        }

        _sortedPrefixes = prefixInfos.OrderByDescending(info => info.Prefix.Length).ToArray();
        _argumentsByName = new(new MemoryComparer(comparison));

        var builder = ImmutableArray.CreateBuilder<CommandLineArgument>();
        _positionalArgumentCount = DetermineMemberArguments(builder);
        DetermineAutomaticArguments(builder);
        // Sort the member arguments in usage order (positional first, then required non-positional
        // arguments, then the rest by name.
        builder.Sort(new CommandLineArgumentComparer(comparison));
        _arguments = builder.DrainToImmutable();
        VerifyPositionalArgumentRules();
    }

    /// <summary>
    /// Gets the command line argument parsing rules used by the parser.
    /// </summary>
    /// <value>
    /// The <see cref="Ookii.CommandLine.ParsingMode" qualifyHint="true"/> for this parser. The default is
    /// <see cref="ParsingMode.Default" qualifyHint="true"/>.
    /// </value>
    /// <seealso cref="ParseOptionsAttribute.Mode" qualifyHint="true"/>
    /// <seealso cref="ParseOptions.Mode" qualifyHint="true"/>
    public ParsingMode Mode => _mode;

    /// <summary>
    /// Gets the argument name prefixes used by this instance.
    /// </summary>
    /// <value>
    /// A list of argument name prefixes.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The argument name prefixes are used to distinguish argument names from positional argument
    ///   values in a command line.
    /// </para>
    /// <para>
    ///   If the <see cref="Mode"/> property is <see cref="ParsingMode.LongShort" qualifyHint="true"/>, these are the
    ///   prefixes for short argument names. Use the <see cref="LongArgumentNamePrefix"/> property
    ///   to get the prefix for long argument names.
    /// </para>
    /// </remarks>
    /// <seealso cref="ParseOptionsAttribute.ArgumentNamePrefixes" qualifyHint="true"/>
    /// <seealso cref="ParseOptions.ArgumentNamePrefixes" qualifyHint="true"/>
    public ImmutableArray<string> ArgumentNamePrefixes => _argumentNamePrefixes;

    /// <summary>
    /// Gets the prefix to use for long argument names.
    /// </summary>
    /// <value>
    /// The prefix for long argument names, or <see langword="null"/> if the <see cref="Mode"/>
    /// property is not <see cref="ParsingMode.LongShort" qualifyHint="true"/> and the
    /// <see cref="ParseOptions.PrefixTermination" qualifyHint="true"/> property is
    /// <see cref="PrefixTerminationMode.None" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The long argument prefix is only used if the <see cref="Mode"/> property is
    ///   <see cref="ParsingMode.LongShort" qualifyHint="true"/>, or if the
    ///   <see cref="ParseOptions.PrefixTermination" qualifyHint="true"/> property is not
    ///   <see cref="PrefixTerminationMode.None" qualifyHint="true"/>. See <see cref="ArgumentNamePrefixes"/> to
    ///   get the prefixes for short argument names, or for all argument names if the
    ///   <see cref="Mode"/> property is <see cref="ParsingMode.Default" qualifyHint="true"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="ParseOptionsAttribute.LongArgumentNamePrefix" qualifyHint="true"/>
    /// <seealso cref="ParseOptions.LongArgumentNamePrefix" qualifyHint="true"/>
    public string? LongArgumentNamePrefix => _longArgumentNamePrefix;

    /// <summary>
    /// Gets the type that was used to define the arguments.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> that was used to define the arguments.
    /// </value>
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)]
#endif
    public Type ArgumentsType => _provider.ArgumentsType;

    /// <summary>
    /// Gets the friendly name of the application for use in the version information.
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
    /// The description of the command line application.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If not empty, this description will be added at the top of the usage help created by the
    ///   <see cref="WriteUsage"/> method. This description can be set by applying the
    ///   <see cref="DescriptionAttribute"/> attribute to the command line arguments class.
    /// </para>
    /// </remarks>
    public string Description => _provider.Description;

    /// <summary>
    /// Gets footer text that is used when generating usage information.
    /// </summary>
    /// <value>
    /// The footer text.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If not empty, this footer will be added at the bottom of the usage help created by the
    ///   <see cref="WriteUsage"/> method. This footer can be set by applying the
    ///   <see cref="UsageFooterAttribute"/> attribute to the command line arguments class.
    /// </para>
    /// </remarks>
    public string UsageFooter => _provider.UsageFooter;

    /// <summary>
    /// Gets the options used by this instance.
    /// </summary>
    /// <value>
    /// An instance of the <see cref="ParseOptions"/> class.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If you change the value of the <see cref="ParseOptions.Culture" qualifyHint="true"/>, <see cref="ParseOptions.DuplicateArguments" qualifyHint="true"/>,
    ///   <see cref="ParseOptions.AllowWhiteSpaceValueSeparator" qualifyHint="true"/>, <see cref="StringProvider"/> or
    ///   <see cref="UsageWriter"/> property, this will affect the behavior of this instance. The
    ///   other properties of the <see cref="ParseOptions"/> class are only used when the
    ///   <see cref="CommandLineParser"/> class is constructed, so changing them afterwards will
    ///   have no effect.
    /// </para>
    /// </remarks>
    public ParseOptions Options => _parseOptions;

    /// <summary>
    /// Gets the culture used to convert command line argument values from their string representation to the argument type.
    /// </summary>
    /// <value>
    /// The culture used to convert command line argument values from their string representation to the argument type. The default value
    /// is <see cref="CultureInfo.InvariantCulture" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Use the <see cref="ParseOptions"/> class to change this value.
    /// </para>
    /// </remarks>
    /// <seealso cref="ParseOptions.Culture" qualifyHint="true"/>
    public CultureInfo Culture => _parseOptions.Culture;

    /// <summary>
    /// Gets a value indicating whether duplicate arguments are allowed.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if it is allowed to supply non-multi-value arguments more than once; otherwise, <see langword="false"/>.
    ///   The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="AllowDuplicateArguments"/> property is <see langword="false"/>, a <see cref="CommandLineArgumentException"/> is thrown by the <see cref="Parse(string[])"/>
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
    ///   Use the <see cref="ParseOptions"/> or <see cref="ParseOptionsAttribute"/> class to
    ///   change this value.
    /// </para>
    /// </remarks>
    /// <see cref="ParseOptionsAttribute.DuplicateArguments" qualifyHint="true"/>
    /// <see cref="ParseOptions.DuplicateArguments" qualifyHint="true"/>
    public bool AllowDuplicateArguments => _parseOptions.DuplicateArgumentsOrDefault != ErrorMode.Error;

    /// <summary>
    /// Gets a value indicating whether the name and the value of an argument may be in separate
    /// argument tokens.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if names and values can be in separate tokens; <see langword="false"/>
    ///   if the characters specified in the <see cref="NameValueSeparators"/> property must be
    ///   used. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="AllowWhiteSpaceValueSeparator"/> property is <see langword="true"/>, the
    ///   value of an argument can be separated from its name either by using the characters
    ///   specified in the <see cref="NameValueSeparators"/> property, or by using white space (i.e.
    ///   by having a second argument that has the value). Given a named argument named "Sample",
    ///   the command lines <c>-Sample:value</c> and <c>-Sample value</c> are both valid and will
    ///   assign the value "value" to the argument. In the latter case, the values "-Sample" and
    ///   "value" will be two separate entry in the <see cref="string"/> array with the unparsed
    ///   arguments.
    /// </para>
    /// <para>
    ///   If the <see cref="AllowWhiteSpaceValueSeparator"/> property is <see langword="false"/>,
    ///   only the characters specified in the <see cref="NameValueSeparators"/> property are
    ///   allowed to separate the value from the name. The command line <c>-Sample:value</c> still
    ///   assigns the value "value" to the argument, but for the command line `-Sample value` the
    ///   argument is considered not to have a value (which is only valid if
    ///   <see cref="CommandLineArgument.IsSwitch" qualifyHint="true"/> is <see langword="true"/>), and "value" is
    ///   considered to be the value for the next positional argument.
    /// </para>
    /// <para>
    ///   For switch arguments (the <see cref="CommandLineArgument.IsSwitch" qualifyHint="true"/> property is <see langword="true"/>),
    ///   only the characters specified in the <see cref="NameValueSeparators"/> property are allowed
    ///   to specify an explicit value regardless of the value of the <see cref="AllowWhiteSpaceValueSeparator"/>
    ///   property. Given a switch argument named "Switch"  the command line <c>-Switch false</c>
    ///   is interpreted to mean that the value of "Switch" is <see langword="true"/> and the value of the
    ///   next positional argument is "false", even if the <see cref="AllowWhiteSpaceValueSeparator"/>
    ///   property is <see langword="true"/>.
    /// </para>
    /// <para>
    ///   Use the <see cref="ParseOptions"/> or <see cref="ParseOptionsAttribute"/> class to
    ///   change this value.
    /// </para>
    /// </remarks>
    /// <seealso cref="ParseOptionsAttribute.AllowWhiteSpaceValueSeparator" qualifyHint="true"/>
    /// <seealso cref="ParseOptions.AllowWhiteSpaceValueSeparator" qualifyHint="true"/>
    public bool AllowWhiteSpaceValueSeparator => _parseOptions.AllowWhiteSpaceValueSeparatorOrDefault;

    /// <summary>
    /// Gets the characters used to separate the name and the value of an argument.
    /// </summary>
    /// <value>
    ///   The characters used to separate the name and the value of an argument.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Use the <see cref="ParseOptions"/> or <see cref="ParseOptionsAttribute"/> class to
    ///   change this value.
    /// </para>
    /// </remarks>
    /// <seealso cref="AllowWhiteSpaceValueSeparator"/>
    /// <seealso cref="ParseOptionsAttribute.NameValueSeparators" qualifyHint="true"/>
    /// <seealso cref="ParseOptions.NameValueSeparators" qualifyHint="true"/>
    public ImmutableArray<char> NameValueSeparators => _nameValueSeparators;

    /// <summary>
    /// Gets or sets a value that indicates whether usage help should be displayed if the <see cref="Parse(string[])"/>
    /// method returned <see langword="null"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if usage help should be displayed; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Check this property after calling the <see cref="Parse(string[])"/> method or one
    ///   of its overloads to see if usage help should be displayed.
    /// </para>
    /// <para>
    ///   This property will always be <see langword="false"/> if the <see cref="Parse(string[])"/>
    ///   method returned a non-<see langword="null"/> value.
    /// </para>
    /// <para>
    ///   This property will always be <see langword="true"/> if the <see cref="Parse(string[])"/>
    ///   method threw a <see cref="CommandLineArgumentException"/>, or if an argument used
    ///   <see cref="CancelMode.Abort" qualifyHint="true"/> with the <see cref="CommandLineArgumentAttribute.CancelParsing" qualifyHint="true"/>
    ///   property or the <see cref="ArgumentParsed"/> event.
    /// </para>
    /// <para>
    ///   If an argument that is defined by a method (<see cref="ArgumentKind.Method" qualifyHint="true"/>) cancels
    ///   parsing by returning <see cref="CancelMode.Abort" qualifyHint="true"/> or <see langword="false"/> from the
    ///   method, this property is <em>not</em> automatically set to <see langword="true"/>.
    ///   Instead, the method should explicitly set the <see cref="HelpRequested"/> property if it
    ///   wants usage help to be displayed.
    /// </para>
    /// <code>
    /// [CommandLineArgument]
    /// public static CancelMode MethodArgument(CommandLineParser parser)
    /// {
    ///     parser.HelpRequested = true;
    ///     return CancelMode.Abort;
    /// }
    /// </code>
    /// </remarks>
    public bool HelpRequested { get; set; }

    /// <summary>
    /// Gets the <see cref="LocalizedStringProvider"/> implementation used to get strings for
    /// error messages and usage help.
    /// </summary>
    /// <value>
    /// An instance of a class inheriting from the <see cref="LocalizedStringProvider"/> class.
    /// </value>
    /// <seealso cref="ParseOptions.StringProvider" qualifyHint="true"/>
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
    /// Gets the string comparison used for argument names.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="StringComparison"/> enumeration.
    /// </value>
    /// <seealso cref="ParseOptionsAttribute.CaseSensitive" qualifyHint="true"/>
    /// <seealso cref="ParseOptions.ArgumentNameComparison" qualifyHint="true"/>
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
    /// <para>
    ///   To find an argument by name or alias, use the <see cref="GetArgument"/> or
    ///   <see cref="GetShortArgument"/> method.
    /// </para>
    /// </remarks>
    public ImmutableArray<CommandLineArgument> Arguments => _arguments;

    /// <summary>
    /// Gets the automatic help argument, or an argument with the same name, if there is one.
    /// </summary>
    /// <value>
    /// A <see cref="CommandLineArgument"/> instance, or <see langword="null"/> if the automatic
    /// help argument was disabled using the <see cref="ParseOptions"/> class or the
    /// <see cref="ParseOptionsAttribute"/> attribute.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the automatic help argument is enabled, this will return either the created help
    ///   argument, or the argument that conflicted with its name or one of its aliases, which is
    ///   assumed to be the argument used to display help in that case.
    /// </para>
    /// <para>
    ///   This is used the <see cref="UsageWriter.WriteMoreInfoMessage" qualifyHint="true"/> method to determine
    ///   whether to show the message and the actual name of the argument to use.
    /// </para>
    /// </remarks>
    public CommandLineArgument? HelpArgument { get; private set; }

    /// <summary>
    /// Gets the result of the last command line argument parsing operation.
    /// </summary>
    /// <value>
    /// An instance of the <see cref="CommandLine.ParseResult" qualifyHint="true"/> class.
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
    /// One of the values of the <see cref="Support.ProviderKind" qualifyHint="true"/> enumeration.
    /// </value>
    public ProviderKind ProviderKind => _provider.Kind;

    internal IComparer<char>? ShortArgumentNameComparer => _argumentsByShortName?.Comparer;

    internal Enum? DefaultArgumentCategory => _provider.OptionsAttribute?.DefaultArgumentCategoryValue;

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
    ///   To determine the executable name, this method first checks the <see cref="Environment.ProcessPath" qualifyHint="true"/>
    ///   property (if using .Net 6.0 or later). If using the .Net Standard package, or if
    ///   <see cref="Environment.ProcessPath" qualifyHint="true"/> returns "dotnet", it checks the first item in
    ///   the array returned by <see cref="Environment.GetCommandLineArgs" qualifyHint="true"/>, and finally falls
    ///   back to the file name of the entry point assembly.
    /// </para>
    /// <para>
    ///   The return value of this function is used as the default executable name to show in
    ///   the usage syntax when generating usage help, unless overridden by the <see cref="UsageWriter.ExecutableName" qualifyHint="true"/>
    ///   property.
    /// </para>
    /// </remarks>
    /// <seealso cref="UsageWriter.IncludeExecutableExtension" qualifyHint="true"/>
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

        // The array returned by GetCommandLineArgs should always contain at least one element, but
        // just in case.
        path ??= Environment.GetCommandLineArgs().FirstOrDefault() ?? string.Empty;
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
    /// Writes command line usage help using the specified <see cref="UsageWriter"/> instance.
    /// </summary>
    /// <param name="usageWriter">
    ///   The <see cref="UsageWriter"/> to use to create the usage. If <see langword="null"/>,
    ///   the value from the <see cref="ParseOptions.UsageWriter" qualifyHint="true"/> property in the
    ///   <see cref="Options"/> property is sued.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     The usage help consists of first the <see cref="Description"/>, followed by the usage
    ///     syntax, followed by a description of all the arguments.
    ///   </para>
    ///   <para>
    ///     You can add descriptions to the usage text by applying the <see cref="DescriptionAttribute"/>
    ///     attribute to your command line arguments type, and the properties and methods defining
    ///     command line arguments.
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
    ///   the value from the <see cref="ParseOptions.UsageWriter" qualifyHint="true"/> property in the
    ///   <see cref="Options"/> property is used.
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
    /// Parses the arguments returned by the <see cref="Environment.GetCommandLineArgs" qualifyHint="true"/>
    /// method.
    /// </summary>
    /// <returns>
    ///   An instance of the type specified by the <see cref="ArgumentsType"/> property, or
    ///   <see langword="null"/> if argument parsing was canceled by the <see cref="ArgumentParsed"/>
    ///   event handler, the <see cref="CommandLineArgumentAttribute.CancelParsing" qualifyHint="true"/> property,
    ///   or a method argument that returned <see cref="CancelMode.Abort" qualifyHint="true"/> or
    ///   <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   If the return value is <see langword="null"/>, check the <see cref="HelpRequested"/>
    ///   property to see if usage help should be displayed.
    /// </para>
    /// </remarks>
    /// <exception cref="CommandLineArgumentException">
    ///   An error occurred parsing the command line. Check the <see cref="CommandLineArgumentException.Category" qualifyHint="true"/>
    ///   property for the exact reason for the error.
    /// </exception>
    public object? Parse()
    {
        // GetCommandLineArgs include the executable, so skip it.
        return Parse(Environment.GetCommandLineArgs().AsMemory(1));
    }

    /// <inheritdoc cref="Parse()" />
    /// <summary>
    /// Parses the specified command line arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="args"/> is <see langword="null"/>.
    /// </exception>
    public object? Parse(string[] args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        return Parse(args.AsMemory());
    }

    /// <inheritdoc cref="Parse()" />
    /// <summary>
    /// Parses the specified command line arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public object? Parse(ReadOnlyMemory<string> args)
    {
        var state = new ParseState() 
        { 
            Parser = this,
            Arguments = args,
        };

        try
        {
            HelpRequested = false;
            return ParseCore(ref state);
        }
        catch (CommandLineArgumentException ex)
        {
            HelpRequested = true;
            ParseResult = ParseResult.FromException(ex, args.Slice(state.Index));
            throw;
        }
    }

    /// <summary>
    /// Parses the arguments returned by the <see cref="Environment.GetCommandLineArgs" qualifyHint="true"/>
    /// method, and displays error messages and usage help if required.
    /// </summary>
    /// <returns>
    ///   An instance of the type specified by the <see cref="ArgumentsType"/> property, or
    ///   <see langword="null"/> if an error occurred, or argument parsing was canceled by the
    ///   <see cref="CommandLineArgumentAttribute.CancelParsing" qualifyHint="true"/> property or a method argument
    ///   that returned <see cref="CancelMode.Abort" qualifyHint="true"/> or <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   If an error occurs or parsing is canceled, it prints errors to the <see cref="ParseOptions.Error" qualifyHint="true"/>
    ///   stream, and usage help using the <see cref="UsageWriter"/> if the <see cref="HelpRequested"/>
    ///   property is <see langword="true"/>. It then returns <see langword="null"/>.
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
        // GetCommandLineArgs includes the executable, so skip it.
        return ParseWithErrorHandling(Environment.GetCommandLineArgs().AsMemory(1));
    }

    /// <inheritdoc cref="ParseWithErrorHandling()" />
    /// <summary>
    /// Parses the specified command line arguments and displays error messages and usage help if
    /// required.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="args"/> is <see langword="null"/>.
    /// </exception>
    public object? ParseWithErrorHandling(string[] args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        return ParseWithErrorHandling(args.AsMemory());
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
        catch (AmbiguousPrefixAliasException ex) when (_parseOptions.ShowUsageOnError != UsageHelpRequest.Full)
        {
            WriteError(_parseOptions, StringProvider.AmbiguousArgumentPrefixAliasErrorOnly(ex.ArgumentName!),
                _parseOptions.ErrorColor, true);

            HelpRequested = false;
            _parseOptions.UsageWriter.WriteParserAmbiguousPrefixAliasUsage(this, ex.PossibleMatches);
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
    /// Parses the arguments returned by the <see cref="Environment.GetCommandLineArgs" qualifyHint="true"/>
    /// method using the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type defining the command line arguments.</typeparam>
    /// <param name="options">
    ///   The options that control parsing behavior and usage help formatting. If
    ///   <see langword="null" />, the default options are used.
    /// </param>
    /// <returns>
    ///   An instance of the type <typeparamref name="T"/>, or <see langword="null"/> if an
    ///   error occurred, or argument parsing was canceled by the <see cref="CommandLineArgumentAttribute.CancelParsing" qualifyHint="true"/>
    ///   property or a method argument that returned <see cref="CancelMode.Abort" qualifyHint="true"/>
    ///   or <see langword="false"/>.
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
    ///   This is a convenience function that instantiates a <see cref="CommandLineParser{T}"/>,
    ///   calls the <see cref="CommandLineParser{T}.ParseWithErrorHandling()"/> method, and returns
    ///   the result. If an error occurs or parsing is canceled, it prints errors to the
    ///   <see cref="ParseOptions.Error" qualifyHint="true"/> stream, and usage help to the
    ///   <see cref="UsageWriter"/> if the <see cref="HelpRequested"/> property is <see langword="true"/>.
    ///   It then returns <see langword="null"/>.
    /// </para>
    /// <para>
    ///   If the <see cref="ParseOptions.Error" qualifyHint="true"/> parameter is <see langword="null"/>, output is
    ///   written to a <see cref="LineWrappingTextWriter"/> for the standard error stream,
    ///   wrapping at the console's window width. If the stream is redirected, output may still
    ///   be wrapped, depending on the value returned by <see cref="Console.WindowWidth" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   Color is applied to the output depending on the value of the <see cref="UsageWriter.UseColor" qualifyHint="true"/>
    ///   property, the <see cref="ParseOptions.UseErrorColor" qualifyHint="true"/> property, and the capabilities
    ///   of the console.
    /// </para>
    /// <para>
    ///   If you want more control over the parsing process, including custom error/usage output
    ///   or handling the <see cref="ArgumentParsed"/> event, you should use the
    ///   instance <see cref="CommandLineParser{T}.Parse()" qualifyHint="true"/> or
    ///   <see cref="CommandLineParser{T}.ParseWithErrorHandling()" qualifyHint="true"/> method.
    /// </para>
    /// <para>
    ///   This method uses reflection to determine the arguments defined by the type <typeparamref name="T"/>
    ///   at runtime, unless the type has the <see cref="GeneratedParserAttribute"/> applied. For a
    ///   type using that attribute, you can also use the generated static
    ///   <see cref="IParserProvider{TSelf}.CreateParser" qualifyHint="true"/> or
    ///   <see cref="IParser{TSelf}.Parse(ParseOptions?)" qualifyHint="true"/> methods on the
    ///   arguments class instead.
    /// </para>
    /// </remarks>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Argument information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute.", Url = UnreferencedCodeHelpUrl)]
#endif
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Consider using the GeneratedParserAttribute.")]
#endif
    public static T? Parse<T>(ParseOptions? options = null)
        where T : class
    {
        var parser = new CommandLineParser<T>(options);
        return parser.ParseWithErrorHandling();
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
    /// <exception cref="NotSupportedException">
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </exception>
    /// <remarks>
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </remarks>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Argument information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute.", Url = UnreferencedCodeHelpUrl)]
#endif
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Consider using the GeneratedParserAttribute.")]
#endif
    public static T? Parse<T>(ReadOnlyMemory<string> args, ParseOptions? options = null)
        where T : class
    {
        var parser = new CommandLineParser<T>(options);
        return parser.ParseWithErrorHandling(args);
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
    /// <remarks>
    ///   <inheritdoc cref="Parse{T}(ParseOptions?)"/>
    /// </remarks>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Argument information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute.", Url = UnreferencedCodeHelpUrl)]
#endif
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Consider using the GeneratedParserAttribute.")]
#endif
    public static T? Parse<T>(string[] args, ParseOptions? options = null)
        where T : class
    {
        var parser = new CommandLineParser<T>(options);
        return parser.ParseWithErrorHandling(args);
    }

    /// <summary>
    /// Gets a command line argument by name or alias.
    /// </summary>
    /// <param name="name">The name or alias of the argument.</param>
    /// <returns>The <see cref="CommandLineArgument"/> instance containing information about
    /// the argument, or <see langword="null" /> if the argument was not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    ///   If the <see cref="Mode"/> property is <see cref="ParsingMode.LongShort" qualifyHint="true"/>, this uses
    ///   the long name and long aliases of the argument.
    /// </para>
    /// <para>
    ///   This method only uses the actual names and aliases; it does not consider auto prefix
    ///   aliases regardless of the value of the <see cref="ParseOptions.AutoPrefixAliases" qualifyHint="true"/>
    ///   property.
    /// </para>
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
    /// Gets a command line argument by short name or alias.
    /// </summary>
    /// <param name="shortName">The short name of the argument.</param>
    /// <returns>The <see cref="CommandLineArgument"/> instance containing information about
    /// the argument, or <see langword="null" /> if the argument was not found.</returns>
    /// <remarks>
    /// <para>
    ///   If <see cref="Mode"/> is not <see cref="ParsingMode.LongShort" qualifyHint="true"/>, this
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
    ///   If the <see cref="Mode"/> property is <see cref="ParsingMode.LongShort" qualifyHint="true"/>, these
    ///   prefixes will be used for short argument names. The <see cref="DefaultLongArgumentNamePrefix"/>
    ///   constant is the default prefix for long argument names regardless of platform.
    /// </para>
    /// </remarks>
    /// <seealso cref="ArgumentNamePrefixes"/>
    /// <seealso cref="ParseOptionsAttribute.ArgumentNamePrefixes" qualifyHint="true"/>
    /// <seealso cref="ParseOptions.ArgumentNamePrefixes" qualifyHint="true"/>
    public static ImmutableArray<string> GetDefaultArgumentNamePrefixes()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ImmutableArray.Create("-", "/")
            : ImmutableArray.Create("-");
    }

    /// <summary>
    /// Gets the default characters used to separate the name and the value of an argument.
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
    protected virtual void OnArgumentParsed(ArgumentParsedEventArgs e) => ArgumentParsed?.Invoke(this, e);

    /// <summary>
    /// Raises the <see cref="DuplicateArgument"/> event.
    /// </summary>
    /// <param name="e">The data for the event.</param>
    protected virtual void OnDuplicateArgument(DuplicateArgumentEventArgs e) => DuplicateArgument?.Invoke(this, e);

    /// <summary>
    /// Raises the <see cref="UnknownArgument"/> event.
    /// </summary>
    /// <param name="e">The data for the event.</param>
    protected virtual void OnUnknownArgument(UnknownArgumentEventArgs e) => UnknownArgument?.Invoke(this, e);

    internal static bool ShouldIndent(LineWrappingTextWriter writer) => writer.MaximumLineLength is 0 or >= 30;

    internal static void WriteError(ParseOptions options, string message, TextFormat color, bool blankLine = false)
    {
        using var errorVtSupport = options.EnableErrorColor();
        using var error = DisposableWrapper.Create(options.Error, LineWrappingTextWriter.ForConsoleError);
        if (errorVtSupport.IsSupported)
        {
            error.Inner.Write(color);
        }

        error.Inner.Write(message);
        if (errorVtSupport.IsSupported)
        {
            error.Inner.Write(options.UsageWriter.ColorReset);
        }

        error.Inner.WriteLine();
        if (blankLine)
        {
            error.Inner.WriteLine();
        }
    }

    internal string GetCategoryDescription(Enum category) => _provider.GetCategoryDescription(category);

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
        int positionalArgumentCount = 0;
        Type? categoryType = DefaultArgumentCategory?.GetType();
        foreach (var argument in _provider.GetArguments(this))
        {
            AddNamedArgument(argument, builder);
            if (argument.Position != null)
            {
                ++positionalArgumentCount;
            }

            // Make sure all arguments use the same category. This is checked here to avoid
            // unexpected exceptions when generating usage help.
            if (argument.Category is Enum category)
            {
                if (categoryType == null)
                {
                    categoryType = category.GetType();
                }
                else if (categoryType != category.GetType())
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.MismatchedCategoryTypesFormat, argument.ArgumentName, category.GetType(), categoryType));
                }
            }
        }

        return positionalArgumentCount;
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
        bool hasMultiValueArgument = false;

        for (int x = 0; x < _positionalArgumentCount; ++x)
        {
            CommandLineArgument argument = _arguments[x];

            if (hasMultiValueArgument)
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

            if (argument.MultiValueInfo != null)
            {
                hasMultiValueArgument = true;
            }

            argument.Position = x;
        }
    }

    private object? ParseCore(ref ParseState state)
    {
        Reset();
        for (; state.Index < state.Arguments.Length; ++state.Index)
        {
            var token = state.Arguments.Span[state.Index];
            state.ResetForNextArgument();
            if (!state.PositionalOnly && token == _longArgumentNamePrefix)
            {
                if (_parseOptions.PrefixTerminationOrDefault == PrefixTerminationMode.PositionalOnly)
                {
                    state.PositionalOnly = true;
                    continue;
                }
                else if (_parseOptions.PrefixTerminationOrDefault == PrefixTerminationMode.CancelWithSuccess)
                {
                    state.CancelParsing = CancelMode.Success;
                    state.ArgumentName = default;
                    break;
                }
            }

            if (state.PositionalOnly || !FindNamedArgument(token, ref state))
            {
                state.IsSpecifiedByPosition = true;
                state.ArgumentValue = token.AsMemory();
                FindPositionalArgument(ref state);
            }

            if (state.IsUnknown)
            {
                HandleUnknownArgument(ref state);
            }

            // Argument can be null without IsUnknown set if the token was a combined short switch
            // argument.
            if (state.Argument != null)
            {
                ParseArgumentValue(ref state);
            }

            if (state.CancelParsing != CancelMode.None)
            {
                break;
            }
        }

        if (state.CancelParsing == CancelMode.Abort)
        {
            ParseResult = ParseResult.FromCanceled(state.RealArgumentName, state.RemainingArguments);
            return null;
        }

        // Check required arguments and post-parsing validation. This is done in usage order.
        foreach (CommandLineArgument argument in _arguments)
        {
            argument.ValidateAfterParsing();
        }

        // Run class validators.
        _provider.RunValidators(this);

        var result = CreateResultInstance();

        ParseResult = state.CancelParsing == CancelMode.None
            ? ParseResult.FromSuccess()
            : ParseResult.FromSuccess(state.Argument?.ArgumentName ?? 
                (state.ArgumentName.Length == 0 ? LongArgumentNamePrefix : state.ArgumentName.ToString()),
                state.RemainingArguments);

        // Reset to false in case it was set by a method argument that didn't cancel parsing.
        HelpRequested = false;
        return result;
    }

    private object CreateResultInstance()
    {
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
            // Apply property argument values (this does nothing for method arguments).
            argument.ApplyPropertyValue(commandLineArguments);
        }

        return commandLineArguments;
    }

    private void Reset()
    {
        HelpRequested = false;

        // Reset all arguments to their default value, and mark them as unassigned.
        foreach (var argument in _arguments)
        {
            argument.Reset();
        }
    }

    private void ParseArgumentValue(ref ParseState state)
    {
        Debug.Assert(state.Argument != null);

        var argument = state.Argument!;
        bool parsedValue = false;
        if (state.ArgumentValue == null && !argument.IsSwitch && AllowWhiteSpaceValueSeparator)
        {
            // No value separator was present in the token, but a value is required and white space is
            // allowed. We take the next token as the value. For multi-value arguments that can consume
            // multiple tokens, we keep going until we hit another argument name.
            var allowMultiToken = argument.MultiValueInfo is MultiValueArgumentInfo info
                && (info.AllowWhiteSpaceSeparator || state.IsSpecifiedByPosition);

            int index;
            for (index = state.Index + 1; index < state.Arguments.Length; ++index)
            {
                var stringValue = state.Arguments.Span[index];
                if (CheckArgumentNamePrefix(stringValue) != null)
                {
                    --index;
                    break;
                }

                parsedValue = true;
                state.CancelParsing = ParseArgumentValue(argument, stringValue, stringValue.AsMemory());
                if (state.CancelParsing != CancelMode.None || !allowMultiToken)
                {
                    break;
                }
            }

            state.Index = index;

            // The caller will increment again, so if we reached the end, decrement to avoid the
            // index going out of range for determining remaining arguments if there's an exception.
            if (state.Index == state.Arguments.Length)
            {
                state.Index = state.Arguments.Length - 1;
            }
        }

        // If the value was not parsed above, parse it now. In case there is no value and it's
        // not a switch, CommandLineArgument.SetValue will throw an exception.
        if (!parsedValue)
        {
            state.CancelParsing = ParseArgumentValue(argument, null, state.ArgumentValue);
        }
    }

    private CancelMode ParseArgumentValue(CommandLineArgument argument, string? stringValue, ReadOnlyMemory<char>? memoryValue)
    {
        if (argument.HasValue && argument.MultiValueInfo == null)
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

    private static void FindPositionalArgument(ref ParseState state)
    {
        // Skip named positional arguments that have already been specified by name, unless it's
        // a multi-value argument which must be the last positional argument.
        while (state.PositionalArgument is CommandLineArgument current && current.MultiValueInfo == null && current.HasValue)
        {
            ++state.PositionalArgumentIndex;
        }

        state.Argument = state.PositionalArgument;
        if (state.Argument == null)
        {
            state.IsUnknown = true;
            return;
        }

        state.ArgumentName = state.Argument.ArgumentName.AsMemory();
    }

    private bool FindNamedArgument(string token, ref ParseState state)
    {
        if (CheckArgumentNamePrefix(token) is not PrefixInfo prefix)
        {
            return false;
        }

        state.ArgumentName = token.AsMemory(prefix.Prefix.Length);
        if (state.ArgumentName.SplitOnceAny(_nameValueSeparators.AsSpan()) is (ReadOnlyMemory<char>, ReadOnlyMemory<char>) split)
        {
            (state.ArgumentName, state.ArgumentValue) = split;
        }

        if (_argumentsByShortName != null && prefix.Short)
        {
            if (state.ArgumentName.Length == 1)
            {
                state.Argument = GetShortArgument(state.ArgumentName.Span[0]);
                state.IsUnknown = state.Argument == null;
                return true;
            }
            else
            {
                ParseCombinedShortArgument(ref state);
                return true;
            }
        }

        if (state.Argument == null && !_argumentsByName.TryGetValue(state.ArgumentName, out state.Argument))
        {
            if (Options.AutoPrefixAliasesOrDefault)
            {
                GetArgumentByNamePrefix(ref state);
            }

            if (state.Argument == null)
            {
                state.IsUnknown = true;
                return true;
            }
        }

        state.Argument.SetUsedArgumentName(state.ArgumentName);
        state.ArgumentName = state.Argument.ArgumentName.AsMemory();
        return true;
    }

    private void GetArgumentByNamePrefix(ref ParseState state)
    {
        var prefix = state.ArgumentName.Span;
        string? previousMatchedName = null;
        foreach (var argument in _arguments)
        {
            // Skip arguments without a long name.
            if (Mode == ParsingMode.LongShort && !argument.HasLongName)
            {
                continue;
            }

            string? matchedName = null;
            if (argument.ArgumentName.AsSpan().StartsWith(prefix, ArgumentNameComparison))
            {
                matchedName = argument.ArgumentName;
            }
            else
            {
                foreach (var alias in argument.Aliases)
                {
                    if (alias.AsSpan().StartsWith(prefix, ArgumentNameComparison))
                    {
                        matchedName = alias;
                        break;
                    }
                }
            }

            if (matchedName != null)
            {
                if (previousMatchedName != null)
                {
                    // Prefix is not unique, and this is the first ambiguous match we found.
                    state.PossibleMatches ??= ImmutableArray.CreateBuilder<string>();
                    state.PossibleMatches.Add(previousMatchedName);
                    state.Argument = null;
                    previousMatchedName = null;
                }
                
                if (state.PossibleMatches?.Count > 0)
                {
                    state.PossibleMatches.Add(matchedName);
                }
                else
                {
                    state.Argument = argument;
                    previousMatchedName = matchedName;
                }
            }
        }
    }

    private void ParseCombinedShortArgument(ref ParseState state)
    {
        var combinedName = state.ArgumentName.Span;
        foreach (var ch in combinedName)
        {
            var argument = GetShortArgument(ch);
            if (argument == null)
            {
                state.ArgumentName = ch.ToString().AsMemory();
                HandleUnknownArgument(ref state, true);
                continue;
            }

            if (!argument.IsSwitch)
            {
                throw StringProvider.CreateException(CommandLineArgumentErrorCategory.CombinedShortNameNonSwitch,
                    combinedName.ToString());
            }

            state.ArgumentName = argument.ArgumentName.AsMemory();
            state.CancelParsing = ParseArgumentValue(argument, null, state.ArgumentValue);
            if (state.CancelParsing != CancelMode.None)
            {
                break;
            }
        }
    }

    private void HandleUnknownArgument(ref ParseState state, bool isCombined = false)
    {
        ImmutableArray<string> possibleMatches;
        if (state.PossibleMatches != null)
        {
            state.PossibleMatches.Sort(ArgumentNameComparison.GetComparer());
            possibleMatches = state.PossibleMatches.DrainToImmutable();
        }
        else
        {
            possibleMatches = [];
        }

        var eventArgs = new UnknownArgumentEventArgs(state.Arguments.Span[state.Index], state.ArgumentName,
            state.ArgumentValue ?? default, isCombined, possibleMatches);

        OnUnknownArgument(eventArgs);
        if (eventArgs.CancelParsing != CancelMode.None)
        {
            state.CancelParsing = eventArgs.CancelParsing;
            return;
        }

        if (!eventArgs.Ignore)
        {
            if (!possibleMatches.IsEmpty)
            {
                var name = state.ArgumentName.ToString();
                var prefix = LongArgumentNamePrefix ?? ArgumentNamePrefixes[0];
                var message = StringProvider.AmbiguousArgumentPrefixAlias(name, prefix, possibleMatches);
                throw new AmbiguousPrefixAliasException(message, name, possibleMatches);
            }

            if (state.ArgumentName.Length > 0)
            {
                throw StringProvider.CreateException(CommandLineArgumentErrorCategory.UnknownArgument,
                    state.ArgumentName.ToString());
            }


            throw StringProvider.CreateException(CommandLineArgumentErrorCategory.TooManyArguments);
        }
    }

    private PrefixInfo? CheckArgumentNamePrefix(string argument)
    {
        // Even if '-' is an argument name prefix, we consider an argument starting with dash
        // followed by a digit as a value, because it could be a negative number.
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Consider using the GeneratedParserAttribute.")]
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
