// See https://aka.ms/new-console-template for more information
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Support;
using Ookii.CommandLine.Validation;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;

var manager = TestProvider.CreateCommandManager();
return manager.RunCommand() ?? 1;

//var arguments = Arguments.Parse();
//if (arguments != null)
//{
//    Console.WriteLine($"Hello, World! {arguments.Test}");
//}

class MyProvider : CommandProvider
{
    public override string? GetApplicationDescription() => "Trim Test";
    public override IEnumerable<CommandInfo> GetCommandsUnsorted(CommandManager manager)
    {
        yield return new GeneratedCommandInfo(manager, typeof(Arguments), new CommandAttribute(), new DescriptionAttribute("This is a command test"), createParser: options => Arguments.CreateParser(options));
    }
}

[GeneratedCommandProvider]
partial class TestProvider { }

[GeneratedParser]
[ParseOptions(CaseSensitive = true)]
[Description("This is a test")]
[ApplicationFriendlyName("Trim Test")]
[RequiresAny(nameof(Test), nameof(Test2))]
[Command]
partial class Arguments : ICommand
{
    [CommandLineArgument]
    [Description("Test argument")]
    [Alias("t")]
    [ValidateNotEmpty]
    public string? Test { get; set; }

    [CommandLineArgument]
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
