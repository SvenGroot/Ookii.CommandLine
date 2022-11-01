// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Ookii.CommandLine.Tests
{
    
    
    /// <summary>
    ///This is a test class for CommandLineParserTest and is intended
    ///to contain all CommandLineParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommandLineParserTest
    {
        /// <summary>
        ///A test for CommandLineParser Constructor
        ///</summary>
        [TestMethod()]
        public void ConstructorEmptyArgumentsTest()
        {
            Type argumentsType = typeof(EmptyArguments);
            CommandLineParser target = new CommandLineParser(argumentsType);
            Assert.AreEqual(CultureInfo.InvariantCulture, target.Culture);
            Assert.AreEqual(false, target.AllowDuplicateArguments);
            Assert.AreEqual(true, target.AllowWhiteSpaceValueSeparator);
            Assert.AreEqual(ParsingMode.Default, target.Mode);
            CollectionAssert.AreEqual(CommandLineParser.GetDefaultArgumentNamePrefixes(), target.ArgumentNamePrefixes);
            Assert.IsNull(target.LongArgumentNamePrefix);
            Assert.AreEqual(argumentsType, target.ArgumentsType);
            Assert.AreEqual(Assembly.GetExecutingAssembly().GetName().Name, target.ApplicationFriendlyName);
            Assert.AreEqual(string.Empty, target.Description);
            Assert.AreEqual(2, target.Arguments.Count);
            using var args = target.Arguments.GetEnumerator();
            TestArguments(target.Arguments, new[]
            {
                new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
                new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
            });
        }

        [TestMethod()]
        public void ConstructorTest()
        {
            Type argumentsType = typeof(TestArguments);
            CommandLineParser target = new CommandLineParser(argumentsType);
            Assert.AreEqual(CultureInfo.InvariantCulture, target.Culture);
            Assert.AreEqual(false, target.AllowDuplicateArguments);
            Assert.AreEqual(true, target.AllowWhiteSpaceValueSeparator);
            Assert.AreEqual(ParsingMode.Default, target.Mode);
            CollectionAssert.AreEqual(CommandLineParser.GetDefaultArgumentNamePrefixes(), target.ArgumentNamePrefixes);
            Assert.IsNull(target.LongArgumentNamePrefix);
            Assert.AreEqual(argumentsType, target.ArgumentsType);
            Assert.AreEqual("Friendly name", target.ApplicationFriendlyName);
            Assert.AreEqual("Test arguments description.", target.Description);
            Assert.AreEqual(18, target.Arguments.Count);
            TestArguments(target.Arguments, new[]
            {
                new ExpectedArgument("arg1", typeof(string)) { Position = 0, IsRequired = true, Description = "Arg1 description." },
                new ExpectedArgument("other", typeof(int)) { MemberName = "arg2", Position = 1, DefaultValue = 42, Description = "Arg2 description.", ValueDescription = "Number" },
                new ExpectedArgument("notSwitch", typeof(bool)) { Position = 2, DefaultValue = false },
                new ExpectedArgument("Arg5", typeof(float)) { Position = 3, Description = "Arg5 description." },
                new ExpectedArgument("other2", typeof(int)) { MemberName = "Arg4", Position = 4, DefaultValue = 47, Description = "Arg4 description.", ValueDescription = "Number" },
                new ExpectedArgument("Arg8", typeof(DayOfWeek[]), ArgumentKind.MultiValue) { ElementType = typeof(DayOfWeek), Position = 5 },
                new ExpectedArgument("Arg6", typeof(string)) { Position = null, IsRequired = true, Description = "Arg6 description.", Aliases = new[] { "Alias1", "Alias2" } },
                new ExpectedArgument("Arg10", typeof(bool[]), ArgumentKind.MultiValue) { ElementType = typeof(bool), Position = null, IsSwitch = true },
                new ExpectedArgument("Arg11", typeof(bool?)) { Position = null, ValueDescription = "Boolean", IsSwitch = true },
                new ExpectedArgument("Arg12", typeof(Collection<int>), ArgumentKind.MultiValue) { ElementType = typeof(int), Position = null, DefaultValue = 42 },
                new ExpectedArgument("Arg13", typeof(Dictionary<string, int>), ArgumentKind.Dictionary) { ElementType = typeof(KeyValuePair<string, int>), ValueDescription = "String=Int32" },
                new ExpectedArgument("Arg14", typeof(IDictionary<string, int>), ArgumentKind.Dictionary) { ElementType = typeof(KeyValuePair<string, int>), ValueDescription = "String=Int32" },
                new ExpectedArgument("Arg15", typeof(KeyValuePair<string, int>)) { ValueDescription = "KeyValuePair<String, Int32>" },
                new ExpectedArgument("Arg3", typeof(string)) { Position = null },
                new ExpectedArgument("Arg7", typeof(bool)) { Position = null, IsSwitch = true, Aliases = new[] { "Alias3" } },
                new ExpectedArgument("Arg9", typeof(int?)) { Position = null, ValueDescription = "Int32" },
                new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
                new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
            });
        }

        [TestMethod]
        public void ConstructorMultipleArgumentConstructorsTest()
        {
            Type argumentsType = typeof(MultipleConstructorsArguments);
            CommandLineParser target = new CommandLineParser(argumentsType);
            Assert.AreEqual(CultureInfo.InvariantCulture, target.Culture);
            Assert.AreEqual(false, target.AllowDuplicateArguments);
            Assert.AreEqual(true, target.AllowWhiteSpaceValueSeparator);
            Assert.AreEqual(ParsingMode.Default, target.Mode);
            CollectionAssert.AreEqual(CommandLineParser.GetDefaultArgumentNamePrefixes(), target.ArgumentNamePrefixes);
            Assert.IsNull(target.LongArgumentNamePrefix);
            Assert.AreEqual(argumentsType, target.ArgumentsType);
            Assert.AreEqual("", target.Description);
            Assert.AreEqual(4, target.Arguments.Count); // Constructor argument + one property argument.
            TestArguments(target.Arguments, new[]
            {
                new ExpectedArgument("arg1", typeof(string)) { Position = 0, IsRequired = true },
                new ExpectedArgument("Help", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticHelp", Description = "Displays this help message.", IsSwitch = true, Aliases = new[] { "?", "h" } },
                new ExpectedArgument("ThrowingArgument", typeof(int)),
                new ExpectedArgument("Version", typeof(bool), ArgumentKind.Method) { MemberName = "AutomaticVersion", Description = "Displays version information.", IsSwitch = true },
            });
        }

        [TestMethod]
        public void ParseTest()
        {
            var target = new CommandLineParser<TestArguments>();

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
            TestParse(target, "val1 2 true -arg3 val3 -other2:4 5.5 -arg6 val6 -arg7 -arg8 Monday -arg8 Tuesday -arg9 9 -arg10 -arg10 -arg10:false -arg11:false -arg12 12 -arg12 13 -arg13 foo=13 -arg13 bar=14 -arg14 hello=1 -arg14 bye=2 -arg15 something=5", "val1", 2, true, "val3", 4, 5.5f, "val6", true, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday }, 9, new[] { true, true, false }, false, new[] { 12, 13 }, new Dictionary<string,int>() { { "foo", 13 }, { "bar", 14 } }, new Dictionary<string,int>() { { "hello", 1 }, { "bye", 2 } }, new KeyValuePair<string,int>("something", 5));
            // Using aliases
            TestParse(target, "val1 2 -alias1 valalias6 -alias3", "val1", 2, arg6: "valalias6", arg7: true);
            // Long prefix cannot be used
            CheckThrows(() => target.Parse(new[] { "val1", "2", "--arg6", "val6" }), target, CommandLineArgumentErrorCategory.UnknownArgument, "-arg6");
            // Short name cannot be used
            CheckThrows(() => target.Parse(new[] { "val1", "2", "-arg6", "val6", "-a:5.5" }), target, CommandLineArgumentErrorCategory.UnknownArgument, "a");
        }

        [TestMethod]
        public void ParseTestEmptyArguments()
        {
            Type argumentsType = typeof(EmptyArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });

            // This test was added because version 2.0 threw an IndexOutOfRangeException when you tried to specify a positional argument when there were no positional arguments defined.
            CheckThrows(() => target.Parse(new[] { "Foo", "Bar" }), target, CommandLineArgumentErrorCategory.TooManyArguments);
        }

        [TestMethod]
        public void ParseTestTooManyArguments()
        {
            Type argumentsType = typeof(MultipleConstructorsArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });

            // Only accepts one positional argument.
            CheckThrows(() => target.Parse(new[] { "Foo", "Bar" }), target, CommandLineArgumentErrorCategory.TooManyArguments);
        }

        [TestMethod]
        public void ParseTestPropertySetterThrows()
        {
            Type argumentsType = typeof(MultipleConstructorsArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });

            CheckThrows(() => target.Parse(new[] { "Foo", "-ThrowingArgument", "-5" }),
                target,
                CommandLineArgumentErrorCategory.ApplyValueError,
                "ThrowingArgument",
                typeof(ArgumentOutOfRangeException));
        }

        [TestMethod]
        public void ParseTestConstructorThrows()
        {
            Type argumentsType = typeof(MultipleConstructorsArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });

            CheckThrows(() => target.Parse(new[] { "invalid" }), 
                target,
                CommandLineArgumentErrorCategory.CreateArgumentsTypeError,
                null,
                typeof(ArgumentException));
        }

        [TestMethod]
        public void ParseTestDuplicateDictionaryKeys()
        {
            Type argumentsType = typeof(DictionaryArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });

            DictionaryArguments args = (DictionaryArguments)target.Parse(new[] { "-DuplicateKeys", "Foo=1", "-DuplicateKeys", "Bar=2", "-DuplicateKeys", "Foo=3" });
            Assert.IsNotNull(args);
            Assert.AreEqual(2, args.DuplicateKeys.Count);
            Assert.AreEqual(3, args.DuplicateKeys["Foo"]);
            Assert.AreEqual(2, args.DuplicateKeys["Bar"]);

            CheckThrows(() => target.Parse(new[] { "-NoDuplicateKeys", "Foo=1", "-NoDuplicateKeys", "Bar=2", "-NoDuplicateKeys", "Foo=3" }), 
                target,
                CommandLineArgumentErrorCategory.InvalidDictionaryValue,
                "NoDuplicateKeys",
                typeof(ArgumentException));
        }

        [TestMethod]
        public void ParseTestMultiValueSeparator()
        {
            Type argumentsType = typeof(MultiValueSeparatorArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });

            MultiValueSeparatorArguments args = (MultiValueSeparatorArguments)target.Parse(new[] { "-NoSeparator", "Value1,Value2", "-NoSeparator", "Value3", "-Separator", "Value1,Value2", "-Separator", "Value3" });
            Assert.IsNotNull(args);
            CollectionAssert.AreEqual(new[] { "Value1,Value2", "Value3" }, args.NoSeparator);
            CollectionAssert.AreEqual(new[] { "Value1", "Value2", "Value3" }, args.Separator);
        }

        [TestMethod]
        public void ParseTestNameValueSeparator()
        {
            Type argumentsType = typeof(SimpleArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });
            Assert.AreEqual(CommandLineParser.DefaultNameValueSeparator, target.NameValueSeparator);
            SimpleArguments args =  (SimpleArguments)target.Parse(new[] { "-Argument1:test", "-Argument2:foo:bar" });
            Assert.IsNotNull(args);
            Assert.AreEqual("test", args.Argument1);
            Assert.AreEqual("foo:bar", args.Argument2);
            CheckThrows(() => target.Parse(new[] { "-Argument1=test" }), 
                target,
                CommandLineArgumentErrorCategory.UnknownArgument,
                "Argument1=test");

            target.NameValueSeparator = '=';
            args = (SimpleArguments)target.Parse(new[] { "-Argument1=test", "-Argument2=foo=bar" });
            Assert.IsNotNull(args);
            Assert.AreEqual("test", args.Argument1);
            Assert.AreEqual("foo=bar", args.Argument2);
            CheckThrows(() => target.Parse(new[] { "-Argument1:test" }),
                target,
                CommandLineArgumentErrorCategory.UnknownArgument,
                "Argument1:test");
        }

        [TestMethod]
        public void ParseTestKeyValueSeparator()
        {
            var target = new CommandLineParser(typeof(KeyValueSeparatorArguments));
            Assert.AreEqual("=", target.GetArgument("DefaultSeparator")!.KeyValueSeparator);
            Assert.AreEqual("String=Int32", target.GetArgument("DefaultSeparator")!.ValueDescription);
            Assert.AreEqual("<=>", target.GetArgument("CustomSeparator")!.KeyValueSeparator);
            Assert.AreEqual("String<=>String", target.GetArgument("CustomSeparator")!.ValueDescription);

            var result = (KeyValueSeparatorArguments)target.Parse(new[] { "-CustomSeparator", "foo<=>bar", "-CustomSeparator", "baz<=>contains<=>separator", "-CustomSeparator", "hello<=>" });
            Assert.IsNotNull(result);
            CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("foo", "bar"), KeyValuePair.Create("baz", "contains<=>separator"), KeyValuePair.Create("hello", "") }, result.CustomSeparator);
            CheckThrows(() => target.Parse(new[] { "-CustomSeparator", "foo=bar" }),
                target,
                CommandLineArgumentErrorCategory.ArgumentValueConversion,
                "CustomSeparator",
                typeof(FormatException));

            // Inner exception is Argument exception because what throws here is trying to convert
            // ">bar" to int.
            CheckThrows(() => target.Parse(new[] { "-DefaultSeparator", "foo<=>bar" }),
                target,
                CommandLineArgumentErrorCategory.ArgumentValueConversion,
                "DefaultSeparator",
                typeof(ArgumentException));
        }

        [TestMethod]
        public void TestWriteUsage()
        {
            Type argumentsType = typeof(TestArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });
            var options = new WriteUsageOptions()
            {
                ExecutableName = _executableName
            };

            string actual = target.GetUsage(0, options);
            Assert.AreEqual(_expectedDefaultUsage, actual);
        }

        [TestMethod]
        public void TestWriteUsageLongShort()
        {
            var target = new CommandLineParser<LongShortArguments>();
            var options = new WriteUsageOptions()
            {
                ExecutableName = _executableName
            };

            string actual = target.GetUsage(0, options);
            Assert.AreEqual(_expectedLongShortUsage, actual);

            options.UseShortNamesForSyntax = true;
            actual = target.GetUsage(0, options);
            Assert.AreEqual(_expectedLongShortUsageShortNameSyntax, actual);

            options = new WriteUsageOptions()
            {
                ExecutableName = _executableName,
                UseAbbreviatedSyntax = true,
            };

            actual = target.GetUsage(0, options);
            Assert.AreEqual(_expectedLongShortUsageAbbreviated, actual);
        }

        [TestMethod]
        public void TestWriteUsageFilter()
        {
            var target = new CommandLineParser<TestArguments>();
            var options = new WriteUsageOptions()
            {
                ExecutableName = _executableName,
                ArgumentDescriptionListFilter = DescriptionListFilterMode.Description
            };

            string actual = target.GetUsage(0, options);
            Assert.AreEqual(_expectedUsageDescriptionOnly, actual);

            options.ArgumentDescriptionListFilter = DescriptionListFilterMode.All;
            actual = target.GetUsage(0, options);
            Assert.AreEqual(_expectedUsageAll, actual);

            options.ArgumentDescriptionListFilter = DescriptionListFilterMode.None;
            actual = target.GetUsage(0, options);
            Assert.AreEqual(_expectedUsageNone, actual);
        }

        [TestMethod]
        public void TestWriteUsageColor()
        {
            var target = new CommandLineParser(typeof(TestArguments), new[] { "/", "-" });
            var options = new WriteUsageOptions()
            {
                ExecutableName = _executableName,
                UseColor = true,
            };

            string actual = target.GetUsage(0, options);
            Assert.AreEqual(_expectedUsageColor, actual);

            target = new CommandLineParser(typeof(LongShortArguments));
            actual = target.GetUsage(0, options);
            Assert.AreEqual(_expectedLongShortUsageColor, actual);
        }

        [TestMethod]
        public void TestStaticParse()
        {
            using var output = new StringWriter();
            using var error = new StringWriter();
            var options = new ParseOptions()
            {
                ArgumentNamePrefixes = new[] { "/", "-" },
                Out = output,
                Error = error,
            };

            options.UsageOptions.ExecutableName = _executableName;

            var result = CommandLineParser.Parse<TestArguments>(new[] { "foo", "-Arg6", "bar" }, options);
            Assert.IsNotNull(result);
            Assert.AreEqual("foo", result.Arg1);
            Assert.AreEqual("bar", result.Arg6);
            Assert.AreEqual(0, output.ToString().Length);
            Assert.AreEqual(0, error.ToString().Length);

            result = CommandLineParser.Parse<TestArguments>(new string[0], options);
            Assert.IsNull(result);
            Assert.IsTrue(error.ToString().Length > 0);
            Assert.AreEqual(_expectedDefaultUsage, output.ToString());

            output.GetStringBuilder().Clear();
            error.GetStringBuilder().Clear();
            result = CommandLineParser.Parse<TestArguments>(new[] { "-Help" }, options);
            Assert.IsNull(result);
            Assert.AreEqual(0, error.ToString().Length);
            Assert.AreEqual(_expectedDefaultUsage, output.ToString());
        }

        [TestMethod]
        public void TestCancelParsing()
        {
            var parser = new CommandLineParser(typeof(CancelArguments));

            // Don't cancel if -DoesCancel not specified.
            var result = (CancelArguments)parser.Parse(new[] { "-Argument1", "foo", "-DoesNotCancel", "-Argument2", "bar" });
            Assert.IsNotNull(result);
            Assert.IsFalse(parser.HelpRequested);
            Assert.IsTrue(result.DoesNotCancel);
            Assert.IsFalse(result.DoesCancel);
            Assert.AreEqual("foo", result.Argument1);
            Assert.AreEqual("bar", result.Argument2);

            // Cancel if -DoesCancel specified.
            result = (CancelArguments)parser.Parse(new[] { "-Argument1", "foo", "-DoesCancel", "-Argument2", "bar" });
            Assert.IsNull(result);
            Assert.IsTrue(parser.HelpRequested);
            Assert.IsTrue(parser.GetArgument("Argument1").HasValue);
            Assert.AreEqual("foo", (string)parser.GetArgument("Argument1").Value);
            Assert.IsTrue(parser.GetArgument("DoesCancel").HasValue);
            Assert.IsTrue((bool)parser.GetArgument("DoesCancel").Value);
            Assert.IsFalse(parser.GetArgument("DoesNotCancel").HasValue);
            Assert.IsNull(parser.GetArgument("DoesNotCancel").Value);
            Assert.IsFalse(parser.GetArgument("Argument2").HasValue);
            Assert.IsNull(parser.GetArgument("Argument2").Value);

            // Use the event handler to cancel on -DoesNotCancel.
            static void handler1(object sender, ArgumentParsedEventArgs e)
            {
                if (e.Argument.ArgumentName == "DoesNotCancel")
                    e.Cancel = true;
            }

            parser.ArgumentParsed += handler1;
            result = (CancelArguments)parser.Parse(new[] { "-Argument1", "foo", "-DoesNotCancel", "-Argument2", "bar" });
            Assert.IsNull(result);
            Assert.IsTrue(parser.HelpRequested);
            Assert.IsTrue(parser.GetArgument("Argument1").HasValue);
            Assert.AreEqual("foo", (string)parser.GetArgument("Argument1").Value);
            Assert.IsTrue(parser.GetArgument("DoesNotCancel").HasValue);
            Assert.IsTrue((bool)parser.GetArgument("DoesNotCancel").Value);
            Assert.IsFalse(parser.GetArgument("DoesCancel").HasValue);
            Assert.IsNull(parser.GetArgument("DoesCancel").Value);
            Assert.IsFalse(parser.GetArgument("Argument2").HasValue);
            Assert.IsNull(parser.GetArgument("Argument2").Value);
            parser.ArgumentParsed -= handler1;

            // Use the event handler to abort cancelling on -DoesCancel.
            static void handler2(object sender, ArgumentParsedEventArgs e)
            {
                if (e.Argument.ArgumentName == "DoesCancel")
                    e.OverrideCancelParsing = true;
            }

            parser.ArgumentParsed += handler2;
            result = (CancelArguments)parser.Parse(new[] { "-Argument1", "foo", "-DoesCancel", "-Argument2", "bar" });
            Assert.IsNotNull(result);
            Assert.IsFalse(parser.HelpRequested);
            Assert.IsFalse(result.DoesNotCancel);
            Assert.IsTrue(result.DoesCancel);
            Assert.AreEqual("foo", result.Argument1);
            Assert.AreEqual("bar", result.Argument2);

            // Automatic help argument should cancel.
            result = (CancelArguments)parser.Parse(new[] { "-Help" });
            Assert.IsNull(result);
            Assert.IsTrue(parser.HelpRequested);
        }

        [TestMethod]
        public void TestParseOptionsAttribute()
        {
            var parser = new CommandLineParser(typeof(ParseOptionsArguments));
            Assert.IsFalse(parser.AllowWhiteSpaceValueSeparator);
            Assert.IsTrue(parser.AllowDuplicateArguments);
            Assert.AreEqual('=', parser.NameValueSeparator);
            Assert.AreEqual(ParsingMode.LongShort, parser.Mode);
            CollectionAssert.AreEqual(new[] { "--", "-" }, parser.ArgumentNamePrefixes);
            Assert.AreEqual("---", parser.LongArgumentNamePrefix);
            // Verify case sensitivity.
            Assert.IsNull(parser.GetArgument("argument"));
            Assert.IsNotNull(parser.GetArgument("Argument"));
            // Verify no auto help argument.
            Assert.IsNull(parser.GetArgument("Help"));

            // Constructor params take precedence.
            parser = new CommandLineParser(typeof(ParseOptionsArguments), new[] { "+" }, StringComparer.OrdinalIgnoreCase);
            Assert.IsFalse(parser.AllowWhiteSpaceValueSeparator);
            Assert.IsTrue(parser.AllowDuplicateArguments);
            Assert.AreEqual('=', parser.NameValueSeparator);
            Assert.AreEqual(ParsingMode.LongShort, parser.Mode);
            CollectionAssert.AreEqual(new[] { "+" }, parser.ArgumentNamePrefixes);
            Assert.AreEqual("---", parser.LongArgumentNamePrefix);
            // Verify case insensitivity.
            Assert.IsNotNull(parser.GetArgument("argument"));
            Assert.IsNotNull(parser.GetArgument("Argument"));
            // Verify no auto help argument.
            Assert.IsNull(parser.GetArgument("Help"));

            // ParseOptions take precedence
            var options = new ParseOptions()
            {
                Mode = ParsingMode.Default,
                ArgumentNameComparer = StringComparer.OrdinalIgnoreCase,
                AllowWhiteSpaceValueSeparator = true,
                AllowDuplicateArguments = false,
                NameValueSeparator = ';',
                ArgumentNamePrefixes = new[] { "+" },
                AutoHelpArgument = true,
            };

            parser = new CommandLineParser(typeof(ParseOptionsArguments), options);
            Assert.IsTrue(parser.AllowWhiteSpaceValueSeparator);
            Assert.IsFalse(parser.AllowDuplicateArguments);
            Assert.AreEqual(';', parser.NameValueSeparator);
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
        public void TestCulture()
        {
            var result = CommandLineParser.Parse<CultureArguments>(new[] { "-Argument", "5.5 " });
            Assert.IsNotNull(result);
            Assert.AreEqual(5.5, result.Argument);
            Assert.IsNull(CommandLineParser.Parse<CultureArguments>(new[] { "-Argument", "5,5 " }));

            var options = new ParseOptions { Culture = new CultureInfo("nl-NL") };
            result = CommandLineParser.Parse<CultureArguments>(new[] { "-Argument", "5,5" }, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(5.5, result.Argument);
            Assert.IsNull(CommandLineParser.Parse<CultureArguments>(new[] { "-Argument", "5.5" }, options));
        }

        [TestMethod]
        public void TestLongShortMode()
        {
            var parser = new CommandLineParser<LongShortArguments>();
            Assert.AreEqual(ParsingMode.LongShort, parser.Mode);
            Assert.AreEqual(CommandLineParser.DefaultLongArgumentNamePrefix, parser.LongArgumentNamePrefix);
            CollectionAssert.AreEqual(CommandLineParser.GetDefaultArgumentNamePrefixes(), parser.ArgumentNamePrefixes);
            Assert.AreSame(parser.GetArgument("foo"), parser.GetShortArgument('f'));
            Assert.AreSame(parser.GetArgument("arg2"), parser.GetShortArgument('a'));
            Assert.AreSame(parser.GetArgument("switch1"), parser.GetShortArgument('s'));
            Assert.AreSame(parser.GetArgument("switch2"), parser.GetShortArgument('t'));
            Assert.IsNull(parser.GetArgument("switch3"));
            Assert.AreEqual("u", parser.GetShortArgument('u').ArgumentName);
            Assert.AreEqual('f', parser.GetArgument("foo").ShortName);
            Assert.IsTrue(parser.GetArgument("foo").HasShortName);
            Assert.AreEqual('\0', parser.GetArgument("bar").ShortName);
            Assert.IsFalse(parser.GetArgument("bar").HasShortName);

            var result = parser.Parse(new[] { "-f", "5", "--bar", "6", "-a", "7", "--arg1", "8", "-s" });
            Assert.AreEqual(5, result.Foo);
            Assert.AreEqual(6, result.Bar);
            Assert.AreEqual(7, result.Arg2);
            Assert.AreEqual(8, result.Arg1);
            Assert.IsTrue(result.Switch1);
            Assert.IsFalse(result.Switch2);
            Assert.IsFalse(result.Switch3);

            // Combine switches.
            result = parser.Parse(new[] { "-su" });
            Assert.IsTrue(result.Switch1);
            Assert.IsFalse(result.Switch2);
            Assert.IsTrue(result.Switch3);

            // Use a short alias.
            result = parser.Parse(new[] { "-b", "5" });
            Assert.AreEqual(5, result.Arg2);

            // Combining non-switches is an error.
            CheckThrows(() => parser.Parse(new[] { "-sf" }), parser, CommandLineArgumentErrorCategory.CombinedShortNameNonSwitch, "sf");

            // Can't use long argument prefix with short names.
            CheckThrows(() => parser.Parse(new[] { "--s" }), parser, CommandLineArgumentErrorCategory.UnknownArgument, "s");

            // And vice versa.
            CheckThrows(() => parser.Parse(new[] { "-Switch1" }), parser, CommandLineArgumentErrorCategory.UnknownArgument, "w");

            // Short alias is ignored on an argument without a short name.
            CheckThrows(() => parser.Parse(new[] { "-c" }), parser, CommandLineArgumentErrorCategory.UnknownArgument, "c");
        }

        [TestMethod]
        public void TestMethodArguments()
        {
            var parser = new CommandLineParser<MethodArguments>();

            Assert.AreEqual(ArgumentKind.Method, parser.GetArgument("NoCancel").Kind);
            Assert.IsNull(parser.GetArgument("NotAnArgument"));
            Assert.IsNull(parser.GetArgument("NotStatic"));
            Assert.IsNull(parser.GetArgument("NotPublic"));

            Assert.IsNotNull(parser.Parse(new[] { "-NoCancel" }));
            Assert.IsFalse(parser.HelpRequested);
            Assert.AreEqual(nameof(MethodArguments.NoCancel), MethodArguments.CalledMethodName);

            Assert.IsNull(parser.Parse(new[] { "-Cancel" }));
            Assert.IsFalse(parser.HelpRequested);
            Assert.AreEqual(nameof(MethodArguments.Cancel), MethodArguments.CalledMethodName);

            Assert.IsNull(parser.Parse(new[] { "-CancelWithHelp" }));
            Assert.IsTrue(parser.HelpRequested);
            Assert.AreEqual(nameof(MethodArguments.CancelWithHelp), MethodArguments.CalledMethodName);

            Assert.IsNotNull(parser.Parse(new[] { "-CancelWithValue", "1" }));
            Assert.IsFalse(parser.HelpRequested);
            Assert.AreEqual(nameof(MethodArguments.CancelWithValue), MethodArguments.CalledMethodName);
            Assert.AreEqual(1, MethodArguments.Value);

            Assert.IsNull(parser.Parse(new[] { "-CancelWithValue", "-1" }));
            Assert.IsFalse(parser.HelpRequested);
            Assert.AreEqual(nameof(MethodArguments.CancelWithValue), MethodArguments.CalledMethodName);
            Assert.AreEqual(-1, MethodArguments.Value);

            Assert.IsNotNull(parser.Parse(new[] { "-CancelWithValueAndHelp", "1" }));
            Assert.IsFalse(parser.HelpRequested);
            Assert.AreEqual(nameof(MethodArguments.CancelWithValueAndHelp), MethodArguments.CalledMethodName);
            Assert.AreEqual(1, MethodArguments.Value);

            Assert.IsNull(parser.Parse(new[] { "-CancelWithValueAndHelp", "-1" }));
            Assert.IsTrue(parser.HelpRequested);
            Assert.AreEqual(nameof(MethodArguments.CancelWithValueAndHelp), MethodArguments.CalledMethodName);
            Assert.AreEqual(-1, MethodArguments.Value);

            Assert.IsNotNull(parser.Parse(new[] { "-NoReturn" }));
            Assert.IsFalse(parser.HelpRequested);
            Assert.AreEqual(nameof(MethodArguments.NoReturn), MethodArguments.CalledMethodName);

            Assert.IsNotNull(parser.Parse(new[] { "42" }));
            Assert.IsFalse(parser.HelpRequested);
            Assert.AreEqual(nameof(MethodArguments.Positional), MethodArguments.CalledMethodName);
            Assert.AreEqual(42, MethodArguments.Value);
        }

        [TestMethod]
        public void TestAutomaticArgumentConflict()
        {
            var parser = new CommandLineParser(typeof(AutomaticConflictingNameArguments));
            TestArgument(parser.GetArgument("Help"), new ExpectedArgument("Help", typeof(int)));
            TestArgument(parser.GetArgument("Version"), new ExpectedArgument("Version", typeof(int)));

            parser = new CommandLineParser(typeof(AutomaticConflictingShortNameArguments));
            TestArgument(parser.GetShortArgument('?'), new ExpectedArgument("Foo", typeof(int)) { ShortName = '?' });
        }

        [TestMethod]
        public void TestHiddenArgument()
        {
            var parser = new CommandLineParser<HiddenArguments>();

            // Verify the hidden argument exists.
            TestArgument(parser.GetArgument("Hidden"), new ExpectedArgument("Hidden", typeof(int)) { IsHidden = true });

            // Verify it's not in the usage.
            var options = new WriteUsageOptions()
            {
                ExecutableName = _executableName,
                ArgumentDescriptionListFilter = DescriptionListFilterMode.All,
            };

            var usage = parser.GetUsage(0, options);
            Assert.AreEqual(_expectedUsageHidden, usage);
        }

        [TestMethod]
        public void TestNameTransformPascalCase()
        {
            var options = new ParseOptions
            {
                NameTransform = NameTransform.PascalCase
            };

            var parser = new CommandLineParser<NameTransformArguments>(options);
            TestArguments(parser.Arguments, new[]
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
        public void TestNameTransformCamelCase()
        {
            var options = new ParseOptions
            {
                NameTransform = NameTransform.CamelCase
            };

            var parser = new CommandLineParser<NameTransformArguments>(options);
            TestArguments(parser.Arguments, new[]
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
        public void TestNameTransformSnakeCase()
        {
            var options = new ParseOptions
            {
                NameTransform = NameTransform.SnakeCase
            };

            var parser = new CommandLineParser<NameTransformArguments>(options);
            TestArguments(parser.Arguments, new[]
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
        public void TestNameTransformDashCase()
        {
            var options = new ParseOptions
            {
                NameTransform = NameTransform.DashCase
            };

            var parser = new CommandLineParser<NameTransformArguments>(options);
            TestArguments(parser.Arguments, new[]
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
        public void TestValidation()
        {
            var parser = new CommandLineParser<ValidationArguments>();

            // Range validator on property
            CheckThrows(() => parser.Parse(new[] { "-Arg1", "0" }), parser, CommandLineArgumentErrorCategory.ValidationFailed, "Arg1");
            var result = parser.Parse(new[] { "-Arg1", "1" });
            Assert.AreEqual(1, result.Arg1);
            result = parser.Parse(new[] { "-Arg1", "5" });
            Assert.AreEqual(5, result.Arg1);
            CheckThrows(() => parser.Parse(new[] { "-Arg1", "6" }), parser, CommandLineArgumentErrorCategory.ValidationFailed, "Arg1");

            // Not null or empty on ctor parameter
            CheckThrows(() => parser.Parse(new[] { "" }), parser, CommandLineArgumentErrorCategory.ValidationFailed, "arg2");
            result = parser.Parse(new[] { " " });
            Assert.AreEqual(" ", result.Arg2);

            // Multiple validators on method
            CheckThrows(() => parser.Parse(new[] { "-Arg3", "1238" }), parser, CommandLineArgumentErrorCategory.ValidationFailed, "Arg3");
            Assert.AreEqual(0, ValidationArguments.Arg3Value);
            CheckThrows(() => parser.Parse(new[] { "-Arg3", "123" }), parser, CommandLineArgumentErrorCategory.ValidationFailed, "Arg3");
            Assert.AreEqual(0, ValidationArguments.Arg3Value);
            CheckThrows(() => parser.Parse(new[] { "-Arg3", "7001" }), parser, CommandLineArgumentErrorCategory.ValidationFailed, "Arg3");
            // Range validation is done after setting the value, so this was set!
            Assert.AreEqual(7001, ValidationArguments.Arg3Value);
            parser.Parse(new[] { "-Arg3", "1023" });
            Assert.AreEqual(1023, ValidationArguments.Arg3Value);

            // Validator on multi-value argument
            CheckThrows(() => parser.Parse(new[] { "-Arg4", "foo;bar;bazz" }), parser, CommandLineArgumentErrorCategory.ValidationFailed, "Arg4");
            CheckThrows(() => parser.Parse(new[] { "-Arg4", "foo", "-Arg4", "bar", "-Arg4", "bazz" }), parser, CommandLineArgumentErrorCategory.ValidationFailed, "Arg4");
            result = parser.Parse(new[] { "-Arg4", "foo;bar" });
            CollectionAssert.AreEqual(new[] { "foo", "bar" }, result.Arg4);
            result = parser.Parse(new[] { "-Arg4", "foo", "-Arg4", "bar" });
            CollectionAssert.AreEqual(new[] { "foo", "bar" }, result.Arg4);

            // Count validator
            CheckThrows(() => parser.Parse(new[] { "-Arg4", "foo" }), parser, CommandLineArgumentErrorCategory.ValidationFailed, "Arg4");
            CheckThrows(() => parser.Parse(new[] { "-Arg4", "foo;bar;baz;ban;bap" }), parser, CommandLineArgumentErrorCategory.ValidationFailed, "Arg4");
            result = parser.Parse(new[] { "-Arg4", "foo;bar;baz;ban" });
            CollectionAssert.AreEqual(new[] { "foo", "bar", "baz", "ban" }, result.Arg4);
        }

        private record class ExpectedArgument
        {
            public ExpectedArgument(string name, Type type, ArgumentKind kind = ArgumentKind.SingleValue)
            {
                Name = name;
                Type = type;
                Kind = kind;
            }

            public string Name { get; set; }
            public string MemberName { get; set; }
            public Type Type { get; set; }
            public Type ElementType { get; set; }
            public int? Position { get; set; }
            public bool IsRequired { get; set; }
            public object DefaultValue { get; set; }
            public string Description { get; set; }
            public string ValueDescription { get; set; }
            public bool IsSwitch { get; set; }
            public ArgumentKind Kind { get; set; }
            public string[] Aliases { get; set; }
            public char? ShortName { get; set; }
            public char[] ShortAliases { get; set; }
            public bool IsHidden { get; set; }
        }

        private static void TestArgument(CommandLineArgument argument, ExpectedArgument expected)
        {
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
            Assert.IsNull(argument.Value);
            Assert.IsFalse(argument.HasValue);
            CollectionAssert.AreEqual(expected.Aliases, argument.Aliases);
            CollectionAssert.AreEqual(expected.ShortAliases, argument.ShortAliases);
        }

        private static void TestArguments(IEnumerable<CommandLineArgument> arguments, ExpectedArgument[] expected)
        {
            int index = 0;
            foreach (var arg in arguments)
            {
                Assert.IsTrue(index < expected.Length, "Too many arguments.");
                TestArgument(arg, expected[index]);
                ++index;
            }
        }

        private static void TestParse(CommandLineParser<TestArguments> target, string commandLine, string arg1 = null, int arg2 = 42, bool notSwitch = false, string arg3 = null, int arg4 = 47, float arg5 = 0.0f, string arg6 = null, bool arg7 = false, DayOfWeek[] arg8 = null, int? arg9 = null, bool[] arg10 = null, bool? arg11 = null, int[] arg12 = null, Dictionary<string, int> arg13 = null, Dictionary<string, int> arg14 = null, KeyValuePair<string, int>? arg15 = null)
        {
            string[] args = commandLine.Split(' '); // not using quoted arguments in the tests, so this is fine.
            var result = target.Parse(args);
            Assert.IsNotNull(result);
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
            if( arg12 == null )
                Assert.AreEqual(0, result.Arg12.Count);
            else
                CollectionAssert.AreEqual(arg12, result.Arg12);
            CollectionAssert.AreEqual(arg13, result.Arg13);
            if( arg14 == null )
                Assert.AreEqual(0, result.Arg14.Count);
            else
                CollectionAssert.AreEqual(arg14, (System.Collections.ICollection)result.Arg14);
            if( arg15 == null )
                Assert.AreEqual(default(KeyValuePair<string, int>), result.Arg15);
            else
                Assert.AreEqual(arg15.Value, result.Arg15);
        }

        private static void CheckThrows(Action operation, CommandLineParser parser, CommandLineArgumentErrorCategory category, string argumentName = null, Type innerExceptionType = null)
        {
            try
            {
                operation();
                Assert.Fail("Expected CommandLineException was not thrown.");
            }
            catch (CommandLineArgumentException ex)
            {
                Assert.IsTrue(parser.HelpRequested);
                Assert.AreEqual(category, ex.Category);
                Assert.AreEqual(argumentName, ex.ArgumentName);
                if (innerExceptionType == null)
                    Assert.IsNull(ex.InnerException);
                else
                    Assert.IsInstanceOfType(ex.InnerException, innerExceptionType);
            }
        }

        #region Expected usage

        private const string _executableName = "test";

        private static readonly string _expectedDefaultUsage = @"Test arguments description.

Usage: test [/arg1] <String> [[/other] <Number>] [[/notSwitch] <Boolean>] [[/Arg5] <Single>] [[/other2] <Number>] [[/Arg8] <DayOfWeek>...] /Arg6 <String> [/Arg10...] [/Arg11] [/Arg12 <Int32>...] [/Arg13 <String=Int32>...] [/Arg14 <String=Int32>...] [/Arg15 <KeyValuePair<String, Int32>>] [/Arg3 <String>] [/Arg7] [/Arg9 <Int32>] [/Help] [/Version]

    /arg1 <String>
        Arg1 description.

    /other <Number>
        Arg2 description. Default value: 42.

    /notSwitch <Boolean>
         Default value: False.

    /Arg5 <Single>
        Arg5 description.

    /other2 <Number>
        Arg4 description. Default value: 47.

    /Arg6 <String> (/Alias1, /Alias2)
        Arg6 description.

    /Arg12 <Int32>
         Default value: 42.

    /Arg7 [<Boolean>] (/Alias3)


    /Help [<Boolean>] (/?, /h)
        Displays this help message.

    /Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

        private static readonly string _expectedLongShortUsage = @"Usage: test [[--foo] <Int32>] [[--bar] <Int32>] [--Arg1 <Int32>] [--Arg2 <Int32>] [--Help] [--Switch1] [--Switch2] [-u] [--Version]

    -f, --foo <Int32>
            Foo description. Default value: 0.

        --bar <Int32>
            Bar description. Default value: 0.

        --Arg1 <Int32>
            Arg1 description.

    -a, --Arg2 <Int32> (-b, --baz)
            Arg2 description.

    -?, --Help [<Boolean>] (-h)
            Displays this help message.

    -S, --Switch1 [<Boolean>]
            Switch1 description.

    -t, --Switch2 [<Boolean>]
            Switch2 description.

    -u [<Boolean>]
            Switch3 description.

        --Version [<Boolean>]
            Displays version information.

".ReplaceLineEndings();

        private static readonly string _expectedLongShortUsageShortNameSyntax = @"Usage: test [[-f] <Int32>] [[--bar] <Int32>] [--Arg1 <Int32>] [-a <Int32>] [-?] [-S] [-t] [-u] [--Version]

    -f, --foo <Int32>
            Foo description. Default value: 0.

        --bar <Int32>
            Bar description. Default value: 0.

        --Arg1 <Int32>
            Arg1 description.

    -a, --Arg2 <Int32> (-b, --baz)
            Arg2 description.

    -?, --Help [<Boolean>] (-h)
            Displays this help message.

    -S, --Switch1 [<Boolean>]
            Switch1 description.

    -t, --Switch2 [<Boolean>]
            Switch2 description.

    -u [<Boolean>]
            Switch3 description.

        --Version [<Boolean>]
            Displays version information.

".ReplaceLineEndings();

        private static readonly string _expectedLongShortUsageAbbreviated = @"Usage: test [[--foo] <Int32>] [[--bar] <Int32>] [arguments]

    -f, --foo <Int32>
            Foo description. Default value: 0.

        --bar <Int32>
            Bar description. Default value: 0.

        --Arg1 <Int32>
            Arg1 description.

    -a, --Arg2 <Int32> (-b, --baz)
            Arg2 description.

    -?, --Help [<Boolean>] (-h)
            Displays this help message.

    -S, --Switch1 [<Boolean>]
            Switch1 description.

    -t, --Switch2 [<Boolean>]
            Switch2 description.

    -u [<Boolean>]
            Switch3 description.

        --Version [<Boolean>]
            Displays version information.

".ReplaceLineEndings();

        private static readonly string _expectedUsageDescriptionOnly = @"Test arguments description.

Usage: test [-arg1] <String> [[-other] <Number>] [[-notSwitch] <Boolean>] [[-Arg5] <Single>] [[-other2] <Number>] [[-Arg8] <DayOfWeek>...] -Arg6 <String> [-Arg10...] [-Arg11] [-Arg12 <Int32>...] [-Arg13 <String=Int32>...] [-Arg14 <String=Int32>...] [-Arg15 <KeyValuePair<String, Int32>>] [-Arg3 <String>] [-Arg7] [-Arg9 <Int32>] [-Help] [-Version]

    -arg1 <String>
        Arg1 description.

    -other <Number>
        Arg2 description. Default value: 42.

    -Arg5 <Single>
        Arg5 description.

    -other2 <Number>
        Arg4 description. Default value: 47.

    -Arg6 <String> (-Alias1, -Alias2)
        Arg6 description.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

        private static readonly string _expectedUsageAll = @"Test arguments description.

Usage: test [-arg1] <String> [[-other] <Number>] [[-notSwitch] <Boolean>] [[-Arg5] <Single>] [[-other2] <Number>] [[-Arg8] <DayOfWeek>...] -Arg6 <String> [-Arg10...] [-Arg11] [-Arg12 <Int32>...] [-Arg13 <String=Int32>...] [-Arg14 <String=Int32>...] [-Arg15 <KeyValuePair<String, Int32>>] [-Arg3 <String>] [-Arg7] [-Arg9 <Int32>] [-Help] [-Version]

    -arg1 <String>
        Arg1 description.

    -other <Number>
        Arg2 description. Default value: 42.

    -notSwitch <Boolean>
         Default value: False.

    -Arg5 <Single>
        Arg5 description.

    -other2 <Number>
        Arg4 description. Default value: 47.

    -Arg8 <DayOfWeek>


    -Arg6 <String> (-Alias1, -Alias2)
        Arg6 description.

    -Arg10 [<Boolean>]


    -Arg11 [<Boolean>]


    -Arg12 <Int32>
         Default value: 42.

    -Arg13 <String=Int32>


    -Arg14 <String=Int32>


    -Arg15 <KeyValuePair<String, Int32>>


    -Arg3 <String>


    -Arg7 [<Boolean>] (-Alias3)


    -Arg9 <Int32>


    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

        private static readonly string _expectedUsageNone = @"Test arguments description.

Usage: test [-arg1] <String> [[-other] <Number>] [[-notSwitch] <Boolean>] [[-Arg5] <Single>] [[-other2] <Number>] [[-Arg8] <DayOfWeek>...] -Arg6 <String> [-Arg10...] [-Arg11] [-Arg12 <Int32>...] [-Arg13 <String=Int32>...] [-Arg14 <String=Int32>...] [-Arg15 <KeyValuePair<String, Int32>>] [-Arg3 <String>] [-Arg7] [-Arg9 <Int32>] [-Help] [-Version]

".ReplaceLineEndings();

        // Raw strings would be nice here so including the escape character directly wouldn't be
        // necessary but that requires C# 11.
        private static readonly string _expectedUsageColor = @"Test arguments description.

[36mUsage:[0m test [/arg1] <String> [[/other] <Number>] [[/notSwitch] <Boolean>] [[/Arg5] <Single>] [[/other2] <Number>] [[/Arg8] <DayOfWeek>...] /Arg6 <String> [/Arg10...] [/Arg11] [/Arg12 <Int32>...] [/Arg13 <String=Int32>...] [/Arg14 <String=Int32>...] [/Arg15 <KeyValuePair<String, Int32>>] [/Arg3 <String>] [/Arg7] [/Arg9 <Int32>] [/Help] [/Version]

    [32m/arg1 <String>[0m
        Arg1 description.

    [32m/other <Number>[0m
        Arg2 description. Default value: 42.

    [32m/notSwitch <Boolean>[0m
         Default value: False.

    [32m/Arg5 <Single>[0m
        Arg5 description.

    [32m/other2 <Number>[0m
        Arg4 description. Default value: 47.

    [32m/Arg6 <String> (/Alias1, /Alias2)[0m
        Arg6 description.

    [32m/Arg12 <Int32>[0m
         Default value: 42.

    [32m/Arg7 [<Boolean>] (/Alias3)[0m


    [32m/Help [<Boolean>] (/?, /h)[0m
        Displays this help message.

    [32m/Version [<Boolean>][0m
        Displays version information.

".ReplaceLineEndings();

        private static readonly string _expectedLongShortUsageColor = @"[36mUsage:[0m test [[--foo] <Int32>] [[--bar] <Int32>] [--Arg1 <Int32>] [--Arg2 <Int32>] [--Help] [--Switch1] [--Switch2] [-u] [--Version]

    [32m-f, --foo <Int32>[0m
            Foo description. Default value: 0.

    [32m    --bar <Int32>[0m
            Bar description. Default value: 0.

    [32m    --Arg1 <Int32>[0m
            Arg1 description.

    [32m-a, --Arg2 <Int32> (-b, --baz)[0m
            Arg2 description.

    [32m-?, --Help [<Boolean>] (-h)[0m
            Displays this help message.

    [32m-S, --Switch1 [<Boolean>][0m
            Switch1 description.

    [32m-t, --Switch2 [<Boolean>][0m
            Switch2 description.

    [32m-u [<Boolean>][0m
            Switch3 description.

    [32m    --Version [<Boolean>][0m
            Displays version information.

".ReplaceLineEndings();

        private static readonly string _expectedUsageHidden = @"Usage: test [-Foo <Int32>] [-Help] [-Version]

    -Foo <Int32>


    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

        #endregion

    }
}
