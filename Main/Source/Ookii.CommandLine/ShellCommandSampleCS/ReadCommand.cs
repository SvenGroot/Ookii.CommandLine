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
    [ShellCommand("read"), Description("Reads and displays data from a file using the specified encoding, wrapping the text to fit the console.")]
    class ReadCommand : ShellCommand
    {
        private readonly string _fileName;

        public ReadCommand([Description("The name of the file to read.")] string fileName)
        {
            // The constructor parameters are the positionl command line arguments for the shell command. This command
            // has only a single argument.
            if( fileName == null )
                throw new ArgumentNullException("fileName");
            _fileName = fileName;
        }

        // A named argument to specify the encoding.
        // Because Encoding doesn't have a TypeConverter, we simple accept the name of the encoding as a string and
        // instantiate the Encoding class ourselves in the run method.
        [CommandLineArgument("encoding", DefaultValue="utf-8"), Description("The encoding to use to read the file. The default value is utf-8.")]
        public string EncodingName { get; set; }

        public override void Run()
        {
            // This method is invoked after all command line arguments have been parsed
            try
            {
                // We use a LineWrappingTextWriter to neatly wrap console output
                using( LineWrappingTextWriter writer = LineWrappingTextWriter.ForConsoleOut() )
                using( StreamReader reader = new StreamReader(_fileName, Encoding.GetEncoding(EncodingName)) )
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
