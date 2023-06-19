using CustomUsage;
using Ookii.CommandLine;

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

var arguments = ProgramArguments.Parse(options);

// No need to do anything when the value is null; Parse() already printed errors and
// usage to the console. We return a non-zero value to indicate failure.
if (arguments == null)
{
    return 1;
}

// We use the LineWrappingTextWriter to neatly wrap console output.
using var writer = LineWrappingTextWriter.ForConsoleOut();

// Print the values of the arguments.
writer.WriteLine("The following argument values were provided:");
writer.WriteLine($"Source: {arguments.Source}");
writer.WriteLine($"Destination: {arguments.Destination}");
writer.WriteLine($"OperationIndex: {arguments.OperationIndex}");
writer.WriteLine($"Date: {arguments.Date?.ToString() ?? "(null)"}");
writer.WriteLine($"Count: {arguments.Count}");
writer.WriteLine($"Verbose: {arguments.Verbose}");
writer.WriteLine($"Process: {arguments.Process}");
var values = arguments.Values == null ? "(null)" : "{ " + string.Join(", ", arguments.Values) + " }";
writer.WriteLine($"Values: {values}");
writer.WriteLine($"Day: {arguments.Day?.ToString() ?? "(null)"}");

return 0;
