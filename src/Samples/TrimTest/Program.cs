// See https://aka.ms/new-console-template for more information
using Ookii.CommandLine;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Support;
using Ookii.CommandLine.Validation;

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

class MyProvider : GeneratedArgumentProvider
{
    public MyProvider()
        : base(typeof(Arguments), null, Enumerable.Empty<ClassValidationAttribute>(), null, null)
    {
    }

    public override bool IsCommand => false;

    public override object CreateInstance(CommandLineParser parser)
    {
        return new Arguments();
    }

    public override IEnumerable<CommandLineArgument> GetArguments(CommandLineParser parser)
    {
        yield return GeneratedArgument.Create(parser, typeof(string), "Test", new CommandLineArgumentAttribute(),
            new StringConverter(), setProperty: (target, value) => ((Arguments)target).Test = (string?)value);
    }
}
