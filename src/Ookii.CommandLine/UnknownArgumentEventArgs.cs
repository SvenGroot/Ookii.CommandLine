using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Ookii.CommandLine;

/// <summary>
/// Provides data for the <see cref="CommandLineParser.UnknownArgument" qualifyHint="true"/> event.
/// </summary>
/// <threadsafety static="true" instance="false"/>
public class UnknownArgumentEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownArgumentEventArgs"/> class.
    /// </summary>
    /// <param name="token">The argument token that contains the unknown argument.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="value">The argument value.</param>
    /// <param name="isCombinedSwithToken">
    /// Indicates whether the argument is part of a combined short switch argument.
    /// </param>
    /// <param name="possibleMatches">
    /// A list of possible arguments that this argument could match by prefix, or
    /// <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="token"/> is <see langword="null"/>.
    /// </exception>
    public UnknownArgumentEventArgs(string token, ReadOnlyMemory<char> name, ReadOnlyMemory<char> value,
        bool isCombinedSwithToken, ImmutableArray<string> possibleMatches)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Name = name;
        Value = value;
        IsCombinedSwitchToken = isCombinedSwithToken;
        PossibleMatches = possibleMatches;
    }

    /// <summary>
    /// Gets the token for the unknown argument.
    /// </summary>
    /// <value>
    /// The raw token value.
    /// </value>
    /// <remarks>
    /// <para>
    ///   For an unknown named argument, the token includes the prefix, and the value if one was
    ///   present using a non-whitespace separator. For example, "-Name:Value" or "--name".
    /// </para>
    /// <para>
    ///   If the unknown argument was part of a combined short switch argument when using
    ///   <see cref="ParsingMode.LongShort" qualifyHint="true"/>, the <see cref="Token"/> property
    ///   will contain all the switch names, while the <see cref="Name"/> property only contains the
    ///   name of the unknown switch. For example, the token could be "-xyz" while the name is
    ///   "y".
    /// </para>
    /// <para>
    ///   For an unknown positional argument value, the <see cref="Token"/> property is equal to
    ///   the <see cref="Value"/> property.
    /// </para>
    /// </remarks>
    public string Token { get; }

    /// <summary>
    /// Gets the name of the unknown argument.
    /// </summary>
    /// <value>
    /// The argument name, or an empty span if this was an unknown positional argument value.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the unknown argument was part of a combined short switch argument when using
    ///   <see cref="ParsingMode.LongShort" qualifyHint="true"/>, the <see cref="Token"/> property
    ///   will contain all the switch names, while the <see cref="Name"/> property only contains the
    ///   name of the unknown switch. For example, the token could be "-xyz" while the name is
    ///   "y".
    /// </para>
    /// </remarks>
    public ReadOnlyMemory<char> Name { get; }

    /// <summary>
    /// Gets the value of the unknown argument.
    /// </summary>
    /// <value>
    /// The argument value, or an empty span if this was a named argument that did not contain a
    /// value using a non-whitespace separator.
    /// </value>
    public ReadOnlyMemory<char> Value { get; }

    /// <summary>
    /// Gets a value that indicates whether this argument is one among a token containing several
    /// combined short name switches.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the unknown argument is part of a combined switch argument when
    /// using <see cref="ParsingMode.LongShort" qualifyHint="true"/>; otherwise,
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the unknown argument was part of a combined short switch argument when using
    ///   <see cref="ParsingMode.LongShort" qualifyHint="true"/>, the <see cref="Token"/> property
    ///   will contain all the switch names, while the <see cref="Name"/> property only contains the
    ///   name of the unknown switch. For example, the token could be "-xyz" while the name is
    ///   "y".
    /// </para>
    /// </remarks>
    public bool IsCombinedSwitchToken { get; }

    /// <summary>
    /// Gets or sets a value that indicates whether the unknown argument will be ignored.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to ignore the unknown argument and continue parsing with the
    /// remaining arguments; <see langword="false"/> for the default behavior where parsing fails.
    /// The default value is <see langword="false"/>
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property is not used if the <see cref="CancelParsing"/> property is set to a value
    ///   other than <see cref="CancelMode.None" qualifyHint="true"/>.
    /// </para>
    /// </remarks>
    public bool Ignore { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether parsing should be canceled when the event
    /// handler returns.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="CancelMode"/> enumeration. The default value is
    /// <see cref="CancelMode.None" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the event handler sets this property to a value other than <see cref="CancelMode.None" qualifyHint="true"/>,
    ///   command line processing will stop immediately, returning either <see langword="null"/> or
    ///   an instance of the arguments class according to the <see cref="CancelMode"/> value.
    /// </para>
    /// <para>
    ///   If you want usage help to be displayed after canceling, set the value to
    ///   <see cref="CancelMode.AbortWithHelp" qualifyHint="true"/>
    /// </para>
    /// </remarks>
    public CancelMode CancelParsing { get; set; }

    /// <summary>
    /// Gets an array of possible arguments that this argument could match by prefix.
    /// </summary>
    /// <value>
    /// An immutable array of possible arguments that this argument could match by prefix, or an
    /// empty list if no such matches were found.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property will always return an empty array if the <see cref="ParseOptions.AutoPrefixAliases" qualifyHint="true"/>
    ///   or <see cref="ParseOptionsAttribute.AutoPrefixAliases" qualifyHint="true"/> property is
    ///   <see langword="false"/>.
    /// </para>
    /// <para>
    ///   If the returned array is not empty, it is guaranteed to contains at least two items.
    /// </para>
    /// </remarks>
    public ImmutableArray<string> PossibleMatches { get; }
}
