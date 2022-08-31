using System;
using System.Reflection;
using Ookii.CommandLine;

namespace CommandLineSampleCS
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ProgramArguments arguments = ProgramArguments.Create(args);
            // No need to do anything when the value is null; Create already printed errors and usage to the console
            if( arguments != null )
            {
                // This application doesn't do anything useful, it's just a sample of using CommandLineParser after all. We use reflection to print
                // the values of all the properties of the sample's CommandLineArguments class, which correspond to the sample's command line arguments.

                // We use the LineWrappingTextWriter to neatly wrap console output.
                using( LineWrappingTextWriter writer = LineWrappingTextWriter.ForConsoleOut() )
                {
                    // Print the full command line as received by the application
                    writer.WriteLine("The command line was: {0}", Environment.CommandLine);
                    writer.WriteLine();
                    // Print the values of the arguments, using reflection to get all the property values
                    writer.WriteLine("The following argument values were provided:");
                    writer.WriteLine("Source: {0}", arguments.Source ?? "(null)");
                    writer.WriteLine("Destination: {0}", arguments.Destination ?? "(null)");
                    writer.WriteLine("Index: {0}", arguments.Index);
                    writer.WriteLine("Date: {0}", arguments.Date == null ? "(null)" : arguments.Date.ToString());
                    writer.WriteLine("Count: {0}", arguments.Count);
                    writer.WriteLine("Verbose: {0}", arguments.Verbose);
                    writer.WriteLine("Values: {0}", arguments.Values == null ? "(null)" : "{ " + string.Join(", ", arguments.Values) + " }");
                    writer.WriteLine("Help: {0}", arguments.Help);
                }
            }
        }
    }
}
