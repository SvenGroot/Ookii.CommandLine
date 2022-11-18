// Copyright (c) Sven Groot (Ookii.org)
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Commands;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
            Assert.AreEqual(6, commands.Length);

            int index = 0;
            VerifyCommand(commands[index++], "AnotherSimpleCommand", typeof(AnotherSimpleCommand), false, new[] { "alias" });
            VerifyCommand(commands[index++], "AsyncCommand", typeof(AsyncCommand));
            VerifyCommand(commands[index++], "custom", typeof(CustomParsingCommand), true);
            VerifyCommand(commands[index++], "HiddenCommand", typeof(HiddenCommand));
            VerifyCommand(commands[index++], "test", typeof(TestCommand));
            VerifyCommand(commands[index++], "version", null);
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

            command = manager.GetCommand("alias");
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
                Error = writer,
                UsageWriter = new UsageWriter(writer)
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
                Error = writer,
                UsageWriter = new UsageWriter(writer)
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
                Error = writer,
                UsageWriter = new UsageWriter(writer, true)
                {
                    ExecutableName = _executableName,
                }
            };

            var manager = new CommandManager(_commandAssembly, options);
            manager.WriteUsage();
            Assert.AreEqual(_expectedUsageColor, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestWriteUsageInstruction()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(0);
            var options = new CommandOptions()
            {
                Error = writer,
                UsageWriter = new UsageWriter(writer)
                {
                    ExecutableName = _executableName,
                    IncludeCommandHelpInstruction = true,
                }
            };

            var manager = new CommandManager(_commandAssembly, options);
            manager.WriteUsage();
            Assert.AreEqual(_expectedUsageInstruction, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestWriteUsageApplicationDescription()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(0);
            var options = new CommandOptions()
            {
                Error = writer,
                UsageWriter = new UsageWriter(writer)
                {
                    IncludeApplicationDescriptionBeforeCommandList = true,
                    ExecutableName = _executableName,
                }
            };

            var manager = new CommandManager(_commandAssembly, options);
            manager.WriteUsage();
            Assert.AreEqual(_expectedUsageWithDescription, writer.BaseWriter.ToString());
        }

        [TestMethod]
        public void TestCommandUsage()
        {
            using var writer = LineWrappingTextWriter.ForStringWriter(0);
            var options = new CommandOptions()
            {
                Error = writer,
                UsageWriter = new UsageWriter(writer)
                {
                    ExecutableName = _executableName,
                }
            };

            // This tests whether the command name is included in the help for the command.
            var manager = new CommandManager(_commandAssembly, options);
            var result = manager.CreateCommand(new[] { "AsyncCommand", "-Help" });
            Assert.IsNull(result);
            Assert.AreEqual(_expectedCommandUsage, writer.BaseWriter.ToString());
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

        [TestMethod]
        public void TestCommandFilter()
        {
            var options = new CommandOptions()
            {
                CommandFilter = cmd => !cmd.UseCustomArgumentParsing,
            };

            var manager = new CommandManager(_commandAssembly, options);
            Assert.IsNull(manager.GetCommand("custom"));
            Assert.IsNotNull(manager.GetCommand("test"));
            Assert.IsNotNull(manager.GetCommand("AnotherSimpleCommand"));
            Assert.IsNotNull(manager.GetCommand("HiddenCommand"));
        }

        [TestMethod]
        public async Task TestAsyncCommand()
        {
            var manager = new CommandManager(_commandAssembly);
            var result = await manager.RunCommandAsync(new[] { "AsyncCommand", "5" });
            Assert.AreEqual(5, result);

            // RunCommand works but calls Run.
            result = manager.RunCommand(new[] { "AsyncCommand", "5" });
            Assert.AreEqual(6, result);

            // RunCommandAsync works on non-async tasks.
            result = await manager.RunCommandAsync(new[] { "AnotherSimpleCommand", "-Value", "5" });
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public async Task TestAsyncCommandBase()
        {
            var command = new AsyncBaseCommand();
            var actual = await command.RunAsync();
            Assert.AreEqual(42, actual);

            // Test Run invokes RunAsync.
            actual = command.Run();
            Assert.AreEqual(42, actual);
        }

        private static void VerifyCommand(CommandInfo command, string name, Type type, bool customParsing = false, string[] aliases = null)
        {
            Assert.AreEqual(name, command.Name);
            if (type != null)
            {
                Assert.AreEqual(type, command.CommandType);
            }

            Assert.AreEqual(customParsing, command.UseCustomArgumentParsing);
            CollectionAssert.AreEqual(aliases ?? Array.Empty<string>(), command.Aliases.ToArray());
        }

        #region Expected usage

        private const string _executableName = "test";

        public static readonly string _expectedUsage = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand, alias

    custom
        Custom parsing command.

    test
        Test command description.

    version
        Displays version information.

".ReplaceLineEndings();

        public static readonly string _expectedUsageNoVersion = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand, alias

    custom
        Custom parsing command.

    test
        Test command description.

".ReplaceLineEndings();

        public static readonly string _expectedUsageColor = @"[36mUsage:[0m test <command> [arguments]

The following commands are available:

    [32mAnotherSimpleCommand, alias[0m

    [32mcustom[0m
        Custom parsing command.

    [32mtest[0m
        Test command description.

    [32mversion[0m
        Displays version information.

".ReplaceLineEndings();

        public static readonly string _expectedUsageInstruction = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand, alias

    custom
        Custom parsing command.

    test
        Test command description.

    version
        Displays version information.

Run 'test <command> -Help' for more information about a command.
".ReplaceLineEndings();

        public static readonly string _expectedUsageWithDescription = @"Tests for Ookii.CommandLine.

Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand, alias

    custom
        Custom parsing command.

    test
        Test command description.

    version
        Displays version information.

".ReplaceLineEndings();

        public static readonly string _expectedCommandUsage = @"Async command description.

Usage: test AsyncCommand [[-Value] <Int32>] [-Help]

    -Value <Int32>
        Argument description.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

".ReplaceLineEndings();

        #endregion
    }
}
