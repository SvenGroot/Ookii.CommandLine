// Copyright (c) Sven Groot (Ookii.org)
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Commands;
using System.Reflection;

namespace Ookii.CommandLine.Tests
{
    [TestClass]
    public class SubCommandTest
    {
        private static readonly Assembly _commandAssembly = Assembly.GetExecutingAssembly();

        [TestMethod]
        public void GetCommandsTest()
        {
            var manager = new CommandManager(_commandAssembly);
            var commands = manager.GetCommands().ToArray();

            Assert.IsNotNull(commands);
            Assert.AreEqual(5, commands.Length);
            Assert.AreEqual("AnotherSimpleCommand", commands[0].Name);
            Assert.AreEqual(typeof(AnotherSimpleCommand), commands[0].CommandType);
            Assert.IsFalse(commands[0].UseCustomArgumentParsing);
            Assert.AreEqual("custom", commands[1].Name);
            Assert.AreEqual(typeof(CustomParsingCommand), commands[1].CommandType);
            Assert.IsTrue(commands[1].UseCustomArgumentParsing);
            Assert.AreEqual("HiddenCommand", commands[2].Name);
            Assert.IsFalse(commands[2].UseCustomArgumentParsing);
            Assert.AreEqual(typeof(HiddenCommand), commands[2].CommandType);
            Assert.AreEqual("test", commands[3].Name);
            Assert.IsFalse(commands[3].UseCustomArgumentParsing);
            Assert.AreEqual(typeof(TestCommand), commands[3].CommandType);
            Assert.AreEqual("version", commands[4].Name);
            Assert.IsFalse(commands[4].UseCustomArgumentParsing);
        }

        [TestMethod]
        public void GetCommandTest()
        {
            var manager = new CommandManager(_commandAssembly);
            var command = manager.GetCommand("test");
            Assert.IsNotNull(command);
            Assert.AreEqual("test", command.Value.Name);
            Assert.AreEqual(typeof(TestCommand), command.Value.CommandType);

            command = manager.GetCommand("wrong");
            Assert.IsNull(command);

            command = manager.GetCommand("Test"); // default is case-insensitive
            Assert.IsNotNull(command);
            Assert.AreEqual("test", command.Value.Name);
            Assert.AreEqual(typeof(TestCommand), command.Value.CommandType);

            var manager2 = new CommandManager(_commandAssembly, new CommandOptions() { CommandNameComparer = StringComparer.Ordinal });
            command = manager2.GetCommand("Test");
            Assert.IsNull(command);

            command = manager.GetCommand("AnotherSimpleCommand");
            Assert.IsNotNull(command);
            Assert.AreEqual("AnotherSimpleCommand", command.Value.Name);
            Assert.AreEqual(typeof(AnotherSimpleCommand), command.Value.CommandType);
        }

        [TestMethod]
        public void IsCommandTest()
        {
            bool isCommand = CommandInfo.IsCommand(typeof(TestCommand));
            Assert.IsTrue(isCommand);

            isCommand = CommandInfo.IsCommand(typeof(NotACommand));
            Assert.IsFalse(isCommand);
        }

        [TestMethod]
        public void CreateCommandTest()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(0);
            var options = new CommandOptions()
            {
                Out = writer,
                Error = writer,
                UsageOptions = new WriteUsageOptions()
                {
                    ExecutableName = _executableName,
                }
            };

            var manager = new CommandManager(_commandAssembly, options);
            TestCommand command = (TestCommand)manager.CreateCommand("test", new[] { "-Argument", "Foo" }, 0);
            Assert.IsNotNull(command);
            Assert.AreEqual("Foo", command.Argument);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            command = (TestCommand)manager.CreateCommand(new[] { "test", "-Argument", "Bar" });
            Assert.IsNotNull(command);
            Assert.AreEqual("Bar", command.Argument);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            var command2 = (AnotherSimpleCommand)manager.CreateCommand("anothersimplecommand", new[] { "skip", "-Value", "42" }, 1);
            Assert.IsNotNull(command2);
            Assert.AreEqual(42, command2.Value);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            CustomParsingCommand command3 = (CustomParsingCommand)manager.CreateCommand(new[] { "custom", "hello" });
            Assert.IsNotNull(command3);
            Assert.AreEqual("hello", command3.Value);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            var versionCommand = manager.CreateCommand(new[] { "version" });
            Assert.IsNotNull(versionCommand);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            options.AutoVersionCommand = false;
            versionCommand = manager.CreateCommand(new[] { "version" });
            Assert.IsNull(versionCommand);
            Assert.AreEqual(_expectedUsageNoVersion, writer.BaseWriter.ToString());

        }

        [TestMethod]
        public void TestWriteUsage()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(0);
            var options = new CommandOptions()
            {
                Out = writer,
                Error = writer,
                UsageOptions = new WriteUsageOptions()
                {
                    ExecutableName = _executableName,
                }
            };

            var manager = new CommandManager(_commandAssembly, options);
            manager.WriteUsage();
            Assert.AreEqual(_expectedUsage, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestWriteUsageColor()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(0);
            var options = new CommandOptions()
            {
                Out = writer,
                Error = writer,
                UsageOptions = new WriteUsageOptions()
                {
                    ExecutableName = _executableName,
                    UseColor = true,
                }
            };

            var manager = new CommandManager(_commandAssembly, options);
            manager.WriteUsage();
            Assert.AreEqual(_expectedUsageColor, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestCommandNameTransform()
        {
            var options = new CommandOptions()
            {
                CommandNameTransform = NameTransform.PascalCase
            };

            var info = new CommandInfo(typeof(AnotherSimpleCommand), options);
            Assert.AreEqual("AnotherSimple", info.Name);

            options.CommandNameTransform = NameTransform.CamelCase;
            info = new CommandInfo(typeof(AnotherSimpleCommand), options);
            Assert.AreEqual("anotherSimple", info.Name);

            options.CommandNameTransform = NameTransform.SnakeCase;
            info = new CommandInfo(typeof(AnotherSimpleCommand), options);
            Assert.AreEqual("another_simple", info.Name);

            options.CommandNameTransform = NameTransform.DashCase;
            info = new CommandInfo(typeof(AnotherSimpleCommand), options);
            Assert.AreEqual("another-simple", info.Name);

            options.StripCommandNameSuffix = null;
            info = new CommandInfo(typeof(AnotherSimpleCommand), options);
            Assert.AreEqual("another-simple-command", info.Name);

            options.StripCommandNameSuffix = "Command";
            var manager = new CommandManager(_commandAssembly, options);
            Assert.IsNotNull(manager.GetCommand("another-simple"));

            // Check automatic command name is affected too.
            options.CommandNameTransform = NameTransform.PascalCase;
            Assert.AreEqual("Version", manager.GetCommand("Version")?.Name);
        }

        #region Expected usage

        private const string _executableName = "test";

        public static readonly string _expectedUsage = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand


    custom
        Custom parsing command.

    test
        Test command description.

    version
        Displays version information.

".ReplaceLineEndings();

        public static readonly string _expectedUsageNoVersion = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand


    custom
        Custom parsing command.

    test
        Test command description.

".ReplaceLineEndings();

        public static readonly string _expectedUsageColor = @"[36mUsage:[0m test <command> [arguments]

The following commands are available:

    [32mAnotherSimpleCommand[0m


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
