// See https://aka.ms/new-console-template for more information
using Ookii.CommandLine;
using Ookii.CommandLine.Support;

var parser = new CommandLineParser(new MyProvider());
var arguments = (Arguments?)parser.ParseWithErrorHandling();
if (arguments != null)
{
    Console.WriteLine($"Hello, World! {arguments.Test}");
}


class Arguments
{
    [CommandLineArgument]
    public string? Test { get; set; }
}

class MyProvider : IArgumentProvider
{
    public Type ArgumentsType => typeof(Arguments);

    public string ApplicationFriendlyName => "Test";

    public string Description => string.Empty;

    public ParseOptionsAttribute? OptionsAttribute => null;

    public bool IsCommand => false;

    public object CreateInstance(CommandLineParser parser)
    {
        return new Arguments();
    }
    public IEnumerable<CommandLineArgument> GetArguments(CommandLineParser parser)
    {
        yield return CustomArgument.Create(parser, "Test", typeof(string), (target, value) => ((Arguments)target).Test = (string?)value);
    }
    public void RunValidators(CommandLineParser parser)
    {
    }
}
