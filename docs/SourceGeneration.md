# Source generation

Ookii.CommandLine has two ways by which it can determine which arguments are available.

- Reflection will inspect the members of the arguments type at runtime, check for the
  [`CommandLineArgumentAttribute`][], and provide that information to the [`CommandLineParser`][]. This was
  the only method available before version 4.0, and is still used if the [`GeneratedParserAttribute`][]
  is not present.
- Source generation will perform the same inspection at compile time, generating C# code that will
  provide the required information to the [`CommandLineParser`][] with less runtime overhead. This is
  used as of version 4.0 when the [`GeneratedParserAttribute`][] is present.

The same also applies to [subcommands](Subcommands.md). The [`CommandManager`][] class uses runtime
reflection by default to discover the subcommands in an assembly, and source generation is available
with the [`GeneratedCommandManagerAttribute`][] to do that same work at compile time.

Using source generation has several benefits:

- Get [errors and warnings](SourceGenerationDiagnostics.md) at compile time for many common mistakes,
  which would cause a runtime exception or be silently ignored when using reflection.
- Use [automatic ordering](#automatic-ordering-of-positional-arguments) for positional arguments.
- Specify [default values using property initializers](#default-values-using-property-initializers).
- Allow your application to be
  [trimmed](https://learn.microsoft.com/dotnet/core/deploying/trimming/trimming-options). It's not
  possible to statically determine what types are needed to determine arguments using reflection,
  so trimming is not possible at all with reflection.
- Improved performance; benchmarks show that instantiating a [`CommandLineParser<T>`][] using a
  generated parser is up to thirty times faster than using reflection.

A few restrictions apply to projects that use Ookii.CommandLine's source generation:

- The project must be a C# project, using C# version 8 or later. Other languages or older C#
  versions are not supported.
- The project must be built using using the .Net 6.0 SDK or a later version.
  - You can still target older runtimes supported by Ookii.CommandLine, down to .Net Framework 4.6,
    but you must build the project using the .Net 6.0 SDK or newer.
- If you use the [`ArgumentConverterAttribute`][] or [`ParentCommandAttribute`][], you must use the
  constructor that takes a [`Type`][] instance. The constructor that takes a string is not supported.
- The generated arguments or command manager class may not be nested in another type.
- The generated arguments or command manager class may not have generic type parameters.

Generally, it's recommended to use source generation unless you cannot meet these requirements.

## Generating a parser

To use source generation to determine the command line arguments defined by a class, apply the
[`GeneratedParserAttribute`][] attribute to that class. You must also mark the class as `partial`,
because the source generator will add additional members to your class.

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    public string? SomeArgument { get; set; }
}
```

The source generator will inspect the members and attributes of the class, and generate C# code
that provides that information to a [`CommandLineParser`][], without needing to use reflection. While
doing so, it checks whether your class violates any rules for defining arguments, and
[emits warnings and errors](SourceGenerationDiagnostics.md) if it does.

If any of the arguments has a type for which there is no built-in [`ArgumentConverter`][] class, and
the argument doesn't use the [`ArgumentConverterAttribute`][], the source generator will check whether
the type supports any of the standard methods of [argument value conversion](Arguments.md#argument-value-conversion),
and if it does, it will generate an [`ArgumentConverter`][] implementation for that type, and uses it
for the argument.

Generated [`ArgumentConverter`][] classes are internal to your project, and placed in the
`Ookii.CommandLine.Conversion.Generated` namespace. The namespace can be customized using the
[`GeneratedConverterNamespaceAttribute`][] attribute.

If you use Visual Studio, you can view the generated files by looking under Dependencies,
Analyzers, Ookii.CommandLine.Generator in the Solution Explorer.

You can also set the `<EmitCompilerGeneratedFiles>` property to true in your project file, in which
case the generated files will be placed under the `obj` folder of your project.

### Using a generated parser

You can use the regular [`CommandLineParser<T>`][] or [`CommandLineParser`][] constructors, or the static
[`CommandLineParser.Parse<T>()`][] methods, which will automatically use the generated argument
information if it is available.

For convenience, the source generator also adds the following methods to your arguments class (where
`Arguments` is the name of your class):

```csharp
public static CommandLineParser<Arguments> CreateParser(ParseOptions? options = null);

public static Arguments? Parse(ParseOptions? options = null);

public static Arguments? Parse(string[] args, ParseOptions? options = null);

public static Arguments? Parse(ReadOnlyMemory<string> args, ParseOptions? options = null);
```

Use the [`CreateParser()`][CreateParser()_1] method as an alternative to the [`CommandLineParser<T>`][] constructor, and the
[`Parse()`][Parse()_7] methods as an alternative to the static [`CommandLineParser.Parse<T>()`][] methods.

Generally, it's recommended to use these generated methods. If you want to trim your application,
you must use them, since the regular [`CommandLineParser`][] constructor will still use reflection to
determine if generated argument information is present, and therefore still prohibits trimming.

So, if you had the following code before using source generation:

```csharp
var arguments = CommandLineParser.Parse<Arguments>();
```

You would replace it with the following:

```csharp
var arguments = Arguments.Parse();
```

Everything else remains the same.

If your project targets .Net 7.0 or later, the generated class will implement the
[`IParserProvider<TSelf>`][] and [`IParser<TSelf>`][] interfaces, which define the generated methods.

Generating the [`Parse()`][Parse()_7] methods is optional, and can be disabled using the
[`GeneratedParserAttribute.GenerateParseMethods`][] property. The [`CreateParser()`][CreateParser()_1] method is always
generated.

### Automatic ordering of positional arguments

When using the [`GeneratedParserAttribute`][], you do not have to specify explicit positions for
positional arguments. Instead, you can use the [`CommandLineArgumentAttribute.IsPositional`][]
property to indicate which arguments are positional, and the order will be determined by the order
of the members that define the arguments.

That means instead of this:

```csharp
class Arguments
{
    [CommandLineArgument(Position = 0)]
    public string? SomeArgument { get; set; }

    [CommandLineArgument(Position = 1)]
    public int OtherArgument { get; set; }
}
```

You can now do this:

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument(IsPositional = true)]
    public string? SomeArgument { get; set; }

    [CommandLineArgument(IsPositional = true)]
    public int OtherArgument { get; set; }
}
```

This means you no longer have to be careful about ordering when adding new arguments, and don't
have to worry about accidentally using the same position more than once.

If your class derives from a base class that defines positional arguments, those will come before
the arguments of the derived class.

If you use automatic ordering, all positional arguments must use it. Mixing explicit positions and
automatic positions is not allowed.

Using automatic ordering is not possible with reflection, because reflection does not guarantee it
will return the members of the class in any particular order.

### Default values using property initializers

When using the source generation, you can use property initializers to specify the default value of
an argument, and still have that value be used in the usage help.

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument(DefaultValue = "foo")]
    public string? Arg1 { get; set; }

    [CommandLineArgument]
    public string Arg2 { get; set; } = "foo";
}
```

When using a reflection-based parser, `Arg2` would have its value set to "foo" when omitted (since
Ookii.CommandLine doesn't assign the property if the argument is not specified), but that default
value would not be included in the usage help, whereas the default value of `Arg1` will be.

With the [`GeneratedParserAttribute`][], both `Arg1` and `Arg2` will have the default value of "foo"
shown in the usage help, making the two forms identical. Additionally, `Arg2` could be marked
non-nullable because it was initialized to a non-null value, something which isn't possible for
`Arg1` without initializing the property to a value that will not be used.

If both a property initializer and the [`DefaultValue`][DefaultValue_1] property are used, the
[`DefaultValue`][DefaultValue_1] property takes precedence.

This only works if the property initializer is a literal, enumeration value, reference to a constant,
reference to a property, or a null-forgiving expression with any of those expression types.

For example, `5`, `"value"`, `DayOfWeek.Tuesday`, `int.MaxValue` and `default!` are all supported
expressions for property initializers.

If a different kind of expression is used in the property initializer, such as a function call or
`new` expression, the value will not be shown in the usage help.

## Generating a command manager

You can apply the [`GeneratedParserAttribute`][] to a command, and generate the parser for that command
at compile time. This will work with the [`CommandManager`][] class without further changes to your
code.

The [`GeneratedParserAttribute`][] works the same for command classes as it does for any other arguments
class, with one exception: the static [`Parse()`][Parse()_7] methods are not generated by default for command
classes. You must explicitly set the [`GeneratedParserAttribute.GenerateParseMethods`][] to `true` if
you want them to be generated.

However, the [`CommandManager`][] class still uses reflection to determine what commands are available
in the assembly or assemblies you specify. To determine the available commands at compile time, you
must define a partial class with the [`GeneratedCommandManagerAttribute`][]:

```csharp
[GeneratedCommandManager]
partial class GeneratedManager
{
}
```

The source generator will find all command classes in your project, and generate C# code to provide
those commands to the generated command manager without needing reflection.

If you need to load commands from a different assembly, or multiple assemblies, you can use the
[`GeneratedCommandManagerAttribute.AssemblyNames`][] property. This property can use either just the
name of the assembly, or the full assembly identity including version, culture and public key
token.

```csharp
[GeneratedCommandManager(AssemblyNames = new[] { "MyCommandAssembly" })]
partial class GeneratedManager
{
}
```

Any assemblies specified in this list must be directly referenced by your application. If you wish
to use commands from an assembly that is dynamically loaded during runtime, you must continue to use
reflection.

### Using a generated command manager

The source generator will add [`CommandManager`][] as a base class to your class, and add the
following constructor to the class:

```csharp
public GeneratedManager(CommandOptions? options = null)
```

This means a class with the [`GeneratedCommandManagerAttribute`][] can be used as a drop-in replacement
of the regular [`CommandManager`][] class.

If you had the following code before using source generation:

```csharp
var manager = new CommandManager();
return manager.RunCommand() ?? 1;
```

You would replace it with the following:

```csharp
var manager = new GeneratedManager();
return manager.RunCommand() ?? 1;
```

Next, we will take a look at several [utility classes](Utilities.md) provided, and used, by
Ookii.CommandLine.

[`ArgumentConverter`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Conversion_ArgumentConverter.htm
[`ArgumentConverterAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Conversion_ArgumentConverterAttribute.htm
[`CommandLineArgumentAttribute.IsPositional`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsPositional.htm
[`CommandLineArgumentAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_CommandLineArgumentAttribute.htm
[`CommandLineParser.Parse<T>()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandLineParser<T>`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_CommandLineParser_1.htm
[`CommandManager`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_CommandManager.htm
[`GeneratedCommandManagerAttribute.AssemblyNames`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Commands_GeneratedCommandManagerAttribute_AssemblyNames.htm
[`GeneratedCommandManagerAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_GeneratedCommandManagerAttribute.htm
[`GeneratedConverterNamespaceAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Conversion_GeneratedConverterNamespaceAttribute.htm
[`GeneratedParserAttribute.GenerateParseMethods`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_GeneratedParserAttribute_GenerateParseMethods.htm
[`GeneratedParserAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_GeneratedParserAttribute.htm
[`IParser<TSelf>`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_IParser_1.htm
[`IParserProvider<TSelf>`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_IParserProvider_1.htm
[`ParentCommandAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_ParentCommandAttribute.htm
[`Type`]: https://learn.microsoft.com/dotnet/api/system.type
[CreateParser()_1]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_IParserProvider_1_CreateParser.htm
[DefaultValue_1]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[Parse()_7]: https://www.ookii.org/docs/commandline-4.0/html/Overload_Ookii_CommandLine_IParser_1_Parse.htm
