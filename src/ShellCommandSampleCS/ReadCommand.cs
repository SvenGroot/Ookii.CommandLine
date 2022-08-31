using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Ookii.CommandLine;

namespace ShellCommandSampleCS
{
    /// <summary>
    /// This is a sample ShellCommand that can be invoked by specifying "read" as the first argument to the sample application.
    /// Shell command argument parsing works just like a regular command line argument class. After the arguments have been parsed,
    /// the Run method is invoked to execute the ShellCommand.
    /// Check the Program.cs file to see how this command is invoked.
    /// </summary>
    [ShellCommand("read"), Description("Reads and displays data from a file using the specified encoding, wrapping the text to fit the console.")]
    class ReadCommand : ShellCommand
    {
        // Positional argument to specify the file name
        [CommandLineArgument(Position = 0, IsRequired = true), Description("The name of the file to read.")]
        public string FileName { get; set; }

        // A named argument to specify the encoding.
        // Because Encoding doesn't have a TypeConverter, we simple accept the name of the encoding as a string and
        // instantiate the Encoding class ourselves in the run method.
        [CommandLineArgument("Encoding", DefaultValue="utf-8"), Description("The encoding to use to read the file.")]
        public string EncodingName { get; set; }

        public override void Run()
        {
            // This method is invoked after all command line arguments have been parsed
            try
            {
                // We use a LineWrappingTextWriter to neatly wrap console output
                using( LineWrappingTextWriter writer = LineWrappingTextWriter.ForConsoleOut() )
                using( StreamReader reader = new StreamReader(FileName, Encoding.GetEncoding(EncodingName)) )
                {
                    // Write the contents of the file to the console
                    string line;
                    while( (line = reader.ReadLine()) != null )
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            catch( ArgumentException ex ) // Happens if the encoding name is invalid
            {
                Program.WriteErrorMessage(ex.Message);
                // The Main method will return the exit status to the operating system. The numbers are made up for the sample, they don't mean anything.
                // Usually, 0 means success, and any other value indicates an error.
                ExitCode = 2;
            }
            catch( IOException ex )
            {
                Program.WriteErrorMessage(ex.Message);
                ExitCode = 2;
            }
            catch( UnauthorizedAccessException ex )
            {
                Program.WriteErrorMessage(ex.Message);
                ExitCode = 2;
            }
        }
    }
}
