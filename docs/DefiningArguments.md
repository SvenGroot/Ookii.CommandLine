# Defining command line arguments

Now that you understand the [parsing rules](Arguments.md), let's see how to define arguments using
Ookii.CommandLine.

To define which arguments are accepted by your application, you create a class whose members specify
the names, types and other attributes (such as whether they're required or positional) of the
arguments.

The class itself has no special requirements, but will typically look like this.

```csharp
[GeneratedParser]
partial class Arguments
{
}
```

The use of the [`GeneratedParserAttribute`][] enables [source generation](SourceGeneration.md),
which has several advantages and should be used unless you cannot meet the requirements.

The class must have a public constructor with no parameters, or one that takes a single
[`CommandLineParser`][] parameters. If the latter is used, the [`CommandLineParser`][] instance that
was used to parse the arguments will be passed to the constructor.

There are two ways to define arguments in the class: using properties and using methods.

## Using properties

Properties are the most common way to define arguments. They can be used to create any type of
argument, and will be used for most arguments.

To indicate a property is an argument, apply the [`CommandLineArgumentAttribute`][] attribute to it.
The property must have a public getter and setter, except for multi-value and dictionary arguments
which can be defined by read-only properties.

The type of the property is used for the type of the argument, and the name of the property is used
as the argument name by default.

If not specified otherwise, the argument will be optional and not positional.

The below defines an argument with the name `-SomeArgument`. Its type is a
[`String`][], it's optional, can only be specified by name, and has no default value:

```csharp
[CommandLineArgument]
public string? SomeArgument { get; set; }
```

> All examples on this page assume you are using the default parsing mode (not long/short) and no
> name transformation, unless specified otherwise. With the [right options](#longshort-mode), this
> same property could also define an argument called `--some-argument`.

If you don't want to use the name of the property (and a [name transformation](#name-transformation)
is not appropriate), you can specify the name explicitly.

```csharp
[CommandLineArgument("OtherName")]
public string? SomeArgument { get; set; }
```

This creates an argument named `-OtherName`.

### Positional arguments

There are two ways to make an argument positional.

When using [source generation](SourceGeneration.md), you can use the [`CommandLineArgumentAttribute.IsPositional`][]
property. With this option, the arguments will have the same order as the members that define them.

```csharp
[CommandLineArgument(IsPositional = true)]
public string? SomeArgument { get; set; }

[CommandLineArgument(IsPositional = true)]
public int OtherArgument { get; set; }
```

Here, `-SomeArgument` will be the first positional argument, and `-OtherArgument` the second.

If not using source generation, you must instead set the [`CommandLineArgumentAttribute.Position`][]
property to a non-negative number. The numbers determine the order.

> Without source generation, reflection is used to determine the arguments, and reflection is not
> guaranteed to return the members of a type in any particular order, which is why the
> [`IsPositional`][] property is only supported when using source generation. The [`Position`][Position_1] property
> works with both source generation and reflection.

```csharp
[CommandLineArgument(Position = 0)]
public string? SomeArgument { get; set; }

[CommandLineArgument(Position = 1)]
public int OtherArgument { get; set; }
```

The [`CommandLineArgumentAttribute.Position`][] property specifies the relative position of the
arguments, not their actual position. Therefore, it's okay to skip numbers; only the order matters.
The order of the properties themselves does not matter in this case.

That means that this:

```csharp
[CommandLineArgument(Position = 0)]
public int Argument1 { get; set; }

[CommandLineArgument(Position = 1)]
public int Argument2 { get; set; }

[CommandLineArgument(Position = 2)]
public int Argument3 { get; set; }
```

Is equivalent to this (assuming there are no other positional arguments):

```csharp
[CommandLineArgument(Position = 123)]
public int Argument3 { get; set; }

[CommandLineArgument(Position = 57)]
public int Argument2 { get; set; }

[CommandLineArgument(Position = 10)]
public int Argument1 { get; set; }
```

And is also equivalent to this when using the [`GeneratedParserAttribute`][]:

```csharp
[CommandLineArgument(IsPositional = true)]
public int Argument1 { get; set; }

[CommandLineArgument(IsPositional = true)]
public int Argument2 { get; set; }

[CommandLineArgument(IsPositional = true)]
public int Argument3 { get; set; }
```

### Required arguments

To create a required argument, use a `required` property (.Net 7 and later only), or set the
[`CommandLineArgumentAttribute.IsRequired`][] property to true. It's recommended for required
properties to also be positional.

```csharp
[CommandLineArgument(IsPositional = true)]
public required string SomeArgument { get; set; }

[CommandLineArgument(IsPositional = true, IsRequired = true)]
public int OtherArgument { get; set; }
```

Here, both `-SomeArgument` and `-OtherArgument` are required and positional.

You cannot define a required positional argument after an optional positional argument, and a
multi-value positional argument must be the last positional argument. If your properties violate
these rules, you will get a compile time error when using source generation, and if not, the
[`CommandLineParser`][] class’s constructor will throw an exception.

### Switch arguments

If the type of an argument is a boolean, a nullable boolean, or an array of booleans, this defines a
switch argument (also known as a flag), unless the argument is positional.

```csharp
[CommandLineArgument]
public bool Switch { get; set; }
```

A nullable boolean type can be used to distinguish between an omitted switch and an explicit value
of false.

```csharp
[CommandLineArgument]
public bool? Switch { get; set; }
```

This property will be null if the argument was not supplied, true if the argument was present or
explicitly set to true with `-Switch:true`, and false only if the user supplied `-Switch:false`.

### Multi-value arguments

There are two ways to define multi-value arguments. The first is to use a read-write property of any
array type:

```csharp
[CommandLineArgument]
public string[]? MultiValue { get; set; }
```

Note that if no values are supplied, the property will not be set, so it can be null after parsing.
If the property has an initial non-null value, that value will be overwritten if the argument was
supplied.

The other option is to use a read-only property of any type implementing [`ICollection<T>`][] (e.g.
[`List<int>`][]). This requires that the property's value is not null, and items will be added to
the list after parsing has completed.

```csharp
[CommandLineArgument]
public List<string> AlsoMultiValue { get; } = new();
```

If you are _not_ using the [`GeneratedParserAttribute`][] attribute, using .Net 6.0 or later, and
using a read-only property like this, it is recommended to use [`ICollection<T>`][] as the type of
the property. Otherwise, [`CommandLineParser`][] will not be able to determine the
[nullability](Arguments.md#arguments-with-non-nullable-types) of the collection's elements. This
limitation does not apply to source generation.

A multi-value argument whose type is a boolean or a nullable boolean is both a switch and a
multi-value argument.

```csharp
[CommandLineArgument]
public bool[] Switch { get; set; }
```

A value of true, or the explicit value, will be added to the array for each time the argument is
supplied.

### Dictionary arguments

Similar to multi-value arguments, there are two ways to define dictionary arguments: a read-write
property of type [`Dictionary<TKey, TValue>`][], or a read-only property of any type implementing
[`IDictionary<TKey, TValue>`][].

When using a read-write property, the property value may be null if the argument was not supplied,
and will be overwritten with a new dictionary if the argument was supplied, just like multi-value
arguments.

```csharp
[CommandLineArgument]
public Dictionary<string, int>? Dictionary { get; set; }

[CommandLineArgument]
public SortedDictionary<string, int> AlsoDictionary { get; } = new();
```

As above, when using a read-only property and not using the [`GeneratedParserAttribute`][]
attribute, you should use either [`Dictionary<TKey, TValue>`][] or [`IDictionary<TKey, TValue>`][]
as the type of the property, otherwise the nullability of `TValue` cannot be determined.

### Default values

There are two ways to set default values for an optional argument. The first is to use a property
initializer:

```csharp
[CommandLineArgument]
public string SomeArgument { get; set; } = "default";
```

If the argument is not supplied, the property will have its initial value, which is "default" in
this case.

When using source generation, the value of the property initializer will be included in the
argument's description in the [usage help](UsageHelp.md) as long as the value is either a literal, a
constant, a property reference, or an enumeration value. Other types of initializers (such as a
`new` expression or a method call), will not have their value shown in the usage help.

> You can disable showing default values in the usage help if you do not want it.

Alternatively, you can specify the default value using the
[`CommandLineArgumentAttribute.DefaultValue`][] property.

```csharp
[CommandLineArgument(DefaultValue = 10)]
public int SomeArgument { get; set; }
```

The [`DefaultValue`][] property must use either the type of the argument, or a string that can be
converted to the argument type. This enables you to set a default value for types that don't have
literals.

```csharp
[CommandLineArgument(DefaultValue = "1969-07-20")]
public DateOnly SomeArgument { get; set; }
```

The value of the [`CommandLineArgumentAttribute.DefaultValue`][] property will be included in the
argument's description in the [usage help](UsageHelp.md). In this case, it will be included
regardless of whether you are using source generation.

Default values will be ignored if specified for a required argument or a multi-value or dictionary
argument.

### Argument descriptions

You can add a description to an argument with the [`System.ComponentModel.DescriptionAttribute`][]
attribute. These descriptions will be used for the [usage help](UsageHelp.md).

```csharp
[CommandLineArgument]
[Description("Provides the name of a file to read.")]
public FileInfo? Path { get; set; }
```

It's strongly recommended to always add descriptions to all your arguments.

If you wish to use a different source, like for example a resource table which can be localized, for
the descriptions, this can be accomplished by creating a class that derives from th
[`DescriptionAttribute`][] class.

### Value descriptions

The value description is a short, often one-word description of the type of values your argument
accepts. It's shown in the [usage help](UsageHelp.md) after the name of your argument, and defaults
to the name of the argument type (in the case of a multi-value argument, the element type, or for a
nullable value type, the underlying type). The unqualified framework type name is used, so for
example, an integer would have the default value description "Int32".

To specify a custom value description, use the [`ValueDescriptionAttribute`][] attribute.

```csharp
[CommandLineArgument]
[ValueDescriptionAttribute("Number")]
public int Argument { get; set; }
```

This should _not_ be used for the description of the argument's purpose; use the
[`DescriptionAttribute`][] for that.

### Custom type conversion

If you want to use a non-default conversion from string, you can specify a custom type converter
using the [`ArgumentConverterAttribute`][].

```csharp
[CommandLineArgument]
[ArgumentConverter(typeof(CustomConverter))]
public int Argument { get; set; }
```

The type specified must be derived from the [`ArgumentConverter`][] class.

To create a custom converter, create a class that derives from the [`ArgumentConverter`][] class.
Argument conversion can use either a [`ReadOnlySpan<char>`][] or a [`String`][], and it's recommended to
support the [`ReadOnlySpan<char>`][] method to avoid unnecessary string allocations.

Previous versions of Ookii.CommandLine used .Net's [`TypeConverter`][] class. Starting with
Ookii.CommandLine 4.0, this is no longer the case, and the [`ArgumentConverter`][] class is used
instead.

To help with transitioning code that relied on [`TypeConverter`][], you can use the
[`WrappedDefaultTypeConverter<T>`][] class to use a type's default type converter.

```csharp
[CommandLineArgument]
[ArgumentConverter(typeof(WrappedDefaultTypeConverter<SomeType>))]
public SomeType Argument { get; set; }
```

This will use [`TypeDescriptor.GetConverter()`][] function to get the default [`TypeConverter`][] for
the type. Note that using that function will make it impossible to trim your application; this is
the main reason [`TypeConverter`][] is no longer the default for converting arguments.

If you were using a custom [`TypeConverter`][], you can use the [`WrappedTypeConverter<T>`][] class.

### Arguments that cancel parsing

You can indicate that argument parsing should stop immediately when an argument is supplied by
setting the [`CommandLineArgumentAttribute.CancelParsing`][] property.

When this property is set to [`CancelMode.Abort`][], parsing is stopped when the argument is
encountered. The rest of the command line is not processed, and
[`CommandLineParser<T>.Parse()`][] will return null. The
[`ParseWithErrorHandling()`][ParseWithErrorHandling()_1] and the static [`Parse<T>()`][Parse<T>()_1]
helper methods will automatically print usage in this case.

This can be used, for example, to implement a custom `-Help` argument, if you don't wish to use the
default one.

```csharp
[CommandLineArgument(CancelParsing = CancelMode.Abort)]
public bool Help { get; set; }
```

Note that this property will never be set to true by the [`CommandLineParser`][], since no instance
will be created if the argument is supplied.

If you set the [`CancelParsing`][CancelParsing_1] property to [`CancelMode.Success`][], parsing is
stopped, and the rest of the command line is not processed, but parsing will complete successfully.
If all the required arguments have been specified before that point, the
[`CommandLineParser<T>.Parse()`][] method and various helper methods will return an instance of the
arguments type.

The remaining arguments that were not parsed are available in the [`ParseResult.RemainingArguments`][]
property. These are available for [`CancelMode.Abort`][], [`CancelMode.Success`][], and if parsing
encountered an error.

[`CancelMode.Success`][] can be used if you wish to pass the remaining arguments to another command
line processor, for example a child application, or a subcommand. See for example the
[top-level arguments sample](../src/Samples/TopLevelArguments).

The `--` argument can also be used to cancel parsing and return success, by setting the
[`ParseOptionsAttribute.PrefixTermination`][] or [`ParseOptions.PrefixTermination`][] property to
[`PrefixTerminationMode.CancelWithSuccess`][].

## Using methods

You can also apply the [`CommandLineArgumentAttribute`][] to a public static method. Method
arguments offer a way to take action immediately if an argument is supplied, without waiting for the
remaining arguments to be parsed.

The method must have one of the following signatures.

- `public static CancelMode Method(ArgumentType value, CommandLineParser parser);`
- `public static CancelMode Method(ArgumentType value);`
- `public static CancelMode Method(CommandLineParser parser);`
- `public static CancelMode Method();`
- `public static bool Method(ArgumentType value, CommandLineParser parser);`
- `public static bool Method(ArgumentType value);`
- `public static bool Method(CommandLineParser parser);`
- `public static bool Method();`
- `public static void Method(ArgumentType value, CommandLineParser parser);`
- `public static void Method(ArgumentType value);`
- `public static void Method(CommandLineParser parser);`
- `public static void Method();`

The method will be called immediately when the argument is supplied, unlike properties, which are
only set after all arguments have been parsed. This is why the method must be static; the instance
hasn't been created yet when the method is invoked.

The type of the `value` parameter is the type of the argument. If the method doesn't have a `value`
parameter, the argument will be a switch argument, and the method will be invoked when the argument
is supplied, even if its value is explicitly set to false (if you want to distinguish this, use
a `bool value` parameter).

Multi-value method arguments are not supported, so the type of the `value` parameter may not be an
array, collection or dictionary type, unless you provide an [`ArgumentConverter`][] that can convert
to that type.

If you use one of the signatures with a [`CancelMode`][] return type, returning [`CancelMode.Abort`][] or
[`CancelMode.Success`][] will immediately [cancel parsing](#arguments-that-cancel-parsing). Unlike the
[`CancelParsing`][CancelParsing_1] property, [`CancelMode.Abort`][] will _not_ automatically display
usage help. If you do want to show help, set the [`CommandLineParser.HelpRequested`][] property to
true before returning false.

```csharp
[CommandLineArgument]
public static CancelMode MoreHelp(CommandLineParser parser)
{
    Console.WriteLine("Some amazingly useful information.")
    parser.HelpRequested = true;
    return CancelMode.Abort;
}
```

When using a signature that returns `bool`, returning `true` is equivalent to [`CancelMode.None`][] and
`false` is equivalent to [`CancelMode.Abort`][].

Using a signature that returns `void` is equivalent to returning [`CancelMode.None`][].

Method arguments allow all the same customizations as property-defined arguments, except that the
[`DefaultValue`][DefaultValue_1] will not be used. The method will never be invoked if the argument
is not explicitly specified by the user.

## Applying parse options

You can set parse options when you use the [`CommandLineParser`][] class using the [`ParseOptions`][]
class, but you can also set many common options on the arguments class directly using the
[`ParseOptionsAttribute`][] class.

For example, the following disables the use of the `/` argument prefix on Windows, and always uses
only `-`.

```csharp
[GeneratedParser]
[ParseOptions(ArgumentNamesPrefixes = new[] { '-' })]
partial class Arguments
{
}
```

### Long/short mode

To enable [long/short mode](Arguments.md#longshort-mode), you typically want to set three options
if you want to mimic typical POSIX conventions: the mode itself, case sensitive argument names,
and dash-case [name transformation](#name-transformation). This can be done with either the
[`ParseOptionsAttribute`][] attribute or the [`ParseOptions`][] class.

A convenient [`IsPosix`][IsPosix_2] property is provided on either class, that sets all relevant options when
set to true.

When using long/short mode, the name derived from the member name, or the explicit name set by the
[`CommandLineArgumentAttribute`][] attribute is the long name.

To set a short name, set [`CommandLineArgumentAttribute.ShortName`][] property. Alternatively, you
can set the [`CommandLineArgumentAttribute.IsShort`][] property to true to use the first character
of the long name (after name transformation) as the short name.

You can disable the long name using the [`CommandLineArgumentAttribute.IsLong`][] property, in which
case the argument will only have a short name.

```csharp
[GeneratedParser]
[ParseOptions(IsPosix = true)]
partial class MyArguments
{
    [CommandLineArgument(IsPositional = true, IsShort = true)]
    public required string FileName { get; set }

    [CommandLineArgument(ShortName = 'F')]
    public int Foo { get; set;}

    [CommandLineArgument(IsShort = true, IsLong = false)]
    public bool Bar { get; set; }
}
```

Using `[ParseOptions(IsPosix = true)]` is equivalent to manually setting the following properties.

```csharp
[ParseOptions(Mode = ParsingMode.LongShort,
    CaseSensitive = true,
    ArgumentNameTransform = NameTransform.DashCase,
    ValueDescriptionNameTransform = NameTransform.DashCase)]
```

In this example, the `FileName` property defines a required positional argument with the long name
`--file-name` and the short name `-f`. The `Foo` property defines an argument with the long name
`--foo` and the explicit short name `-F`, which is distinct from `-f` because case sensitivity is
enabled. The `Bar` property defines an argument with the short name `-b`, but no long name. The
names are all lower case due to the name transformation.

## Defining aliases

An alias is an alternative name that can be used to specify a command line argument. Aliases can be
added to a command line argument by applying the [`AliasAttribute`][] to the property or method.

For example, the following code defines a switch argument that can be specified using either the
name `-Verbose` or the alias `-v`:

```csharp
[CommandLineArgument]
[Alias("v")]
public bool Verbose { get; set; }
```

To specify more than one alias for an argument, simply apply the [`AliasAttribute`][] multiple times.

When using [long/short mode](Arguments.md#longshort-mode), the [`AliasAttribute`][] specifies long
name aliases, and will be ignored if the argument doesn't have a long name. Use the
[`ShortAliasAttribute`][] to specify short aliases. These will be ignored if the argument doesn't
have a short name.

## Automatic prefix aliases

By default, Ookii.CommandLine will accept any prefix that uniquely identifies an argument by either
its name or one of its explicit aliases as an alias. For example, if you have an argument named
`-File`, it would be possible to specify it with `-F`, `-Fi`, and `-Fil`, as well as `-File`,
assuming none of those prefixes are ambiguous.

In the above example using the `-Verbose` argument, `-v` would be ambiguous between `-Verbose` and
the [automatic `-Version` argument](#automatic-arguments), so it would not work as an alias without
explicitly specifying it. However, `-Verb` would work as an automatic prefix alias for `-Verbose`,
because it is not ambiguous.

Automatic prefix aliases will not be shown in the [usage help](UsageHelp.md), so it can still be
useful to explicitly define an alias even if it's a prefix, if you wish to call more attention to it.

If you do not want to use automatic prefix aliases, set the [`ParseOptionsAttribute.AutoPrefixAliases`][]
or [`ParseOptions.AutoPrefixAliases`][] property to false.

## Name transformation

If your desired argument naming convention doesn't match your .Net naming convention, you can use
a name transformation to automatically change names that were derived from the property, method, or
parameter name.

The following transformations are available:

Value          | Description                                                                                                                                                                                                                                                  | Example
---------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------
**None**       | Member names are used as-is, without changing them. This is the default.                                                                                                                                                                                      |
**PascalCase** | Member names are transformed to PascalCase. This removes all underscores, and the first character and every character after an underscore is changed to uppercase. The case of other characters is not changed.                                               | `SomeName`, `someName`, `_someName_` => SomeName
**CamelCase**  | Member names are transformed to camelCase. Similar to PascalCase, but the first character will not be uppercase.                                                                                                                                             | `SomeName`, `someName`, `_someName_`=> someName
**SnakeCase**  | Member names are transformed to snake_case. This removes leading and trailing underscores, changes all characters to lower-case, and reduces consecutive underscores to a single underscore. An underscore is inserted before previously capitalized letters. | `SomeName`, `someName`, `_someName_` => some_name
**DashCase**   | Member names are transformed to dash-case. Similar to SnakeCase, but uses a dash instead of an underscore.                                                                                                                                                    | `SomeName`, `someName`, `_someName_` => some-name

Name transformations are set by using the [`ParseOptions.ArgumentNameTransform`][] property, or the [`ParseOptionsAttribute`][]
attribute.

```csharp
[GeneratedParser]
[ParseOptions(ArgumentNameTransform = NameTransform.DashCase)]
partial class Arguments
{
    [CommandLineArgument]
    public string? SomeArgument;

    [CommandLineArgument]
    public int OtherArgument;
}
```

This defines two arguments named `-some-argument` and `-other-argument`, without the need to specify
explicit names.

If you have an argument with an automatic short name when using [long/short mode](Arguments.md#longshort-mode),
name transformation is applied to the name before the short name is determined, so the case of the
short name will match the case of the first letter of the transformed long name.

## Automatic arguments

Besides the arguments you define in your class, the [`CommandLineParser`][] will, by default, add
two automatic arguments to your application: `-Help` and `-Version`.

The `-Help` argument will cancel parsing, and immediately show usage help. The `-Version` argument
will cancel parsing, show version information, but will not show usage help.

The automatic `-Help` argument has two aliases, `-?` and `-h`. The `-Version` argument doesn't have
any aliases.

When using [long/short mode](Arguments.md#longshort-mode), the `--Help` argument has the short name
`-?`, and a short alias `-h`, while the `--Version` argument has no short name.

If you use a name transformation, that transformation is also applied to the automatic argument
names. So with long/short mode and the dash-case transformation, you would have arguments named
`--help` and `--version`, all lower case.

The names and aliases of the automatic arguments can be customized using the
[`LocalizedStringProvider`][] class.

If your class defines an argument where the name or an alias matches the names or aliases of either
of the automatic arguments, that argument will not be automatically added. In addition, you can
disable either automatic argument using the [`ParseOptions`][] class.

Next, we'll take a look at how to [parse the arguments we've defined](ParsingArguments.md)

[`AliasAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_AliasAttribute.htm
[`ArgumentConverter`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_ArgumentConverter.htm
[`ArgumentConverterAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_ArgumentConverterAttribute.htm
[`CancelMode.Abort`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_CancelMode.htm
[`CancelMode.None`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_CancelMode.htm
[`CancelMode.Success`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_CancelMode.htm
[`CancelMode`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_CancelMode.htm
[`CommandLineArgumentAttribute.CancelParsing`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[`CommandLineArgumentAttribute.DefaultValue`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[`CommandLineArgumentAttribute.IsLong`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsLong.htm
[`CommandLineArgumentAttribute.IsPositional`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsPositional.htm
[`CommandLineArgumentAttribute.IsRequired`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsRequired.htm
[`CommandLineArgumentAttribute.IsShort`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsShort.htm
[`CommandLineArgumentAttribute.Position`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_Position.htm
[`CommandLineArgumentAttribute.ShortName`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_ShortName.htm
[`CommandLineArgumentAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_CommandLineArgumentAttribute.htm
[`CommandLineParser.HelpRequested`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineParser_HelpRequested.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandLineParser<T>.Parse()`]: https://www.ookii.org/docs/commandline-4.1/html/Overload_Ookii_CommandLine_CommandLineParser_1_Parse.htm
[`DefaultValue`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[`DescriptionAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute
[`Dictionary<TKey, TValue>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2
[`GeneratedParserAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_GeneratedParserAttribute.htm
[`ICollection<T>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.icollection-1
[`IDictionary<TKey, TValue>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2
[`IsPositional`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsPositional.htm
[`List<int>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1
[`LocalizedStringProvider`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_LocalizedStringProvider.htm
[`ParseOptions.ArgumentNameTransform`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptions_ArgumentNameTransform.htm
[`ParseOptions.AutoPrefixAliases`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptions_AutoPrefixAliases.htm
[`ParseOptions.PrefixTermination`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptions_PrefixTermination.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute.AutoPrefixAliases`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_AutoPrefixAliases.htm
[`ParseOptionsAttribute.PrefixTermination`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_PrefixTermination.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`ParseResult.RemainingArguments`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseResult_RemainingArguments.htm
[`PrefixTerminationMode.CancelWithSuccess`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_PrefixTerminationMode.htm
[`ReadOnlySpan<char>`]: https://learn.microsoft.com/dotnet/api/system.readonlyspan-1
[`ShortAliasAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_ShortAliasAttribute.htm
[`String`]: https://learn.microsoft.com/dotnet/api/system.string
[`System.ComponentModel.DescriptionAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute
[`TypeConverter`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverter
[`TypeDescriptor.GetConverter()`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typedescriptor.getconverter
[`ValueDescriptionAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_ValueDescriptionAttribute.htm
[`WrappedDefaultTypeConverter<T>`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_WrappedDefaultTypeConverter_1.htm
[`WrappedTypeConverter<T>`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Conversion_WrappedTypeConverter_1.htm
[CancelParsing_1]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[DefaultValue_1]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[IsPosix_2]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_IsPosix.htm
[Parse<T>()_1]: https://www.ookii.org/docs/commandline-4.1/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[ParseWithErrorHandling()_1]: https://www.ookii.org/docs/commandline-4.1/html/M_Ookii_CommandLine_CommandLineParser_1_ParseWithErrorHandling.htm
[Position_1]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_Position.htm
