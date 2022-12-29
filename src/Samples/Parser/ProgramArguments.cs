using Ookii.CommandLine;
using Ookii.CommandLine.Validation;
using System;
using System.ComponentModel;

namespace ParserSample;

// This class defines the arguments for the sample. Its properties determine what arguments are
// accepted.
//
// In addition to the arguments defined here, the sample will also have the automatic "-Help" and
// "-Version" arguments.
//
// We add a friendly name for the application, used by the "-Version" argument, and a description
// used when displaying usage help.
[ApplicationFriendlyName("Ookii.CommandLine Sample")]
[Description("Sample command line application. The application parses the command line and prints the results, but otherwise does nothing and none of the arguments are actually used for anything.")]
class ProgramArguments
{
    // This property defines a required positional argument called "-Source".
    //
    // It can be set by name as e.g. "-Source value", or by position by supplying "value" as the
    // first positional argument.
    //
    // By default, argument names are case insensitive, so you can also use "-source value".
    //
    // On Windows, you can also use "/Source", but on other platforms only "-Source" works by
    // default.
    //
    // This argument is required, so if it is not supplied, an error message is printed and parsing
    // fails.
    //
    // We add a description that will be shown when displaying usage help.
    [CommandLineArgument(Position = 0, IsRequired = true)]
    [Description("The source data.")]
    public string? Source { get; set; }

    // This property defines a required positional argument called "-Destination".
    [CommandLineArgument(Position = 1, IsRequired = true)]
    [Description("The destination data.")]
    public string? Destination { get; set; }

    // This property defines a optional positional argument called "-OperationIndex". If the argument
    // is not supplied, this property will be set to the default value 1.
    //
    // The argument's type is "int", so only valid integer values will be accepted. Anything else
    // will cause an error.
    //
    // For types other than string, CommandLineParser will use the TypeConverter for the argument's
    // type to try to convert the string to the correct type. It can also convert types with a
    // public static Parse method, or with a constructor that takes a string.
    [CommandLineArgument(Position = 2, DefaultValue = 1)]
    [Description("The operation's index.")]
    public int OperationIndex { get; set; }

    // This property defines an argument named "-Date". This argument is not positional, so it can
    // only be supplied by name, for example as "-Date 1969-07-20".
    //
    // This argument uses a nullable value type so it will be set to null if the value is not
    // supplied, rather than having to choose a default value. Since there is no default value, the
    // CommandLineParser won't set this property at all if the argument is not supplied.
    //
    // The type conversion from string to DateTime is culture sensitive. The CommandLineParser
    // defaults to CultureInfo.InvariantCulture to ensure a consistent experience regardless of the
    // user's culture, though you can change that if you want.
    [CommandLineArgument]
    [Description("Provides a date to the application.")]
    public DateTime? Date { get; set; }

    // This property defines an argument named "-Count".
    //
    // This argument uses a custom ValueDescription so it shows up as "-Count <Number>" in the usage
    // help rather than as "-Count <Int32>".
    //
    // It uses a validator that ensures the value is within the specified range. The usage help will
    // show that requirement as well.
    [CommandLineArgument(ValueDescription = "Number")]
    [Description("Provides the count for something to the application.")]
    [ValidateRange(0, 100)]
    public int Count { get; set; }

    // This property defines a switch argument named "-Verbose".
    //
    // Non-positional arguments whose type is "bool" act as a switch; if they are supplied on the
    // command line, their value will be true, otherwise it will be false. Supply it with
    // "-Verbose". You can also explicitly set the value by using "-Verbose:true" or
    // "-Verbose:false" if you want.
    //
    // These kinds of arguments are sometimes also called flags.
    //
    // If you give an argument the type bool?, it will be true if present, null if omitted, and false
    // only when explicitly set to false using "-Verbose:false".
    //
    // This argument has an alias, so it can also be specified using "-v" instead of its regular
    // name. An argument can have multiple aliases by specifying the Alias attribute more than once.
    [CommandLineArgument]
    [Description("Print verbose information; this is an example of a switch argument.")]
    [Alias("v")]
    public bool Verbose { get; set; }

    // This property defines a multi-value argument named "-Value". Its name is specified explicitly,
    // so it differs from the property name.
    //
    // A multi-value argument can be specified multiple times. Every time it is specified, the value
    // will be added to the array.
    //
    // To set multiple values, simply repeat the argument, e.g. if you use "-Value foo -Value bar
    // -Value baz" the result will be an array containing { "foo", "bar", "baz" }.
    //
    // As with any other argument, you can use any type for the array's element type, as long as it
    // can be converted from a string.
    [CommandLineArgument("Value")]
    [Description("This is an example of a multi-value argument, which can be repeated multiple times to set more than one value.")]
    public string[]? Values { get; set; }

    // This property defines an argument named "-Day", whose accepted values are the values of the
    // DayOfWeek enumeration, so you can use for example "-day monday". This works with any
    // enumeration. String values that are not defined in the enumeration will cause an error.
    //
    // The string conversion for enumerations also allows the underlying values, so "-day 1" also
    // means Monday. This doesn't filter for defined values, so "-day 9" would be accepted even
    // though DayOfWeek has no member with the value 9. The ValidateEnumValueAttribute makes sure
    // that only defined enum values are allowed. As a bonus, it also adds all the possible values
    // to the usage help.
    [CommandLineArgument]
    [Description("This is an argument using an enumeration type.")]
    [ValidateEnumValue]
    public DayOfWeek? Day { get; set; }

    // Using a static creation function for a command line arguments class is not required, but it's
    // a convenient way to place all command-line related functionality in one file. To parse the
    // arguments (eg. from the Main method) you only need to call this function.
    public static ProgramArguments? Parse()
    {
        // Many aspects of the parsing behavior and usage help generation can be customized using
        // the ParseOptions. You can also use the ParseOptionsAttribute for some of them (see the
        // LongShort sample for an example of that).
        var options = new ParseOptions()
        {
            // If you have a lot of arguments, showing full help if there's a parsing error can make
            // the error message hard to spot. We set it to show syntax only here, and require the
            // use of the "-Help" argument for full help.
            ShowUsageOnError = UsageHelpRequest.SyntaxOnly,
            // By default, repeating an argument more than once (except for multi-value arguments),
            // causes an error. By changing this option, we set it to show a warning instead, and
            // use the last value supplied.
            DuplicateArguments = ErrorMode.Warning,
        };

        // The static Parse method parses the arguments, handles errors, and shows usage help if
        // necessary (using a LineWrappingTextWriter to neatly white-space wrap console output).
        // 
        // It takes the arguments from Environment.GetCommandLineArgs(), but also has an overload
        // that takes a string[] array, if you prefer.
        //
        // If you want more control over parsing and error handling, you can create an instance of
        // the CommandLineParser<T> class. See docs/ParsingArguments.md for an example of that.
        return CommandLineParser.Parse<ProgramArguments>(options);
    }
}
