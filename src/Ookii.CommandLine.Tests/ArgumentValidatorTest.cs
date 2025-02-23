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
        Assert.IsTrue(validator.IsValidPostConversion(argument, 0));
        Assert.IsTrue(validator.IsValidPostConversion(argument, 5));
        Assert.IsTrue(validator.IsValidPostConversion(argument, 10));
        Assert.IsFalse(validator.IsValidPostConversion(argument, -1));
        Assert.IsFalse(validator.IsValidPostConversion(argument, 11));
        Assert.IsFalse(validator.IsValidPostConversion(argument, null));

        validator = new ValidateRangeAttribute(null, 10);
        Assert.IsTrue(validator.IsValidPostConversion(argument, 0));
        Assert.IsTrue(validator.IsValidPostConversion(argument, 5));
        Assert.IsTrue(validator.IsValidPostConversion(argument, 10));
        Assert.IsTrue(validator.IsValidPostConversion(argument, int.MinValue));
        Assert.IsFalse(validator.IsValidPostConversion(argument, 11));
        Assert.IsTrue(validator.IsValidPostConversion(argument, null));

        validator = new ValidateRangeAttribute(10, null);
        Assert.IsTrue(validator.IsValidPostConversion(argument, 10));
        Assert.IsTrue(validator.IsValidPostConversion(argument, int.MaxValue));
        Assert.IsFalse(validator.IsValidPostConversion(argument, 9));
        Assert.IsFalse(validator.IsValidPostConversion(argument, null));
    }

    [TestMethod]
    public void TestValidateNotNull()
    {
        var argument = GetArgument();
        var validator = new ValidateNotNullAttribute();
        Assert.IsTrue(validator.IsValidPostConversion(argument, 1));
        Assert.IsTrue(validator.IsValidPostConversion(argument, "hello"));
        Assert.IsFalse(validator.IsValidPostConversion(argument, null));
    }

    [TestMethod]
    public void TestValidateNotNullOrEmpty()
    {
        var argument = GetArgument();
        var validator = new ValidateNotEmptyAttribute();
        Assert.IsTrue(validator.IsValidPreConversion(argument, "hello".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, " ".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "".AsMemory()));
    }

    [TestMethod]
    public void TestValidateNotNullOrWhiteSpace()
    {
        var argument = GetArgument();
        var validator = new ValidateNotWhiteSpaceAttribute();
        Assert.IsTrue(validator.IsValidPreConversion(argument, "hello".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, " ".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "".AsMemory()));
    }

    [TestMethod]
    public void TestValidateStringLength()
    {
        var argument = GetArgument();
        var validator = new ValidateStringLengthAttribute(2, 5);
        Assert.IsTrue(validator.IsValidPreConversion(argument, "ab".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, "abcde".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "a".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "abcdef".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "".AsMemory()));

        validator = new ValidateStringLengthAttribute(0, 5);
        Assert.IsTrue(validator.IsValidPreConversion(argument, "".AsMemory()));
    }

    [TestMethod]
    public void ValidatePatternAttribute()
    {
        var argument = GetArgument();

        // Partial match.
        var validator = new ValidatePatternAttribute("[a-z]+");
        Assert.IsTrue(validator.IsValidPreConversion(argument, "abc".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, "0cde2".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "02".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "ABCD".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, null));

        // Exact match.
        validator = new ValidatePatternAttribute("^[a-z]+$");
        Assert.IsTrue(validator.IsValidPreConversion(argument, "abc".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "0cde2".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "02".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "ABCD".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, null));

        // Options
        validator = new ValidatePatternAttribute("^[a-z]+$", RegexOptions.IgnoreCase);
        Assert.IsTrue(validator.IsValidPreConversion(argument, "abc".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "0cde2".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "02".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, "ABCD".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, null));

        Assert.AreEqual("The value for the argument 'Arg3' is not valid.", validator.GetErrorMessage(argument, "foo"));
        validator.ErrorMessage = "Name {0}, value {1}, pattern {2}";
        Assert.AreEqual("Name Arg3, value foo, pattern ^[a-z]+$", validator.GetErrorMessage(argument, "foo"));
    }

    [TestMethod]
    public void TestValidateEnumValue()
    {
        var parser = new CommandLineParser<ValidationArguments>();
        var validator = new ValidateEnumValueAttribute();
        Assert.AreEqual(validator.AllowCommaSeparatedValues, TriState.Auto);
        Assert.AreEqual(validator.AllowNonDefinedValues, TriState.Auto);
        Assert.AreEqual(validator.AllowNumericValues, false);
        Assert.AreEqual(validator.CaseSensitive, false);

        var argument = parser.GetArgument("Day")!;
        Assert.IsTrue(validator.IsValidPostConversion(argument, DayOfWeek.Sunday));
        Assert.IsTrue(validator.IsValidPostConversion(argument, DayOfWeek.Saturday));
        Assert.IsTrue(validator.IsValidPostConversion(argument, null));
        Assert.IsFalse(validator.IsValidPostConversion(argument, (DayOfWeek)9));
        validator.AllowNonDefinedValues = TriState.True;
        Assert.IsTrue(validator.IsValidPostConversion(argument, (DayOfWeek)9));
        validator.AllowNonDefinedValues = TriState.False;
        Assert.IsFalse(validator.IsValidPostConversion(argument, (DayOfWeek)9));

        Assert.IsTrue(validator.IsValidPreConversion(argument, "Sunday".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, " Sunday ".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, "".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "Monday,Tuesday".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "1".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "-2".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, " 3 ".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, " -4 ".AsMemory()));
        validator.AllowCommaSeparatedValues = TriState.True;
        Assert.IsTrue(validator.IsValidPreConversion(argument, "Monday,Tuesday".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "Monday,1,Tuesday".AsMemory()));
        Assert.IsFalse(validator.IsValidPreConversion(argument, "Monday, 2 ,Tuesday".AsMemory()));
        validator.AllowNumericValues = true;
        Assert.IsTrue(validator.IsValidPreConversion(argument, "1".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, "-2".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, " -2 ".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, "Monday,1,Tuesday".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, "Monday, 2 ,Tuesday".AsMemory()));
        validator.AllowCommaSeparatedValues = TriState.False;
        Assert.IsFalse(validator.IsValidPreConversion(argument, "Monday,Tuesday".AsMemory()));

        // Using a nullable type.
        argument = parser.GetArgument("Day2")!;
        validator.AllowNonDefinedValues = TriState.Auto;
        Assert.IsTrue(validator.IsValidPostConversion(argument, (DayOfWeek?)DayOfWeek.Sunday));
        Assert.IsTrue(validator.IsValidPostConversion(argument, (DayOfWeek?)DayOfWeek.Saturday));
        Assert.IsTrue(validator.IsValidPostConversion(argument, null));
        Assert.IsFalse(validator.IsValidPostConversion(argument, (DayOfWeek?)9));

        // Allow commas and non-defined values based on flags attribute.
        argument = parser.GetArgument("Modifiers")!;
        validator.AllowCommaSeparatedValues = TriState.Auto;
        Assert.IsTrue(validator.IsValidPreConversion(argument, "Alt".AsMemory()));
        Assert.IsTrue(validator.IsValidPreConversion(argument, "Alt,Control".AsMemory()));
        Assert.IsTrue(validator.IsValidPostConversion(argument, ConsoleModifiers.Alt | ConsoleModifiers.Control));
        validator.AllowCommaSeparatedValues = TriState.False;
        Assert.IsFalse(validator.IsValidPreConversion(argument, "Alt,Control".AsMemory()));
        validator.AllowNonDefinedValues = TriState.False;
        Assert.IsFalse(validator.IsValidPostConversion(argument, ConsoleModifiers.Alt | ConsoleModifiers.Control));
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
