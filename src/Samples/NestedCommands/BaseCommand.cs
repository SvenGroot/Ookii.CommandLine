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
    [CommandLineArgument(DefaultValue = "data.json")]
    [Description("The json file holding the data.")]
    public string Path { get; set; } = string.Empty;

    // Implement the task's RunAsync method to load the database and handle some errors.
    public override async Task<int> RunAsync()
    {
        try
        {
            return await RunAsync(await Database.Load(Path));
        }
        catch (IOException ex)
        {
            WriteErrorMessage(ex.Message);
            return (int)ExitCode.IOError;
        }
        catch (UnauthorizedAccessException ex)
        {
            WriteErrorMessage(ex.Message);
            return (int)ExitCode.IOError;
        }
    }

    // Derived classes will implement this instead of the normal RunAsync.
    protected abstract Task<int> RunAsync(Database db);

    // Helper method to print error messages.
    private static void WriteErrorMessage(string message)
    {
        using var support = VirtualTerminal.EnableColor(StandardStream.Error);
        using var writer = LineWrappingTextWriter.ForConsoleError();

        // Add some color if we can.
        if (support.IsSupported)
        {
            writer.Write(TextFormat.ForegroundRed);
        }

        writer.WriteLine(message);
        if (support.IsSupported)
        {
            writer.Write(TextFormat.Default);
        }
    }
}
