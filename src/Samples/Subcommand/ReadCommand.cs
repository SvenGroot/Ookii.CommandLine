using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SubcommandSample;

// This is a sample subcommand that can be invoked by specifying "read" as the first argument
// to the sample application.
//
// Subcommand argument parsing works just like a regular command line argument class. After the
// arguments have been parsed, the RunAsync method is invoked to execute the command.
//
// This is an asynchronous command. It uses the AsyncCommandBase class to get a default
// implementation of Run, so we only need to worry about RunAsync, but we could also implement
// IAsyncCommand ourselves.
//
// Check the Program.cs file to see how this command is invoked.
[Command]
[Description("Reads and displays data from a file using the specified encoding, wrapping the text to fit the console.")]
[ParseOptions(ArgumentNameTransform = NameTransform.PascalCase)]
class ReadCommand : AsyncCommandBase
{
    private readonly FileInfo _path;

    // The constructor is used to define the path property. Since it's a required argument, it's
    // good to use a non-nullable reference type, but FileInfo doesn't have a good default to
    // initialize a property with. So, we use the constructor.
    //
    // The NameTransform makes sure the argument matches the naming style of the other arguments.
    public ReadCommand([Description("The name of the file to read.")] FileInfo path)
    {
        _path = path;
    }

    // An argument to specify the encoding.
    // Because Encoding doesn't have a default TypeConverter, we use a custom one provided in
    // this sample.
    [CommandLineArgument]
    [Description("The encoding to use to read the file. The default value is utf-8.")]
    [TypeConverter(typeof(EncodingConverter))]
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    // Run the command after the arguments have been parsed.
    public override async Task<int> RunAsync()
    {
        try
        {
            // We use a LineWrappingTextWriter to neatly wrap console output
            using var writer = LineWrappingTextWriter.ForConsoleOut();
            using var reader = new StreamReader(_path.FullName, Encoding, true);

            // Write the contents of the file to the console.
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                await writer.WriteLineAsync(line);
            }

            // The Main method will return the exit status to the operating system. The numbers are made up for the sample, they don't mean anything.
            // Usually, 0 means success, and any other value indicates an error.
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
