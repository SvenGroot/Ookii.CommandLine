using Ookii.CommandLine;
using Ookii.CommandLine.Validation;
using System.ComponentModel;

namespace LongShort;

// This class defines the arguments for the sample. It uses the same arguments as the Parser
// sample, so see that sample for more detailed descriptions.
//
// We use the ParseOptionsAttribute attribute to customize the behavior here, instead of passing
// ParseOptions to the Parse() method.
//
// This sample uses the alternate long/short parsing mode, transforms argument and value description
// names to dash-case, and uses case sensitive argument name matching. The IsPosix property sets all
// these behaviors for convenience. This makes the behavior similar to POSIX conventions for command
// line arguments.
//
// The name transformation is applied to all automatically derived names, but also to the names
// of the automatic help and version argument, which are now called "--help" and "--version".
[GeneratedParser]
[ApplicationFriendlyName("Ookii.CommandLine Long/Short Mode Sample")]
[Description("Sample command line application using long/short parsing mode. The application parses the command line and prints the results, but otherwise does nothing and none of the arguments are actually used for anything.")]
[ParseOptions(IsPosix = true, DuplicateArguments = ErrorMode.Warning)]
partial class ProgramArguments
{
    // This argument has a short name, derived from the first letter of its long name. The long
    // name is "--source", and the short name is "-s".
    [CommandLineArgument(IsPositional = true, IsShort = true)]
    [Description("The source data.")]
    public required string Source { get; set; }

    // Similarly, this argument has a long name "--destination", and a short name "-d".
    [CommandLineArgument(IsPositional = true, IsShort = true)]
    [Description("The destination data.")]
    public required string Destination { get; set; }

    // This argument does not have a short name. Its long name is "--operation-index".
    [CommandLineArgument(IsPositional = true)]
    [Description("The operation's index.")]
    public int OperationIndex { get; set; } = 1;

    // This argument has the long name "--date" and the short name "-D", explicitly specified to
    // make it uppercase, and distinguish it from the lower case "-d" for "--destination".
    [CommandLineArgument(ShortName = 'D')]
    [Description("Provides a date to the application.")]
    public DateTime? Date { get; set; }

    // This argument's long name is "--day", and it does not have a short name.
    [CommandLineArgument]
    [Description("This is an argument using an enumeration type.")]
    [ValidateEnumValue]
    public DayOfWeek? Day { get; set; }

    // This argument's long name is "--count", with the short name "-c".
    [CommandLineArgument(IsShort = true)]
    [Description("Provides the count for something to the application.")]
    [ValidateRange(0, 100)]
    public int Count { get; set; }

    // This argument's long name is "--verbose", with the short name "-v".
    //
    // Instead of the alias used in the Parser samples, this argument now has a short name. Note
    // that you can still use aliases in LongShort mode. Long name aliases are given with the
    // AliasAttribute, and short name aliases with the ShortAliasAttribute. Automatic prefix
    // aliases work for the long names of arguments.
    [CommandLineArgument(IsShort = true)]
    [Description("Print verbose information; this is an example of a switch argument.")]
    public bool Verbose { get; set; }

    // Another switch argument, called "--process" with the short name "-p". Switch arguments with
    // short names can be combined; for example, "-vp" sets both the verbose and process switch
    // (this only works for switch arguments).
    [CommandLineArgument(IsShort = true)]
    [Description("Does the processing.")]
    public bool Process { get; set; }

    // This argument's long name is "--value", with no short name.
    //
    // In another change from the Parser sample, this argument uses the
    // MultiValueSeparatorAttribute, which means you can supply multiple values like "--value foo
    // bar baz". All following argument tokens are consumed until another name is found (this also
    // works in regular parsing mode).
    //
    // The name here is explicitly specified, and explicit names aren't subject to the NameTransform,
    // so it must be explicitly given as lower case to match the other arguments.
    [CommandLineArgument("value")]
    [Description("This is an example of a multi-value argument, which can be repeated multiple times to set more than one value.")]
    [MultiValueSeparator]
    public string[]? Values { get; set; }
}
