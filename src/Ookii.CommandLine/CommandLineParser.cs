// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Parses command line arguments into a class of the specified type.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The <see cref="CommandLineParser"/> class can parse a set of command line arguments into values. Which arguments are
    ///   accepted is determined from the constructor parameters and properties of the type passed to the <see cref="CommandLineParser.CommandLineParser(Type, IEnumerable{string}?, IComparer{string}?)"/>
    ///   constructor. The result of a parsing operation is an instance of that type that was constructed using the constructor parameters and
    ///   property values from their respective command line arguments.
    /// </para>
    /// <para>
    ///   The <see cref="CommandLineParser"/> class can parse a command line and can generate usage help for arguments defined by the type
    ///   passed to its constructor. This usage help can be presented to the user to provide information about how to invoke your application
    ///   from the command line.
    /// </para>
    /// <para>
    ///   The <see cref="Parse{T}(string[], ParseOptions)"/> method is a helper that will parse arguments and print error and
    ///   usage information if required. For most use cases, this is all you need in addition to a class that defines arguments.
    /// </para>
    /// <para>
    ///   The command line arguments are parsed using the parsing rules described below. A command line consists of a series of
    ///   argument values; each value is assigned to the appropriate argument based on either the name or the position of the argument.
    /// </para>
    /// <para>
    ///   Every argument has a name, and can have its value specified by name. To specify an argument name on the command line it must
    ///   be preceded by a special prefix. On Windows, the argument name prefix is typically a forward
    ///   slash (/), while on Unix platforms it is usually a single dash (-) or double dash (--). Which prefixes
    ///   are accepted by the <see cref="CommandLineParser"/> class can be specified by using the <see cref="CommandLineParser.CommandLineParser(Type, IEnumerable{string}?, IComparer{string}?)"/>
    ///   constructor. By default, it will accept both "/" and "-" on Windows, and only a "-" on all other platforms (other platforms are
    ///   supported via <a href="http://www.mono-project.com">Mono</a>).
    /// </para>
    /// <note>
    ///   Although almost any argument name is allowed as long as it isn't empty and doesn't contain the character
    ///   specified in the <see cref="NameValueSeparator"/> property, certain argument names may not be advisable.
    ///   Particularly, avoid argument names that start with a number, as they it will not be possible to specify
    ///   them by name if the argument name prefix is a single dash; arguments starting with a single dash followed
    ///   by a digit are always considered values during parsing, even if there is an argument with that name.
    /// </note>
    /// <para>
    ///   The name of the argument must be followed by its value. The value can be either in the next argument (separated from the name
    ///   by white space), or separated by  the character specified in the <see cref="NameValueSeparator"/> property. For example,
    ///   to assign the value "foo" to the argument "sample", you can use either <c>-sample foo</c> or <c>-sample:foo</c>.
    /// </para>
    /// <para>
    ///   If an argument has a type of <see cref="Boolean"/> (and is not a positional argument as described below), it is a switch argument, and doesn't require a value. Its value is determined
    ///   by its presence on the command line; if it is absent the value is <see langword="false"/>; if it is present the value is
    ///   <see langword="true"/>. For example, to set a switch argument named "verbose" to true, you can simply use the command line
    ///   <c>-verbose</c>. You can still explicitly specify the value of a switch argument, for example <c>-verbose:true</c>.
    ///   Note that you cannot use white space to separate a switch argument name and value; you must use the character
    ///   specified in the <see cref="NameValueSeparator"/> property.
    /// </para>
    /// <para>
    ///   If the type of the argument is <see cref="Nullable{T}"/> of <see cref="Boolean"/>, its value will be <see langword="null"/> if it is not supplied, <see langword="true"/> if it is supplied without
    ///   an explicit value (or with an explicit value of <see langword="true"/>), and <see langword="false"/> only if its value was explicitly specified as <see langword="false"/>.
    /// </para>
    /// <para>
    ///   An argument value can also refer to an argument by position. A positional argument is an argument that can be set both by
    ///   name and position. When specified by name, it can appear in any position on the command line, but when specified by
    ///   position, it must appear in the correct position.
    /// </para>
    /// <para>
    ///   For example, if you have two arguments named "foo" and "bar" which have positions 0 and 1 respectively, you could
    ///   specify their values using <c>value1 value2</c>, which assigns "value1" to "foo" and "value2" to "bar".
    ///   However, you could also use <c>-bar value2 -foo value1</c> to achieve the same effect.
    /// </para>
    /// <para>
    ///   If a positional argument was already specified by name, it is no longer considered as a target for positional argument values.
    ///   In the previous example, if the command line <c>-foo value1 value2</c> is used, "value2" is the first positional argument value,
    ///   but is assigned to "bar", the second positional argument, because "foo" had already been assigned a value by name.
    /// </para>
    /// <para>
    ///   Arguments can either be required or optional. If an argument is required, the <see cref="CommandLineParser.Parse(string[], int)"/>
    ///   method will throw a <see cref="CommandLineArgumentException"/> if it is not supplied on the command line. For positional
    ///   arguments, it is not allowed to have a required argument following a positional argument.
    /// </para>
    /// <para>
    ///   If an argument has a type other than <see cref="String"/>, the <see cref="CommandLineParser"/> class will use the
    ///   <see cref="TypeDescriptor"/> class to get a <see cref="TypeConverter"/> for that type to convert the supplied string value
    ///   to the correct type. You can also use the <see cref="TypeConverterAttribute"/> on the property or constructor parameter
    ///   that defines the attribute to specify a custom type converter.
    /// </para>
    /// <para>
    ///   If an argument has an array type, it can be specified more than once, and the value for each time is it specified
    ///   is added to the array. Given an array argument named "sample", the command line <c>-sample 1 -sample 2 -sample 3</c>
    ///   would set the value of "sample" to an array holding the values 1, 2 and 3. A required array argument must have at
    ///   least one value. A positional array argument must be the last positional argument.
    /// </para>
    /// <para>
    ///   To specify which arguments are accepted by the <see cref="CommandLineParser"/> class, you can use either constructor
    ///   parameters or properties on the type holding the argument values.
    /// </para>
    /// <para>
    ///   If the arguments type has only one constructor, its parameters are automatically used. If it has more than one
    ///   constructor, one of the constructors must be marked using the <see cref="CommandLineConstructorAttribute"/> attribute.
    /// </para>
    /// <para>
    ///   Arguments for constructor parameters are always positional arguments, so can be specified by both name and position. The
    ///   position of the command line argument will match the position of the constructor parameter. By default, the name of the
    ///   argument matches the name of the parameter, but this can be overridden using the <see cref="ArgumentNameAttribute"/> attribute.
    ///   The argument is optional if the parameter has the <see cref="System.Runtime.InteropServices.OptionalAttribute"/> attribute applied,
    ///   and its default value can be specified using the <see cref="System.Runtime.InteropServices.DefaultParameterValueAttribute"/> attribute.
    ///   With Visual Basic and C# 4.0, you can use the built-in syntax for optional parameters to create an optional command line argument and
    ///   specify the default value.
    /// </para>
    /// <para>
    ///   A property defines a command line argument if it is <see langword="public"/>, not <see langword="static"/>, and has the
    ///   <see cref="CommandLineArgumentAttribute"/> attribute defined. The argument will only be positional if the <see cref="CommandLineArgumentAttribute.Position"/>
    ///   property is set to a non-negative value, and will be required only if the <see cref="CommandLineArgumentAttribute.IsRequired"/>
    ///   property is set to <see langword="true"/>.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class CommandLineParser
    {
        #region Nested types

        private sealed class CommandLineArgumentComparer : IComparer<CommandLineArgument>
        {
            private readonly IComparer<string> _stringComparer;

            public CommandLineArgumentComparer(IComparer<string> stringComparer)
            {
                _stringComparer = stringComparer;
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
                return _stringComparer.Compare(x.ArgumentName, y.ArgumentName);
            }
        }

        private struct PrefixInfo
        {
            public string Prefix { get; set; }
            public bool Short { get; set; }
        }

        #endregion

        // Don't apply indentation to console output if the line width is less than this.
        internal const int MinimumLineWidthForIndent = 30;

        private readonly Type _argumentsType;
        private readonly List<CommandLineArgument> _arguments = new();
        private readonly SortedDictionary<string, CommandLineArgument> _argumentsByName;
        // Uses string, even though short names are single char, so it can use the same comparer
        // as _argumentsByName.
        private readonly SortedDictionary<string, CommandLineArgument>? _argumentsByShortName;
        private readonly ConstructorInfo _commandLineConstructor;
        private readonly int _constructorArgumentCount;
        private readonly int _positionalArgumentCount;
        private readonly string[] _argumentNamePrefixes;
        private readonly PrefixInfo[] _sortedPrefixes;
        private ReadOnlyCollection<CommandLineArgument>? _argumentsReadOnlyWrapper;
        private ReadOnlyCollection<string>? _argumentNamePrefixesReadOnlyWrapper;
        private readonly ParsingMode _mode;
        private readonly string? _longArgumentNamePrefix;
        private readonly NameTransform _nameTransform;
        private readonly LocalizedStringProvider _stringProvider;

        /// <summary>
        /// Gets the default character used to separate the name and the value of an argument.
        /// </summary>
        /// <value>
        /// The default character used to separate the name and the value of an argument, which is ':'.
        /// </value>
        /// <remarks>
        /// This constant is used as the default value of the <see cref="NameValueSeparator"/> property.
        /// </remarks>
        public const char DefaultNameValueSeparator = ':';

        /// <summary>
        /// Gets the default prefix used for long argument names if <see cref="Mode"/> is
        /// <see cref="ParsingMode.LongShort"/>.
        /// </summary>
        public const string DefaultLongArgumentNamePrefix = "--";

        /// <summary>
        /// Event raised when an argument is parsed from the command line.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   If the event handler sets the <see cref="CancelEventArgs.Cancel"/> property to <see langword="true"/>, command line processing will stop immediately,
        ///   and the <see cref="CommandLineParser.Parse(string[],int)"/> method will return <see langword="null"/>. The
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
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class using the specified arguments type, the specified argument name prefixes,
        /// and the specified <see cref="IComparer{T}"/> for comparing argument names.
        /// </summary>
        /// <param name="argumentsType">The <see cref="Type"/> of the class that defines the command line arguments.</param>
        /// <param name="argumentNamePrefixes">
        ///   Optional prefixes that are used to indicate argument names on the command line, or
        ///   <see langword="null"/> to use the prefixes from <see cref="ParseOptionsAttribute.ArgumentNamePrefixes"/>
        ///   or the default prefixes for the current platform.
        /// </param>
        /// <param name="argumentNameComparer">
        ///   An optional <see cref="IComparer{T}"/> that is used to match the names of arguments, or
        ///   <see langword="null"/> to use the default comparer, case-insensitive by default or
        ///   case-sensitive if specified using <see cref="ParseOptionsAttribute.CaseSensitive"/>.
        ///   </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="argumentsType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="argumentNamePrefixes"/> contains no elements or contains a <see langword="null"/> or empty string value.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   The <see cref="CommandLineParser"/> cannot use <paramref name="argumentsType"/> as the command line arguments type, because it defines a required
        ///   positional argument after an optional positional argument, it defines a positional array argument that is not the last positional argument, it defines an argument with an invalid name,
        ///   it defines two arguments with the same name, or it has two properties with the same <see cref="CommandLineArgumentAttribute.Position"/> property value.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   If you specify multiple argument name prefixes, the first one will be used when generating usage information using the <see cref="WriteUsage(TextWriter,int,WriteUsageOptions)"/> method.
        /// </para>
        /// </remarks>
        public CommandLineParser(Type argumentsType, IEnumerable<string>? argumentNamePrefixes, IComparer<string>? argumentNameComparer = null)
            : this(argumentsType, CreateOptions(argumentNamePrefixes, argumentNameComparer))
        {
        }

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
        /// <remarks>
        /// <para>
        ///   The <see cref="ParseOptions.UsageOptions"/> are not used here. If you want those to
        ///   take effect, they must still be passed to <see cref="WriteUsage(TextWriter, int, WriteUsageOptions)"/>.
        /// </para>
        /// </remarks>
        public CommandLineParser(Type argumentsType, ParseOptions? options = null)
        {
            _argumentsType = argumentsType ?? throw new ArgumentNullException(nameof(argumentsType));
            _stringProvider = options?.StringProvider ?? new LocalizedStringProvider();

            var optionsAttribute = _argumentsType.GetCustomAttribute<ParseOptionsAttribute>();
            _mode = options?.Mode ?? optionsAttribute?.Mode ?? ParsingMode.Default;
            _nameTransform = options?.NameTransform ?? optionsAttribute?.NameTransform ?? NameTransform.None;
            var comparer = options?.ArgumentNameComparer ?? optionsAttribute?.GetStringComparer() ?? StringComparer.OrdinalIgnoreCase;
            var prefixes = options?.ArgumentNamePrefixes ?? optionsAttribute?.ArgumentNamePrefixes;
            _argumentNamePrefixes = DetermineArgumentNamePrefixes(prefixes);
            var prefixInfos = _argumentNamePrefixes.Select(p => new PrefixInfo { Prefix = p, Short = true });
            if (_mode == ParsingMode.LongShort)
            {
                _longArgumentNamePrefix = options?.LongArgumentNamePrefix ?? optionsAttribute?.LongArgumentNamePrefix ??
                    DefaultLongArgumentNamePrefix;

                if (string.IsNullOrWhiteSpace(_longArgumentNamePrefix))
                {
                    throw new ArgumentException(Properties.Resources.EmptyArgumentNamePrefix, nameof(options));
                }

                var longInfo = new PrefixInfo { Prefix = _longArgumentNamePrefix, Short = false };
                prefixInfos = prefixInfos.Append(longInfo);
                _argumentsByShortName = new(comparer);
            }

            _sortedPrefixes = prefixInfos.OrderByDescending(info => info.Prefix.Length).ToArray();

            _argumentsByName = new(comparer);

            _commandLineConstructor = GetCommandLineConstructor();

            DetermineConstructorArguments();
            _constructorArgumentCount = _arguments.Count;
            _positionalArgumentCount = _constructorArgumentCount + DetermineMemberArguments();
            DetermineAutomaticArguments(options, optionsAttribute);
            if (_arguments.Count > _constructorArgumentCount)
            {
                // Sort the member arguments in usage order (positional first, then required
                // non-positional arguments, then the rest by name.
                _arguments.Sort(_constructorArgumentCount, _arguments.Count - _constructorArgumentCount, new CommandLineArgumentComparer(_argumentsByName.Comparer));
            }

            VerifyPositionalArgumentRules();

            AllowDuplicateArguments = options?.AllowDuplicateArguments ?? optionsAttribute?.AllowDuplicateArguments ?? false;
            AllowWhiteSpaceValueSeparator = options?.AllowWhiteSpaceValueSeparator ?? optionsAttribute?.AllowWhiteSpaceValueSeparator ?? true;
            NameValueSeparator = options?.NameValueSeparator ?? optionsAttribute?.NameValueSeparator ?? DefaultNameValueSeparator;
            Culture = options?.Culture ?? CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Gets the command line argument parsing rules used by the parser.
        /// </summary>
        /// <value>
        /// The <see cref="Ookii.CommandLine.ParsingMode"/> for this parser. The default is
        /// <see cref="ParsingMode.Default"/>.
        /// </value>
        public ParsingMode Mode => _mode;

        /// <summary>
        /// Gets or sets a value that indicates how names were created for arguments that didn't have
        /// an explicit name.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="NameTransform"/> enumeration.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If an argument didn't have the <see cref="CommandLineArgumentAttribute.ArgumentName"/>
        ///   property set (or doesn't have an <see cref="ArgumentNameAttribute"/> attribute for
        ///   constructor parameters), the argument name was determined by taking the name of the
        ///   property, constructor parameter, or method that defines it, and applying the specified
        ///   transform.
        /// </para>
        /// </remarks>
        public NameTransform NameTransform => _nameTransform;

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
        public ReadOnlyCollection<string> ArgumentNamePrefixes =>
            _argumentNamePrefixesReadOnlyWrapper ??= new(_argumentNamePrefixes);

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
        public string? LongArgumentNamePrefix => _longArgumentNamePrefix;

        /// <summary>
        /// Gets the type that was used to define the arguments.
        /// </summary>
        /// <value>
        /// The type that was used to define the arguments.
        /// </value>
        public Type ArgumentsType
        {
            get { return _argumentsType; }
        }

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
        ///   neither exists, the arguments type's assembly's name is used.
        /// </para>
        /// <para>
        ///   This name is only used in the output of the automatically created "-Version"
        ///   attribute.
        /// </para>
        /// </remarks>
        public string ApplicationFriendlyName
        {
            get
            {
                var attribute = _argumentsType.GetCustomAttribute<ApplicationFriendlyNameAttribute>() ??
                    _argumentsType.Assembly.GetCustomAttribute<ApplicationFriendlyNameAttribute>();

                return attribute?.Name ?? _argumentsType.Assembly.GetName().Name ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets a description that is used when generating usage information.
        /// </summary>
        /// <value>
        /// The description of the command line application. The default value is an empty string ("").
        /// </value>
        /// <remarks>
        /// <para>
        ///   This description will be added to the usage returned by the <see cref="CommandLineParser.WriteUsage(TextWriter, int, WriteUsageOptions)"/> property. This description can be set by applying
        ///   the <see cref="DescriptionAttribute"/> to the command line arguments type.
        /// </para>
        /// </remarks>
        public string Description
        {
            get
            {
                var description = (DescriptionAttribute?)Attribute.GetCustomAttribute(_argumentsType, typeof(DescriptionAttribute));
                return description?.Description ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the culture used to convert command line argument values from their string representation to the argument type.
        /// </summary>
        /// <value>
        /// The culture used to convert command line argument values from their string representation to the argument type. The default value
        /// is <see cref="CultureInfo.InvariantCulture"/>.
        /// </value>
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;


        /// <summary>
        /// Gets or sets a value indicating whether duplicate arguments are allowed.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if it is allowed to supply non-array arguments more than once; otherwise, <see langword="false"/>.
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
        ///   The <see cref="AllowDuplicateArguments"/> property has no effect on arguments whose <see cref="CommandLineArgument.ArgumentType"/> is an array, which can
        ///   always be supplied multiple times.
        /// </para>
        /// </remarks>
        public bool AllowDuplicateArguments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value of arguments may be separated from the name by white space.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if white space is allowed to separate an argument name and its value; <see langword="false"/> if only the character
        ///   specified in the <see cref="NameValueSeparator"/> property is allowed.
        ///   The default value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If the <see cref="AllowWhiteSpaceValueSeparator"/> property is <see langword="true"/>,
        ///   the value of an argument can be separated from its name either by using  the character
        ///   specified in the <see cref="NameValueSeparator"/> property or by using white space.
        ///   Given a named argument named "sample", the command lines <c>-sample:value</c> and <c>-sample value</c>
        ///   are both valid and will assign the value "value" to the argument.
        /// </para>
        /// <para>
        ///   If the <see cref="AllowWhiteSpaceValueSeparator"/> property is <see langword="false"/>, only the character
        ///   specified in the <see cref="NameValueSeparator"/> property is allowed to separate the value from the name.
        ///   The command line <c>-sample:value</c> still assigns the value "value" to the argument, but for the command line "-sample value" the argument 
        ///   is considered not to have a value (which is only valid if <see cref="CommandLineArgument.IsSwitch"/> is <see langword="true"/>), and
        ///   "value" is considered to be the value for the next positional argument.
        /// </para>
        /// <para>
        ///   For switch arguments (<see cref="CommandLineArgument.IsSwitch"/> is <see langword="true"/>),
        ///   only  the character specified in the <see cref="NameValueSeparator"/> property is allowed
        ///   to specify an explicit value regardless of the value of the <see cref="AllowWhiteSpaceValueSeparator"/>
        ///   property. Given a switch argument named "switch"  the command line <c>-switch false</c>
        ///   is interpreted to mean that the value of "switch" is <see langword="true"/> and the value of the
        ///   next positional argument is "false", even if the <see cref="AllowWhiteSpaceValueSeparator"/>
        ///   property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        public bool AllowWhiteSpaceValueSeparator { get; set; }

        /// <summary>
        /// Gets or sets the character used to separate the name and the value of an argument.
        /// </summary>
        /// <value>
        ///   The character used to separate the name and the value of an argument. The default value is the
        ///   <see cref="DefaultNameValueSeparator"/> constant, a colon (:).
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
        public char NameValueSeparator { get; set; } = DefaultNameValueSeparator;

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
        ///  This property will be <see langword="true"/> if the <see cref="Parse(string[], int)"/>
        ///  method threw a <see cref="CommandLineArgumentException"/>, if an argument used
        ///  <see cref="CommandLineArgumentAttribute.CancelParsing"/>, if parsing was canceled
        ///  using the <see cref="ArgumentParsed"/> event, or if an argument with <see cref="ArgumentKind.Method"/>
        ///  canceled parsing and explicitly set this value.
        /// </para>
        /// <para>
        ///   It will always be <see langword="false"/> if <see cref="Parse(string[], int)"/>
        ///   returned a non-null value.
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
        public LocalizedStringProvider StringProvider => _stringProvider;

        /// <summary>
        /// Gets the string comparer used for argument names.
        /// </summary>
        /// <value>
        /// An instance of a class implementing the <see cref="IComparer{T}"/> interface.
        /// </value>
        public IComparer<string> ArgumentNameComparer => _argumentsByName.Comparer;

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
        public ReadOnlyCollection<CommandLineArgument> Arguments
        {
            get
            {
                return _argumentsReadOnlyWrapper ?? (_argumentsReadOnlyWrapper = _arguments.AsReadOnly());
            }
        }

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
        public static string GetExecutableName(bool includeExtension = false)
        {
            string? path = null;
#if NET6_0_OR_GREATER
            // Prefer this because it actually returns the exe name, not the dll.
            path = Environment.ProcessPath;

            // Fall back if this returned the dotnet executable.
            if (Path.GetFileNameWithoutExtension(path) == "dotnet")
            {
                path = null;
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
                path = Path.GetFileNameWithoutExtension(path);
            }

            return path;
        }

        /// <summary>
        /// Writes command line usage help to the standard output stream using the specified options.
        /// </summary>
        /// <param name="options">
        ///   The options to use for formatting the usage. If <see langword="null"/>, the default
        ///   options are used.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <see cref="WriteUsageOptions.Indent"/> is less than zero or greater than or equal to <see cref="Console.WindowWidth"/> - 1, or 
        ///   <see cref="WriteUsageOptions.ArgumentDescriptionIndent"/> is less than zero or greater than or equal to <see cref="Console.WindowWidth"/> - 1.
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     The usage help consists of first the <see cref="Description"/>, followed by the usage syntax, followed by a description of all the arguments.
        ///   </para>
        ///   <para>
        ///     You can add descriptions to the usage text by applying the <see cref="DescriptionAttribute"/> attribute to your command line arguments type,
        ///     and the constructor parameters and properties defining command line arguments.
        ///   </para>
        ///   <para>
        ///     Line wrapping at word boundaries is applied to the output, wrapping at the console's window width. When the console output is
        ///     redirected to a file, Microsoft .Net will still report the console's actual window width, but on Mono the value of
        ///     the <see cref="Console.WindowWidth"/> property will be 0. In that case, the usage information will not be wrapped.
        ///   </para>
        ///   <para>
        ///     This method indents additional lines for the usage syntax and argument descriptions, unless the <see cref="Console.WindowWidth"/> property is less than 31.
        ///   </para>
        /// </remarks>
        public void WriteUsageToConsole(WriteUsageOptions? options = null)
        {
            options ??= new();
            using var vtSupport = options.EnableColor();

            WriteUsage(Console.Out, Console.WindowWidth - 1, options);
        }

        /// <summary>
        /// Writes command line usage help to the specified <see cref="TextWriter"/> using the specified options.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the usage to.</param>
        /// <param name="maximumLineLength">
        ///   The maximum line length of lines in the usage text; if <paramref name="writer"/> is an instance 
        ///   of <see cref="LineWrappingTextWriter"/>, this parameter is ignored. A value less than 1 or larger than 65536 is interpreted as infinite line length.
        /// </param>
        /// <param name="options">
        ///   The options to use for formatting the usage. If <see langword="null"/>, the default
        ///   options are used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="writer"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <see cref="WriteUsageOptions.Indent"/> is less than zero or greater than or equal to <paramref name="maximumLineLength"/>, or 
        ///   <see cref="WriteUsageOptions.ArgumentDescriptionIndent"/> is less than zero or greater than or equal to <paramref name="maximumLineLength"/>.
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     The usage help consists of first the <see cref="Description"/>, followed by the usage syntax, followed by a description of all the arguments.
        ///   </para>
        ///   <para>
        ///     You can add descriptions to the usage text by applying the <see cref="DescriptionAttribute"/> attribute to your command line arguments type,
        ///     and the constructor parameters and properties defining command line arguments.
        ///   </para>
        ///   <para>
        ///     Line wrapping at word boundaries is applied to the output, wrapping at the specified line length. If the specified <paramref name="writer"/>
        ///     is an instance of the <see cref="LineWrappingTextWriter"/> class, its <see cref="LineWrappingTextWriter.MaximumLineLength"/> property is used
        ///     and the <paramref name="maximumLineLength"/> parameter is ignored.
        ///   </para>
        ///   <para>
        ///     This method indents additional lines for the usage syntax and argument descriptions, unless the maximum line length is less than 30.
        ///   </para>
        /// </remarks>
        public void WriteUsage(TextWriter writer, int maximumLineLength, WriteUsageOptions? options = null)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            options ??= new();
            using var lineWriter = DisposableWrapper.Create(writer as LineWrappingTextWriter,
                () => new LineWrappingTextWriter(writer, maximumLineLength, false));

            if (options.IncludeApplicationDescription && !string.IsNullOrEmpty(Description))
            {
                lineWriter.Inner.WriteLine(Description);
                lineWriter.Inner.WriteLine();
            }

            WriteUsageSyntax(lineWriter.Inner, options);
            WriteClassValidatorHelp(lineWriter.Inner, options);
            WriteArgumentDescriptions(lineWriter.Inner, options);
        }

        /// <summary>
        /// Gets command line usage help using the default options and no line wrapping.
        /// </summary>
        /// <returns>
        ///   A string containing usage help for the command line options defined by the type
        ///   specified by <see cref="ArgumentsType"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     The usage help consists of first the <see cref="Description"/>, followed by the usage syntax, followed by a description of all the arguments.
        ///   </para>
        ///   <para>
        ///     You can add descriptions to the usage text by applying the <see cref="DescriptionAttribute"/> attribute to your command line arguments type,
        ///     and the constructor parameters and properties defining command line arguments.
        ///   </para>
        ///   <para>
        ///     This method indents additional lines for the usage syntax and argument descriptions.
        ///   </para>
        /// </remarks>
        public string GetUsage()
        {
            return GetUsage(0, new WriteUsageOptions());
        }

        /// <summary>
        /// Gets command line usage help.
        /// </summary>
        /// <param name="maximumLineLength">
        ///   The maximum line length of lines in the usage text; . A value less than 1 or larger
        ///   than 65536 is interpreted as infinite line length.
        /// </param>
        /// <param name="options">
        ///   The options to use for formatting the usage. If <see langword="null"/>, the default
        ///   options are used.
        /// </param>
        /// <returns>
        ///   A string containing usage help for the command line options defined by the type
        ///   specified by <see cref="ArgumentsType"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <see cref="WriteUsageOptions.Indent"/> is less than zero or greater than or equal to <paramref name="maximumLineLength"/>, or 
        ///   <see cref="WriteUsageOptions.ArgumentDescriptionIndent"/> is less than zero or greater than or equal to <paramref name="maximumLineLength"/>.
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     The usage help consists of first the <see cref="Description"/>, followed by the usage syntax, followed by a description of all the arguments.
        ///   </para>
        ///   <para>
        ///     You can add descriptions to the usage text by applying the <see cref="DescriptionAttribute"/> attribute to your command line arguments type,
        ///     and the constructor parameters and properties defining command line arguments.
        ///   </para>
        ///   <para>
        ///     Line wrapping at word boundaries is applied to the output, wrapping at the specified line length.
        ///   </para>
        ///   <para>
        ///     This method indents additional lines for the usage syntax and argument descriptions, unless the maximum line length is less than 30.
        ///   </para>
        /// </remarks>
        public string GetUsage(int maximumLineLength = 0, WriteUsageOptions? options = null)
        {
            using var writer = new StringWriter();
            WriteUsage(writer, maximumLineLength, options);
            return writer.ToString();
        }

        /// <summary>
        /// Parses the current application's arguments.
        /// </summary>
        /// <returns>
        ///   An instance of the type specified by the <see cref="ArgumentsType"/> property, or <see langword="null"/> if argument
        ///   parsing was canceled by the <see cref="ArgumentParsed"/> event handler, the
        ///   <see cref="CommandLineArgumentAttribute.CancelParsing"/> property, or an argument
        ///   with <see cref="ArgumentKind.Method"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   If the return value is <see langword="null"/>, check the <see cref="HelpRequested"/>
        ///   property to see if usage help should be displayed.
        /// </para>
        /// </remarks>
        /// <exception cref="CommandLineArgumentException">
        ///   Too many positional arguments were supplied, a required argument was not supplied, an unknown argument name was supplied,
        ///   no value was supplied for a named argument, an argument was supplied more than once and <see cref="AllowDuplicateArguments"/>
        ///   is <see langword="false"/>, or one of the argument values could not be converted to the argument's type.
        /// </exception>
        public object? Parse()
        {
            // GetCommandLineArgs include the executable, so skip it.
            return Parse(Environment.GetCommandLineArgs(), 1);
        }

        /// <summary>
        /// Parses the specified command line arguments, starting at the specified index.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="index">The index of the first argument to parse.</param>
        /// <returns>
        ///   An instance of the type specified by the <see cref="ArgumentsType"/> property, or <see langword="null"/> if argument
        ///   parsing was canceled by the <see cref="ArgumentParsed"/> event handler, the
        ///   <see cref="CommandLineArgumentAttribute.CancelParsing"/> property, or an argument
        ///   with <see cref="ArgumentKind.Method"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   If the return value is <see langword="null"/>, check the <see cref="HelpRequested"/>
        ///   property to see if usage help should be displayed.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="args"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="index"/> does not fall within the bounds of <paramref name="args"/>.
        /// </exception>
        /// <exception cref="CommandLineArgumentException">
        ///   Too many positional arguments were supplied, a required argument was not supplied, an unknown argument name was supplied,
        ///   no value was supplied for a named argument, an argument was supplied more than once and <see cref="AllowDuplicateArguments"/>
        ///   is <see langword="false"/>, or one of the argument values could not be converted to the argument's type.
        /// </exception>
        public object? Parse(string[] args, int index = 0)
        {
            try
            {
                HelpRequested = false;
                return ParseCore(args, index);
            }
            catch (CommandLineArgumentException)
            {
                HelpRequested = true;
                throw;
            }
        }

        /// <summary>
        /// Parses the current application's arguments.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   This is a convenience function that instantiates a <see cref="CommandLineParser"/>,
        ///   calls <see cref="Parse()"/>, and returns the result. If an error occurs
        ///   or parsing is canceled, it prints error and usage according to <see cref="ParseOptions.Error"/>
        ///   and <see cref="ParseOptions.Out"/> respectively.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property or <see cref="ParseOptions.Error"/>
        ///   property is <see langword="null"/>, output is written to a <see cref="LineWrappingTextWriter"/>
        ///   for the standard output and error streams respectively, wrapping at the console's
        ///   window width. When the console output is redirected to a file, Microsoft .Net will
        ///   still report the console's actual window width, but on Mono the value of the
        ///   <see cref="Console.WindowWidth"/> property will be 0. In that case, the usage
        ///   information will not be wrapped.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property is instance of the
        ///   <see cref="LineWrappingTextWriter"/> class, this method indents additional lines for
        ///   the usage syntax and argument descriptions according to the values specified by the
        ///   <see cref="CommandOptions"/>, unless the <see cref="LineWrappingTextWriter.MaximumLineLength"/>
        ///   property is less than 30.
        /// </para>
        /// <para>
        ///   If you want more control over the parsing process, including custom error/usage output
        ///   or handling the <see cref="ArgumentParsed"/> event, do not use this function; instead
        ///   perform these steps manually.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type defining the command line arguments.</typeparam>
        /// <param name="options">
        ///   The options that control parsing behavior. If <see langword="null" />, the default
        ///   options are used.
        /// </param>
        /// <returns>
        ///   An instance of the type <typeparamref name="T"/>, or <see langword="null"/> if an
        ///   error occurred or if argument parsing was canceled by the <see cref="ArgumentParsed"/>
        ///   event handler or the <see cref="CommandLineArgumentAttribute.CancelParsing"/> property.
        /// </returns>
        /// <exception cref="CommandLineArgumentException">
        ///   Too many positional arguments were supplied, a required argument was not supplied, an unknown argument name was supplied,
        ///   no value was supplied for a named argument, an argument was supplied more than once and <see cref="AllowDuplicateArguments"/>
        ///   is <see langword="false"/>, or one of the argument values could not be converted to the argument's type.
        /// </exception>
        public static T? Parse<T>(ParseOptions? options = null)
            where T : class
        {
            // GetCommandLineArgs include the executable, so skip it.
            return Parse<T>(Environment.GetCommandLineArgs(), 1, options);
        }

        /// <summary>
        /// Parses the specified command line arguments, starting at the specified index, into the
        /// specified type.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   This is a convenience function that instantiates a <see cref="CommandLineParser"/>,
        ///   calls <see cref="Parse(string[], int)"/>, and returns the result. If an error occurs
        ///   or parsing is canceled, it prints error and usage according to <see cref="ParseOptions.Error"/>
        ///   and <see cref="ParseOptions.Out"/> respectively.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property or <see cref="ParseOptions.Error"/>
        ///   property is <see langword="null"/>, output is written to a <see cref="LineWrappingTextWriter"/>
        ///   for the standard output and error streams respectively, wrapping at the console's
        ///   window width. When the console output is redirected to a file, Microsoft .Net will
        ///   still report the console's actual window width, but on Mono the value of the
        ///   <see cref="Console.WindowWidth"/> property will be 0. In that case, the usage
        ///   information will not be wrapped.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property is instance of the
        ///   <see cref="LineWrappingTextWriter"/> class, this method indents additional lines for
        ///   the usage syntax and argument descriptions according to the values specified by the
        ///   <see cref="CommandOptions"/>, unless the <see cref="LineWrappingTextWriter.MaximumLineLength"/>
        ///   property is less than 30.
        /// </para>
        /// <para>
        ///   If you want more control over the parsing process, including custom error/usage output
        ///   or handling the <see cref="ArgumentParsed"/> event, do not use this function; instead
        ///   perform these steps manually.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type defining the command line arguments.</typeparam>
        /// <param name="args">The command line arguments.</param>
        /// <param name="index">The index of the first argument to parse.</param>
        /// <param name="options">
        ///   The options that control parsing behavior. If <see langword="null" />, the default
        ///   options are used.
        /// </param>
        /// <returns>
        ///   An instance of the type <typeparamref name="T"/>, or <see langword="null"/> if an
        ///   error occurred or if argument parsing was canceled by the <see cref="ArgumentParsed"/>
        ///   event handler or the <see cref="CommandLineArgumentAttribute.CancelParsing"/> property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="args"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="index"/> does not fall within the bounds of <paramref name="args"/>.
        /// </exception>
        /// <exception cref="CommandLineArgumentException">
        ///   Too many positional arguments were supplied, a required argument was not supplied, an unknown argument name was supplied,
        ///   no value was supplied for a named argument, an argument was supplied more than once and <see cref="AllowDuplicateArguments"/>
        ///   is <see langword="false"/>, or one of the argument values could not be converted to the argument's type.
        /// </exception>
        public static T? Parse<T>(string[] args, int index, ParseOptions? options = null)
            where T : class
        {
            return (T?)ParseInternal(typeof(T), args, index, options);
        }

        /// <summary>
        /// Parses the specified command line arguments into the specified type.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   This is a convenience function that instantiates a <see cref="CommandLineParser"/>,
        ///   calls <see cref="Parse(string[], int)"/>, and returns the result. If an error occurs
        ///   or parsing is canceled, it prints error and usage according to <see cref="ParseOptions.Error"/>
        ///   and <see cref="ParseOptions.Out"/> respectively.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property or <see cref="ParseOptions.Error"/>
        ///   property is <see langword="null"/>, output is written to a <see cref="LineWrappingTextWriter"/>
        ///   for the standard output and error streams respectively, wrapping at the console's
        ///   window width. When the console output is redirected to a file, Microsoft .Net will
        ///   still report the console's actual window width, but on Mono the value of the
        ///   <see cref="Console.WindowWidth"/> property will be 0. In that case, the usage
        ///   information will not be wrapped.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property is instance of the
        ///   <see cref="LineWrappingTextWriter"/> class, this method indents additional lines for
        ///   the usage syntax and argument descriptions according to the values specified by the
        ///   <see cref="CommandOptions"/>, unless the <see cref="LineWrappingTextWriter.MaximumLineLength"/>
        ///   property is less than 30.
        /// </para>
        /// <para>
        ///   If you want more control over the parsing process, including custom error/usage output
        ///   or handling the <see cref="ArgumentParsed"/> event, do not use this function; instead
        ///   perform these steps manually.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type defining the command line arguments.</typeparam>
        /// <param name="args">The command line arguments.</param>
        /// <param name="options">
        ///   The options that control parsing behavior. If <see langword="null" />, the default
        ///   options are used.
        /// </param>
        /// <returns>
        ///   An instance of the type <typeparamref name="T"/>, or <see langword="null"/> if an
        ///   error occurred or if argument parsing was canceled by the <see cref="ArgumentParsed"/>
        ///   event handler or the <see cref="CommandLineArgumentAttribute.CancelParsing"/> property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="args"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="CommandLineArgumentException">
        ///   Too many positional arguments were supplied, a required argument was not supplied, an unknown argument name was supplied,
        ///   no value was supplied for a named argument, an argument was supplied more than once and <see cref="AllowDuplicateArguments"/>
        ///   is <see langword="false"/>, or one of the argument values could not be converted to the argument's type.
        /// </exception>
        public static T? Parse<T>(string[] args, ParseOptions? options = null)
            where T : class
        {
            return Parse<T>(args, 0, options);
        }

        /// <summary>
        /// Gets a command line argument by name.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <returns>The <see cref="CommandLineArgument"/> instance containing information about
        /// the argument, or <see langword="null" /> if the argument was not found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        public CommandLineArgument? GetArgument(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_argumentsByName.TryGetValue(name, out var argument))
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
            if (_argumentsByShortName != null && _argumentsByShortName.TryGetValue(shortName.ToString(), out var argument))
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
        /// An array containing the default separators for the current platform.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   The default prefixes for each platform are:
        /// </para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Platform</term>
        ///     <description>Separators</description>
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
        ///   If <see cref="Mode"/> is <see cref="ParsingMode.LongShort"/>, these
        ///   prefixes will be used for short argument names. The <see cref="DefaultLongArgumentNamePrefix"/>
        ///   is the default prefix for long argument names.
        /// </para>
        /// </remarks>
        public static string[] GetDefaultArgumentNamePrefixes()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new[] { "-", "/" }
                : new[] { "-" };
        }

        /// <summary>
        /// Raises the <see cref="ArgumentParsed"/> event.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected virtual void OnArgumentParsed(ArgumentParsedEventArgs e)
        {
            ArgumentParsed?.Invoke(this, e);
        }

        internal static object? ParseInternal(Type argumentsType, string[] args, int index, ParseOptions? options)
        {
            options ??= new();
            var parser = new CommandLineParser(argumentsType, options);

            using var vtSupport = options.EnableOutputColor();
            using var output = DisposableWrapper.Create(options.Out, LineWrappingTextWriter.ForConsoleOut);
            object? result = null;
            try
            {
                result = parser.Parse(args, index);
            }
            catch (CommandLineArgumentException ex)
            {
                using var errorVtSupport = options.EnableErrorColor();
                using var error = DisposableWrapper.Create(options.Error, LineWrappingTextWriter.ForConsoleError);
                if (options.UseErrorColor ?? false)
                {
                    error.Inner.Write(options.ErrorColor);
                }

                error.Inner.Write(ex.Message);
                if (options.UseErrorColor ?? false)
                {
                    error.Inner.Write(options.UsageOptions.ColorReset);
                }

                error.Inner.WriteLine();
                error.Inner.WriteLine();
            }

            if (parser.HelpRequested)
            {
                // If we're writing this to the console, output should already be a
                // LineWrappingTextWriter, so the max line length argument here is ignored.
                parser.WriteUsage(output.Inner, 0, options.UsageOptions);
            }

            return result;
        }

        internal static bool ShouldIndent(LineWrappingTextWriter writer)
        {
            return writer.MaximumLineLength is 0 or >= MinimumLineWidthForIndent;
        }

        private static string[] DetermineArgumentNamePrefixes(IEnumerable<string>? namedArgumentPrefixes)
        {
            if (namedArgumentPrefixes == null)
            {
                return GetDefaultArgumentNamePrefixes();
            }
            else
            {
                var result = namedArgumentPrefixes.ToArray();
                if (result.Length == 0)
                {
                    throw new ArgumentException(Properties.Resources.EmptyArgumentNamePrefixes, nameof(namedArgumentPrefixes));
                }

                if (result.Any(prefix => string.IsNullOrWhiteSpace(prefix)))
                {
                    throw new ArgumentException(Properties.Resources.EmptyArgumentNamePrefix, nameof(namedArgumentPrefixes));
                }

                return result;
            }
        }

        private void DetermineConstructorArguments()
        {
            ParameterInfo[] parameters = _commandLineConstructor.GetParameters();
            foreach (ParameterInfo parameter in parameters)
            {
                CommandLineArgument argument = CommandLineArgument.Create(this, parameter);
                AddNamedArgument(argument);
            }
        }

        private int DetermineMemberArguments()
        {
            int additionalPositionalArgumentCount = 0;

            MemberInfo[] properties = _argumentsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo[] methods = _argumentsType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var member in properties.Concat(methods))
            {
                if (Attribute.IsDefined(member, typeof(CommandLineArgumentAttribute)))
                {
                    var argument = member switch
                    {
                        PropertyInfo prop => CommandLineArgument.Create(this, prop),
                        MethodInfo method => CommandLineArgument.Create(this, method),
                        _ => throw new InvalidOperationException(),
                    };

                    AddNamedArgument(argument);
                    if (argument.Position != null)
                    {
                        ++additionalPositionalArgumentCount;
                    }
                }
            }

            return additionalPositionalArgumentCount;
        }

        private void DetermineAutomaticArguments(ParseOptions? options, ParseOptionsAttribute? optionsAttribute)
        {
            bool autoHelp = options?.AutoHelpArgument ?? optionsAttribute?.AutoHelpArgument ?? true;
            if (autoHelp)
            {
                var argument = CommandLineArgument.CreateAutomaticHelp(this);
                if (argument != null)
                {
                    AddNamedArgument(argument);
                }
            }

            bool autoVersion = options?.AutoVersionArgument ?? optionsAttribute?.AutoVersionArgument ?? true;
            if (autoVersion && !CommandInfo.IsCommand(_argumentsType))
            {
                var argument = CommandLineArgument.CreateAutomaticVersion(this);
                if (argument != null)
                {
                    AddNamedArgument(argument);
                }
            }
        }

        private void AddNamedArgument(CommandLineArgument argument)
        {
            if (argument.ArgumentName.Contains(NameValueSeparator))
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.ArgumentNameContainsSeparatorFormat, argument.ArgumentName));
            }

            if (argument.HasLongName)
            {
                _argumentsByName.Add(argument.ArgumentName, argument);
                if (argument.Aliases != null)
                {
                    foreach (string alias in argument.Aliases)
                    {
                        _argumentsByName.Add(alias, argument);
                    }
                }
            }

            if (_argumentsByShortName != null && argument.HasShortName)
            {
                _argumentsByShortName.Add(argument.ShortName.ToString(), argument);
                if (argument.ShortAliases != null)
                {
                    foreach (var alias in argument.ShortAliases)
                    {
                        _argumentsByShortName.Add(alias.ToString(), argument);
                    }
                }
            }

            _arguments.Add(argument);
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

        private object? ParseCore(string[] args, int index)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(index));
            }

            if (index < 0 || index > args.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            // Reset all arguments to their default value.
            foreach (CommandLineArgument argument in _arguments)
            {
                argument.Reset();
            }

            HelpRequested = false;
            int positionalArgumentIndex = 0;

            for (int x = index; x < args.Length; ++x)
            {
                string arg = args[x];
                var argumentNamePrefix = CheckArgumentNamePrefix(arg);
                if (argumentNamePrefix != null)
                {
                    // If white space was the value separator, this function returns the index of argument containing the value for the named argument.
                    // It returns -1 if parsing was canceled by the ArgumentParsed event handler or the CancelParsing property.
                    x = ParseNamedArgument(args, x, argumentNamePrefix.Value);
                    if (x < 0)
                    {
                        return null;
                    }
                }
                else
                {
                    // If this is an array argument is must be the last argument.
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

                    // ParseArgumentValue returns true if parsing was canceled by the ArgumentParsed event handler
                    // or the CancelParsing property.
                    if (ParseArgumentValue(_arguments[positionalArgumentIndex], arg))
                    {
                        return null;
                    }
                }
            }

            // Check required arguments and post-parsing validation. This is done in usage order.
            foreach (CommandLineArgument argument in _arguments)
            {
                argument.ValidateAfterParsing();
            }

            // Run class validators.
            foreach (var validator in _argumentsType.GetCustomAttributes<ClassValidationAttribute>())
            {
                validator.Validate(this);
            }

            var constructorArgumentValues = new object?[_constructorArgumentCount];
            for (int x = 0; x < _constructorArgumentCount; ++x)
            {
                constructorArgumentValues[x] = _arguments[x].Value;
            }

            object commandLineArguments = CreateArgumentsTypeInstance(constructorArgumentValues);
            foreach (CommandLineArgument argument in _arguments)
            {
                // Apply property argument values (this does nothing for constructor or method arguments).
                argument.ApplyPropertyValue(commandLineArguments);
            }

            return commandLineArguments;
        }

        private bool ParseArgumentValue(CommandLineArgument argument, string? value)
        {
            bool continueParsing = argument.SetValue(Culture, value);
            var e = new ArgumentParsedEventArgs(argument)
            {
                Cancel = !continueParsing
            };

            OnArgumentParsed(e);
            var cancel = e.Cancel || (argument.CancelParsing && !e.OverrideCancelParsing);

            // Automatically request help only if the cancellation was not due to the SetValue
            // call.
            if (continueParsing)
            {
                HelpRequested = cancel;
            }
            return cancel;
        }

        private int ParseNamedArgument(string[] args, int index, PrefixInfo prefix)
        {
            string argumentName;
            string? argumentValue = null;

            string arg = args[index];
            // Extract the argument name
            // We don't use Split because if there's more than one separator we want to ignore the others.
            int separatorIndex = arg.IndexOf(NameValueSeparator);
            if (separatorIndex >= 0)
            {
                argumentName = arg.Substring(prefix.Prefix.Length, separatorIndex - prefix.Prefix.Length);
                argumentValue = arg.Substring(separatorIndex + 1);
            }
            else
            {
                argumentName = arg.Substring(prefix.Prefix.Length);
            }

            CommandLineArgument? argument = null;
            if (_argumentsByShortName != null && prefix.Short)
            {
                if (argumentName.Length == 1)
                {
                    argument = GetShortArgumentOrThrow(argumentName);
                }
                else
                {
                    // ParseShortArgument returns true if parsing was canceled by the
                    // ArgumentParsed event handler or the CancelParsing property.
                    return ParseShortArgument(argumentName, argumentValue) ? -1 : index;
                }
            }

            if (argument == null && !_argumentsByName.TryGetValue(argumentName, out argument))
            {
                throw StringProvider.CreateException(CommandLineArgumentErrorCategory.UnknownArgument, argumentName);
            }

            if (argumentValue == null && !argument.IsSwitch && AllowWhiteSpaceValueSeparator && ++index < args.Length && CheckArgumentNamePrefix(args[index]) == null)
            {
                // No separator was present but a value is required. We take the next argument as its value.
                argumentValue = args[index];
            }

            // ParseArgumentValue returns true if parsing was canceled by the ArgumentParsed event handler
            // or the CancelParsing property.
            argument.UsedArgumentName = argumentName;
            return ParseArgumentValue(argument, argumentValue) ? -1 : index;
        }

        private bool ParseShortArgument(string name, string? value)
        {
            foreach (var ch in name)
            {
                var arg = GetShortArgumentOrThrow(ch.ToString());
                if (!arg.IsSwitch)
                {
                    throw StringProvider.CreateException(CommandLineArgumentErrorCategory.CombinedShortNameNonSwitch, name);
                }

                if (ParseArgumentValue(arg, value))
                {
                    return true;
                }
            }

            return false;
        }

        private CommandLineArgument GetShortArgumentOrThrow(string shortName)
        {
            Debug.Assert(shortName.Length == 1);
            if (_argumentsByShortName!.TryGetValue(shortName, out CommandLineArgument? argument))
            {
                return argument;
            }

            throw StringProvider.CreateException(CommandLineArgumentErrorCategory.UnknownArgument, shortName);
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

        private ConstructorInfo GetCommandLineConstructor()
        {
            ConstructorInfo[] ctors = _argumentsType.GetConstructors();
            if (ctors.Length < 1)
            {
                throw new NotSupportedException(Properties.Resources.NoConstructor);
            }
            else if (ctors.Length == 1)
            {
                return ctors[0];
            }

            var markedCtors = ctors.Where(c => Attribute.IsDefined(c, typeof(CommandLineConstructorAttribute)));
            if (!markedCtors.Any())
            {
                throw new NotSupportedException(Properties.Resources.NoMarkedConstructor);
            }
            else if (markedCtors.Count() > 1)
            {
                throw new NotSupportedException(Properties.Resources.MultipleMarkedConstructors);
            }

            return markedCtors.First();
        }

        private void WriteArgumentDescriptions(LineWrappingTextWriter writer, WriteUsageOptions options)
        {
            if (options.ArgumentDescriptionListFilter == DescriptionListFilterMode.None)
            {
                return;
            }

            if (!ShouldIndent(writer))
            {
                writer.Indent = 0;
            }
            else
            {
                writer.Indent = Mode == ParsingMode.LongShort
                    ? options.LongShortArgumentDescriptionIndent
                    : options.ArgumentDescriptionIndent;
            }

            var comparer = _argumentsByName.Comparer;
            IEnumerable<CommandLineArgument> arguments = options.ArgumentDescriptionListOrder switch
            {
                DescriptionListSortMode.Alphabetical => _arguments.OrderBy(arg => arg.ArgumentName, comparer),
                DescriptionListSortMode.AlphabeticalDescending => _arguments.OrderByDescending(arg => arg.ArgumentName, comparer),
                DescriptionListSortMode.AlphabeticalShortName =>
                    _arguments.OrderBy(arg => arg.HasShortName ? arg.ShortName.ToString() : arg.ArgumentName, comparer),
                DescriptionListSortMode.AlphabeticalShortNameDescending =>
                    _arguments.OrderByDescending(arg => arg.HasShortName ? arg.ShortName.ToString() : arg.ArgumentName, comparer),
                _ => _arguments,
            };

            foreach (var argument in arguments)
            {
                bool include = !argument.IsHidden && options.ArgumentDescriptionListFilter switch
                {
                    DescriptionListFilterMode.Information => argument.HasInformation(options),
                    DescriptionListFilterMode.Description => !string.IsNullOrEmpty(argument.Description),
                    DescriptionListFilterMode.All => true,
                    _ => false,
                };

                // Omit arguments that don't fit the filter.
                if (!include)
                {
                    continue;
                }

                writer.ResetIndent();
                writer.WriteLine(StringProvider.ArgumentDescription(argument, options));
            }
        }

        private void WriteUsageSyntax(LineWrappingTextWriter writer, WriteUsageOptions options)
        {
            writer.ResetIndent();
            writer.Indent = ShouldIndent(writer) ? options.Indent : 0;

            bool useColor = options.UseColor ?? false;
            string colorStart = string.Empty;
            string colorEnd = string.Empty;
            if (useColor)
            {
                colorStart = options.UsagePrefixColor;
                colorEnd = options.ColorReset;
            }

            string executableName = options.ExecutableName ?? GetExecutableName(options.IncludeExecutableExtension);
            string prefix = options.CommandName == null
                ? StringProvider.UsagePrefix(executableName, colorStart, colorEnd)
                : StringProvider.CommandUsagePrefix(executableName, options.CommandName, colorStart, colorEnd);

            writer.Write(prefix);

            foreach (CommandLineArgument argument in _arguments)
            {
                if (argument.IsHidden)
                {
                    continue;
                }

                writer.Write(" ");
                if (options.UseAbbreviatedSyntax && argument.Position == null)
                {
                    writer.Write(StringProvider.AbbreviatedRemainingArguments(useColor));
                    break;
                }

                writer.Write(argument.ToString(options));
            }

            writer.WriteLine(); // End syntax line
            writer.WriteLine(); // Blank line
        }

        private void WriteClassValidatorHelp(LineWrappingTextWriter writer, WriteUsageOptions options)
        {
            if (!options.IncludeValidatorsInDescription)
            {
                return;
            }

            writer.Indent = 0;
            bool hasHelp = false;
            foreach (var validator in _argumentsType.GetCustomAttributes<ClassValidationAttribute>())
            {
                var help = validator.GetUsageHelp(this);
                if (!string.IsNullOrEmpty(help))
                {
                    hasHelp = true;
                    writer.WriteLine(help);
                }
            }

            if (hasHelp)
            {
                writer.WriteLine(); // Blank line.
            }
        }

        private object CreateArgumentsTypeInstance(object?[] constructorArgumentValues)
        {
            try
            {
                return _commandLineConstructor.Invoke(constructorArgumentValues);
            }
            catch (TargetInvocationException ex)
            {
                throw StringProvider.CreateException(CommandLineArgumentErrorCategory.CreateArgumentsTypeError, ex.InnerException);
            }
        }

        private static ParseOptions? CreateOptions(IEnumerable<string>? argumentNamePrefixes, IComparer<string>? argumentNameComparer)
        {
            if (argumentNamePrefixes == null && argumentNameComparer == null)
            {
                return null;
            }

            return new ParseOptions()
            {
                ArgumentNamePrefixes = argumentNamePrefixes,
                ArgumentNameComparer = argumentNameComparer,
            };
        }
    }
}
