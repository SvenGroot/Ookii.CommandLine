# Defining command line arguments

Now that you understand the [parsing rules](Arguments.md), let's see how to define arguments using
Ookii.CommandLine.

To define which arguments are accepted by your application, you create a class whose members specify
the names, types and other attributes (such as whether they're required or positional) of the
arguments.

There are three ways to define arguments in the class: using properties, methods, and constructor
parameters.

## Using properties

Properties are the most flexible way to define arguments. They can be used to create any type of
argument, and lend themselves well to using attributes without the code becoming cluttered.

To indicate a property is an argument, apply the [`CommandLineArgumentAttribute`] attribute to it.
The property must have a public getter and setter, except for multi-value and dictionary arguments
which can be defined by read-only properties.

The type of the property is used for the type of the argument, and the name of the property is used
as the argument name by default.

If not specified otherwise, a property defines an optional and not positional.

The below defines an argument with the name `-SomeArgument`. Its type is a
[`String`][], it's optional, can only be specified by name, and has no default value:

```csharp
[CommandLineArgument]
public string? SomeArgument { get; set; }
```

> All examples on this page assume you are using the default parsing mode (not long/short) and no
> name transformation, unless specified otherwise.

If you don't want to use the name of the property (and a [name transformation](#name-transformation))
is not appropriate), you can specify the name explicitly.

```csharp
[CommandLineArgument("OtherName")]
public string? SomeArgument { get; set; }
```

This creates an argument named `-OtherName`.

### Required and positional arguments

To create a required argument, set the [`CommandLineArgumentAttribute.IsRequired`][] property to
true. To create a positional argument, set the [`CommandLineArgumentAttribute.Position`][] property
to a non-negative number.

```csharp
[CommandLineArgument(Position = 0, IsRequired = true)]
public int OtherArgument { get; set; }
```

This defines a required positional argument named `-OtherArgument`.

The [`CommandLineArgumentAttribute.Position`][] property specifies the relative position of the
arguments, not their actual position. Therefore, it's okay to skip numbers; only the order matters.
The order of the properties themselves does not matter.

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

You cannot define a required positional argument after an optional positional argument, and a
multi-value positional argument must be the last positional argument. If your properties violate
these rules, the [`CommandLineParser`][] class’s constructor will throw an exception.

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

There are two ways to define multi-value arguments using properties. The first is to use a
read-write property of any array type:

```csharp
[CommandLineArgument]
public string[]? MultiValue { get; set; }
```

Note that if no values are supplied, the property will not be set, so it can be null after parsing.
If the property has an initial non-null value, that value will be overwritten if the argument was
supplied.

The other option is to a read-only property of any type implementing [`ICollection<T>`][] (e.g.
[`List<int>`][]). This requires that the property's value is not null, and items will be added to
the list after parsing has completed.

```csharp
[CommandLineArgument]
public ICollection<string> AlsoMultiValue { get; } = new List<string>();
```

It is possible to use [`List<string>`][] (or any other type implementing [`ICollection<T>`][]) as
the type of the property itself, but, if using .Net 6.0 or later, [`CommandLineParser`][] can only
determine the [nullability](Arguments.md#arguments-with-non-nullable-types) of the collection's
elements if the property type is either an array or [`ICollection<T>`][] itself. This limitation
does not apply if [source generation](SourceGeneration.md) is used.

### Dictionary arguments

Similar to array arguments, there are two ways to define dictionary arguments: a read-write property
of type [`Dictionary<TKey, TValue>`][], or a read-only property of any type implementing
[`IDictionary<TKey, TValue>`][].

When using a read-write property, the property value may be null if the argument was not supplied,
and will be overwritten with a new dictionary if the argument was supplied, just like multi-value
arguments.

```csharp
[CommandLineArgument]
public Dictionary<string, int>? Dictionary { get; set; }

[CommandLineArgument]
public IDictionary<string, int> AlsoDictionary { get; } = new SortedDictionary<string, int>();
```

As above, it is possible to use the actual type (in this case, [`SortedDictionary<string, int>`][]) as
the property type for the second case, but nullability for the dictionary values can only be
determined if the type is [`Dictionary<TKey, TValue>`][] or [`IDictionary<TKey, TValue>`][].

### Default values

For an optional argument, you can specify the default value using the
[`CommandLineArgumentAttribute.DefaultValue`][] property.

```csharp
[CommandLineArgument(DefaultValue = 10)]
public int SomeArgument { get; set; }
```

The default value must be either the type of the argument, or a type that can be converted to the
argument type. Since all argument types must be convertible from a string, this enables you to use
strings for types that don't have literals.

```csharp
[CommandLineArgument(DefaultValue = "1969-07-20")]
public DateOnly SomeArgument { get; set; }
```

The default value is used if an optional argument is not supplied; in that case
[`CommandLineParser`][] will set the property to the specified default value.

The value of the [`CommandLineArgumentAttribute.DefaultValue`][] property will be included in the
argument's description in the [usage help](UsageHelp.md) by default, so you don't need to manually
duplicate the value in your description.

If no default value is specified (the value is null), the [`CommandLineParser`][] will never set the
property if the argument was not supplied. This means that if you initialized the property to some
value, this value will not be changed.

```csharp
[CommandLineArgument]
public string SomeProperty { get; set; } = "default";
```

Here, the property's value will remain "default" if the argument was not specified. This can be
useful if the argument uses a [non-nullable reference type](Arguments.md#arguments-with-non-nullable-types),
which must be initialized with a non-null value.

When using this method, the property's initial value will not be included in the usage help, so you
must include it manually if desired.

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
nullable value type, the underlying type).

To specify a custom value description, use the [`CommandLineArgumentAttribute.ValueDescription`][]
property.

```csharp
[CommandLineArgument(ValueDescription = "Number")]
public int Argument { get; set; }
```

This should *not* be used for the description of the argument's purpose; use the
[`DescriptionAttribute`][] for that.

### Custom type conversion

If you want to use a non-default conversion from string, you can specify a custom type converter
using the [`TypeConverterAttribute`][].

```csharp
[CommandLineArgument]
[TypeConverter(typeof(CustomConverter))]
public int Argument { get; set; }
```

The type specified must be derived from the [`TypeConverter`][] class.

To make it easy to implement custom type converters to/from a string, Ookii.CommandLine provides
the [`TypeConverterBase<T>`][] type.

### Arguments that cancel parsing

You can indicate that argument parsing should stop and immediately print usage help when an argument
is supplied by setting the [`CommandLineArgumentAttribute.CancelParsing`][] property to true.

When this property is set, parsing is stopped when the argument is encountered. The rest of the
command line is not processed, and [`CommandLineParser.Parse()`][CommandLineParser.Parse()_2] will
return null. The [`ParseWithErrorHandling()`][ParseWithErrorHandling()_1] and the static [`Parse<T>()`][Parse<T>()_1] helper
methods will automatically print usage in this case.

This can be used to implement a custom `-Help` argument, if you don't wish to use the default one.

```csharp
[CommandLineArgument(CancelParsing = true)]
public bool Help { get; set; }
```

Note that this property will never be set to true by the [`CommandLineParser`][], since no instance
will be created if the argument is supplied.

## Using methods

You can also apply the [`CommandLineArgumentAttribute`][] to a method. Method arguments offer a way
to take action immediately if an argument is supplied, without waiting for the remaining arguments
to be parsed.

The method must have one of the following signatures.

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
is supplied, even if its value is explicitly set to false.

Multi-value method arguments are not supported, so the type of the `value` parameter may not be an
array, collection or dictionary type.

If you use one of the signatures with a `bool` return type, returning false will cancel parsing.
Unlike the [`CancelParsing`][CancelParsing_1] property, this will *not* automatically display usage
help. If you do want to show help, set the [`CommandLineParser.HelpRequested`][] property to true
before returning false.

```csharp
[CommandLineArgument]
public static bool MoreHelp(CommandLineParser parser)
{
    Console.WriteLine("Some amazingly useful information.")
    parser.HelpRequested = true;
    return false;
}
```

Method arguments allow all the same customizations as property-defined arguments, except that the
[`DefaultValue`][DefaultValue_1] will not be used. The method will never be invoked if the argument is not explicitly
specified by the user.

## Using constructor parameters

An alternative way to define arguments is using a public constructor on your arguments class. These
arguments will be positional arguments, and required unless the constructor parameter is optional.

The following creates a required positional argument named `-arg1`, a required positional argument
named `-arg2`, and an optional positional argument named `-arg3`, with a default value of 0 (which
will be included in the usage help).

```csharp
public class MyArguments
{
    public MyArguments(string arg1, int arg2, float arg3 = 0f)
    {
        /* ... */
    }
}
```

Arguments defined by constructor parameters will always be positional, with their order matching the
order of the parameters. If there are properties defining positional arguments, those will always
come after the arguments defined by the constructor.

```csharp
public class MyArguments
{
    public MyArguments(string arg1, int arg2, float arg3 = 0f)
    {
        /* ... */
    }

    [CommandLineArgument(Position = 0)]
    public int PropertyArg { get; set; }
}
```

In this case, `-PropertyArg` will be the fourth positional argument.

You cannot use the [`CommandLineArgumentAttribute`] on a constructor parameter, so things that
are normally specified this way are specified using other attributes. The [`ArgumentNameAttribute`][]
is used if you want an argument name different than the parameter name. It can also be used to set
the short name for [long/short mode](Arguments.md#longshort-mode).

The [`ValueDescriptionAttribute`][] is used to set a custom value description, and full descriptions
are still set using the [`DescriptionAttribute`][].

```csharp
public MyArguments(
    [ArgumentName("Count", IsShort = true)]
    [ValueDescription("Number")],
    [Description("Provides a count to the application.")]
    int count)
{
    /* ... */
}
```

As you can see, it becomes rather awkward to use all these attributes on constructor parameters,
which is why using properties is typically recommended.

If your type has more than one constructor, you must mark one of them using the
[`CommandLineConstructorAttribute`][] attribute. You don’t need to use this attribute if you have
only one constructor.

If you don’t wish to define arguments using the constructor, simply use a constructor without any
parameters (or don’t define an explicit constructor).

If you follow .Net coding conventions, property names will be PascalCase and parameter names will be
camelCase. If you use both to define arguments, and rely on automatically determined names, this
causes inconsistent naming for your arguments. You can fix this by specifying explicit names for
either type of argument, or by using a [name transformation](#name-transformation) to make all
automatic names consistent.

### Nullable reference types

One area where constructor parameters offer an advantage is when using non-nullable reference types.

If you use a a property to define an argument whose type is a non-nullable reference type, the C#
compiler requires you to initialize it to a non-null value.

```csharp
[CommandLineArgument(Position = 0, IsRequired = true)]
public string SomeArgument { get; set; } = string.Empty;
```

The compiler requires the initialization in this example, even though the argument is requires and
the initial value will therefore always be replaced by the [`CommandLineParser`][], unless you
instantiate the class manually without using [`CommandLineParser`][].

Constructor parameters provide a way to use a non-nullable reference type without requiring the
unnecessary initialization:

```csharp
public MyArguments(string someArgument)
{
    SomeArgument = someArgument;
}

public string SomeArgument { get; }
```

In this case, initialization is performed by the constructor, and (if using .Net 6.0 or later),
the [`CommandLineParser`][] class guarantees it will never pass a non-null value to the constructor
if the type is not nullable.

### CommandLineParser injection

If your constructor has a parameter whose type is [`CommandLineParser`][], this does not define an
argument. Instead, this property will be set to the [`CommandLineParser`][] instance that was used
to parse the arguments. This is useful if you want to access the [`CommandLineParser`][] instance
after parsing for whatever reason (for example, to see which alias was used to specify a particular
argument), but still want to use the static [`Parse<T>()`][Parse<T>()_1] method for automatic error
and usage help handling.

Using [`CommandLineParser`][] injection can be used by itself, or combined with other parameters that
define arguments.

```csharp
public MyArguments(CommandLineParser parser, string argument)
{
}
```

## Long/short mode

To enable [long/short mode](Arguments.md#longshort-mode), you typically want to set three options
if you want to mimic typical POSIX conventions: the mode itself, case sensitive argument names,
and dash-case [name transformation](#name-transformation). This can be done with either the
[`ParseOptionsAttribute`][] attribute or the [`ParseOptions`][] class.

When using long/short mode, the name derived from the member or constructor parameter name, or the
explicit name set by the [`CommandLineArgumentAttribute`][] or [`ArgumentNameAttribute`][] attribute
is the long name.

To set a short name, set [`CommandLineArgumentAttribute.ShortName`][] property. Alternatively, you
can set the [`CommandLineArgumentAttribute.IsShort`][] property to true to use the first character
of the long name (after name transformation) as the short name. For constructor parameters, you use
the [`ArgumentNameAttribute.IsShort`][] and [`ArgumentNameAttribute.ShortName`][] properties for this
purpose.

You can disable the long name using the [`CommandLineArgumentAttribute.IsLong`][] or
[`ArgumentNameAttribute.IsLong`][] property, in which case the argument will only have a short name.

```csharp
[ParseOptions(Mode = ParsingMode.LongShort,
    CaseSensitive = true,
    ArgumentNameTransform = NameTransform.DashCase,
    ValueDescriptionNameTransform = NameTransform.DashCase)]
class MyArguments
{
    public MyArguments([ArgumentName(IsShort = true)] string fileName)
    {
        FileName = fileName;
    }

    public string FileName { get; }

    [CommandLineArgument(ShortName = 'F')]
    public int Foo { get; set;}

    [CommandLineArgument(IsShort = true, IsLong = false)]
    public bool Bar { get; set; }
}
```

In this example, the `fileName` constructor parameter defines an argument with the long name
`--file-name` and the short name `-f`. The `Foo` property defines an argument with the long name
`--foo` and the explicit short name `-F`, which is distinct from `-f` because case sensitivity is
enabled. The `Bar` property defines an argument with the short name `-b`, but no long name. The
names are all lower case due to the name transformation.

## Defining aliases

An alias is an alternative name that can be used to specify a command line argument. Aliases can be
added to a command line argument by applying the [`AliasAttribute`][] to the property, method, or
constructor parameter that defines the argument.

For example, the following code defines a switch argument that can be specified using either the
name `-Verbose` or the alias `-v`:

```csharp
[CommandLineArgument]
[Alias("v")]
public bool Verbose { get; set; }
```

To specify more than one alias for an argument, simply apply the [`AliasAttribute`][] multiple times.

When using [long/short mode](Arguments.md#longshort-mode), the [`AliasAttribute`][] specifies long name
aliases, and will be ignored if the argument doesn't have a long name. Use the [`ShortAliasAttribute`][]
to specify short aliases. These will be ignored if the argument doesn't have a short name.

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

Name transformations are set by using the [`ParseOptions.ArgumentNameTransform`][] property, or the [`ParseOptionsAttribute`][] which
can be applied to your arguments class.

```csharp
[ParseOptions(ArgumentNameTransform = NameTransform.DashCase)]
class Arguments
{
    [CommandLineArgument]
    public string? SomeArgument;

    [CommandLineArgument]
    public int OtherArgument;
}
```

This defines two arguments named `-some-argument` and `-other-argument`, without the need to specify
explicit names.

This can be useful if you combine constructor parameters and properties to define arguments.

```csharp
[ParseOptions(ArgumentNameTransform = NameTransform.PascalCase)]
class Arguments
{
    public Arguments(string someArgument)
    {
    }

    [CommandLineArgument]
    public int OtherArgument;
}
```

In this case the constructor-defined argument name will be `-SomeArgument`, consistent with the
property-defined argument `-OtherArgument`, without needing to use explicit names.

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

If your class defined an argument with the a name or alias matching the names or aliases of either
of the automatic arguments, that argument will not be automatically added. In addition, you can
disable either automatic argument using the [`ParseOptions`][].

Next, we'll take a look at how to [parse the arguments we've defined](ParsingArguments.md)

[`AliasAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_AliasAttribute.htm
[`ArgumentNameAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ArgumentNameAttribute.htm
[`ArgumentNameAttribute.IsLong`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_ArgumentNameAttribute_IsLong.htm
[`ArgumentNameAttribute.IsShort`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_ArgumentNameAttribute_IsShort.htm
[`ArgumentNameAttribute.ShortName`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_ArgumentNameAttribute_ShortName.htm
[`CommandLineArgumentAttribute.CancelParsing`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[`CommandLineArgumentAttribute.DefaultValue`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[`CommandLineArgumentAttribute.IsLong`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsLong.htm
[`CommandLineArgumentAttribute.IsRequired`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsRequired.htm
[`CommandLineArgumentAttribute.IsShort`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsShort.htm
[`CommandLineArgumentAttribute.Position`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_Position.htm
[`CommandLineArgumentAttribute.ShortName`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_ShortName.htm
[`CommandLineArgumentAttribute.ValueDescription`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_ValueDescription.htm
[`CommandLineArgumentAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_CommandLineArgumentAttribute.htm
[`CommandLineConstructorAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_CommandLineConstructorAttribute.htm
[`CommandLineParser.HelpRequested`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineParser_HelpRequested.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_CommandLineParser.htm
[`DescriptionAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute
[`Dictionary<TKey, TValue>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2
[`ICollection<T>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.icollection-1
[`IDictionary<TKey, TValue>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2
[`List<int>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1
[`List<string>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1
[`LocalizedStringProvider`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_LocalizedStringProvider.htm
[`ParseOptions.ArgumentNameTransform`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_ParseOptions_ArgumentNameTransform.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`ShortAliasAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ShortAliasAttribute.htm
[`SortedDictionary<string, int>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.sorteddictionary-2
[`String`]: https://learn.microsoft.com/dotnet/api/system.string
[`System.ComponentModel.DescriptionAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute
[`TypeConverter`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverter
[`TypeConverterAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverterattribute
[`TypeConverterBase<T>`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_TypeConverterBase_1.htm
[`ValueDescriptionAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ValueDescriptionAttribute.htm
[CancelParsing_1]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[CommandLineParser.Parse()_2]: https://www.ookii.org/docs/commandline-3.1/html/Overload_Ookii_CommandLine_CommandLineParser_Parse.htm
[DefaultValue_1]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[Parse<T>()_1]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm

[ParseWithErrorHandling()_1]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_CommandLineParser_1_ParseWithErrorHandling.htm
