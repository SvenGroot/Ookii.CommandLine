// Copyright (c) Sven Groot (Ookii.org) 2013
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at https://github.com/SvenGroot/ookii.commandline. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;
using System.ComponentModel;
using Ookii.CommandLine;

namespace CommandLineSampleCS
{

    enum Command { Unassigned, Add, Delete, Update};
    /// <summary>
    /// Class that defines the sample's command line arguments.
    /// </summary>
    [Description("Sample command line application. The application parses the command line and prints the results, but otherwise does nothing and none of the arguments are actually used for anything.")]
    class ProgramArguments
    {
        [CommandLineArgument(Position = 0, IsRequired = true), Description("The command to execute.")]
        public Command Command { get; set; }

        // This property defines a required positional argument called "Source". It can be set by name as e.g. "-Source value", or by position
        // by specifying "value" as the first positional argument. Note that by default command line argument names are case insensitive, so
        // this argument can also be specified as e.g. "-source value".
        // On Windows, the default command name prefixes are "/" and "-", so the argument can also be specified as e.g. "/Source value". On Unix
        // (using Mono), only "-" is accepted by default.
        [CommandLineArgument(Position = 1, IsRequired = true), Description("The source data.")]
        public string Source { get; set; }

        // This property defines a required positional argument called "Destination". It can be set by name as e.g. "-Destination value", or by position
        // by specifying "value" as the second positional argument.
        [CommandLineArgument(Position = 2, IsRequired = true), Description("The destination data.")]
        public string Destination { get; set; }

        // This property defines a optional positional argument called "Index". It can be set by name as e.g. "-Index 5", or by position
        // by specifying e.g. "5" as the third positional argument. If the argument is not specified, this property
        // will be set to the default value 1.
        [CommandLineArgument(Position = 3, DefaultValue = 1), Description("The operation's index.")]
        public int Index { get; set; }

        // This property defines an argument named "Date". This argument is not positional, so it can be supplied only by name, for example as "-Date 2013-01-31".
        // This argument uses a nullable value type so it will be set to null if the value is not supplied, rather than having to choose a default value.
        // For types other than string, CommandLineParser will use the TypeConverter for the argument's type to try to convert the string to
        // the correct type. You can use your own custom classes or structures for command line arguments as long as you create a TypeConverter for
        // the type.
        // The type conversion from string to DateTime is culture sensitive. Which culture is used is indicated by the CommandLineParser.Culture
        // property, which defaults to the user's current culture. Always pay attention when a conversion is culture specific (this goes for
        // dates, numbers, and various other types) and consider whether the current culture is the right choice for your application. In some cases
        // using CultureInfo.InvariantCulture could be more appropriate.
        [CommandLineArgument, Description("Provides a date to the application; the format to use depends on your regional settings.")]
        public DateTime? Date { get; set; }

        // This property defines an argument named "Count".
        // This argument uses a custom ValueDescription so it shows up as "-Count <Number>" in the usage rather than as "-Count <Int32>"
        [CommandLineArgument(ValueDescription = "Number"), Description("Provides the count for something to the application.")]
        public int Count { get; set; }

        // This property defines a switch argument named "Verbose".
        // Non-positional arguments whose type is "bool" act as a switch; if they are supplied on the command line, their value will be true, otherwise
        // it will be false. You don't need to specify a value, just specify "-Verbose" to set it to true. You can explicitly set the value by
        // using "-Verbose:true" or "-Verbose:false" if you want, but it is not needed.
        // If you give an argument the type bool?, it will be true if present, null if omitted, and false only when explicitly set to false using "-Verbose:false"
        // This argument has an alias, so it can also be specified using "-v" instead of its regular name. An argument can have multiple aliases by specifying
        // the Alias attribute more than once.
        [CommandLineArgument, Alias("v"), Description("Print verbose information; this is an example of a switch argument.")]
        public bool Verbose { get; set; }

        // This property defines a multi-value argument named "Value". Its name is specified explicitly so it differs from the property name.
        // A multi-value argument can be specified multiple times. Every time it is specified, the value will be added to the array.
        // To set multiple values, simply repeat the argument, e.g. "-Value foo -Value bar -Value baz" will set it to an array containing { "foo", "bar", "baz" }
        // Since no default value is specified, the property will be null if -Value is not supplied at all.
        // Multi-value arguments can be created either using a property of an array type, or using a read-only property of any collection type (e.g. List<T>).
        // The element type doesn't have to be a string. Any type that can be used for normal arguments can also be used for multi-value arguments.
        [CommandLineArgument("Value"), Description("This is an example of a multi-value argument, which can be repeated multiple times to set more than one value.")]
        public string[] Values { get; set; }

        // This property defines a switch argument named "Help", with the alias "?".
        // For this argument, we handle the CommandLineParser.ArgumentParsed event to cancel
        // command line processing when this argument is supplied. That way, we can print usage regardless of what other arguments are
        // present. For more details, see the CommandLineParser.ArgumentParser event handler in Program.cs
        [CommandLineHelpArgument, Alias("?"), Description("Displays this help message.")]
        public bool Help { get; set; }

        public static ProgramArguments Create(string[] args)
        {
            // Using a static creation function for a command line arguments class is not required, but it's a convenient
            // way to place all command-line related functionality in one place. To parse the arguments (eg. from the Main method)
            // you then only need to call this function.
            CommandLineParser parser = new CommandLineParser(typeof(ProgramArguments));
            ProgramArguments result = null;
            bool showFullHelp = false;
            try
            {
                // The Parse function returns null only when the ArgumentParsed event handler cancelled parsing.
                result = (ProgramArguments)parser.Parse(args);
                showFullHelp = result.Help;
                if (result.Help)
                {
                    showFullHelp = false;
                    switch (result.Command)
                    {
                        case Command.Add:
                            Console.WriteLine("Help specific to Add command here.");
                            break;
                        case Command.Delete:
                            Console.WriteLine("Help specific to Delete command here.");
                            break;
                        case Command.Update:
                            Console.WriteLine("Help specific to Update command here.");
                            break;

                        default:
                            showFullHelp = true;
                            break;
                    }

                }
            }
            catch( CommandLineArgumentException ex )
            {
                // We use the LineWrappingTextWriter to neatly wrap console output.
                using( LineWrappingTextWriter writer = LineWrappingTextWriter.ForConsoleError() )
                {
                    // Tell the user what went wrong.
                    writer.WriteLine(ex.Message);
                    writer.WriteLine();
                }
            }

            if (showFullHelp)
            {
                // If we got here, we should print usage information to the console.
                // By default, aliases and default values are not included in the usage descriptions; for this sample, I do want to include them.
                WriteUsageOptions options = new WriteUsageOptions() { IncludeDefaultValueInDescription = true, IncludeAliasInDescription = true };
                // WriteUsageToConsole automatically uses a LineWrappingTextWriter to properly word-wrap the text.
                parser.WriteUsageToConsole(options);
            }
            return result;
        }
        public static ProgramArguments Create_old(string[] args)
        {
            // Using a static creation function for a command line arguments class is not required, but it's a convenient
            // way to place all command-line related functionality in one place. To parse the arguments (eg. from the Main method)
            // you then only need to call this function.
            CommandLineParser parser = new CommandLineParser(typeof(ProgramArguments));
            // The ArgumentParsed event is used by this sample to stop parsing after the -Help argument is specified.
            parser.ArgumentParsed += CommandLineParser_ArgumentParsed;
            try
            {
                // The Parse function returns null only when the ArgumentParsed event handler cancelled parsing.
                ProgramArguments result = (ProgramArguments)parser.Parse(args);
                if (result != null)
                    return result;
            }
            catch (CommandLineArgumentException ex)
            {
                // We use the LineWrappingTextWriter to neatly wrap console output.
                using (LineWrappingTextWriter writer = LineWrappingTextWriter.ForConsoleError())
                {
                    // Tell the user what went wrong.
                    writer.WriteLine(ex.Message);
                    writer.WriteLine();
                }
            }

            // If we got here, we should print usage information to the console.
            // By default, aliases and default values are not included in the usage descriptions; for this sample, I do want to include them.
            WriteUsageOptions options = new WriteUsageOptions() { IncludeDefaultValueInDescription = true, IncludeAliasInDescription = true };
            // WriteUsageToConsole automatically uses a LineWrappingTextWriter to properly word-wrap the text.
            parser.WriteUsageToConsole(options);
            return null;
        }

        private static void CommandLineParser_ArgumentParsed(object sender, ArgumentParsedEventArgs e)
        {
            // When the -Help argument (or -? using its alias) is specified, parsing is immediately cancelled. That way, CommandLineParser.Parse will
            // return null, and the Create method will display usage even if the correct number of positional arguments was supplied.
            // Try it: just call the sample with "CommandLineSampleCS.exe foo bar -Help", which will print usage even though both the Source and Destination
            // arguments are supplied.
            if( e.Argument.ArgumentName == "Help" ) // The name is always Help even if the alias was used to specify the argument
                e.Cancel = true;
        }
    }
}
