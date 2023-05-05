using Ookii.CommandLine.Commands;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests;

[GeneratedCommandManager]
partial class GeneratedManager { }

[GeneratedParser]
[Command("test")]
[Description("Test command description.")]
public partial class TestCommand : ICommand
{
    [CommandLineArgument]
    public string Argument { get; set; }

    public int Run()
    {
        throw new NotImplementedException();
    }
}

[GeneratedParser]
[Command]
[Alias("alias")]
public partial class AnotherSimpleCommand : ICommand
{
    [CommandLineArgument]
    [Description("Argument description")]
    public int Value { get; set; }

    public int Run()
    {
        return Value;
    }
}

[Command("custom")]
[Description("Custom parsing command.")]
partial class CustomParsingCommand : ICommandWithCustomParsing
{
    public void Parse(string[] args, int index, CommandOptions options)
    {
        Value = args[index];
    }

    public string Value { get; set; }

    public int Run()
    {
        throw new NotImplementedException();
    }
}

[GeneratedParser]
[Command(IsHidden = true)]
partial class HiddenCommand : ICommand
{
    public int Run()
    {
        throw new NotImplementedException();
    }
}

// Hidden so I don't have to update the expected usage.
// Not generated to test registration of plain commands without generation.
[Command(IsHidden = true)]
[Description("Async command description.")]
partial class AsyncCommand : IAsyncCommand
{
    [CommandLineArgument(Position = 0)]
    [Description("Argument description.")]
    public int Value { get; set; }

    public int Run()
    {
        // Do something different than RunAsync so the test can differentiate which one was
        // called.
        return Value + 1;
    }

    public Task<int> RunAsync()
    {
        return Task.FromResult(Value);
    }
}

// Used in stand-alone test, so not an actual command.
class AsyncBaseCommand : AsyncCommandBase
{
    public override async Task<int> RunAsync()
    {
        // Do something actually async to test the wait in Run().
        await Task.Yield();
        return 42;
    }
}

public class NotACommand : ICommand
{
    public int Run()
    {
        throw new NotImplementedException();
    }
}
