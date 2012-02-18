// Copyright (c) Sven Groot (Ookii.org) 2012
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at http://ookiicommandline.codeplex.com. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;
using System.Collections.Generic;
using System.Text;
using Ookii.CommandLine;
using System.ComponentModel;
using System.IO;

namespace ShellCommandSampleCS
{
    /// <summary>
    /// This is a sample ShellCommand that can be invoked by specifying "read" as the first argument to the sample application.
    /// Shell command argument parsing works just like a regular command line argument class. After the arguments have been parsed,
    /// the Run method is invoked to execute the ShellCommand.
    /// Check the Program.cs file to see how this command is invoked.
    /// </summary>
    [ShellCommand("write"), Description("Writes lines to a file, wrapping them to the specified width.")]
    class WriteCommand : ShellCommand
    {
        private string _fileName;
        private string[] _lines;

        public WriteCommand([Description("The name of the file to write to.")] string fileName,
                            [Description("The lines of text to write from the file; if no lines are specified, this application will read from standard input instead.")] string[] lines = null)
        {
            // The constructor parameters are the positionl command line arguments for the shell command. This command
            // has a single required argument, and an optional argument that can have multiple values.
            if( fileName == null )
                throw new ArgumentNullException("fileName");

            _fileName = fileName;
            _lines = lines;
        }

        // A named argument to specify the encoding.
        // Because Encoding doesn't have a TypeConverter, we simple accept the name of the encoding as a string and
        // instantiate the Encoding class ourselves in the run method.
        [CommandLineArgument("encoding", DefaultValue = "utf-8"), Description("The encoding to use to write the file. The default value is utf-8.")]
        public string EncodingName { get; set; }

        // A named argument that specifies the maximum line length of the output
        [CommandLineArgument("length", DefaultValue = 79), Description("The maximum length of the lines in the file, or zero to have no limit. The default value is 79.")]
        public int MaximumLineLength { get; set; }

        // A named argument switch that indicates it's okay to overwrite files.
        [CommandLineArgument("overwrite", DefaultValue = false), Description("When this option is specified, the file will be overwritten if it already exists.")]
        public bool OverwriteFile { get; set; }

        public override void Run()
        {
            // This method is invoked after all command line arguments have been parsed
            try
            {
                // Check if we're allowed to overwrite the file.
                if( !OverwriteFile && File.Exists(_fileName) )
                {
                    // The Main method will return the exit status to the operating system. The numbers are made up for the sample, they don't mean anything.
                    // Usually, 0 means success, and any other value indicates an error.
                    Program.WriteErrorMessage("File already exists.");
                    ExitCode = 3;
                }
                else
                {
                    // We use a LineWrappingTextWriter to neatly wrap the output.
                    using( StreamWriter writer = new StreamWriter(_fileName, false, Encoding.GetEncoding(EncodingName)) )
                    using( LineWrappingTextWriter lineWriter = new LineWrappingTextWriter(writer, MaximumLineLength, true) )
                    {
                        // Write the specified content to the file
                        foreach( string line in GetLines() )
                        {
                            lineWriter.WriteLine(line);
                        }
                    }
                }
            }
            catch( ArgumentException ex ) // Happens if the encoding name is invalid
            {
                Program.WriteErrorMessage(ex.Message);
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

        private IEnumerable<string> GetLines()
        {
            // Some magic to choose between the specified lines or standard input.
            if( _lines == null || _lines.Length == 0 )
                return EnumerateStandardInput();
            else
                return _lines;
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
