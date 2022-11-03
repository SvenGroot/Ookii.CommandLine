using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;

namespace SubCommandSample
{
    /// <summary>
    /// This is a sample subcommand that can be invoked by specifying "read" as the first argument to the sample application.
    /// Subcommand argument parsing works just like a regular command line argument class. After the arguments have been parsed,
    /// the Run method is invoked to execute the command.
    /// Check the Program.cs file to see how this command is invoked.
    /// </summary>
    [Command("write"), Description("Writes lines to a file, wrapping them to the specified width.")]
    class WriteCommand : ICommand
    {
        // Positional argument to specify the file name
        [CommandLineArgument(Position = 0, IsRequired = true), Description("The name of the file to write to.")]
        public string FileName { get; set; }

        // Positional multi-value argument to specify the text to write
        [CommandLineArgument(Position = 1), Description("The lines of text to write to the file; if no lines are specified, this application will read from standard input instead.")]
        public string[] Lines { get; set; }

        // A named argument to specify the encoding.
        // Because Encoding doesn't have a TypeConverter, we simple accept the name of the encoding as a string and
        // instantiate the Encoding class ourselves in the run method.
        [CommandLineArgument("Encoding", DefaultValue = "utf-8"), Description("The encoding to use to write the file.")]
        public string EncodingName { get; set; }

        // A named argument that specifies the maximum line length of the output
        [CommandLineArgument(DefaultValue = 79), Alias("Length"), Description("The maximum length of the lines in the file, or zero to have no limit.")]
        public int MaximumLineLength { get; set; }

        // A named argument switch that indicates it's okay to overwrite files.
        [CommandLineArgument, Description("When this option is specified, the file will be overwritten if it already exists.")]
        public bool Overwrite { get; set; }

        public int Run()
        {
            // This method is invoked after all command line arguments have been parsed
            try
            {
                // Check if we're allowed to overwrite the file.
                if( !Overwrite && File.Exists(FileName) )
                {
                    // The Main method will return the exit status to the operating system. The numbers are made up for the sample, they don't mean anything.
                    // Usually, 0 means success, and any other value indicates an error.
                    Program.WriteErrorMessage("File already exists.");
                    return 3;
                }
                else
                {
                    // We use a LineWrappingTextWriter to neatly wrap the output.
                    using StreamWriter writer = new StreamWriter(FileName, false, Encoding.GetEncoding(EncodingName));
                    using LineWrappingTextWriter lineWriter = new LineWrappingTextWriter(writer, MaximumLineLength, true);

                    // Write the specified content to the file
                    foreach (string line in GetLines())
                    {
                        lineWriter.WriteLine(line);
                    }
                }

                return 0;
            }
            catch( ArgumentException ex ) // Happens if the encoding name is invalid
            {
                Program.WriteErrorMessage(ex.Message);
                return 2;
            }
            catch( IOException ex )
            {
                Program.WriteErrorMessage(ex.Message);
                return 2;
            }
            catch ( UnauthorizedAccessException ex )
            {
                Program.WriteErrorMessage(ex.Message);
                return 2;
            }
        }

        private IEnumerable<string> GetLines()
        {
            // Some magic to choose between the specified lines or standard input.
            if( Lines == null || Lines.Length == 0 )
                return EnumerateStandardInput();
            else
                return Lines;
        }

        private static IEnumerable<string> EnumerateStandardInput()
        {
            // Read from standard input. You can pipe a file to the input, or use it interactively (in that case, press CTRL-Z to send an EOF character and stop writing).
            string line;
            while( (line = Console.ReadLine()) != null )
            {
                yield return line;
            }
        }
    }
}
