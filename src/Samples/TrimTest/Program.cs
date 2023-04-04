// See https://aka.ms/new-console-template for more information
using Ookii.CommandLine;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Support;
using Ookii.CommandLine.Validation;

var arguments = Arguments.Parse();
if (arguments != null)
{
    Console.WriteLine($"Hello, World! {arguments.Test}");
}

[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    public string? Test { get; set; }

    [CommandLineArgument(ValueDescription = "Stuff")]
    public Dictionary<string, string?>? Test2 { get; set; } = default!;

    [CommandLineArgument]
    public int Test3 { get; set; }

    [CommandLineArgument]
    public int? Test4 { get; set; }
}
