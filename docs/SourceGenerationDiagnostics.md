# Source generation diagnostics

The [source generator](SourceGeneration.md) will analyze your arguments class to see if it
does anything unsupported by Ookii.CommandLine. Among others, it checks for things such as:

- Whether [positional arguments](Arguments.md#positional-arguments) that are required or multi-value
  arguments follow the rules for their ordering.
- Whether positional arguments have duplicate numbering.
- Arguments with types that cannot be converted from a string.
- Attribute or property combinations that are ignored.
- Using the [`CommandLineArgumentAttribute`][] with a private member, or a method with an incorrect
  signature.

Without source generation, these mistakes would either lead to a runtime exception when creating the
[`CommandLineParser<T>`][] class, or would be silently ignored. With source generation, you can instead
catch any problems during compile time, which reduces the risk of bugs.

Not all errors can be caught at compile time. For example, the source generator does not check for
duplicate argument names, because the [`ParseOptions.ArgumentNameTransform`][] and
[`ParseOptions.ArgumentNameComparison`][] properties can render the result of this check inaccurate.

## Errors

### OCL0001

The command line arguments or command manager type must be a reference type.

A command line arguments type, or a type using the [`GeneratedCommandManagerAttribute`][], must be a
reference type, or class. Value types (or structures) cannot be used.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial struct Arguments // ERROR: The type must be a class.
{
    [CommandLineArgument]
    public string? Argument { get; set; }
}
```

### OCL0002

The command line arguments or command manager class must be partial.

When using the [`GeneratedParserAttribute`][] or [`GeneratedCommandManagerAttribute`][], the target type
must use the `partial` modifier.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
class Arguments // ERROR: The class must be partial
{
    [CommandLineArgument]
    public string? Argument { get; set; }
}
```

### OCL0003

The command line arguments or command manager class must not have any generic type arguments.

When using the [`GeneratedParserAttribute`][] or [`GeneratedCommandManagerAttribute`][], the target type
cannot be a generic type.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments<T> // ERROR: The class must not be generic
{
    [CommandLineArgument]
    public T? Argument { get; set; }
}
```

### OCL0004

The command line arguments or command manager class must not be nested in another type.

When using the [`GeneratedParserAttribute`][] or [`GeneratedCommandManagerAttribute`][], the target type
cannot be nested in another type.

For example, the following code triggers this error:

```csharp
class SomeClass
{
    [GeneratedParser]
    public partial class Arguments<T> // ERROR: The class must not be nested
    {
        [CommandLineArgument]
        public T? Argument { get; set; }
    }
}
```

### OCL0005

A multi-value argument defined by a property with an array type must use an array rank of one.
Arrays with different ranks are not supported.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
     // ERROR: Argument using an array rank other than one.
    [CommandLineArgument]
    public string[,]? Argument { get; set; }
}
```

### OCL0006

A command line argument property must have a public set accessor.

The only exceptions to this rule are [multi-value](DefiningArguments.md#multi-value-arguments) or
[dictionary](DefiningArguments.md#dictionary-arguments) arguments, which may use a read-only
property depending on their type.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
     // ERROR: Property must use a public set accessor.
    [CommandLineArgument]
    public string? Argument { get; private set; }
}
```

### OCL0007

No command line argument converter exists for the argument's type.

The argument uses a type (or in the case of a multi-value or dictionary argument, an element type)
that cannot be converted from a string using the [default rules for argument conversion](Arguments.md#argument-value-conversion),
and no custom [`ArgumentConverter`][] was specified.

To fix this error, either change the type of the argument, or create a custom [`ArgumentConverter`][]
and use the [`ArgumentConverterAttribute`][] on the argument.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
     // ERROR: Argument type must have a converter
    [CommandLineArgument]
    public Socket? Argument { get; set; }
}
```

### OCL0008

A method argument must use a supported signature.

When using a method to define an argument, only [specific signatures](DefiningArguments.md#using-static-methods)
are allowed. This error indicates the method is not using one of the supported signatures; for
example, it has additional parameters.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
     // ERROR: the method has an unrecognized parameter
    [CommandLineArgument]
    public static void Argument(string value, int value2);
}
```

### OCL0009

Init accessors may only be used with required properties.

A property that defines a command line argument may only use an `init` accessor if it also uses the
`required` keyword. It's not sufficient to mark the argument required; you must use a required
property (required properties are only supported on .Net 7 or later).

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
     // ERROR: The property uses init but is not required
    [CommandLineArgument(IsRequired = true)]
    public string? Argument { get; init; }
}
```

To fix this error, either use a regular `set` accessor, or if using .Net 7.0 or later, use the
`required` keyword (setting [`IsRequired`][IsRequired_1] is not necessary in this case):

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    public required string Argument { get; init; }
}
```

### OCL0010

The [`GeneratedParserAttribute`][] cannot be used with a class that implements the
[`ICommandWithCustomParsing`][] interface.

The [`ICommandWithCustomParsing`][] interface is for commands that do not use the [`CommandLineParser`][]
class, so a generated parser would not be used.

For example, the following code triggers this error:

```csharp
[Command]
[GeneratedParser]
partial class Arguments : ICommandWithCustomParsing // ERROR: The command uses custom parsing.
{
    public void Parse(ReadOnlyMemory<string> args, CommandManager manager)
    {
        // Omitted
    }

    public int Run()
    {
        // Omitted
    }
}
```

### OCL0011

A positional multi-value argument must be the last positional argument.

If you use a multi-value argument as a positional argument, there cannot be any additional
positional arguments after that one.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument(IsPositional = true)]
    public string[]? Argument1 { get; set; }

     // ERROR: Argument2 comes after Argument1, which is multi-value.
    [CommandLineArgument(IsPositional = true]
    public string? Argument2 { get; set; }
}
```

### OCL0012

Required positional arguments must come before optional positional arguments.

If you have an optional positional argument, it must come after any required ones.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument(IsPositional = true)]
    public string? Argument1 { get; set; }

     // ERROR: Required argument Argument2 comes after Argument1, which is optional.
    [CommandLineArgument(IsPositional = true)]
    public required string Argument2 { get; set; }
}
```

### OCL0013

One of the assembly names specified in the [`GeneratedCommandManagerAttribute.AssemblyNames`][] property
is not valid. This error is used when you give the full assembly identify, but it cannot be parsed.

For example, the following code triggers this error:

```csharp
// ERROR: The assembly name has an extra comma
[GeneratedCommandManager(AssemblyNames = new[] { "SomeAssembly,, Version=1.0.0.0" })]
partial class MyCommandManager
{
}
```

### OCL0014

One of the assembly names specified in the [`GeneratedCommandManagerAttribute.AssemblyNames`][] property
could not be resolved. Make sure it's an assembly that is referenced by the current project.

If you wish to load commands from an assembly that is not directly referenced by your project, you
must use the regular [`CommandManager`][] class, using reflection instead of source generation, instead.

For example, the following code triggers this error:

```csharp
// ERROR: The assembly isn't referenced
[GeneratedCommandManager(AssemblyNames = new[] { "UnreferencedAssembly" })]
partial class MyCommandManager
{
}
```

### OCL0015

The [`ArgumentConverterAttribute`][] or [`ParentCommandAttribute`][] must use the `typeof` keyword.

The [`ArgumentConverterAttribute`][] and [`ParentCommandAttribute`][] have two constructors; one that takes
the [`Type`][] of a converter or parent command, and one that takes the name of a type as a string. The
string constructor is not supported when using source generation.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    [ArgumentConverter("MyNamespace.MyConverter")] // ERROR: Can't use a string type name.
    public CustomType? Argument { get; set; }
}
```

To fix this error, either use the constructor that takes a [`Type`][] using the `typeof` keyword, or
do not use source generation.

### OCL0031

The argument does not have a long name or a short name. This happens when both the
[`CommandLineArgumentAttribute.IsLong`][] and [`CommandLineArgumentAttribute.IsShort`][] properties are set
to false. This means that when using [long/short mode](Arguments.md#longshort-mode), the argument
would not be usable.

This error will be triggered regardless of the parsing mode you actually use, since that can be
changed at runtime by the [`ParseOptions.Mode`][] property and is therefore not known at compile time.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
    // ERROR: No long or short name (IsShort is false by default).
    [CommandLineArgument(IsLong = false)]
    public string? Argument { get; set; }
}
```

### OCL0037

Source generation with the [`GeneratedParserAttribute`][] or [`GeneratedCommandManagerAttribute`][] requires
at least C# language version 8.0.

The code that is generated by Ookii.CommandLine's [source generation](SourceGeneration.md) uses
language features that are only available in C# 8.0. Use the `<LangVersion>` configuration property
to specify the language version if you are targeting a framework that does not use a new enough
version by default.

```xml
<PropertyGroup>
  <LangVersion>8.0</LangVersion>
</PropertyGroup>
```

If you cannot change the language version, remove the [`GeneratedParserAttribute`][] or
[`GeneratedCommandManagerAttribute`][] and use the [`CommandLineParser<T>`][] class,
[`CommandLineParser.Parse<T>()`][] methods, or [`CommandManager`][] class directly to use reflection
instead of source generation.

### OCL0038

Positional arguments using an explicit position with the [`CommandLineArgumentAttribute.Position`][]
property, and those using a position derived from their member ordering using the
[`CommandLineArgumentAttribute.IsPositional`][] property cannot be mixed. Note that this includes any
arguments defined in a base class.

For example, the following code triggers this error:

```csharp
// ERROR: Argument1 uses automatic positioning, and Argument2 uses an explicit position.
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument(IsPositional = true)]
    public string? Argument1 { get; set; }

    [CommandLineArgument(Position = 0)]
    public string? Argument2 { get; set; }
}
```

Please switch all arguments to use either explicit or automatic positions.

Note that using [`CommandLineArgumentAttribute.IsPositional`][] without an explicit position does not
work without the [`GeneratedParserAttribute`][].

## Warnings

### OCL0016

The [`TypeConverterAttribute`][] is no longer used by Ookii.CommandLine, and will be ignored.

As of Ookii.CommandLine 4.0, argument values are converted from a string using the
[`ArgumentConverter`][] class and [`TypeConverter`][] is no longer used. Custom converters should be
specified using the [`ArgumentConverterAttribute`][] attribute.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    [TypeConverter(typeof(MyNamespace.MyConverter)] // WARNING: TypeConverterAttribute is not used
    public CustomType? Argument { get; set; }
}
```

To fix this warning, switch to using the [`ArgumentConverterAttribute`][] attribute. To use the
existing [`TypeConverter`][], you can use the [`WrappedTypeConverter<T>`][] class.

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    [ArgumentConverter(typeof(WrappedTypeConverter<MyNamespace.MyConverter>)]
    public CustomType? Argument { get; set; }
}
```

### OCL0017

Methods that are not public and static will be ignored.

If the [`CommandLineArgumentAttribute`][] is used on a method that is not a `public static` method, no
argument will be generated for this method.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
     // WARNING: the method must be public
    [CommandLineArgument]
    private static void Argument(string value, int value2);
}
```

### OCL0018

Properties that are not public instance properties will be ignored.

If the [`CommandLineArgumentAttribute`][] is used on a property that is not a `public` property, or
that is a `static` property, no argument will be generated for this property.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
     // WARNING: the property must be public
    [CommandLineArgument]
    private string? Argument { get; set; }
}
```

### OCL0019

A command line arguments class has the [`CommandAttribute`][] but does not implement the [`ICommand`][]
interface, or vice versa.

Without the interface, the [`CommandAttribute`][] is ignored and the class will not be treated as a
command by a regular or generated [`CommandManager`][]. Both the [`CommandAttribute`][] and the [`ICommand`][]
interface are required for commands.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
[Command]
partial class MyCommand // WARNING: The class doesn't implement ICommand
{
    [CommandLineArgument]
    public string? Argument { get; set; }
}
```

The inverse, implementing [`ICommand`][] without using the [`CommandAttribute`][], can be used for
subcommand base classes. This still triggers a warning with the [`GeneratedParserAttribute`][],
since that attribute does not need to be applied to base classes, only to the derived classes that
are actually used as commands.

### OCL0020

An argument that is required, multi-value, or a method argument, specifies a default value. The
default value will not be used for these kinds of arguments.

For a required argument, the default value is not used because not specifying an explicit value is
an error. For a multi-value argument, the default value is never used and the collection will still
be empty or null if no explicit value was given. The method for a method argument is only invoked
if the argument is explicitly provided; it is never invoked with a default value.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
    // WARNING: Default value is unused on a required argument.
    [CommandLineArgument(DefaultValue = "foo")]
    public required string Argument { get; set; }
}
```

### OCL0021

The [`CommandLineArgumentAttribute.IsRequired`][] property is ignored for a property with the
`required` keyword. If the `required` keyword is present, the argument is required, even if you
set the [`IsRequired`][IsRequired_1] property to false explicitly.

> [!NOTE]
> The `required` keyword is only available in .Net 7.0 and later; the [`IsRequired`][IsRequired_1]
> property should be used to create required arguments in older versions of .Net.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
    // WARNING: the argument will be required regardless of the value of IsRequired.
    [CommandLineArgument(IsRequired = false)]
    public required string Argument { get; set; }
}
```

### OCL0022

The same position value is used for two or more arguments.

While the actual position values do not matter—merely the order of the values do, so skipping
numbers is fine—using the same number more than once can lead to unpredictable or unstable ordering
of the arguments, which should be avoided.

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument(Position = 0)]
    public string? Argument1 { get; set; }

    // WARNING: Argument2 has the same position as Argument1.
    [CommandLineArgument(Position = 0)]
    public string? Argument2 { get; set; }
}
```

When using the [`GeneratedParserAttribute`][], you can use the [`CommandLineArgumentAttribute.IsPositional`][]
property to create positional arguments by their definition order, without having to worry about
keeping explicitly set numbers correct.

### OCL0023

The [`ShortAliasAttribute`][] is ignored on an argument that does not have a short name. Set the
[`CommandLineArgumentAttribute.IsShort`][] property to true or set an explicit short name using the
[`CommandLineArgumentAttribute.ShortName`][] property. Without a short name, any short aliases will not
be used.

Note that the [`ShortAliasAttribute`][] is also ignored if [`ParsingMode.LongShort`][] is not used, which is
not checked by the source generator, because it can be changed at runtime using the
[`ParseOptions.Mode`][] property.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
[ParseOptions(Mode = ParsingMode.LongShort)]
partial class Arguments
{
    // WARNING: The short alias is not used since the argument has no short name.
    [CommandLineArgument]
    [ShortAlias('a')]
    public string? Argument { get; set; }
}
```

### OLC0024

The [`AliasAttribute`][] is ignored on an argument with no long name. An argument has no long name only
if the [`CommandLineArgumentAttribute.IsLong`][] property is set to false.

Note that the [`AliasAttribute`][] may still be used if [`ParsingMode.LongShort`][] is not used, which is
not checked by the source generator, because it can be changed at runtime using the
[`ParseOptions.Mode`][] property.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
[ParseOptions(Mode = ParsingMode.LongShort)]
partial class Arguments
{
    // WARNING: The long alias is not used since the argument has no long name.
    [CommandLineArgument(IsLong = false, IsShort = true)]
    [Alias("arg")]
    public string? Argument { get; set; }
}
```

### OCL0025

The [`CommandLineArgumentAttribute.IsHidden`][] property is ignored for positional or required
arguments.

Positional arguments cannot be hidden, because excluding them from the usage help would give
incorrect positions for any additional positional arguments. A positional argument is therefore not
hidden even if [`IsHidden`][IsHidden_1] is set to true.

Required arguments cannot be hidden, because the application cannot be easily used if they user does
not know about them.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
    // WARNING: The argument is not hidden because it's positional.
    [CommandLineArgument(IsPositional = true, IsHidden = true)]
    public string? Argument { get; set; }
}
```

### OCL0026

The namespace specified in the [`GeneratedConverterNamespaceAttribute`][] is not a valid C# namespace
name, for example because one of the elements contains an unsupported character or starts with a
digit.

For example, the following code triggers this warning:

```csharp
[assembly: GeneratedConverterNamespace("MyApp.5Invalid")]
```

### OCL0027

The [`KeyConverterAttribute`][], [`ValueConverterAttribute`][], [`KeyValueSeparatorAttribute`][] and
[`AllowDuplicateDictionaryKeysAttribute`][] attributes are only used for dictionary arguments, and will
be ignored if the argument is not a dictionary argument.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    [AllowDuplicateDictionaryKeys] // WARNING: Ignored on non-dictionary arguments
    public string? Argument { get; set; }
}
```

### OCL0028

The [`KeyConverterAttribute`][], [`ValueConverterAttribute`][],  and [`KeyValueSeparatorAttribute`][] attributes
are used by the default [`KeyValuePairConverter<TKey, TValue>`][] for dictionary arguments, and will be
ignored if the argument uses the [`ArgumentConverterAttribute`][] to specify a different converter.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    [ArgumentConverter(typeof(CustomKeyValuePairConverter))]
    [KeyValueSeparator(":")] // WARNING: Ignored on dictionary arguments with an explicit converter.
    public Dictionary<string, int>? Argument { get; set; }
}
```

### OCL0029

The [`MultiValueSeparatorAttribute`][] is only used for multi-value arguments (including dictionary
arguments), and will be ignored if the argument is not a multi-value argument.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    [MultiValueSeparator(",")] // WARNING: Ignored on non-multi-value arguments
    public string? Argument { get; set; }
}
```

### OCL0030

An argument has an explicit name or short name starting with a number, which cannot be used with
the '-' prefix.

If the [`CommandLineParser`][] sees a dash followed by a digit, it will always interpret this as a
value, because it may be a negative number. It is never interpreted as an argument name, even if
the rest of the argument is not a valid number.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
    // WARNING: Name starts with a number.
    [CommandLineArgument("1Arg")]
    public string? Argument { get; set; }
}
```

This warning may be a false positive if you are using a different argument name prefix with the
[`ParseOptionsAttribute.ArgumentNamePrefixes`][] or [`ParseOptions.ArgumentNamePrefixes`][] property, or if
you are using long/short mode and the name is a long name. In these cases, you should suppress or
disable this warning.

### OCL0032

The [`CommandLineArgumentAttribute.IsShort`][] property is ignored if an explicit short name is set
using the [`CommandLineArgumentAttribute.ShortName`][] property.

If the [`ShortName`][ShortName_1] property is set, it implies that [`IsShort`][] is true, and manually setting it to
false will have no effect.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
[ParseOptions(Mode = ParsingMode.LongShort)]
partial class Arguments
{
    // WARNING: Argument has a short name so IsShort is ignored.
    [CommandLineArgument(ShortName = 'a', IsShort = false)]
    public string? Argument { get; set; }
}
```

### OCL0033

Arguments should have a description, set using the [`DescriptionAttribute`][] attribute, for use in the
usage help.

Arguments without a description are not guaranteed to be listed in the description list of the
usage help, and provide no additional information about their use when the user requests usage
help (for example using the automatic `-Help` argument).

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
    // WARNING: No DescriptionAttribute on this member.
    [CommandLineArgument]
    public string? Argument { get; set; }
}
```

To fix this, write a concise description explaining the argument's purpose and usage, and apply the
[`DescriptionAttribute`][] (or a derived attribute) to the member that defines the argument.

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    [Description("A description of the argument.")]
    public string? Argument { get; set; }
}
```

This warning will not be emitted for arguments that are hidden using the
[`CommandLineArgumentAttribute.IsHidden`][] property.

### OCL0034

Subcommands should have a description, set using the [`DescriptionAttribute`][] attribute, for use in
the usage help.

For example, the following code triggers this warning:

```csharp
// WARNING: No DescriptionAttribute on this subcommand class.
[GeneratedParser]
[Command]
partial class MyCommand : ICommand
{
    [CommandLineArgument]
    [Description("A description of the argument.")]
    public string? Argument { get; set; }
}
```

To fix this, write a concise description explaining the command's purpose, and apply the
[`DescriptionAttribute`][] (or a derived attribute) to the class that defines the command.

```csharp
[GeneratedParser]
[Description("A description of the command.")]
[Command]
partial class MyCommand : ICommand
{
    [CommandLineArgument]
    [Description("A description of the argument.")]
    public string? Argument { get; set; }
}
```

This warning will not be emitted for subcommands that are hidden using the
[`CommandAttribute.IsHidden`][] property.

### OCL0035

The [`ParentCommandAttribute`][] attribute is only used for subcommands, but was used on an arguments
type that isn't a subcommand.

For example, the following code triggers this warning:

```csharp
// WARNING: ParentCommandAttribute is ignored for non-commands.
[GeneratedParser]
[ParentCommand(typeof(SomeCommand))]
partial class MyCommand
{
    [CommandLineArgument]
    [Description("A description of the argument.")]
    public string? Argument { get; set; }
}
```

### OCL0036

The [`ApplicationFriendlyNameAttribute`][] attribute was used on a subcommand. The
[`ApplicationFriendlyNameAttribute`][] is used by the automatic `-Version` argument, which is not
created for subcommands, and the automatic `version` command only uses the
[`ApplicationFriendlyNameAttribute`][] when applied to the entry assembly for the application.

For example, the following code triggers this warning:

```csharp
// WARNING: ApplicationFriendlyName is ignored for commands.
[GeneratedParser]
[ApplicationFriendlyName("My Application")]
[Command]
partial class MyCommand : ICommand
{
    [CommandLineArgument]
    [Description("A description of the argument.")]
    public string? Argument { get; set; }
}
```

Instead, the attribute should be applied to the assembly:

```csharp
[assembly: ApplicationFriendlyName("My Application")]
```

### OCL0039

The initial value of a property will not be included in the usage help, because it uses an
expression type that is not supported by the source generator. Supported expression types are
literals, enumeration values, constants, properties, and null-forgiving expressions containing any
of those expression types.

For example, `5`, `"value"`, `DayOfWeek.Tuesday`, `int.MaxValue` and `default!` are all supported
expressions for property initializers.

Any other type of expression, such as a method invocation or constructing a new object, is not
supported and will not be included in the usage help.

For example, the following code triggers this warning:

```csharp
[GeneratedParser]
partial class Arguments
{
    // WARNING: Method call for property initializer is not supported for the usage help.
    [CommandLineArgument]
    public string? Argument { get; set; } = GetDefaultValue();

    private static int GetDefaultValue()
    {
        // omitted.
    }
}
```

This will not affect the actual value of the argument, since the property will not be set by the
[`CommandLineParser`][] if the [`CommandLineArgumentAttribute.DefaultValue`][] property is null.
Therefore, you can safely suppress this warning and include the relevant explanation of the default
value in the property's description manually, if desired.

To avoid this warning, use one of the supported expression types, or use the
[`CommandLineArgumentAttribute.DefaultValue`][] property. This warning will not be emitted if the
[`CommandLineArgumentAttribute.DefaultValue`][] property is not null, regardless of the initializer.
It will also not be emitted if the [`CommandLineArgumentAttribute.IncludeDefaultInUsageHelp`][]
property is false.

Note that default values set by property initializers are only shown in the usage help if the
[`GeneratedParserAttribute`][] is used. When reflection is used, only
[`CommandLineArgumentAttribute.DefaultValue`][] is supported.

### OCL0040

Command line arguments classes should use source generation.

This warning is emitted for any class that contains members with the
[`CommandLineArgumentAttribute`][], but which does not have the [`GeneratedParserAttribute`][]
applied. Using [source generation](SourceGeneration.md) is recommended in all cases, unless you
cannot meet the requirements.

This warning is not emitted if the project does not use C# 8 or later, or the class is abstract,
nested in another type, or has generic type arguments.

For example, the following code triggers this warning:

```csharp
// WARNING: The "Argument" property has the CommandLineArgumentAttribute, but the class does not
// have the GeneratedParserAttribute.
class Arguments
{
    [CommandLineArgument]
    public string? Argument { get; set; }
}
```

A code fix is provided that lets you use the lightbulb UI in Visual Studio to quickly add the
[`GeneratedParserAttribute`][] to the class. This will also make the class `partial` if it isn't
already.

If you cannot use source generation for some reason, you should disable this warning.

### OCL0041

The [`ValidateEnumValueAttribute`][] attribute was applied to an argument whose type is not an
enumeration type, a nullable enumeration type, or an array or collection containing an enumeration
type.

The [`ValidateEnumValueAttribute`][] attribute only supports enumeration types, and will throw an
exception at runtime if used to validate an argument whose type is not an enumeration.

For example, the following code triggers this warning:

```csharp
class Arguments
{
    // WARNING: String isn't an enumeration type.
    [CommandLineArgument]
    [ValidateEnumValue]
    public string? Argument { get; set; }
}
```

To fix this warning, either remove the [`ValidateEnumValueAttribute`][] attribute or change the type
of the argument to an enumeration type.

### OCL0042

An argument has the [`ArgumentConverterAttribute`][] set, and uses properties of the
[`ValidateEnumValueAttribute`][] that may not be supported by a custom converter.

The [`CaseSensitive`][CaseSensitive_1] property of the [`ValidateEnumValueAttribute`][] attribute is
not used by the [`ValidateEnumValueAttribute`][] attribute itself, but instead alters the behavior
of the [`EnumConverter`][] class. If an argument uses a custom converter rather than the
[`EnumConverter`][], it is not guaranteed that this property will have any effect.

For example, the following code triggers this warning:

```csharp
class Arguments
{
    // WARNING: ValidateEnumValueAttribute.CaseSensitive used with a custom argument converter.
    [CommandLineArgument]
    [ArgumentConverter(typeof(MyConverter))]
    [ValidateEnumValue(CaseSensitive = true)]
    public DayOfWeek Argument { get; set; }
}
```

To fix this warning, either use the default [`EnumConverter`][], or do not use the
[`CaseSensitive`][] property. If the custom converter does check the value of that property, you can
disable this warning.

[`AliasAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_AliasAttribute.htm
[`AllowDuplicateDictionaryKeysAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_AllowDuplicateDictionaryKeysAttribute.htm
[`ApplicationFriendlyNameAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_ApplicationFriendlyNameAttribute.htm
[`ArgumentConverter`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Conversion_ArgumentConverter.htm
[`ArgumentConverterAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Conversion_ArgumentConverterAttribute.htm
[`CommandAttribute.IsHidden`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_Commands_CommandAttribute_IsHidden.htm
[`CommandAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Commands_CommandAttribute.htm
[`CommandLineArgumentAttribute.DefaultValue`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[`CommandLineArgumentAttribute.IncludeDefaultInUsageHelp`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IncludeDefaultInUsageHelp.htm
[`CommandLineArgumentAttribute.IsHidden`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsHidden.htm
[`CommandLineArgumentAttribute.IsLong`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsLong.htm
[`CommandLineArgumentAttribute.IsPositional`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsPositional.htm
[`CommandLineArgumentAttribute.IsRequired`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsRequired.htm
[`CommandLineArgumentAttribute.IsShort`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsShort.htm
[`CommandLineArgumentAttribute.Position`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_Position.htm
[`CommandLineArgumentAttribute.ShortName`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_ShortName.htm
[`CommandLineArgumentAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_CommandLineArgumentAttribute.htm
[`CommandLineParser.Parse<T>()`]: https://www.ookii.org/docs/commandline-4.2/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandLineParser<T>`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_CommandLineParser_1.htm
[`CommandManager`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Commands_CommandManager.htm
[`DescriptionAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute
[`EnumConverter`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Conversion_EnumConverter.htm
[`GeneratedCommandManagerAttribute.AssemblyNames`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_Commands_GeneratedCommandManagerAttribute_AssemblyNames.htm
[`GeneratedCommandManagerAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Commands_GeneratedCommandManagerAttribute.htm
[`GeneratedConverterNamespaceAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Conversion_GeneratedConverterNamespaceAttribute.htm
[`GeneratedParserAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_GeneratedParserAttribute.htm
[`ICommand`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Commands_ICommand.htm
[`ICommandWithCustomParsing`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Commands_ICommandWithCustomParsing.htm
[`IsShort`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsShort.htm
[`KeyConverterAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Conversion_KeyConverterAttribute.htm
[`KeyValuePairConverter<TKey, TValue>`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Conversion_KeyValuePairConverter_2.htm
[`KeyValueSeparatorAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Conversion_KeyValueSeparatorAttribute.htm
[`MultiValueSeparatorAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_MultiValueSeparatorAttribute.htm
[`ParentCommandAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Commands_ParentCommandAttribute.htm
[`ParseOptions.ArgumentNameComparison`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_ParseOptions_ArgumentNameComparison.htm
[`ParseOptions.ArgumentNamePrefixes`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_ParseOptions_ArgumentNamePrefixes.htm
[`ParseOptions.ArgumentNameTransform`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_ParseOptions_ArgumentNameTransform.htm
[`ParseOptions.Mode`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_ParseOptions_Mode.htm
[`ParseOptionsAttribute.ArgumentNamePrefixes`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_ParseOptionsAttribute_ArgumentNamePrefixes.htm
[`ParsingMode.LongShort`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_ParsingMode.htm
[`ShortAliasAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_ShortAliasAttribute.htm
[`Type`]: https://learn.microsoft.com/dotnet/api/system.type
[`TypeConverter`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverter
[`TypeConverterAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverterattribute
[`ValidateEnumValueAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Validation_ValidateEnumValueAttribute.htm
[`ValueConverterAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Conversion_ValueConverterAttribute.htm
[`WrappedTypeConverter<T>`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_Conversion_WrappedTypeConverter_1.htm
[CaseSensitive_1]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_CaseSensitive.htm
[IsHidden_1]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsHidden.htm
[IsRequired_1]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsRequired.htm
[ShortName_1]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_ShortName.htm
