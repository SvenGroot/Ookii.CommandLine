using Ookii.CommandLine;
using Ookii.CommandLine.Validation;
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
//
// The GeneratedParserAttribute indicates this class uses source generation, building the parser at
// compile time instead of during runtime. This gives us improved performance, some additional
// features, and compile-time errors and warnings. Arguments classes that use the
// GeneratedParserAttribute must be partial.
[GeneratedParser]
[ApplicationFriendlyName("Ookii.CommandLine Sample")]
[Description("Sample command line application. The application parses the command line and prints the results, but otherwise does nothing and none of the arguments are actually used for anything.")]
partial class ProgramArguments
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
    [CommandLineArgument(IsPositional = true)]
    [Description("The source data.")]
    public required string Source { get; set; }

    // If not using .Net 7 and C# 11 or later, the required keyword is not available. In that case,
    // use the following to create a required argument:
    // [CommandLineArgument(IsRequired = true, IsPositional = true)]
    // [Description("The source data.")]
    // public string? Source { get; set; }


    // This property defines a required positional argument called "-Destination".
    [CommandLineArgument(IsPositional = true)]
    [Description("The destination data.")]
    public required string Destination { get; set; }

    // This property defines a optional positional argument called "-OperationIndex". If the
    // argument is not supplied, this property will be set to the default value 1. This default
    // value will also be shown in the usage help.
    //
    // The argument's type is "int", so only valid integer values will be accepted. Anything else
    // will cause an error.
    //
    // For types other than string, Ookii.CommandLine can use any type with a public static Parse
    // method (preferably ISpanParsable<T> in .Net 7), or with a constructor that takes a string.
    [CommandLineArgument(IsPositional = true)]
    [Description("The operation's index.")]
    public int OperationIndex { get; set; } = 1;

    // This property defines an argument named "-Date". This argument is not positional, so it can
    // only be supplied by name, for example as "-Date 1969-07-20".
    //
    // This argument uses a nullable value type so it will be set to null if the value is not
    // supplied, rather than having to choose a default value. Since there is no default value, the
    // CommandLineParser won't set this property at all if the argument is not supplied.
    //
    // The conversion from string to DateTime is culture sensitive. The CommandLineParser
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
    [CommandLineArgument]
    [ValueDescription("Number")]
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
    //
    // Any unique prefix of an argument name or alias is also an alias, unless
    // ParseOptions.AutoPrefixAliases is set to false. The prefix "v", however, is not unique, since
    // it could be for either "-Verbose" or "-Version", so it won't work unless specifically added
    // as an alias. However, e.g. "-Verb" will work as an alias automatically.
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
}
