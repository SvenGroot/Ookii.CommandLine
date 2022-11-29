# Defining command line arguments

In order to use Ookii.CommandLine, you must create a class that defines the arguments accepted by
your application. This type will specify the names, types and attributes (required, positional,
etc.) of each argument.

There are two ways to define arguments: using the properties of the class, or using constructor
parameters for the class.

## Using properties

The preferred way to define arguments is by using properties. A property defines an argument only
when it has the [`CommandLineArgumentAttribute`][] attribute applied to it. The property must have a
public getter and setter, except for multi-value and dictionary arguments which can be defined by
read-only properties under certain conditions.

The type of the argument is the type of the property, and the name of the argument matches the
property name by default, but this can be overridden using the [`CommandLineArgumentAttribute`][]
constructor.

An argument defined by a property is by default optional and not positional. Its default value can
be set using the [`CommandLineArgumentAttribute.DefaultValue`][] property.

The below defines an argument with the name "SomeArgument". Its type is a
`string`, it's optional, and it can only be specified by name:

```csharp
[CommandLineArgument(DefaultValue = "default")]
public string? SomeArgument { get; set; }
```

To create a required argument, set the [`CommandLineArgumentAttribute.IsRequired`][] property to true.

To create a positional argument, set the [`CommandLineArgumentAttribute.Position`][] property to a
non-negative number. This property determines the relative ordering of the positional arguments
only, not their actual position, so it’s fine if you skip numbers. Positional arguments defined by
properties come after arguments defined by constructor parameters, so for example if there are three
constructor parameters, the property with the lowest position value will be the fourth positional
argument.

You cannot have required positional arguments after optional ones, and a multi-value positional
argument must be the last positional argument. If your properties violate these rules, the
[`CommandLineParser`][] class’s constructor will throw an exception.

The following defines a required, positional argument with the explicit name "OtherName":

```csharp
[CommandLineArgument("OtherName", Position = 0, IsRequired = true)]
public int OtherArgument { get; set; }
```

If the type of an argument is a Boolean, a nullable Boolean, or an array of Booleans, this defines a
switch argument, unless the argument is positional.

```csharp
[CommandLineArgument]
public bool Switch { get; set; }
```

To define a multi-value argument, you can use either a read-write property of an array type (e.g.
`int[]`) or a read-only property of any type implementing [`ICollection<T>`][] (e.g. [`List<int>`][]). If
the property is read-only, it must not return a null value.

```csharp
[CommandLineArgument]
public string[]? MultiValue { get; set; }

[CommandLineArgument]
public ICollection<string> AlsoMultiValue { get; } = new List<string>();
```

It is possible to use [`List<string>`][] (or any other type implementing [`ICollection<T>`][]) as the type
of the property for the second argument, but, if using .Net 6.0 or later, [`CommandLineParser`][] can
only determine the [nullability](Arguments.md#arguments-with-non-nullable-types) of the collection's
elements if the property type is either an array or [`ICollection<T>`][] itself.

To define a dictionary argument, you can use either a read-write property of type `Dictionary<TKey, TValue>`
or a read-only property of any type implementing `IDictionary<TKey, TValue>`.

Consider the following properties:

```csharp
[CommandLineArgument]
public Dictionary<string, int>? Dictionary { get; set; }

[CommandLineArgument]
public IDictionary<string, int> AlsoDictionary { get; } = new SortedDictionary<string, int>();
```

As above, it is possible to use `SortedDictionary<string, int>` as the property type, but
nullability for the dictionary values can only be determined if the type is `Dictionary<TKey, TValue>`
or `IDictionary<TKey, TValue>`.

### Default values

If the default value is specified using the [`CommandLineArgumentAttribute.DefaultValue`][] property, it
must either match the type of the property, or be a type that the argument's [`TypeConverter`][] can
convert from.

Default argument values set by the [`CommandLineArgumentAttribute.DefaultValue`][] property are applied
only if the argument is not required, it was not specified on the command line, and the default
value is not `null`.

If the default value is `null`, the [`CommandLineParser`][] will not set the property even if the
argument was not specified. This enables you to use property initialization as an alternative way to
specify default values:

```csharp
[CommandLineArgument]
public string SomeProperty { get; set; } = "default";
```

Here, the value “default” will not be changed if the argument was not specified. This is
particularly useful if the argument uses a [non-nullable reference type](Arguments.md#arguments-with-non-nullable-types),
which must be initialized with a non-null value.

However, if this method is used, the default value will not be included in the usage description.
Therefore, it's preferable to use the [`CommandLineArgumentAttribute.DefaultValue`][] property when
possible.

### Argument descriptions

You can add a description to an argument with the [`System.ComponentModel.DescriptionAttribute`][]
attribute. These descriptions will be used for the [usage help](UsageHelp.md).

```csharp
[CommandLineArgument]
[Description("Provides the name of a file to read.")]
public FileInfo? Path { get; set; }
```

It's strongly recommended to always add descriptions to all your arguments.

### Value descriptions

The value description is a short, often one-word description of the type of values your argument
accepts. It defaults to the name of the argument type (in the case of a multi-value argument,
the element type, or a nullable value type, the underlying type). To specify a custom value
description, use the [`CommandLineArgumentAttribute.ValueDescription`][] property.

```csharp
[CommandLineArgument(ValueDescription = "Number")]
public int Argument { get; set; }
```

### Custom type conversion

If you want to use a non-default conversion from string, you can specify a custom type converter
using the [`TypeConverterAttribute`][].

```csharp
[CommandLineArgument]
[TypeConverter(typeof(CustomConverter))]
public int Argument { get; set; }
```

To make it easy to implement custom type converters to/from a string, Ookii.CommandLine provides
the [`TypeConverterBase<T>`][] type.

### Arguments that cancel parsing

You can indicate that argument parsing should stop and immediately print usage help when an argument
is supplied by setting the [`CommandLineArgumentAttribute.CancelParsing`][] property to `true`.

When this property is set, parsing is stopped when the argument is encountered. The rest of the
command line is not processed, and [`CommandLineParser.Parse()`][CommandLineParser.Parse()_2] will return `null`. The static
[`Parse<T>()`][Parse<T>()_1] helper method will automatically print usage in this case.

This can be used to implement a custom "-Help" argument, if you don't wish to use the default one.

```csharp
[CommandLineArgument(CancelParsing = true)]
public bool Help { get; set; }
```

### Long/short mode

To enable [long/short mode](Arguments.md#longshort-mode), you typically want to set three options
if you want to mimic typical POSIX conventions: the mode itself, case sensitive argument names,
and dash-case [name transformation](#name-transformation).

When using long/short mode, the name derived from the property name or explicitly given in the
[`CommandLineArgumentAttribute`][] constructor is the long name.

To set a short name, set [`CommandLineArgumentAttribute.ShortName`][] property. Alternatively,
you can set the [`CommandLineArgumentAttribute.IsShort`][] property to `true` to use the first character
of the long name as the short name.

You can disable the long name using the [`CommandLineArgumentAttribute.IsLong`](TODO) property, in
which case the argument will only have a short name (it must have at least either a short or a long
name).

```csharp
[ParseOptions(Mode = ParsingMode.LongShort,
    CaseSensitive = true,
    ArgumentNameTransform = NameTransform.DashCase,
    ValueDescriptionNameTransform = NameTransform.DashCase)]
class MyArguments
{
    [CommandLineArgument(IsShort = true)]
    public string? Path { get; set; }

    [CommandLineArgument(ShortName = 'a')]
    public int Foo { get; set;}

    [CommandLineArgument(IsShort = true, IsLong = false)]
    public bool Bar { get; set; }
}
```

In this case, the `--path` argument will have the short name `-p`, and the `--foo` argument will
have the short name `-a`. The `-b` argument from the `Bar` property has no long name. The names are
all lower case due to the name transformation.

## Using methods

You can also apply the [`CommandLineArgumentAttribute`][] to a method. This method must have one of the
following signatures:

- `public static bool Method(ArgumentType value, CommandLineParser parser);`
- `public static bool Method(ArgumentType value);`
- `public static bool Method(CommandLineParser parser);`
- `public static bool Method();`
- `public static void Method(ArgumentType value, CommandLineParser parser);`
- `public static void Method(ArgumentType value);`
- `public static void Method(CommandLineParser parser);`
- `public static void Method();`

The method will be called immediately when the argument is supplied (unlike properties, which are
only set after all arguments have been parsed), which is why the method must be static (the type
hasn't been created yet).

The type [`ArgumentType`][] is the type of the argument. If the method doesn't take an argument like
that, it will be a switch argument, and the method will be invoked when the argument is supplied,
even if its value is explicitly set to `false`. The type may not be an array, collection, or
dictionary type, as multi-value method arguments are not supported.

If you use one of the signatures with a `bool` return type, returning `false` will cancel parsing.
Unlike the [`CancelParsing`][CancelParsing_1] property, this will _not_ automatically display usage help. If you do
want to show help, set the [`CommandLineParser.HelpRequested`][] property to true.

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

An alternative way to define positional parameters is using a constructor. The parameters of the
public constructor for the class will be used to define arguments. These arguments will be
positional arguments, and required if the parameter is required.

Every constructor parameter creates a positional argument with its position matching the position of
the parameter. The type of the parameter is the type of the argument, and by
default the name of the constructor parameter is used of as the argument name, but this can be
overridden using the [`ArgumentNameAttribute`][] attribute. The default value is specified by supplying
a default value for the parameter, and this default value will also be included in the help text.

For example, consider a class with the following constructor:

```csharp
public MyArguments(string arg1,
                   int arg2,
                   [ArgumentName("CustomName")] float arg3 = 0f)
{
    /* omitted */
}
```

This constructor defines the following arguments: a required positional argument of type `string`
with the name “arg1”, a required positional argument of type `int` with the name “arg2”, and an
optional positional argument of type `float` with the name “CustomName” and a default value of 0.

If the type of a constructor parameter is an array, this defines a multi-value positional argument.
This must be the last positional argument. The same is true if the type is `Dictionary<TKey, TValue>`
for a positional dictionary argument.

If your type has more than one constructor, you must mark one of them using the
[`CommandLineConstructorAttribute`][] attribute. You don’t need to use this attribute if you have only
one constructor.

If you don’t wish to define arguments using the constructor, simply use a constructor without any
parameters (or don’t define an explicit constructor).

The [`ArgumentNameAttribute`][] can also be used to specify the argument's short name. Use the
[`ValueDescriptionAttribute`][] to set a custom value description, and the [`DescriptionAttribute`][] to
set a description. Using all these arguments on constructor parameters is rather awkward, which is
why it's recommended to use properties for must arguments.

If you follow .Net coding conventions, property names will be PascalCase and parameter names will be
camelCase. If you use both to define arguments, and rely on automatically determined names, this
causes inconsistent naming for your arguments. You can fix this by specifying explicit names for
either type of argument, or by using a [name transformation](#name-transformation) to make all
automatic names consistent.

### Nullable reference types

While it's generally cleaner to use properties to define arguments, one legitimate use for
constructor parameters is when you have an argument whose type is non-nullable. If using a property
to define such an argument, the C# compiler requires you to initialize it to a non-null value, even
if the argument is required.

```csharp
[CommandLineArgument(Position = 0, IsRequired = true)]
public string SomeArgument { get; set; } = string.Empty;
```

The property is initialized to an empty string, because we have to, but that value will never be
used, unless you instantiate the class manually without using [`CommandLineParser`][].

Constructor parameters get around this restriction:

```csharp
private readonly string _someArgument;

public MyArguments(string someArgument)
{
    _someArgument = someArgument;
}

public string SomeArgument => _someArgument;
```

Here, the extra initialization is not necessary, because the non-nullable field is set by the
constructor. Ookii.CommandLine will guarantee it will never pass a null value to an argument that
uses a non-nullable type (if using .Net 6.0 or later).

### CommandLineParser injection

If your constructor has a parameter whose type is [`CommandLineParser`][], this does not define an
argument. Instead, this property will be set to the [`CommandLineParser`][] instance that was used to
parse the arguments. This is useful if you want to access the [`CommandLineParser`][] instance after
parsing for whatever reason (for example, to see which alias was used to specify a particular
argument), but still want to use the static [`Parse<T>()`][Parse<T>()_1] method for automatic error and usage help
handling.

Using [`CommandLineParser`][] injection can be used by itself, or combined with other parameters that
define define arguments.

```csharp
public MyArguments(CommandLineParser parser, string argument)
{
}
```

## Defining aliases

An alias is an alternative name that can be used to specify a command line argument. Aliases can be
added to a command line argument by applying the [`AliasAttribute`][] to the property or constructor
parameter that defines the argument.

For example, the following code defines a switch argument that can be specified using either the
name “Verbose” or the alias “v”:

```csharp
[CommandLineArgument, Alias("v")]
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

This defines two arguments named "some-argument" and "other-argument", without the need to specify
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

In this case the constructor-defined argument name will be "SomeArgument", consistent with the
property-defined argument "OtherArgument", again without needing to use explicit names.

When using [long/short mode](Arguments.md#longshort-mode), and an argument with an automatic short
name, name transformation is applied to the name before the short name is determined, so the case of
the short name will match the case of the first letter of the transformed long name.

## Automatic arguments

Besides the arguments you define in your class, Ookii.CommandLine will, by default, add two automatic
arguments to your application: "Help" and "Version".

The "Help" argument will cancel parsing, and immediately show usage help. The "Version" argument
will cancel parsing, show version information, but will not show usage help.

The "Help" argument has two aliases, "?" and "h". The "Version" argument doesn't have any aliases.

When using [long/short mode](Arguments.md#longshort-mode), the "Help" argument has the short name
"?", and a short alias "h", while the "Version" argument has no short name.

If you use a name transformation, that transformation is also applied to the automatic argument
names.

The names and aliases of the automatic arguments can be customized using the [`LocalizedStringProvider`][]
class.

If your class defined an argument with the a name or alias matching the names or aliases of either
of the automatic arguments, that argument will not be automatically added. In addition, you can
disable either automatic argument using the [`ParseOptions`][].

Next, we'll take a look at how to [parse the arguments we've defined](ParsingArguments.md)

[`AliasAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_AliasAttribute.htm
[`ArgumentNameAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ArgumentNameAttribute.htm
[`ArgumentType`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgument_ArgumentType.htm
[`CommandLineArgumentAttribute.CancelParsing`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[`CommandLineArgumentAttribute.DefaultValue`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[`CommandLineArgumentAttribute.IsRequired`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsRequired.htm
[`CommandLineArgumentAttribute.IsShort`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsShort.htm
[`CommandLineArgumentAttribute.Position`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_Position.htm
[`CommandLineArgumentAttribute.ShortName`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_ShortName.htm
[`CommandLineArgumentAttribute.ValueDescription`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_ValueDescription.htm
[`CommandLineArgumentAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineArgumentAttribute.htm
[`CommandLineConstructorAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineConstructorAttribute.htm
[`CommandLineParser.HelpRequested`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineParser_HelpRequested.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineParser.htm
[`DescriptionAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute
[`ICollection<T>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.icollection-1
[`List<int>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1
[`List<string>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1
[`LocalizedStringProvider`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_LocalizedStringProvider.htm
[`ParseOptions.ArgumentNameTransform`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_ParseOptions_ArgumentNameTransform.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`ShortAliasAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ShortAliasAttribute.htm
[`System.ComponentModel.DescriptionAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute
[`TypeConverter`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverter
[`TypeConverterAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverterattribute
[`TypeConverterBase<T>`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_TypeConverterBase_1.htm
[`ValueDescriptionAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ValueDescriptionAttribute.htm
[CancelParsing_1]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[CommandLineParser.Parse()_2]: https://www.ookii.org/docs/commandline-3.0-preview/html/Overload_Ookii_CommandLine_CommandLineParser_Parse.htm
[DefaultValue_1]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[Parse<T>()_1]: https://www.ookii.org/docs/commandline-3.0-preview/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
