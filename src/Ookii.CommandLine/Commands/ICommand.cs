namespace Ookii.CommandLine.Commands;

/// <summary>
/// Represents a subcommand of the application.
/// </summary>
/// <remarks>
/// <para>
///   To create a subcommand for your application, create a class that implements this interface,
///   then apply the <see cref="CommandAttribute"/> attribute to it.
/// </para>
/// <para>
///   The class will be used as an arguments type with the <see cref="CommandLineParser"/>.
///   Alternatively, a command can implement its own argument parsing by implementing the
///   <see cref="ICommandWithCustomParsing"/> interface.
/// </para>
/// </remarks>
/// <seealso cref="CommandManager"/>
public interface ICommand
{
    /// <summary>
    /// Runs the command.
    /// </summary>
    /// <returns>The exit code for the command.</returns>
    /// <remarks>
    /// <para>
    ///   Typically, your application's <c>Main()</c> method should return the exit code of the
    ///   command that was executed.
    /// </para>
    /// </remarks>
    int Run();
}
