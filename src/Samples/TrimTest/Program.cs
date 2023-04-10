// See https://aka.ms/new-console-template for more information
using Ookii.CommandLine;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Support;
using Ookii.CommandLine.Validation;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

var arguments = Arguments.Parse();
if (arguments != null)
{
    Console.WriteLine($"Hello, World! {arguments.Test}");
}

[GeneratedParser]
[ParseOptions(CaseSensitive = true)]
[Description("This is a test")]
[ApplicationFriendlyName("Trim Test")]
[RequiresAny(nameof(Test), nameof(Test2))]
partial class Arguments
{
    [CommandLineArgument]
    [Description("Test argument")]
    [Alias("t")]
    [ValidateNotEmpty]
    public string? Test { get; set; }

    [CommandLineArgument(ValueDescription = "Stuff")]
    [KeyValueSeparator("==")]
    [MultiValueSeparator]
    public Dictionary<int, string?> Test2 { get; set; } = default!;

    [CommandLineArgument]
    public int Test3 { get; set; }

    [CommandLineArgument]
    public int? Test4 { get; set; }
}
