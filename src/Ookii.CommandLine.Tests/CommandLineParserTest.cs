﻿using Microsoft.VisualStudio.TestPlatform.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Support;
using Ookii.CommandLine.Tests.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
        VerifyArguments(target.Arguments,
        [
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = ["?", "h"] },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        ]);
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
        Assert.AreEqual(19, target.Arguments.Length);
        VerifyArguments(target.Arguments,
        [
            new ExpectedArgument("arg1", typeof(string)) { MemberName = "Arg1", Position = 0, IsRequired = true, Description = "Arg1 description." },
            new ExpectedArgument("other", typeof(int)) { MemberName = "Arg2", Position = 1, DefaultValue = 42, Description = "Arg2 description.", ValueDescription = "Number" },
            new ExpectedArgument("notSwitch", typeof(bool)) { MemberName = "NotSwitch", Position = 2, DefaultValue = false },
            new ExpectedArgument("Arg5", typeof(float)) { Position = 3, Description = "Arg5 description.", DefaultValue = 1.0f },
            new ExpectedArgument("other2", typeof(int)) { MemberName = "Arg4", Position = 4, DefaultValue = 47, Description = "Arg4 description.", ValueDescription = "Number", Aliases = ["HiddenAlias"] },
            new ExpectedArgument("Arg8", typeof(DayOfWeek[]), ArgumentKind.MultiValue) { ElementType = typeof(DayOfWeek), Position = 5, Aliases = ["HiddenAliasOnArgNotIncludedInList"] },
            new ExpectedArgument("Arg6", typeof(string)) { Position = null, IsRequired = true, Description = "Arg6 description.", Aliases = ["Alias1", "Alias2"] },
            new ExpectedArgument("Arg10", typeof(bool[]), ArgumentKind.MultiValue) { ElementType = typeof(bool), Position = null, IsSwitch = true },
            new ExpectedArgument("Arg11", typeof(bool?)) { ElementType = typeof(bool), Position = null, ValueDescription = "Boolean", IsSwitch = true },
            new ExpectedArgument("Arg12", typeof(Collection<int>), ArgumentKind.MultiValue) { ElementType = typeof(int), Position = null, DefaultValue = 42 },
            new ExpectedArgument("Arg13", typeof(Dictionary<string, int>), ArgumentKind.Dictionary) { ElementType = typeof(KeyValuePair<string, int>), ValueDescription = "String=Int32" },
            new ExpectedArgument("Arg14", typeof(IDictionary<string, int>), ArgumentKind.Dictionary) { ElementType = typeof(KeyValuePair<string, int>), ValueDescription = "String=Int32" },
            new ExpectedArgument("Arg15", typeof(KeyValuePair<string, int>)) { ValueDescription = "KeyValuePair<String, Int32>" },
            new ExpectedArgument("Arg3", typeof(string)) { Position = null },
            new ExpectedArgument("Arg7", typeof(bool)) { Position = null, IsSwitch = true, Aliases = ["Alias3"] },
            new ExpectedArgument("Arg9", typeof(int?)) { ElementType = typeof(int), Position = null, ValueDescription = "Int32" },
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = ["?", "h"] },
            new ExpectedArgument("NotSwitch2", typeof(NonSwitchBoolean)) { ValueDescription = "Boolean", Description = "NotSwitch2 description." },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        ]);
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
        var options = new ParseOptions()
        {
            AutoPrefixAliases = false
        };

        var target = CreateParser<TestArguments>(kind, options);
        // Only required arguments
        TestParse(target, "val1 2 -arg6 val6", "val1", 2, arg6: "val6");
        // Make sure negative numbers are accepted, and not considered an argument name.
        TestParse(target, "val1 -2 -arg6 val6", "val1", -2, arg6: "val6");
        // All positional arguments except array
        TestParse(target, "val1 2 true 5.5 4 -arg6 arg6", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6");
        // All positional arguments including array
        TestParse(target, "val1 2 true 5.5 4 -arg6 arg6 Monday Tuesday", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6", arg8: [DayOfWeek.Monday, DayOfWeek.Tuesday]);
        // All positional arguments including array, which is specified by name first and then by position
        TestParse(target, "val1 2 true 5.5 4 -arg6 arg6 -arg8 Monday Tuesday", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6", arg8: [DayOfWeek.Monday, DayOfWeek.Tuesday]);
        // Some positional arguments using names, in order
        TestParse(target, "-arg1 val1 2 true -arg5 5.5 4 -arg6 arg6", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6");
        // Some position arguments using names, out of order (also uses : and - for one of them to mix things up)
        TestParse(target, "-other 2 val1 -arg5:5.5 true 4 -arg6 arg6", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6");
        // All arguments
        TestParse(target, "val1 2 true -arg3 val3 -other2:4 5.5 -arg6 val6 -arg7 -arg8 Monday -arg8 Tuesday -arg9 9 -arg10 -arg10 -arg10:false -arg11:false -arg12 12 -arg12 13 -arg13 foo=13 -arg13 bar=14 -arg14 hello=1 -arg14 bye=2 -arg15 something=5", "val1", 2, true, "val3", 4, 5.5f, "val6", true, [DayOfWeek.Monday, DayOfWeek.Tuesday], 9, [true, true, false], false, [12, 13], new Dictionary<string, int>() { { "foo", 13 }, { "bar", 14 } }, new Dictionary<string, int>() { { "hello", 1 }, { "bye", 2 } }, new KeyValuePair<string, int>("something", 5));
        // Using aliases
        TestParse(target, "val1 2 -alias1 valalias6 -alias3", "val1", 2, arg6: "valalias6", arg7: true);
        // Long prefix cannot be used
        CheckThrows(target, ["val1", "2", "--arg6", "val6"], CommandLineArgumentErrorCategory.UnknownArgument, "-arg6", remainingArgumentCount: 2);
        // Short name cannot be used
        CheckThrows(target, ["val1", "2", "-arg6", "val6", "-a:5.5"], CommandLineArgumentErrorCategory.UnknownArgument, "a", remainingArgumentCount: 1);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestEmptyArguments(ProviderKind kind)
    {
        var target = CreateParser<EmptyArguments>(kind);
        // This test was added because version 2.0 threw an IndexOutOfRangeException when you tried to specify a positional argument when there were no positional arguments defined.
        CheckThrows(target, ["Foo", "Bar"], CommandLineArgumentErrorCategory.TooManyArguments, remainingArgumentCount: 2);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestTooManyArguments(ProviderKind kind)
    {
        var target = CreateParser<ThrowingArguments>(kind);

        // Only accepts one positional argument.
        CheckThrows(target, ["Foo", "Bar"], CommandLineArgumentErrorCategory.TooManyArguments, remainingArgumentCount: 1);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestPropertySetterThrows(ProviderKind kind)
    {
        var target = CreateParser<ThrowingArguments>(kind);

        // No remaining arguments; exception happens after parsing finishes.
        CheckThrows(target,
            ["-ThrowingArgument", "-5"],
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

        var args = target.Parse(["-DuplicateKeys", "Foo=1", "-DuplicateKeys", "Bar=2", "-DuplicateKeys", "Foo=3"]);
        Assert.IsNotNull(args);
        Assert.AreEqual(2, args.DuplicateKeys.Count);
        Assert.AreEqual(3, args.DuplicateKeys["Foo"]);
        Assert.AreEqual(2, args.DuplicateKeys["Bar"]);

        CheckThrows(target,
            ["-NoDuplicateKeys", "Foo=1", "-NoDuplicateKeys", "Bar=2", "-NoDuplicateKeys", "Foo=3"],
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

        var args = target.Parse(["-NoSeparator", "Value1,Value2", "-NoSeparator", "Value3", "-Separator", "Value1,Value2", "-Separator", "Value3"]);
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
        var args = CheckSuccess(target, ["-Argument1:test", "-Argument2:foo:bar"]);
        Assert.IsNotNull(args);
        Assert.AreEqual("test", args.Argument1);
        Assert.AreEqual("foo:bar", args.Argument2);
        args = CheckSuccess(target, ["-Argument1=test", "-Argument2=foo:bar"]);
        Assert.AreEqual("test", args.Argument1);
        Assert.AreEqual("foo:bar", args.Argument2);
        args = CheckSuccess(target, ["-Argument2:foo=bar"]);
        Assert.AreEqual("foo=bar", args.Argument2);

        CheckThrows(target,
            ["-Argument1>test"],
            CommandLineArgumentErrorCategory.UnknownArgument,
            "Argument1>test",
            remainingArgumentCount: 1);

        var options = new ParseOptions()
        {
            NameValueSeparators = new[] { '>' },
        };

        target = CreateParser<SimpleArguments>(kind, options);
        args = target.Parse(["-Argument1>test", "-Argument2>foo>bar"]);
        Assert.IsNotNull(args);
        Assert.AreEqual("test", args.Argument1);
        Assert.AreEqual("foo>bar", args.Argument2);
        CheckThrows(target,
            ["-Argument1:test"],
            CommandLineArgumentErrorCategory.UnknownArgument,
            "Argument1:test",
            remainingArgumentCount: 1);

        CheckThrows(target,
            ["-Argument1=test"],
            CommandLineArgumentErrorCategory.UnknownArgument,
            "Argument1=test",
            remainingArgumentCount: 1);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void ParseTestKeyValueSeparator(ProviderKind kind)
    {
        var target = CreateParser<KeyValueSeparatorArguments>(kind);
        Assert.AreEqual("=", target.GetArgument("DefaultSeparator")!.DictionaryInfo!.KeyValueSeparator);
        Assert.AreEqual("String=Int32", target.GetArgument("DefaultSeparator")!.ValueDescription);
        Assert.AreEqual("<=>", target.GetArgument("CustomSeparator")!.DictionaryInfo!.KeyValueSeparator);
        Assert.AreEqual("String<=>String", target.GetArgument("CustomSeparator")!.ValueDescription);

        var result = CheckSuccess(target, ["-CustomSeparator", "foo<=>bar", "-CustomSeparator", "baz<=>contains<=>separator", "-CustomSeparator", "hello<=>"]);
        Assert.IsNotNull(result);
        CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("foo", "bar"), KeyValuePair.Create("baz", "contains<=>separator"), KeyValuePair.Create("hello", "") }, result.CustomSeparator);
        CheckThrows(target,
            ["-CustomSeparator", "foo=bar"],
            CommandLineArgumentErrorCategory.ArgumentValueConversion,
            "CustomSeparator",
            typeof(FormatException),
            remainingArgumentCount: 2);

        // Inner exception is FormatException because what throws here is trying to convert
        // ">bar" to int.
        CheckThrows(target,
            ["-DefaultSeparator", "foo<=>bar"],
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
            ExecutableName = ExecutableName
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
            ExecutableName = ExecutableName
        };

        string actual = target.GetUsage(options);
        Assert.AreEqual(_expectedLongShortUsage, actual);

        options.UseShortNamesForSyntax = true;
        actual = target.GetUsage(options);
        Assert.AreEqual(_expectedLongShortUsageShortNameSyntax, actual);

        options = new UsageWriter()
        {
            ExecutableName = ExecutableName,
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
            ExecutableName = ExecutableName,
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
        var writer = new UsageWriter(useColor: TriState.True)
        {
            ExecutableName = ExecutableName,
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
            ExecutableName = ExecutableName,
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
                ExecutableName = ExecutableName,
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
                ExecutableName = ExecutableName,
                ArgumentDescriptionIndent = 4,
            }
        };
        var target = CreateParser<TestArguments>(kind, options);
        string actual = target.GetUsage(options.UsageWriter);
        Assert.AreEqual(_expectedCustomIndentUsage, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageIndentAfterBlankLine(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            UsageWriter = new UsageWriter()
            {
                ExecutableName = ExecutableName,
            }
        };

        var target = CreateParser<EmptyLineDescriptionArguments>(kind, options);
        string actual = target.GetUsage();
        Assert.AreEqual(_expectedEmptyLineDefaultUsage, actual);

        options.UsageWriter.IndentAfterEmptyLine = true;
        actual = target.GetUsage();
        Assert.AreEqual(_expectedEmptyLineIndentAfterBlankLineUsage, actual);

        // Test again with a max length to make sure indents are properly reset where expected.
        using var writer = LineWrappingTextWriter.ForStringWriter(80);
        var usageWriter = new UsageWriter(writer)
        {
            ExecutableName = ExecutableName,
        };

        target.WriteUsage(usageWriter);
        Assert.AreEqual(_expectedEmptyLineDefaultUsage, writer.ToString());

        ((StringWriter)writer.BaseWriter).GetStringBuilder().Clear();
        writer.ResetIndent();
        usageWriter.IndentAfterEmptyLine = true;
        target.WriteUsage(usageWriter);
        Assert.AreEqual(_expectedEmptyLineIndentAfterBlankLineUsage, writer.ToString());
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageDefaultValueFormat(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            UsageWriter = new UsageWriter()
            {
                ExecutableName = ExecutableName,
            }
        };

        var parser = CreateParser<DefaultValueFormatArguments>(kind, options);
        string actual = parser.GetUsage();
        Assert.AreEqual(_expectedDefaultValueFormatUsage, actual);

        // Stream culture should be ignored for the default value in favor of the parser culture.
        options.Culture = CultureInfo.GetCultureInfo("nl-NL");
        actual = parser.GetUsage();
        Assert.AreEqual(_expectedDefaultValueFormatCultureUsage, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageFooter(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            UsageWriter = new CustomUsageWriter()
            {
                ExecutableName = ExecutableName
            },
        };

        var target = CreateParser<TestArguments>(kind, options);
        string actual = target.GetUsage();
        Assert.AreEqual(_expectedFooterUsage, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageCategories(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            UsageWriter = new UsageWriter(useColor: TriState.True)
            {
                ExecutableName = ExecutableName
            },
        };

        var target = CreateParser<CategoryArguments>(kind, options);
        string actual = target.GetUsage();
        Assert.AreEqual(_expectedCategoryUsage, actual);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestWriteUsageDefaultCategory(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            UsageWriter = new UsageWriter()
            {
                ExecutableName = ExecutableName
            },
        };

        var target = CreateParser<DefaultCategoryArguments>(kind, options);
        string actual = target.GetUsage();
        Assert.AreEqual(_expectedDefaultCategoryUsage, actual);
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
            ArgumentNamePrefixes = ["/", "-"],
            Error = error,
            ShowUsageOnError = UsageHelpRequest.Full,
            UsageWriter = new UsageWriter(lineWriter)
            {
                ExecutableName = ExecutableName,
            }
        };

        var result = StaticParse<TestArguments>(kind, ["foo", "-Arg6", "bar"], options);
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
        result = StaticParse<TestArguments>(kind, ["-Help"], options);
        Assert.IsNull(result);
        Assert.AreEqual(0, error.ToString().Length);
        Assert.AreEqual(_expectedDefaultUsage, output.ToString());

        // With full help requested, no special handling of ambiguous usage prefixes.
        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        result = StaticParse<TestArguments>(kind, ["-a"], options);
        Assert.IsNull(result);
        Assert.IsTrue(error.ToString().Length > 0);
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
        result = StaticParse<TestArguments>(kind, ["-Help"], options);
        Assert.IsNull(result);
        Assert.AreEqual(0, error.ToString().Length);
        Assert.AreEqual(_expectedDefaultUsage, output.ToString());
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestStaticParseAutoPrefixUsage(ProviderKind kind)
    {
        using var output = new StringWriter();
        using var lineWriter = new LineWrappingTextWriter(output, 0);
        using var error = new StringWriter();
        var options = new ParseOptions()
        {
            Error = error,
            UsageWriter = new UsageWriter(lineWriter, TriState.True)
            {
                ExecutableName = ExecutableName,
            }
        };

        var expectedError = "The provided argument name 'p' is an ambiguous prefix alias.\n\n".ReplaceLineEndings();
        Assert.IsNull(StaticParse<AutoPrefixAliasesArguments>(kind, ["-p"], options));
        Assert.AreEqual(expectedError, error.ToString());
        Assert.AreEqual(_expectedAutoPrefixUsage, output.ToString());

        options.IsPosix = true;
        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        Assert.IsNull(StaticParse<AutoPrefixAliasesArguments>(kind, ["--p"], options));
        Assert.AreEqual(expectedError, error.ToString());
        // The alias doesn't match this time because it's not transformed and case sensitive.
        Assert.AreEqual(_expectedAutoPrefixUsageLongShort, output.ToString());
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestCancelParsing(ProviderKind kind)
    {
        var parser = CreateParser<CancelArguments>(kind);

        // Don't cancel if -DoesCancel not specified.
        var result = parser.Parse(["-Argument1", "foo", "-DoesNotCancel", "-Argument2", "bar"]);
        Assert.IsNotNull(result);
        Assert.IsFalse(parser.ParseResult.HelpRequested);
        Assert.IsTrue(result.DoesNotCancel);
        Assert.IsFalse(result.DoesCancel);
        Assert.AreEqual("foo", result.Argument1);
        Assert.AreEqual("bar", result.Argument2);
        Assert.AreEqual(ParseStatus.Success, parser.ParseResult.Status);
        Assert.IsNull(parser.ParseResult.ArgumentName);
        Assert.AreEqual(0, parser.ParseResult.RemainingArguments.Length);

        // Cancel if -DoesCancel specified.
        result = parser.Parse(["-Argument1", "foo", "-DoesCancel", "-Argument2", "bar"]);
        Assert.IsNull(result);
        Assert.IsTrue(parser.ParseResult.HelpRequested);
        Assert.AreEqual(ParseStatus.Canceled, parser.ParseResult.Status);
        Assert.IsNull(parser.ParseResult.LastException);
        AssertSpanEqual(["-Argument2", "bar"], parser.ParseResult.RemainingArguments.Span);
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
        result = parser.Parse(["-Argument1", "foo", "-DoesNotCancel", "-Argument2", "bar"]);
        Assert.IsNull(result);
        Assert.AreEqual(ParseStatus.Canceled, parser.ParseResult.Status);
        Assert.IsNull(parser.ParseResult.LastException);
        Assert.AreEqual("DoesNotCancel", parser.ParseResult.ArgumentName);
        AssertSpanEqual(new[] { "-Argument2", "bar" }.AsSpan(), parser.ParseResult.RemainingArguments.Span);
        Assert.IsFalse(parser.ParseResult.HelpRequested);
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
                Assert.AreEqual(CancelMode.AbortWithHelp, e.CancelParsing);
                e.CancelParsing = CancelMode.None;
            }
        }

        parser.ArgumentParsed += handler2;
        result = parser.Parse(["-Argument1", "foo", "-DoesCancel", "-Argument2", "bar"]);
        Assert.AreEqual(ParseStatus.Success, parser.ParseResult.Status);
        Assert.IsNull(parser.ParseResult.ArgumentName);
        Assert.AreEqual(0, parser.ParseResult.RemainingArguments.Length);
        Assert.IsNotNull(result);
        Assert.IsFalse(parser.ParseResult.HelpRequested);
        Assert.IsFalse(result.DoesNotCancel);
        Assert.IsTrue(result.DoesCancel);
        Assert.AreEqual("foo", result.Argument1);
        Assert.AreEqual("bar", result.Argument2);

        // Automatic help argument should cancel.
        result = parser.Parse(["-Help"]);
        Assert.AreEqual(ParseStatus.Canceled, parser.ParseResult.Status);
        Assert.IsNull(parser.ParseResult.LastException);
        Assert.AreEqual("Help", parser.ParseResult.ArgumentName);
        Assert.AreEqual(0, parser.ParseResult.RemainingArguments.Length);
        Assert.IsNull(result);
        Assert.IsTrue(parser.ParseResult.HelpRequested);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestCancelParsingSuccess(ProviderKind kind)
    {
        var parser = CreateParser<CancelArguments>(kind);
        var result = parser.Parse(["-Argument1", "foo", "-DoesCancelWithSuccess", "-Argument2", "bar"]);
        Assert.AreEqual(ParseStatus.Success, parser.ParseResult.Status);
        Assert.AreEqual("DoesCancelWithSuccess", parser.ParseResult.ArgumentName);
        AssertSpanEqual(new[] { "-Argument2", "bar" }.AsSpan(), parser.ParseResult.RemainingArguments.Span);
        Assert.IsNotNull(result);
        Assert.IsFalse(parser.ParseResult.HelpRequested);
        Assert.IsFalse(result.DoesNotCancel);
        Assert.IsFalse(result.DoesCancel);
        Assert.IsTrue(result.DoesCancelWithSuccess);
        Assert.AreEqual("foo", result.Argument1);
        Assert.IsNull(result.Argument2);

        // No remaining arguments.
        result = parser.Parse(["-Argument1", "foo", "-DoesCancelWithSuccess"]);
        Assert.AreEqual(ParseStatus.Success, parser.ParseResult.Status);
        Assert.AreEqual("DoesCancelWithSuccess", parser.ParseResult.ArgumentName);
        Assert.AreEqual(0, parser.ParseResult.RemainingArguments.Length);
        Assert.IsNotNull(result);
        Assert.IsFalse(parser.ParseResult.HelpRequested);
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
        var result = StaticParse<CultureArguments>(kind, ["-Argument", "5.5"]);
        Assert.IsNotNull(result);
        Assert.AreEqual(5.5, result.Argument);
        result = StaticParse<CultureArguments>(kind, ["-Argument", "5,5"]);
        Assert.IsNotNull(result);
        // , was interpreted as a thousands separator.
        Assert.AreEqual(55, result.Argument);

        var options = new ParseOptions { Culture = new CultureInfo("nl-NL") };
        result = StaticParse<CultureArguments>(kind, ["-Argument", "5,5"], options);
        Assert.IsNotNull(result);
        Assert.AreEqual(5.5, result.Argument);
        result = StaticParse<CultureArguments>(kind, ["-Argument", "5,5"]);
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

        var result = CheckSuccess(parser, ["-f", "5", "--bar", "6", "-a", "7", "--arg1", "8", "-s"]);
        Assert.AreEqual(5, result.Foo);
        Assert.AreEqual(6, result.Bar);
        Assert.AreEqual(7, result.Arg2);
        Assert.AreEqual(8, result.Arg1);
        Assert.IsTrue(result.Switch1);
        Assert.IsFalse(LongShortArguments.Switch2Value);
        Assert.IsFalse(result.Switch3);

        // Combine switches.
        result = CheckSuccess(parser, ["-su"]);
        Assert.IsTrue(result.Switch1);
        Assert.IsFalse(LongShortArguments.Switch2Value);
        Assert.IsTrue(result.Switch3);

        // Use a short alias.
        result = CheckSuccess(parser, ["-b", "5"]);
        Assert.AreEqual(5, result.Arg2);

        // Combining non-switches is an error.
        CheckThrows(parser, ["-sf"], CommandLineArgumentErrorCategory.CombinedShortNameNonSwitch, "sf", remainingArgumentCount: 1);

        // Can't use long argument prefix with short names.
        CheckThrows(parser, ["--s"], CommandLineArgumentErrorCategory.AmbiguousPrefixAlias, "s", remainingArgumentCount: 1, possibleMatches: ["Switch1", "Switch2"]);

        // And vice versa.
        CheckThrows(parser, ["-Switch1"], CommandLineArgumentErrorCategory.UnknownArgument, "w", remainingArgumentCount: 1);

        // Short alias is ignored on an argument without a short name.
        CheckThrows(parser, ["-c"], CommandLineArgumentErrorCategory.UnknownArgument, "c", remainingArgumentCount: 1);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestMethodArguments(ProviderKind kind)
    {
        var parser = CreateParser<MethodArguments>(kind);

        Assert.AreEqual(ArgumentKind.Method, parser.GetArgument("CancelWithHelp")!.Kind);
        Assert.IsNull(parser.GetArgument("NotAnArgument"));
        Assert.IsNull(parser.GetArgument("NotStatic"));
        Assert.IsNull(parser.GetArgument("NotPublic"));

        CheckCanceled(parser, ["-CancelWithHelp"], "CancelWithHelp", true, 0);
        Assert.AreEqual(nameof(MethodArguments.CancelWithHelp), MethodArguments.CalledMethodName);

        CheckSuccess(parser, ["-CancelWithValue", "1"]);
        Assert.AreEqual(nameof(MethodArguments.CancelWithValue), MethodArguments.CalledMethodName);
        Assert.AreEqual(1, MethodArguments.Value);

        CheckCanceled(parser, ["-CancelWithValue", "-1"], "CancelWithValue", false);
        Assert.AreEqual(nameof(MethodArguments.CancelWithValue), MethodArguments.CalledMethodName);
        Assert.AreEqual(-1, MethodArguments.Value);

        CheckSuccess(parser, ["-CancelWithValueAndHelp", "1"]);
        Assert.AreEqual(nameof(MethodArguments.CancelWithValueAndHelp), MethodArguments.CalledMethodName);
        Assert.AreEqual(1, MethodArguments.Value);

        CheckCanceled(parser, ["-CancelWithValueAndHelp", "-1", "bar"], "CancelWithValueAndHelp", true, 1);
        Assert.AreEqual(nameof(MethodArguments.CancelWithValueAndHelp), MethodArguments.CalledMethodName);
        Assert.AreEqual(-1, MethodArguments.Value);

        CheckSuccess(parser, ["-NoReturn"]);
        Assert.AreEqual(nameof(MethodArguments.NoReturn), MethodArguments.CalledMethodName);

        CheckSuccess(parser, ["42"]);
        Assert.AreEqual(nameof(MethodArguments.Positional), MethodArguments.CalledMethodName);
        Assert.AreEqual(42, MethodArguments.Value);

        CheckCanceled(parser, ["-CancelModeAbort", "Foo"], "CancelModeAbort", false, 1);
        Assert.AreEqual(nameof(MethodArguments.CancelModeAbort), MethodArguments.CalledMethodName);

        CheckSuccess(parser, ["-CancelModeSuccess", "Foo"], "CancelModeSuccess", 1);
        Assert.AreEqual(nameof(MethodArguments.CancelModeSuccess), MethodArguments.CalledMethodName);

        CheckSuccess(parser, ["-CancelModeNone"]);
        Assert.AreEqual(nameof(MethodArguments.CancelModeNone), MethodArguments.CalledMethodName);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestPrefixTermination(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            PrefixTermination = PrefixTerminationMode.PositionalOnly
        };

        var parser = CreateParser<PrefixTerminationArguments>(kind, options);
        Assert.AreEqual("--", parser.LongArgumentNamePrefix);
        Assert.AreEqual(ParsingMode.Default, parser.Mode);
        var result = CheckSuccess(parser, ["Foo", "--", "-Arg4", "Bar"]);
        Assert.AreEqual("Foo", result.Arg1);
        Assert.AreEqual("-Arg4", result.Arg2);
        Assert.AreEqual("Bar", result.Arg3);
        Assert.IsNull(result.Arg4);
        options.PrefixTermination = PrefixTerminationMode.CancelWithSuccess;
        result = CheckSuccess(parser, ["Foo", "--", "-Arg4", "Bar"], "--", 2);
        Assert.AreEqual("Foo", result.Arg1);
        Assert.IsNull(result.Arg2);
        Assert.IsNull(result.Arg3);
        Assert.IsNull(result.Arg4);
        options.PrefixTermination = PrefixTerminationMode.CancelWithSuccess;
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestPrefixTerminationLongShort(ProviderKind kind)
    {
        var options = new ParseOptions()
        {
            IsPosix = true,
            PrefixTermination = PrefixTerminationMode.PositionalOnly
        };

        var parser = CreateParser<PrefixTerminationArguments>(kind, options);
        Assert.AreEqual("--", parser.LongArgumentNamePrefix);
        Assert.AreEqual(ParsingMode.LongShort, parser.Mode);
        var result = CheckSuccess(parser, ["--arg4", "Foo", "--", "--arg1", "Bar"]);
        Assert.AreEqual("Foo", result.Arg4);
        Assert.AreEqual("--arg1", result.Arg1);
        Assert.AreEqual("Bar", result.Arg2);
        Assert.IsNull(result.Arg3);
        options.PrefixTermination = PrefixTerminationMode.CancelWithSuccess;
        result = CheckSuccess(parser, ["Foo", "--", "--arg4", "Bar"], "--", 2);
        Assert.AreEqual("Foo", result.Arg1);
        Assert.IsNull(result.Arg2);
        Assert.IsNull(result.Arg3);
        Assert.IsNull(result.Arg4);
        options.PrefixTermination = PrefixTerminationMode.CancelWithSuccess;
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
            ExecutableName = ExecutableName,
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
        VerifyArguments(parser.Arguments,
        [
            new ExpectedArgument("TestArg", typeof(string)) { MemberName = "testArg", Position = 0, IsRequired = true },
            new ExpectedArgument("ExplicitName", typeof(int)) { MemberName = "Explicit" },
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = ["?", "h"] },
            new ExpectedArgument("TestArg2", typeof(int)) { MemberName = "TestArg2" },
            new ExpectedArgument("TestArg3", typeof(int)) { MemberName = "__test__arg3__" },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        ]);
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
        VerifyArguments(parser.Arguments,
        [
            new ExpectedArgument("testArg", typeof(string)) { MemberName = "testArg", Position = 0, IsRequired = true },
            new ExpectedArgument("ExplicitName", typeof(int)) { MemberName = "Explicit" },
            new ExpectedArgument("help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = ["?", "h"] },
            new ExpectedArgument("testArg2", typeof(int)) { MemberName = "TestArg2" },
            new ExpectedArgument("testArg3", typeof(int)) { MemberName = "__test__arg3__" },
            new ExpectedArgument("version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        ]);
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
        VerifyArguments(parser.Arguments,
        [
            new ExpectedArgument("test_arg", typeof(string)) { MemberName = "testArg", Position = 0, IsRequired = true },
            new ExpectedArgument("ExplicitName", typeof(int)) { MemberName = "Explicit" },
            new ExpectedArgument("help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = ["?", "h"] },
            new ExpectedArgument("test_arg2", typeof(int)) { MemberName = "TestArg2" },
            new ExpectedArgument("test_arg3", typeof(int)) { MemberName = "__test__arg3__" },
            new ExpectedArgument("version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        ]);
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
        VerifyArguments(parser.Arguments,
        [
            new ExpectedArgument("test-arg", typeof(string)) { MemberName = "testArg", Position = 0, IsRequired = true },
            new ExpectedArgument("ExplicitName", typeof(int)) { MemberName = "Explicit" },
            new ExpectedArgument("help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = ["?", "h"] },
            new ExpectedArgument("test-arg2", typeof(int)) { MemberName = "TestArg2" },
            new ExpectedArgument("test-arg3", typeof(int)) { MemberName = "__test__arg3__" },
            new ExpectedArgument("version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        ]);
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
        VerifyArguments(parser.Arguments,
        [
            new ExpectedArgument("Arg1", typeof(FileInfo)) { ValueDescription = "file-info" },
            new ExpectedArgument("Arg2", typeof(int)) { ValueDescription = "int32" },
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { ValueDescription = "boolean", MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = ["?", "h"] },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { ValueDescription = "boolean", MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        ]);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestValidation(ProviderKind kind)
    {
        // Reset for multiple runs.
        ValidationArguments.Arg3Value = 0;
        var parser = CreateParser<ValidationArguments>(kind);

        // Range validator on property
        CheckThrows(parser, ["-Arg1", "0"], CommandLineArgumentErrorCategory.ValidationFailed, "Arg1", remainingArgumentCount: 2);
        var result = CheckSuccess(parser, ["-Arg1", "1"]);
        Assert.AreEqual(1, result.Arg1);
        result = CheckSuccess(parser, ["-Arg1", "5"]);
        Assert.AreEqual(5, result.Arg1);
        CheckThrows(parser, ["-Arg1", "6"], CommandLineArgumentErrorCategory.ValidationFailed, "Arg1", remainingArgumentCount: 2);

        // Not null or empty on ctor parameter
        CheckThrows(parser, [""], CommandLineArgumentErrorCategory.ValidationFailed, "arg2", remainingArgumentCount: 1);
        result = CheckSuccess(parser, [" "]);
        Assert.AreEqual(" ", result.Arg2);

        // Multiple validators on method
        CheckThrows(parser, ["-Arg3", "1238"], CommandLineArgumentErrorCategory.ValidationFailed, "Arg3", remainingArgumentCount: 2);
        Assert.AreEqual(0, ValidationArguments.Arg3Value);
        CheckThrows(parser, ["-Arg3", "123"], CommandLineArgumentErrorCategory.ValidationFailed, "Arg3", remainingArgumentCount: 2);
        Assert.AreEqual(0, ValidationArguments.Arg3Value);
        CheckThrows(parser, ["-Arg3", "7001"], CommandLineArgumentErrorCategory.ValidationFailed, "Arg3", remainingArgumentCount: 2);
        // Range validation is done after setting the value, so this was set!
        Assert.AreEqual(7001, ValidationArguments.Arg3Value);
        CheckSuccess(parser, ["-Arg3", "1023"]);
        Assert.AreEqual(1023, ValidationArguments.Arg3Value);

        // Validator on multi-value argument
        CheckThrows(parser, ["-Arg4", "foo;bar;bazz"], CommandLineArgumentErrorCategory.ValidationFailed, "Arg4", remainingArgumentCount: 2);
        CheckThrows(parser, ["-Arg4", "foo", "-Arg4", "bar", "-Arg4", "bazz"], CommandLineArgumentErrorCategory.ValidationFailed, "Arg4", remainingArgumentCount: 2);
        result = CheckSuccess(parser, ["-Arg4", "foo;bar"]);
        CollectionAssert.AreEqual(new[] { "foo", "bar" }, result.Arg4);
        result = CheckSuccess(parser, ["-Arg4", "foo", "-Arg4", "bar"]);
        CollectionAssert.AreEqual(new[] { "foo", "bar" }, result.Arg4);

        // Count validator
        // No remaining arguments because validation happens after parsing.
        CheckThrows(parser, ["-Arg4", "foo"], CommandLineArgumentErrorCategory.ValidationFailed, "Arg4");
        CheckThrows(parser, ["-Arg4", "foo;bar;baz;ban;bap"], CommandLineArgumentErrorCategory.ValidationFailed, "Arg4");
        result = CheckSuccess(parser, ["-Arg4", "foo;bar;baz;ban"]);
        CollectionAssert.AreEqual(new[] { "foo", "bar", "baz", "ban" }, result.Arg4);

        // Enum validator
        CheckThrows(parser, ["-Day", "foo"], CommandLineArgumentErrorCategory.ArgumentValueConversion, "Day", typeof(ArgumentException), remainingArgumentCount: 2);
        CheckThrows(parser, ["-Day", "9"], CommandLineArgumentErrorCategory.ValidationFailed, "Day", remainingArgumentCount: 2);
        CheckThrows(parser, ["-Day", ""], CommandLineArgumentErrorCategory.ArgumentValueConversion, "Day", typeof(ArgumentException), remainingArgumentCount: 2);
        result = CheckSuccess(parser, ["-Day3", "1"]);
        Assert.AreEqual(DayOfWeek.Monday, result.Day3);
        CheckThrows(parser, ["-Day2", "foo"], CommandLineArgumentErrorCategory.ArgumentValueConversion, "Day2", typeof(ArgumentException), remainingArgumentCount: 2);
        result = CheckSuccess(parser, ["-Day3", "9"]); // This one allows it.
        Assert.AreEqual((DayOfWeek)9, result.Day3);
        CheckThrows(parser, ["-Day2", "1"], CommandLineArgumentErrorCategory.ValidationFailed, "Day2", null, remainingArgumentCount: 2);
        result = CheckSuccess(parser, ["-Day2", ""]);
        Assert.IsNull(result.Day2);

        // Case sensitive enums
        CheckSuccess(parser, ["-Day", "tuesday"]);
        CheckSuccess(parser, ["-Day2", "Tuesday"]);
        CheckThrows(parser, ["-Day2", "tuesday"], CommandLineArgumentErrorCategory.ArgumentValueConversion, "Day2", typeof(ArgumentException), remainingArgumentCount: 2);

        // Disallow commas.
        result = CheckSuccess(parser, ["-Day2", "Monday,Tuesday"]);
        Assert.AreEqual(DayOfWeek.Wednesday, result.Day2);
        CheckThrows(parser, ["-Day", "Monday,Tuesday"], CommandLineArgumentErrorCategory.ValidationFailed, "Day", remainingArgumentCount: 2);

        // Disallow numbers.
        CheckThrows(parser, ["-Day2", "5"], CommandLineArgumentErrorCategory.ValidationFailed, "Day2", remainingArgumentCount: 2);
        CheckThrows(parser, ["-Day2", "Tuesday,5"], CommandLineArgumentErrorCategory.ValidationFailed, "Day2", remainingArgumentCount: 2);

        // Allow commas because of flags attribute.
        result = CheckSuccess(parser, ["-Modifiers", "Control,Alt"]);
        Assert.AreEqual(ConsoleModifiers.Control | ConsoleModifiers.Alt, result.Modifiers);

        // Numbers still not allowed despite no attribute
        CheckThrows(parser, ["-Modifiers", "0"], CommandLineArgumentErrorCategory.ValidationFailed, "Modifiers", remainingArgumentCount: 2);

        // NotNull validator with Nullable<T>.
        CheckThrows(parser, ["-NotNull", ""], CommandLineArgumentErrorCategory.ValidationFailed, "NotNull", remainingArgumentCount: 2);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestRequires(ProviderKind kind)
    {
        var parser = CreateParser<DependencyArguments>(kind);

        // None of these have remaining arguments because validation happens after parsing.
        var result = CheckSuccess(parser, ["-Address", "127.0.0.1"]);
        Assert.AreEqual(IPAddress.Loopback, result.Address);
        CheckThrows(parser, ["-Port", "9000"], CommandLineArgumentErrorCategory.DependencyFailed, "Port");
        result = CheckSuccess(parser, ["-Address", "127.0.0.1", "-Port", "9000"]);
        Assert.AreEqual(IPAddress.Loopback, result.Address);
        Assert.AreEqual(9000, result.Port);
        CheckThrows(parser, ["-Protocol", "1"], CommandLineArgumentErrorCategory.DependencyFailed, "Protocol");
        CheckThrows(parser, ["-Address", "127.0.0.1", "-Protocol", "1"], CommandLineArgumentErrorCategory.DependencyFailed, "Protocol");
        CheckThrows(parser, ["-Throughput", "10", "-Protocol", "1"], CommandLineArgumentErrorCategory.DependencyFailed, "Protocol");
        result = CheckSuccess(parser, ["-Protocol", "1", "-Address", "127.0.0.1", "-Throughput", "10"]);
        Assert.AreEqual(IPAddress.Loopback, result.Address);
        Assert.AreEqual(10, result.Throughput);
        Assert.AreEqual(1, result.Protocol);

        CheckThrows(parser, ["-Value", "foo", "bar"], CommandLineArgumentErrorCategory.DependencyFailed, "Value");
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestProhibits(ProviderKind kind)
    {
        var parser = CreateParser<DependencyArguments>(kind);

        var result = CheckSuccess(parser, ["-Path", "test"]);
        Assert.AreEqual("test", result.Path.Name);
        // No remaining arguments because validation happens after parsing.
        CheckThrows(parser, ["-Path", "test", "-Address", "127.0.0.1"], CommandLineArgumentErrorCategory.DependencyFailed, "Path");
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
            ExecutableName = ExecutableName,
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
        Assert.IsTrue(parser.GetArgument("Multi")!.MultiValueInfo!.AllowWhiteSpaceSeparator);
        Assert.IsFalse(parser.GetArgument("MultiSwitch")!.MultiValueInfo!.AllowWhiteSpaceSeparator);
        Assert.IsNull(parser.GetArgument("Other")!.MultiValueInfo);

        var result = CheckSuccess(parser, ["1", "-Multi", "2", "3", "4", "-Other", "5", "6"]);
        Assert.AreEqual(result.Arg1, 1);
        Assert.AreEqual(result.Arg2, 6);
        Assert.AreEqual(result.Other, 5);
        CollectionAssert.AreEqual(new[] { 2, 3, 4 }, result.Multi);

        result = CheckSuccess(parser, ["-Multi", "1", "-Multi", "2"]);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result.Multi);

        CheckThrows(parser, ["1", "-Multi", "-Other", "5", "6"], CommandLineArgumentErrorCategory.MissingNamedArgumentValue, "Multi", remainingArgumentCount: 4);
        CheckThrows(parser, ["-MultiSwitch", "true", "false"], CommandLineArgumentErrorCategory.ArgumentValueConversion, "Arg1", typeof(FormatException), remainingArgumentCount: 2);
        parser.Options.AllowWhiteSpaceValueSeparator = false;
        CheckThrows(parser, ["1", "-Multi:2", "2", "3", "4", "-Other", "5", "6"], CommandLineArgumentErrorCategory.TooManyArguments, remainingArgumentCount: 5);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestInjection(ProviderKind kind)
    {
        var parser = CreateParser<InjectionArguments>(kind);
        var result = CheckSuccess(parser, ["-Arg", "1"]);
        Assert.AreSame(parser, result.Parser);
        Assert.AreEqual(1, result.Arg);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestDuplicateArguments(ProviderKind kind)
    {
        var parser = CreateParser<SimpleArguments>(kind);
        CheckThrows(parser, ["-Argument1", "foo", "-Argument1", "bar"], CommandLineArgumentErrorCategory.DuplicateArgument, "Argument1", remainingArgumentCount: 2);
        parser.Options.DuplicateArguments = ErrorMode.Allow;
        var result = CheckSuccess(parser, ["-Argument1", "foo", "-Argument1", "bar"]);
        Assert.AreEqual("bar", result.Argument1);

        bool handlerCalled = false;
        bool keepOldValue = false;
        void handler(object? sender, DuplicateArgumentEventArgs e)
        {
            Assert.AreEqual("Argument1", e.Argument.ArgumentName);
            Assert.AreEqual("foo", e.Argument.Value);
            Assert.AreEqual("bar", e.NewValue!.Value.ToString());
            handlerCalled = true;
            if (keepOldValue)
            {
                e.KeepOldValue = true;
            }
        }

        parser.DuplicateArgument += handler;

        // Handler is not called when duplicates not allowed.
        parser.Options.DuplicateArguments = ErrorMode.Error;
        CheckThrows(parser, ["-Argument1", "foo", "-Argument1", "bar"], CommandLineArgumentErrorCategory.DuplicateArgument, "Argument1", remainingArgumentCount: 2);
        Assert.IsFalse(handlerCalled);

        // Now it is called.
        parser.Options.DuplicateArguments = ErrorMode.Allow;
        result = CheckSuccess(parser, ["-Argument1", "foo", "-Argument1", "bar"]);
        Assert.AreEqual("bar", result.Argument1);
        Assert.IsTrue(handlerCalled);

        // Also called for warning, and keep the old value.
        parser.Options.DuplicateArguments = ErrorMode.Warning;
        handlerCalled = false;
        keepOldValue = true;
        result = CheckSuccess(parser, ["-Argument1", "foo", "-Argument1", "bar"]);
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

        result = CheckSuccess(parser, ["-ParseNullable", "", "-NullableMulti", "1", "", "2", "-ParseNullableMulti", "3", "", "4"]);
        Assert.IsNull(result.ParseNullable);
        Assert.AreEqual(1, result.NullableMulti[0]!.Value);
        Assert.IsNull(result.NullableMulti[1]);
        Assert.AreEqual(2, result.NullableMulti[2]!.Value);
        Assert.AreEqual(3, result.ParseNullableMulti[0]!.Value.Value);
        Assert.IsNull(result.ParseNullableMulti[1]!);
        Assert.AreEqual(4, result.ParseNullableMulti[2]!.Value.Value);
#if NET7_0_OR_GREATER
        Assert.IsInstanceOfType(((NullableConverter)parser.GetArgument("Nullable")!.Converter).BaseConverter, typeof(SpanParsableConverter<int>));
#endif
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestConversionInvalid(ProviderKind kind)
    {
        var parser = CreateParser<ConversionArguments>(kind);
        CheckThrows(parser, ["-Nullable", "abc"], CommandLineArgumentErrorCategory.ArgumentValueConversion, "Nullable", typeof(FormatException), 2);
        CheckThrows(parser, ["-Nullable", "12345678901234567890"], CommandLineArgumentErrorCategory.ArgumentValueConversion, "Nullable", typeof(OverflowException), 2);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestDerivedClass(ProviderKind kind)
    {
        var parser = CreateParser<DerivedArguments>(kind);
        Assert.AreEqual("Base class attribute.", parser.Description);
        Assert.AreEqual(4, parser.Arguments.Length);
        VerifyArguments(parser.Arguments,
        [
            new ExpectedArgument("BaseArg", typeof(string), ArgumentKind.SingleValue),
            new ExpectedArgument("DerivedArg", typeof(int), ArgumentKind.SingleValue),
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = ["?", "h"] },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        ]);
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
        // Null because IncludeDefaultInUsageHelp is false.
        Assert.IsNull(parser.GetArgument("Arg10")!.DefaultValue);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestAutoPrefixAliases(ProviderKind kind)
    {
        var parser = CreateParser<AutoPrefixAliasesArguments>(kind);

        // Shortest possible prefixes
        var result = parser.Parse(["-pro", "foo", "-Po", "5", "-e"]);
        Assert.IsNotNull(result);
        Assert.AreEqual("foo", result.Protocol);
        Assert.AreEqual(5, result.Port);
        Assert.IsTrue(result.EnablePrefix);

        // Ambiguous prefix
        CheckThrows(parser, ["-p", "foo"], CommandLineArgumentErrorCategory.AmbiguousPrefixAlias, "p", remainingArgumentCount: 2,
            possibleMatches: ["Port", "Prefix", "Protocol"]);

        // Ambiguous due to alias.
        CheckThrows(parser, ["-pr", "foo"], CommandLineArgumentErrorCategory.AmbiguousPrefixAlias, "pr", remainingArgumentCount: 2,
            possibleMatches: ["Prefix", "Protocol"]);

        // Prefix of an alias.
        result = parser.Parse(["-pre"]);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.EnablePrefix);

        // Disable auto prefix aliases.
        var options = new ParseOptions() { AutoPrefixAliases = false };
        parser = CreateParser<AutoPrefixAliasesArguments>(kind, options);
        CheckThrows(parser, ["-pro", "foo", "-Po", "5", "-e"], CommandLineArgumentErrorCategory.UnknownArgument, "pro", remainingArgumentCount: 5);
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
        VerifyArguments(parser.Arguments,
        [
            new ExpectedArgument("BaseArg1", typeof(string), ArgumentKind.SingleValue) { Position = 0, IsRequired = true },
            new ExpectedArgument("BaseArg2", typeof(int), ArgumentKind.SingleValue) { Position = 1 },
            new ExpectedArgument("Arg1", typeof(string), ArgumentKind.SingleValue) { Position = 2 },
            new ExpectedArgument("Arg2", typeof(int), ArgumentKind.SingleValue) { Position = 3 },
            new ExpectedArgument("Arg3", typeof(int), ArgumentKind.SingleValue),
            new ExpectedArgument("BaseArg3", typeof(int), ArgumentKind.SingleValue),
            new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = ["?", "h"] },
            new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
        ]);

        try
        {
            parser = new CommandLineParser<AutoPositionArguments>();
            Debug.Fail("Expected exception not thrown.");
        }
        catch (NotSupportedException)
        {
        }
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestUnknownArgument(ProviderKind kind)
    {
        var parser = CreateParser<LongShortArguments>(kind);
        ReadOnlyMemory<char> expectedName = default;
        ReadOnlyMemory<char> expectedValue = default;
        var expectedToken = "";
        var expectedCombined = false;
        var ignore = false;
        var cancel = CancelMode.None;
        var eventRaised = false;
        parser.UnknownArgument += (_, e) =>
        {
            AssertMemoryEqual(expectedName, e.Name);
            AssertMemoryEqual(expectedValue, e.Value);
            Assert.AreEqual(expectedToken, e.Token);
            Assert.AreEqual(expectedCombined, e.IsCombinedSwitchToken);
            e.CancelParsing = cancel;
            e.Ignore = ignore;
            eventRaised = true;
        };

        expectedName = "Unknown".AsMemory();
        expectedToken = "--Unknown";
        CheckThrows(parser, ["--arg1", "5", "--Unknown", "foo"], CommandLineArgumentErrorCategory.UnknownArgument, "Unknown", remainingArgumentCount: 2);
        Assert.IsTrue(eventRaised);

        eventRaised = false;
        ignore = true;
        var result = CheckSuccess(parser, ["--arg1", "5", "--Unknown", "1"]);
        Assert.AreEqual(1, result.Foo);
        Assert.AreEqual(5, result.Arg1);
        Assert.IsTrue(eventRaised);

        eventRaised = false;
        cancel = CancelMode.Success;
        result = CheckSuccess(parser, ["--arg1", "5", "--Unknown", "1"], "Unknown", 1);
        Assert.AreEqual(0, result.Foo);
        Assert.AreEqual(5, result.Arg1);
        Assert.IsTrue(eventRaised);

        eventRaised = false;
        cancel = CancelMode.Abort;
        CheckCanceled(parser, ["--arg1", "5", "--Unknown", "1"], "Unknown", false, 1);
        Assert.IsTrue(eventRaised);

        // With a value.
        expectedValue = "foo".AsMemory();
        expectedToken = "--Unknown:foo";
        CheckCanceled(parser, ["--arg1", "5", "--Unknown:foo", "1"], "Unknown", false, 1);
        Assert.IsTrue(eventRaised);

        // Now with a short name.
        eventRaised = false;
        expectedName = "z".AsMemory();
        expectedValue = default;
        expectedToken = "-z";
        cancel = CancelMode.None;
        result = CheckSuccess(parser, ["--arg1", "5", "-z", "1"]);
        Assert.AreEqual(1, result.Foo);
        Assert.AreEqual(5, result.Arg1);
        Assert.IsTrue(eventRaised);

        // One in a combined short name.
        eventRaised = false;
        expectedToken = "-szu";
        expectedCombined = true;
        cancel = CancelMode.None;
        result = CheckSuccess(parser, ["--arg1", "5", "-szu", "1"]);
        Assert.AreEqual(1, result.Foo);
        Assert.AreEqual(5, result.Arg1);
        Assert.IsTrue(result.Switch1);
        Assert.IsTrue(result.Switch3);
        Assert.IsTrue(eventRaised);

        // Positional
        eventRaised = false;
        expectedName = default;
        expectedValue = "4".AsMemory();
        expectedToken = "4";
        expectedCombined = false;
        result = CheckSuccess(parser, ["1", "2", "3", "4", "--arg1", "5"]);
        Assert.AreEqual(1, result.Foo);
        Assert.AreEqual(2, result.Bar);
        Assert.AreEqual(3, result.Arg2);
        Assert.AreEqual(5, result.Arg1);
        Assert.IsTrue(eventRaised);

        eventRaised = false;
        ignore = false;
        CheckThrows(parser, ["1", "2", "3", "4", "--arg1", "5"], CommandLineArgumentErrorCategory.TooManyArguments, remainingArgumentCount: 3);
        Assert.IsTrue(eventRaised);
    }

    [TestMethod]
    [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
    public void TestTypeValueDescription(ProviderKind kind)
    {
        var parser = CreateParser<TypeValueDescriptionArguments>(kind);
        VerifyArgument(parser.GetArgument("NonSwitch")!, new ExpectedArgument("NonSwitch", typeof(NonSwitchBoolean)) { ValueDescription = "Boolean" });
        VerifyArgument(parser.GetArgument("Nullable")!, new ExpectedArgument("Nullable", typeof(NonSwitchBoolean?)) { ValueDescription = "Boolean", ElementType = typeof(NonSwitchBoolean) });
        VerifyArgument(parser.GetArgument("Array")!, new ExpectedArgument("Array", typeof(NonSwitchBoolean[])) { ValueDescription = "Boolean", ElementType = typeof(NonSwitchBoolean), Kind = ArgumentKind.MultiValue });
        VerifyArgument(parser.GetArgument("Dict")!, new ExpectedArgument("Dict", typeof(Dictionary<string, NonSwitchBoolean>)) { ValueDescription = "String=Boolean", ElementType = typeof(KeyValuePair<string, NonSwitchBoolean>), Kind = ArgumentKind.Dictionary });
        VerifyArgument(parser.GetArgument("Overridden")!, new ExpectedArgument("Overridden", typeof(NonSwitchBoolean)) { ValueDescription = "Other" });
        VerifyArgument(parser.GetArgument("OverriddenTransform")!, new ExpectedArgument("OverriddenTransform", typeof(NonSwitchBoolean)) { ValueDescription = "Other" });
        VerifyArgument(parser.GetArgument("Enum")!, new ExpectedArgument("Enum", typeof(CustomEnum)) { ValueDescription = "MyEnum" });
        VerifyArgument(parser.GetArgument("Generic")!, new ExpectedArgument("Generic", typeof(KeyValuePair<NonSwitchBoolean, CustomEnum>)) { ValueDescription = "KeyValuePair<Boolean, MyEnum>" });

        var options = new ParseOptions()
        {
            ValueDescriptionTransform = NameTransform.DashCase
        };

        parser = CreateParser<TypeValueDescriptionArguments>(kind, options);
        VerifyArgument(parser.GetArgument("NonSwitch")!, new ExpectedArgument("NonSwitch", typeof(NonSwitchBoolean)) { ValueDescription = "boolean" });
        VerifyArgument(parser.GetArgument("Nullable")!, new ExpectedArgument("Nullable", typeof(NonSwitchBoolean?)) { ValueDescription = "boolean", ElementType = typeof(NonSwitchBoolean) });
        VerifyArgument(parser.GetArgument("Array")!, new ExpectedArgument("Array", typeof(NonSwitchBoolean[])) { ValueDescription = "boolean", ElementType = typeof(NonSwitchBoolean), Kind = ArgumentKind.MultiValue });
        VerifyArgument(parser.GetArgument("Dict")!, new ExpectedArgument("Dict", typeof(Dictionary<string, NonSwitchBoolean>)) { ValueDescription = "string=boolean", ElementType = typeof(KeyValuePair<string, NonSwitchBoolean>), Kind = ArgumentKind.Dictionary });
        VerifyArgument(parser.GetArgument("OverriddenTransform")!, new ExpectedArgument("OverriddenTransform", typeof(NonSwitchBoolean)) { ValueDescription = "other" });
        VerifyArgument(parser.GetArgument("Generic")!, new ExpectedArgument("Generic", typeof(KeyValuePair<NonSwitchBoolean, CustomEnum>)) { ValueDescription = "key-value-pair<boolean, MyEnum>" });
        // Don't have the transformation applied
        VerifyArgument(parser.GetArgument("Overridden")!, new ExpectedArgument("Overridden", typeof(NonSwitchBoolean)) { ValueDescription = "Other" });
        VerifyArgument(parser.GetArgument("Enum")!, new ExpectedArgument("Enum", typeof(CustomEnum)) { ValueDescription = "MyEnum" });

        options.DefaultValueDescriptions = new Dictionary<Type, string>()
        {
            { typeof(NonSwitchBoolean), "Other2" }
        };

        parser = CreateParser<TypeValueDescriptionArguments>(kind, options);
        VerifyArgument(parser.GetArgument("NonSwitch")!, new ExpectedArgument("NonSwitch", typeof(NonSwitchBoolean)) { ValueDescription = "Other2" });
        VerifyArgument(parser.GetArgument("Nullable")!, new ExpectedArgument("Nullable", typeof(NonSwitchBoolean?)) { ValueDescription = "Other2", ElementType = typeof(NonSwitchBoolean) });
        VerifyArgument(parser.GetArgument("Array")!, new ExpectedArgument("Array", typeof(NonSwitchBoolean[])) { ValueDescription = "Other2", ElementType = typeof(NonSwitchBoolean), Kind = ArgumentKind.MultiValue });
        VerifyArgument(parser.GetArgument("Dict")!, new ExpectedArgument("Dict", typeof(Dictionary<string, NonSwitchBoolean>)) { ValueDescription = "string=Other2", ElementType = typeof(KeyValuePair<string, NonSwitchBoolean>), Kind = ArgumentKind.Dictionary });
        VerifyArgument(parser.GetArgument("Overridden")!, new ExpectedArgument("Overridden", typeof(NonSwitchBoolean)) { ValueDescription = "Other" });
        VerifyArgument(parser.GetArgument("OverriddenTransform")!, new ExpectedArgument("OverriddenTransform", typeof(NonSwitchBoolean)) { ValueDescription = "other" });
        VerifyArgument(parser.GetArgument("Enum")!, new ExpectedArgument("Enum", typeof(CustomEnum)) { ValueDescription = "MyEnum" });
        VerifyArgument(parser.GetArgument("Generic")!, new ExpectedArgument("Generic", typeof(KeyValuePair<NonSwitchBoolean, CustomEnum>)) { ValueDescription = "key-value-pair<Other2, MyEnum>" });
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
        Assert.AreEqual(expected.Kind is ArgumentKind.MultiValue or ArgumentKind.Dictionary, argument.MultiValueInfo != null);
        Assert.AreEqual(expected.Kind == ArgumentKind.Dictionary, argument.DictionaryInfo != null);
        Assert.AreEqual(expected.IsSwitch, argument.IsSwitch);
        Assert.AreEqual(expected.DefaultValue, argument.DefaultValue);
        Assert.AreEqual(expected.IsHidden, argument.IsHidden);
        Assert.IsFalse(argument.MultiValueInfo?.AllowWhiteSpaceSeparator ?? false);
        Assert.IsNull(argument.Value);
        Assert.IsFalse(argument.HasValue);
        CollectionAssert.AreEqual(expected.Aliases ?? [], argument.Aliases.Select(a => a.Alias).ToArray());
        CollectionAssert.AreEqual(expected.ShortAliases ?? [], argument.ShortAliases.Select(a => a.Alias).ToArray());
        if (argument.MemberName.StartsWith("Automatic"))
        {
            Assert.IsNull(argument.Member);
        }
        else
        {
            Assert.IsNotNull(argument.Member);
            Assert.AreSame(argument.Parser.ArgumentsType.GetMember(argument.MemberName)[0], argument.Member);
        }
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
        Assert.IsFalse(target.ParseResult.HelpRequested);
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

    private static void CheckThrows(CommandLineParser parser, string[] arguments, CommandLineArgumentErrorCategory category,
        string? argumentName = null, Type? innerExceptionType = null, int remainingArgumentCount = 0, string[]? possibleMatches = null)
    {
        try
        {
            parser.Parse(arguments);
            Assert.Fail("Expected CommandLineException was not thrown.");
        }
        catch (CommandLineArgumentException ex)
        {
            Assert.IsTrue(parser.ParseResult.HelpRequested);
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

            if (possibleMatches != null)
            {
                Assert.IsInstanceOfType(ex, typeof(AmbiguousPrefixAliasException));
                CollectionAssert.AreEqual(possibleMatches, ((AmbiguousPrefixAliasException)ex).PossibleMatches);
            }
            else
            {
                Assert.IsNotInstanceOfType(ex, typeof(AmbiguousPrefixAliasException));
            }
        }
    }

    private static void CheckCanceled(CommandLineParser parser, string[] arguments, string argumentName, bool helpRequested, int remainingArgumentCount = 0)
    {
        Assert.IsNull(parser.Parse(arguments));
        Assert.AreEqual(ParseStatus.Canceled, parser.ParseResult.Status);
        Assert.AreEqual(argumentName, parser.ParseResult.ArgumentName);
        Assert.AreEqual(helpRequested, parser.ParseResult.HelpRequested);
        Assert.IsNull(parser.ParseResult.LastException);
        var remaining = arguments.AsMemory(arguments.Length - remainingArgumentCount);
        AssertMemoryEqual(remaining, parser.ParseResult.RemainingArguments);
    }

    private static T CheckSuccess<T>(CommandLineParser<T> parser, string[] arguments, string? argumentName = null, int remainingArgumentCount = 0)
        where T : class
    {
        var result = parser.Parse(arguments);
        Assert.IsNotNull(result);
        Assert.IsFalse(parser.ParseResult.HelpRequested);
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
            [ProviderKind.Generated]
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
