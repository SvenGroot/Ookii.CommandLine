# Source generation

Ookii.CommandLine provides the option to use compile-time source generation to create a
`CommandLineParser<T>` for an arguments type, and to create a `CommandManager`. Source generation
is only available for C# projects.

## Generating a parser

Normally, the `CommandLineParser` class uses runtime reflection to determine the command line
arguments defined by an arguments class. Instead, you can apply the `GeneratedParserAttribute` to
a class, which will use compile-time C# source generation to determine the arguments instead. This
approach has the following advantages:

- Get [errors and warnings](TODO) at compile time for argument rule violations (such as a required
  positional argument after an optional positional argument), ignored options (such as setting a
  default value for a required attribute), and other problems (such as using the same position
  number more than once, method arguments with the wrong signature, or using the
  `CommandLineArgumentAttribute` on a private or read-only property). These would normally be
  silently ignored or cause a runtime exception, but now you can catch problems during compilation.
- Allow your application to be
  [trimmed](https://learn.microsoft.com/dotnet/core/deploying/trimming/trimming-options). The way
  Ookii.CommandLine uses reflection prevents trimming entirely.
- Improved performance. Benchmarks show that instantiating a `CommandLineParser<T>` using a
  generated parser is up to thirty times faster than using reflection. However, since we're still
  talking about microseconds, this is unlikely to matter much unless you're creating instances in a
  loop for some reason.

Generally, it's recommended to use source generation unless you have a reason not to. Source
generation puts the following constraints on application:

- The arguments class must be in a project using C# 8 or later.
- The project must be compiled using a recent version of the .Net SDK (TODO: Exact version). You
  can target older runtimes supported by Ookii.CommandLine, down to .Net Framework 4.6, but you
  must build the project using an SDK that supports the source generator.
- If you use the `ArgumentConverterAttribute`, you must use the constructor that takes a `Type`
  instance. The constructor that takes a string is not supported.
- The arguments class may not be nested in another type.
- The arguments class may not be a generic type.

Other than that, source generation offers all the same features that reflection does.

To generate a parser, you must mark the class as `partial`, and use the `GeneratedParserAttribute`
on the class.

```csharp
[GeneratedParser]
partial class Arguments
{
    [CommandLineArgument]
    public string? SomeArgument { get; set; }
}
```

### Using a generated parser

When using the `GeneratedParserAttribute`, you must *not* use the regular `CommandLineParser` or
`CommandLineParser<T>` constructor, or the static `CommandLineParser.Parse<T>()` methods. These will
still use reflection, even if a generated parser is available for a class.

> By default, these constructors and methods will throw an exception if you try to use them with a
> class that has the `GeneratedParserAttribute`, to prevent accidentally using reflection when it
> was not intended. If for some reason you need to use reflection on a class that has that
> attribute, you can set the `ParseOptions.AllowReflectionWithGeneratedParser` property to `true`.

Instead, the source generator will add the following methods to the arguments class (where
`Arguments` is the name of your class):

```csharp
public static CommandLineParser<Arguments> CreateParser(ParseOptions? options = null);

public static Arguments? Parse(ParseOptions? options = null);

public static Arguments? Parse(string[] args, ParseOptions? options = null);

public static Arguments? Parse(string[] args, int index, ParseOptions? options = null);
```

If your project target .Net 7 or later, these methods will implement the `IParserProvider<TSelf>`
and `IParser<TSelf>` interfaces.

Generating the `Parse()` methods is optional and can be disabled using the
`GeneratedParserAttribute.GenerateParseMethods` property. The `CreateParser()` method is always
generated.

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

## Generating a command manager

Just like the `CommandLineParser` class, the `CommandManager` class normally uses reflection to
locate all command classes in the assembly or assemblies you specify. Instead, you can create a
class with the `GeneratedCommandManagerAttribute` which can perform this same job at compile time.

Using a generated command manager has the same benefits and restrictions as a generated parser,
with one additional caveat: a generated command manager can only use commands defined in the same
assembly as the manager (TODO: update if changed). If you use commands from different or multiple
assemblies, you must continue to use the reflection method.

To create a generated command manager, define a partial class with the
`GeneratedCommandManagerAttribute`:

```csharp
[GeneratedCommandManager]
partial class MyCommandManager
{
}
```

The source generator will make it so that the class derives from `CommandManager`, and add the
following constructor to the class:

```csharp
public MyCommandManager(CommandOptions? options = null)
```

If you had the following code before using source generation:

```csharp
var manager = new CommandManager();
return manager.RunCommand() ?? 1;
```

You would simply replace it with the following:

```csharp
var manager = new MyCommandManager();
return manager.RunCommand() ?? 1;
```

### Commands with generated parsers

You can apply the `GeneratedParserAttribute` to a command class, and the generated command manager
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
