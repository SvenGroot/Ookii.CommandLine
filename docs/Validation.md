# Argument validation and dependencies

It's often necessary to validate the value of an argument beyond just its type. For example, you may
wish to make sure a string argument is not empty, or a numeric argument falls within a certain
range.

While it's possible to do this kind of validation after the arguments have been parsed, or to write
custom property setters that perform the validation, Ookii.CommandLine also provides validation
attributes. The advantage of this is that you can reuse common validation rules, if you use one of
the generated [`Parse()`][Parse()_7], static [`CommandLineParser.Parse<T>()`][] or
[`CommandLineParser<T>.ParseWithErrorHandling()`][] methods it will handle printing validation error
messages, and validators can also add a help message to the argument descriptions in the [usage
help](UsageHelp.md).

## Built-in validators

All validators are in the [`Ookii.CommandLine.Validation`][] namespace, and derive from the
[`ArgumentValidationAttribute`][] class. A validator can also apply to the arguments class as a whole,
rather than a specific argument, and these derive from the [`ClassValidationAttribute`][] class.

There are validators that check the value of an argument, and validators that check argument
inter-dependencies. The following are the built-in argument value validators (dependency validators
are discussed [below](#argument-dependencies-and-restrictions)):

Validator                            | Description                                                                                                                                                                                                                                                                                                                                                                | Applied
-------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------
[`ValidateCountAttribute`][]         | Validates that the number of items for a multi-value argument is in the specified range.                                                                                                                                                                                                                                                                                   | After parsing.
[`ValidateEnumValueAttribute`][]     | Validates that the value is one of the defined values for an enumeration. The [`EnumConverter`][] class allows conversion from the underlying value, even if that value is not a defined value for the enumeration. This validator prevents that. See also [enumeration type conversion](Arguments.md#enumeration-conversion).                                             | After conversion.
[`ValidateNotEmptyAttribute`][]      | Validates that the value of an argument is not an empty string.                                                                                                                                                                                                                                                                                                            | Before conversion.
[`ValidateNotNullAttribute`][]       | Validates that the value of an argument is not null. This is only useful if the [`ArgumentConverter`][] for an argument can return null (for example, the [`NullableConverter`][] can). It's not necessary to use this validator on non-nullable value types, or if using .Net 6.0 or later, or [source generation](SourceGeneration.md), on non-nullable reference types. | After conversion.
[`ValidateNotWhiteSpaceAttribute`][] | Validates that the value of an argument is not an empty string or a string containing only white-space characters.                                                                                                                                                                                                                                                         | Before conversion.
[`ValidatePatternAttribute`][]       | Validates that the value of an argument matches the specified regular expression.                                                                                                                                                                                                                                                                                          | Before conversion.
[`ValidateRangeAttribute`][]         | Validates that the value of an argument is in the specified range. This can be used on any type that implements the [`IComparable<T>`][] interface.                                                                                                                                                                                                                        | After conversion.
[`ValidateStringLengthAttribute`][]  | Validates that the length of an argument's string value is in the specified range.                                                                                                                                                                                                                                                                                         | Before conversion.

Note that there is no `ValidateSetAttribute`, or an equivalent way to make sure that an argument is
one of a predefined set of values, because you're encouraged to use an enumeration type for this
instead, in combination with the [`ValidateEnumValueAttribute`][] if desired. You can of course use
the [`ValidatePatternAttribute`][] for this purpose as well, or you can create a custom validator.

The [`ValidateRangeAttribute`][], [`ValidateCountAttribute`][] and
[`ValidateStringLengthAttribute`][] all allow the use of open-ended ranges, without either a lower
or upper bound.

Depending on the type of validation being done, validation occurs at different times. As indicated
in the table above, validation can happen on the raw string value, before it is converted to the
argument's type, on the value after conversion to the argument's type, and after all arguments have
been parsed. That last one is used by the [`ValidateCountAttribute`][], because it cannot know the
total number of values before that point.

If a built-in validator, other than [`ValidateCountAttribute`][], is used with a multi-value
argument, it's applied to each value individually.

The code below shows some examples of validators:

```csharp
// Must be between 0 and a 100.
[CommandLineArgument]
[ValidateRange(0, 100)]
public int Count { get; set; }

// Must have at least 5 items.
[CommandLineArgument]
[ValidateCount(5, null)]
public string[]? Values { get; set; }

// Must start with a letter, followed by zero or more letters or numbers, case insensitive.
[CommandLineArgument]
[ValidatePattern("^[a-z][a-z0-9]*$", RegexOptions.IgnoreCase)]
public string? Identifier { get; set; }

// Constrain the value to valid enumeration values, and don't allow the use of commas to prevent
// lists of values with this non-flags enumeration.
[CommandLineArgument]
[ValidatePattern("^[^,]*$")]
[ValidateEnumValue]
public DayOfWeek Day { get; set; }
```

### Validation failure

If a validator fails, a [`CommandLineArgumentException`][] is thrown with the [`Category`][]
property set to [`CommandLineArgumentErrorCategory.ValidationFailed`][], and the exception message
set to a custom message provided by the validator. The static [`CommandLineParser.Parse<T>()`][]
method and the [`CommandLineParser<T>.ParseWithErrorHandling()`][] method will print the error message
and show usage help, as always.

For example, the [`ValidateRangeAttribute`][] will use an error message like "The argument 'Count'
must be between 0 and 100." or "The argument 'Count' must be at least 1."

The [`ValidatePatternAttribute`][] validator does not have a custom error message by default,
because it cannot know the purpose of the of the pattern used. Instead, it will return a generic
error message stating the value is invalid. You can use the
[`ValidatePatternAttribute.ErrorMessage`][] property to specify a custom error message.

The [`ValidateEnumValueAttribute`][] validator includes the possible enumeration values in the error
message by default. If there are a lot of values, you may wish to disable this, which can be done
with the [`ValidateEnumValueAttribute.IncludeValuesInErrorMessage`][] property. This same error
message, with the values included, is used by the [`EnumConverter`][] class when the value is an
unrecognized name.

As with all other library error messages, the messages for all built-in validators are obtained from
the [`LocalizedStringProvider`][] class and can be customized by deriving a custom string provider
from that class.

### Usage help

One benefit of using validators is that they can add a help message for their constraint to the
usage help. For example, the [`ValidateRangeAttribute`][] will show a usage help message like "Must
be between 0 and 100." These messages will be added to the end of the argument's description.

The only exceptions are the [`ValidatePatternAttribute`][], which does not know the intent of the
pattern and can therefore not provide a meaningful help message to the user, and the
[`ValidateNotNullAttribute`][]. In this case, you should manually add a message to the argument's
description to make the intent clear.

If you don't wish to include a validator's message in the usage help, you can turn this off using
the [`IncludeInUsageHelp`][IncludeInUsageHelp_0] property, which all built-in validators with usage
help provide. You can also disable the messages for all validators using the
[`UsageWriter.IncludeValidatorsInDescription`][] message.

The [`ValidateEnumValueAttribute`][] will list all defined enumeration values, which may be rather
long depending on the number of values. If the number of values is large, you may wish to exclude it
from the usage help using the [`IncludeInUsageHelp`][IncludeInUsageHelp_0] property.

The validator usage help messages can be customized by deriving a class from the
[`LocalizedStringProvider`][] class. For example, the [custom usage sample](../src/Samples/CustomUsage)
changes the message for the [`ValidateRangeAttribute`][] to look like "[range: 0-100]" instead.

## Argument dependencies and restrictions

Besides argument value validators, there are also a number of built-in validators that specify
dependencies or restrictions on other arguments. The following validators are available:

Validate                   | Description
---------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
[`ProhibitsAttribute`][]   | Indicates that an argument cannot be used in combination with another argument.
[`RequiresAttribute`][]    | Indicates that an argument can only be used in combination with another argument.
[`RequiresAnyAttribute`][] | Validates that at least one of the specified arguments is present on the command line. This is a class validator, that must be applied to the arguments class instead of an argument.

For example, you might have an application that can read data from a file, or from a server at a
specified IP address and port. You could express these arguments as follows:

```csharp
[GeneratedParser]
[RequiresAny(nameof(Path), nameof(Ip))]
partial class ProgramArguments
{
    [CommandLineArgument(IsPositional = true)]
    [Description("The path to use.")]
    public FileInfo? Path { get; set; }

    [CommandLineArgument]
    [Description("The IP address to connect to.")]
    [Prohibits(nameof(Path))]
    public IPAddress? Ip { get; set; }

    [CommandLineArgument]
    [Description("The port to connect to.")]
    [Requires(nameof(Ip))]
    public int Port { get; set; } = 80;
}
```

:warning: **IMPORTANT:** The [`RequiresAttribute`][], [`ProhibitsAttribute`][] and
[`RequiresAnyAttribute`][] all take the name of an _argument_ as their parameters. The use of
`nameof()` as above is only safe if the member names match the argument names.

The `-Ip` argument uses the [`ProhibitsAttribute`][] to indicate it is mutually exclusive with the
"Path" argument. The `-Port` argument uses the [`RequiresAttribute`][] to indicate it can only be
used when the `-Ip` argument is also specified.

The application requires the use of either `-Path` or `-Ip`, but we cannot mark either one required,
because doing so would make it impossible to specify the other argument. Instead, the
[`RequiresAnyAttribute`][] on the class indicates that one or the other must be present for a
successful invocation.

Just like the argument value validators, the dependency validators will add a usage help message if
desired. In the case of a class validator like the [`RequiresAnyAttribute`][], this message is shown
before the description list.

For example, the usage help of the above arguments looks like this:

```text
Usage: cscoretest [[-Path] <FileInfo>] [-Help] [-Ip <IPAddress>] [-Port <Int32>] [-Version]

You must use at least one of: -Path, -Ip.

    -Path <FileInfo>
        The path to use.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Ip <IPAddress>
        The IP address to connect to. Cannot be used with: -Path.

    -Port <Int32>
        The port to connect to. Must be used with: -Ip. Default value: 80.

    -Version [<Boolean>]
        Displays version information.
```

Check out the [argument dependencies sample](../src/Samples/ArgumentDependencies/) to see this in
action.

## Custom validators

Besides the built-in validators, you can also create your own validators by deriving from the
[`ArgumentValidationAttribute`][] class or the [`ClassValidationAttribute`][] class depending on
what type of validation you wish to perform.

If you plan to include a usage help message, derive from the
[`ArgumentValidationWithHelpAttribute`][] class to provide an
[`IncludeInUsageHelp`][IncludeInUsageHelp_0] property, though this is not required.

You must implement at least the [`IsValid()`][] method, which returns a boolean indicating whether
the value is valid (you should not throw an exception). Override the [`GetErrorMessage()`][] method
to provide a custom error message, and the [`GetUsageHelp()`][] method to provide a help message (if
you derived from the [`ArgumentValidationWithHelpAttribute`][] class, override
[`GetUsageHelpCore()`][] instead).

You can also override the [`ErrorCategory`][] property to use a different error category for
validation failure, instead of the default [`ValidationFailed`][ValidationFailed_1].

For the [`ArgumentValidationAttribute`][] class, override the [`Mode`][Mode_3] property to specify
whether you want to run validation before the value is converted to the argument type, after the
conversion (this is the default), or after argument parsing is finished.

For example, the following is a validator that checks if a number is even:

```csharp
class ValidateIsEvenAttribute<T> : ArgumentValidationWithHelpAttribute
    where T : INumberBase<T>
{
    public override bool IsValid(CommandLineArgument argument, object? value)
        => value is T number && T.IsEvenInteger(number);

    public override string GetErrorMessage(CommandLineArgument argument, object? value)
        => $"The argument '{argument.ArgumentName}' must be an even number.";

    protected override string GetUsageHelpCore(CommandLineArgument argument)
        => "Must be an even number.";
}
```

This sample requires .Net 7, because it uses the C# 11 features generic math and generic attributes
so the validator can be used on any numeric argument.

You can also derive from existing validators to customize their behavior. For example, the following
validator customizes the range validator to use a non-constant lower bound, in this case to check
whether a date is in the future for the [`DateOnly`][] structure:

```csharp
class ValidateFutureDateAttribute : ValidateRangeAttribute
{
    public ValidateFutureDateAttribute()
        : base(DateOnly.FromDateTime(DateTime.Today).AddDays(1), null)
    {
    }

    public override string GetErrorMessage(CommandLineArgument argument, object? value)
        => $"The argument '{argument.ArgumentName}' must specify a future date.";

    protected override string GetUsageHelpCore(CommandLineArgument argument)
        => "Must be a date in the future.";
}
```

### Validation using ReadOnlySpan\<char>

If an argument is provided using the name/value separator (e.g. `-Argument:value`), the
[`CommandLineParser`][] class will try to avoid allocating a new string for the value as long as the
argument converter and any pre-conversion validators support using a [`ReadOnlySpan<char>`][]. For
this reason, it's strongly recommended that you implement the
[`ArgumentValidationAttribute.IsSpanValid`][] method for a custom pre-conversion validator. This
does not apply to validators that don't use [`ValidationMode.BeforeConversion`][].

Now that you know (almost) everything there is to know about arguments, let's move on to
[subcommands](Subcommands.md).

[`ArgumentConverter`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Conversion_ArgumentConverter.htm
[`ArgumentValidationAttribute.IsSpanValid`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Validation_ArgumentValidationAttribute_IsSpanValid.htm
[`ArgumentValidationAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ArgumentValidationAttribute.htm
[`ArgumentValidationWithHelpAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ArgumentValidationWithHelpAttribute.htm
[`Category`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_CommandLineArgumentException_Category.htm
[`ClassValidationAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ClassValidationAttribute.htm
[`CommandLineArgumentErrorCategory.ValidationFailed`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_CommandLineArgumentErrorCategory.htm
[`CommandLineArgumentException`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_CommandLineArgumentException.htm
[`CommandLineParser.Parse<T>()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandLineParser<T>.ParseWithErrorHandling()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_CommandLineParser_1_ParseWithErrorHandling.htm
[`DateOnly`]: https://learn.microsoft.com/dotnet/api/system.dateonly
[`EnumConverter`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Conversion_EnumConverter.htm
[`ErrorCategory`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Validation_ArgumentValidationAttribute_ErrorCategory.htm
[`GetErrorMessage()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Validation_ArgumentValidationAttribute_GetErrorMessage.htm
[`GetUsageHelp()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Validation_ArgumentValidationAttribute_GetUsageHelp.htm
[`GetUsageHelpCore()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Validation_ArgumentValidationWithHelpAttribute_GetUsageHelpCore.htm
[`IComparable<T>`]: https://learn.microsoft.com/dotnet/api/system.icomparable-1
[`IsValid()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Validation_ArgumentValidationAttribute_IsValid.htm
[`LocalizedStringProvider`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_LocalizedStringProvider.htm
[`NullableConverter`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Conversion_NullableConverter.htm
[`Ookii.CommandLine.Validation`]: https://www.ookii.org/docs/commandline-4.0/html/N_Ookii_CommandLine_Validation.htm
[`ProhibitsAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ProhibitsAttribute.htm
[`ReadOnlySpan<char>`]: https://learn.microsoft.com/dotnet/api/system.readonlyspan-1
[`RequiresAnyAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_RequiresAnyAttribute.htm
[`RequiresAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_RequiresAttribute.htm
[`UsageWriter.IncludeValidatorsInDescription`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_UsageWriter_IncludeValidatorsInDescription.htm
[`ValidateCountAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ValidateCountAttribute.htm
[`ValidateEnumValueAttribute.IncludeValuesInErrorMessage`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_IncludeValuesInErrorMessage.htm
[`ValidateEnumValueAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ValidateEnumValueAttribute.htm
[`ValidateNotEmptyAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ValidateNotEmptyAttribute.htm
[`ValidateNotNullAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ValidateNotNullAttribute.htm
[`ValidateNotWhiteSpaceAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ValidateNotWhiteSpaceAttribute.htm
[`ValidatePatternAttribute.ErrorMessage`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Validation_ValidatePatternAttribute_ErrorMessage.htm
[`ValidatePatternAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ValidatePatternAttribute.htm
[`ValidateRangeAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ValidateRangeAttribute.htm
[`ValidateStringLengthAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ValidateStringLengthAttribute.htm
[`ValidationMode.BeforeConversion`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Validation_ValidationMode.htm
[IncludeInUsageHelp_0]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Validation_ArgumentValidationWithHelpAttribute_IncludeInUsageHelp.htm
[Mode_3]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Validation_ArgumentValidationAttribute_Mode.htm
[Parse()_7]: https://www.ookii.org/docs/commandline-4.0/html/Overload_Ookii_CommandLine_IParser_1_Parse.htm
[ValidationFailed_1]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_CommandLineArgumentErrorCategory.htm
