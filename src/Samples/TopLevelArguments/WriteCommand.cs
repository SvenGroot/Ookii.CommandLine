using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Validation;
using System.ComponentModel;

namespace TopLevelArguments;

// This command is identical to the write command of the Subcommand sample; see that for a more
// detailed description.
[GeneratedParser]
[Command]
[Description("Writes lines to a file, wrapping them to the specified width.")]
partial class WriteCommand : AsyncCommandBase
{
    // Positional multi-value argument to specify the text to write
    [CommandLineArgument(IsPositional = true)]
    [Description("The lines of text to write to the file; if no lines are specified, this application will read from standard input instead.")]
    public string[]? Lines { get; set; }

    // An argument that specifies the maximum line length of the output.
    [CommandLineArgument(IsShort = true)]
    [Description("The maximum length of the lines in the file, or 0 to have no limit.")]
    [ValidateRange(0, null)]
    public int MaximumLineLength { get; set; } = 79;

    // A switch argument that indicates it's okay to overwrite files.
    [CommandLineArgument(IsShort = true)]
    [Description("When this option is specified, the file will be overwritten if it already exists.")]
    public bool Overwrite { get; set; }

    // Run the command after the arguments have been parsed.
    public override async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check if we're allowed to overwrite the file.
            if (!Overwrite && Program.Arguments!.Path.Exists)
            {
                // The Main method will return the exit status to the operating system. The numbers
                // are made up for the sample, they don't mean anything. Usually, 0 means success,
                // and any other value indicates an error.
                Program.WriteErrorMessage("File already exists.");
                return (int)ExitCode.FileExists;
            }

            var options = new FileStreamOptions()
            {
                Access = FileAccess.Write,
                Mode = Overwrite ? FileMode.Create : FileMode.CreateNew,
                Share = FileShare.ReadWrite | FileShare.Delete,
                Options = FileOptions.Asynchronous
            };

            using var writer = new StreamWriter(Program.Arguments!.Path.FullName, Program.Arguments.Encoding, options);

            // We use a LineWrappingTextWriter to neatly white-space wrap the output.
            using var lineWriter = new LineWrappingTextWriter(writer, MaximumLineLength);

            // Write the specified content to the file
            foreach (string line in GetLines())
            {
                await lineWriter.WriteLineAsync(line.AsMemory(), cancellationToken);
            }

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

    private IEnumerable<string> GetLines()
    {
        // Choose between the specified lines or standard input.
        if (Lines == null || Lines.Length == 0)
        {
            return EnumerateStandardInput();
        }

        return Lines;
    }

    private static IEnumerable<string> EnumerateStandardInput()
    {
        // Read from standard input. You can pipe a file to the input, or use it interactively (in
        // that case, press CTRL-D (CTRL-Z on Windows) to send an EOF character and stop writing).
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            yield return line;
        }
    }
}
