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
        /// <param name="index">The index of the first argument.</param>
        /// <param name="options">The options to use for parsing and usage help.</param>
        void Parse(string[] args, int index, CommandOptions options);
    }
}
