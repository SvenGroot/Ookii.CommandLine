using Ookii.CommandLine;
using Ookii.CommandLine.Validation;
using System.ComponentModel;

namespace CustomUsage;

// This class defines the arguments for the sample. It uses the same arguments as the LongShort
// sample, so see that sample for more detailed descriptions.
//
// This sample uses the long/short argument mode, and customizes the usage help.
[ApplicationFriendlyName("Ookii.CommandLine Long/Short Mode Sample")]
[Description("Sample command line application with highly customized usage help. The application parses the command line and prints the results, but otherwise does nothing and none of the arguments are actually used for anything.")]
[ParseOptions(Mode = ParsingMode.LongShort,
    ArgumentNameTransform = NameTransform.DashCase,
    ValueDescriptionTransform = NameTransform.DashCase,
    CaseSensitive = true,
    DuplicateArguments = ErrorMode.Warning)]
class ProgramArguments
{
    [CommandLineArgument(Position = 0, IsRequired = true, IsShort = true)]
    [Description("The source data.")]
    public string Source { get; set; } = string.Empty;

    [CommandLineArgument(Position = 1, IsRequired = true, IsShort = true)]
    [Description("The destination data.")]
    public string Destination { get; set; } = string.Empty;

    [CommandLineArgument(DefaultValue = 1)]
    [Description("The operation's index.")]
    public int OperationIndex { get; set; }

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

    public static ProgramArguments? Parse()
    {
        // Not all options can be set with the ParseOptionsAttribute.
        var options = new ParseOptions()
        {
            // Set the value description of all int arguments to "number", instead of doing it
            // separately on each argument.
            DefaultValueDescriptions = new Dictionary<Type, string>()
            {
                { typeof(int), "number" }
            },
            // Use our own string provider and usage writer for the custom usage strings.
            StringProvider = new CustomStringProvider(),
            UsageWriter = new CustomUsageWriter(),
        };

        return CommandLineParser.Parse<ProgramArguments>(options);
    }
}
