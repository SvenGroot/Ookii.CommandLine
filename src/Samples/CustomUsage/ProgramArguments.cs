using Ookii.CommandLine;
using Ookii.CommandLine.Validation;
using System.ComponentModel;

namespace CustomUsage;

// This class defines the arguments for the sample. It uses the same arguments as the LongShort
// sample, so see that sample for more detailed descriptions.
//
// This sample sets the mode, case sensitivity and name transform to use POSIX conventions.
//
// See the Parse() method below to see how the usage customization is applied.
[GeneratedParser]
[ApplicationFriendlyName("Ookii.CommandLine Long/Short Mode Sample")]
[Description("Sample command line application with highly customized usage help. The application parses the command line and prints the results, but otherwise does nothing and none of the arguments are actually used for anything.")]
[ParseOptions(IsPosix = true, DuplicateArguments = ErrorMode.Warning)]
partial class ProgramArguments
{
    [CommandLineArgument(IsPositional = true, IsShort = true)]
    [Description("The source data.")]
    public required string Source { get; set; }

    [CommandLineArgument(IsPositional = true, IsShort = true)]
    [Description("The destination data.")]
    public required string Destination { get; set; }

    [CommandLineArgument(IsPositional = true)]
    [Description("The operation's index.")]
    public int OperationIndex { get; set; } = 1;

    [CommandLineArgument(ShortName = 'D')]
    [Description("Provides a date to the application.")]
    public DateTime? Date { get; set; }

    [CommandLineArgument]
    [Description("This is an argument using an enumeration type.")]
    [ValidateEnumValue]
    public DayOfWeek? Day { get; set; }

    [CommandLineArgument(IsShort = true)]
    [Description("Provides the count for something to the application.")]
    [ValidateRange(0, 100)]
    public int Count { get; set; }

    [CommandLineArgument(IsShort = true)]
    [Description("Print verbose information; this is an example of a switch argument.")]
    public bool Verbose { get; set; }

    [CommandLineArgument(IsShort = true)]
    [Description("Does the processing.")]
    public bool Process { get; set; }

    [CommandLineArgument("value")]
    [Description("This is an example of a multi-value argument, which can be repeated multiple times to set more than one value.")]
    [MultiValueSeparator]
    public string[]? Values { get; set; }
}
