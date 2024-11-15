using Categories;
using Ookii.CommandLine;

var options = new ParseOptions()
{
    UsageWriter = new UsageWriter()
    {
        // Because this application has so many arguments, we'll use abbreviated syntax to make the
        // usage help look cleaner.
        UseAbbreviatedSyntax = true,
    },
    // Add a default value description for integer arguments.
    DefaultValueDescriptions = new Dictionary<Type, string>()
    {
        { typeof(int), "Number" }
    }
};

var arguments = Arguments.Parse(options);
if (arguments == null)
{
    return 1;
}

Console.WriteLine($"This is a sample of how to use categories in the usage help. Please run '{CommandLineParser.GetExecutableName()} -Help' to show the help.");
return 0;
