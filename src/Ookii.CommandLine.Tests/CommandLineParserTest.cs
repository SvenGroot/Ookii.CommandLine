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
            Assert.AreEqual(string.Empty, target.Description);
            Assert.AreEqual(0, target.Arguments.Count);
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
            Assert.AreEqual("Test arguments description.", target.Description);
            Assert.AreEqual(16, target.Arguments.Count);
            using( IEnumerator<CommandLineArgument> args = target.Arguments.GetEnumerator() )
            {
                TestArgument(args, "arg1", "arg1", typeof(string), null, 0, true, null, "Arg1 description.", "String", false, false);
                TestArgument(args, "other", "arg2", typeof(int), null, 1, false, 42, "Arg2 description.", "Number", false, false);
                TestArgument(args, "notSwitch", "notSwitch", typeof(bool), null, 2, false, false, "", "Boolean", false, false);
                TestArgument(args, "Arg5", "Arg5", typeof(float), null, 3, false, null, "Arg5 description.", "Single", false, false);
                TestArgument(args, "other2", "Arg4", typeof(int), null, 4, false, 47, "Arg4 description.", "Number", false, false);
                TestArgument(args, "Arg8", "Arg8", typeof(DayOfWeek[]), typeof(DayOfWeek), 5, false, null, "", "DayOfWeek", false, true);
                TestArgument(args, "Arg6", "Arg6", typeof(string), null, null, true, null, "Arg6 description.", "String", false, false, false, "Alias1", "Alias2");
                TestArgument(args, "Arg10", "Arg10", typeof(bool[]), typeof(bool), null, false, null, "", "Boolean", true, true);
                TestArgument(args, "Arg11", "Arg11", typeof(bool?), null, null, false, null, "", "Boolean", true, false);
                TestArgument(args, "Arg12", "Arg12", typeof(Collection<int>), typeof(int), null, false, 42, "", "Int32", false, true);
                TestArgument(args, "Arg13", "Arg13", typeof(Dictionary<string, int>), typeof(KeyValuePair<string, int>), null, false, null, "", "String=Int32", false, true, true);
                TestArgument(args, "Arg14", "Arg14", typeof(IDictionary<string, int>), typeof(KeyValuePair<string, int>), null, false, null, "", "String=Int32", false, true, true);
                TestArgument(args, "Arg15", "Arg15", typeof(KeyValuePair<string, int>), typeof(KeyValuePair<string, int>), null, false, null, "", "KeyValuePair<String, Int32>", false, false, false);
                TestArgument(args, "Arg3", "Arg3", typeof(string), null, null, false, null, "", "String", false, false);
                TestArgument(args, "Arg7", "Arg7", typeof(bool), null, null, false, null, "", "Boolean", true, false, false, "Alias3");
                TestArgument(args, "Arg9", "Arg9", typeof(int?), null, null, false, null, "", "Int32", false, false);
            }
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
            Assert.AreEqual(2, target.Arguments.Count); // Constructor argument + one property argument.
            IEnumerator<CommandLineArgument> args = target.Arguments.GetEnumerator();
            TestArgument(args, "arg1", "arg1", typeof(string), null, 0, true, null, "", "String", false, false);
            TestArgument(args, "ThrowingArgument", "ThrowingArgument", typeof(int), null, null, false, null, "", "Int32", false, false);
        }

        [TestMethod]
        public void ParseTest()
        {
            Type argumentsType = typeof(TestArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });

            // Only required arguments
            TestParse(target, "val1 2 /arg6 val6", "val1", 2, arg6: "val6");
            // Make sure negative numbers are accepted, and not considered an argument name.
            TestParse(target, "val1 -2 /arg6 val6", "val1", -2, arg6: "val6");
            // All positional arguments except array
            TestParse(target, "val1 2 true 5.5 4 /arg6 arg6", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6");
            // All positional arguments including array
            TestParse(target, "val1 2 true 5.5 4 /arg6 arg6 Monday Tuesday", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6", arg8: new[] { DayOfWeek.Monday, DayOfWeek.Tuesday });
            // All positional arguments including array, which is specified by name first and then by position
            TestParse(target, "val1 2 true 5.5 4 /arg6 arg6 /arg8 Monday Tuesday", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6", arg8: new[] { DayOfWeek.Monday, DayOfWeek.Tuesday });
            // Some positional arguments using names, in order
            TestParse(target, "/arg1 val1 2 true /arg5 5.5 4 /arg6 arg6", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6");
            // Some position arguments using names, out of order (also uses : and - for one of them to mix things up)
            TestParse(target, "/other 2 val1 -arg5:5.5 true 4 /arg6 arg6", "val1", 2, true, arg4: 4, arg5: 5.5f, arg6: "arg6");
            // All arguments
            TestParse(target, "val1 2 true /arg3 val3 -other2:4 5.5 /arg6 val6 /arg7 /arg8 Monday /arg8 Tuesday /arg9 9 /arg10 /arg10 /arg10:false /arg11:false /arg12 12 /arg12 13 /arg13 foo=13 /arg13 bar=14 /arg14 hello=1 /arg14 bye=2 /arg15 something=5", "val1", 2, true, "val3", 4, 5.5f, "val6", true, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday }, 9, new[] { true, true, false }, false, new[] { 12, 13 }, new Dictionary<string,int>() { { "foo", 13 }, { "bar", 14 } }, new Dictionary<string,int>() { { "hello", 1 }, { "bye", 2 } }, new KeyValuePair<string,int>("something", 5));
            // Using aliases
            TestParse(target, "val1 2 /alias1 valalias6 /alias3", "val1", 2, arg6: "valalias6", arg7: true);
            // Long prefix cannot be used
            CheckThrows(() => target.Parse(new[] { "val1", "2", "--arg6", "val6" }), CommandLineArgumentErrorCategory.UnknownArgument, "-arg6");
            // Short name cannot be used
            CheckThrows(() => target.Parse(new[] { "val1", "2", "-arg6", "val6", "-a:5.5" }), CommandLineArgumentErrorCategory.UnknownArgument, "a");
        }

        [TestMethod]
        public void ParseTestEmptyArguments()
        {
            Type argumentsType = typeof(EmptyArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });

            // This test was added because version 2.0 threw an IndexOutOfRangeException when you tried to specify a positional argument when there were no positional arguments defined.
            CheckThrows(() => target.Parse(new[] { "Foo", "Bar" }), CommandLineArgumentErrorCategory.TooManyArguments);
        }

        [TestMethod]
        public void ParseTestTooManyArguments()
        {
            Type argumentsType = typeof(MultipleConstructorsArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });

            // Only accepts one positional argument.
            CheckThrows(() => target.Parse(new[] { "Foo", "Bar" }), CommandLineArgumentErrorCategory.TooManyArguments);
        }

        [TestMethod]
        public void ParseTestPropertySetterThrows()
        {
            Type argumentsType = typeof(MultipleConstructorsArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" });

            CheckThrows(() => target.Parse(new[] { "Foo", "-ThrowingArgument", "-5" }),
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
                CommandLineArgumentErrorCategory.UnknownArgument,
                "Argument1=test");

            target.NameValueSeparator = '=';
            args = (SimpleArguments)target.Parse(new[] { "-Argument1=test", "-Argument2=foo=bar" });
            Assert.IsNotNull(args);
            Assert.AreEqual("test", args.Argument1);
            Assert.AreEqual("foo=bar", args.Argument2);
            CheckThrows(() => target.Parse(new[] { "-Argument1:test" }),
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
                CommandLineArgumentErrorCategory.ArgumentValueConversion,
                "CustomSeparator",
                typeof(FormatException));

            // Inner exception is Argument exception because what throws here is trying to convert
            // ">bar" to int.
            CheckThrows(() => target.Parse(new[] { "-DefaultSeparator", "foo<=>bar" }),
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
                UsagePrefix = "Usage: test"
            };

            string actual = target.GetUsage(0, options);
            Assert.AreEqual(_expectedDefaultUsage, actual);
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

            options.UsageOptions.UsagePrefix = "Usage: test";

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
        }

        [TestMethod]
        public void TestCancelParsing()
        {
            var parser = new CommandLineParser(typeof(CancelArguments));

            // Don't cancel if -DoesCancel not specified.
            var result = (CancelArguments)parser.Parse(new[] { "-Argument1", "foo", "-DoesNotCancel", "-Argument2", "bar" });
            Assert.IsNotNull(result);
            Assert.IsTrue(result.DoesNotCancel);
            Assert.IsFalse(result.DoesCancel);
            Assert.AreEqual("foo", result.Argument1);
            Assert.AreEqual("bar", result.Argument2);

            // Cancel if -DoesCancel specified.
            result = (CancelArguments)parser.Parse(new[] { "-Argument1", "foo", "-DoesCancel", "-Argument2", "bar" });
            Assert.IsNull(result);
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
            Assert.IsFalse(result.DoesNotCancel);
            Assert.IsTrue(result.DoesCancel);
            Assert.AreEqual("foo", result.Argument1);
            Assert.AreEqual("bar", result.Argument2);
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

            // ParseOptions take precedence
            var options = new ParseOptions()
            {
                Mode = ParsingMode.Default,
                ArgumentNameComparer = StringComparer.OrdinalIgnoreCase,
                AllowWhiteSpaceValueSeparator = true,
                AllowDuplicateArguments = false,
                NameValueSeparator = ';',
                ArgumentNamePrefixes = new[] { "+" },
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
            var parser = new CommandLineParser(typeof(LongShortArguments));
            Assert.AreEqual(ParsingMode.LongShort, parser.Mode);
            Assert.AreEqual(CommandLineParser.DefaultLongArgumentNamePrefix, parser.LongArgumentNamePrefix);
            CollectionAssert.AreEqual(CommandLineParser.GetDefaultArgumentNamePrefixes(), parser.ArgumentNamePrefixes);
            Assert.AreSame(parser.GetArgument("foo"), parser.GetShortArgument('f'));
            Assert.AreSame(parser.GetArgument("arg2"), parser.GetShortArgument('a'));
            Assert.AreSame(parser.GetArgument("switch1"), parser.GetShortArgument('s'));
            Assert.AreSame(parser.GetArgument("switch2"), parser.GetShortArgument('t'));
            Assert.AreSame(parser.GetArgument("switch3"), parser.GetShortArgument('u'));
            Assert.AreEqual('f', parser.GetArgument("foo").ShortName);
            Assert.IsTrue(parser.GetArgument("foo").HasShortName);
            Assert.AreEqual('\0', parser.GetArgument("bar").ShortName);
            Assert.IsFalse(parser.GetArgument("bar").HasShortName);

            var result = (LongShortArguments)parser.Parse(new[] { "-f", "5", "--bar", "6", "-a", "7", "--arg1", "8", "-s" });
            Assert.AreEqual(5, result.Foo);
            Assert.AreEqual(6, result.Bar);
            Assert.AreEqual(7, result.Arg2);
            Assert.AreEqual(8, result.Arg1);
            Assert.IsTrue(result.Switch1);
            Assert.IsFalse(result.Switch2);
            Assert.IsFalse(result.Switch3);

            // Combine switches.
            result = (LongShortArguments)parser.Parse(new[] { "-su" });
            Assert.IsTrue(result.Switch1);
            Assert.IsFalse(result.Switch2);
            Assert.IsTrue(result.Switch3);

            // Combining non-switches is an error.
            CheckThrows(() => parser.Parse(new[] { "-sf" }), CommandLineArgumentErrorCategory.CombinedShortNameNonSwitch, "sf");

            // Can't use long argument prefix with short names.
            CheckThrows(() => parser.Parse(new[] { "--s" }), CommandLineArgumentErrorCategory.UnknownArgument, "s");

            // And vice versa.
            CheckThrows(() => parser.Parse(new[] { "-Switch1" }), CommandLineArgumentErrorCategory.UnknownArgument, "w");
        }

        private static void TestArgument(IEnumerator<CommandLineArgument> arguments, string name, string memberName, Type type, Type elementType, int? position, bool isRequired, object defaultValue, string description, string valueDescription, bool isSwitch, bool isMultiValue, bool isDictionary = false, params string[] aliases)
        {
            arguments.MoveNext();
            CommandLineArgument argument = arguments.Current;
            Assert.AreEqual(memberName, argument.MemberName);
            Assert.AreEqual(name, argument.ArgumentName);
            Assert.IsFalse(argument.HasShortName);
            Assert.AreEqual('\0', argument.ShortName);
            Assert.AreEqual(type, argument.ArgumentType);
            if( elementType == null )
                Assert.AreEqual(argument.ArgumentType, argument.ElementType);
            else
                Assert.AreEqual(elementType, argument.ElementType);
            Assert.AreEqual(position, argument.Position);
            Assert.AreEqual(isRequired, argument.IsRequired);
            Assert.AreEqual(description, argument.Description);
            Assert.AreEqual(valueDescription, argument.ValueDescription);
            Assert.AreEqual(isMultiValue, argument.IsMultiValue);
            Assert.AreEqual(isDictionary, argument.IsDictionary);
            Assert.AreEqual(isSwitch, argument.IsSwitch);
            Assert.AreEqual(defaultValue, argument.DefaultValue);
            Assert.IsNull(argument.Value);
            Assert.IsFalse(argument.HasValue);
            if( aliases == null || aliases.Length == 0 )
                Assert.IsNull(argument.Aliases);
            else
            {
                Assert.IsNotNull(argument.Aliases);
                CollectionAssert.AreEqual(aliases.ToArray(), argument.Aliases.ToArray());
            }
        }

        private static void TestParse(CommandLineParser target, string commandLine, string arg1 = null, int arg2 = 42, bool notSwitch = false, string arg3 = null, int arg4 = 47, float arg5 = 0.0f, string arg6 = null, bool arg7 = false, DayOfWeek[] arg8 = null, int? arg9 = null, bool[] arg10 = null, bool? arg11 = null, int[] arg12 = null, Dictionary<string, int> arg13 = null, Dictionary<string, int> arg14 = null, KeyValuePair<string, int>? arg15 = null)
        {
            string[] args = commandLine.Split(' '); // not using quoted arguments in the tests, so this is fine.
            TestArguments result = (TestArguments)target.Parse(args);
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

        private static void CheckThrows(Action operation, CommandLineArgumentErrorCategory category, string argumentName = null, Type innerExceptionType = null)
        {
            try
            {
                operation();
                Assert.Fail("Expected CommandLineException was not thrown.");
            }
            catch (CommandLineArgumentException ex)
            {
                Assert.AreEqual(category, ex.Category);
                Assert.AreEqual(argumentName, ex.ArgumentName);
                if (innerExceptionType == null)
                    Assert.IsNull(ex.InnerException);
                else
                    Assert.IsInstanceOfType(ex.InnerException, innerExceptionType);
            }
        }

        #region Expected usage

        private const string _expectedDefaultUsage = @"Test arguments description.

Usage: test [/arg1] <String> [[/other] <Number>] [[/notSwitch] <Boolean>] [[/Arg5] <Single>] [[/other2] <Number>] [[/Arg8] <DayOfWeek>...] /Arg6 <String> [/Arg10...] [/Arg11] [/Arg12 <Int32>...] [/Arg13 <String=Int32>...] [/Arg14 <String=Int32>...] [/Arg15 <KeyValuePair<String, Int32>>] [/Arg3 <String>] [/Arg7] [/Arg9 <Int32>]

    /arg1 <String>
        Arg1 description.

    /other <Number>
        Arg2 description. Default value: 42.

    /Arg5 <Single>
        Arg5 description.

    /other2 <Number>
        Arg4 description. Default value: 47.

    /Arg6 <String> (/Alias1, /Alias2)
        Arg6 description.

";

        #endregion

    }
}
