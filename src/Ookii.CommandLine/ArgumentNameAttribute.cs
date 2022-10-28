// Copyright (c) Sven Groot (Ookii.org)
using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Indicates an alternative argument name for an argument defined by a constructor parameter.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Apply the <see cref="ArgumentNameAttribute"/> attribute to a constructor parameter to indicate
    ///   that the name of the argument should be different than the parameter name, or to specify
    ///   a short name if the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>.
    /// </para>
    /// <para>
    ///   If no argument name is specified, the parameter name will be used, applying the
    ///   <see cref="NameTransform"/> that is being used.
    /// </para>
    /// <para>
    ///   The <see cref="NameTransform"/> will not be applied to explicitly specified names.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ArgumentNameAttribute : Attribute
    {
        private readonly string? _argumentName;
        private bool _short;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentNameAttribute"/> class.
        /// </summary>
        /// <param name="argumentName">
        ///   The name of the argument, or <see langword="null"/> to indicate the parameter name
        ///   should be used (applying the <see cref="NameTransform"/> that is being used).
        /// </param>
        /// <remarks>
        /// <para>
        ///   The <see cref="NameTransform"/> will not be applied to explicitly specified names.
        /// </para>
        /// </remarks>
        public ArgumentNameAttribute(string? argumentName = null)
        {
            _argumentName = argumentName;
        }

        /// <summary>
        /// Gets the name of the argument.
        /// </summary>
        /// <value>
        /// The name of the argument.
        /// </value>
        public string? ArgumentName => _argumentName;

        /// <summary>
        /// Gets or sets a value that indicates whether the argument has a long name.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the argument has a long name; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <note>
        ///   This property is ignored if <see cref="CommandLineParser.Mode"/> is not
        ///   <see cref="ParsingMode.LongShort"/>.
        /// </note>
        /// </remarks>
        public bool Long { get; set; } = true;

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
        ///   If <see cref="ShortName"/> is not set but this property is set to <see langword="true"/>,
        ///   the short name will be derived using the first character of the long name.
        /// </para>
        /// </remarks>
        public bool Short
        {
            get => _short || ShortName != '\0';
            set => _short = value;
        }
        /// <summary>
        /// Gets or sets the argument's short name.
        /// </summary>
        /// <value>The short name, or a null character ('\0') if the argument has no short name.</value>
        /// <remarks>
        /// <para>
        ///   This property is ignored if <see cref="CommandLineParser.Mode"/> is not
        ///   <see cref="ParsingMode.LongShort"/>.
        /// </para>
        /// </remarks>
        public char ShortName { get; set; }
    }
}
