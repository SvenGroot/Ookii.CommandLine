using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Terminal;
using Ookii.CommandLine.Validation;
using System.ComponentModel;
using System.Text;

namespace SubcommandSample;

// This is a sample subcommand that can be invoked by specifying "write" as the first argument
// to the sample application.
//
// Subcommand argument parsing works just like a regular command line argument class. After the
// arguments have been parsed, the Run method is invoked to execute the command.
//
// This is an asynchronous command. It uses the AsyncCommandBase class to get a default
// implementation of Run, so we only need to worry about RunAsync, but we could also implement
// IAsyncCommand ourselves.
//
// Check the Program.cs file to see how this command is invoked.
[GeneratedParser]
[Command]
[Description("Writes lines to a file, wrapping them to the specified width.")]
partial class WriteCommand : AsyncCommandBase
{
    // A required, positional argument to specify the file name.
    [CommandLineArgument(IsPositional = true)]
    [Description("The path of the file to write to.")]
    public required FileInfo Path { get; set; }

    // Positional multi-value argument to specify the text to write
    [CommandLineArgument(IsPositional = true)]
    [Description("The lines of text to write to the file; if no lines are specified, this application will read from standard input instead.")]
    public string[]? Lines { get; set; }

    // An argument to specify the encoding.
    // Because Encoding doesn't have a default ArgumentConverter, we use a custom one provided in
    // this sample.
    // Encoding's ToString() implementation just gives the class name, so don't include the default
    // value in the usage help; we'll write it ourself instead.
    [CommandLineArgument(IncludeDefaultInUsageHelp = false)]
    [Description("The encoding to use to write the file. Default value: utf-8.")]
    [ArgumentConverter(typeof(EncodingConverter))]
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    // An argument that specifies the maximum line length of the output.
    [CommandLineArgument]
    [Description("The maximum length of the lines in the file, or 0 to have no limit.")]
    [Alias("Length")]
    [ValidateRange(0, null)]
    public int MaximumLineLength { get; set; } = 79;

    // A switch argument that indicates it's okay to overwrite files.
    [CommandLineArgument]
    [Description("When this option is specified, the file will be overwritten if it already exists.")]
    public bool Overwrite { get; set; }

    // Run the command after the arguments have been parsed.
    public override async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check if we're allowed to overwrite the file.
            if (!Overwrite && Path.Exists)
            {
                // The Main method will return the exit status to the operating system. The numbers
                // are made up for the sample, they don't mean anything. Usually, 0 means success,
                // and any other value indicates an error.
                VirtualTerminal.WriteLineErrorFormatted("File already exists.");
                return (int)ExitCode.FileExists;
            }

            var options = new FileStreamOptions()
            {
                Access = FileAccess.Write,
                Mode = Overwrite ? FileMode.Create : FileMode.CreateNew,
                Share = FileShare.ReadWrite | FileShare.Delete,
                Options = FileOptions.Asynchronous
            };

            using var writer = new StreamWriter(Path.FullName, Encoding, options);

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
            VirtualTerminal.WriteLineErrorFormatted(ex.Message);
            return (int)ExitCode.ReadWriteFailure;
        }
        catch (UnauthorizedAccessException ex)
        {
            VirtualTerminal.WriteLineErrorFormatted(ex.Message);
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
