# Command line arguments

If you've gone through the [tutorial](Tutorial.md), you'll already have some idea of how
Ookii.CommandLine parses arguments. This page will explain the rules in detail, including all the
possible kinds or arguments.

Command line arguments are passed to your application when it is started, and are typically accessed
through the parameter of the `static void Main(string[] args)` method. This provides the arguments
as an array of strings, which Ookii.CommandLine will parse to extract strongly-typed, named values
that you can easily access in your application.

The way the raw string arguments are interpreted is determined by the command line argument parsing
rules. Ookii.CommandLine supports two sets of parsing rules: the default mode, which uses parsing
rules similar to those used by PowerShell, and [long/short mode](#longshort-mode), which is more
POSIX-like, and lets arguments have a long name and a short name, with different prefixes. Most of
the below information applies to both modes, with the differences described when applicable.

## Named arguments

In Ookii.CommandLine, all command line arguments have a name, and can be assigned a value on the
command line using that name. They follow the name of your application's executable on the command
prompt, and typically take the following form:

```text
-ArgumentName value
```

The argument name is preceded by the _argument name prefix_. This prefix is configurable, but
defaults to accepting a dash (`-`) and a forward slash (`/`) on Windows, and only a dash (`-`) on
other platforms such as Linux or MacOS. In long/short mode, this may be the long argument name
prefix, which is `--` by default.

Argument names are case insensitive by default, though this can be customized using the
[`ParseOptionsAttribute.CaseSensitive`][] property or the [`ParseOptions.ArgumentNameComparison`][]
property.

The argument's value follows the name, separated by either white space (as a separate argument
token), or by the argument name/value separator; by default, both a colon (`:`) and an equals sign
(`=`) are accepted. The following three example are identical:

```text
-ArgumentName value
-ArgumentName:value
-ArgumentName=value
```

Whether white-space is allowed to separate the name and value is configured using the
[`ParseOptionsAttribute.AllowWhiteSpaceValueSeparator`][] or
[`ParseOptions.AllowWhiteSpaceValueSeparator`][] property, and the argument name/value separator(s)
can be customized using the [`ParseOptionsAttribute.NameValueSeparators`][] or
[`ParseOptions.NameValueSeparators`][] property.

The name/value separator cannot occur in the argument name; however, it can still be used in
argument values. For example, `-ArgumentName:foo:bar` will give `-ArgumentName` the value `foo:bar`.

Not all arguments require values; those that do not are called [_switch arguments_](#switch-arguments)
and have a value determined by their presence or absence on the command line.

### Short names

In long/short mode, an argument can have an additional, one-character short name. This short name
is often the first character of the long name, but it can be any character. Where long names in
long/short mode use the long argument prefix (`--` by default), short names have their own prefix,
which is `-` (and on Windows, `/`) by default.

For example, if the argument `--argument-name` has the short name `-a`, the following are equivalent:

```text
--argument-name value
-a value
```

### Aliases

An argument can have one or more aliases: alternative names that can also be used to supply the same
argument. For example, an argument named `-Verbose` might use the alias `-v` as a shorter to type
alternative. In long/short mode, an argument can have both long and short aliases.

By default, Ookii.CommandLine accepts [any prefix](DefiningArguments.md#automatic-prefix-aliases)
that uniquely identifies a single argument as an alias for that argument, without having to
explicitly define those aliases.

For example, if you have two arguments named `-File` and `-Folder`, you can refer to the first
argument with `-Fi` and `-Fil` (also case insensitive by default). For the second one, `-Fo`,
`-Fol`, `-Fold` and `-Folde`. However, `-F` is not an automatic prefix alias, because it could refer
to either argument.

When using long/short mode, automatic prefix aliases apply to arguments' long names. An argument
named `--argument` can automatically be used with the prefix alias `--a` (assuming it is unique),
but the short name `-a` will only exist if it was explicitly created.

## Positional arguments

An argument can be _positional_, which means in addition to being supplied by name, it can also be
supplied without the name, using the ordering of the values. Which argument the value belongs to
is determined by its position relative to other positional arguments.

If an argument value is encountered without being preceded by a name, it is matched to the
next positional argument without a value. For example, take an application that has three arguments:
`-Positional1`, `-Positional2` and `-Positional3` are positional, in that order, and `-NamedOnly` is
non-positional.

Now, consider the following invocation:

```text
value1 -NamedOnly value2 value3
```

In this case, "value1" is not preceded by a name; therefore, it is matched to the argument
`-Positional1`. The value "value2" follows a name, so it is matched to the argument with the name
`-NamedOnly`. Finally, "value3" is matched to the second positional argument, which is
`-Positional2`.

A positional argument can still be supplied by name. If a positional argument is supplied by name,
it cannot also be specified by position. Take the following example:

```text
value1 -Positional2 value2 value3
```

In this case, "value1" is still matched to `-Positional1`. The value for `-Positional2` is now
given by name, and is "value2". The value "value3" is for the next positional argument, but since
`-Positional2` already has a value, it will be assigned to `-Positional3` instead.

The following example would cause an error:

```text
value1 -Positional1 value2
```

This is because `-Positional1` is assigned to twice; first by position, and then by name. Duplicate
arguments cause an error by default, though this can be changed.

## The `--` argument

Optionally, when an argument is encountered that consists only of `--` without a name following it,
this indicates that all following values must be treated as positional values, even if they begin
with an argument name prefix.

For example, take the following command line:

```text
value1 -- --value2
```

In this example, the second positional argument would be set to the value "--value2". If there is
an argument named "value2", it would not be set.

This behavior is disabled by default, but can be enabled using the
[`ParseOptionsAttribute.PrefixTermination`][] or [`ParseOptions.PrefixTermination`][] property. It
can be used with both the default parsing mode and long/short mode. Alternatively, you can also set
it so that the `--` argument will [cancel parsing](DefiningArguments.md#arguments-that-cancel-parsing).

## Required arguments

A command line argument that is required must be supplied on all invocations of the application. If
a required argument is not supplied, this is considered an error and parsing will fail.

Any argument can be made required. Usually, it is recommended for any required argument to also be a
positional argument, but this is not mandatory.

For positional arguments, required arguments must always come before optional arguments; it is not
allowed to define a required positional argument after an optional positional argument.

## Switch arguments

A switch argument, sometimes also called a flag, is an argument with a Boolean type (`bool`). Its
value is determined by its presence or absence on the command line; the value will be true if the
argument is supplied, and false if not. The following sets the switch argument named “Switch” to
true:

```text
-Switch
```

A switch argument’s value can be specified explicitly, as in the following example:

```text
-Switch:false
```

You must use the name/value separator (a colon or equals sign by default) to specify an explicit
value for a switch argument; you cannot use white space. If the command line contains `-Switch false`,
then `false` is the value of the next positional argument, not the value for `-Switch`.

### Combined switch arguments

For switch arguments with short names when using long/short mode, the switches can be combined in a
single argument. For example, given the switches with the short names `-a`, `-b` and `-c`, the
following command line sets all three switches:

```text
-abc
```

This is equivalent to:

```text
-a -b -c
```

This only works for switch arguments, and does not apply to long names or the default parsing mode.

## Arguments with multiple values

Some arguments can take multiple values; these are _multi-value arguments_. These arguments can be
supplied multiple times, and each value is added to the set of values. For example, consider the
following command line arguments:

```text
-ArgumentName value1 –ArgumentName value2 –ArgumentName value3
```

In this case, if `-ArgumentName` is a multi-value argument, the value of the argument will be a list
holding all three values.

It’s possible to specify a separator for multi-value arguments using the
[`MultiValueSeparatorAttribute`][] attribute. This makes it possible to specify multiple values for
the argument while the argument itself is specified only once. For example, if the separator is set
to a comma, you can specify the values as follows:

```text
-ArgumentName value1,value2,value3
```

In this case, the value of the argument named `-ArgumentName` will be a list with the three values
"value1", "value2" and "value3".

**Note:** if you specify a separator for a multi-value argument, it is _not_ possible to have an
argument value containing the separator. There is no way to escape the separator. Therefore, make
sure you pick a separator that will never be used in the argument values, and be extra careful with
culture-sensitive argument types.

You can also use the [`MultiValueSeparatorAttribute`][] to indicate the argument allows white-space
separated values, which means it will consume multiple argument tokens that follow it.

```text
-ArgumentName value1 value2 value3
```

This has the same effect as above, with all three values treated as values for the one argument. A
multi-value argument that uses this option will take all following values until another argument
name is encountered. That means that you cannot specify positional arguments until you've used
another named argument.

If a multi-value argument is positional, it must be the last positional argument. All remaining
positional argument values will be considered values for the multi-value argument.

If a multi-value argument is required, it means it must have at least one value. You cannot set a
default value for an optional multi-value argument.

An argument can be both multi-value and a switch. A value of true (or the explicit value if one is
given) gets added to the list for every time that the argument is supplied.

If an argument is not a multi-value argument, it is an error to supply it more than once, unless
duplicate arguments are allowed in the [`ParseOptions`][] or [`ParseOptionsAttribute`][], in which
case only the last value is used. It's also possible to emit a warning for duplicate values using
the [`ParseOptions`][].

## Dictionary arguments

Dictionary arguments are multi-value arguments that specify a set of key/value pairs. Each value for
a dictionary argument takes the form `key=value`, like in the following example:

```text
-ArgumentName key1=value1 –ArgumentName key2=value2
```

In this case, the value of the argument named `-ArgumentName` will be a dictionary with two keys,
"key1" and "key2", with the associated values "value1" and "value2" respectively.

If you specify the same key more than once, an exception will be thrown, unless the
[`AllowDuplicateDictionaryKeysAttribute`][] attribute is specified for the argument.

The default key/value separator (which is `=`) can be overridden using the
[`KeyValueSeparatorAttribute`][] attribute.

## Argument value conversion

Ookii.CommandLine allows you to define arguments with any .Net type, including types such as
[`String`][], [`Int32`][], [`DateTime`][], [`FileInfo`][], [`Uri`][], any enumeration type, and many more.
Any type can be used; the only requirement is that it is possible to convert a string value to that
type.

Ookii.CommandLine will try to convert the argument using the following options, in order of
preference:

1. If the argument has the [`ArgumentConverterAttribute`][] applied, the specified custom
   [`ArgumentConverter`][].
2. For .Net 7 and later:
   1. An implementation of the [`ISpanParsable<TSelf>`][] interface.
   2. An implementation of the [`IParsable<TSelf>`][] interface.
3. A `public static Parse(string, ICultureInfo)` method.
4. A `public static Parse(string)` method.
5. A public constructor that takes a single `string` argument.

This will cover the majority of types you'd want to use for arguments without having to write any
conversion code. If you write your own custom type, you can use it for arguments as long as it meets
one of the above criteria.

It is possible to override the default conversion by specifying a custom converter using the
[`ArgumentConverterAttribute`][]. When this attribute is applied to an argument, the specified type
converter will be used for conversion instead of any of the default methods.

Previous versions of Ookii.CommandLine used .Net's [`TypeConverter`][] class. Starting with
Ookii.CommandLine 4.0, this is no longer the case, and the [`ArgumentConverter`][] class is used
instead. [See here](DefiningArguments.md#custom-type-conversion) for more information on how to
upgrade code that relied on a [`TypeConverter`][].

### Enumeration conversion

The [`EnumConverter`][] used for enumeration types relies on the [`Enum.Parse()`][] method. Its
default behavior is to use case insensitive conversion, and to allow both the names and underlying
value of the enumeration to be used. This means that e.g. for the [`DayOfWeek`][] enumeration,
"Monday", "monday", and "1" can all be used to indicate [`DayOfWeek.Monday`][].

In the case of a numeric value, the converter does not check if the resulting value is valid for the
enumeration type, so again for [`DayOfWeek`][], a value of "9" would be converted to `(DayOfWeek)9`
even though there is no such value in the enumeration.

To ensure the result is constrained to only the defined values of the enumeration, use the
[`ValidateEnumValueAttribute` validator](Validation.md). This validator can also be used to alter
the conversion behavior. You can enable case sensitivity with the
[`ValidateEnumValueAttribute.CaseSensitive`][] property, and disallow numeric values with the
[`ValidateEnumValueAttribute.AllowNumericValues`][] property.

By default, the converter allows the use of comma-separated values, which will be combined using a
bitwise or operation. This is allowed regardless of whether or not the [`FlagsAttribute`][]
attribute is present on the enumeration, which can have unexpected results. Using the
[`DayOfWeek`][] example again, "Monday,Tuesday" would result in the value
`DayOfWeek.Monday | DayOfWeek.Tuesday`, which is actually equivalent to [`DayOfWeek.Wednesday`][].

Comma-separated values can be disabled by using the
[`ValidateEnumValueAttribute.AllowCommaSeparatedValues`][] property.

These properties of the [`ValidateEnumValueAttribute`][] attribute only work if the default
[`EnumConverter`][] is used; a custom converter may or may not check them.

### Multi-value and dictionary value conversion

For multi-value and dictionary arguments, the converter must be for the element type (e.g. if the
argument is a multi-value argument of type `int[]`, the argument converter must be able to convert to
`int`).

For a dictionary argument the element type is [`KeyValuePair<TKey, TValue>`][], and the type
converter is responsible for parsing the key and value from the argument value.

Ookii.CommandLine provides the [`KeyValuePairConverter<TKey, TValue>`][] class that is used by default
for dictionary arguments. You can override this using the [`ArgumentConverterAttribute`][] as usual, but
if you only want to customize the parsing of the key and value types, you can use the
[`KeyConverterAttribute`][] and the [`ValueConverterAttribute`][] attributes respectively.

The [`KeyValuePairConverter<TKey, TValue>`][] will use those attributes to determine which converter to
use instead of the default for the key and value types. You can also customize the key/value
separator used by this converter using the [`KeyValueSeparatorAttribute`][] attribute.

If you do specify the [`ArgumentConverterAttribute`][] for a dictionary argument, the
[`KeyConverterAttribute`][], [`ValueConverterAttribute`][], and [`KeyValueSeparatorAttribute`][]
attributes will be ignored.

### Conversion culture

For many types, the conversion can be culture dependent. For example, converting numbers or dates
depends on the [`CultureInfo`][] class, which defines the accepted formats and how they’re
interpreted; for example, some cultures might use a period as the decimal separator, while others
use a comma.

To ensure a consistent parsing experience for all users regardless of their machine's regional
format settings, Ookii.CommandLine defaults to using [`CultureInfo.InvariantCulture`][]. You can
change this using the [`ParseOptions.Culture`][] property, but be very careful if you do.

## Arguments with non-nullable types

Ookii.CommandLine provides support for nullable reference types. Not only is the library itself
fully annotated, but if you use [source generation](SourceGeneration.md) or the .Net 6.0 version of
the library, command line argument parsing takes into account the nullability of the argument types.
If the argument is declared with a nullable reference or value type (e.g. `string?` or `int?`),
nothing changes. But if the argument is not nullable (e.g. `string` (in a context with NRT support)
or `int`), [`CommandLineParser`][] will ensure that the value will not be null.

Assigning a null value to an argument only happens if the [`ArgumentConverter`][] for that argument
returns null as the result of the conversion. If this happens and the argument is not nullable, a
[`CommandLineArgumentException`][] is thrown with the category set to
[`NullArgumentValue`][NullArgumentValue_0].

For multi-value arguments, the nullability check applies to the type of the elements (e.g.
`string?[]` for an array), and for dictionary arguments, it applies to the value (e.g.
`Dictionary<string, string?>`); the key may never be nullable for a dictionary argument.

Null-checking for non-nullable reference types is available for all runtime versions if you use
source generation. If you cannot use source generation, only the .Net 6.0 and later versions of
Ookii.CommandLine can determine the nullability of reference types when using reflection. The
.Net Standard versions of the library will only apply this check to value types unless source
generation was used.

See also the [`CommandLineArgument.AllowNull`][] property.

## Long/short mode

The default behavior of Ookii.CommandLine is similar to how PowerShell parses arguments. However,
many command line tools like `dotnet`, `git`, and many others use POSIX or GNU conventions. This is
especially common for Linux or cross-platform applications.

POSIX and GNU conventions specify that options use a dash (`-`) followed by a single character, and
define the concept of long options, which use `--` followed by an a multi-character name.

Ookii.CommandLine calls this style of parsing "long/short mode," and offers it as an alternative
mode to the default parsing rules. In this mode, an argument can have a long name, which takes the
place of the regular argument name, and an additional single-character short name. By default,
Ookii.CommandLine follows the convention of using the prefix `--` for long names, and `-` (and `/`
on Windows only) for short names.

Besides allowing the alternative names, long/short mode follows the same rules as the default mode,
with the differences as explained above.

POSIX conventions also specify the use of lower case argument names, with dashes separating words
("dash-case"), which you can easily achieve using [name transformation](DefiningArguments.md#name-transformation),
and case-sensitive argument names. For information on how to set these options,
[see here](DefiningArguments.md#longshort-mode).

## More information

Next, let's take a look at how to [define arguments](DefiningArguments.md).

[`AllowDuplicateDictionaryKeysAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_AllowDuplicateDictionaryKeysAttribute.htm
[`ArgumentConverter`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_ArgumentConverter.htm
[`ArgumentConverterAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_ArgumentConverterAttribute.htm
[`CommandLineArgument.AllowNull`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgument_AllowNull.htm
[`CommandLineArgumentException`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_CommandLineArgumentException.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CultureInfo.InvariantCulture`]: https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo.invariantculture
[`CultureInfo`]: https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo
[`DateTime`]: https://learn.microsoft.com/dotnet/api/system.datetime
[`DayOfWeek.Monday`]: https://learn.microsoft.com/dotnet/api/system.dayofweek
[`DayOfWeek.Wednesday`]: https://learn.microsoft.com/dotnet/api/system.dayofweek
[`DayOfWeek`]: https://learn.microsoft.com/dotnet/api/system.dayofweek
[`Enum.Parse()`]: https://learn.microsoft.com/dotnet/api/system.enum.parse
[`EnumConverter`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_EnumConverter.htm
[`FileInfo`]: https://learn.microsoft.com/dotnet/api/system.io.fileinfo
[`FlagsAttribute`]: https://learn.microsoft.com/dotnet/api/system.flagsattribute
[`Int32`]: https://learn.microsoft.com/dotnet/api/system.int32
[`IParsable<TSelf>`]: https://learn.microsoft.com/dotnet/api/system.iparsable-1
[`ISpanParsable<TSelf>`]: https://learn.microsoft.com/dotnet/api/system.ispanparsable-1
[`KeyConverterAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_KeyConverterAttribute.htm
[`KeyValuePair<TKey, TValue>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.keyvaluepair-2
[`KeyValuePairConverter<TKey, TValue>`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_KeyValuePairConverter_2.htm
[`KeyValueSeparatorAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_KeyValueSeparatorAttribute.htm
[`MultiValueSeparatorAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_MultiValueSeparatorAttribute.htm
[`ParseOptions.AllowWhiteSpaceValueSeparator`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptions_AllowWhiteSpaceValueSeparator.htm
[`ParseOptions.ArgumentNameComparison`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptions_ArgumentNameComparison.htm
[`ParseOptions.Culture`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptions_Culture.htm
[`ParseOptions.NameValueSeparators`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptions_NameValueSeparators.htm
[`ParseOptions.PrefixTermination`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptions_PrefixTermination.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute.AllowWhiteSpaceValueSeparator`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_AllowWhiteSpaceValueSeparator.htm
[`ParseOptionsAttribute.CaseSensitive`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_CaseSensitive.htm
[`ParseOptionsAttribute.NameValueSeparators`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_NameValueSeparators.htm
[`ParseOptionsAttribute.PrefixTermination`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_PrefixTermination.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`String`]: https://learn.microsoft.com/dotnet/api/system.string
[`TypeConverter`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverter
[`Uri`]: https://learn.microsoft.com/dotnet/api/system.uri
[`ValidateEnumValueAttribute.AllowCommaSeparatedValues`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_AllowCommaSeparatedValues.htm
[`ValidateEnumValueAttribute.AllowNumericValues`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_AllowNumericValues.htm
[`ValidateEnumValueAttribute.CaseSensitive`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_CaseSensitive.htm
[`ValidateEnumValueAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Validation_ValidateEnumValueAttribute.htm
[`ValueConverterAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_ValueConverterAttribute.htm
[NullArgumentValue_0]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_CommandLineArgumentErrorCategory.htm
