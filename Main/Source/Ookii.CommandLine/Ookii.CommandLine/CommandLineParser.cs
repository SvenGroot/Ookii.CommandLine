// Copyright (c) Sven Groot (Ookii.org)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at http://ookiicommandline.codeplex.com. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Parses command line arguments into a class of the specified type.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The <see cref="CommandLineParser"/> class can parse a set of command line arguments into values. Which arguments are
    ///   accepted is determined from the constructor parameters and properties of the type passed to the <see cref="CommandLineParser.CommandLineParser(Type)"/>
    ///   constructor. The result of a parsing operation is an instance of that type that was constructed using the constructor parameters and
    ///   property values from their respective command line arguments.
    /// </para>
    /// <para>
    ///   The <see cref="CommandLineParser"/> class can parse a command line and can generate usage help for arguments defined by the type
    ///   passed to its constructor. This usage help can be presented to the user to provide information about how to invoke your application
    ///   from the command line.
    /// </para>
    /// <para>
    ///   The command line arguments are parsed using the parsing rules described below. A command line consists of a series of
    ///   argument values; each value is assigned to the appropriate argument based on either the name or the position of the argument.
    /// </para>
    /// <para>
    ///   Every argument has a name, and can have its value specified by name. To specify an argument name on the command line it must
    ///   be preceded by a special prefix. On Windows, the argument name prefix is typically a forward
    ///   slash (/), while on Unix platforms it is usually a single dash (-) or double dash (--). Which prefixes
    ///   are accepted by the <see cref="CommandLineParser"/> class can be specified by using the <see cref="CommandLineParser.CommandLineParser(Type,IEnumerable{string})"/>
    ///   constructor. By default, it will accept both "/" and "-" on Windows, and only a "-" on all other platforms (other platforms are
    ///   supported via <a href="http://www.mono-project.com">Mono</a>).
    /// </para>
    /// <note>
    ///   Although almost any argument name is allowed as long as it isn't empty and doesn't contain a colon (:),
    ///   certain argument names may not be advisable. Particularly, avoid argument names that start with a number, as they it will
    ///   not be possible to specify them by name if the argument name prefix is a single dash; arguments starting with a single dash
    ///   followed by a digit are always considered values during parsing, even if there is an argument with that name.
    /// </note>
    /// <para>
    ///   The name of the argument must be followed by its value. The value can be either in the next argument (separated from the name
    ///   by white space), or separated by a colon (:). For example, to assign the value "foo" to the argument "sample", you can use
    ///   either <c>-sample foo</c> or <c>-sample:foo</c>.
    /// </para>
    /// <para>
    ///   If an argument has a type of <see cref="Boolean"/> (and is not a positional argument as described below), it is a switch argument, and doesn't require a value. Its value is determined
    ///   by its presence on the command line; if it is absent the value is <see langword="false"/>; if it is present the value is
    ///   <see langword="true"/>. For example, to set a switch argument named "verbose" to true, you can simply use the command line
    ///   <c>-verbose</c>. You can still explicitly specify the value of a switch argument, for example <c>-verbose:true</c>.
    ///   Note that you cannot use white space to separate a switch argument name and value; you must use a colon.
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
    ///   Arguments can either be required or optional. If an argument is required, the <see cref="CommandLineParser.Parse(string[])"/>
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
    ///   <see cref="CommandLineArgumentAttribute"/> attibute defined. The argument will only be positional if the <see cref="CommandLineArgumentAttribute.Position"/>
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

            public int Compare(CommandLineArgument x, CommandLineArgument y)
            {
                if( x == null )
                {
                    if( y == null )
                        return 0;
                    else
                        return -1;
                }
                else if( y == null )
                    return 1;

                // Positional arguments come before non-positional ones, and must be sorted by position
                if( x.Position != null )
                {
                    if( y.Position != null )
                        return x.Position.Value.CompareTo(y.Position.Value);
                    else
                        return -1;
                }
                else if( y.Position != null )
                    return 1;

                // Non-positional required arguments come before optional arguments
                if( x.IsRequired )
                {
                    if( !y.IsRequired )
                        return -1;
                    // If both are required, sort by name
                }
                else if( y.IsRequired )
                    return 1;

                // Sort the rest by name
                return _stringComparer.Compare(x.ArgumentName, y.ArgumentName);
            }
        }

        #endregion

        internal const char NameValueSeparator = ':';
        internal const int MaximumLineWidthForIndent = 30; // Don't apply indentation to console output if the line width is less than this.

        private readonly Type _argumentsType;
        private readonly List<CommandLineArgument> _arguments = new List<CommandLineArgument>();
        private readonly SortedList<string, CommandLineArgument> _argumentsByName;
        private readonly ConstructorInfo _commandLineConstructor;
        private readonly int _constructorArgumentCount;
        private readonly int _positionalArgumentCount;
        private readonly string[] _argumentNamePrefixes;
        private ReadOnlyCollection<CommandLineArgument> _argumentsReadOnlyWrapper;
        private ReadOnlyCollection<string> _argumentNamePrefixesReadOnlyWrapper;
        private CultureInfo _culture;

        /// <summary>
        /// Event raised when an argument is parsed from the command line.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   If the event handler sets the <see cref="CancelEventArgs.Cancel"/> property to <see langword="true"/>, command line processing will stop immediately,
        ///   and the <see cref="CommandLineParser.Parse(string[],int)"/> method will return <see langword="null"/>. You can use this for instance to implement a "-help"
        ///   argument that will display usage and quit regardless of the other command line arguments.
        /// </para>
        /// <para>
        ///   This event is invoked after the <see cref="CommandLineArgument.Value"/> and <see cref="CommandLineArgument.UsedArgumentName"/> properties have been set.
        /// </para>
        /// </remarks>
        public event EventHandler<ArgumentParsedEventArgs> ArgumentParsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class using the specified arguments type, the default argument name prefixes,
        /// and the default case-insensitive argument name comparer.
        /// </summary>
        /// <param name="argumentsType">The <see cref="Type"/> of the class that defines the command line arguments.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="argumentsType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   The <see cref="CommandLineParser"/> cannot use <paramref name="argumentsType"/> as the command line arguments type, because it defines a required
        ///   postional argument after an optional positional argument, it defines a positional array argument that is not the last positional argument, it defines an argument with an invalid name,
        ///   it defines two arguments with the same name, or it has two properties with the same <see cref="CommandLineArgumentAttribute.Position"/> property value.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   This constructor uses the <see cref="StringComparer.OrdinalIgnoreCase"/> comparer for argument names.
        /// </para>
        /// </remarks>
        public CommandLineParser(Type argumentsType)
            : this(argumentsType, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class using the specified arguments type, the specified argument name prefixes,
        /// and the default case-insensitive argument name comparer.
        /// </summary>
        /// <param name="argumentsType">The <see cref="Type"/> of the class that defines the command line arguments.</param>
        /// <param name="argumentNamePrefixes">The prefixes that are used to indicate argument names on the command line, or <see langword="null"/> to use the default prefixes for the current platform.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="argumentsType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="argumentNamePrefixes"/> contains no elements or contains a <see langword="null"/> or empty string value.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   The <see cref="CommandLineParser"/> cannot use <paramref name="argumentsType"/> as the command line arguments type, because it defines a required
        ///   postional argument after an optional positional argument, it defines a positional array argument that is not the last positional argument, it defines an argument with an invalid name,
        ///   it defines two arguments with the same name, or it has two properties with the same <see cref="CommandLineArgumentAttribute.Position"/> property value.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   If you specify multiple argument name prefixes, the first one will be used when generating usage information using the <see cref="WriteUsage(TextWriter,int,WriteUsageOptions)"/> method.
        /// </para>
        /// <para>
        ///   This constructor uses the <see cref="StringComparer.OrdinalIgnoreCase"/> comparer for argument names.
        /// </para>
        /// </remarks>
        public CommandLineParser(Type argumentsType, IEnumerable<string> argumentNamePrefixes)
            : this(argumentsType, argumentNamePrefixes, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class using the specified arguments type, the specified argument name prefixes,
        /// and the specified <see cref="IComparer{T}"/> for comparing argument names.
        /// </summary>
        /// <param name="argumentsType">The <see cref="Type"/> of the class that defines the command line arguments.</param>
        /// <param name="argumentNamePrefixes">The prefixes that are used to indicate argument names on the command line, or <see langword="null"/> to use the default prefixes for the current platform.</param>
        /// <param name="argumentNameComparer">An <see cref="IComparer{T}"/> that is used to match the names of arguments, or <see langword="null"/> to use the default case-insensitive comparer.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="argumentsType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="argumentNamePrefixes"/> contains no elements or contains a <see langword="null"/> or empty string value.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///   The <see cref="CommandLineParser"/> cannot use <paramref name="argumentsType"/> as the command line arguments type, because it defines a required
        ///   postional argument after an optional positional argument, it defines a positional array argument that is not the last positional argument, it defines an argument with an invalid name,
        ///   it defines two arguments with the same name, or it has two properties with the same <see cref="CommandLineArgumentAttribute.Position"/> property value.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   If you specify multiple argument name prefixes, the first one will be used when generating usage information using the <see cref="WriteUsage(TextWriter,int,WriteUsageOptions)"/> method.
        /// </para>
        /// </remarks>
        public CommandLineParser(Type argumentsType, IEnumerable<string> argumentNamePrefixes, IComparer<string> argumentNameComparer)
        {
            if( argumentsType == null )
                throw new ArgumentNullException("argumentsType");

            _argumentNamePrefixes = DetermineArgumentNamePrefixes(argumentNamePrefixes);

            _argumentsByName = new SortedList<string, CommandLineArgument>(argumentNameComparer ?? StringComparer.OrdinalIgnoreCase);

            _argumentsType = argumentsType;
            _commandLineConstructor = GetCommandLineConstructor();

            DetermineConstructorArguments();
            _constructorArgumentCount = _arguments.Count; // Named positional arguments added by DeterminePropertyParameters are not constructor arguments.

            _positionalArgumentCount = _constructorArgumentCount + DeterminePropertyArguments();

            VerifyPositionalArgumentRules();

            AllowWhiteSpaceValueSeparator = true;
        }

        /// <summary>
        /// Gets the default prefix for the command line usage information.
        /// </summary>
        /// <value>
        /// A string consisting of the text "Usage: " followed by the file name of the application's entry point assembly.
        /// </value>
        public static string DefaultUsagePrefix
        {
            get { return string.Format(CultureInfo.CurrentCulture, Properties.Resources.DefaultUsagePrefixFormat, Path.GetFileName(Assembly.GetEntryAssembly().Location)); }
        }

        /// <summary>
        /// Gets the default argument name prefixes for the current platform.
        /// </summary>
        /// <value>
        /// A forward slash (/) and a dash (-) for Windows, or a dash (-) for all other platforms platforms.
        /// </value>
        public static IEnumerable<string> DefaultArgumentNamePrefixes
        {
            get { return DefaultArgumentNamePrefixesCore; }
        }

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
        /// </remarks>
        public ReadOnlyCollection<string> ArgumentNamePrefixes
        {
            get
            {
                return _argumentNamePrefixesReadOnlyWrapper ?? (_argumentNamePrefixesReadOnlyWrapper = new ReadOnlyCollection<string>(_argumentNamePrefixes));
            }
        }

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
                DescriptionAttribute description = (DescriptionAttribute)Attribute.GetCustomAttribute(_argumentsType, typeof(DescriptionAttribute));
                return description == null ? string.Empty : description.Description;
            }
        }

        /// <summary>
        /// Gets or sets the culture used to convert command line argument values from their string representation to the argument type.
        /// </summary>
        /// <value>
        /// The culture used to convert command line argument values from their string representation to the argument type. The default value
        /// is <see cref="CultureInfo.CurrentCulture"/>.
        /// </value>
        public CultureInfo Culture
        {
            get { return _culture ?? CultureInfo.CurrentCulture; }
            set { _culture = value; }
        }
        

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
        ///   <see langword="true"/> if white space is allowed to separate an argument name and its value; <see langword="false"/> if only the colon (:) is allowed.
        ///   The default value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If the <see cref="AllowWhiteSpaceValueSeparator"/> property is <see langword="true"/>, the value of an argument can be separated from its name either
        ///   by using a colon (:) or by using white space. Given a named argument named "sample", the command lines <c>-sample:value</c> and <c>-sample value</c>
        ///   are both valid and will assign the value "value" to the argument.
        /// </para>
        /// <para>
        ///   If the <see cref="AllowWhiteSpaceValueSeparator"/> property is <see langword="false"/>, only the colon (:) is allowed to separate the value from the name.
        ///   The command line <c>-sample:value</c> still assigns the value "value" to the argument, but for the command line "-sample value" the argument 
        ///   is considered not to have a value (which is only valid if <see cref="CommandLineArgument.IsSwitch"/> is <see langword="true"/>), and
        ///   "value" is considered to be the value for the next positional argument.
        /// </para>
        /// <para>
        ///   For switch arguments (<see cref="CommandLineArgument.IsSwitch"/> is <see langword="true"/>), only the colon (:) is allowed to
        ///   specify an explicit value regardless of the value of the <see cref="AllowWhiteSpaceValueSeparator"/> property. Given a switch argument named "switch" 
        ///   the command line <c>-switch false</c> is interpreted to mean that the value of "switch" is <see langword="true"/> and the value of the
        ///   next positional argument is "false", even if the <see cref="AllowWhiteSpaceValueSeparator"/> property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        public bool AllowWhiteSpaceValueSeparator { get; set; }

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

        // This property is used so we can assign it directly to _argumentNamePrefixes if we want the default values,
        // rather than getting an IEnumerable{T} from DefaultArgumentNamePrefixes.
        private static string[] DefaultArgumentNamePrefixesCore
        {
            get
            {
                // The Windows platforms are the first 4 values of the PlatformID enum, and WinCE is the last one.
                // We allocate a new array each time, because we don't want this to be changed.
                return Environment.OSVersion.Platform <= PlatformID.WinCE ? new[] { "-", "/" } : new[] { "-" };
            }
        }

        /// <summary>
        /// Writes command line usage help to the standard output stream using the default options.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The usage help consists of first the <see cref="Description"/>, followed by the usage syntax, followed by a description of all the arguments.
        ///   </para>
        ///   <para>
        ///     This method uses the default usage options, as specified by the default values of the <see cref="WriteUsageOptions"/> class.
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
        public void WriteUsageToConsole()
        {
            WriteUsage(Console.Out, Console.WindowWidth - 1);
        }
        
        /// <summary>
        /// Writes command line usage help to the standard output stream using the specified options.
        /// </summary>
        /// <param name="options">The options to use for formatting the usage.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
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
        public void WriteUsageToConsole(WriteUsageOptions options)
        {
            WriteUsage(Console.Out, Console.WindowWidth - 1, options);
        }
        
        /// <summary>
        /// Writes command line usage help to the specified <see cref="TextWriter"/> using the default options.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the usage to.</param>
        /// <param name="maximumLineLength">
        ///   The maximum line length of lines in the usage text; if <paramref name="writer"/> is an instance 
        ///   of <see cref="LineWrappingTextWriter"/>, this parameter is ignored. A value less than 1 or larger than 65536 is interpreted as infinite line length.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="writer"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     The usage help consists of first the <see cref="Description"/>, followed by the usage syntax, followed by a description of all the arguments.
        ///   </para>
        ///   <para>
        ///     This method uses the default usage options, as specified by the default values of the <see cref="WriteUsageOptions"/> class.
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
        public void WriteUsage(TextWriter writer, int maximumLineLength)
        {
            WriteUsage(writer, maximumLineLength, new WriteUsageOptions());
        }

        /// <summary>
        /// Writes command line usage help to the specified <see cref="TextWriter"/> using the specified options.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the usage to.</param>
        /// <param name="maximumLineLength">
        ///   The maximum line length of lines in the usage text; if <paramref name="writer"/> is an instance 
        ///   of <see cref="LineWrappingTextWriter"/>, this parameter is ignored. A value less than 1 or larger than 65536 is interpreted as infinite line length.
        /// </param>
        /// <param name="options">The options to use for formatting the usage.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="writer"/> or <paramref name="options"/> is <see langword="null"/>.
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
        public void WriteUsage(TextWriter writer, int maximumLineLength, WriteUsageOptions options)
        {
            if( writer == null )
                throw new ArgumentNullException("writer");
            if( options == null )
                throw new ArgumentNullException("options");

            bool disposeWriter = false;
            LineWrappingTextWriter lineWriter = null;
            try
            {
                lineWriter = writer as LineWrappingTextWriter;
                if( lineWriter == null )
                {
                    disposeWriter = true;
                    lineWriter = new LineWrappingTextWriter(writer, maximumLineLength, false);
                }

                if( options.IncludeApplicationDescription && !string.IsNullOrEmpty(Description) )
                {
                    lineWriter.WriteLine(Description);
                    lineWriter.WriteLine();
                }

                WriteUsageSyntax(lineWriter, options);

                WriteArgumentDescriptions(lineWriter, options);
            }
            finally
            {
                if( disposeWriter && lineWriter != null )
                    lineWriter.Dispose();
            }
        }

        /// <summary>
        /// Parses the specified command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>
        ///   An instance of the type specified by the <see cref="ArgumentsType"/> property, or <see langword="null"/> if argument
        ///   parsing was cancelled by the <see cref="ArgumentParsed"/> event handler.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="args"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="CommandLineArgumentException">
        ///   Too many positional arguments were supplied, a required argument was not supplied, an unknown argument name was supplied,
        ///   no value was supplied for a named argument, an argument was supplied more than once and <see cref="AllowDuplicateArguments"/>
        ///   is <see langword="false"/>, or one of the argument values could not be converted to the argument's type.
        /// </exception>
        public object Parse(string[] args)
        {
            return Parse(args, 0);
        }
        
        /// <summary>
        /// Parses the specified command line arguments, starting at the specified index.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="index">The index of the first argument to parse.</param>
        /// <returns>
        ///   An instance of the type specified by the <see cref="ArgumentsType"/> property, or <see langword="null"/> if argument
        ///   parsing was cancelled by the <see cref="ArgumentParsed"/> event handler.
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
        public object Parse(string[] args, int index)
        {
            if( args == null )
                throw new ArgumentNullException("args");
            if( index < 0 || index > args.Length )
                throw new ArgumentOutOfRangeException("index");

            // Reset all arguments to their default value.
            foreach( CommandLineArgument argument in _arguments )
                argument.Reset();
 
            int positionalArgumentIndex = 0;

            for( int x = index; x < args.Length; ++x )
            {
                string arg = args[x];
                string argumentNamePrefix = CheckArgumentNamePrefix(arg);
                if( argumentNamePrefix != null )
                {
                    // If white space was the value separator, this function returns the index of argument containing the value for the named argument.
                    // It returns -1 if parsing was cancelled by the ArgumentParsed event handler.
                    x = ParseNamedArgument(args, x, argumentNamePrefix);
                    if( x < 0 )
                        return null;
                }
                else
                {
                    // If this is an array argument is must be the last argument.
                    if( positionalArgumentIndex < _positionalArgumentCount && !_arguments[positionalArgumentIndex].IsMultiValue )
                    {
                        // Skip named positional arguments that have already been specified by name.
                        while( positionalArgumentIndex < _positionalArgumentCount && !_arguments[positionalArgumentIndex].IsMultiValue && _arguments[positionalArgumentIndex].HasValue )
                        {
                            ++positionalArgumentIndex;
                        }
                    }

                    if( positionalArgumentIndex >= _positionalArgumentCount )
                        throw new CommandLineArgumentException(Properties.Resources.TooManyArguments, CommandLineArgumentErrorCategory.TooManyArguments);

                    // ParseArgumentValue returns true if parsing was cancelled by the ArgumentParsed event handler.
                    if( ParseArgumentValue(_arguments[positionalArgumentIndex], arg) )
                        return null;
                }
            }

            // Check required arguments and convert array arguments. This is done in usage order so the first missing positional argument is reported, rather
            // than the missing argument that is first alphabetically.
            foreach( CommandLineArgument argument in _arguments )
            {
                if( argument.IsRequired && !argument.HasValue )
                    throw new CommandLineArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.MissingRequiredArgumentFormat, argument.ArgumentName), argument.ArgumentName, CommandLineArgumentErrorCategory.MissingRequiredArgument);
            }

            object[] constructorArgumentValues = new object[_constructorArgumentCount];
            for( int x = 0; x < _constructorArgumentCount; ++x )
                constructorArgumentValues[x] = _arguments[x].Value;

            
            object commandLineArguments = CreateArgumentsTypeInstance(constructorArgumentValues);
            foreach( CommandLineArgument argument in _arguments )
            {
                // Apply property argument values (this does nothing for constructor arguments).
                argument.ApplyPropertyValue(commandLineArguments);
            }
            return commandLineArguments;
        }

        /// <summary>
        /// Raises the <see cref="ArgumentParsed"/> event.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected virtual void OnArgumentParsed(ArgumentParsedEventArgs e)
        {
            EventHandler<ArgumentParsedEventArgs> handler = ArgumentParsed;
            if( handler != null )
                handler(this, e);
        }

        private static string[] DetermineArgumentNamePrefixes(IEnumerable<string> namedArgumentPrefixes)
        {
            if( namedArgumentPrefixes == null )
                return DefaultArgumentNamePrefixesCore;
            else
            {
                List<string> result = new List<string>(namedArgumentPrefixes);
                if( result.Count == 0 )
                    throw new ArgumentException(Properties.Resources.EmptyArgumentNamePrefixes, "namedArgumentPrefixes");
                foreach( string prefix in result )
                {
                    if( string.IsNullOrEmpty(prefix) )
                        throw new ArgumentException(Properties.Resources.EmptyArgumentNamePrefix, "namedArgumentPrefixes");
                }
                return result.ToArray();
            }
        }

        private void DetermineConstructorArguments()
        {
            ParameterInfo[] parameters = _commandLineConstructor.GetParameters();
            foreach( ParameterInfo parameter in parameters )
            {
                CommandLineArgument argument = CommandLineArgument.Create(this, parameter);
                AddNamedArgument(argument);
            }
        }

        private int DeterminePropertyArguments()
        {
            int additionalPositionalArgumentCount = 0;

            PropertyInfo[] properties = _argumentsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach( PropertyInfo prop in properties )
            {
                if( Attribute.IsDefined(prop, typeof(CommandLineArgumentAttribute)) )
                {
                    CommandLineArgument argument = CommandLineArgument.Create(this, prop);
                    AddNamedArgument(argument);
                    if( argument.Position != null )
                    {
                        ++additionalPositionalArgumentCount;
                    }
                }
            }

            if( _arguments.Count > _constructorArgumentCount )
            {
                // Sort the added arguments in usage order (positional first, then required non-positional arguments, then the rest by name
                _arguments.Sort(_constructorArgumentCount, _arguments.Count - _constructorArgumentCount, new CommandLineArgumentComparer(_argumentsByName.Comparer));
            }

            return additionalPositionalArgumentCount;
        }

        private void AddNamedArgument(CommandLineArgument argument)
        {
            if( argument.ArgumentName.IndexOf(NameValueSeparator) >= 0 )
                throw new NotSupportedException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.ArgumentNameContainsSeparatorFormat, argument.ArgumentName));
            _argumentsByName.Add(argument.ArgumentName, argument);
            if( argument.Aliases != null )
            {
                foreach( string alias in argument.Aliases )
                    _argumentsByName.Add(alias, argument);
            }
            _arguments.Add(argument);
        }

        private void VerifyPositionalArgumentRules()
        {
            bool hasOptionalArgument = false;
            bool hasArrayArgument = false;

            for( int x = 0; x < _positionalArgumentCount; ++x )
            {
                CommandLineArgument argument = _arguments[x];

                if( hasArrayArgument )
                    throw new NotSupportedException(Properties.Resources.ArrayNotLastArgument);
                if( argument.IsRequired && hasOptionalArgument )
                    throw new NotSupportedException(Properties.Resources.InvalidOptionalArgumentOrder);

                if( !argument.IsRequired )
                    hasOptionalArgument = true;

                if( argument.IsMultiValue )
                    hasArrayArgument = true;

                argument.Position = x;
            }
        }

        private bool ParseArgumentValue(CommandLineArgument argument, string value)
        {
            argument.SetValue(Culture, value);

            ArgumentParsedEventArgs e = new ArgumentParsedEventArgs(argument);
            OnArgumentParsed(e);
            return e.Cancel;
        }

        private int ParseNamedArgument(string[] args, int index, string prefix)
        {
            string argumentName = null;
            string argumentValue = null;

            string arg = args[index];
            // Extract the argument name
            // We don't use Split because if there's more than one colon we want to ignore the others.
            int colonIndex = arg.IndexOf(NameValueSeparator);
            if( colonIndex >= 0 )
            {
                argumentName = arg.Substring(prefix.Length, colonIndex - prefix.Length);
                argumentValue = arg.Substring(colonIndex + 1);
            }
            else
                argumentName = arg.Substring(prefix.Length);

            CommandLineArgument argument;
            if( _argumentsByName.TryGetValue(argumentName, out argument) )
            {
                if( argumentValue == null && !argument.IsSwitch && AllowWhiteSpaceValueSeparator && ++index < args.Length && CheckArgumentNamePrefix(args[index]) == null )
                {
                    // No separator was present but a value is required. We take the next argument as its value.
                    argumentValue = args[index];
                }
                // ParseArgumentValue returns true if parsing was cancelled by the ArgumentParsed event handler.
                argument.UsedArgumentName = argumentName;
                return ParseArgumentValue(argument, argumentValue) ? -1 : index;
            }
            else
                throw new CommandLineArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.UnknownArgumentFormat, argumentName), argumentName, CommandLineArgumentErrorCategory.UnknownArgument);
        }

        private string CheckArgumentNamePrefix(string argument)
        {
            // Even if '-' is the argument name prefix, we consider an argument starting with dash followed by a digit as a value, because it could be a negative number.
            if( argument.Length >= 2 && argument[0] == '-' && char.IsDigit(argument, 1) )
                return null;

            foreach( string namedArgumentPrefix in _argumentNamePrefixes )
            {
                if( argument.StartsWith(namedArgumentPrefix, StringComparison.Ordinal) )
                    return namedArgumentPrefix;
            }
            return null;
        }
        
        private ConstructorInfo GetCommandLineConstructor()
        {
            ConstructorInfo[] ctors = _argumentsType.GetConstructors();
            ConstructorInfo ctor = null;
            if( ctors.Length < 1 )
                throw new NotSupportedException(Properties.Resources.NoConstructor);
            else if( ctors.Length > 1 )
            {
                foreach( ConstructorInfo c in ctors )
                {
                    if( Attribute.IsDefined(c, typeof(CommandLineConstructorAttribute)) )
                    {
                        if( ctor == null )
                            ctor = c;
                        else
                            throw new NotSupportedException(Properties.Resources.MultipleMarkedConstructors);
                    }
                }

                if( ctor == null )
                    throw new NotSupportedException(Properties.Resources.NoMarkedConstructor);
            }
            else // ctors.Length == 1
                ctor = ctors[0];
            return ctor;
        }

        private void WriteArgumentDescriptions(LineWrappingTextWriter writer, WriteUsageOptions options)
        {
            writer.ResetIndent();
            writer.Indent = writer.MaximumLineLength < MaximumLineWidthForIndent ? 0 : options.ArgumentDescriptionIndent;

            foreach( CommandLineArgument argument in _arguments )
            {
                // Omit arguments that don't have a description.
                if( !string.IsNullOrEmpty(argument.Description) )
                {
                    writer.ResetIndent();
                    string valueDescription = string.Format(CultureInfo.CurrentCulture, options.ValueDescriptionFormat, argument.ValueDescription);
                    if( argument.IsSwitch )
                        valueDescription = string.Format(CultureInfo.CurrentCulture, options.OptionalArgumentFormat, valueDescription);
                    string defaultValue = options.IncludeDefaultValueInDescription && argument.DefaultValue != null ? string.Format(Culture, options.DefaultValueFormat, argument.DefaultValue) : string.Empty;
                    string alias = FormatAliasesForDescription(options, argument);
                    writer.WriteLine(options.ArgumentDescriptionFormat, argument.ArgumentName, argument.Description, valueDescription, _argumentNamePrefixes[0], defaultValue, alias);
                }
            }
        }

        private string FormatAliasesForDescription(WriteUsageOptions options, CommandLineArgument argument)
        {
            if( !options.IncludeAliasInDescription || argument.Aliases == null || argument.Aliases.Count == 0 )
                return string.Empty;
            else
            {
                StringBuilder result = new StringBuilder();
                foreach( string alias in argument.Aliases )
                {
                    if( result.Length > 0 )
                        result.Append(", ");
                    result.Append(_argumentNamePrefixes[0]);
                    result.Append(alias);
                }
                return string.Format(Culture, argument.Aliases.Count == 1 ? options.AliasFormat : options.AliasesFormat, result);
            }
        }

        private void WriteUsageSyntax(LineWrappingTextWriter writer, WriteUsageOptions options)
        {
            writer.ResetIndent();
            writer.Indent = writer.MaximumLineLength < MaximumLineWidthForIndent ? 0 : options.Indent;

            writer.Write(options.UsagePrefix);

            foreach( CommandLineArgument argument in _arguments )
            {
                writer.Write(" ");
                writer.Write(argument.ToString(options));
            }

            writer.WriteLine(); // End syntax line
            writer.WriteLine(); // Blank line
        }

        private object CreateArgumentsTypeInstance(object[] constructorArgumentValues)
        {
            try
            {
                return _commandLineConstructor.Invoke(constructorArgumentValues);
            }
            catch( TargetInvocationException ex )
            {
                throw new CommandLineArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.CreateArgumentsTypeErrorFormat, ex.InnerException.Message), CommandLineArgumentErrorCategory.CreateArgumentsTypeError, ex.InnerException);
            }
        }
    }
}
