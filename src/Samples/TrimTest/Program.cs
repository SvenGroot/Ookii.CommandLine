// See https://aka.ms/new-console-template for more information
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Support;
using Ookii.CommandLine.Validation;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;

var manager = new TestManager();
return manager.RunCommand() ?? 1;

//var arguments = Arguments.Parse();
//if (arguments != null)
//{
//    Console.WriteLine($"Hello, World! {arguments.Test}");
//}


[GeneratedCommandManager]
partial class TestManager { }

[GeneratedParser]
[ParseOptions(CaseSensitive = true)]
[Description("This is a test")]
[ApplicationFriendlyName("Trim Test")]
[RequiresAny(nameof(Test), nameof(Test2))]
[Command]
partial class Arguments : ICommand
{
    [CommandLineArgument(Position = 0)]
    [Description("Test argument")]
    [Alias("t")]
    [ValidateNotEmpty]
    public string? Test { get; set; }

    [CommandLineArgument(Position = 1)]
    [ValueDescription("Stuff")]
    [KeyValueSeparator("==")]
    [MultiValueSeparator]
    public Dictionary<int, string?> Test2 { get; set; } = default!;

    [CommandLineArgument]
    public int Test3 { get; set; }

    [CommandLineArgument]
    public int? Test4 { get; set; }

    [CommandLineArgument]
    public FileInfo[]? File { get; set; }

    [CommandLineArgument]
    public IPAddress? Ip { get; set; }

    [CommandLineArgument]
    public IDictionary<string, int> Arg14 { get; } = new SortedDictionary<string, int>();

    [CommandLineArgument]
    public static void Foo(CommandLineParser p)
    {
    }

    public int Run()
    {
        Console.WriteLine("Hello");
        return 0;
    }
}
