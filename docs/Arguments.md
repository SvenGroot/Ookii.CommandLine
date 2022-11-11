# Command line arguments

Command line arguments are passed to your application when it is started, and are typically accessed through the parameter of the `static void Main(string[] args)` method (`Shared Sub Main(ByVal args() As String)` in Visual Basic). This provides the arguments as an array of strings, which is not terribly useful. What Ookii.CommandLine allows you to do is to convert that array of strings into a strongly typed set of named values, which are stored in the properties of the class that was used to define the arguments.

The method used to extract values from the array of string arguments is determined by the command line argument parsing rules. Ookii.CommandLine uses parsing rules that are very similar to how Microsoft PowerShell parses arguments for cmdlets, so if you have used PowerShell these rules will be familiar to you.

Command line arguments follow the name of your application on the command prompt, and typically take the following form:

    -ArgumentName ArgumentValue

The argument name is preceded by the _argument name prefix_. This prefix is configurable, but Ookii.CommandLine defaults to accepting a forward slash (`/`) and a dash (`-`) on Windows, and only a dash (`-`) on other platforms (other platforms are supported through .Net Core and Mono).

The argument value follows the name, separated either by a space or a colon (`:`). You can configure how argument names and values
can be separated by using the `CommandLineParser.AllowWhiteSpaceValueSeparator` and the `CommandLineParser.NameValueSeparator`
properties.

Not all arguments require values; those that do not are called _switch arguments_ and have a value determined by their presence or absence on the command line.

An argument can have one or more aliases: alternative names that can also be used to specify the command name. For example, a parameter named “Verbose” might use the alias “v” as a shorter to type alternative.

## Positional arguments

An argument can be _positional_, which means that its value can be specified either by name as indicated above, or by position. In this case the name of the argument is not required, and the argument’s value can be supplied by specifying it in the correct position in relation to other positional arguments.

If an argument value is encountered without being preceded by a name, it is matched to the positional argument at the current position. For example, take the following command line arguments:

    Value1 –ArgumentName Value2 Value3

In this case, Value1 is not preceded by a name; therefore it is matched to the first positional argument. Value2 follows a name, so it is matched to the argument with the name “ArgumentName”. Finally, Value3 is matched to the second positional argument.

A positional argument can still be supplied by explicitly supplying its name. If a positional argument is supplied by name, it cannot also be specified by position; in the previous example, if the argument named “ArgumentName” was the second positional argument, then Value3 becomes the value for the third positional argument, because the value for “ArgumentName” was already specified by name.

## Required arguments

A command line argument that is required must be present on all invocations of the application. If a required argument is not present, the `CommandLineParser` class will throw an exception during parsing.

Any argument can be made required. Usually it is recommended for any required argument to also be a positional argument, but this is not mandatory.

For positional arguments, required arguments must always come before optional arguments; it is an error to define a required positional argument after an optional positional argument.

## Switch arguments

A switch argument is an argument with a Boolean type (`bool` in C#). Its value is determined by its presence or absence on the command line; the value will be true if the argument is supplied, and false if not. The following arguments set the switch argument named “Switch” to true:

    -Switch

A switch argument’s value can be specified explicitly, as in the following example:

    -Switch:true

You must use a colon (or your custom name-value separator if configured) to specify an explicit value for a switch argument; you cannot use white space to separate the name and the value.

If you use a nullable Boolean type (`bool?` in C#) as the type of the argument, it will be null if omitted, true if supplied, and false only if explicitly set to false using `-Switch:false`.

## Arguments with multiple values

Some arguments can take multiple values; these are _multi-value arguments_, also known as _array arguments_. These arguments can be supplied multiple times, and each value is added to the set of values. For example, consider the following command line arguments:

    -ArgumentName Value1 –ArgumentName Value2 –ArgumentName Value3

In this case, the value of the argument named “ArgumentName” will be a list holding all three values.

The type of a multi-value argument must be an array type, for example `string[]` or `int[]` in C#, or any type implementing `ICollection<T>` when the argument is defined by a read-only property.

It’s possible to specify a separator for multi-value arguments using the `MultiValueSeparatorAttribute` attribute. This makes it possible to specify multiple values for the argument while the argument itself is specified only once. For example, if the separator is set to a comma, you can specify the values as follows:

    -ArgumentName Value1,Value2,Value3

In this case, the value of the argument named “ArgumentName” will be a list with the three values “Value1”, “Value2” and “Value3”.

**Note:** if you specify a separator for a multi-value argument, it is _not_ possible to have an argument value containing the separator. There is no way to escape the separator. Therefore, make sure you pick a separator that will never be used in the argument values, and be extra careful with culture-sensitive argument types (for example, if you use a comma as the separator for a multi-value argument of floating point numbers, cultures that use a comma as the decimal separator will not be able to specify values properly).

If an argument is not a multi-value argument, it is an error to supply it more than once, unless duplicate arguments are allowed in which case only the last value is used.

If a multi-value argument is positional, it must be the last positional argument. All remaining positional argument values will be considered values for the multi-value argument.

If a multi-value argument is required, it means it must have at least one value.

If the type of the argument is an array of Boolean values (`bool[]`), it will act as a multi-value argument and a switch. A value of true (or the explicit value if one is given) gets added to the array for every time that the argument is supplied.

## Dictionary arguments

Dictionary arguments are multi-value arguments that specify a set of key/value pairs. Each value for a dictionary argument takes the form key=value, like in the following example:

    -ArgumentName Key1=Value1 –ArgumentName Key2=Value2

In this case, the value of the argument named “ArgumentName” will be a dictionary with two keys, Key1 and Key2, with the associated values Value1 and Value2 respectively.

A dictionary argument must have a type of `Dictionary<TKey, TValue>` where TKey and TValue are the types of the key and value. When using a read-only property to define the argument, you can use any type that implements `IDictionary<TKey, TValue>`.

If you specify the same key more than once an exception will be thrown unless the `AllowDuplicateDictionaryKeysAttribute` attribute is specified on the constructor parameter or property that defines the dictionary argument.

The default key/value separator (which is '=') can be overridden using the `KeyValueSeparatorAttribute` attribute.

## Argument value conversion

Ookii.CommandLine allows you to define arguments with any .Net type, including types such as `System.String`, `System.Int32`, `System.DateTime`, and many more. Any type can be used; the only requirement is that it is possible to convert a string value to that type.

The .Net Framework provides a very flexible method for converting one type to another through the `System.ComponentModel.TypeConverter` class. You can use any type that has a type converter than can convert from a string for a command line argument. Most built-in types in .Net Framework have such a type converter. You can also use your own types by creating a type converter for that type.

It is possible to override the default conversion by specifying a custom type converter using the `System.ComponentModel.TypeConverterAttribute`. When this attribute is applied to a constructor parameter or property that defines an argument, the specified type converter will be used for conversion instead. Note that for multi-value, dictionary and nullable value-type arguments the converter must be for the element type (e.g. if the argument is a multi-value argument of type `int[]`, the type converter must be able to convert to `int`). For a dictionary argument the element type is `KeyValuePair<TKey, TValue>`, and the type converter is responsible for parsing the key and value from the argument value.

For a dictionary argument, instead of creating a custom `TypeConverter` that parses into a `KeyValuePair<TKey, TValue>`, you can
also customize conversion of the key and/or value alone by specifying the `KeyTypeConverterAttribute` and/or the
`ValueTypeConverterAttribute` respectively. This is the recommended way of customizing type conversion for dictionary arguments.

If you do specify the `TypeConverterAttribute` for a dictionary argument, the `KeyTypeConverterAttribute`, `ValueTypeConverterAttribute`,
and `KeyValueSeparatorAttribute` arguments will be ignored.

For many types, the conversion can be culture dependent. For example, converting numbers or dates depends on the culture which defines the accepted formats and how they’re interpreted; some cultures might use a period as the decimal separators, while others use a comma.

The culture used for argument value conversions is specified the `CommandLineParser.Culture` property, which defaults to the current culture. If you wish your argument parsing to be independent of the user’s culture, set this property to `System.Globalization.CultureInfo.InvariantCulture`.

## Arguments with non-nullable types

Ookii.CommandLine provides support for Nullable Reference Types. Not only is the library itself fully annotated (if using
the .Net 6.0 version of the library), but command line argument parsing takes into account the nullability of the properties or
parameters that define the arguments. If the argument is declared with a nullable reference or value type (e.g. `string?` or `int?`),
nothing changes. But if the argument is not nullable (e.g. `string` (in a context with NRT support) or `int`), `CommandLineParser`
will ensure that the value will not be null.

Assigning a null value to an argument only happens if the `TypeConverter` for that argument returns `null` as the result of the
conversion. If this happens and the argument is not nullable, a `CommandLineArgumentException` is thrown with the category set to
`NullArgumentValue`.

Null-checking for non-nullable reference types is only available in .Net 6.0 and later. If you are using the .Net Framework 2.0
or .Net Standard 2.0 version of Ookii.CommandLine, this check is only done for value types.

For multi-value arguments, the nullability check applies to the type of the elements (e.g. `string?[]` for an array),
and for dictionary arguments, it applies to the value (e.g. `Dictionary<string, string?>`); the key may never be null for a
dictionary argument.

See also the [API documentation](https://www.ookii.org/Link/CommandLineDoc) for the `CommandLineArgument.AllowNull` property.

## Long/short mode

TODO

* [Defining Command Line Arguments](DefiningArguments.md)
* [Parsing Command Line Arguments](Parsing%20Command%20Line%20Arguments.md)
* [Generating Usage Help](UsageHelp.md)
