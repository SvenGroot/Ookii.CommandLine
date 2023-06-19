using Ookii.CommandLine;
using ParserSample;

// Many aspects of the parsing behavior and usage help generation can be customized using the
// ParseOptions. You can also use the ParseOptionsAttribute for some of them (see the LongShort
// sample for an example of that).
var options = new ParseOptions()
{
    // By default, repeating an argument more than once (except for multi-value arguments), causes
    // an error. By changing this option, we set it to show a warning instead, and use the last
    // value supplied.
    DuplicateArguments = ErrorMode.Warning,
};

// The GeneratedParserAttribute adds a static Parse method to your class, which parses the
// arguments, handles errors, and shows usage help if necessary (using a LineWrappingTextWriter to
// neatly white-space wrap console output).
// 
// It takes the arguments from Environment.GetCommandLineArgs(), but also has an overload
// that takes a string[] array, if you prefer.
//
// If you want more control over parsing and error handling, you can create an instance of
// the CommandLineParser<T> class. See docs/ParsingArguments.md for an example of that.
var arguments = ProgramArguments.Parse(options);

// No need to do anything when the value is null; Parse() already printed errors and
// usage help to the console. We return a non-zero value to indicate failure.
if (arguments == null)
{
    return 1;
}

// We use the LineWrappingTextWriter to neatly white-space wrap console output.
using var writer = LineWrappingTextWriter.ForConsoleOut();

// Print the values of the arguments.
writer.WriteLine("The following argument values were provided:");
writer.WriteLine($"Source: {arguments.Source}");
writer.WriteLine($"Destination: {arguments.Destination}");
writer.WriteLine($"OperationIndex: {arguments.OperationIndex}");
writer.WriteLine($"Date: {arguments.Date?.ToString() ?? "(null)"}");
writer.WriteLine($"Count: {arguments.Count}");
writer.WriteLine($"Verbose: {arguments.Verbose}");
var values = arguments.Values == null ? "(null)" : "{ " + string.Join(", ", arguments.Values) + " }";
writer.WriteLine($"Values: {values}");
writer.WriteLine($"Day: {arguments.Day?.ToString() ?? "(null)"}");

return 0;
