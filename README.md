# Ookii.CommandLine [![NuGet](https://img.shields.io/nuget/v/Ookii.CommandLine)](https://www.nuget.org/packages/Ookii.CommandLine/)

Ookii.CommandLine is a powerful, flexible and highly customizable command line argument parsing
library for .Net applications.

- Easily define arguments by creating a class with properties.
- Create applications with multiple subcommands.
- Generate fully customizable usage help.
- Supports PowerShell-like and POSIX-like parsing rules.
- Trim-friendly

Ookii.CommandLine is [provided in versions](#requirements) for .Net Standard 2.0, .Net Standard 2.1,
.Net 6.0, and .Net 7.0 and later.

Ookii.CommandLine can be added to your project using [NuGet](https://nuget.org/packages/Ookii.CommandLine).
[Code snippets](docs/CodeSnippets.md) for Visual Studio are available on the
[Visual Studio marketplace](https://www.ookii.org/Link/CommandLineSnippets).

A [C++ version](https://github.com/SvenGroot/Ookii.CommandLine.Cpp) is also available.

## Overview

Ookii.CommandLine is a library that lets you parse the command line arguments for your application
into a set of strongly-typed, named values. You can easily define the accepted arguments, and then
parse the supplied arguments for those values. In addition, you can generate usage help that can be
displayed to the user.

Ookii.CommandLine can be used with any kind of .Net application, whether console or GUI. Some
functionality, such as creating usage help, is primarily designed for console applications, but
even those can be easily adapted for use with other styles of applications.

Two styles of [command line parsing rules](docs/Arguments.md) are supported: the default mode uses
rules similar to those used by PowerShell, and the alternative [long/short mode](docs/Arguments.md#longshort-mode)
uses a style influenced by POSIX conventions, where arguments have separate long and short names
with different prefixes. Many aspects of the parsing rules are configurable.

To determine which arguments are accepted, you create a class, with properties and methods that
define the arguments. Attributes are used to specify names, create required or positional arguments,
and to specify descriptions for use in the generated usage help.

For example, the following class defines four arguments: a required positional argument, an optional
positional argument, a named-only argument, and a switch argument (sometimes also called a flag):

```csharp
[GeneratedParser]
partial class MyArguments
{
    [CommandLineArgument(IsPositional = true)]
    [Description("A required positional argument.")]
    public required string Required { get; set; }

    [CommandLineArgument(IsPositional = true)]
    [Description("An optional positional argument.")]
    public int Optional { get; set; } = 42;

    [CommandLineArgument]
    [Description("An argument that can only be supplied by name.")]
    public DateTime Named { get; set; }

    [CommandLineArgument]
    [Description("A switch argument, which doesn't require a value.")]
    public bool Switch { get; set; }
}
```

Each argument has a different type that determines the kinds of values it can accept.

> If you are using an older version of .Net where the `required` keyword is not available, you can
> use `[CommandLineArgument(IsRequired = true)]` to create a required argument instead.

To parse these arguments, all you have to do is add the following line to your `Main` method:

```csharp
var arguments = MyArguments.Parse();
```

The `Parse()` method is added to the class through [source generation](docs/SourceGeneration.md).

This method will take the arguments from `Environment.GetCommandLineArgs()` (you can also manually
pass a `string[]` array if you want), will handle and print errors to the console, and will print
usage help if needed. It returns an instance of `MyArguments` if successful, and `null` if not.

If the arguments are invalid, or help is requested, this application will print the following usage
help:

```text
Usage: MyApplication [-Required] <String> [[-Optional] <Int32>] [-Help] [-Named <DateTime>]
   [-Switch] [-Version]

    -Required <String>
        A required positional argument.

    -Optional <Int32>
        An optional positional argument with a default value. Default value: 42.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Named <DateTime>
        An argument that can only supplied by name.

    -Switch [<Boolean>]
        A switch argument, which doesn't require a value.

    -Version [<Boolean>]
        Displays version information.
```

The [usage help](docs/UsageHelp.md) includes the descriptions given for the arguments, as well as
things like default values and aliases. The usage help format can also be
[fully customized](src/Samples/CustomUsage).

The application also has two arguments that weren't in the class, `-Help` and `-Version`, which are
automatically added by default.

An example invocation of this application, specifying all the arguments, would look like this
(argument names are case insensitive by default):

```text
./MyApplication foo 42 -switch -named 2022-08-14
```

In addition, Ookii.CommandLine can be used to create applications that have [multiple subcommands](docs/Subcommands.md),
each with their own arguments.

[Try it yourself on .Net Fiddle](https://dotnetfiddle.net/fgLvSl), or
[try out subcommands](https://dotnetfiddle.net/vGIG78).

## Requirements

Ookii.CommandLine is a class library for use in your own applications for [Microsoft .Net](https://dotnet.microsoft.com/).
It can be used with applications supporting one of the following:

- .Net Standard 2.0
- .Net Standard 2.1
- .Net 6.0
- .Net 7.0 and later

As of version 3.0, .Net Framework 2.0 is no longer supported. You can still target .Net Framework
4.6.1 and later using the .Net Standard 2.0 assembly. If you need to support an older version of
.Net, please continue to use [version 2.4](https://github.com/SvenGroot/ookii.commandline/releases/tag/v2.4).

The .Net Standard 2.1 and .Net 6.0 and 7.0 versions utilize the framework `ReadOnlySpan<T>` and
`ReadOnlyMemory<T>` types without a dependency on the System.Memory package.

The .Net 6.0 version has additional support for [nullable reference types](docs/Arguments.md#arguments-with-non-nullable-types),
and is annotated to allow [trimming](https://learn.microsoft.com/dotnet/core/deploying/trimming/trimming-options)
when [source generation](docs/SourceGeneration.md) is used.

The .Net 7.0 version has additional support for `required` properties, and can utilize
`ISpanParsable<TSelf>` and `IParsable<TSelf>` for argument value conversions.

## Building and testing

To build Ookii.CommandLine, make sure you have the following installed:

- [Microsoft .Net 7.0 SDK](https://dotnet.microsoft.com/download) or later
- [Microsoft PowerShell 6 or later](https://github.com/PowerShell/PowerShell)

PowerShell is used to generate some source files during the build. Besides installing it normally,
you can also install it as a .Net global tool using `dotnet tool install PowerShell --global`.

To build the library, tests and samples, simply use the `dotnet build` command in the `src`
directory. You can run the unit tests using `dotnet test`. The tests should pass on all platforms
(Windows and Linux have been tested).

The tests are built and run for .Net 7.0, .Net 6.0, and .Net Framework 4.8. Running the .Net
Framework tests on a non-Windows platform may require the use of [Mono](https://www.mono-project.com/).

Ookii.CommandLine uses a strongly-typed resources file, which will not update correctly unless the
`Resources.resx` file is edited with [Microsoft Visual Studio](https://visualstudio.microsoft.com/).
I could not find a way to make this work correctly with both Visual Studio and the `dotnet` command.

The class library documentation is generated using [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB).

## Comparing with System.CommandLine

Back when Ookii.CommandLine was first written, there was no official way to parse command line
arguments; the only way was a third-party library, or do it yourself.

Nowadays, System.CommandLine offers an official Microsoft solution for command line parsing. Why,
then, should you use Ookii.CommandLine?

Here are some of the most important differences (as of this writing, and to the best of my knowledge):

Ookii.CommandLine                                                             | System.CommandLine
------------------------------------------------------------------------------|---------------------------------------------------------------------------------------
Declarative approach to defining arguments with properties and attributes.    | Fluent API with a builder pattern to define arguments.
Supports PowerShell-like and POSIX-like parsing rules.                        | Supports POSIX-like rules with some modifications.
Supports any type with a `Parse()` method or constructor that takes a string. | Supports a limited number of types, and requires custom conversion methods for others.
Supports automatic prefix aliases.                                            | Does not support automatic prefix aliases.
Does not support middleware or dependency injection.                          | Supports middleware and dependency injection.
Fully released with a stable API between major releases.                      | Still in preview.

These are by no means the only differences. Both are highly customizable, and each has its pros and
cons. In the end, it mostly comes down to personal preference. You should use whichever one suits
your needs and coding style best.

## More information

Please check out the following to get started:

- [Tutorial: getting started with Ookii.CommandLine](docs/Tutorial.md)
- [Migrating from Ookii.CommandLine 2.x / 3.x](docs/Migrating.md)
- [Usage documentation](docs/README.md)
- [Class library documentation](https://www.ookii.org/Link/CommandLineDoc)
- [Sample applications](src/Samples) with detailed explanations and sample output.
