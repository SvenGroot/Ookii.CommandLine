// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Ookii.CommandLine.Tests
{
    [TestClass]
    public class ShellCommandTest
    {
        private static readonly Assembly _commandAssembly = Assembly.GetExecutingAssembly();

        [TestMethod]
        public void GetShellCommandsTest()
        {
            var commands = ShellCommand.GetShellCommands(_commandAssembly).ToArray();

            Assert.IsNotNull(commands);
            Assert.AreEqual(5, commands.Length);
            Assert.AreEqual("AnotherCommand", commands[0].Name);
            Assert.AreEqual(typeof(AnotherCommand), commands[0].CommandType);
            Assert.IsFalse(commands[0].CustomArgumentParsing);
            Assert.AreEqual("custom", commands[1].Name);
            Assert.AreEqual(typeof(CustomParsingCommand), commands[1].CommandType);
            Assert.IsTrue(commands[1].CustomArgumentParsing);
            Assert.AreEqual("HiddenCommand", commands[2].Name);
            Assert.IsFalse(commands[2].CustomArgumentParsing);
            Assert.AreEqual(typeof(HiddenCommand), commands[2].CommandType);
            Assert.AreEqual("test", commands[3].Name);
            Assert.IsFalse(commands[3].CustomArgumentParsing);
            Assert.AreEqual(typeof(TestCommand), commands[3].CommandType);
            Assert.AreEqual("version", commands[4].Name);
            Assert.IsFalse(commands[4].CustomArgumentParsing);
        }

        [TestMethod]
        public void GetShellCommandTest()
        {
            var command = ShellCommand.GetShellCommand(_commandAssembly, "test");
            Assert.IsNotNull(command);
            Assert.AreEqual("test", command.Value.Name);
            Assert.AreEqual(typeof(TestCommand), command.Value.CommandType);

            command = ShellCommand.GetShellCommand(_commandAssembly, "wrong");
            Assert.IsNull(command);

            command = ShellCommand.GetShellCommand(_commandAssembly, "Test"); // default is case-insensitive
            Assert.IsNotNull(command);
            Assert.AreEqual("test", command.Value.Name);
            Assert.AreEqual(typeof(TestCommand), command.Value.CommandType);

            command = ShellCommand.GetShellCommand(_commandAssembly, "Test", StringComparer.Ordinal);
            Assert.IsNull(command);

            command = ShellCommand.GetShellCommand(_commandAssembly, "AnotherCommand");
            Assert.IsNotNull(command);
            Assert.AreEqual("AnotherCommand", command.Value.Name);
            Assert.AreEqual(typeof(AnotherCommand), command.Value.CommandType);
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
            using var writer = LineWrappingTextWriter.ForStringWriter(0);
            var options = new CreateShellCommandOptions()
            {
                Out = writer,
                Error = writer,
                UsageOptions = new WriteUsageOptions()
                {
                    ExecutableName = _executableName,
                }
            };

            TestCommand command = (TestCommand)ShellCommand.CreateShellCommand(_commandAssembly, "test", new[] { "-Argument", "Foo" }, 0, options);
            Assert.IsNotNull(command);
            Assert.AreEqual("Foo", command.Argument);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            command = (TestCommand)ShellCommand.CreateShellCommand(_commandAssembly, new[] { "test", "-Argument", "Bar" }, 0, options);
            Assert.IsNotNull(command);
            Assert.AreEqual("Bar", command.Argument);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            AnotherCommand command2 = (AnotherCommand)ShellCommand.CreateShellCommand(_commandAssembly, "anothercommand", new[] { "skip", "-Value", "42" }, 1, options);
            Assert.IsNotNull(command2);
            Assert.AreEqual(42, command2.Value);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            CustomParsingCommand command3 = (CustomParsingCommand)ShellCommand.CreateShellCommand(_commandAssembly, new[] { "custom", "hello" }, 0, options);
            Assert.IsNotNull(command3);
            Assert.AreEqual("hello", command3.Value);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            var versionCommand = ShellCommand.CreateShellCommand(_commandAssembly, new[] { "version" }, 0, options);
            Assert.IsNotNull(versionCommand);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            options.AutoVersionCommand = false;
            versionCommand = ShellCommand.CreateShellCommand(_commandAssembly, new[] { "version" }, 0, options);
            Assert.IsNull(versionCommand);
            Assert.AreEqual(_expectedUsageNoVersion, writer.BaseWriter.ToString());

        }

        [TestMethod]
        public void TestWriteUsage()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(0);
            var options = new CreateShellCommandOptions()
            {
                Out = writer,
                Error = writer,
                UsageOptions = new WriteUsageOptions()
                {
                    ExecutableName = _executableName,
                }
            };

            ShellCommand.WriteUsage(_commandAssembly, options);
            Assert.AreEqual(_expectedUsage, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestWriteUsageColor()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(0);
            var options = new CreateShellCommandOptions()
            {
                Out = writer,
                Error = writer,
                UsageOptions = new WriteUsageOptions()
                {
                    ExecutableName = _executableName,
                    UseColor = true,
                }
            };

            ShellCommand.WriteUsage(_commandAssembly, options);
            Assert.AreEqual(_expectedUsageColor, writer.BaseWriter.ToString());
        }

        #region Expected usage

        private const string _executableName = "test";

        public static readonly string _expectedUsage = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherCommand


    custom
        Custom parsing command.

    test
        Test command description.

    version
        Displays version information.

".ReplaceLineEndings();

        public static readonly string _expectedUsageNoVersion = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherCommand


    custom
        Custom parsing command.

    test
        Test command description.

".ReplaceLineEndings();

        public static readonly string _expectedUsageColor = @"[36mUsage:[0m test <command> [arguments]

The following commands are available:

    [32mAnotherCommand[0m


    [32mcustom[0m
        Custom parsing command.

    [32mtest[0m
        Test command description.

    [32mversion[0m
        Displays version information.

".ReplaceLineEndings();

        #endregion
    }
}
