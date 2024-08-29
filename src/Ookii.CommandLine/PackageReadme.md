# Ookii.CommandLine

Ookii.CommandLine is a powerful, flexible and highly customizable command line argument parsing
library for .Net applications.

- Easily define arguments by creating a class with properties.
- Create applications with multiple subcommands.
- Generate fully customizable usage help.
- Supports PowerShell-like and POSIX-like parsing rules.
- Compatible with trimming and native AOT.

Two styles of command line parsing rules are supported: the default mode uses rules similar to those
used by PowerShell, and the alternative long/short mode uses a style influenced by POSIX
conventions, where arguments have separate long and short names with different prefixes. Many
aspects of the parsing rules are configurable.

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

In addition, Ookii.CommandLine can be used to create applications that have multiple subcommands,
each with their own arguments.

For more information, including a tutorial and samples, see the [full documentation on GitHub](https://github.com/SvenGroot/Ookii.CommandLine).
