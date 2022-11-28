# Command line arguments

If you've gone through the [tutorial](Tutorial.md), you'll already have some idea of how
Ookii.CommandLine parses arguments. This page will explain the rules in detail, including all the
possible kinds or arguments.

Command line arguments are passed to your application when it is started, and are typically accessed
through the parameter of the `static void Main(string[] args)` method. This provides the arguments
as an array of strings, which Ookii.CommandLine will parse to extract strongly-typed, named values
that you can easily access in your application.

The method used to extract values from the array of string arguments is determined by the command
line argument parsing rules. Ookii.CommandLine supports two sets of parsing rules: the default mode,
which uses parsing rules similar to those used by PowerShell, and [long/short mode](#longshort-mode),
which lets arguments have a long name and a short name, with different prefixes. Most of the below
information applies to both modes, with the differences described at the end.

## Named arguments

In Ookii.CommandLine, all command line arguments have a name, and can be assigned a value on the
command line using that name. They follow the name of your application's executable on the command
prompt, and typically take the following form:

```text
-ArgumentName value
```

The argument name is preceded by the _argument name prefix_. This prefix is configurable, but
defaults to accepting a dash (`-`) and a forward slash (`/`) on Windows, and only a dash (`-`) on
other platforms such as Linux or MacOS.

Argument names are case insensitive by default, though this can be customized using the
[`ParseOptionsAttribute.CaseSensitive`][] property or the [`ParseOptions.ArgumentNameComparer`][]
property.

The argument's value follow the name, separated by either white space (as a separate argument token),
or by the argument name/value separator, which is a colon (`:`) by default. The following is
identical to the previous example:

```text
-ArgumentName:value
```

Whether white-space is allowed to separate the name and value is configured using the
[`ParseOptionsAttribute.AllowWhiteSpaceValueSeparator`][] or
[`ParseOptions.AllowWhiteSpaceValueSeparator`][] property, and the argument name/value separator can
be customized using the [`ParseOptionsAttribute.NameValueSeparator`][] or
[`ParseOptions.NameValueSeparator`][] property.

Not all arguments require values; those that do not are called [_switch arguments_](#switch-arguments)
and have a value determined by their presence or absence on the command line.

An argument can have one or more aliases: alternative names that can also be used to supply the same
argument. For example, an argument named `-Verbose` might use the alias `-v` as a shorter to type
alternative.

## Positional arguments

An argument can be _positional_, which means in addition to being supplied by name, it can also be
supplied without the name, using the position of the value. Which argument the value belongs to
is determined by its position relative to other positional arguments.

If an argument value is encountered without being preceded by a name, it is matched to the
next positional argument without a value. For example, take the following command line arguments:

```text
value1 –ArgumentName value2 value3
```

In this case, value1 is not preceded by a name; therefore, it is matched to the first positional
argument. Value2 follows a name, so it is matched to the argument with the name `-ArgumentName`.
Finally, value3 is matched to the second positional argument.

A positional argument can still be supplied by name. If a positional argument is supplied by name,
it cannot also be specified by position; in the previous example, if the argument named
`-ArgumentName` was the second positional argument, then value3 becomes the value for the third
positional argument, because the value for `-ArgumentName` was already specified by name. If
`-ArgumentName` is the first positional argument, this would cause an error (unless duplicate
arguments are allowed in the options), because it already had a value set by `value`.

## Required arguments

A command line argument that is required must be supplied on all invocations of the application. If a
required argument is not supplied, this is considered an error and parsing will fail.

Any argument can be made required. Usually, it is recommended for any required argument to also be a
positional argument, but this is not mandatory.

For positional arguments, required arguments must always come before optional arguments; it is not
allowed to define a required positional argument after an optional positional argument.

## Switch arguments

A switch argument is an argument with a Boolean type (`bool`). Its value is determined by its
presence or absence on the command line; the value will be true if the argument is supplied, and
false if not. The following sets the switch argument named “Switch” to true:

```text
-Switch
```

A switch argument’s value can be specified explicitly, as in the following example:

```text
-Switch:false
```

You must use the name/value separator (a colon by default) to specify an explicit value for a switch
argument; you cannot use white space. If the command line contains `-Switch false`, then `false` is
the value of the next positional argument, not the value for `-Switch`.

If you use a nullable Boolean type (`bool?`) as the type of the argument, it will be `null` if
not supplied, `true` if supplied, and `false` only if explicitly set to false using `-Switch:false`.

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
[`MultiValueSeparatorAttribute`][] attribute. This makes it possible to specify multiple values for the
argument while the argument itself is specified only once. For example, if the separator is set to a
comma, you can specify the values as follows:

```text
-ArgumentName value1,value2,value3
```

In this case, the value of the argument named `-ArgumentName` will be a list with the three values "value1", "value2" and "value3".

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

If the type of the argument is a list of Boolean values (e.g. `bool[]`), it will act as a
multi-value argument and a switch. A value of true (or the explicit value if one is given) gets
added to the list for every time that the argument is supplied.

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

The default key/value separator (which is `=`) can be overridden using the [`KeyValueSeparatorAttribute`][] attribute.

## Argument value conversion

Ookii.CommandLine allows you to define arguments with any .Net type, including types such as
[`String`][], [`Int32`][], [`DateTime`][], [`FileInfo`][], [`Uri`][], any enumeration type, and many more.
Any type can be used; the only requirement is that it is possible to convert a string value to that
type.

Ookii.CommandLine will try to convert the argument using the following options, in order of
preference:

1. If the argument has the [`TypeConverterAttribute`][] applied, the specified custom
   [`TypeConverter`][].
2. The argument type's default [`TypeConverter`][], if it can convert from a string.
3. A `public static Parse(String, ICultureInfo)` method.
4. A `public static Parse(String)` method.
5. A public constructor that takes a single string argument.

This will cover the majority of types you'd want to use for arguments without having to write any
conversion code. If you write your own custom type, you can use it for arguments as long as it meets
one of the above criteria (a [`TypeConverter`][] is preferred).

It is possible to override the default conversion by specifying a custom type converter using the
[`System.ComponentModel.TypeConverterAttribute`][]. When this attribute is applied to an argument,
the specified type converter will be used for conversion instead of any of the default methods.

### Enumeration type conversion

The default [`TypeConverter`][] for enumeration types uses case insensitive conversion, and allows
both the names and underlying value of the enumeration to be used. This means that e.g. for the
[`DayOfWeek`][] enumeration, "Monday", "monday", and "1" can all be used to indicate
[`DayOfWeek.Monday`][].

In the case of a numeric value, the converter does not check if the resulting value is valid for
the enumeration type, so again for [`DayOfWeek`][], a value of "9" would be converted to `(DayOfWeek)9`
even though there is no such value in the enumeration.

To ensure the result is constrained to only the defined values of the enumeration, use the
[`ValidateEnumValueAttribute` validator](Validation.md).

The converter allows the use of comma-separated values, which will be combined using a bitwise or
operation. This is allowed regardless of whether or not the [`FlagsAttribute`][] attribute is present on
the enumeration, which can have unexpected results. Using the [`DayOfWeek`][] example again,
"Monday,Tuesday" would result in the value `DayOfWeek.Monday | DayOfWeek.Tuesday`, which is actually
equivalent to [`DayOfWeek.Wednesday`][].

One way to avoid this is to use the following pattern validator, which ensures that the
string value before conversion does not contain a comma:

```csharp
[ValidatePattern("^[^,]*$")]
```

You can also use a pattern like `"^[a-zA-Z]"` to ensure the value starts with a letter, to disallow
the use of numeric values entirely.

### Multi-value and dictionary value conversion

For multi-value and dictionary arguments, the converter must be for the element type (e.g. if the
argument is a multi-value argument of type `int[]`, the type converter must be able to convert to
`int`). For a dictionary argument the element type is [`KeyValuePair<TKey, TValue>`][], and the type
converter is responsible for parsing the key and value from the argument value.

Ookii.CommandLine provides the [`KeyValuePairConverter<TKey, TValue>`][] class that is used by default
for dictionary arguments. You can override this using the [`TypeConverterAttribute`][] as usual, but
if you only want to customize the parsing of the key and value types, you can use the
[`KeyTypeConverterAttribute`][] and the [`ValueTypeConverterAttribute`][] attributes respectively.
The [`KeyValuePairConverter<TKey, TValue>`][] will use those attributes to locate a custom converter.
You can also customize the key/value separator used by this converter using the
[`KeyValueSeparatorAttribute`][] attribute.

If you do specify the [`TypeConverterAttribute`][] for a dictionary argument, the
[`KeyTypeConverterAttribute`][], [`ValueTypeConverterAttribute`][], and [`KeyValueSeparatorAttribute`][]
attributes will be ignored.

### Conversion culture

For many types, the conversion can be culture dependent. For example, converting numbers or dates
depends on the [`CultureInfo`][] class, which defines the accepted formats and how they’re
interpreted; for example, some cultures might use a period as the decimal separator, while others
use a comma.

To ensure a consistent parsing experience for all users regardless of their machine's regional
format settings, Ookii.CommandLine defaults to using [`CultureInfo.InvariantCulture`][]. You can change
this using the [`ParseOptions.Culture`][] property, but be very careful if you do.

## Arguments with non-nullable types

Ookii.CommandLine provides support for nullable reference types. Not only is the library itself
fully annotated, but if you use the .Net 6.0 version of the library, command line argument parsing
takes into account the nullability of the argument types. If the argument is declared with a
nullable reference or value type (e.g. `string?` or `int?`), nothing changes. But if the argument is
not nullable (e.g. `string` (in a context with NRT support) or `int`), [`CommandLineParser`][] will
ensure that the value will not be null.

Assigning a null value to an argument only happens if the [`TypeConverter`][] for that argument returns
`null` as the result of the conversion. If this happens and the argument is not nullable, a
[`CommandLineArgumentException`][] is thrown with the category set to [`NullArgumentValue`][NullArgumentValue_0].

Null-checking for non-nullable reference types is only available in .Net 6.0 and later. If you are
using the .Net Standard versions of Ookii.CommandLine, this check is only done for value types.

For multi-value arguments, the nullability check applies to the type of the elements (e.g.
`string?[]` for an array), and for dictionary arguments, it applies to the value (e.g.
`Dictionary<string, string?>`); the key may never be null for a dictionary argument.

See also the [`CommandLineArgument.AllowNull`][] property.

## Long/short mode

Ookii.CommandLine supports an alternative parsing mode called "long/short mode." In this mode,
arguments can have long names and single-character short names, each with their own argument name
prefix. By default, the prefix `--` is used for long names, and `-` (and `/` on Windows) for short
names.

For example, an argument named `--path` could have a short name `-p`. It could then be supplied
using either name:

```text
--path value
```

Or:

```text
-p value
```

Note that you must use the correct prefix: using `-path` or `--p` will not work.

An argument can have either a short name or a long name, or both.

Arguments in this mode can still have aliases. You can set separate long and short aliases, which
follow the same rules as the long and short names.

For switch arguments with short names, the switches can be combined in a single argument. For
example, given the switches `-a`, `-b` and `-c`, the following command line sets all three switches:

```text
-abc
```

This only works for switch arguments, and does not apply to long names.

Besides these differences, long/short mode follows the same rules and conventions as the default
mode outlined above, with all the same options.

Long/short mode mimics the behavior of many tools like `dotnet` and `git` more closely. Usually,
those tools also use lower-case argument names, which you can easily achieve using
[name transformation](DefiningArguments.md#name-transformation).

## More information

Next, let's take a look at how to [define arguments](DefiningArguments.md).

[`AllowDuplicateDictionaryKeysAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_AllowDuplicateDictionaryKeysAttribute.htm
[`CommandLineArgument.AllowNull`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgument_AllowNull.htm
[`CommandLineArgumentException`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineArgumentException.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CultureInfo.InvariantCulture`]: https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo.invariantculture
[`CultureInfo`]: https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo
[`DateTime`]: https://learn.microsoft.com/dotnet/api/system.datetime
[`DayOfWeek.Monday`]: https://learn.microsoft.com/dotnet/api/system.dayofweek
[`DayOfWeek.Wednesday`]: https://learn.microsoft.com/dotnet/api/system.dayofweek
[`DayOfWeek`]: https://learn.microsoft.com/dotnet/api/system.dayofweek
[`FileInfo`]: https://learn.microsoft.com/dotnet/api/system.io.fileinfo
[`FlagsAttribute`]: https://learn.microsoft.com/dotnet/api/system.flagsattribute
[`Int32`]: https://learn.microsoft.com/dotnet/api/system.int32
[`KeyTypeConverterAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_KeyTypeConverterAttribute.htm
[`KeyValuePair<TKey, TValue>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.keyvaluepair-2
[`KeyValuePairConverter<TKey, TValue>`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_KeyValuePairConverter_2.htm
[`KeyValueSeparatorAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_KeyValueSeparatorAttribute.htm
[`MultiValueSeparatorAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_MultiValueSeparatorAttribute.htm
[`ParseOptions.AllowWhiteSpaceValueSeparator`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_ParseOptions_AllowWhiteSpaceValueSeparator.htm
[`ParseOptions.ArgumentNameComparer`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_ParseOptions_ArgumentNameComparer.htm
[`ParseOptions.Culture`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_ParseOptions_Culture.htm
[`ParseOptions.NameValueSeparator`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_ParseOptions_NameValueSeparator.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute.AllowWhiteSpaceValueSeparator`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_ParseOptionsAttribute_AllowWhiteSpaceValueSeparator.htm
[`ParseOptionsAttribute.CaseSensitive`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_ParseOptionsAttribute_CaseSensitive.htm
[`ParseOptionsAttribute.NameValueSeparator`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_ParseOptionsAttribute_NameValueSeparator.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`String`]: https://learn.microsoft.com/dotnet/api/system.string
[`System.ComponentModel.TypeConverterAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverterattribute
[`TypeConverter`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverter
[`TypeConverterAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverterattribute
[`Uri`]: https://learn.microsoft.com/dotnet/api/system.uri
[`ValueTypeConverterAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ValueTypeConverterAttribute.htm
[NullArgumentValue_0]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineArgumentErrorCategory.htm
