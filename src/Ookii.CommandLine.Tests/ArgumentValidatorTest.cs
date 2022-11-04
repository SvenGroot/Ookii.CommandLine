using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Validation;
using System.Text.RegularExpressions;

namespace Ookii.CommandLine.Tests
{
    /// <summary>
    /// Independent tests of argument validators without having to go through parsing.
    /// </summary>
    [TestClass]
    public class ArgumentValidatorTest
    {
        CommandLineArgument _argument;

        [TestInitialize]
        public void Initialize()
        {
            // Just so we have a CommandLineArgument instance to pass. None of the built-in
            // validators use that for anything other than the name and type.
            var parser = new CommandLineParser<ValidationArguments>();
            _argument = parser.GetArgument("Arg3");
        }

        [TestMethod]
        public void TestValidateRange()
        {
            var validator = new ValidateRangeAttribute(0, 10);
            Assert.IsTrue(validator.IsValid(_argument, 0));
            Assert.IsTrue(validator.IsValid(_argument, 5));
            Assert.IsTrue(validator.IsValid(_argument, 10));
            Assert.IsFalse(validator.IsValid(_argument, -1));
            Assert.IsFalse(validator.IsValid(_argument, 11));
            Assert.IsFalse(validator.IsValid(_argument, null));

            validator = new ValidateRangeAttribute(null, 10);
            Assert.IsTrue(validator.IsValid(_argument, 0));
            Assert.IsTrue(validator.IsValid(_argument, 5));
            Assert.IsTrue(validator.IsValid(_argument, 10));
            Assert.IsTrue(validator.IsValid(_argument, int.MinValue));
            Assert.IsFalse(validator.IsValid(_argument, 11));
            Assert.IsTrue(validator.IsValid(_argument, null));

            validator = new ValidateRangeAttribute(10, null);
            Assert.IsTrue(validator.IsValid(_argument, 10));
            Assert.IsTrue(validator.IsValid(_argument, int.MaxValue));
            Assert.IsFalse(validator.IsValid(_argument, 9));
            Assert.IsFalse(validator.IsValid(_argument, null));
        }

        [TestMethod]
        public void TestValidateNotNull()
        {
            var validator = new ValidateNotNullAttribute();
            Assert.IsTrue(validator.IsValid(_argument, 1));
            Assert.IsTrue(validator.IsValid(_argument, "hello"));
            Assert.IsFalse(validator.IsValid(_argument, null));
        }

        [TestMethod]
        public void TestValidateNotNullOrEmpty()
        {
            var validator = new ValidateNotNullOrEmptyAttribute();
            Assert.IsTrue(validator.IsValid(_argument, "hello"));
            Assert.IsTrue(validator.IsValid(_argument, " "));
            Assert.IsFalse(validator.IsValid(_argument, null));
            Assert.IsFalse(validator.IsValid(_argument, ""));
        }

        [TestMethod]
        public void TestValidateNotNullOrWhiteSpace()
        {
            var validator = new ValidateNotNullOrWhiteSpaceAttribute();
            Assert.IsTrue(validator.IsValid(_argument, "hello"));
            Assert.IsFalse(validator.IsValid(_argument, " "));
            Assert.IsFalse(validator.IsValid(_argument, null));
            Assert.IsFalse(validator.IsValid(_argument, ""));
        }

        [TestMethod]
        public void TestValidateStringLength()
        {
            var validator = new ValidateStringLengthAttribute(2, 5);
            Assert.IsTrue(validator.IsValid(_argument, "ab"));
            Assert.IsTrue(validator.IsValid(_argument, "abcde"));
            Assert.IsFalse(validator.IsValid(_argument, "a"));
            Assert.IsFalse(validator.IsValid(_argument, "abcdef"));
            Assert.IsFalse(validator.IsValid(_argument, ""));
            Assert.IsFalse(validator.IsValid(_argument, null));

            validator = new ValidateStringLengthAttribute(0, 5);
            Assert.IsTrue(validator.IsValid(_argument, ""));
            Assert.IsTrue(validator.IsValid(_argument, null));
        }

        [TestMethod]
        public void ValidatePatternAttribute()
        {
            // Partial match.
            var validator = new ValidatePatternAttribute("[a-z]+");
            Assert.IsTrue(validator.IsValid(_argument, "abc"));
            Assert.IsTrue(validator.IsValid(_argument, "0cde2"));
            Assert.IsFalse(validator.IsValid(_argument, "02"));
            Assert.IsFalse(validator.IsValid(_argument, "ABCD"));
            Assert.IsFalse(validator.IsValid(_argument, ""));
            Assert.IsFalse(validator.IsValid(_argument, null));

            // Exact match.
            validator = new ValidatePatternAttribute("^[a-z]+$");
            Assert.IsTrue(validator.IsValid(_argument, "abc"));
            Assert.IsFalse(validator.IsValid(_argument, "0cde2"));
            Assert.IsFalse(validator.IsValid(_argument, "02"));
            Assert.IsFalse(validator.IsValid(_argument, "ABCD"));
            Assert.IsFalse(validator.IsValid(_argument, ""));
            Assert.IsFalse(validator.IsValid(_argument, null));

            // Options
            validator = new ValidatePatternAttribute("^[a-z]+$", RegexOptions.IgnoreCase);
            Assert.IsTrue(validator.IsValid(_argument, "abc"));
            Assert.IsFalse(validator.IsValid(_argument, "0cde2"));
            Assert.IsFalse(validator.IsValid(_argument, "02"));
            Assert.IsTrue(validator.IsValid(_argument, "ABCD"));
            Assert.IsFalse(validator.IsValid(_argument, ""));
            Assert.IsFalse(validator.IsValid(_argument, null));
        }
    }
}
