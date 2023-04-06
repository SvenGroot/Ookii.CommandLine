// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Commands;
using System;
using System.ComponentModel;

namespace Ookii.CommandLine
{
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
    /// public static bool Method(ArgumentType value, CommandLineParser parser);
    /// public static bool Method(ArgumentType value);
    /// public static bool Method(CommandLineParser parser);
    /// public static bool Method();
    /// public static void Method(ArgumentType value, CommandLineParser parser);
    /// public static void Method(ArgumentType value);
    /// public static void Method(CommandLineParser parser);
    /// public static void Method();
    /// </code>
    /// <para>
    ///   In this case, the <c>ArgumentType</c> type determines the type of values the argument accepts. If there
    ///   is no <c>value</c> parameter, the argument will be a switch argument, and the method will
    ///   be invoked if the switch is present, even if it was explicitly set to <see langword="false"/>.
    /// </para>
    /// <para>
    ///   The method will be invoked as soon as the argument is parsed, before parsing the entire
    ///   command line is complete. Return <see langword="false"/> to cancel parsing, in which case
    ///   the remaining arguments will not be parsed and the <see cref="CommandLineParser.Parse(string[], int)"/>
    ///   method returns <see langword="null"/>.
    /// </para>
    /// <para>
    ///   Unlike using the <see cref="CancelParsing"/> or <see cref="CommandLineParser.ArgumentParsed"/>
    ///   event, canceling parsing with the return value does not automatically print the usage
    ///   help when using the <see cref="CommandLineParser{T}.ParseWithErrorHandling()"/> method, the
    ///   <see cref="CommandLineParser.Parse{T}(string[], int, ParseOptions?)"/> method or the
    ///   <see cref="CommandManager"/> class. Instead, it must be requested using by setting the
    ///   <see cref="CommandLineParser.HelpRequested"/> property to <see langword="true"/>.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <seealso cref="CommandLineParser"/>
    /// <seealso cref="CommandLineArgument"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class CommandLineArgumentAttribute : Attribute
    {
        private readonly string? _argumentName;
        private bool _short;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineArgumentAttribute"/> class using the specified argument name.
        /// </summary>
        /// <param name="argumentName">
        ///   The name of the argument, or <see langword="null"/> to indicate the member name
        ///   should be used, applying the <see cref="NameTransform"/> specified by the
        ///   <see cref="ParseOptions.ArgumentNameTransform"/> property or the <see cref="ParseOptionsAttribute.ArgumentNameTransform"/>
        ///   property.
        /// </param>
        /// <para>
        ///   If the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>,
        ///   the <paramref name="argumentName"/> parameter is the long name of the argument.
        /// </para>
        /// <para>
        ///   If the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>
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
        ///   If the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>,
        ///   this is the long name of the argument.
        /// </para>
        /// <para>
        ///   If the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>
        ///   and the <see cref="IsLong"/> property is <see langword="false"/>, the <see cref="ArgumentName"/>
        ///   property is ignored.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineArgument.ArgumentName"/>
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
        ///   This property is ignored if <see cref="CommandLineParser.Mode"/> is not
        ///   <see cref="ParsingMode.LongShort"/>.
        /// </para>
        /// <para>
        ///   If the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>
        ///   and the <see cref="IsLong"/> property is <see langword="false"/>, the <see cref="ArgumentName"/>
        ///   property is ignored.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineArgument.HasLongName"/>
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
        ///   This property is ignored if <see cref="CommandLineParser.Mode"/> is not
        ///   <see cref="ParsingMode.LongShort"/>.
        /// </note>
        /// <para>
        ///   If the <see cref="ShortName"/> property is not set but this property is set to <see langword="true"/>,
        ///   the short name will be derived using the first character of the long name.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineArgument.HasShortName"/>
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
        ///   This property is ignored if <see cref="CommandLineParser.Mode"/> is not
        ///   <see cref="ParsingMode.LongShort"/>.
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
        /// <see cref="CommandLineArgument.ShortName"/>
        public char ShortName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the argument is required.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the argument must be supplied on the command line; otherwise, <see langword="false"/>.
        ///   The default value is <see langword="false"/>.
        /// </value>
        /// <see cref="CommandLineArgument.IsRequired"/>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the position of a positional argument.
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
        ///   If you have arguments defined by the type's constructor parameters, positional arguments defined by properties will
        ///   always come after them; for example, if you have two constructor parameter arguments and one property positional argument with
        ///   position 0, then that argument will actually be the third positional argument.
        /// </para>
        /// <para>
        ///   The <see cref="CommandLineArgument.Position"/> property will be set to reflect the actual position of the argument,
        ///   which may not match the value of the <see cref="Position"/> property.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineArgument.Position"/>
        public int Position { get; set; } = -1;

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
        ///   By default, the command line usage help generated by <see cref="CommandLineParser.WriteUsage"/>
        ///   does not include the default value. Either manually add it to the description, or set the <see cref="UsageWriter.IncludeDefaultValueInDescription"/>
        ///   property to <see langword="true"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineArgument.DefaultValue"/>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a short description of the property's value to use when printing usage information.
        /// </summary>
        /// <value>
        /// The description of the value, or <see langword="null"/> to indicate that the property's
        /// type name should be used, applying the <see cref="NameTransform"/> specified by the
        /// <see cref="ParseOptions.ValueDescriptionTransform"/> or <see cref="ParseOptionsAttribute.ValueDescriptionTransform"/>
        /// property.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The value description is a short, typically one-word description that indicates the
        ///   type of value that the user should supply.
        /// </para>
        /// <para>
        ///   If not specified here, it is retrieved from the <see cref="ParseOptions.DefaultValueDescriptions"/>
        ///   property, and if not found there, the type of the property is used, applying the
        ///   <see cref="NameTransform"/> specified by the <see cref="ParseOptions.ValueDescriptionTransform"/>
        ///   property or the <see cref="ParseOptionsAttribute.ValueDescriptionTransform"/> property.
        ///   If this is a multi-value argument, the element type is used. If the type is <see cref="Nullable{T}"/>,
        ///   its underlying type is used.
        /// </para>
        /// <para>
        ///   If you want to override the value description for all arguments of a specific type, 
        ///   use the <see cref="ParseOptions.DefaultValueDescriptions"/> property.
        /// </para>
        /// <para>
        ///   The value description is used only when generating usage help. For example, the usage for an argument named Sample with
        ///   a value description of String would look like "-Sample &lt;String&gt;".
        /// </para>
        /// <note>
        ///   This is not the long description used to describe the purpose of the argument. That can be set
        ///   using the <see cref="DescriptionAttribute"/> attribute.
        /// </note>
        /// </remarks>
        /// <seealso cref="CommandLineArgument.ValueDescription"/>
        public string? ValueDescription { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether argument parsing should be canceled if
        /// this argument is encountered.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if argument parsing should be canceled after this argument;
        /// otherwise, <see langword="false"/>. The default value is <see langword="false" />.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is <see langword="true"/>, the <see cref="CommandLineParser"/> will
        ///   stop parsing the command line arguments after seeing this argument, and return
        ///   <see langword="null"/> from the <see cref="CommandLineParser.Parse(string[], int)"/> method
        ///   or one of its overloads. Since no instance of the arguments type is returned, it's
        ///   not possible to determine argument values, or which argument caused the cancellation,
        ///   except by inspecting the <see cref="CommandLineParser.Arguments"/> property.
        /// </para>
        /// <para>
        ///   This property is most commonly useful to implement a "-Help" or "-?" style switch
        ///   argument, where the presence of that argument causes usage help to be printed and
        ///   the program to exit, regardless of whether the rest of the command line is valid
        ///   or not.
        /// </para>
        /// <para>
        ///   The <see cref="CommandLineParser{T}.ParseWithErrorHandling()"/> method and the
        ///   <see cref="CommandLineParser.Parse{T}(string[], ParseOptions?)"/> static helper method
        ///   will print usage information if parsing was canceled through this method.
        /// </para>
        /// <para>
        ///   Canceling parsing in this way is identical to handling the <see cref="CommandLineParser.ArgumentParsed"/>
        ///   event and setting <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to
        ///   <see langword="true" />.
        /// </para>
        /// <para>
        ///   It's possible to prevent cancellation when an argument has this property set by
        ///   handling the <see cref="CommandLineParser.ArgumentParsed"/> event and setting the
        ///   <see cref="ArgumentParsedEventArgs.OverrideCancelParsing"/> property to 
        ///   <see langword="true"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineArgument.CancelParsing"/>
        public bool CancelParsing { get; set; }

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
        ///   list, even if <see cref="DescriptionListFilterMode.All"/> is used.
        /// </para>
        /// <para>
        ///   This property is ignored for positional or required arguments, which may not be
        ///   hidden.
        /// </para>
        /// </remarks>
        /// <seealso cref="CommandLineArgument.IsHidden"/>
        public bool IsHidden { get; set; }
    }
}
