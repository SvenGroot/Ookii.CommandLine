using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Attribute that specifies the name of a <see cref="ShellCommand"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ShellCommandAttribute : Attribute
    {
        private readonly string _commandName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellCommandAttribute"/> class.
        /// </summary>
        /// <param name="commandName">The name of the command, which can be used to locate it using the <see cref="ShellCommand.GetShellCommand(System.Reflection.Assembly,string)"/> method.</param>
        public ShellCommandAttribute(string commandName)
        {
            if( commandName == null )
                throw new ArgumentNullException("commandName");

            _commandName = commandName;
        }

        /// <summary>
        /// Gets the name of the command, which can be used to locate it using the <see cref="ShellCommand.GetShellCommand(System.Reflection.Assembly,string)"/> method.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        public string CommandName
        {
            get { return _commandName; }
        }
    }
}
