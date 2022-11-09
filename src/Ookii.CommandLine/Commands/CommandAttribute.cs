// Copyright (c) Sven Groot (Ookii.org)
using System;

namespace Ookii.CommandLine.Commands
{
    /// <summary>
    /// Attribute that indicates a class implementing the <see cref="ICommand"/> interface is a
    /// subcommand.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   To be considered a subcommand, a class must both implement the <see cref="ICommand"/>
    ///   interface and have the <see cref="CommandAttribute"/> applied.
    /// </para>
    /// <para>
    ///   This allows classes implementing <see cref="ICommand"/> but without the attribute to be
    ///   used as common base classes for other commands, without being commands themselves.
    /// </para>
    /// <para>
    ///   If a command has no explicit name, its name is determined by taking the type name
    ///   and applying the transformation specified by the <see cref="CommandOptions.CommandNameTransform"/>
    ///   property.
    /// </para>
    /// <para>
    ///   A command can be given more than one name by using the <see cref="AliasAttribute"/>
    ///   attribute.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandManager"/>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandAttribute : Attribute
    {
        private readonly string? _commandName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class using the target's
        /// type name as the command name.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   If a command has no explicit name, its name is determined by taking the type name
        ///   and applying the transformation specified by the <see cref="CommandOptions.CommandNameTransform"/>
        ///   property.
        /// </para>
        /// </remarks>
        public CommandAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class using the specified command name.
        /// </summary>
        /// <param name="commandName">The name of the command, which can be used to locate it using the <see cref="CommandManager.GetCommand"/> method.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandName"/> is <see langword="null"/>.
        /// </exception>
        public CommandAttribute(string commandName)
        {
            _commandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
        }

        /// <summary>
        /// Gets the name of the command, which can be used to locate it using the <see cref="CommandManager.GetCommand"/> method.
        /// </summary>
        /// <value>
        /// The name of the command, or <see langword="null"/> to use the type name as the command
        /// name.
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
}
