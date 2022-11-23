# Subcommands

Ookii.CommandLine allows you to create applications that have multiple commands, each with their own
arguments. This is a common pattern used by many applications; for example, the `dotnet` binary
uses it with commands like `dotnet build` and `dotnet run`, as does `git` with commands like
`git pull` and `git cherry-pick`.

Ookii.CommandLine makes it trivial to define and use subcommands, using the same techniques we've
already seen for defining and parsing arguments. Subcommand specific functionality is all in the
`Ookii.CommandLine.Commands` namespace.

In an application using subcommands, the first argument to the application is the name of the
command. The remaining arguments are arguments to that command. You cannot have arguments that are
not associated with a command using the subcommand functionality in Ookii.CommandLine, though you
can still easily define [common arguments](#multiple-commands-with-common-arguments).

For example, the [subcommand sample](../src/Samples/SubCommand) can be invoked as follows:

```text
./SubCommand read file.txt -Encoding utf-16
```

This command line invokes the command named `read`, and passes the remaining arguments to that
command.

## Defining subcommands

A subcommand class is essentially no different than a [regular arguments class](DefiningArguments.md).
Arguments can be defined using its constructor parameters, properties and method, exactly as was
shown before.

Subcommand classes have a few differences from regular arguments classes:

1. They must implement the `ICommand` interface.
2. They must use the `CommandAttribute` attribute.
3. The `DescriptionAttribute` sets the description for the command, not the application.
4. You can't apply the `ApplicationFriendlyNameAttribute` to a command class (apply it to the
   assembly instead).
5. An automatic `-Version` argument will not be created for subcommands, regardless of the value of
   the `ParseOptions.AutoVersionArgument` property.

It's therefore trivial to take any arguments class, and convert it into a subcommand:

```csharp
[Command("sample")]
[Description("This is a sample command.")]
class SampleCommand : ICommand
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    [Description("A sample argument for the sample command.")]
    public string? SampleArgument { get; set; }

    public int Run()
    {
        // Command functionality goes here.
        return 0;
    }
}
```

This code creates a subcommand which can be invoked with the name `sample`, and which has a single
positional required argument.

The `ICommand` interface defines a single method, `ICommand.Run()`, which all subcommands must
implement. This function is invoked to run your command. The return value is typically used as the
exit code for the application, after the command finishes running.

When using the `CommandManager` class as [shown below](#using-subcommands), the class will be
created using the `CommandLineParser` as usual, using all the arguments except for the command name.
Then, the `ICommand.Run()` method will be called.

All of the functionality available with regular arguments types is available with commands too,
including [usage help generation](#subcommand-usage-help), [long/short mode](Arguments.md#longshort-mode),
all types or arguments, validators, etc.

While you can use the `ParseOptionsAttribute` to customize the behavior of a subcommand class, keep
in mind that this only applies to the class with that attribute. For this reason, it's typically
better to use the `CommandOptions` class (which derives from `ParseOptions`) instead, to ensure
consistent behavior between all your commands.

### Name transformation

The sample above used the `CommandAttribute` attribute to set an explicit name for the command. If
no named is specified, the name is derived from the type name.

```csharp
[Command]
class ReadDirectoryCommand : ICommand
{
    /* omitted */
}
```

This creates a command with the name `ReadDirectoryCommand`.

Just like with argument names and value descriptions, it's possible to apply a name transformation
to command names. This is done by setting the `CommandOptions.CommandNameTransform` property. The
[same transformations](DefiningArguments.md#name-transformation) are available as for argument
names.

In addition to just transforming the case and separators, command name transformation can also strip
a suffix from the end of the type name. This is set with the `CommandOptions.StripCommandNameSuffix`
property, and defaults to "Command". This is only used if the `CommandNameTransform` is not
`NameTransform.None`.

So, if you use the `NameTransform.DashCase` transform, with the default `StripCommandNameSuffix`
value, the `ReadDirectoryCommand` class above will create a command named `read-directory`.

### Command aliases

Like argument names, a command can have one or more aliases, alternative names that can be used
to invoke the command. Simply apply the `AliasAttribute` to the command class.

```csharp
[Command]
[Alias("ls")]
class ReadDirectoryCommand : ICommand
{
    /* omitted */
}
```

### Asynchronous commands

It's possible to use asynchronous code with subcommands. To do this, implement the `IAsyncCommand`
interface, which derives from `ICommand`. This interface adds the `IAsyncCommand.RunAsync()` method.

Because `IAsyncCommand` derives from `ICommand`, it's still necessary to implement the
`ICommand.Run()` method. The `AsyncCommandBase` class is provided for convenience, which provides
an implementation of `ICommand.Run()` which invokes `IAsyncCommand.RunAsync()` and waits for it.

```csharp
[Command]
[Description("Sleeps for a specified amount of time.")]
class AsyncSleepCommand : AsyncCommandBase
{
    [CommandLineArgument(Position = 0, DefaultValue = 1000)]
    [Description("The sleep time in milliseconds.")]
    public int SleepTime;

    public override async Task<int> RunAsync()
    {
        await Task.Delay(SleepTime);
        return 0;
    }
}
```

### Multiple commands with common arguments

Sometimes, you have multiple commands that all need some of the same arguments. For example, you may
have a database application where every command needs the connection string as an argument. Because
`CommandLineParser` considers base class members when defining arguments, this can be accomplished
by having a common base class for each command that needs the common arguments.

```csharp
abstract class DatabaseCommand : ICommand
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    public string? ConnectionString { get; set; }

    public abstract int Run();
}

[Command]
class AddCommand : DatabaseCommand
{
    [CommandLineArgument(Position = 1, IsRequired = true)]
    public string? NewValue { get; set; }

    public override int Run()
    {
        /* omitted */
    }
}

[Command]
class DeleteCommand : DatabaseCommand
{
    [CommandLineArgument(Position = 1, IsRequired = true)]
    public int Id { get; set; }

    [CommandLineArgument]
    public bool Force { get; set; }

    public override int Run()
    {
        /* omitted */
    }
}
```

The two commands, `AddCommand` and `DeleteCommand` both inherit the `ConnectionString` argument, and
add their own additional arguments.

The `DatabaseCommand` class is not considered a subcommand by the `CommandManager`, because it does
not have the `CommandAttribute` attribute, and because it is abstract.

### Custom parsing

In rare cases, you may want to create commands that do not use the `CommandLineParser` class to parse
their arguments. For this purpose, you can implement the `ICommandWithCustomParsing` method instead.
You must still use the `CommandAttribute`.

Your type must have a constructor with no parameters, and implement the
`ICommandWithCustomParsing.Parse()` method, which will be called before `ICommand.Run()` to allow
you to parse the command line arguments.

## Using subcommands

To write an application that uses subcommands, you use the `CommandManager` class in the `Main()`
method of your application.

In the majority of cases, it's sufficient to write code like the following.

```csharp
public static int Main()
{
    var manager = new CommandManager();
    return manager.RunCommand() ?? 1;
}
```

This code does the following:

1. Creates a command manager with default options, which looks for command classes in the assembly
   that called the constructor (the assembly containing `Main()`, in this case).
2. Calls the `RunCommand()` method, which:
   1. Gets the arguments using `Environment.GetCommandLineArgs()` (you can also pass a `string[]` array
      to the `RunCommand` method).
   2. Uses the first argument to determine the command name.
   3. Creates the command, and invokes the `ICommand.Run()` method, and returns its return value.
   4. If the command could not be created, for example because no command name was supplied, an unknown
      command name was supplied, or an error occurred parsing the commands arguments, it will print
      the error message and usage help, similar to the static `CommandLineParser.Parse<T>()` method,
      and return `null`.
3. If `RunCommand()` returned null, returns an error exit code.

If you use the `IAsyncCommand` interface or `AsyncCommandBase` class, use the following code instead.

```csharp
public static async Task<int> Main()
{
    var manager = new CommandManager();
    return await manager.RunCommandAsync() ?? 1;
}
```

Note that the `RunCommandAsync()` method can still run commands that only implement `ICommand`, and
not `ICommandAsync`, so you can freely mix both types of command.

TODO: options, other CommandManager methods.

To use a shell command, you must first determine the shell command the user wishes to invoke, typically by inspecting the first element of the array of arguments passed to the `Main` method of your application.

You can then get the `Type` instance of the shell command’s class by calling the `ShellCommand.GetShellCommand` method. This method searches the specified assembly for a type that inherits from the `ShellCommand` class, and has the `ShellCommandAttribute` attribute applied with the `ShellCommandAttribute.Name` property set to the specified name. You can also get a list of all shell commands in an assembly by using the `ShellCommand.GetShellCommands` method.

This `Type` instance can be passed to the constructor of the `CommandLineParser` class, after which you can parse arguments for the command as usual (make sure to pass an index so that the command name is not treated as an argument), and finally invoke its `ShellCommand.Run` method.

The `ShellCommand` class provides static utility methods that perform these tasks for you. The `ShellCommand.CreateShellCommand` method finds and creates a shell command, and writes error and usage information to the output if it failed. If no command name was specified, or the specified command name could not be found, it writes a list of all shell commands in the assembly and their descriptions to the output. If the command was found but parsing its arguments failed, it writes usage information for that command to the output.

The `ShellCommand.RunShellCommand` method works the same as the `ShellCommand.CreateShellCommand` method, but also invokes the `ShellCommand.Run` method if the command was successfully created.

It is recommended to return the value of the `ShellCommand.ExitCode` property to the operating system (by returning it from the `Main` method or by using the `Environment.``ExitCode` property) after running the shell command.

The source code of a full sample application that defines two commands is included with the Ookii.CommandLine library.

## Subcommand usage help

TODO
