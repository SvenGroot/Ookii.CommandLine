﻿using System;

namespace Ookii.CommandLine;

/// <summary>
/// Defines an alternative name for a command line argument or a subcommand.
/// </summary>
/// <remarks>
/// <note>
///   To specify multiple aliases, apply this attribute multiple times.
/// </note>
/// <para>
///   The aliases for a command line argument can be used instead of their regular name to specify the parameter on the command line.
///   For example, this can be used to have a shorter name for an argument (e.g. "-v" as an alternative to "-Verbose").
/// </para>
/// <para>
///   All regular command line argument names and aliases used by an instance of the <see cref="CommandLineParser"/> class must be
///   unique.
/// </para>
/// <para>
///   By default, the command line usage help generated by the <see cref="CommandLineParser.WriteUsage(UsageWriter)"/>
///   method includes the aliases. Set the <see cref="UsageWriter.IncludeAliasInDescription"/>
///   property to <see langword="false"/> to exclude them.
/// </para>
/// <note>
///   If the <see cref="CommandLineParser.Mode"/> property is <see cref="ParsingMode.LongShort"/>, and the argument
///   this is applied to does not have a long name, this attribute is ignored.
/// </note>
/// <para>
///   This attribute can also be applied to classes that implement the <see cref="Commands.ICommand"/>
///   interface to specify an alias for that command. In that case, inclusion of the aliases in
///   the command list usage help is controlled by the <see cref="UsageWriter.IncludeCommandAliasInCommandList"/>
///   property.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
/// <seealso cref="CommandLineArgument.Aliases" qualifyHint="true"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = true)]
public sealed class AliasAttribute : Attribute
{
    private readonly string _alias;

    /// <summary>
    /// Initializes a new instance of the <see cref="AliasAttribute"/> class.
    /// </summary>
    /// <param name="alias">The alternative name for the command line argument or subcommand.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="alias"/> is <see langword="null"/>.
    /// </exception>
    public AliasAttribute(string alias)
    {
        _alias = alias ?? throw new ArgumentNullException(nameof(alias));
    }

    /// <summary>
    /// Gets the alternative name for the command line argument or subcommand.
    /// </summary>
    /// <value>
    /// The alternative name.
    /// </value>
    public string Alias
    {
        get { return _alias; }
    }
}
