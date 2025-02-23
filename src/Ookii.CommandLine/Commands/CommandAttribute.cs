using System;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Attribute that indicates a class implementing the <see cref="ICommand"/> interface is a
/// subcommand.
/// </summary>
/// <remarks>
/// <para>
///   To be considered a subcommand, a class must both implement the <see cref="ICommand"/>
///   interface and have the <see cref="CommandAttribute"/> attribute applied. This allows classes
///   that implement the <see cref="ICommand"/> interface, but do not have the attribute, to be used
///   as common base classes for other commands, without being commands themselves.
/// </para>
/// <para>
///   If a command does not have an explicit name, its name is determined by taking the type name
///   of the command class and applying the transformation specified by the
///   <see cref="CommandOptions.CommandNameTransform" qualifyHint="true"/> property.
/// </para>
/// <para>
///   Alternative names for a command can be given by using the <see cref="AliasAttribute"/>
///   attribute.
/// </para>
/// </remarks>
/// <seealso cref="CommandManager"/>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class CommandAttribute : Attribute
{
    private readonly string? _commandName;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAttribute"/> class using the target's
    /// type name as the command name.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   If a command does not have an explicit name, its name is determined by taking the type name
    ///   and applying the transformation specified by the <see cref="CommandOptions.CommandNameTransform" qualifyHint="true"/>
    ///   property.
    /// </para>
    /// </remarks>
    public CommandAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAttribute"/> class using the specified command name.
    /// </summary>
    /// <param name="commandName">
    /// The name of the command, which can be used to invoke the command or to retrieve it using the
    /// <see cref="CommandManager.GetCommand" qualifyHint="true"/> method.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="commandName"/> is <see langword="null"/>.
    /// </exception>
    public CommandAttribute(string commandName)
    {
        _commandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
    }

    /// <summary>
    /// Gets the name of the command, which can be used to invoke the command or to retrieve it
    /// using the <see cref="CommandManager.GetCommand" qualifyHint="true"/> method.
    /// </summary>
    /// <value>
    /// The name of the command, or <see langword="null"/> if the target type name will be used as
    /// the name.
    /// </value>
    public string? CommandName => _commandName;

    /// <summary>
    /// Gets or sets a value that indicates whether the command is hidden from the usage help.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the command is hidden from the usage help; otherwise,
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   A hidden command will not be included in the command list when usage help is
    ///   displayed, but can still be invoked from the command line.
    /// </para>
    /// </remarks>
    public bool IsHidden { get; set; }
}
