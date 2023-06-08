using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using System.ComponentModel;

namespace TopLevelArguments;

// This command is identical to the read command of the Subcommand sample; see that for a more
// detailed description.
[GeneratedParser]
[Command]
[Description("Reads and displays data from a file using the specified encoding, wrapping the text to fit the console.")]
partial class ReadCommand : AsyncCommandBase
{
    // Run the command after the arguments have been parsed.
    public override async Task<int> RunAsync()
    {
        try
        {
            var options = new FileStreamOptions()
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Share = FileShare.ReadWrite | FileShare.Delete,
                Options = FileOptions.Asynchronous
            };

            using var reader = new StreamReader(Program.Arguments!.Path.FullName, Program.Arguments.Encoding, true, options);

            // We use a LineWrappingTextWriter to neatly wrap console output
            using var writer = LineWrappingTextWriter.ForConsoleOut();

            // Write the contents of the file to the console.
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                await writer.WriteLineAsync(line);
            }

            // The Main method will return the exit status to the operating system. The numbers are
            // made up for the sample, they don't mean anything. Usually, 0 means success, and any
            // other value indicates an error.
            return (int)ExitCode.Success;
        }
        catch (IOException ex)
        {
            Program.WriteErrorMessage(ex.Message);
            return (int)ExitCode.ReadWriteFailure;
        }
        catch (UnauthorizedAccessException ex)
        {
            Program.WriteErrorMessage(ex.Message);
            return (int)ExitCode.ReadWriteFailure;
        }
    }
}
