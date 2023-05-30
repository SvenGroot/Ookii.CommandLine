using System;

namespace Ookii.CommandLine.Commands
{
    /// <summary>
    /// Represents a subcommand that does its own argument parsing.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Unlike commands that only implement the <see cref="ICommand"/> interfaces, commands that
    ///   implement the <see cref="ICommandWithCustomParsing"/> interface are not created with the
    ///   <see cref="CommandLineParser"/>. Instead, they must have a public constructor with no
    ///   parameters, and must parse the arguments manually by implementing the <see cref="Parse"/>
    ///   method.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandManager"/>
    public interface ICommandWithCustomParsing : ICommand
    {
        /// <summary>
        /// Parses the arguments for the command.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="manager">The <see cref="CommandManager"/> that was used to create this command.</param>
        void Parse(ReadOnlyMemory<string> args, CommandManager manager);
    }
}
