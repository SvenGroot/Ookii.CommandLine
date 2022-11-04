using Ookii.CommandLine;
using Ookii.CommandLine.Commands;

namespace SubCommandSample
{
    static class Program
    {
        static int Main()
        {
            // Set options so the command names are determined by the class name, transformed to
            // dash-case and with the "Command" suffix stripped.
            // You can use the CommmandOptions class to further customize the parsing behavior and
            // usage help output.
            var options = new CommandOptions()
            {
                CommandNameTransform = NameTransform.DashCase
            };

            // Create a CommandManager for the commands in the current assembly.
            var manager = new CommandManager(options);

            // Run the command indicated in the first argument to this application, and use the
            // return value of its Run method as the applicatino exit code. If the command could
            // not be created, we return an error code.
            return manager.RunCommand() ?? 1;
        }

        /// <summary>
        /// Utility method used by the commands to write exception data to the console.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteErrorMessage(string message)
        {
            using LineWrappingTextWriter writer = LineWrappingTextWriter.ForConsoleError();
            writer.WriteLine(message);
        }
    }
}
