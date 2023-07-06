using Ookii.CommandLine.Commands;
using System;

namespace Ookii.CommandLine;

/// <summary>
/// Indicates a property or method of a class defines a command line argument.
/// </summary>
/// <remarks>
/// <para>
///   If this attribute is applied to a property, the property's type determines the argument
///   type, and the property will be set with either the set value or the default value after
///   parsing is complete.
/// </para>
/// <para>
///   If an argument was not provided on the command line, and the default value is <see langword="null"/>,
///   the property will not be set and will remain at its initial value.
/// </para>
/// <para>
///   If this attribute is applied to a method, that method must have one of the following
///   signatures:
/// </para>
/// <code>
/// public static (void|bool|CancelMode) Method(ArgumentType value, CommandLineParser parser);
/// public static (void|bool|CancelMode) Method(ArgumentType value);
/// public static (void|bool|CancelMode) Method(CommandLineParser parser);
/// public static (void|bool|CancelMode) Method();
/// </code>
/// <para>
///   In this case, the <c>ArgumentType</c> type determines the type of values the argument accepts. If there
///   is no <c>value</c> parameter, the argument will be a switch argument, and the method will
///   be invoked if the switch is present, even if it was explicitly set to <see langword="false"/>.
/// </para>
/// <para>
///   The method will be invoked as soon as the argument is parsed, before parsing the entire
///   command line is complete.
/// </para>
/// <para>
///   The return type must be either <see cref="void"/>, <see cref="bool"/> or <see cref="CancelMode"/>.
///   Using <see cref="void"/> is equivalent to returning <see cref="CancelMode.None" qualifyHint="true"/>, and when
///   using <see cref="bool"/>, returning <see langword="false"/> is equivalent to returning
///   <see cref="CancelMode.Abort" qualifyHint="true"/>.
/// </para>
/// <para>
///   Unlike using the <see cref="CancelParsing"/> property, canceling parsing with the return
///   value does not automatically print the usage help when using the
///   <see cref="CommandLineParser{T}.ParseWithErrorHandling()" qualifyHint="true"/> method, the
///   <see cref="CommandLineParser.Parse{T}(string[], ParseOptions?)" qualifyHint="true"/> method or the
///   <see cref="CommandManager"/> class. Instead, it must be requested using by setting the
///   <see cref="CommandLineParser.HelpRequested" qualifyHint="true"/> property to <see langword="true"/> in the
///   target method.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
/// <seealso cref="CommandLineParser"/>
/// <seealso cref="CommandLineArgument"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public sealed class CommandLineArgumentAttribute : Attribute
{
    private readonly string? _argumentName;
    private bool _short;
    private bool _isPositional;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentAttribute"/> class using the specified argument name.
    /// </summary>
    /// <param name="argumentName">
    ///   The name of the argument, or <see langword="null"/> to indicate the member name
    ///   should be used, applying the <see cref="NameTransform"/> specified by the
    ///   <see cref="ParseOptions.ArgumentNameTransform" qualifyHint="true"/> property or the <see cref="ParseOptionsAttribute.ArgumentNameTransform" qualifyHint="true"/>
    ///   property.
    /// </param>
    /// <para>
    ///   If the <see cref="CommandLineParser.Mode" qualifyHint="true"/> property is <see cref="ParsingMode.LongShort" qualifyHint="true"/>,
    ///   the <paramref name="argumentName"/> parameter is the long name of the argument.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandLineParser.Mode" qualifyHint="true"/> property is <see cref="ParsingMode.LongShort" qualifyHint="true"/>
    ///   and the <see cref="IsLong"/> property is <see langword="false"/>, the <paramref name="argumentName"/>
    ///   parameter will not be used.
    /// </para>
    /// <remarks>
    /// <para>
    ///   The <see cref="NameTransform"/> will not be applied to explicitly specified names.
    /// </para>
    /// </remarks>
    public CommandLineArgumentAttribute(string? argumentName = null)
    {
        _argumentName = argumentName;
    }

    /// <summary>
    /// Gets the name of the argument.
    /// </summary>
    /// <value>
    /// The name that can be used to supply the argument, or <see langword="null"/> if the
    /// member name should be used.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="CommandLineParser.Mode" qualifyHint="true"/> property is <see cref="ParsingMode.LongShort" qualifyHint="true"/>,
    ///   this is the long name of the argument.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandLineParser.Mode" qualifyHint="true"/> property is <see cref="ParsingMode.LongShort" qualifyHint="true"/>
    ///   and the <see cref="IsLong"/> property is <see langword="false"/>, the <see cref="ArgumentName"/>
    ///   property is ignored.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.ArgumentName" qualifyHint="true"/>
    public string? ArgumentName
    {
        get { return _argumentName; }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the argument has a long name.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the argument has a long name; otherwise, <see langword="false"/>.
    /// The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property is ignored if <see cref="CommandLineParser.Mode" qualifyHint="true"/> is not
    ///   <see cref="ParsingMode.LongShort" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandLineParser.Mode" qualifyHint="true"/> property is <see cref="ParsingMode.LongShort" qualifyHint="true"/>
    ///   and the <see cref="IsLong"/> property is <see langword="false"/>, the <see cref="ArgumentName"/>
    ///   property is ignored.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.HasLongName" qualifyHint="true"/>
    public bool IsLong { get; set; } = true;

    /// <summary>
    /// Gets or sets a value that indicates whether the argument has a short name.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the argument has a short name; otherwise, <see langword="false"/>.
    /// The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <note>
    ///   This property is ignored if the <see cref="CommandLineParser.Mode" qualifyHint="true"/>
    ///   property is not <see cref="ParsingMode.LongShort" qualifyHint="true"/>.
    /// </note>
    /// <para>
    ///   If the <see cref="ShortName"/> property is not set but this property is set to <see langword="true"/>,
    ///   the short name will be derived using the first character of the long name.
    /// </para>
    /// <para>
    ///   If the <see cref="ShortName"/> property is set to a value other than the null character,
    ///   this property will always return <see langword="true"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.HasShortName" qualifyHint="true"/>
    public bool IsShort
    {
        get => _short || ShortName != '\0';
        set => _short = value;
    }

    /// <summary>
    /// Gets or sets the argument's short name.
    /// </summary>
    /// <value>The short name, or a null character ('\0') if the argument has no short name.</value>
    /// <remarks>
    /// <note>
    /// This property is ignored if the <see cref="CommandLineParser.Mode" qualifyHint="true"/>
    /// property is not <see cref="ParsingMode.LongShort" qualifyHint="true"/>.
    /// </note>
    /// <para>
    ///   Setting this property implies the <see cref="IsShort"/> property is <see langword="true"/>.
    /// </para>
    /// <para>
    ///   To derive the short name from the first character of the long name, set the
    ///   <see cref="IsShort"/> property to <see langword="true"/> without setting the
    ///   <see cref="ShortName"/> property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.ShortName" qualifyHint="true"/>
    public char ShortName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the argument is required.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if the argument must be supplied on the command line; otherwise, <see langword="false"/>.
    ///   The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="CommandLineArgumentAttribute"/> attribute is used on a property with
    ///   the C# <c>required</c> keyword, the argument will always be required, and the value of
    ///   this property is ignored.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.IsRequired" qualifyHint="true"/>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the relative position of a positional argument.
    /// </summary>
    /// <value>
    /// The position of the argument, or a negative value if the argument can only be specified by name. The default value is -1.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The <see cref="Position"/> property specifies the relative position of the positional
    ///   arguments created by properties. The actual numbers are not important, only their
    ///   order is. For example, if you have two positional arguments with positions set to
    ///   4 and 7, and no other positional arguments, they will be the first and second
    ///   positional arguments, not the forth and seventh. It is an error to use the same number
    ///   more than once.
    /// </para>
    /// <para>
    ///   When using the <see cref="GeneratedParserAttribute"/>, you can also set the <see cref="IsPositional"/>
    ///   property to <see langword="true"/>, without setting the <see cref="Position"/> property,
    ///   to order the positional arguments using the order of the members that define them.
    /// </para>
    /// <para>
    ///   If you set the <see cref="Position"/> property to a non-negative value, it is not
    ///   necessary to set the <see cref="IsPositional"/> property.
    /// </para>
    /// <para>
    ///   The <see cref="CommandLineArgument.Position" qualifyHint="true"/> property will be set to reflect the actual position of the argument,
    ///   which may not match the value of this property.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.Position" qualifyHint="true"/>
    public int Position { get; set; } = -1;

    /// <summary>
    /// Gets or sets a value that indicates that an argument is positional.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the argument is positional; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="Position"/> property is set to a non-negative value, this property
    ///   always returns <see langword="true"/>.
    /// </para>
    /// <para>
    ///   When using the <see cref="GeneratedParserAttribute"/> attribute, you can set the
    ///   <see cref="IsPositional"/> property to <see langword="true"/> without setting the
    ///   <see cref="Position"/> property, to order positional arguments using the order of the
    ///   members that define them.
    /// </para>
    /// <para>
    ///   Doing this is not supported without the <see cref="GeneratedParserAttribute"/> attribute,
    ///   because reflection is not guaranteed to return class members in any particular order. The
    ///   <see cref="CommandLineParser"/> class will throw an exception if the <see cref="IsPositional"/>
    ///   property is <see langword="true"/> without a non-negative <see cref="Position"/> property
    ///   value if reflection is used.
    /// </para>
    /// </remarks>
    public bool IsPositional
    {
        get => _isPositional || Position >= 0;
        set => _isPositional = value;
    }

    /// <summary>
    /// Gets or sets the default value to be assigned to the property if the argument is not supplied on the command line.
    /// </summary>
    /// <value>
    /// The default value for the argument, or <see langword="null"/> to not set the property
    /// if the argument is not supplied. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The <see cref="DefaultValue"/> property will not be used if the <see cref="IsRequired"/> property is <see langword="true"/>,
    ///   or if the argument is a multi-value or dictionary argument, or if the <see cref="CommandLineArgumentAttribute"/>
    ///   attribute was applied to a method.
    /// </para>
    /// <para>
    ///   By default, the command line usage help generated by <see cref="CommandLineParser.WriteUsage" qualifyHint="true"/>
    ///   includes the default value. To change that, set the <see cref="IncludeDefaultInUsageHelp"/>
    ///   property to <see langword="false"/>, or to change it for all arguments set the
    ///   <see cref="UsageWriter.IncludeDefaultValueInDescription" qualifyHint="true"/> property to
    ///   <see langword="false"/>.
    /// </para>
    /// <para>
    ///   The default value can also be set by using a property initializer. When using the
    ///   <see cref="GeneratedParserAttribute"/> attribute, a default value set using a property
    ///   initializer will also be shown in the usage help, as long as it's a literal, enumeration
    ///   value, or constant. Without the attribute, only default values set with the
    ///   <see cref="DefaultValue"/> property are shown in the usage help.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.DefaultValue" qualifyHint="true"/>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the argument's default value should be shown
    /// in the usage help.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to show the default value in the usage help; otherwise,
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The default value can be set using the <see cref="DefaultValue"/> property, or, when
    ///   using source generation with the <see cref="GeneratedParserAttribute"/> attribute, using a
    ///   property initializer.
    /// </para>
    /// <para>
    ///   This property is ignored if the <see cref="UsageWriter.IncludeDefaultValueInDescription" qualifyHint="true"/>
    ///   property is <see langword="false"/>.
    /// </para>
    /// </remarks>
    public bool IncludeDefaultInUsageHelp { get; set; } = true;

    /// <summary>
    /// Gets or sets a value that indicates whether argument parsing should be canceled if
    /// this argument is encountered.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="CancelMode"/> enumeration.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If this property is not <see cref="CancelMode.None" qualifyHint="true"/>, the <see cref="CommandLineParser"/>
    ///   will stop parsing the command line arguments after seeing this argument. The result of
    ///   the operation will be <see langword="null"/> if this property is <see cref="CancelMode.Abort" qualifyHint="true"/>,
    ///   or an instance of the arguments class with the results up to this point if this property
    ///   is <see cref="CancelMode.Success" qualifyHint="true"/>. In the latter case, the <see cref="ParseResult.RemainingArguments" qualifyHint="true"/>
    ///   property will contain all arguments that were not parsed.
    /// </para>
    /// <para>
    ///   If <see cref="CancelMode.Success" qualifyHint="true"/> is used, all required arguments must have a value at
    ///   the point this argument is encountered, otherwise a <see cref="CommandLineArgumentException"/>
    ///   is thrown.
    /// </para>
    /// <para>
    ///   Use the <see cref="ParseResult.ArgumentName" qualifyHint="true"/> property to determine which argument caused
    ///   cancellation.
    /// </para>
    /// <para>
    ///   If this property is <see cref="CancelMode.Abort" qualifyHint="true"/>, the <see cref="CommandLineParser.HelpRequested" qualifyHint="true"/>
    ///   property will be automatically set to <see langword="true"/> when parsing is canceled.
    /// </para>
    /// <para>
    ///   It's possible to prevent cancellation when an argument has this property set by
    ///   handling the <see cref="CommandLineParser.ArgumentParsed" qualifyHint="true"/> event and setting the
    ///   <see cref="ArgumentParsedEventArgs.CancelParsing" qualifyHint="true"/> property to <see cref="CancelMode.None" qualifyHint="true"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.CancelParsing" qualifyHint="true"/>
    public CancelMode CancelParsing { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the argument is hidden from the usage help.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the argument is hidden from the usage help; otherwise,
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   A hidden argument will not be included in the usage syntax or the argument description
    ///   list, even if <see cref="DescriptionListFilterMode.All" qualifyHint="true"/> is used.
    /// </para>
    /// <para>
    ///   This property is ignored for positional or required arguments, which may not be
    ///   hidden.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.IsHidden" qualifyHint="true"/>
    public bool IsHidden { get; set; }
}
