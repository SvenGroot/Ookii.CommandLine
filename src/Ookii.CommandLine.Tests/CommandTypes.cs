using Ookii.CommandLine.Commands;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests
{
    [Command("test")]
    [Description("Test command description.")]
    public class TestCommand : ICommand
    {
        [CommandLineArgument]
        public string Argument { get; set; }

        public int Run()
        {
            throw new NotImplementedException();
        }
    }

    [Command]
    [Alias("alias")]
    public class AnotherSimpleCommand : ICommand
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
    internal class CustomParsingCommand : ICommandWithCustomParsing
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

    [Command(IsHidden = true)]
    class HiddenCommand : ICommand
    {
        public int Run()
        {
            throw new NotImplementedException();
        }
    }

    // Hidden so I don't have to update the expected usage.
    [Command(IsHidden = true)]
    [Description("Async command description.")]
    class AsyncCommand : IAsyncCommand
    {
        [CommandLineArgument(Position = 0)]
        [Description("Argument description.")]
        public int Value { get; set; }

        public int Run()
        {
            // Do somehting different than RunAsync so the test can differentiate which one was
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
            await Task.Delay(100);
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
}
