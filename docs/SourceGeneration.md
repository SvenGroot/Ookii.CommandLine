# Source generation

Ookii.CommandLine includes a source generator that can be used to generate a `CommandLineParser<T>`
for an arguments type, or a `CommandManager` for the commands in an assembly, at compile time. The
source generator will generate C# code that creates those classes using information about your
arguments or command types available during compilation, rather than determining that information at
runtime using reflection.

Using source generation has several benefits:

- Get [errors and warnings](SourceGenerationDiagnostics.md) at compile time for argument rule
  violations (such as a required positional argument after an optional positional argument), ignored
  options (such as setting a default value for a required attribute), and other problems (such as
  using the same position number more than once, method arguments with the wrong signature, or using
  the `CommandLineArgumentAttribute` on a private or read-only property). These would normally be
  silently ignored or cause a runtime exception, but now you can catch problems during compilation.
- Allow your application to be
  [trimmed](https://learn.microsoft.com/dotnet/core/deploying/trimming/trimming-options). When
  source generation is not used, the way Ookii.CommandLine uses reflection prevents trimming
  entirely.
- Improved performance. Benchmarks show that instantiating a `CommandLineParser<T>` using a
  generated parser is up to thirty times faster than using reflection. However, since we're still
  talking about microseconds, this is unlikely to matter that much to a typical application.

A few restrictions apply to projects that use Ookii.ComandLine's source generation:

- The project must a C# project (other languages are not supported), using C# version 8 or later.
- The project must be built using a recent version of the .Net SDK (TODO: Exact version).
  - You can still target older runtimes supported by Ookii.CommandLine, down to .Net Framework 4.6,
    but you must build the project using an SDK that supports the source generator, and set the
    appropriate language version using the `<LangVersion>` property in your project file.
- If you use the `ArgumentConverterAttribute`, you must use the constructor that takes a `Type`
  instance. The constructor that takes a string is not supported.
- The arguments or command manager class may not be nested in another type.
- The arguments or command manager class may not have generic type parameters.

Generally, it's recommended to use source generation unless you cannot meet these requirements, or
you have another reason why you cannot use it.

## Generating a parser

Normally, the `CommandLineParser` class uses runtime reflection to determine the command line
arguments defined by an arguments class. To use source generation instead, use the
`GeneratedParserAttribute` attribute on your arguments class. You must also mark the class as
`partial`, because the source generator will add additional members to your class.

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    public string? SomeArgument { get; set; }
}
```

The source generator will inspect the members and attributes of the class, and generates C# code
that provides that information to a `CommandLineParser`, without needing to use reflection. While
doing so, it checks whether your class violates any rules for defining arguments, and
[emits warnings and errors](SourceGenerationDiagnostics.md) if it does.

If any of the arguments has a type for which there is no built-in `ArgumentConverter` class, and
the argument doesn't use the `ArgumentConverterAttribute`, the source generator will check whether
the type supports any of the standard methods of [argument value conversion](Arguments.md#argument-value-conversion),
and if it does, it will generate an `ArgumentConverter` implementation for that type (without
source generation, conversion for these types would normally also use reflection), and uses it
for the argument.

Generated `ArgumentConverter` classes are internal to your project, and placed in the `Ookii.CommandLine.Conversion.Generated`
namespace. The namespace can be customized using the `GeneratedConverterNamespaceAttribute`
attribute.

You can view any of the generated files using Visual Studio by looking under Dependencies,
Analyzers, Ookii.CommandLine.Generator in the Solution Explorer, or by setting the
`<EmitCompilerGeneratedFiles>` property to true in your project file, in which case the generated
files will be placed under the `obj` folder of your project.

### Using a generated parser

When using the `GeneratedParserAttribute`, you must *not* use the regular `CommandLineParser` or
`CommandLineParser<T>` constructor, or the static `CommandLineParser.Parse<T>()` methods. These will
still use reflection, even if a generated parser is available for a class.

> By default, these constructors and methods will throw an exception if you try to use them with a
> class that has the `GeneratedParserAttribute`, to prevent accidentally using reflection when it
> was not intended. If for some reason you need to use reflection on a class that has that
> attribute, you can set the `ParseOptions.AllowReflectionWithGeneratedParser` property to `true`.

Instead, you should use one of the methods that the source generator will add to your arguments
class (where `Arguments` is the name of your class):

```csharp
public static CommandLineParser<Arguments> CreateParser(ParseOptions? options = null);

public static Arguments? Parse(ParseOptions? options = null);

public static Arguments? Parse(string[] args, ParseOptions? options = null);

public static Arguments? Parse(string[] args, int index, ParseOptions? options = null);
```

Use the `CreateParser()` method as an alternative to the `CommandLineParser<T>` constructor, and the
`Parse()` methods as an alternative to the static `CommandLineParser.Parse<T>()` methods.

So, if you had the following code before using source generation:

```csharp
var arguments = CommandLineParser.Parse<Arguments>();
```

You would replace it with the following:

```csharp
var arguments = Arguments.Parse();
```

If your project targets .Net 7 or later, the generated class will implement the `IParserProvider<TSelf>`
and `IParser<TSelf>` interfaces, which define these methods.

Generating the `Parse()` methods is optional, and can be disabled using the
`GeneratedParserAttribute.GenerateParseMethods` property. The `CreateParser()` method is always
generated.

## Generating a command manager

Just like the `CommandLineParser` class, the `CommandManager` class normally uses reflection to
locate all command classes in the assembly or assemblies you specify. Instead, you can create a
class with the `GeneratedCommandManagerAttribute` which can perform this same job at compile time.

To create a generated command manager, define a partial class with the
`GeneratedCommandManagerAttribute`:

```csharp
[GeneratedCommandManager]
partial class MyCommandManager
{
}
```

The source generator will find all command classes in your project, and generate C# code to provide
those arguments to the `CommandManager` without needing reflection.

If you need to load commands from a different assembly, or multiple assemblies, you can use the
`GeneratedCommandManagerAttribute.AssemblyNames` property. This property can use either just the
name of the assembly, or the full assembly identity including version, culture and public key
token.

```csharp
[GeneratedCommandManager(AssemblyNames = new[] { "MyCommandAssembly" })]
partial class MyCommandManager
{
}
```

If you wish to use commands from an assembly that is dynamically loaded during runtime, you must
continue using reflection.

### Using a generated command manager

The source generator will add `CommandManager` as a base class to your class, and add the
following constructor to the class:

```csharp
public MyCommandManager(CommandOptions? options = null)
```

Instead of instantiation the `CommandManager` class, you use your generated class instead.

If you had the following code before using source generation:

```csharp
var manager = new CommandManager();
return manager.RunCommand() ?? 1;
```

You would replace it with the following:

```csharp
var manager = new MyCommandManager();
return manager.RunCommand() ?? 1;
```

### Commands with generated parsers

You can apply the `GeneratedParserAttribute` to a command class, and a generated command manager
will use the generated parser for that command.

```csharp
[Command]
[GeneratedParser]
partial class MyCommand : ICommand
{
    [CommandLineArgument]
    public string? SomeArgument { get; set; }

    public int Run()
    {
        /* ... */
    }
}
```

Note that if you create a normal `CommandManager` instance which uses reflection, it will always use
reflection to create a parser for its commands, even if the command has the
`GeneratedParserAttribute`.

The `GeneratedParserAttribute` works the same for command classes as it does for any other arguments
class, with one exception: the static `Parse()` methods are not generated by default for command
classes. You must explicitly set the `GeneratedParserAttribute.GenerateParseMethods` to `true` if
you want them to be generated.

Next, we will take a look at several [utility classes](Utilities.md) provided, and used, by
Ookii.CommandLine.
