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

namespace Ookii.CommandLine.Tests;

[TestClass]
public partial class SubCommandTest
{
    private static readonly Assembly _commandAssembly = Assembly.GetExecutingAssembly();

    [ClassInitialize]
    public static void TestFixtureSetup(TestContext context)
    {
        // Get test coverage of reflection provider even on types that have the
        // GeneratedParserAttribute.
        ParseOptions.ForceReflectionDefault = true;
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
            new("TestParentCommand", typeof(TestParentCommand), true),
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

        var manager2 = new CommandManager(_commandAssembly, new CommandOptions() { CommandNameComparison = StringComparison.Ordinal, AutoCommandPrefixAliases = false });
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

        // Can't get a command with an parent that's not currently set in the options.
        command = manager.GetCommand("TestChildCommand");
        Assert.IsNull(command);
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
            Assert.AreEqual(7, mgr.GetCommands().Count());
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
            new("TestParentCommand", typeof(TestParentCommand), true),
            new("version", null)
        );
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestParentCommand(ProviderKind kind)
    {
        var options = new CommandOptions
        {
            ParentCommand = typeof(TestParentCommand),
            AutoCommandPrefixAliases = false,
        };

        var manager = CreateManager(kind, options);
        VerifyCommands(
            manager.GetCommands(),
            new("NestedParentCommand", typeof(NestedParentCommand), true) { ParentCommand = typeof(TestParentCommand) },
            new("OtherTestChildCommand", typeof(OtherTestChildCommand)) { ParentCommand = typeof(TestParentCommand) },
            new("TestChildCommand", typeof(TestChildCommand)) { ParentCommand = typeof(TestParentCommand) }
        );

        var command = manager.GetCommand("TestChildCommand");
        Assert.IsNotNull(command);

        command = manager.GetCommand("version");
        Assert.IsNull(command);

        command = manager.GetCommand("test");
        Assert.IsNull(command);

        manager.Options.ParentCommand = null;
        var result = manager.RunCommand(new[] { "TestParentCommand", "TestChildCommand", "-Value", "5" });
        Assert.AreEqual(5, result);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestParentCommandUsage(ProviderKind kind)
    {
        using var writer = LineWrappingTextWriter.ForStringWriter(0);
        var options = new CommandOptions()
        {
            Error = writer,
            ShowUsageOnError = UsageHelpRequest.Full,
            UsageWriter = new UsageWriter(writer)
            {
                ExecutableName = _executableName,
                IncludeCommandHelpInstruction = true,
                IncludeApplicationDescriptionBeforeCommandList = true,
            }
        };

        var manager = CreateManager(kind, options);
        var result = manager.RunCommand(new[] { "TestParentCommand" });
        Assert.AreEqual(1, result);
        Assert.AreEqual(_expectedParentCommandUsage, writer.ToString());

        ((StringWriter)writer.BaseWriter).GetStringBuilder().Clear();
        result = manager.RunCommand(new[] { "TestParentCommand", "NestedParentCommand" });
        Assert.AreEqual(1, result);
        Assert.AreEqual(_expectedNestedParentCommandUsage, writer.ToString());

        ((StringWriter)writer.BaseWriter).GetStringBuilder().Clear();
        result = manager.RunCommand(new[] { "TestParentCommand", "NestedParentCommand", "NestedParentChildCommand", "-Foo" });
        Assert.AreEqual(1, result);
        Assert.AreEqual(_expectedNestedChildCommandUsage, writer.ToString());
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestAutoPrefixAliases(ProviderKind kind)
    {
        var manager = CreateManager(kind);
        
        // Ambiguous between test and TestParentCommand.
        Assert.IsNull(manager.GetCommand("tes"));

        // Not ambiguous
        Assert.AreEqual("TestParentCommand", manager.GetCommand("testp").Name);
        Assert.AreEqual("version", manager.GetCommand("v").Name);

        // Case sensitive, "tes" is no longer ambigous.
        manager = CreateManager(kind, new CommandOptions() { CommandNameComparison = StringComparison.Ordinal });
        Assert.AreEqual("test", manager.GetCommand("tes").Name);
    }

    private class VersionCommandStringProvider : LocalizedStringProvider
    {
        public override string AutomaticVersionCommandName() => "AnotherSimpleCommand";
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestVersionCommandConflict(ProviderKind kind)
    {
        // Change the name of the version command so it matches one of the explicit commands.
        var options = new CommandOptions()
        {
            StringProvider = new VersionCommandStringProvider(),
        };

        var manager = CreateManager(kind, options);

        // There is no command named version.
        Assert.IsNull(manager.GetCommand("version"));

        // Name returns our command.
        Assert.AreEqual(typeof(AnotherSimpleCommand), manager.GetCommand("AnotherSimpleCommand").CommandType);

        // There is only one in the list of commands.
        Assert.AreEqual(1, manager.GetCommands().Where(c => c.Name == "AnotherSimpleCommand").Count());

        // Prefix is not ambiguous because the automatic command doesn't exist.
        Assert.AreEqual(typeof(AnotherSimpleCommand), manager.GetCommand("Another").CommandType);

        // If we filter out our command, the automatic one gets returned.
        options.CommandFilter = c => c.CommandType != typeof(AnotherSimpleCommand);
        Assert.AreEqual(typeof(AutomaticVersionCommand), manager.GetCommand("AnotherSimpleCommand").CommandType);
        Assert.AreEqual(typeof(AutomaticVersionCommand), manager.GetCommands().Where(c => c.Name == "AnotherSimpleCommand").SingleOrDefault().CommandType);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestNoVersionCommand(ProviderKind kind)
    {
        var options = new CommandOptions()
        {
            AutoVersionCommand = false,
        };

        var manager = CreateManager(kind, options);
        Assert.IsNull(manager.GetCommand("version"));
        Assert.IsFalse(manager.GetCommands().Any(c => c.Name == "version"));

        // We can also filter it out.
        options.AutoVersionCommand = true;
        Assert.IsNotNull(manager.GetCommand("version"));
        options.CommandFilter = c => c.Name != "version";
        Assert.IsNull(manager.GetCommand("version"));
        Assert.IsFalse(manager.GetCommands().Any(c => c.Name == "version"));

        // Setting ParentCommand means there is no version command.
        options.CommandFilter = null;
        options.ParentCommand = typeof(ParentCommand);
        Assert.IsNull(manager.GetCommand("version"));
        Assert.IsFalse(manager.GetCommands().Any(c => c.Name == "version"));
    }

    private record struct ExpectedCommand(string Name, Type Type, bool CustomParsing = false, params string[] Aliases)
    {
        public Type ParentCommand { get; set; }
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

    private static void VerifyCommands(IEnumerable<CommandInfo> actual, params ExpectedCommand[] expected)
    {
        Assert.AreEqual(expected.Length, actual.Count());
        var index = 0;
        foreach (var command in actual)
        {
            var info = expected[index];
            VerifyCommand(command, info.Name, info.Type, info.CustomParsing, info.Aliases);
            Assert.AreEqual(info.ParentCommand, command.ParentCommandType);
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
}
