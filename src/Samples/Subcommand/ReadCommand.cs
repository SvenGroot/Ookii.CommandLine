using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Terminal;
using System.ComponentModel;
using System.Text;

namespace SubcommandSample;

// This is a sample subcommand that can be invoked by specifying "read" as the first argument
// to the sample application.
//
// Subcommand argument parsing works just like a regular command line arguments class. After the
// arguments have been parsed, the RunAsync method is invoked to execute the command.
//
// This is an asynchronous command. It uses the AsyncCommandBase class to get a default
// implementation of Run, so we only need to worry about RunAsync, but we could also implement
// IAsyncCommand ourselves.
//
// Check the Program.cs file to see how this command is invoked.
[GeneratedParser]
[Command]
[Description("Reads and displays data from a file using the specified encoding, wrapping the text to fit the console.")]
partial class ReadCommand : AsyncCommandBase
{
    // A required, positional argument to specify the file name.
    [CommandLineArgument(IsPositional = true)]
    [Description("The path of the file to read.")]
    public required FileInfo Path { get; set; }

    // An argument to specify the encoding.
    // Because Encoding doesn't have a default ArgumentConverter, we use a custom one provided in
    // this sample.
    // Encoding's ToString() implementation just gives the class name, so don't include the default
    // value in the usage help; we'll write it ourself instead.
    [CommandLineArgument(IncludeDefaultInUsageHelp = false)]
    [Description("The encoding to use to read the file. The default value is utf-8.")]
    [ArgumentConverter(typeof(EncodingConverter))]
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    // Run the command after the arguments have been parsed.
    public override async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            // We use a LineWrappingTextWriter to neatly wrap console output
            using var writer = LineWrappingTextWriter.ForConsoleOut();

            // Write the contents of the file to the console.
            await foreach (var line in File.ReadLinesAsync(Path.FullName, Encoding, cancellationToken))
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
            VirtualTerminal.WriteLineErrorFormatted(ex.Message);
            return (int)ExitCode.ReadWriteFailure;
        }
        catch (UnauthorizedAccessException ex)
        {
            VirtualTerminal.WriteLineErrorFormatted(ex.Message);
            return (int)ExitCode.ReadWriteFailure;
        }
    }
}
