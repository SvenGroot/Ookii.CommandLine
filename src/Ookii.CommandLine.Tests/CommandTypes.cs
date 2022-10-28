using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests
{
    [ShellCommand("test")]
    [Description("Test command description.")]
    public class TestCommand : ShellCommand
    {
        [CommandLineArgument]
        public string Argument { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    [ShellCommand]
    public class AnotherCommand : ShellCommand
    {
        [CommandLineArgument]
        public int Value { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    [ShellCommand("custom", CustomArgumentParsing = true)]
    [Description("Custom parsing command.")]
    internal class CustomParsingCommand : ShellCommand
    {
        public CustomParsingCommand(string[] args, int index, CreateShellCommandOptions options)
        {
            Value = args[index];
        }

        public string Value { get; set; }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    [ShellCommand(IsHidden = true)]
    class HiddenCommand : ShellCommand
    {
        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class NotACommand : ShellCommand
    {
        public override void Run()
        {
            throw new NotImplementedException();
        }
    }
}
