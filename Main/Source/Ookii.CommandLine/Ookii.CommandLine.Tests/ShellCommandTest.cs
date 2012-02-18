using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Ookii.CommandLine.Tests
{
    #region Shell commands

    [ShellCommand("test")]
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

    public class NotACommand : ShellCommand
    {
        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    [TestClass]
    public class ShellCommandTest
    {
        [TestMethod]
        public void GetShellCommandsTest()
        {
            Type[] commands = ShellCommand.GetShellCommands(Assembly.GetExecutingAssembly());

            Assert.IsNotNull(commands);
            Assert.AreEqual(3, commands.Length);
            CollectionAssert.Contains(commands, typeof(TestCommand));
            CollectionAssert.Contains(commands, typeof(AnotherCommand));
            CollectionAssert.Contains(commands, typeof(CustomParsingCommand));
        }

        [TestMethod]
        public void GetShellCommandTest()
        {
            Type command = ShellCommand.GetShellCommand(Assembly.GetExecutingAssembly(), "test");
            Assert.AreEqual(typeof(TestCommand), command);

            command = ShellCommand.GetShellCommand(Assembly.GetExecutingAssembly(), "wrong");
            Assert.IsNull(command);

            command = ShellCommand.GetShellCommand(Assembly.GetExecutingAssembly(), "Test"); // default is case-insensitive
            Assert.AreEqual(typeof(TestCommand), command);

            command = ShellCommand.GetShellCommand(Assembly.GetExecutingAssembly(), "Test", StringComparer.Ordinal);
            Assert.IsNull(command);

            command = ShellCommand.GetShellCommand(Assembly.GetExecutingAssembly(), "AnotherCommand");
            Assert.AreEqual(typeof(AnotherCommand), command);        
        }

        [TestMethod]
        public void GetShellCommandNameTest()
        {
            string name = ShellCommand.GetShellCommandName(typeof(TestCommand));
            Assert.AreEqual("test", name);

            name = ShellCommand.GetShellCommandName(typeof(AnotherCommand));
            Assert.AreEqual("AnotherCommand", name);
        }

        [TestMethod]
        public void IsShellCommandTest()
        {
            bool isCommand = ShellCommand.IsShellCommand(typeof(TestCommand));
            Assert.IsTrue(isCommand);

            isCommand = ShellCommand.IsShellCommand(typeof(NotACommand));
            Assert.IsFalse(isCommand);
        }

        [TestMethod]
        public void CreateShellCommandTest()
        {
            TestCommand command = (TestCommand)ShellCommand.CreateShellCommand(Assembly.GetExecutingAssembly(), "test", new[] { "-Argument", "Foo" }, 0);
            Assert.IsNotNull(command);
            Assert.AreEqual("Foo", command.Argument);

            command = (TestCommand)ShellCommand.CreateShellCommand(Assembly.GetExecutingAssembly(), new[] { "test", "-Argument", "Bar" }, 0);
            Assert.IsNotNull(command);
            Assert.AreEqual("Bar", command.Argument);

            AnotherCommand command2 = (AnotherCommand)ShellCommand.CreateShellCommand(Assembly.GetExecutingAssembly(), "anothercommand", new[] { "skip", "-Value", "42" }, 1);
            Assert.IsNotNull(command2);
            Assert.AreEqual(42, command2.Value);

            CustomParsingCommand command3 = (CustomParsingCommand)ShellCommand.CreateShellCommand(Assembly.GetExecutingAssembly(), new[] { "custom", "hello" }, 0);
            Assert.IsNotNull(command3);
            Assert.AreEqual("hello", command3.Value);
        }
    }
}
