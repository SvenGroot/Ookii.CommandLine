using System;
using System.ComponentModel;
using System.Globalization;
using Ookii.CommandLine;

namespace CommandLineSampleCS
{
    /// <summary>
    /// Class that defines the sample's command line arguments.
    /// </summary>
    [Description("Sample command line application. The application parses the command line and prints the results, but otherwise does nothing and none of the arguments are actually used for anything.")]
    class ProgramArguments
    {
        // This property defines a required positional argument called "Source". It can be set by name as e.g. "-Source value", or by position
        // by specifying "value" as the first positional argument. Note that by default command line argument names are case insensitive, so
        // this argument can also be specified as e.g. "-source value".
        // On Windows, the default command name prefixes are "/" and "-", so the argument can also be specified as e.g. "/Source value". On Unix
        // (using Mono), only "-" is accepted by default.
        [CommandLineArgument(Position = 0, IsRequired = true), Description("The source data.")]
        public string Source { get; set; }

        // This property defines a required positional argument called "Destination". It can be set by name as e.g. "-Destination value", or by position
        // by specifying "value" as the second positional argument.
        [CommandLineArgument(Position = 1, IsRequired = true), Description("The destination data.")]
        public string Destination { get; set; }

        // This property defines a optional positional argument called "Index". It can be set by name as e.g. "-Index 5", or by position
        // by specifying e.g. "5" as the third positional argument. If the argument is not specified, this property
        // will be set to the default value 1.
        [CommandLineArgument(Position = 2, DefaultValue = 1), Description("The operation's index.")]
        public int Index { get; set; }

        // This property defines an argument named "Date". This argument is not positional, so it can be supplied only by name, for example as "-Date 2013-01-31".
        // This argument uses a nullable value type so it will be set to null if the value is not supplied, rather than having to choose a default value.
        // For types other than string, CommandLineParser will use the TypeConverter for the argument's type to try to convert the string to
        // the correct type. You can use your own custom classes or structures for command line arguments as long as you create a TypeConverter for
        // the type.
        // The type conversion from string to DateTime is culture sensitive. Which culture is used is indicated by the CommandLineParser.Culture
        // property, which defaults to the invariant culture. This is preferred to ensure a
        // consistent parsing experience regardless of the user's culture.
        // Always pay attention when a conversion is culture specific (this goes for dates, numbers,
        // and various other types) and consider which culture is the right choice for your
        // application.
        [CommandLineArgument, Description("Provides a date to the application.")]
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
        // For this argument, CancelParsing is set to true so that command line processing is stopped
        // when this argument is supplied. That way, we can print usage regardless of what other arguments are
        // present.
        [CommandLineArgument(CancelParsing = true), Alias("?"), Description("Displays this help message.")]
        public bool Help { get; set; }

        // Using a static creation function for a command line arguments class is not required, but it's a convenient
        // way to place all command-line related functionality in one place. To parse the arguments (eg. from the Main method)
        // you then only need to call this function.
        public static ProgramArguments Create(string[] args)
        {
            var options = new ParseOptions()
            {
                // UsageOptions are used to print usage information if there was an error parsing
                // the command line or parsing was cancelled (by the -Help property above).
                // By default, aliases and default values are not included in the usage descriptions;
                // for this sample, I do want to include them.
                UsageOptions = new WriteUsageOptions()
                {
                    IncludeDefaultValueInDescription = true,
                    IncludeAliasInDescription = true,
                }
            };

            // The static Parse method handles parsing, printing error and usage information
            // (using a LineWrappingTextWriter to neatly wrap console output), and converting to
            // the proper type.
            return CommandLineParser.Parse<ProgramArguments>(args, options);
        }

        // If you want more control over the parsing behavior, you can manually create an instance
        // of the CommandLineParser class and handle errors and usage yourself, as below.
        // This is only necessary if you want to deviate from what the static Parse method does,
        // which is not the case here; this method is only provided for demonstrative purposes.
        public static ProgramArguments CreateCustom(string[] args)
        {
            var parser = new CommandLineParser(typeof(ProgramArguments));

            try
            {
                // The Parse function returns null only when the Help argument cancelled parsing.
                var result = (ProgramArguments)parser.Parse(args);
                if (result != null)
                    return result;
            }
            catch (CommandLineArgumentException ex)
            {
                // We use the LineWrappingTextWriter to neatly wrap console output.
                using (var writer = LineWrappingTextWriter.ForConsoleError())
                {
                    // Tell the user what went wrong.
                    writer.WriteLine(ex.Message);
                    writer.WriteLine();
                }
            }

            // If we got here, we should print usage information to the console.
            // By default, aliases and default values are not included in the usage descriptions; for this sample, I do want to include them.
            var options = new WriteUsageOptions()
            {
                IncludeDefaultValueInDescription = true,
                IncludeAliasInDescription = true
            };

            // WriteUsageToConsole automatically uses a LineWrappingTextWriter to properly word-wrap the text.
            parser.WriteUsageToConsole(options);
            return null;
        }
    }
}
