using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Support;
using Ookii.CommandLine.Tests.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;

namespace Ookii.CommandLine.Tests;

/// <summary>
///This is a test class for CommandLineParserTest and is intended
///to contain all CommandLineParserTest Unit Tests
///</summary>
[TestClass()]
public partial class CommandLineParserTest
{
    [ClassInitialize]
    public static void TestFixtureSetup(TestContext context)
    {
        // Get test coverage of reflection provider even on types that have the
        // GeneratedParserAttribute.
        ParseOptions.ForceReflectionDefault = true;
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ConstructorEmptyArgumentsTest(ProviderKind kind)
    {
        Type argumentsType = typeof(EmptyArguments);
        var target = CreateParser<EmptyArguments>(kind);
        Assert.AreEqual(CultureInfo.InvariantCulture, target.Culture);
        Assert.AreEqual(false, target.AllowDuplicateArguments);
        Assert.AreEqual(true, target.AllowWhiteSpaceValueSeparator);
        Assert.AreEqual(ParsingMode.Default, target.Mode);
        CollectionAssert.AreEqual(CommandLineParser.GetDefaultArgumentNamePrefixes(), target.ArgumentNamePrefixes);
        Assert.IsNull(target.LongArgumentNamePrefix);
        Assert.AreEqual(argumentsType, target.ArgumentsType);
        Assert.AreEqual("Ookii.CommandLine Unit Tests", target.ApplicationFriendlyName);
        Assert.AreEqual(string.Empty, target.Description);
        Assert.AreEqual(2, target.Arguments.Length);
        VerifyArguments(target.Arguments, new[]
        {
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        });
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ConstructorTest(ProviderKind kind)
    {
        Type argumentsType = typeof(TestArguments);
        var target = CreateParser<TestArguments>(kind);
        Assert.AreEqual(CultureInfo.InvariantCulture, target.Culture);
        Assert.AreEqual(false, target.AllowDuplicateArguments);
        Assert.AreEqual(true, target.AllowWhiteSpaceValueSeparator);
        Assert.AreEqual(ParsingMode.Default, target.Mode);
        CollectionAssert.AreEqual(CommandLineParser.GetDefaultArgumentNamePrefixes(), target.ArgumentNamePrefixes);
        Assert.IsNull(target.LongArgumentNamePrefix);
        Assert.AreEqual(argumentsType, target.ArgumentsType);
        Assert.AreEqual("Friendly name", target.ApplicationFriendlyName);
        Assert.AreEqual("Test arguments description.", target.Description);
        Assert.AreEqual(18, target.Arguments.Length);
        VerifyArguments(target.Arguments, new[]
        {
            new ExpectedArgument("arg1", typeof(string)) { MemberName = "Arg1", Position = 0, IsRequired = true, Description = "Arg1 description." },
            new ExpectedArgument("other", typeof(int)) { MemberName = "Arg2", Position = 1, DefaultValue = 42, Description = "Arg2 description.", ValueDescription = "Number" },
            new ExpectedArgument("notSwitch", typeof(bool)) { MemberName = "NotSwitch", Position = 2, DefaultValue = false },
            new ExpectedArgument("Arg5", typeof(float)) { Position = 3, Description = "Arg5 description.", DefaultValue = 1.0f },
            new ExpectedArgument("other2", typeof(int)) { MemberName = "Arg4", Position = 4, DefaultValue = 47, Description = "Arg4 description.", ValueDescription = "Number" },
            new ExpectedArgument("Arg8", typeof(DayOfWeek[]), ArgumentKind.MultiValue) { ElementType = typeof(DayOfWeek), Position = 5 },
            new ExpectedArgument("Arg6", typeof(string)) { Position = null, IsRequired = true, Description = "Arg6 description.", Aliases = new[] { "Alias1", "Alias2" } },
            new ExpectedArgument("Arg10", typeof(bool[]), ArgumentKind.MultiValue) { ElementType = typeof(bool), Position = null, IsSwitch = true },
            new ExpectedArgument("Arg11", typeof(bool?)) { ElementType = typeof(bool), Position = null, ValueDescription = "Boolean", IsSwitch = true },
            new ExpectedArgument("Arg12", typeof(Collection<int>), ArgumentKind.MultiValue) { ElementType = typeof(int), Position = null, DefaultValue = 42 },
            new ExpectedArgument("Arg13", typeof(Dictionary<string, int>), ArgumentKind.Dictionary) { ElementType = typeof(KeyValuePair<string, int>), ValueDescription = "String=Int32" },
            new ExpectedArgument("Arg14", typeof(IDictionary<string, int>), ArgumentKind.Dictionary) { ElementType = typeof(KeyValuePair<string, int>), ValueDescription = "String=Int32" },
            new ExpectedArgument("Arg15", typeof(KeyValuePair<string, int>)) { ValueDescription = "KeyValuePair<String, Int32>" },
            new ExpectedArgument("Arg3", typeof(string)) { Position = null },
            new ExpectedArgument("Arg7", typeof(bool)) { Position = null, IsSwitch = true, Aliases = new[] { "Alias3" } },
            new ExpectedArgument("Arg9", typeof(int?)) { ElementType = typeof(int), Position = null, ValueDescription = "Int32" },
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        });
    }

    [TestMethod]
    public void TestConstructorGeneratedProvider()
    {
        // Modify the default instead of explicitly creating options to make sure that the default
        // is correct.
        ParseOptions.ForceReflectionDefault = false;

        // The constructor should find and use the generated provider.
        var parser = new CommandLineParser<TestArguments>();
        Assert.AreEqual(ProviderKind.Generated, parser.ProviderKind);

        // Change back for other tests.
        ParseOptions.ForceReflectionDefault = true;
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTest(ProviderKind kind)
    {
        var target = CreateParser<TestArguments>(kind);
        // Only required arguments
        TestParse(target, "val1 2 -arg6 val6", "val1", 2, arg6: "val6");
        // Make sure negative numbers are accepted, and not considered an argument name.
        TestParse(target, "val1 -2 -arg6 val6", "val1", -2, arg6: "val6");
        // All positional arguments except array
        TestParse(target, "val1 2 true 5.5 4 -arg6 arg6", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6");
        // All positional arguments including array
        TestParse(target, "val1 2 true 5.5 4 -arg6 arg6 Monday Tuesday", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6", arg8: new[] { DayOfWeek.Monday, DayOfWeek.Tuesday });
        // All positional arguments including array, which is specified by name first and then by position
        TestParse(target, "val1 2 true 5.5 4 -arg6 arg6 -arg8 Monday Tuesday", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6", arg8: new[] { DayOfWeek.Monday, DayOfWeek.Tuesday });
        // Some positional arguments using names, in order
        TestParse(target, "-arg1 val1 2 true -arg5 5.5 4 -arg6 arg6", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6");
        // Some position arguments using names, out of order (also uses : and - for one of them to mix things up)
        TestParse(target, "-other 2 val1 -arg5:5.5 true 4 -arg6 arg6", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6");
        // All arguments
        TestParse(target, "val1 2 true -arg3 val3 -other2:4 5.5 -arg6 val6 -arg7 -arg8 Monday -arg8 Tuesday -arg9 9 -arg10 -arg10 -arg10:false -arg11:false -arg12 12 -arg12 13 -arg13 foo=13 -arg13 bar=14 -arg14 hello=1 -arg14 bye=2 -arg15 something=5", "val1", 2, true, "val3", 4, 5.5f, "val6", true, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday }, 9, new[] { true, true, false }, false, new[] { 12, 13 }, new Dictionary<string, int>() { { "foo", 13 }, { "bar", 14 } }, new Dictionary<string, int>() { { "hello", 1 }, { "bye", 2 } }, new KeyValuePair<string, int>("something", 5));
        // Using aliases
        TestParse(target, "val1 2 -alias1 valalias6 -alias3", "val1", 2, arg6: "valalias6", arg7: true);
        // Long prefix cannot be used
        CheckThrows(target, new[] { "val1", "2", "--arg6", "val6" }, CommandLineArgumentErrorCategory.UnknownArgument, "-arg6", remainingArgumentCount: 2);
        // Short name cannot be used
        CheckThrows(target, new[] { "val1", "2", "-arg6", "val6", "-a:5.5" }, CommandLineArgumentErrorCategory.UnknownArgument, "a", remainingArgumentCount: 1);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestEmptyArguments(ProviderKind kind)
    {
        var target = CreateParser<EmptyArguments>(kind);
        // This test was added because version 2.0 threw an IndexOutOfRangeException when you tried to specify a positional argument when there were no positional arguments defined.
        CheckThrows(target, new[] { "Foo", "Bar" }, CommandLineArgumentErrorCategory.TooManyArguments, remainingArgumentCount: 2);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestTooManyArguments(ProviderKind kind)
    {
        var target = CreateParser<ThrowingArguments>(kind);

        // Only accepts one positional argument.
        CheckThrows(target, new[] { "Foo", "Bar" }, CommandLineArgumentErrorCategory.TooManyArguments, remainingArgumentCount: 1);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestPropertySetterThrows(ProviderKind kind)
    {
        var target = CreateParser<ThrowingArguments>(kind);

        // No remaining arguments; exception happens after parsing finishes.
        CheckThrows(target,
            new[] { "-ThrowingArgument", "-5" },
            CommandLineArgumentErrorCategory.ApplyValueError,
            "ThrowingArgument",
            typeof(ArgumentOutOfRangeException));
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestConstructorThrows(ProviderKind kind)
    {
        var target = CreateParser<ThrowingConstructor>(kind);

        CheckThrows(target,
            Array.Empty<string>(),
            CommandLineArgumentErrorCategory.CreateArgumentsTypeError,
            null,
            typeof(ArgumentException));
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestDuplicateDictionaryKeys(ProviderKind kind)
    {
        var target = CreateParser<DictionaryArguments>(kind);

        var args = target.Parse(new[] { "-DuplicateKeys", "Foo=1", "-DuplicateKeys", "Bar=2", "-DuplicateKeys", "Foo=3" });
        Assert.IsNotNull(args);
        Assert.AreEqual(2, args.DuplicateKeys.Count);
        Assert.AreEqual(3, args.DuplicateKeys["Foo"]);
        Assert.AreEqual(2, args.DuplicateKeys["Bar"]);

        CheckThrows(target,
            new[] { "-NoDuplicateKeys", "Foo=1", "-NoDuplicateKeys", "Bar=2", "-NoDuplicateKeys", "Foo=3" },
            CommandLineArgumentErrorCategory.InvalidDictionaryValue,
            "NoDuplicateKeys",
            typeof(ArgumentException),
            remainingArgumentCount: 2);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestMultiValueSeparator(ProviderKind kind)
    {
        var target = CreateParser<MultiValueSeparatorArguments>(kind);

        var args = target.Parse(new[] { "-NoSeparator", "Value1,Value2", "-NoSeparator", "Value3", "-Separator", "Value1,Value2", "-Separator", "Value3" });
        Assert.IsNotNull(args);
        CollectionAssert.AreEqual(new[] { "Value1,Value2", "Value3" }, args.NoSeparator);
        CollectionAssert.AreEqual(new[] { "Value1", "Value2", "Value3" }, args.Separator);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestNameValueSeparator(ProviderKind kind)
    {
        var target = CreateParser<SimpleArguments>(kind);
        CollectionAssert.AreEquivalent(new[] { ':', '=' }, target.NameValueSeparators);
        var args = CheckSuccess(target, new[] { "-Argument1:test", "-Argument2:foo:bar" });
        Assert.IsNotNull(args);
        Assert.AreEqual("test", args.Argument1);
        Assert.AreEqual("foo:bar", args.Argument2);
        args = CheckSuccess(target, new[] { "-Argument1=test", "-Argument2=foo:bar" });
        Assert.AreEqual("test", args.Argument1);
        Assert.AreEqual("foo:bar", args.Argument2);
        args = CheckSuccess(target, new[] { "-Argument2:foo=bar" });
        Assert.AreEqual("foo=bar", args.Argument2);

        CheckThrows(target,
            new[] { "-Argument1>test" },
            CommandLineArgumentErrorCategory.UnknownArgument,
            "Argument1>test",
            remainingArgumentCount: 1);

        var options = new ParseOptions()
        {
            NameValueSeparators = new[] { '>' },
        };

        target = CreateParser<SimpleArguments>(kind, options);
        args = target.Parse(new[] { "-Argument1>test", "-Argument2>foo>bar" });
        Assert.IsNotNull(args);
        Assert.AreEqual("test", args.Argument1);
        Assert.AreEqual("foo>bar", args.Argument2);
        CheckThrows(target,
            new[] { "-Argument1:test" },
            CommandLineArgumentErrorCategory.UnknownArgument,
            "Argument1:test",
            remainingArgumentCount: 1);

        CheckThrows(target,
            new[] { "-Argument1=test" },
            CommandLineArgumentErrorCategory.UnknownArgument,
            "Argument1=test",
            remainingArgumentCount: 1);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestKeyValueSeparator(ProviderKind kind)
    {
        var target = CreateParser<KeyValueSeparatorArguments>(kind);
        Assert.AreEqual("=", target.GetArgument("DefaultSeparator")!.KeyValueSeparator);
        Assert.AreEqual("String=Int32", target.GetArgument("DefaultSeparator")!.ValueDescription);
        Assert.AreEqual("<=>", target.GetArgument("CustomSeparator")!.KeyValueSeparator);
        Assert.AreEqual("String<=>String", target.GetArgument("CustomSeparator")!.ValueDescription);

        var result = CheckSuccess(target, new[] { "-CustomSeparator", "foo<=>bar", "-CustomSeparator", "baz<=>contains<=>separator", "-CustomSeparator", "hello<=>" });
        Assert.IsNotNull(result);
        CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("foo", "bar"), KeyValuePair.Create("baz", "contains<=>separator"), KeyValuePair.Create("hello", "") }, result.CustomSeparator);
        CheckThrows(target,
            new[] { "-CustomSeparator", "foo=bar" },
            CommandLineArgumentErrorCategory.ArgumentValueConversion,
            "CustomSeparator",
            typeof(FormatException),
            remainingArgumentCount: 2);

        // Inner exception is FormatException because what throws here is trying to convert
        // ">bar" to int.
        CheckThrows(target,
            new[] { "-DefaultSeparator", "foo<=>bar" },
            CommandLineArgumentErrorCategory.ArgumentValueConversion,
            "DefaultSeparator",
            typeof(FormatException),
            remainingArgumentCount: 2);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsage(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            ArgumentNamePrefixes = new[] { "/", "-" }
        };

        var target = CreateParser<TestArguments>(kind, options);
        var writer = new UsageWriter()
        {
            ExecutableName = _executableName
        };

        string actual = target.GetUsage(writer);
        Assert.AreEqual(_expectedDefaultUsage, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageLongShort(ProviderKind kind)
    {
        var target = CreateParser<LongShortArguments>(kind);
        var options = new UsageWriter()
        {
            ExecutableName = _executableName
        };

        string actual = target.GetUsage(options);
        Assert.AreEqual(_expectedLongShortUsage, actual);

        options.UseShortNamesForSyntax = true;
        actual = target.GetUsage(options);
        Assert.AreEqual(_expectedLongShortUsageShortNameSyntax, actual);

        options = new UsageWriter()
        {
            ExecutableName = _executableName,
            UseAbbreviatedSyntax = true,
        };

        actual = target.GetUsage(options);
        Assert.AreEqual(_expectedLongShortUsageAbbreviated, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageFilter(ProviderKind kind)
    {
        var target = CreateParser<TestArguments>(kind);
        var options = new UsageWriter()
        {
            ExecutableName = _executableName,
            ArgumentDescriptionListFilter = DescriptionListFilterMode.Description
        };

        string actual = target.GetUsage(options);
        Assert.AreEqual(_expectedUsageDescriptionOnly, actual);

        options.ArgumentDescriptionListFilter = DescriptionListFilterMode.All;
        actual = target.GetUsage(options);
        Assert.AreEqual(_expectedUsageAll, actual);

        options.ArgumentDescriptionListFilter = DescriptionListFilterMode.None;
        actual = target.GetUsage(options);
        Assert.AreEqual(_expectedUsageNone, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageColor(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            ArgumentNamePrefixes = new[] { "/", "-" }
        };

        CommandLineParser target = CreateParser<TestArguments>(kind, options);
        var writer = new UsageWriter(useColor: true)
        {
            ExecutableName = _executableName,
        };

        string actual = target.GetUsage(writer);
        Assert.AreEqual(_expectedUsageColor, actual);

        target = CreateParser<LongShortArguments>(kind);
        actual = target.GetUsage(writer);
        Assert.AreEqual(_expectedLongShortUsageColor, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageOrder(ProviderKind kind)
    {
        var parser = CreateParser<LongShortArguments>(kind);
        var options = new UsageWriter()
        {
            ExecutableName = _executableName,
            ArgumentDescriptionListOrder = DescriptionListSortMode.Alphabetical,
        };

        var usage = parser.GetUsage(options);
        Assert.AreEqual(_expectedUsageAlphabeticalLongName, usage);

        options.ArgumentDescriptionListOrder = DescriptionListSortMode.AlphabeticalDescending;
        usage = parser.GetUsage(options);
        Assert.AreEqual(_expectedUsageAlphabeticalLongNameDescending, usage);

        options.ArgumentDescriptionListOrder = DescriptionListSortMode.AlphabeticalShortName;
        usage = parser.GetUsage(options);
        Assert.AreEqual(_expectedUsageAlphabeticalShortName, usage);

        options.ArgumentDescriptionListOrder = DescriptionListSortMode.AlphabeticalShortNameDescending;
        usage = parser.GetUsage(options);
        Assert.AreEqual(_expectedUsageAlphabeticalShortNameDescending, usage);

        parser = CreateParser<LongShortArguments>(kind, new ParseOptions() { Mode = ParsingMode.Default });
        options.ArgumentDescriptionListOrder = DescriptionListSortMode.Alphabetical;
        usage = parser.GetUsage(options);
        Assert.AreEqual(_expectedUsageAlphabetical, usage);

        options.ArgumentDescriptionListOrder = DescriptionListSortMode.AlphabeticalDescending;
        usage = parser.GetUsage(options);
        Assert.AreEqual(_expectedUsageAlphabeticalDescending, usage);

        // ShortName versions work like regular if not in LongShortMode.
        options.ArgumentDescriptionListOrder = DescriptionListSortMode.AlphabeticalShortName;
        usage = parser.GetUsage(options);
        Assert.AreEqual(_expectedUsageAlphabetical, usage);

        options.ArgumentDescriptionListOrder = DescriptionListSortMode.AlphabeticalShortNameDescending;
        usage = parser.GetUsage(options);
        Assert.AreEqual(_expectedUsageAlphabeticalDescending, usage);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageSeparator(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            ArgumentNamePrefixes = new[] { "/", "-" },
            UsageWriter = new UsageWriter()
            {
                ExecutableName = _executableName,
                UseWhiteSpaceValueSeparator = false,
            }
        };
        var target = CreateParser<TestArguments>(kind, options);
        string actual = target.GetUsage(options.UsageWriter);
        Assert.AreEqual(_expectedUsageSeparator, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageCustomIndent(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            UsageWriter = new UsageWriter()
            {
                ExecutableName = _executableName,
                ArgumentDescriptionIndent = 4,
            }
        };
        var target = CreateParser<TestArguments>(kind, options);
        string actual = target.GetUsage(options.UsageWriter);
        Assert.AreEqual(_expectedCustomIndentUsage, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestStaticParse(ProviderKind kind)
    {
        using var output = new StringWriter();
        using var lineWriter = new LineWrappingTextWriter(output, 0);
        using var error = new StringWriter();
        var options = new ParseOptions()
        {
            ArgumentNamePrefixes = new[] { "/", "-" },
            Error = error,
            ShowUsageOnError = UsageHelpRequest.Full,
            UsageWriter = new UsageWriter(lineWriter)
            {
                ExecutableName = _executableName,
            }
        };

        var result = StaticParse<TestArguments>(kind, new[] { "foo", "-Arg6", "bar" }, options);
        Assert.IsNotNull(result);
        Assert.AreEqual("foo", result.Arg1);
        Assert.AreEqual("bar", result.Arg6);
        Assert.AreEqual(0, output.ToString().Length);
        Assert.AreEqual(0, error.ToString().Length);

        result = StaticParse<TestArguments>(kind, Array.Empty<string>(), options);
        Assert.IsNull(result);
        Assert.IsTrue(error.ToString().Length > 0);
        Assert.AreEqual(_expectedDefaultUsage, output.ToString());

        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        result = StaticParse<TestArguments>(kind, new[] { "-Help" }, options);
        Assert.IsNull(result);
        Assert.AreEqual(0, error.ToString().Length);
        Assert.AreEqual(_expectedDefaultUsage, output.ToString());

        options.ShowUsageOnError = UsageHelpRequest.SyntaxOnly;
        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        result = StaticParse<TestArguments>(kind, Array.Empty<string>(), options);
        Assert.IsNull(result);
        Assert.IsTrue(error.ToString().Length > 0);
        Assert.AreEqual(_expectedUsageSyntaxOnly, output.ToString());

        options.ShowUsageOnError = UsageHelpRequest.None;
        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        result = StaticParse<TestArguments>(kind, Array.Empty<string>(), options);
        Assert.IsNull(result);
        Assert.IsTrue(error.ToString().Length > 0);
        Assert.AreEqual(_expectedUsageMessageOnly, output.ToString());

        // Still get full help with -Help arg.
        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        result = StaticParse<TestArguments>(kind, new[] { "-Help" }, options);
        Assert.IsNull(result);
        Assert.AreEqual(0, error.ToString().Length);
        Assert.AreEqual(_expectedDefaultUsage, output.ToString());
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestCancelParsing(ProviderKind kind)
    {
        var parser = CreateParser<CancelArguments>(kind);

        // Don't cancel if -DoesCancel not specified.
        var result = parser.Parse(new[] { "-Argument1", "foo", "-DoesNotCancel", "-Argument2", "bar" });
        Assert.IsNotNull(result);
        Assert.IsFalse(parser.HelpRequested);
        Assert.IsTrue(result.DoesNotCancel);
        Assert.IsFalse(result.DoesCancel);
        Assert.AreEqual("foo", result.Argument1);
        Assert.AreEqual("bar", result.Argument2);
        Assert.AreEqual(ParseStatus.Success, parser.ParseResult.Status);
        Assert.IsNull(parser.ParseResult.ArgumentName);
        Assert.AreEqual(0, parser.ParseResult.RemainingArguments.Length);

        // Cancel if -DoesCancel specified.
        result = parser.Parse(new[] { "-Argument1", "foo", "-DoesCancel", "-Argument2", "bar" });
        Assert.IsNull(result);
        Assert.IsTrue(parser.HelpRequested);
        Assert.AreEqual(ParseStatus.Canceled, parser.ParseResult.Status);
        Assert.IsNull(parser.ParseResult.LastException);
        AssertSpanEqual(new[] { "-Argument2", "bar" }.AsSpan(), parser.ParseResult.RemainingArguments.Span);
        Assert.AreEqual("DoesCancel", parser.ParseResult.ArgumentName);
        Assert.IsTrue(parser.GetArgument("Argument1")!.HasValue);
        Assert.AreEqual("foo", (string?)parser.GetArgument("Argument1")!.Value);
        Assert.IsTrue(parser.GetArgument("DoesCancel")!.HasValue);
        Assert.IsTrue((bool)parser.GetArgument("DoesCancel")!.Value!);
        Assert.IsFalse(parser.GetArgument("DoesNotCancel")!.HasValue);
        Assert.IsNull(parser.GetArgument("DoesNotCancel")!.Value);
        Assert.IsFalse(parser.GetArgument("Argument2")!.HasValue);
        Assert.IsNull(parser.GetArgument("Argument2")!.Value);

        // Use the event handler to cancel on -DoesNotCancel.
        static void handler1(object? sender, ArgumentParsedEventArgs e)
        {
            if (e.Argument.ArgumentName == "DoesNotCancel")
            {
                e.CancelParsing = CancelMode.Abort;
            }
        }

        parser.ArgumentParsed += handler1;
        result = parser.Parse(new[] { "-Argument1", "foo", "-DoesNotCancel", "-Argument2", "bar" });
        Assert.IsNull(result);
        Assert.AreEqual(ParseStatus.Canceled, parser.ParseResult.Status);
        Assert.IsNull(parser.ParseResult.LastException);
        Assert.AreEqual("DoesNotCancel", parser.ParseResult.ArgumentName);
        AssertSpanEqual(new[] { "-Argument2", "bar" }.AsSpan(), parser.ParseResult.RemainingArguments.Span);
        Assert.IsFalse(parser.HelpRequested);
        Assert.IsTrue(parser.GetArgument("Argument1")!.HasValue);
        Assert.AreEqual("foo", (string?)parser.GetArgument("Argument1")!.Value);
        Assert.IsTrue(parser.GetArgument("DoesNotCancel")!.HasValue);
        Assert.IsTrue((bool)parser.GetArgument("DoesNotCancel")!.Value!);
        Assert.IsFalse(parser.GetArgument("DoesCancel")!.HasValue);
        Assert.IsNull(parser.GetArgument("DoesCancel")!.Value);
        Assert.IsFalse(parser.GetArgument("Argument2")!.HasValue);
        Assert.IsNull(parser.GetArgument("Argument2")!.Value);
        parser.ArgumentParsed -= handler1;

        // Use the event handler to abort cancelling on -DoesCancel.
        static void handler2(object? sender, ArgumentParsedEventArgs e)
        {
            if (e.Argument.ArgumentName == "DoesCancel")
            {
                Assert.AreEqual(CancelMode.Abort, e.CancelParsing);
                e.CancelParsing = CancelMode.None;
            }
        }

        parser.ArgumentParsed += handler2;
        result = parser.Parse(new[] { "-Argument1", "foo", "-DoesCancel", "-Argument2", "bar" });
        Assert.AreEqual(ParseStatus.Success, parser.ParseResult.Status);
        Assert.IsNull(parser.ParseResult.ArgumentName);
        Assert.AreEqual(0, parser.ParseResult.RemainingArguments.Length);
        Assert.IsNotNull(result);
        Assert.IsFalse(parser.HelpRequested);
        Assert.IsFalse(result.DoesNotCancel);
        Assert.IsTrue(result.DoesCancel);
        Assert.AreEqual("foo", result.Argument1);
        Assert.AreEqual("bar", result.Argument2);

        // Automatic help argument should cancel.
        result = parser.Parse(new[] { "-Help" });
        Assert.AreEqual(ParseStatus.Canceled, parser.ParseResult.Status);
        Assert.IsNull(parser.ParseResult.LastException);
        Assert.AreEqual("Help", parser.ParseResult.ArgumentName);
        Assert.AreEqual(0, parser.ParseResult.RemainingArguments.Length);
        Assert.IsNull(result);
        Assert.IsTrue(parser.HelpRequested);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestCancelParsingSuccess(ProviderKind kind)
    {
        var parser = CreateParser<CancelArguments>(kind);
        var result = parser.Parse(new[] { "-Argument1", "foo", "-DoesCancelWithSuccess", "-Argument2", "bar" });
        Assert.AreEqual(ParseStatus.Success, parser.ParseResult.Status);
        Assert.AreEqual("DoesCancelWithSuccess", parser.ParseResult.ArgumentName);
        AssertSpanEqual(new[] { "-Argument2", "bar" }.AsSpan(), parser.ParseResult.RemainingArguments.Span);
        Assert.IsNotNull(result);
        Assert.IsFalse(parser.HelpRequested);
        Assert.IsFalse(result.DoesNotCancel);
        Assert.IsFalse(result.DoesCancel);
        Assert.IsTrue(result.DoesCancelWithSuccess);
        Assert.AreEqual("foo", result.Argument1);
        Assert.IsNull(result.Argument2);

        // No remaining arguments.
        result = parser.Parse(new[] { "-Argument1", "foo", "-DoesCancelWithSuccess" });
        Assert.AreEqual(ParseStatus.Success, parser.ParseResult.Status);
        Assert.AreEqual("DoesCancelWithSuccess", parser.ParseResult.ArgumentName);
        Assert.AreEqual(0, parser.ParseResult.RemainingArguments.Length);
        Assert.IsNotNull(result);
        Assert.IsFalse(parser.HelpRequested);
        Assert.IsFalse(result.DoesNotCancel);
        Assert.IsFalse(result.DoesCancel);
        Assert.IsTrue(result.DoesCancelWithSuccess);
        Assert.AreEqual("foo", result.Argument1);
        Assert.IsNull(result.Argument2);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestParseOptionsAttribute(ProviderKind kind)
    {
        var parser = CreateParser<ParseOptionsArguments>(kind);
        Assert.IsFalse(parser.AllowWhiteSpaceValueSeparator);
        Assert.IsTrue(parser.AllowDuplicateArguments);
        CollectionAssert.AreEquivalent(new[] { '=' }, parser.NameValueSeparators);
        Assert.AreEqual(ParsingMode.LongShort, parser.Mode);
        CollectionAssert.AreEqual(new[] { "--", "-" }, parser.ArgumentNamePrefixes);
        Assert.AreEqual("---", parser.LongArgumentNamePrefix);
        // Verify case sensitivity.
        Assert.IsNull(parser.GetArgument("argument"));
        Assert.IsNotNull(parser.GetArgument("Argument"));
        // Verify no auto help argument.
        Assert.IsNull(parser.GetArgument("Help"));

        // ParseOptions take precedence
        var options = new ParseOptions()
        {
            Mode = ParsingMode.Default,
            ArgumentNameComparison = StringComparison.OrdinalIgnoreCase,
            AllowWhiteSpaceValueSeparator = true,
            DuplicateArguments = ErrorMode.Error,
            NameValueSeparators = new[] { ';' },
            ArgumentNamePrefixes = new[] { "+" },
            AutoHelpArgument = true,
        };

        parser = CreateParser<ParseOptionsArguments>(kind, options);
        Assert.IsTrue(parser.AllowWhiteSpaceValueSeparator);
        Assert.IsFalse(parser.AllowDuplicateArguments);
        CollectionAssert.AreEquivalent(new[] { ';' }, parser.NameValueSeparators);
        Assert.AreEqual(ParsingMode.Default, parser.Mode);
        CollectionAssert.AreEqual(new[] { "+" }, parser.ArgumentNamePrefixes);
        Assert.IsNull(parser.LongArgumentNamePrefix);
        // Verify case insensitivity.
        Assert.IsNotNull(parser.GetArgument("argument"));
        Assert.IsNotNull(parser.GetArgument("Argument"));
        // Verify auto help argument.
        Assert.IsNotNull(parser.GetArgument("Help"));
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestCulture(ProviderKind kind)
    {
        var result = StaticParse<CultureArguments>(kind, new[] { "-Argument", "5.5" });
        Assert.IsNotNull(result);
        Assert.AreEqual(5.5, result.Argument);
        result = StaticParse<CultureArguments>(kind, new[] { "-Argument", "5,5" });
        Assert.IsNotNull(result);
        // , was interpreted as a thousands separator.
        Assert.AreEqual(55, result.Argument);

        var options = new ParseOptions { Culture = new CultureInfo("nl-NL") };
        result = StaticParse<CultureArguments>(kind, new[] { "-Argument", "5,5" }, options);
        Assert.IsNotNull(result);
        Assert.AreEqual(5.5, result.Argument);
        result = StaticParse<CultureArguments>(kind, new[] { "-Argument", "5,5" });
        Assert.IsNotNull(result);
        // . was interpreted as a thousands separator.
        Assert.AreEqual(55, result.Argument);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestLongShortMode(ProviderKind kind)
    {
        var parser = CreateParser<LongShortArguments>(kind);
        Assert.AreEqual(ParsingMode.LongShort, parser.Mode);
        Assert.AreEqual(CommandLineParser.DefaultLongArgumentNamePrefix, parser.LongArgumentNamePrefix);
        CollectionAssert.AreEqual(CommandLineParser.GetDefaultArgumentNamePrefixes(), parser.ArgumentNamePrefixes);
        Assert.AreSame(parser.GetArgument("foo"), parser.GetShortArgument('f'));
        Assert.AreSame(parser.GetArgument("arg2"), parser.GetShortArgument('a'));
        Assert.AreSame(parser.GetArgument("switch1"), parser.GetShortArgument('s'));
        Assert.AreSame(parser.GetArgument("switch2"), parser.GetShortArgument('k'));
        Assert.IsNull(parser.GetArgument("switch3"));
        Assert.AreEqual("u", parser.GetShortArgument('u')!.ArgumentName);
        Assert.AreEqual('f', parser.GetArgument("foo")!.ShortName);
        Assert.IsTrue(parser.GetArgument("foo")!.HasShortName);
        Assert.AreEqual('\0', parser.GetArgument("bar")!.ShortName);
        Assert.IsFalse(parser.GetArgument("bar")!.HasShortName);

        var result = CheckSuccess(parser, new[] { "-f", "5", "--bar", "6", "-a", "7", "--arg1", "8", "-s" });
        Assert.AreEqual(5, result.Foo);
        Assert.AreEqual(6, result.Bar);
        Assert.AreEqual(7, result.Arg2);
        Assert.AreEqual(8, result.Arg1);
        Assert.IsTrue(result.Switch1);
        Assert.IsFalse(result.Switch2);
        Assert.IsFalse(result.Switch3);

        // Combine switches.
        result = CheckSuccess(parser, new[] { "-su" });
        Assert.IsTrue(result.Switch1);
        Assert.IsFalse(result.Switch2);
        Assert.IsTrue(result.Switch3);

        // Use a short alias.
        result = CheckSuccess(parser, new[] { "-b", "5" });
        Assert.AreEqual(5, result.Arg2);

        // Combining non-switches is an error.
        CheckThrows(parser, new[] { "-sf" }, CommandLineArgumentErrorCategory.CombinedShortNameNonSwitch, "sf", remainingArgumentCount: 1);

        // Can't use long argument prefix with short names.
        CheckThrows(parser, new[] { "--s" }, CommandLineArgumentErrorCategory.UnknownArgument, "s", remainingArgumentCount: 1);

        // And vice versa.
        CheckThrows(parser, new[] { "-Switch1" }, CommandLineArgumentErrorCategory.UnknownArgument, "w", remainingArgumentCount: 1);

        // Short alias is ignored on an argument without a short name.
        CheckThrows(parser, new[] { "-c" }, CommandLineArgumentErrorCategory.UnknownArgument, "c", remainingArgumentCount: 1);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestMethodArguments(ProviderKind kind)
    {
        var parser = CreateParser<MethodArguments>(kind);

        Assert.AreEqual(ArgumentKind.Method, parser.GetArgument("NoCancel")!.Kind);
        Assert.IsNull(parser.GetArgument("NotAnArgument"));
        Assert.IsNull(parser.GetArgument("NotStatic"));
        Assert.IsNull(parser.GetArgument("NotPublic"));

        CheckSuccess(parser, new[] { "-NoCancel" });
        Assert.AreEqual(nameof(MethodArguments.NoCancel), MethodArguments.CalledMethodName);

        CheckCanceled(parser, new[] { "-Cancel", "Foo" }, "Cancel", false, 1);
        Assert.AreEqual(nameof(MethodArguments.Cancel), MethodArguments.CalledMethodName);

        CheckCanceled(parser, new[] { "-CancelWithHelp" }, "CancelWithHelp", true, 0);
        Assert.AreEqual(nameof(MethodArguments.CancelWithHelp), MethodArguments.CalledMethodName);

        CheckSuccess(parser, new[] { "-CancelWithValue", "1" });
        Assert.AreEqual(nameof(MethodArguments.CancelWithValue), MethodArguments.CalledMethodName);
        Assert.AreEqual(1, MethodArguments.Value);

        CheckCanceled(parser, new[] { "-CancelWithValue", "-1" }, "CancelWithValue", false);
        Assert.AreEqual(nameof(MethodArguments.CancelWithValue), MethodArguments.CalledMethodName);
        Assert.AreEqual(-1, MethodArguments.Value);

        CheckSuccess(parser, new[] { "-CancelWithValueAndHelp", "1" });
        Assert.AreEqual(nameof(MethodArguments.CancelWithValueAndHelp), MethodArguments.CalledMethodName);
        Assert.AreEqual(1, MethodArguments.Value);

        CheckCanceled(parser, new[] { "-CancelWithValueAndHelp", "-1", "bar" }, "CancelWithValueAndHelp", true, 1);
        Assert.AreEqual(nameof(MethodArguments.CancelWithValueAndHelp), MethodArguments.CalledMethodName);
        Assert.AreEqual(-1, MethodArguments.Value);

        CheckSuccess(parser, new[] { "-NoReturn" });
        Assert.AreEqual(nameof(MethodArguments.NoReturn), MethodArguments.CalledMethodName);

        CheckSuccess(parser, new[] { "42" });
        Assert.AreEqual(nameof(MethodArguments.Positional), MethodArguments.CalledMethodName);
        Assert.AreEqual(42, MethodArguments.Value);

        CheckCanceled(parser, new[] { "-CancelModeAbort", "Foo" }, "CancelModeAbort", false, 1);
        Assert.AreEqual(nameof(MethodArguments.CancelModeAbort), MethodArguments.CalledMethodName);

        CheckSuccess(parser, new[] { "-CancelModeSuccess", "Foo" }, "CancelModeSuccess", 1);
        Assert.AreEqual(nameof(MethodArguments.CancelModeSuccess), MethodArguments.CalledMethodName);

        CheckSuccess(parser, new[] { "-CancelModeNone" });
        Assert.AreEqual(nameof(MethodArguments.CancelModeNone), MethodArguments.CalledMethodName);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestAutomaticArgumentConflict(ProviderKind kind)
    {
        CommandLineParser parser = CreateParser<AutomaticConflictingNameArguments>(kind);
        VerifyArgument(parser.GetArgument("Help"), new ExpectedArgument("Help", typeof(int)));
        VerifyArgument(parser.GetArgument("Version"), new ExpectedArgument("Version", typeof(int)));

        parser = CreateParser<AutomaticConflictingShortNameArguments>(kind);
        VerifyArgument(parser.GetShortArgument('?'), new ExpectedArgument("Foo", typeof(int)) { ShortName = '?' });
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestHiddenArgument(ProviderKind kind)
    {
        var parser = CreateParser<HiddenArguments>(kind);

        // Verify the hidden argument exists.
        VerifyArgument(parser.GetArgument("Hidden"), new ExpectedArgument("Hidden", typeof(int)) { IsHidden = true });

        // Verify it's not in the usage.
        var options = new UsageWriter()
        {
            ExecutableName = _executableName,
            ArgumentDescriptionListFilter = DescriptionListFilterMode.All,
        };

        var usage = parser.GetUsage(options);
        Assert.AreEqual(_expectedUsageHidden, usage);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestNameTransformPascalCase(ProviderKind kind)
    {
        var options = new ParseOptions
        {
            ArgumentNameTransform = NameTransform.PascalCase
        };

        var parser = CreateParser<NameTransformArguments>(kind, options);
        VerifyArguments(parser.Arguments, new[]
        {
            new ExpectedArgument("TestArg", typeof(string)) { MemberName = "testArg", Position = 0, IsRequired = true },
            new ExpectedArgument("ExplicitName", typeof(int)) { MemberName = "Explicit" },
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
            new ExpectedArgument("TestArg2", typeof(int)) { MemberName = "TestArg2" },
            new ExpectedArgument("TestArg3", typeof(int)) { MemberName = "__test__arg3__" },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        });
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestNameTransformCamelCase(ProviderKind kind)
    {
        var options = new ParseOptions
        {
            ArgumentNameTransform = NameTransform.CamelCase
        };

        var parser = CreateParser<NameTransformArguments>(kind, options);
        VerifyArguments(parser.Arguments, new[]
        {
            new ExpectedArgument("testArg", typeof(string)) { MemberName = "testArg", Position = 0, IsRequired = true },
            new ExpectedArgument("ExplicitName", typeof(int)) { MemberName = "Explicit" },
            new ExpectedArgument("help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
            new ExpectedArgument("testArg2", typeof(int)) { MemberName = "TestArg2" },
            new ExpectedArgument("testArg3", typeof(int)) { MemberName = "__test__arg3__" },
            new ExpectedArgument("version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        });
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestNameTransformSnakeCase(ProviderKind kind)
    {
        var options = new ParseOptions
        {
            ArgumentNameTransform = NameTransform.SnakeCase
        };

        var parser = CreateParser<NameTransformArguments>(kind, options);
        VerifyArguments(parser.Arguments, new[]
        {
            new ExpectedArgument("test_arg", typeof(string)) { MemberName = "testArg", Position = 0, IsRequired = true },
            new ExpectedArgument("ExplicitName", typeof(int)) { MemberName = "Explicit" },
            new ExpectedArgument("help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
            new ExpectedArgument("test_arg2", typeof(int)) { MemberName = "TestArg2" },
            new ExpectedArgument("test_arg3", typeof(int)) { MemberName = "__test__arg3__" },
            new ExpectedArgument("version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        });
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestNameTransformDashCase(ProviderKind kind)
    {
        var options = new ParseOptions
        {
            ArgumentNameTransform = NameTransform.DashCase
        };

        var parser = CreateParser<NameTransformArguments>(kind, options);
        VerifyArguments(parser.Arguments, new[]
        {
            new ExpectedArgument("test-arg", typeof(string)) { MemberName = "testArg", Position = 0, IsRequired = true },
            new ExpectedArgument("ExplicitName", typeof(int)) { MemberName = "Explicit" },
            new ExpectedArgument("help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
            new ExpectedArgument("test-arg2", typeof(int)) { MemberName = "TestArg2" },
            new ExpectedArgument("test-arg3", typeof(int)) { MemberName = "__test__arg3__" },
            new ExpectedArgument("version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        });
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestValueDescriptionTransform(ProviderKind kind)
    {
        var options = new ParseOptions
        {
            ValueDescriptionTransform = NameTransform.DashCase
        };

        var parser = CreateParser<ValueDescriptionTransformArguments>(kind, options);
        VerifyArguments(parser.Arguments, new[]
        {
            new ExpectedArgument("Arg1", typeof(FileInfo)) { ValueDescription = "file-info" },
            new ExpectedArgument("Arg2", typeof(int)) { ValueDescription = "int32" },
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { ValueDescription = "boolean", MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { ValueDescription = "boolean", MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        });
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestValidation(ProviderKind kind)
    {
        // Reset for multiple runs.
        ValidationArguments.Arg3Value = 0;
        var parser = CreateParser<ValidationArguments>(kind);

        // Range validator on property
        CheckThrows(parser, new[] { "-Arg1", "0" }, CommandLineArgumentErrorCategory.ValidationFailed, "Arg1", remainingArgumentCount: 2);
        var result = CheckSuccess(parser, new[] { "-Arg1", "1" });
        Assert.AreEqual(1, result.Arg1);
        result = CheckSuccess(parser, new[] { "-Arg1", "5" });
        Assert.AreEqual(5, result.Arg1);
        CheckThrows(parser, new[] { "-Arg1", "6" }, CommandLineArgumentErrorCategory.ValidationFailed, "Arg1", remainingArgumentCount: 2);

        // Not null or empty on ctor parameter
        CheckThrows(parser, new[] { "" }, CommandLineArgumentErrorCategory.ValidationFailed, "arg2", remainingArgumentCount: 1);
        result = CheckSuccess(parser, new[] { " " });
        Assert.AreEqual(" ", result.Arg2);

        // Multiple validators on method
        CheckThrows(parser, new[] { "-Arg3", "1238" }, CommandLineArgumentErrorCategory.ValidationFailed, "Arg3", remainingArgumentCount: 2);
        Assert.AreEqual(0, ValidationArguments.Arg3Value);
        CheckThrows(parser, new[] { "-Arg3", "123" }, CommandLineArgumentErrorCategory.ValidationFailed, "Arg3", remainingArgumentCount: 2);
        Assert.AreEqual(0, ValidationArguments.Arg3Value);
        CheckThrows(parser, new[] { "-Arg3", "7001" }, CommandLineArgumentErrorCategory.ValidationFailed, "Arg3", remainingArgumentCount: 2);
        // Range validation is done after setting the value, so this was set!
        Assert.AreEqual(7001, ValidationArguments.Arg3Value);
        CheckSuccess(parser, new[] { "-Arg3", "1023" });
        Assert.AreEqual(1023, ValidationArguments.Arg3Value);

        // Validator on multi-value argument
        CheckThrows(parser, new[] { "-Arg4", "foo;bar;bazz" }, CommandLineArgumentErrorCategory.ValidationFailed, "Arg4", remainingArgumentCount: 2);
        CheckThrows(parser, new[] { "-Arg4", "foo", "-Arg4", "bar", "-Arg4", "bazz" }, CommandLineArgumentErrorCategory.ValidationFailed, "Arg4", remainingArgumentCount: 2);
        result = CheckSuccess(parser, new[] { "-Arg4", "foo;bar" });
        CollectionAssert.AreEqual(new[] { "foo", "bar" }, result.Arg4);
        result = CheckSuccess(parser, new[] { "-Arg4", "foo", "-Arg4", "bar" });
        CollectionAssert.AreEqual(new[] { "foo", "bar" }, result.Arg4);

        // Count validator
        // No remaining arguments because validation happens after parsing.
        CheckThrows(parser, new[] { "-Arg4", "foo" }, CommandLineArgumentErrorCategory.ValidationFailed, "Arg4");
        CheckThrows(parser, new[] { "-Arg4", "foo;bar;baz;ban;bap" }, CommandLineArgumentErrorCategory.ValidationFailed, "Arg4");
        result = CheckSuccess(parser, new[] { "-Arg4", "foo;bar;baz;ban" });
        CollectionAssert.AreEqual(new[] { "foo", "bar", "baz", "ban" }, result.Arg4);

        // Enum validator
        CheckThrows(parser, new[] { "-Day", "foo" }, CommandLineArgumentErrorCategory.ArgumentValueConversion, "Day", typeof(ArgumentException), remainingArgumentCount: 2);
        CheckThrows(parser, new[] { "-Day", "9" }, CommandLineArgumentErrorCategory.ValidationFailed, "Day", remainingArgumentCount: 2);
        CheckThrows(parser, new[] { "-Day", "" }, CommandLineArgumentErrorCategory.ArgumentValueConversion, "Day", typeof(ArgumentException), remainingArgumentCount: 2);
        result = CheckSuccess(parser, new[] { "-Day", "1" });
        Assert.AreEqual(DayOfWeek.Monday, result.Day);
        CheckThrows(parser, new[] { "-Day2", "foo" }, CommandLineArgumentErrorCategory.ArgumentValueConversion, "Day2", typeof(ArgumentException), remainingArgumentCount: 2);
        CheckThrows(parser, new[] { "-Day2", "9" }, CommandLineArgumentErrorCategory.ValidationFailed, "Day2", remainingArgumentCount: 2);
        result = CheckSuccess(parser, new[] { "-Day2", "1" });
        Assert.AreEqual(DayOfWeek.Monday, result.Day2);
        result = CheckSuccess(parser, new[] { "-Day2", "" });
        Assert.IsNull(result.Day2);

        // NotNull validator with Nullable<T>.
        CheckThrows(parser, new[] { "-NotNull", "" }, CommandLineArgumentErrorCategory.ValidationFailed, "NotNull", remainingArgumentCount: 2);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestRequires(ProviderKind kind)
    {
        var parser = CreateParser<DependencyArguments>(kind);

        // None of these have remaining arguments because validation happens after parsing.
        var result = CheckSuccess(parser, new[] { "-Address", "127.0.0.1" });
        Assert.AreEqual(IPAddress.Loopback, result.Address);
        CheckThrows(parser, new[] { "-Port", "9000" }, CommandLineArgumentErrorCategory.DependencyFailed, "Port");
        result = CheckSuccess(parser, new[] { "-Address", "127.0.0.1", "-Port", "9000" });
        Assert.AreEqual(IPAddress.Loopback, result.Address);
        Assert.AreEqual(9000, result.Port);
        CheckThrows(parser, new[] { "-Protocol", "1" }, CommandLineArgumentErrorCategory.DependencyFailed, "Protocol");
        CheckThrows(parser, new[] { "-Address", "127.0.0.1", "-Protocol", "1" }, CommandLineArgumentErrorCategory.DependencyFailed, "Protocol");
        CheckThrows(parser, new[] { "-Throughput", "10", "-Protocol", "1" }, CommandLineArgumentErrorCategory.DependencyFailed, "Protocol");
        result = CheckSuccess(parser, new[] { "-Protocol", "1", "-Address", "127.0.0.1", "-Throughput", "10" });
        Assert.AreEqual(IPAddress.Loopback, result.Address);
        Assert.AreEqual(10, result.Throughput);
        Assert.AreEqual(1, result.Protocol);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestProhibits(ProviderKind kind)
    {
        var parser = CreateParser<DependencyArguments>(kind);

        var result = CheckSuccess(parser, new[] { "-Path", "test" });
        Assert.AreEqual("test", result.Path.Name);
        // No remaining arguments because validation happens after parsing.
        CheckThrows(parser, new[] { "-Path", "test", "-Address", "127.0.0.1" }, CommandLineArgumentErrorCategory.DependencyFailed, "Path");
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestRequiresAny(ProviderKind kind)
    {
        var parser = CreateParser<DependencyArguments>(kind);

        // No need to check if the arguments work indivially since TestRequires and TestProhibits already did that.
        CheckThrows(parser, Array.Empty<string>(), CommandLineArgumentErrorCategory.MissingRequiredArgument);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestValidatorUsageHelp(ProviderKind kind)
    {
        CommandLineParser parser = CreateParser<ValidationArguments>(kind);
        var options = new UsageWriter()
        {
            ExecutableName = _executableName,
        };

        Assert.AreEqual(_expectedUsageValidators, parser.GetUsage(options));

        parser = CreateParser<DependencyArguments>(kind);
        Assert.AreEqual(_expectedUsageDependencies, parser.GetUsage(options));

        options.IncludeValidatorsInDescription = false;
        Assert.AreEqual(_expectedUsageDependenciesDisabled, parser.GetUsage(options));
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestDefaultValueDescriptions(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            DefaultValueDescriptions = new Dictionary<Type, string>()
            {
                { typeof(bool), "Switch" },
                { typeof(int), "Number" },
            },
        };

        var parser = CreateParser<TestArguments>(kind, options);
        Assert.AreEqual("Switch", parser.GetArgument("Arg7")!.ValueDescription);
        Assert.AreEqual("Number", parser.GetArgument("Arg9")!.ValueDescription);
        Assert.AreEqual("String=Number", parser.GetArgument("Arg13")!.ValueDescription);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestMultiValueWhiteSpaceSeparator(ProviderKind kind)
    {
        var parser = CreateParser<MultiValueWhiteSpaceArguments>(kind);
        Assert.IsTrue(parser.GetArgument("Multi")!.AllowMultiValueWhiteSpaceSeparator);
        Assert.IsFalse(parser.GetArgument("MultiSwitch")!.AllowMultiValueWhiteSpaceSeparator);
        Assert.IsFalse(parser.GetArgument("Other")!.AllowMultiValueWhiteSpaceSeparator);

        var result = CheckSuccess(parser, new[] { "1", "-Multi", "2", "3", "4", "-Other", "5", "6" });
        Assert.AreEqual(result.Arg1, 1);
        Assert.AreEqual(result.Arg2, 6);
        Assert.AreEqual(result.Other, 5);
        CollectionAssert.AreEqual(new[] { 2, 3, 4 }, result.Multi);

        result = CheckSuccess(parser, new[] { "-Multi", "1", "-Multi", "2" });
        CollectionAssert.AreEqual(new[] { 1, 2 }, result.Multi);

        CheckThrows(parser, new[] { "1", "-Multi", "-Other", "5", "6" }, CommandLineArgumentErrorCategory.MissingNamedArgumentValue, "Multi", remainingArgumentCount: 4);
        CheckThrows(parser, new[] { "-MultiSwitch", "true", "false" }, CommandLineArgumentErrorCategory.ArgumentValueConversion, "Arg1", typeof(FormatException), remainingArgumentCount: 2);
        parser.Options.AllowWhiteSpaceValueSeparator = false;
        CheckThrows(parser, new[] { "1", "-Multi:2", "2", "3", "4", "-Other", "5", "6" }, CommandLineArgumentErrorCategory.TooManyArguments, remainingArgumentCount: 5);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestInjection(ProviderKind kind)
    {
        var parser = CreateParser<InjectionArguments>(kind);
        var result = CheckSuccess(parser, new[] { "-Arg", "1" });
        Assert.AreSame(parser, result.Parser);
        Assert.AreEqual(1, result.Arg);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestDuplicateArguments(ProviderKind kind)
    {
        var parser = CreateParser<SimpleArguments>(kind);
        CheckThrows(parser, new[] { "-Argument1", "foo", "-Argument1", "bar" }, CommandLineArgumentErrorCategory.DuplicateArgument, "Argument1", remainingArgumentCount: 2);
        parser.Options.DuplicateArguments = ErrorMode.Allow;
        var result = CheckSuccess(parser, new[] { "-Argument1", "foo", "-Argument1", "bar" });
        Assert.AreEqual("bar", result.Argument1);

        bool handlerCalled = false;
        bool keepOldValue = false;
        EventHandler<DuplicateArgumentEventArgs> handler = (sender, e) =>
        {
            Assert.AreEqual("Argument1", e.Argument.ArgumentName);
            Assert.AreEqual("foo", e.Argument.Value);
            Assert.AreEqual("bar", e.NewValue);
            handlerCalled = true;
            if (keepOldValue)
            {
                e.KeepOldValue = true;
            }
        };

        parser.DuplicateArgument += handler;

        // Handler is not called when duplicates not allowed.
        parser.Options.DuplicateArguments = ErrorMode.Error;
        CheckThrows(parser, new[] { "-Argument1", "foo", "-Argument1", "bar" }, CommandLineArgumentErrorCategory.DuplicateArgument, "Argument1", remainingArgumentCount: 2);
        Assert.IsFalse(handlerCalled);

        // Now it is called.
        parser.Options.DuplicateArguments = ErrorMode.Allow;
        result = CheckSuccess(parser, new[] { "-Argument1", "foo", "-Argument1", "bar" });
        Assert.AreEqual("bar", result.Argument1);
        Assert.IsTrue(handlerCalled);

        // Also called for warning, and keep the old value.
        parser.Options.DuplicateArguments = ErrorMode.Warning;
        handlerCalled = false;
        keepOldValue = true;
        result = CheckSuccess(parser, new[] { "-Argument1", "foo", "-Argument1", "bar" });
        Assert.AreEqual("foo", result.Argument1);
        Assert.IsTrue(handlerCalled);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestConversion(ProviderKind kind)
    {
        var parser = CreateParser<ConversionArguments>(kind);
        var result = CheckSuccess(parser, "-ParseCulture 1 -ParseStruct 2 -Ctor 3 -ParseNullable 4 -ParseMulti 5 6 -ParseNullableMulti 7 8 -NullableMulti 9 10 -Nullable 11".Split(' '));
        Assert.AreEqual(1, result.ParseCulture.Value);
        Assert.AreEqual(2, result.ParseStruct.Value);
        Assert.AreEqual(3, result.Ctor.Value);
        Assert.AreEqual(4, result.ParseNullable!.Value.Value);
        Assert.AreEqual(5, result.ParseMulti[0].Value);
        Assert.AreEqual(6, result.ParseMulti[1].Value);
        Assert.AreEqual(7, result.ParseNullableMulti[0]!.Value.Value);
        Assert.AreEqual(8, result.ParseNullableMulti[1]!.Value.Value);
        Assert.AreEqual(9, result.NullableMulti[0]!.Value);
        Assert.AreEqual(10, result.NullableMulti[1]!.Value);
        Assert.AreEqual(11, result.Nullable);

        result = CheckSuccess(parser, new[] { "-ParseNullable", "", "-NullableMulti", "1", "", "2", "-ParseNullableMulti", "3", "", "4" });
        Assert.IsNull(result.ParseNullable);
        Assert.AreEqual(1, result.NullableMulti[0]!.Value);
        Assert.IsNull(result.NullableMulti[1]);
        Assert.AreEqual(2, result.NullableMulti[2]!.Value);
        Assert.AreEqual(3, result.ParseNullableMulti[0]!.Value.Value);
        Assert.IsNull(result.ParseNullableMulti[1]!);
        Assert.AreEqual(4, result.ParseNullableMulti[2]!.Value.Value);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestDerivedClass(ProviderKind kind)
    {
        var parser = CreateParser<DerivedArguments>(kind);
        Assert.AreEqual("Base class attribute.", parser.Description);
        Assert.AreEqual(4, parser.Arguments.Length);
        VerifyArguments(parser.Arguments, new[]
        {
            new ExpectedArgument("BaseArg", typeof(string), ArgumentKind.SingleValue),
            new ExpectedArgument("DerivedArg", typeof(int), ArgumentKind.SingleValue),
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        });
    }

    [TestMethod]
    public void TestInitializerDefaultValues()
    {
        var parser = InitializerDefaultValueArguments.CreateParser();
        Assert.AreEqual("foo\tbar\"", parser.GetArgument("Arg1")!.DefaultValue);
        Assert.AreEqual(5.5f, parser.GetArgument("Arg2")!.DefaultValue);
        Assert.AreEqual(int.MaxValue, parser.GetArgument("Arg3")!.DefaultValue);
        Assert.AreEqual(DayOfWeek.Tuesday, parser.GetArgument("Arg4")!.DefaultValue);
        Assert.AreEqual(47, parser.GetArgument("Arg5")!.DefaultValue);
        // Does not use a supported expression type.
        Assert.IsNull(parser.GetArgument("Arg6")!.DefaultValue);
        Assert.AreEqual(0, parser.GetArgument("Arg7")!.DefaultValue);
        // Null because set to "default".
        Assert.IsNull(parser.GetArgument("Arg8")!.DefaultValue);
        // Null because explicit null.
        Assert.IsNull(parser.GetArgument("Arg9")!.DefaultValue);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestAutoPrefixAliases(ProviderKind kind)
    {
        var parser = CreateParser<AutoPrefixAliasesArguments>(kind);

        // Shortest possible prefixes
        var result = parser.Parse(new[] { "-pro", "foo", "-Po", "5", "-e" });
        Assert.IsNotNull(result);
        Assert.AreEqual("foo", result.Protocol);
        Assert.AreEqual(5, result.Port);
        Assert.IsTrue(result.EnablePrefix);

        // Ambiguous prefix
        CheckThrows(parser, new[] { "-p", "foo" }, CommandLineArgumentErrorCategory.UnknownArgument, "p", remainingArgumentCount: 2);

        // Ambiguous due to alias.
        CheckThrows(parser, new[] { "-pr", "foo" }, CommandLineArgumentErrorCategory.UnknownArgument, "pr", remainingArgumentCount: 2);

        // Prefix of an alias.
        result = parser.Parse(new[] { "-pre" });
        Assert.IsNotNull(result);
        Assert.IsTrue(result.EnablePrefix);

        // Disable auto prefix aliases.
        var options = new ParseOptions() { AutoPrefixAliases = false };
        parser = CreateParser<AutoPrefixAliasesArguments>(kind, options);
        CheckThrows(parser, new[] { "-pro", "foo", "-Po", "5", "-e" }, CommandLineArgumentErrorCategory.UnknownArgument, "pro", remainingArgumentCount: 5);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestApplicationFriendlyName(ProviderKind kind)
    {
        CommandLineParser parser = CreateParser<TestArguments>(kind);
        Assert.AreEqual("Friendly name", parser.ApplicationFriendlyName);

        // Default to assembly title if no friendly name.
        parser = CreateParser<SimpleArguments>(kind);
        Assert.AreEqual("Ookii.CommandLine Unit Tests", parser.ApplicationFriendlyName);

        parser = CreateParser<ExternalCommand>(kind);
        Assert.AreEqual("Ookii.CommandLine.Tests.Commands", parser.ApplicationFriendlyName);
    }

    [TestMethod]
    public void TestAutoPosition()
    {
        var parser = AutoPositionArguments.CreateParser();
        VerifyArguments(parser.Arguments, new[]
        {
            new ExpectedArgument("BaseArg1", typeof(string), ArgumentKind.SingleValue) { Position = 0, IsRequired = true },
            new ExpectedArgument("BaseArg2", typeof(int), ArgumentKind.SingleValue) { Position = 1 },
            new ExpectedArgument("Arg1", typeof(string), ArgumentKind.SingleValue) { Position = 2 },
            new ExpectedArgument("Arg2", typeof(int), ArgumentKind.SingleValue) { Position = 3 },
            new ExpectedArgument("Arg3", typeof(int), ArgumentKind.SingleValue),
            new ExpectedArgument("BaseArg3", typeof(int), ArgumentKind.SingleValue),
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        });

        try
        {
            parser = new CommandLineParser<AutoPositionArguments>();
            Debug.Fail("Expected exception not thrown.");
        }
        catch (NotSupportedException)
        {
        }
    }

    private class ExpectedArgument
    {
        public ExpectedArgument(string name, Type type, ArgumentKind kind = ArgumentKind.SingleValue)
        {
            Name = name;
            Type = type;
            Kind = kind;
        }

        public string Name { get; set; }
        public string? MemberName { get; set; }
        public Type Type { get; set; }
        public Type? ElementType { get; set; }
        public int? Position { get; set; }
        public bool IsRequired { get; set; }
        public object? DefaultValue { get; set; }
        public string? Description { get; set; }
        public string? ValueDescription { get; set; }
        public bool IsSwitch { get; set; }
        public ArgumentKind Kind { get; set; }
        public string[]? Aliases { get; set; }
        public char? ShortName { get; set; }
        public char[]? ShortAliases { get; set; }
        public bool IsHidden { get; set; }
    }

    private static void VerifyArgument(CommandLineArgument? argument, ExpectedArgument expected)
    {
        Assert.IsNotNull(argument);
        Assert.AreEqual(expected.Name, argument.ArgumentName);
        Assert.AreEqual(expected.MemberName ?? expected.Name, argument.MemberName);
        Assert.AreEqual(expected.ShortName.HasValue, argument.HasShortName);
        Assert.AreEqual(expected.ShortName ?? '\0', argument.ShortName);
        Assert.AreEqual(expected.Type, argument.ArgumentType);
        Assert.AreEqual(expected.ElementType ?? expected.Type, argument.ElementType);
        Assert.AreEqual(expected.Position, argument.Position);
        Assert.AreEqual(expected.IsRequired, argument.IsRequired);
        Assert.AreEqual(expected.Description ?? string.Empty, argument.Description);
        Assert.AreEqual(expected.ValueDescription ?? argument.ElementType.Name, argument.ValueDescription);
        Assert.AreEqual(expected.Kind, argument.Kind);
        Assert.AreEqual(expected.Kind == ArgumentKind.MultiValue || expected.Kind == ArgumentKind.Dictionary, argument.IsMultiValue);
        Assert.AreEqual(expected.Kind == ArgumentKind.Dictionary, argument.IsDictionary);
        Assert.AreEqual(expected.IsSwitch, argument.IsSwitch);
        Assert.AreEqual(expected.DefaultValue, argument.DefaultValue);
        Assert.AreEqual(expected.IsHidden, argument.IsHidden);
        Assert.IsFalse(argument.AllowMultiValueWhiteSpaceSeparator);
        Assert.IsNull(argument.Value);
        Assert.IsFalse(argument.HasValue);
        CollectionAssert.AreEqual(expected.Aliases ?? Array.Empty<string>(), argument.Aliases);
        CollectionAssert.AreEqual(expected.ShortAliases ?? Array.Empty<char>(), argument.ShortAliases);
    }

    private static void VerifyArguments(IEnumerable<CommandLineArgument> arguments, ExpectedArgument[] expected)
    {
        int index = 0;
        foreach (var arg in arguments)
        {
            Assert.IsTrue(index < expected.Length, "Too many arguments.");
            VerifyArgument(arg, expected[index]);
            ++index;
        }

        Assert.AreEqual(expected.Length, index);
    }

    private static void TestParse(CommandLineParser<TestArguments> target, string commandLine, string? arg1 = null, int arg2 = 42, bool notSwitch = false, string? arg3 = null, int arg4 = 47, float arg5 = 1.0f, string? arg6 = null, bool arg7 = false, DayOfWeek[]? arg8 = null, int? arg9 = null, bool[]? arg10 = null, bool? arg11 = null, int[]? arg12 = null, Dictionary<string, int>? arg13 = null, Dictionary<string, int>? arg14 = null, KeyValuePair<string, int>? arg15 = null)
    {
        string[] args = commandLine.Split(' '); // not using quoted arguments in the tests, so this is fine.
        var result = target.Parse(args);
        Assert.IsNotNull(result);
        Assert.AreEqual(ParseStatus.Success, target.ParseResult.Status);
        Assert.IsNull(target.ParseResult.LastException);
        Assert.IsNull(target.ParseResult.ArgumentName);
        Assert.AreEqual(0, target.ParseResult.RemainingArguments.Length);
        Assert.IsFalse(target.HelpRequested);
        Assert.AreEqual(arg1, result.Arg1);
        Assert.AreEqual(arg2, result.Arg2);
        Assert.AreEqual(arg3, result.Arg3);
        Assert.AreEqual(arg4, result.Arg4);
        Assert.AreEqual(arg5, result.Arg5);
        Assert.AreEqual(arg6, result.Arg6);
        Assert.AreEqual(arg7, result.Arg7);
        CollectionAssert.AreEqual(arg8, result.Arg8);
        Assert.AreEqual(arg9, result.Arg9);
        CollectionAssert.AreEqual(arg10, result.Arg10);
        Assert.AreEqual(arg11, result.Arg11);
        Assert.AreEqual(notSwitch, result.NotSwitch);
        if (arg12 == null)
        {
            Assert.AreEqual(0, result.Arg12.Count);
        }
        else
        {
            CollectionAssert.AreEqual(arg12, result.Arg12);
        }

        CollectionAssert.AreEqual(arg13, result.Arg13);
        if (arg14 == null)
        {
            Assert.AreEqual(0, result.Arg14.Count);
        }
        else
        {
            CollectionAssert.AreEqual(arg14, (System.Collections.ICollection)result.Arg14);
        }

        if (arg15 == null)
        {
            Assert.AreEqual(default, result.Arg15);
        }
        else
        {
            Assert.AreEqual(arg15.Value, result.Arg15);
        }
    }

    private static void CheckThrows(CommandLineParser parser, string[] arguments, CommandLineArgumentErrorCategory category, string? argumentName = null, Type? innerExceptionType = null, int remainingArgumentCount = 0)
    {
        try
        {
            parser.Parse(arguments);
            Assert.Fail("Expected CommandLineException was not thrown.");
        }
        catch (CommandLineArgumentException ex)
        {
            Assert.IsTrue(parser.HelpRequested);
            Assert.AreEqual(ParseStatus.Error, parser.ParseResult.Status);
            Assert.AreEqual(ex, parser.ParseResult.LastException);
            Assert.AreEqual(ex.ArgumentName, parser.ParseResult.LastException!.ArgumentName);
            Assert.AreEqual(category, ex.Category);
            Assert.AreEqual(argumentName, ex.ArgumentName);
            if (innerExceptionType == null)
            {
                Assert.IsNull(ex.InnerException);
            }
            else
            {
                Assert.IsInstanceOfType(ex.InnerException, innerExceptionType);
            }

            var remaining = arguments.AsMemory(arguments.Length - remainingArgumentCount);
            AssertMemoryEqual(remaining, parser.ParseResult.RemainingArguments);
        }
    }

    private static void CheckCanceled(CommandLineParser parser, string[] arguments, string argumentName, bool helpRequested, int remainingArgumentCount = 0)
    {
        Assert.IsNull(parser.Parse(arguments));
        Assert.AreEqual(ParseStatus.Canceled, parser.ParseResult.Status);
        Assert.AreEqual(argumentName, parser.ParseResult.ArgumentName);
        Assert.AreEqual(helpRequested, parser.HelpRequested);
        Assert.IsNull(parser.ParseResult.LastException);
        var remaining = arguments.AsMemory(arguments.Length - remainingArgumentCount);
        AssertMemoryEqual(remaining, parser.ParseResult.RemainingArguments);
    }

    private static T CheckSuccess<T>(CommandLineParser<T> parser, string[] arguments, string? argumentName = null, int remainingArgumentCount = 0)
        where T : class
    {
        var result = parser.Parse(arguments);
        Assert.IsNotNull(result);
        Assert.IsFalse(parser.HelpRequested);
        Assert.AreEqual(ParseStatus.Success, parser.ParseResult.Status);
        Assert.AreEqual(argumentName, parser.ParseResult.ArgumentName);
        Assert.IsNull(parser.ParseResult.LastException);
        var remaining = arguments.AsMemory(arguments.Length - remainingArgumentCount);
        AssertMemoryEqual(remaining, parser.ParseResult.RemainingArguments);
        return result;
    }

    internal static CommandLineParser<T> CreateParser<T>(ProviderKind kind, ParseOptions? options = null)
#if NET7_0_OR_GREATER
        where T : class, IParserProvider<T>
#else
        where T : class
#endif
    {
        var parser = kind switch
        {
            ProviderKind.Reflection => new CommandLineParser<T>(options),
#if NET7_0_OR_GREATER
            ProviderKind.Generated => T.CreateParser(options),
#else
            ProviderKind.Generated => (CommandLineParser<T>)typeof(T).InvokeMember("CreateParser", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object?[] { options })!,
#endif
            _ => throw new InvalidOperationException()
        };

        Assert.AreEqual(kind, parser.ProviderKind);
        return parser;
    }

    private static T? StaticParse<T>(ProviderKind kind, string[] args, ParseOptions? options = null)
#if NET7_0_OR_GREATER
        where T : class, IParser<T>
#else
        where T : class
#endif
    {
        return kind switch
        {
            ProviderKind.Reflection => CommandLineParser.Parse<T>(args, options),
#if NET7_0_OR_GREATER
            ProviderKind.Generated => T.Parse(args, options),
#else
            ProviderKind.Generated => (T?)typeof(T).InvokeMember("Parse", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object?[] { args, options }),
#endif
            _ => throw new InvalidOperationException()
        };
    }


    public static string GetCustomDynamicDataDisplayName(MethodInfo methodInfo, object[] data)
        => $"{methodInfo.Name} ({data[0]})";


    public static IEnumerable<object[]> ProviderKinds
        => new[]
        {
            new object[] { ProviderKind.Reflection },
            new object[] { ProviderKind.Generated }
        };

    public static void AssertSpanEqual<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual)
        where T : IEquatable<T>
    {
        if (!expected.SequenceEqual(actual))
        {
            Assert.Fail($"Span not equal. Expected: {{ {string.Join(", ", expected.ToArray())} }}, Actual: {{ {string.Join(", ", actual.ToArray())} }}");
        }
    }

    public static void AssertMemoryEqual<T>(ReadOnlyMemory<T> expected, ReadOnlyMemory<T> actual)
        where T : IEquatable<T>
    {
        AssertSpanEqual(expected.Span, actual.Span);
    }
}
