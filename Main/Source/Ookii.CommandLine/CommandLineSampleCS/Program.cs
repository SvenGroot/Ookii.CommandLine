using System;
using System.Reflection;
using Ookii.CommandLine;

namespace CommandLineSampleCS
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Create a command line parser for the type that defines this sample's command line arguments
            CommandLineParser parser = new CommandLineParser(typeof(CommandLineArguments));
            // The ArgumentParsed event is used by this sample to stop processing after the -? argument is received.
            parser.ArgumentParsed += new EventHandler<ArgumentParsedEventArgs>(CommandLineParser_ArgumentParsed);

            try
            {
                // Parse the command line arguments. This will throw a CommandLineArgumentException if the right arguments weren't supplied
                CommandLineArguments arguments = (CommandLineArguments)parser.Parse(args);

                if( arguments == null )
                {
                    // null means that parsing was cancelled by our ArgumentParsed event handler,
                    // which indicates the -? argument was supplied, so we should print usage.
                    parser.WriteUsageToConsole();
                }
                else
                {
                    // We use the LineWrappingTextWriter to neatly wrap console output.
                    using( LineWrappingTextWriter writer = LineWrappingTextWriter.ForConsoleOut() )
                    {
                        // Print the full command line as received by the application
                        writer.WriteLine("The command line was: {0}", Environment.CommandLine);
                        writer.WriteLine();
                        // This application doesn't do anything useful, it's just a sample of using CommandLineParser after all. We use reflection to print
                        // the values of all the properties of the sample's CommandLineArguments class, which correspond to the sample's command line arguments.
                        writer.WriteLine("The following arguments were provided:");
                        PropertyInfo[] properties = typeof(CommandLineArguments).GetProperties();
                        foreach( PropertyInfo property in properties )
                        {
                            if( property.PropertyType.IsArray )
                            {
                                // Print a list of all the values for an array argument.
                                writer.Write("{0}: ", property.Name);
                                Array array = (Array)property.GetValue(arguments, null);
                                if( array == null )
                                    writer.WriteLine("(null)");
                                else
                                {
                                    writer.Write("{ ");
                                    for( int x = 0; x < array.GetLength(0); ++x )
                                    {
                                        if( x > 0 )
                                            writer.Write(", ");
                                        writer.Write(array.GetValue(x));
                                    }
                                    writer.WriteLine(" }");
                                }
                            }
                            else
                                writer.WriteLine("{0}: {1}", property.Name, property.GetValue(arguments, null) ?? "(null)");
                        }
                    }
                }
            }
            catch( CommandLineArgumentException ex )
            {
                // We use the LineWrappingTextWriter to neatly wrap console output.
                using( LineWrappingTextWriter writer = LineWrappingTextWriter.ForConsoleError() )
                {
                    // Tell the user what went wrong
                    writer.WriteLine(ex.Message);
                    writer.WriteLine();
                }
                // Print usage information so the user can see how to correctly invoke the program
                parser.WriteUsageToConsole();
            }
        }

        private static void CommandLineParser_ArgumentParsed(object sender, ArgumentParsedEventArgs e)
        {
            // When we receive the -? argument, we immediately cancel processing. That way, CommandLineParser<T>.Parse will
            // return null, and the Main method will display usage, even if the correct number of positional arguments was supplied.
            // Try it: just call the sample with "CommandLineSampleCS.exe foo bar -?", which will print usage even though both the source and destination
            // arguments are supplied.
            if( e.Argument.ArgumentName == "?" )
                e.Cancel = true;
        }
    }
}
