// Copyright (c) Sven Groot (Ookii.org) 2012
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at http://ookiicommandline.codeplex.com. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
//
// I apologize: unit tests for Ookii.CommandLine are not entirely comprehensive.
// The most important functionality is covered, however. Improved unit tests are on the to do list.
using Ookii.CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Ookii.CommandLine.Tests
{


    /// <summary>
    ///This is a test class for CommandLineParserTest and is intended
    ///to contain all CommandLineParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommandLineParserTest
    {
        #region Nested types

        class EmptyArguments
        {
        }

        [System.ComponentModel.Description("Test arguments description.")]
        class TestArguments
        {
            private readonly Collection<int> _arg12 = new Collection<int>();
            private readonly Dictionary<string, int> _arg14 = new Dictionary<string, int>();

            private TestArguments(string notAnArg)
            {
            }

            public TestArguments([System.ComponentModel.Description("Arg1 description.")] string arg1, [System.ComponentModel.Description("Arg2 description."), ArgumentName("other"), ValueDescription("Number")] int arg2 = 42, bool notSwitch = false)
            {
                Arg1 = arg1;
                Arg2 = arg2;
                NotSwitch = notSwitch;
            }

            public string Arg1 { get; private set; }

            public int Arg2 { get; private set; }

            public bool NotSwitch { get; private set; }

            [CommandLineArgument()]
            public string Arg3 { get; set; }

            // Default value is intentionally a string to test default value conversion.
            [CommandLineArgument("other2", DefaultValue = "47", ValueDescription = "Number", Position = 1), System.ComponentModel.Description("Arg4 description.")]
            public int Arg4 { get; set; }

            [CommandLineArgument(Position = 0), System.ComponentModel.Description("Arg5 description.")]
            public float Arg5 { get; set; }

            [Alias("Alias1")]
            [Alias("Alias2")]
            [CommandLineArgument(IsRequired = true), System.ComponentModel.Description("Arg6 description.")]
            public string Arg6 { get; set; }

            [Alias("Alias3")]
            [CommandLineArgument()]
            public bool Arg7 { get; set; }

            [CommandLineArgument(Position=2)]
            public DayOfWeek[] Arg8 { get; set; }

            [CommandLineArgument()]
            public int? Arg9 { get; set; }

            [CommandLineArgument]
            public bool[] Arg10 { get; set; }

            [CommandLineArgument]
            public bool? Arg11 { get; set; }

            [CommandLineArgument(DefaultValue=42)] // Default value is ignored for collection types.
            public Collection<int> Arg12
            {
                get { return _arg12; }
            }

            [CommandLineArgument]
            public Dictionary<string, int> Arg13 { get; set; }

            [CommandLineArgument]
            public IDictionary<string, int> Arg14
            {
                get { return _arg14; }
            }

            [CommandLineArgument, TypeConverter(typeof(KeyValuePairConverter<string, int>))]
            public KeyValuePair<string, int> Arg15 { get; set; }

            public string NotAnArg { get; set; }

            [CommandLineArgument()]
            private string NotAnArg2 { get; set; }

            [CommandLineArgument()]
            public static string NotAnArg3 { get; set; }
        }

        class MultipleConstructorsArguments
        {
            private int _throwingArgument;

            public MultipleConstructorsArguments() { }
            public MultipleConstructorsArguments(string notArg1, int notArg2) { }
            [CommandLineConstructor]
            public MultipleConstructorsArguments(string arg1)
            {
                if( arg1 == "invalid" )
                    throw new ArgumentException("Invalid argument value.", "arg1");
            }

            [CommandLineArgument]
            public int ThrowingArgument
            {
                get { return _throwingArgument; }
                set
                {
                    if( value < 0 )
                        throw new ArgumentOutOfRangeException("value");
                    _throwingArgument = value;
                }
            }

        }

        class DictionaryArguments
        {
            [CommandLineArgument]
            public Dictionary<string, int> NoDuplicateKeys { get; set; }
            [CommandLineArgument, AllowDuplicateDictionaryKeys]
            public Dictionary<string, int> DuplicateKeys { get; set; }
        }

        class MultiValueSeparatorArguments
        {
            [CommandLineArgument]
            public string[] NoSeparator { get; set; }
            [CommandLineArgument, MultiValueSeparator(",")]
            public string[] Separator { get; set; }
        }

        #endregion

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        /// <summary>
        ///A test for CommandLineParser Constructor
        ///</summary>
        [TestMethod()]
        public void ConstructorEmptyArgumentsTest()
        {
            Type argumentsType = typeof(EmptyArguments);
            CommandLineParser target = new CommandLineParser(argumentsType);
            Assert.AreEqual(CultureInfo.CurrentCulture, target.Culture);
            Assert.AreEqual(false, target.AllowDuplicateArguments);
            Assert.AreEqual(true, target.AllowWhiteSpaceValueSeparator);
            CollectionAssert.AreEqual(CommandLineParser.DefaultArgumentNamePrefixes.ToArray(), target.ArgumentNamePrefixes);
            Assert.AreEqual(argumentsType, target.ArgumentsType);
            Assert.AreEqual(string.Empty, target.Description);
            Assert.AreEqual(0, target.Arguments.Count);
        }

        [TestMethod()]
        public void ConstructorTest()
        {
            Type argumentsType = typeof(TestArguments);
            CommandLineParser target = new CommandLineParser(argumentsType);
            Assert.AreEqual(CultureInfo.CurrentCulture, target.Culture);
            Assert.AreEqual(false, target.AllowDuplicateArguments);
            Assert.AreEqual(true, target.AllowWhiteSpaceValueSeparator);
            CollectionAssert.AreEqual(CommandLineParser.DefaultArgumentNamePrefixes.ToArray(), target.ArgumentNamePrefixes);
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
            Assert.AreEqual(CultureInfo.CurrentCulture, target.Culture);
            Assert.AreEqual(false, target.AllowDuplicateArguments);
            Assert.AreEqual(true, target.AllowWhiteSpaceValueSeparator);
            CollectionAssert.AreEqual(CommandLineParser.DefaultArgumentNamePrefixes.ToArray(), target.ArgumentNamePrefixes);
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
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" }) { Culture = CultureInfo.InvariantCulture };

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
        }

        [TestMethod]
        public void ParseTestEmptyArguments()
        {
            Type argumentsType = typeof(EmptyArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" }) { Culture = CultureInfo.InvariantCulture };

            // This test was added because version 2.0 threw an IndexOutOfRangeException when you tried to specify a positional argument when there were no positional arguments defined.
            try
            {
                target.Parse(new[] { "Foo", "Bar" });
                Assert.Fail("Expected CommandLineArgumentException.");
            }
            catch( CommandLineArgumentException ex )
            {
                Assert.AreEqual(CommandLineArgumentErrorCategory.TooManyArguments, ex.Category);
            }
        }

        [TestMethod]
        public void ParseTestTooManyArguments()
        {
            Type argumentsType = typeof(MultipleConstructorsArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" }) { Culture = CultureInfo.InvariantCulture };

            try
            {
                // Only accepts one positional argument.
                target.Parse(new[] { "Foo", "Bar" });
                Assert.Fail("Expected CommandLineArgumentException.");
            }
            catch( CommandLineArgumentException ex )
            {
                Assert.AreEqual(CommandLineArgumentErrorCategory.TooManyArguments, ex.Category);
            }
        }

        [TestMethod]
        public void ParseTestPropertySetterThrows()
        {
            Type argumentsType = typeof(MultipleConstructorsArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" }) { Culture = CultureInfo.InvariantCulture };

            try
            {
                target.Parse(new[] { "Foo", "-ThrowingArgument", "-5" });
                Assert.Fail("Expected CommandLineArgumentException.");
            }
            catch( CommandLineArgumentException ex )
            {
                Assert.AreEqual(CommandLineArgumentErrorCategory.ApplyValueError, ex.Category);
                Assert.AreEqual("ThrowingArgument", ex.ArgumentName);
                Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentOutOfRangeException));
            }
        }

        [TestMethod]
        public void ParseTestConstructorThrows()
        {
            Type argumentsType = typeof(MultipleConstructorsArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" }) { Culture = CultureInfo.InvariantCulture };

            try
            {
                target.Parse(new[] { "invalid" });
                Assert.Fail("Expected CommandLineArgumentException.");
            }
            catch( CommandLineArgumentException ex )
            {
                Assert.AreEqual(CommandLineArgumentErrorCategory.CreateArgumentsTypeError, ex.Category);
                Assert.IsNull(ex.ArgumentName);
                Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentException));
            }
        }

        [TestMethod]
        public void ParseTestDuplicateDictionaryKeys()
        {
            Type argumentsType = typeof(DictionaryArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" }) { Culture = CultureInfo.InvariantCulture };

            DictionaryArguments args = (DictionaryArguments)target.Parse(new[] { "-DuplicateKeys", "Foo=1", "-DuplicateKeys", "Bar=2", "-DuplicateKeys", "Foo=3" });
            Assert.IsNotNull(args);
            Assert.AreEqual(2, args.DuplicateKeys.Count);
            Assert.AreEqual(3, args.DuplicateKeys["Foo"]);
            Assert.AreEqual(2, args.DuplicateKeys["Bar"]);
            try
            {
                target.Parse(new[] { "-NoDuplicateKeys", "Foo=1", "-NoDuplicateKeys", "Bar=2", "-NoDuplicateKeys", "Foo=3" });
            }
            catch( CommandLineArgumentException ex )
            {
                Assert.AreEqual(CommandLineArgumentErrorCategory.InvalidDictionaryValue, ex.Category);
                Assert.AreEqual("NoDuplicateKeys", ex.ArgumentName);
                Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentException));
            }
        }

        [TestMethod]
        public void ParseTestMultiValueSeparator()
        {
            Type argumentsType = typeof(MultiValueSeparatorArguments);
            CommandLineParser target = new CommandLineParser(argumentsType, new[] { "/", "-" }) { Culture = CultureInfo.InvariantCulture };

            MultiValueSeparatorArguments args = (MultiValueSeparatorArguments)target.Parse(new[] { "-NoSeparator", "Value1,Value2", "-NoSeparator", "Value3", "-Separator", "Value1,Value2", "-Separator", "Value3" });
            Assert.IsNotNull(args);
            CollectionAssert.AreEqual(new[] { "Value1,Value2", "Value3" }, args.NoSeparator);
            CollectionAssert.AreEqual(new[] { "Value1", "Value2", "Value3" }, args.Separator);
        }

        [TestMethod]
        public void WriteUsage_Test_AliasesInCommandLine()
        {
            var argumentsType = typeof(TestArguments);
            var parser = new CommandLineParser(argumentsType);

            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb))
            {
                parser.WriteUsage(
                    tw,
                    int.MaxValue,
                    new WriteUsageOptions
                    {
                        IncludeAliasInDescription = true,
                        IncludeDefaultValueInDescription = true,
                        IncludeAliasInCommandLine = true
                    });
            }

            var usage = sb.ToString();

            Assert.IsTrue(usage.Contains("-Arg6"));
            Assert.IsTrue(usage.Contains("-Arg6|-Alias1|-Alias2"));
        }

        [TestMethod]
        public void WriteUsage_Test_AliasesInCommandLine_Disabled()
        {
            var argumentsType = typeof(TestArguments);
            var parser = new CommandLineParser(argumentsType);

            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb))
            {
                parser.WriteUsage(
                    tw,
                    int.MaxValue,
                    new WriteUsageOptions
                    {
                        IncludeAliasInDescription = true,
                        IncludeDefaultValueInDescription = true,
                        IncludeAliasInCommandLine = false
                    });
            }

            var usage = sb.ToString();

            Assert.IsTrue(usage.Contains("-Arg6"));
            Assert.IsFalse(usage.Contains("-Arg6|-Alias1|-Alias2"));
        }

        private static void TestArgument(IEnumerator<CommandLineArgument> arguments, string name, string memberName, Type type, Type elementType, int? position, bool isRequired, object defaultValue, string description, string valueDescription, bool isSwitch, bool isMultiValue, bool isDictionary = false, params string[] aliases)
        {
            arguments.MoveNext();
            CommandLineArgument argument = arguments.Current;
            Assert.AreEqual(memberName, argument.MemberName);
            Assert.AreEqual(name, argument.ArgumentName);
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
            Assert.AreEqual(null, argument.Value);
            Assert.AreEqual(false, argument.HasValue);
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
    }
}
