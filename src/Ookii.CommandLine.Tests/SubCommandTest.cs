// Copyright (c) Sven Groot (Ookii.org)
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Support;
using Ookii.CommandLine.Tests.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests
{
    [TestClass]
    public class SubCommandTest
    {
        private static readonly Assembly _commandAssembly = Assembly.GetExecutingAssembly();

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Avoid exception when testing reflection on argument types that also have the
            // GeneratedParseAttribute set.
            ParseOptions.AllowReflectionWithGeneratedParserDefault = true;
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void GetCommandsTest(ProviderKind kind)
        {
            var manager = CreateManager(kind);
            VerifyCommands(
                manager.GetCommands(),
                new("AnotherSimpleCommand", typeof(AnotherSimpleCommand), false, "alias"),
                new("AsyncCommand", typeof(AsyncCommand)),
                new("custom", typeof(CustomParsingCommand), true),
                new("HiddenCommand", typeof(HiddenCommand)),
                new("test", typeof(TestCommand)),
                new("version", null)
            );
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void GetCommandTest(ProviderKind kind)
        {
            var manager = CreateManager(kind);
            var command = manager.GetCommand("test");
            Assert.IsNotNull(command);
            Assert.AreEqual("test", command.Name);
            Assert.AreEqual(typeof(TestCommand), command.CommandType);

            command = manager.GetCommand("wrong");
            Assert.IsNull(command);

            command = manager.GetCommand("Test"); // default is case-insensitive
            Assert.IsNotNull(command);
            Assert.AreEqual("test", command.Name);
            Assert.AreEqual(typeof(TestCommand), command.CommandType);

            var manager2 = new CommandManager(_commandAssembly, new CommandOptions() { CommandNameComparer = StringComparer.Ordinal });
            command = manager2.GetCommand("Test");
            Assert.IsNull(command);

            command = manager.GetCommand("AnotherSimpleCommand");
            Assert.IsNotNull(command);
            Assert.AreEqual("AnotherSimpleCommand", command.Name);
            Assert.AreEqual(typeof(AnotherSimpleCommand), command.CommandType);

            command = manager.GetCommand("alias");
            Assert.IsNotNull(command);
            Assert.AreEqual("AnotherSimpleCommand", command.Name);
            Assert.AreEqual(typeof(AnotherSimpleCommand), command.CommandType);
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
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void CreateCommandTest(ProviderKind kind)
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

            var manager = CreateManager(kind, options);
            TestCommand command = (TestCommand)manager.CreateCommand("test", new[] { "-Argument", "Foo" }, 0);
            Assert.IsNotNull(command);
            Assert.AreEqual(ParseStatus.Success, manager.ParseResult.Status);
            Assert.AreEqual("Foo", command.Argument);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            command = (TestCommand)manager.CreateCommand(new[] { "test", "-Argument", "Bar" });
            Assert.IsNotNull(command);
            Assert.AreEqual(ParseStatus.Success, manager.ParseResult.Status);
            Assert.AreEqual("Bar", command.Argument);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            var command2 = (AnotherSimpleCommand)manager.CreateCommand("anothersimplecommand", new[] { "skip", "-Value", "42" }, 1);
            Assert.IsNotNull(command2);
            Assert.AreEqual(ParseStatus.Success, manager.ParseResult.Status);
            Assert.AreEqual(42, command2.Value);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            CustomParsingCommand command3 = (CustomParsingCommand)manager.CreateCommand(new[] { "custom", "hello" });
            Assert.IsNotNull(command3);
            // None because of custom parsing.
            Assert.AreEqual(ParseStatus.None, manager.ParseResult.Status);
            Assert.AreEqual("hello", command3.Value);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            var versionCommand = manager.CreateCommand(new[] { "version" });
            Assert.IsNotNull(versionCommand);
            Assert.AreEqual(ParseStatus.Success, manager.ParseResult.Status);
            Assert.AreEqual("", writer.BaseWriter.ToString());

            options.AutoVersionCommand = false;
            versionCommand = manager.CreateCommand(new[] { "version" });
            Assert.IsNull(versionCommand);
            Assert.AreEqual(ParseStatus.None, manager.ParseResult.Status);
            Assert.AreEqual(_expectedUsageNoVersion, writer.BaseWriter.ToString());

            ((StringWriter)writer.BaseWriter).GetStringBuilder().Clear();
            versionCommand = manager.CreateCommand(new[] { "test", "-Foo" });
            Assert.IsNull(versionCommand);
            Assert.AreEqual(ParseStatus.Error, manager.ParseResult.Status);
            Assert.AreEqual(CommandLineArgumentErrorCategory.UnknownArgument, manager.ParseResult.LastException.Category);
            Assert.AreEqual(manager.ParseResult.ArgumentName, manager.ParseResult.LastException.ArgumentName);
            Assert.AreNotEqual("", writer.BaseWriter.ToString());

        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestWriteUsage(ProviderKind kind)
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

            var manager = CreateManager(kind, options);
            manager.WriteUsage();
            Assert.AreEqual(_expectedUsage, writer.BaseWriter.ToString());
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestWriteUsageColor(ProviderKind kind)
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

            var manager = CreateManager(kind, options);
            manager.WriteUsage();
            Assert.AreEqual(_expectedUsageColor, writer.BaseWriter.ToString());
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestWriteUsageInstruction(ProviderKind kind)
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

            var manager = CreateManager(kind, options);
            manager.WriteUsage();
            Assert.AreEqual(_expectedUsageInstruction, writer.BaseWriter.ToString());
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestWriteUsageApplicationDescription(ProviderKind kind)
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

            var manager = CreateManager(kind, options);
            manager.WriteUsage();
            Assert.AreEqual(_expectedUsageWithDescription, writer.BaseWriter.ToString());
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestCommandUsage(ProviderKind kind)
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
            var manager = CreateManager(kind, options);
            var result = manager.CreateCommand(new[] { "AsyncCommand", "-Help" });
            Assert.IsNull(result);
            Assert.AreEqual(ParseStatus.Canceled, manager.ParseResult.Status);
            Assert.AreEqual("Help", manager.ParseResult.ArgumentName);
            Assert.AreEqual(_expectedCommandUsage, writer.BaseWriter.ToString());
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestCommandNameTransform(ProviderKind kind)
        {
            var options = new CommandOptions()
            {
                CommandNameTransform = NameTransform.PascalCase
            };

            var manager = CreateManager(kind, options);
            var info = CommandInfo.Create(typeof(AnotherSimpleCommand), manager);
            Assert.AreEqual("AnotherSimple", info.Name);

            options.CommandNameTransform = NameTransform.CamelCase;
            info = CommandInfo.Create(typeof(AnotherSimpleCommand), manager);
            Assert.AreEqual("anotherSimple", info.Name);

            options.CommandNameTransform = NameTransform.SnakeCase;
            info = CommandInfo.Create(typeof(AnotherSimpleCommand), manager);
            Assert.AreEqual("another_simple", info.Name);

            options.CommandNameTransform = NameTransform.DashCase;
            info = CommandInfo.Create(typeof(AnotherSimpleCommand), manager);
            Assert.AreEqual("another-simple", info.Name);

            options.StripCommandNameSuffix = null;
            info = CommandInfo.Create(typeof(AnotherSimpleCommand), manager);
            Assert.AreEqual("another-simple-command", info.Name);

            options.StripCommandNameSuffix = "Command";
            Assert.IsNotNull(manager.GetCommand("another-simple"));

            // Check automatic command name is affected too.
            options.CommandNameTransform = NameTransform.PascalCase;
            Assert.AreEqual("Version", manager.GetCommand("Version")?.Name);
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestCommandFilter(ProviderKind kind)
        {
            var options = new CommandOptions()
            {
                CommandFilter = cmd => !cmd.UseCustomArgumentParsing,
            };

            var manager = CreateManager(kind, options);
            Assert.IsNull(manager.GetCommand("custom"));
            Assert.IsNotNull(manager.GetCommand("test"));
            Assert.IsNotNull(manager.GetCommand("AnotherSimpleCommand"));
            Assert.IsNotNull(manager.GetCommand("HiddenCommand"));
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public async Task TestAsyncCommand(ProviderKind kind)
        {
            var manager = CreateManager(kind);
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

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestExplicitAssembly(ProviderKind kind)
        {
            if (kind == ProviderKind.Reflection)
            {
                // Using the calling assembly explicitly loads all the commands, including internal,
                // same as the default constructor.
                var mgr = new CommandManager(_commandAssembly);
                Assert.AreEqual(6, mgr.GetCommands().Count());
            }

            // Explicitly specify the external assembly, which loads only public commands.
            var manager = kind == ProviderKind.Reflection
                ? new CommandManager(typeof(ExternalCommand).Assembly)
                : new GeneratedManagerWithExplicitAssembly();

            VerifyCommands(
                manager.GetCommands(),
                new("external", typeof(ExternalCommand)),
                new("OtherExternalCommand", typeof(OtherExternalCommand)),
                new("version", null)
            );

            // Public commands from external assembly plus public and internal commands from
            // calling assembly.
            manager = kind == ProviderKind.Reflection
                ? new CommandManager(new[] { typeof(ExternalCommand).Assembly, _commandAssembly })
                : new GeneratedManagerWithMultipleAssemblies();

            VerifyCommands(
                manager.GetCommands(),
                new("AnotherSimpleCommand", typeof(AnotherSimpleCommand), false, "alias"),
                new("AsyncCommand", typeof(AsyncCommand)),
                new("custom", typeof(CustomParsingCommand), true),
                new("external", typeof(ExternalCommand)),
                new("HiddenCommand", typeof(HiddenCommand)),
                new("OtherExternalCommand", typeof(OtherExternalCommand)),
                new("test", typeof(TestCommand)),
                new("version", null)
            );
        }

        private record struct ExpectedCommand(string Name, Type Type, bool CustomParsing = false, params string[] Aliases);


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

        private static void VerifyCommands(IEnumerable<CommandInfo> actual, params ExpectedCommand[] expected)
        {
            Assert.AreEqual(expected.Length, actual.Count());
            var index = 0;
            foreach (var command in actual)
            {
                var info = expected[index];
                VerifyCommand(command, info.Name, info.Type, info.CustomParsing, info.Aliases);
                ++index;
            }
        }


        public static CommandManager CreateManager(ProviderKind kind, CommandOptions options = null)
        {
            var manager = kind switch
            {
                ProviderKind.Reflection => new CommandManager(options),
                ProviderKind.Generated => new GeneratedManager(options),
                _ => throw new InvalidOperationException()
            };

            Assert.AreEqual(kind, manager.ProviderKind);
            return manager;
        }

        public static string GetCustomDynamicDataDisplayName(MethodInfo methodInfo, object[] data)
            => $"{methodInfo.Name} ({data[0]})";


        public static IEnumerable<object[]> ProviderKinds
            => new[]
            {
                new object[] { ProviderKind.Reflection },
                new object[] { ProviderKind.Generated }
            };

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
