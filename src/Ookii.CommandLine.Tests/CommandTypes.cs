using Ookii.CommandLine.Commands;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable OCL0033,OCL0034,OCL0040

namespace Ookii.CommandLine.Tests;

[GeneratedCommandManager]
partial class GeneratedManager { }

[GeneratedCommandManager(AssemblyNames = new[] { "Ookii.CommandLine.Tests.Commands" })]
partial class GeneratedManagerWithExplicitAssembly { }

// Also tests using identity instead of name.
[GeneratedCommandManager(AssemblyNames = new[] { "Ookii.CommandLine.Tests", "Ookii.CommandLine.Tests.Commands, Version=1.0.0.0, Culture=neutral, PublicKeyToken=0c15020868fd6249" })]
partial class GeneratedManagerWithMultipleAssemblies { }

[GeneratedParser]
[Command("test")]
[Description("Test command description.")]
public partial class TestCommand : ICommand
{
    [CommandLineArgument]
    public string? Argument { get; set; }

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
    public void Parse(ReadOnlyMemory<string> args, CommandManager manager)
    {
        Value = args.Span[0];
    }

    public string? Value { get; set; }

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
class AsyncCommand : IAsyncCommand
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

[Command(IsHidden = true)]
[Description("Async command description.")]
class AsyncCancelableCommand : AsyncCommandBase
{
    [CommandLineArgument(Position = 0)]
    [Description("Argument description.")]
    public int Value { get; set; }

    public override async Task<int> RunAsync()
    {
        await Task.Delay(Value, CancellationToken);
        return Value;
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

[Command(IsHidden = true)]
[Description("Parent command description.")]
class TestParentCommand : ParentCommand
{
}

[GeneratedParser]
[Command]
[ParentCommand(typeof(TestParentCommand))]
partial class TestChildCommand : ICommand
{
    [CommandLineArgument]
    public int Value { get; set; }

    public int Run() => Value;
}

[GeneratedParser]
[Command]
[ParentCommand(typeof(TestParentCommand))]
partial class OtherTestChildCommand : ICommand
{
    public int Run() => throw new NotImplementedException();
}

[Command]
[ParentCommand(typeof(TestParentCommand))]
[Description("Other parent command description.")]
class NestedParentCommand : ParentCommand
{
}


[GeneratedParser]
[Command]
[ParentCommand(typeof(NestedParentCommand))]
partial class NestedParentChildCommand : ICommand
{
    public int Run() => throw new NotImplementedException();
}
