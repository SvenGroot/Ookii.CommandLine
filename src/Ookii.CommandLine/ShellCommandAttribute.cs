// Copyright (c) Sven Groot (Ookii.org)
using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Attribute that specifies the name of a <see cref="ShellCommand"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ShellCommandAttribute : Attribute
    {
        private readonly string? _commandName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellCommandAttribute"/> class using the target's type name as the command name.
        /// </summary>
        public ShellCommandAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellCommandAttribute"/> class using the specified command name.
        /// </summary>
        /// <param name="commandName">The name of the command, which can be used to locate it using the <see cref="ShellCommand.GetShellCommand"/> method.</param>
        public ShellCommandAttribute(string commandName)
        {
            if( commandName == null )
                throw new ArgumentNullException(nameof(commandName));

            _commandName = commandName;
        }

        /// <summary>
        /// Gets the name of the command, which can be used to locate it using the <see cref="ShellCommand.GetShellCommand"/> method.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        public string? CommandName
        {
            get { return _commandName; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command does its own argument parsing.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if the command does its own argument parsing; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is set to <see langword="true"/>, the <see cref="ShellCommand.CreateShellCommand(System.Reflection.Assembly, string[], int, CreateShellCommandOptions?)"/> method and 
        ///   the <see cref="ShellCommand.RunShellCommand(System.Reflection.Assembly, string[], int, CreateShellCommandOptions?)"/>  will not create the command with the <see cref="CommandLineParser"/>. Instead,
        ///   the command type must define a constructor that takes three arguments: an array of <see cref="String"/> values that will contain the raw command line arguments, an <see cref="Int32"/> that
        ///   indicates the index of the first argument in the array after the command name, and a <see cref="CreateShellCommandOptions"/> instance specifying argument parsing
        ///   and error handling options.
        /// </para>
        /// <para>
        ///   This constructor should not throw an exception is argument parsing fails. Instead, it should write error and usage information to the <see cref="System.IO.TextWriter"/> instances specified by
        ///   <see cref="ParseOptions.Error"/> and <see cref="ParseOptions.Out"/>.
        /// </para>
        /// <para>
        ///   If this property is set to <see langword="true"/> and the shell command type does not have a constructor with those arguments, creating the command will fail.
        /// </para>
        /// </remarks>
        public bool CustomArgumentParsing { get; set; }

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
        ///   displayed.
        /// </para>
        /// </remarks>
        public bool IsHidden { get; set; }
    }
}
