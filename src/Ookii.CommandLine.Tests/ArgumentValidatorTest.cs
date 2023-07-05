using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Validation;
using System;
using System.Text.RegularExpressions;

namespace Ookii.CommandLine.Tests;

/// <summary>
/// Independent tests of argument validators without having to go through parsing.
/// </summary>
[TestClass]
public class ArgumentValidatorTest
{
    [TestMethod]
    public void TestValidateRange()
    {
        var argument = GetArgument();
        var validator = new ValidateRangeAttribute(0, 10);
        Assert.IsTrue(validator.IsValid(argument, 0));
        Assert.IsTrue(validator.IsValid(argument, 5));
        Assert.IsTrue(validator.IsValid(argument, 10));
        Assert.IsFalse(validator.IsValid(argument, -1));
        Assert.IsFalse(validator.IsValid(argument, 11));
        Assert.IsFalse(validator.IsValid(argument, null));

        validator = new ValidateRangeAttribute(null, 10);
        Assert.IsTrue(validator.IsValid(argument, 0));
        Assert.IsTrue(validator.IsValid(argument, 5));
        Assert.IsTrue(validator.IsValid(argument, 10));
        Assert.IsTrue(validator.IsValid(argument, int.MinValue));
        Assert.IsFalse(validator.IsValid(argument, 11));
        Assert.IsTrue(validator.IsValid(argument, null));

        validator = new ValidateRangeAttribute(10, null);
        Assert.IsTrue(validator.IsValid(argument, 10));
        Assert.IsTrue(validator.IsValid(argument, int.MaxValue));
        Assert.IsFalse(validator.IsValid(argument, 9));
        Assert.IsFalse(validator.IsValid(argument, null));
    }

    [TestMethod]
    public void TestValidateNotNull()
    {
        var argument = GetArgument();
        var validator = new ValidateNotNullAttribute();
        Assert.IsTrue(validator.IsValid(argument, 1));
        Assert.IsTrue(validator.IsValid(argument, "hello"));
        Assert.IsFalse(validator.IsValid(argument, null));
    }

    [TestMethod]
    public void TestValidateNotNullOrEmpty()
    {
        var argument = GetArgument();
        var validator = new ValidateNotEmptyAttribute();
        Assert.IsTrue(validator.IsValid(argument, "hello"));
        Assert.IsTrue(validator.IsValid(argument, " "));
        Assert.IsFalse(validator.IsValid(argument, null));
        Assert.IsFalse(validator.IsValid(argument, ""));
    }

    [TestMethod]
    public void TestValidateNotNullOrWhiteSpace()
    {
        var argument = GetArgument();
        var validator = new ValidateNotWhiteSpaceAttribute();
        Assert.IsTrue(validator.IsValid(argument, "hello"));
        Assert.IsFalse(validator.IsValid(argument, " "));
        Assert.IsFalse(validator.IsValid(argument, null));
        Assert.IsFalse(validator.IsValid(argument, ""));
    }

    [TestMethod]
    public void TestValidateStringLength()
    {
        var argument = GetArgument();
        var validator = new ValidateStringLengthAttribute(2, 5);
        Assert.IsTrue(validator.IsValid(argument, "ab"));
        Assert.IsTrue(validator.IsValid(argument, "abcde"));
        Assert.IsFalse(validator.IsValid(argument, "a"));
        Assert.IsFalse(validator.IsValid(argument, "abcdef"));
        Assert.IsFalse(validator.IsValid(argument, ""));
        Assert.IsFalse(validator.IsValid(argument, null));

        validator = new ValidateStringLengthAttribute(0, 5);
        Assert.IsTrue(validator.IsValid(argument, ""));
        Assert.IsTrue(validator.IsValid(argument, null));
    }

    [TestMethod]
    public void ValidatePatternAttribute()
    {
        var argument = GetArgument();

        // Partial match.
        var validator = new ValidatePatternAttribute("[a-z]+");
        Assert.IsTrue(validator.IsValid(argument, "abc"));
        Assert.IsTrue(validator.IsValid(argument, "0cde2"));
        Assert.IsFalse(validator.IsValid(argument, "02"));
        Assert.IsFalse(validator.IsValid(argument, "ABCD"));
        Assert.IsFalse(validator.IsValid(argument, ""));
        Assert.IsFalse(validator.IsValid(argument, null));

        // Exact match.
        validator = new ValidatePatternAttribute("^[a-z]+$");
        Assert.IsTrue(validator.IsValid(argument, "abc"));
        Assert.IsFalse(validator.IsValid(argument, "0cde2"));
        Assert.IsFalse(validator.IsValid(argument, "02"));
        Assert.IsFalse(validator.IsValid(argument, "ABCD"));
        Assert.IsFalse(validator.IsValid(argument, ""));
        Assert.IsFalse(validator.IsValid(argument, null));

        // Options
        validator = new ValidatePatternAttribute("^[a-z]+$", RegexOptions.IgnoreCase);
        Assert.IsTrue(validator.IsValid(argument, "abc"));
        Assert.IsFalse(validator.IsValid(argument, "0cde2"));
        Assert.IsFalse(validator.IsValid(argument, "02"));
        Assert.IsTrue(validator.IsValid(argument, "ABCD"));
        Assert.IsFalse(validator.IsValid(argument, ""));
        Assert.IsFalse(validator.IsValid(argument, null));

        Assert.AreEqual("The value for the argument 'Arg3' is not valid.", validator.GetErrorMessage(argument, "foo"));
        validator.ErrorMessage = "Name {0}, value {1}, pattern {2}";
        Assert.AreEqual("Name Arg3, value foo, pattern ^[a-z]+$", validator.GetErrorMessage(argument, "foo"));
    }

    [TestMethod]
    public void TestValidateEnumValue()
    {
        var parser = new CommandLineParser<ValidationArguments>();
        var validator = new ValidateEnumValueAttribute();
        var argument = parser.GetArgument("Day")!;
        Assert.IsTrue(validator.IsValid(argument, DayOfWeek.Sunday));
        Assert.IsTrue(validator.IsValid(argument, DayOfWeek.Saturday));
        Assert.IsTrue(validator.IsValid(argument, null));
        Assert.IsFalse(validator.IsValid(argument, (DayOfWeek)9));

        argument = parser.GetArgument("Day2")!;
        Assert.IsTrue(validator.IsValid(argument, (DayOfWeek?)DayOfWeek.Sunday));
        Assert.IsTrue(validator.IsValid(argument, (DayOfWeek?)DayOfWeek.Saturday));
        Assert.IsTrue(validator.IsValid(argument, null));
        Assert.IsFalse(validator.IsValid(argument, (DayOfWeek?)9));
    }

    private static CommandLineArgument GetArgument()
    {
        // Just so we have a CommandLineArgument instance to pass. None of the built-in
        // validators use that for anything other than the name and type.
        var parser = ValidationArguments.CreateParser();
        var arg = parser.GetArgument("Arg3");
        Assert.IsNotNull(arg);
        return arg;
    }
}
