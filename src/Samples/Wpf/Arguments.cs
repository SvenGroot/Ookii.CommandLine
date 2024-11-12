using Ookii.CommandLine;
using Ookii.CommandLine.Validation;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.Reflection;

namespace WpfSample;

// This class defines the arguments for the sample. It uses the same arguments as the Parser
// sample, so see that sample for more detailed descriptions.
[GeneratedParser]
[ApplicationFriendlyName("Ookii.CommandLine WPF Sample")]
[Description("Sample command line application for WPF. The application parses the command line and shows the results, but otherwise does nothing and none of the arguments are actually used for anything.")]
public partial class Arguments
{
    // The automatic version argument writes to the console, which is not useful in a WPF
    // application. Instead, we define our own, which shows the same information in a dialog.
    // Because we have an argument named "-Version", the automatic one won't get added.
    //
    // This is an example of a method argument. It uses a method instead of a property, and gets
    // invoked as soon as the argument is parsed, not waiting for all the arguments.
    //
    // A method argument can set its type using a parameter. If there isn't a parameter like that
    // (as in this case), it defaults to a switch argument.
    [CommandLineArgument]
    [Description("Displays version information.")]
    public static CancelMode Version(CommandLineParser parser)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Use the StringProvider to get the same information the automatic version command uses.
        var version = parser.StringProvider.ApplicationNameAndVersion(assembly, parser.ApplicationFriendlyName);
        var copyright = parser.StringProvider.ApplicationCopyright(assembly);

        // Show it using a task dialog (from Ookii.Dialogs).
        var dialog = new TaskDialog()
        {
            WindowTitle = parser.ApplicationFriendlyName,
            MainInstruction = version,
            Content = copyright,
        };

        dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
        dialog.Show();

        // Indicate parsing should be canceled and the application should exit. Because we didn't
        // return CancelMode.AbortWithHelp, usage help will not be shown.
        return CancelMode.Abort;
    }

    [CommandLineArgument(IsPositional = true)]
    [Description("The source data.")]
    public required string Source { get; set; }

    [CommandLineArgument(IsPositional = true)]
    [Description("The destination data.")]
    public required string Destination { get; set; }

    [CommandLineArgument(IsPositional = true)]
    [Description("The operation's index.")]
    public int OperationIndex { get; set; } = 1;

    [CommandLineArgument]
    [Description("Provides a date to the application.")]
    public DateTime? Date { get; set; }

    [CommandLineArgument]
    [ValueDescription("Number")]
    [Description("Provides the count for something to the application.")]
    [ValidateRange(0, 100)]
    public int Count { get; set; }

    [CommandLineArgument]
    [Description("Print verbose information; this is an example of a switch argument.")]
    [Alias("v")]
    public bool Verbose { get; set; }

    [CommandLineArgument("Value")]
    [Description("This is an example of a multi-value argument, which can be repeated multiple times to set more than one value.")]
    public string[]? Values { get; set; }

    [CommandLineArgument]
    [Description("This is an argument using an enumeration type.")]
    [ValidateEnumValue]
    public DayOfWeek? Day { get; set; }
}
