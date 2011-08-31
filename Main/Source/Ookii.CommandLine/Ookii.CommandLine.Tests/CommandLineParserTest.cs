// Copyright (c) Sven Groot (Ookii.org) 2011
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

            [CommandLineArgument(IsRequired = true), System.ComponentModel.Description("Arg6 description.")]
            public string Arg6 { get; set; }

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

            [CommandLineArgument]
            public Collection<int> Arg12
            {
                get { return _arg12; }
            }

            public string NotAnArg { get; set; }

            [CommandLineArgument()]
            private string NotAnArg2 { get; set; }

            [CommandLineArgument()]
            public static string NotAnArg3 { get; set; }
        }

        class MultipleConstructorsArguments
        {
            public MultipleConstructorsArguments() { }
            public MultipleConstructorsArguments(string notArg1, int notArg2) { }
            [CommandLineConstructor]
            public MultipleConstructorsArguments(string arg1) { }
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


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
            Assert.AreEqual(13, target.Arguments.Count);
            using( IEnumerator<CommandLineArgument> args = target.Arguments.GetEnumerator() )
            {
                TestArgument(args, "arg1", "arg1", typeof(string), null, 0, true, null, "Arg1 description.", "String", false, false);
                TestArgument(args, "Arg10", "Arg10", typeof(bool[]), typeof(bool), null, false, null, "", "Boolean", true, true);
                TestArgument(args, "Arg11", "Arg11", typeof(bool?), null, null, false, null, "", "Boolean", true, false);
                TestArgument(args, "Arg12", "Arg12", typeof(Collection<int>), typeof(int), null, false, null, "", "Int32", false, true);
                TestArgument(args, "Arg3", "Arg3", typeof(string), null, null, false, null, "", "String", false, false);
                TestArgument(args, "Arg5", "Arg5", typeof(float), null, 3, false, null, "Arg5 description.", "Single", false, false);
                TestArgument(args, "Arg6", "Arg6", typeof(string), null, null, true, null, "Arg6 description.", "String", false, false);
                TestArgument(args, "Arg7", "Arg7", typeof(bool), null, null, false, null, "", "Boolean", true, false);
                TestArgument(args, "Arg8", "Arg8", typeof(DayOfWeek[]), typeof(DayOfWeek), 5, false, null, "", "DayOfWeek", false, true);
                TestArgument(args, "Arg9", "Arg9", typeof(int?), null, null, false, null, "", "Int32", false, false);
                TestArgument(args, "notSwitch", "notSwitch", typeof(bool), null, 2, false, false, "", "Boolean", false, false);
                TestArgument(args, "other", "arg2", typeof(int), null, 1, false, 42, "Arg2 description.", "Number", false, false);
                TestArgument(args, "other2", "Arg4", typeof(int), null, 4, false, 47, "Arg4 description.", "Number", false, false);
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
            Assert.AreEqual(1, target.Arguments.Count);
            IEnumerator<CommandLineArgument> args = target.Arguments.GetEnumerator();
            TestArgument(args, "arg1", "arg1", typeof(string), null, 0, true, null, "", "String", false, false);

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
            TestParse(target, "val1 2 true /arg3 val3 -other2:4 5.5 /arg6 val6 /arg7 /arg8 Monday /arg8 Tuesday /arg9 9 /arg10 /arg10 /arg10:false /arg11:false", "val1", 2, true, "val3", 4, 5.5f, "val6", true, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday }, 9, new[] { true, true, false }, false);
        }

        private static void TestArgument(IEnumerator<CommandLineArgument> arguments, string name, string memberName, Type type, Type elementType, int? position, bool isRequired, object defaultValue, string description, string valueDescription, bool isSwitch, bool isMultiValue)
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
            Assert.AreEqual(isSwitch, argument.IsSwitch);
            Assert.AreEqual(defaultValue, argument.DefaultValue);
            Assert.AreEqual(null, argument.Value);
            Assert.AreEqual(false, argument.HasValue);
        }

        private static void TestParse(CommandLineParser target, string commandLine, string arg1 = null, int arg2 = 42, bool notSwitch = false, string arg3 = null, int arg4 = 47, float arg5 = 0.0f, string arg6 = null, bool arg7 = false, DayOfWeek[] arg8 = null, int? arg9 = null, bool[] arg10 = null, bool? arg11 = null)
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

        }
    }
}
