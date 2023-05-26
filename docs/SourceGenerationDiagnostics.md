# Source generation diagnostics

The [source generator](SourceGeneration.md) will analyze your arguments class to see if it
does anything unsupported by Ookii.CommandLine. Among others, it checks for things such as:

- Whether [positional arguments](Arguments.md#positional-arguments) that are required or multi-value
  arguments follow the rules for their ordering.
- Whether positional arguments have duplicate numbering.
- Arguments with types that cannot be converted from a string.
- Attribute or property combinations that are ignored.
- Using the `CommandLineArgument` with a private member, or a method with an incorrect signature.

Without source generation, these mistakes would either lead to a runtime exception when creating the
`CommandLineParser<T>` class, or would be silently ignored. With source generation, you can instead
catch any problems during compile time, which reduces the risk of bugs.

Not all errors can be caught at compile time. For example, the source generator does not check for
duplicate argument names, because the `ParseOptions.ArgumentNameTransform` property can modify the
names, which makes this impossible to determine at compile time.

## Errors

### OCL0001

The command line arguments or command manager type must be a reference type.

A command line arguments type, or a type using the `GeneratedCommandManagerAttribute`, must be a
reference type, or class. Value types (or structures) cannot be used.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial struct Arguments // ERROR: The type must be a class.
{
    [CommandLineAttribute]
    public string? Argument { get; set; }
}
```

### OCL0002

The command line arguments or command manager class must be partial.

When using the `GeneratedParserAttribute` or `GeneratedCommandManagerAttribute`, the target type
must use the `partial` modifier.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
class Arguments // ERROR: The class must be partial
{
    [CommandLineAttribute]
    public string? Argument { get; set; }
}
```

### OCL0003

The command line arguments or command manager class must not have any generic type arguments.

When using the `GeneratedParserAttribute` or `GeneratedCommandManagerAttribute`, the target type
cannot be a generic type.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments<T> // ERROR: The class must not be generic
{
    [CommandLineAttribute]
    public T? Argument { get; set; }
}
```

### OCL0004

The command line arguments or command manager class must not be nested in another type.

When using the `GeneratedParserAttribute` or `GeneratedCommandManagerAttribute`, the target type
cannot be nested in another type.

For example, the following code triggers this error:

```csharp
class SomeClass
{
    [GeneratedParser]
    public partial class Arguments<T> // ERROR: The class must not be nested
    {
        [CommandLineAttribute]
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
    [CommandLineAttribute]
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
    [CommandLineAttribute]
    public string? Argument { get; private set; }
}
```

### OCL0007

No command line argument converter exists for the argument's type.

The argument uses a type (or in the case of a multi-value or dictionary argument, an element type)
that cannot be converted from a string using the [default rules for argument conversion](Arguments.md#argument-value-conversion),
and no custom `ArgumentConverter` was specified.

To fix this error, either change the type of the argument, or create a custom `ArgumentConverter`
and use the `ArgumentConverterAttribute` on the argument.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
     // ERROR: Argument type must have a converter
    [CommandLineAttribute]
    public Socket? Argument { get; set; }
}
```

### OCL0008

A method argument must use a supported signature.

When using a method to define an argument, only [specific signatures](DefiningArguments.md#using-methods)
are allowed. This error indicates the method is not using one of the supported signatures, for
example it has additional parameters, or is not static.

For example, the following code triggers this error:

```csharp
[GeneratedParser]
partial class Arguments
{
     // ERROR: the method must be static
    [CommandLineAttribute]
    public void Argument(string value);
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
    [CommandLineAttribute(IsRequired = true)]
    public string? Argument { get; init; }
}
```

To fix this error, either use a regular `set` accessor, or if using .Net 7.0 or later, use the
`required` keyword (setting `IsRequired` is not necessary in this case):

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineAttribute]
    public required string Argument { get; init; }
}
```

### OCL0010

The `GeneratedParserAttribute` cannot be used with a class that implements the
`ICommandWithCustomParsing` interface.

The `ICommandWIthCustomParsing` interface is for commands that do not use the `CommandLineParser`
class, so a generated parser would not be used.

For example, the following code triggers this error:

TODO: Update with span/memory if used.

```csharp
[Command]
[GeneratedParser]
partial class Arguments : ICommandWithCustomParsing // ERROR: The command uses custom parsing.
{
    public void Parse(string[] args, int index, CommandOptions options)
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
    [CommandLineAttribute(Position = 0)]
    public string[]? Argument1 { get; set; }

     // ERROR: Argument2 comes after Argument1, which is multi-value.
    [CommandLineAttribute(Position = 1)]
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
    [CommandLineAttribute(Position = 0)]
    public string? Argument1 { get; set; }

     // ERROR: Required argument Argument2 comes after Argument1, which is optional.
    [CommandLineAttribute(IsRequired = true, Position = 1)]
    public string? Argument2 { get; set; }
}
```

### OCL0013

One of the assembly names specified in the `GeneratedCommandManagerAttribute.AssemblyNames` property
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

One of the assembly names specified in the `GeneratedCommandManagerAttribute.AssemblyNames` property
could not be resolved. Make sure it's an assembly that is referenced by the current project.

If you wish to load commands from an assembly that is not directly referenced by your project, you
must use the regular `CommandManager` class, using reflection instead of source generation, instead.

For example, the following code triggers this error:

```csharp
// ERROR: The assembly isn't referenced
[GeneratedCommandManager(AssemblyNames = new[] { "UnreferencedAssembly" })]
partial class MyCommandManager
{
}
```

### OCL0015

The `ArgumentConverterAttribute` must use the `typeof` keyword.

The `ArgumentConverterAttribute` has two constructors, one that takes the `Type` of a converter,
and one that takes the name of a converter type as a string. The string constructor is not supported
when using source generation.

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineAttribute]
    [ArgumentConverter("MyNamespace.MyConverter")] // ERROR: Can't use a string type name.
    public CustomType? Argument { get; set; }
}
```

To fix this error, either use the constructor that takes a `Type` using the `typeof` keyword, or
use a `CommandLineParser<T>` without using source generation.

## Warnings

TODO
