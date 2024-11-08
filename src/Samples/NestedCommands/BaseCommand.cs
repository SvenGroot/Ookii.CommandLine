using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Terminal;
using System.ComponentModel;

namespace NestedCommands;

// This is a base class that adds am argument and some functionality that is common to all the
// commands in this application.
// It is not exposed as a command itself because it lacks the [Command] attribute, and is abstract.
internal abstract class BaseCommand : AsyncCommandBase
{
    // The path argument can be used by any command that inherits from this class.
    [CommandLineArgument]
    [Description("The json file holding the data.")]
    public string Path { get; set; } = "data.json";

    // Implement the task's RunAsync method to load the database and handle some errors.
    public override async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            var database = await Database.Load(Path, cancellationToken);
            return await RunAsync(database, cancellationToken);
        }
        catch (IOException ex)
        {
            VirtualTerminal.WriteLineErrorFormatted(ex.Message);
            return (int)ExitCode.IOError;
        }
        catch (UnauthorizedAccessException ex)
        {
            VirtualTerminal.WriteLineErrorFormatted(ex.Message);
            return (int)ExitCode.IOError;
        }
    }

    // Derived classes will implement this instead of the normal RunAsync.
    protected abstract Task<int> RunAsync(Database db, CancellationToken cancellationToken);
}
