using System;
using System.Collections.Generic;
using System.Text;
using Ookii.CommandLine;
using System.Reflection;
using System.IO;

namespace ShellCommandSampleCS
{
    static class Program
    {
        static int Main(string[] args)
        {
            // Create a shell command based on the arguments. The CreateShellCommand method will catch any command line errors
            // and print error details and usage information on the console, so we don't have to worry about that here.
            ShellCommand command = ShellCommand.CreateShellCommand(Assembly.GetExecutingAssembly(), args, 0);
            if( command != null )
            {
                // The command line arguments were successfully parsed, so run the command.
                command.Run();
                // When using shell commands, it's good practice to return the value of the ExitStatus property to the operating system.
                // The application or script that invoked your application can check the exit code from your application to
                // see if you were successful or not. Error codes are completely application specific, but usually 0 is used to
                // indicate success, and any other value indicates an error.
                return command.ExitCode;
            }

            return 1; // Return an error status if the command couldn't be created.
        }

        /// <summary>
        /// Utility method used by the commands to write exception data to the console.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteErrorMessage(string message)
        {
            using( LineWrappingTextWriter writer = LineWrappingTextWriter.ForConsoleError() )
            {
                writer.WriteLine(message);
            }
        }
    }
}
