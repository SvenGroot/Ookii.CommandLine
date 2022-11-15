# Argument validation and dependencies

It's often necessary to validate the value of an argument beyond its type. For example, you may
wish to make sure a string argument is not empty, or a numeric argument falls within a certain
range.

While it's possible to do this kind of validation after the arguments have been parsed, or to write
custom property setters that perform the validation, Ookii.CommandLine also provides validation
attributes. The advantage of this is that you can reuse common validation rules, if you use the
static `CommandLineParser.Parse<T>()` method it will handle printing validation error messages, and
validators can also add a help message to the argument descriptions in the [usage help](UsageHelp.md).

## Built-in validators

All validators are in the `Ookii.CommandLine.Validation` namespace, and derive from the
`ArgumentValidationAttribute` class. A validator can also apply to the arguments class as a whole,
rather than a specific argument, and these derive from the `ClassValidationAttribute` class.

There are validators that check the value of an argument, and validators that check argument
inter-dependencies. The following are the built-in argument value validators:

Validator                        | Description
---------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
`ValidateCountAttribute`         | Makes sure the number of items for a multi-value argument is in the specified range.
`ValidateNotEmptyAttribute`      | Makes sure the value of an argument is not an empty string.
`ValidateNotNullAttribute`       | Makes sure the value of an argument is not `null`. This is only useful if the `TypeConverter` for an argument can return `null`. It's not necessary to use this argument on value types, or if using .Net 6.0 or later, on non-nullable reference types.
`ValidateNotWhiteSpaceAttribute` | Makes sure the value of argument of an argument is not an empty string or a string containing only white-space characters.
`ValidatePatternAttribute`       | Makes sure the value of an argument matches the specified regular expression.
`ValidateRangeAttribute`         | Makes sure the value of an argument in in the specified range.
`ValidateStringLengthAttribute`  | Makes sure the length of an argument's string value is in the specified range.

Note that there is no `ValidateSetAttribute`, or an equivalent way to make sure that an argument is
one of a predefined set of values, because you're encouraged to use an enumeration type for this
instead. You can of course use the `ValidatePatternAttribute` for this purpose as well.

The `ValidateRangeAttribute`, `ValidateCountAttribute` and `ValidateStringLengthAttribute` all allow
the use of open-ended ranges, without either a lower or upper bound.

Depending on the type of validation being done, validation occurs at different times. The
`ValidateNotNullAttribute` and the `ValidateRangeAttribute` are applied to the converted value, when
the argument is parsed.

The `ValidateCountAttribute` is applied after all arguments are parsed, because it cannot know the
total number of values before that point.

The remaining validators are applied to the string value before type conversion occurs. This means
you can use these validators regardless of the actual type of the argument.

If a validator, other than `ValidateCountAttribute` is used with a multi-value argument, it's
applied to each value.

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
```

### Validation failure

If a validator fails, a `CommandLineArgumentException` is thrown with the `Category` property set to
`CommandLineArgumentErrorCategory.ValidationFailed`, and the . The static
`CommandLineParse.Parse<T>()` method will print the validator's custom error message.

As with all other error messages, the messages for all built-in validators are obtained from the
`LocalizedStringProvider` class and can be customized by deriving a custom string provider from that
class.
