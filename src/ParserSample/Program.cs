using System;
using System.Reflection;
using Ookii.CommandLine;

namespace ParserSample
{
    public static class Program
    {
        public static void Main()
        {
            var arguments = ProgramArguments.Create();
            // No need to do anything when the value is null; Create already printed errors and usage to the console
            if( arguments == null )
            {
                return;
            }

            // We use the LineWrappingTextWriter to neatly wrap console output.
            using var writer = LineWrappingTextWriter.ForConsoleOut();
            // Print the values of the arguments.
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
