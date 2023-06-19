# Subcommands

Ookii.CommandLine allows you to create applications that have multiple commands, each with their own
arguments. This is a common pattern used by many applications; for example, the `dotnet` binary
uses it with commands like `dotnet build` and `dotnet run`, as does `git` with commands like
`git pull` and `git cherry-pick`.

Ookii.CommandLine makes it trivial to define and use subcommands, using the same techniques we've
already seen for defining and parsing arguments. Subcommand-specific functionality is all in the
[`Ookii.CommandLine.Commands`][] namespace.

In an application using subcommands, the first argument to the application is typically the name of
the command. The remaining arguments are arguments to that command. Sometimes, there are also
arguments that are [common to all commands](#multiple-commands-with-common-arguments).

For example, the [subcommand sample](../src/Samples/Subcommand) can be invoked as follows:

```text
./Subcommand read file.txt -Encoding utf-16
```

This command line invokes the command named `read`, and passes the remaining arguments to that
command.

## Defining subcommands

A subcommand class is essentially the same as a [regular arguments class](DefiningArguments.md).
Arguments can be defined using its properties and methods, exactly as was shown before.

Subcommand classes have the following differences from regular arguments classes:

1. They must implement the [`ICommand`][] interface.
2. They must use the [`CommandAttribute`][] attribute.
3. The [`DescriptionAttribute`][] sets the description for the command, not the application.
4. You can't apply the [`ApplicationFriendlyNameAttribute`][] to a command class (apply it to the
   assembly instead).
5. An automatic `-Version` argument will not be created for subcommands, regardless of the value of
   the [`ParseOptions.AutoVersionArgument`][] property.

It's therefore trivial to take any arguments class, and convert it into a subcommand:

```csharp
[GeneratedParser]
[Command("sample")]
[Description("This is a sample command.")]
partial class SampleCommand : ICommand
{
    [CommandLineArgument(IsPositional = true)]
    [Description("A sample argument for the sample command.")]
    public required string SampleArgument { get; set; }

    public int Run()
    {
        // Command functionality goes here.
        return 0;
    }
}
```

This code creates a subcommand which can be invoked with the name `sample`, and which has a single
positional required argument.

The [`ICommand`][] interface defines a single method, [`ICommand.Run()`][], which all subcommands
must implement. This function is invoked to run your command. The return value is typically used as
the exit code for the application, after the command finishes running.

When using the [`CommandManager`][] class as [shown below](#using-subcommands), the class will be
created using the [`CommandLineParser`][] as usual, using all the arguments except for the command
name. Then, the [`ICommand.Run()`][] method will be called.

All of the functionality and [options](#subcommand-options) available with regular arguments types
are available with commands too, including [usage help generation](#subcommand-usage-help),
[long/short mode](Arguments.md#longshort-mode), all kinds of arguments, validators, source
generation, etc.

### Name transformation

The sample above used the [`CommandAttribute`][] attribute to set an explicit name for the command. If
no name is specified, the name is derived from the type name.

```csharp
[GeneratedParser]
[Command]
partial class ReadDirectoryCommand : ICommand
{
    /* omitted */
}
```

This creates a command with the name `ReadDirectoryCommand`.

Just like with argument names and value descriptions, it's possible to apply a name transformation
to command names. This is done by setting the [`CommandOptions.CommandNameTransform`][] property. The
[same transformations](DefiningArguments.md#name-transformation) are available as for argument
names.

In addition to just transforming the case and separators, command name transformation can also strip
a suffix from the end of the type name. This is set with the
[`CommandOptions.StripCommandNameSuffix`][] property, and defaults to "Command". This is only used
if the [`CommandNameTransform`][] is not [`NameTransform.None`][].

So, if you use the [`NameTransform.DashCase`][] transform, with the default [`StripCommandNameSuffix`][]
value, the `ReadDirectoryCommand` class above will create a command named `read-directory`.

### Command aliases

Like argument names, a command can have one or more aliases, alternative names that can be used
to invoke the command. Simply apply the [`AliasAttribute`][] to the command class.

```csharp
[GeneratedParser]
[Command]
[Alias("ls")]
partial class ReadDirectoryCommand : ICommand
{
    /* omitted */
}
```

Command names also use automatic prefix aliases by default, so any prefix that uniquely identifies a
command by its name or one of its explicit aliases can be used to invoke that command.

For example, with two commands `read` and `record`, the prefix `rea` would be an alias for the
`read` command, and `rec`, `reco` and `recor` are automatic aliases of the `record` command. The
prefixes `r` and `re` are not automatic aliases, because they are ambiguous between the two
commands.

Automatic prefix aliases for command names can be disabled using the
[`CommandOptions.AutoCommandPrefixAliases`][] property.

### Asynchronous commands

It's possible to use asynchronous code with subcommands. To do this, implement the
[`IAsyncCommand`][] interface, which derives from [`ICommand`][], and use the
[`CommandManager.RunCommandAsync()`][] method (see [below](#using-subcommands)).

The [`IAsyncCommand`][] interface adds a new [`IAsyncCommand.RunAsync()`][] method, but because
[`IAsyncCommand`][] derives from [`ICommand`][], it's still necessary to implement the
[`ICommand.Run()`][] method. If you use [`RunCommandAsync()`][], the [`ICommand.Run()`][] method is
guaranteed to never be called on a command that implements [`IAsyncCommand`][], so you can just
leave this empty.

However, a better option is to use the [`AsyncCommandBase`][] class, which is provided for
convenience, and provides an implementation of [`ICommand.Run()`][] which invokes
[`IAsyncCommand.RunAsync()`][] and waits for it. That way, your command is compatible with both
[`RunCommand()`][] and [`RunCommandAsync()`][].

```csharp
[GeneratedParser]
[Command]
[Description("Sleeps for a specified amount of time.")]
partial class AsyncSleepCommand : AsyncCommandBase
{
    [CommandLineArgument(IsPositional = true)]
    [Description("The sleep time in milliseconds.")]
    public int SleepTime { get; set; } = 1000;

    public override async Task<int> RunAsync()
    {
        await Task.Delay(SleepTime);
        return 0;
    }
}
```

### Multiple commands with common arguments

You may have multiple commands that have one or more arguments in common. For example, you may have
a database application where every command needs the connection string as an argument. Because
[`CommandLineParser`][] considers base class members when defining arguments, this can be
accomplished by having a common base class for each command that needs the common arguments.

```csharp
abstract class DatabaseCommand : ICommand
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    public string? ConnectionString { get; set; }

    public abstract int Run();
}

[GeneratedParser]
[Command]
partial class AddCommand : DatabaseCommand
{
    [CommandLineArgument(Position = 1, IsRequired = true)]
    public string? NewValue { get; set; }

    public override int Run()
    {
        /* omitted */
    }
}

[GeneratedParser]
[Command]
partial class DeleteCommand : DatabaseCommand
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

The two commands, `AddCommand` and `DeleteCommand` both inherit the `-ConnectionString` argument, and
add their own additional arguments.

The `DatabaseCommand` class is not considered a subcommand by the [`CommandManager`][], because it
does not have the [`CommandAttribute`][] attribute, and because it is abstract. It also does not
need the [`GeneratedParserAttribute`][], because the attribute on the derived classes will process the
base class arguments.

Some applications also have options that don't belong to any specific command, but can instead be
specified before the command name. The default behavior of Ookii.CommandLine treats the first
argument as the command name, but it is possible to build an application where this is not the case.

To do so, you need to define an arguments class (not a subcommand) that defines the top-level
arguments, one of which (typically the last positional argument) is the command name. That argument
should set the [`CommandLineArgumentAttribute.CancelParsing`][] property to [`CancelMode.Success`][]. After
parsing the arguments for this class, you can then invoke the [`CommandManager`][] using the command
name from that argument, and the remaining arguments from the [`ParseResult.RemainingArguments`][]
property.

An example of how to do this can be found in the [top-level arguments sample](../src/Samples/TopLevelArguments).

### Custom parsing

In some cases, you may want to create commands that do not use the [`CommandLineParser`][] class to
parse their arguments. For this purpose, you can implement the [`ICommandWithCustomParsing`][]
method instead. You must still use the [`CommandAttribute`][].

Your type must have a constructor with no parameters, and implement the
[`ICommandWithCustomParsing.Parse()`][] method, which will be called before [`ICommand.Run()`][] to
allow you to parse the command line arguments. You can combine [`ICommandWithCustomParsing`][] with
[`IAsyncCommand`][] if you wish.

In this case, it is up to the command to handle argument parsing, and handle errors and display
usage help if appropriate. Source generation cannot be used with a command that uses custom parsing.

For example, you may have a command that launches an external executable, and wants to pass the
arguments to that executable.

```csharp
[Command]
class LaunchCommand : AsyncCommandBase, ICommandWithCustomParsing
{
    private ReadOnlyMemory<string> _args;

    public void Parse(ReadOnlyMemory<string> args, CommandManager manager)
    {
        _args = args;
    }

    public override async Task<int> RunAsync()
    {
        var info = new ProcessStartInfo("executable");
        if (_args != null)
        {
            foreach (var arg in _args)
            {
                info.ArgumentList.Add(arg);
            }
        }

        var process = Process.Start(info);
        if (process != null)
        {
            await process.WaitForExitAsync();
            return process.ExitCode;
        }

        return 1;
    }
}
```

## Using subcommands

To write an application that uses subcommands, you use the [`CommandManager`][] class in the `Main()`
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
2. Calls the [`RunCommand()`][] method, which:
   1. Gets the arguments using [`Environment.GetCommandLineArgs()`][] (you can also pass a
      `string[]` array to the [`RunCommand`][] method).
   2. Uses the first argument to determine the command name.
   3. Creates the command, invokes the [`ICommand.Run()`][] method, and returns its return value.
   4. If the command could not be created, for example because no command name was supplied, an
      unknown command name was supplied, or an error occurred parsing the command's arguments, it
      will print the error message and usage help, similar to the static
      [`CommandLineParser.Parse<T>()`][] method, and return null.
3. If [`RunCommand()`][] returned null, returns an error exit code.

> Note: the [`CommandManager`][] does not check if command names and aliases are unique. If you have
> multiple commands with the same names, the first matching one will be used, and there is no
> guarantee on the order in which command classes are checked.

If you use the [`IAsyncCommand`][] interface or [`AsyncCommandBase`][] class, use the following code instead.

```csharp
public static async Task<int> Main()
{
    var manager = new CommandManager();
    return await manager.RunCommandAsync() ?? 1;
}
```

Note that the [`RunCommandAsync()`][] method can still run commands that only implement
[`ICommand`][], and not [`IAsyncCommand`][], so you can freely mix both types of command.

If you use [`RunCommand()`][] with asynchronous commands, it will call the [`ICommand.Run()`][]
method, so whether this works depends on the command's implementation of that method. If you used
[`AsyncCommandBase`][], this will call the [`RunAsync()`][RunAsync()_0] method, so the command will
work correctly. However, in all cases, it's strongly recommended to use [`RunCommandAsync()`][] if
you use any asynchronous commands.

Check out the [tutorial](Tutorial.md) and the [subcommand sample](../src/Samples/Subcommand) for
more detailed examples of how to create and use commands.

### Other assemblies

The default constructor for the [`CommandManager`][] class will look for command classes only in the
calling assembly. If your command classes are all in the same assembly as your main method, this
will be sufficient. However, you may want to have your commands in a separate assembly, or split
amongst several assemblies. You could even want to dynamically load plugins with additional commands.

The [`CommandManager`][] constructor has overloads that take a single assembly, or an array of
assemblies. This allows you to load commands from one or more sources. You can even filter which
commands you actually want to use from those assemblies using the [`CommandOptions.CommandFilter`][]
property.

```csharp
public static int Main()
{
    var assemblies = new[] { Assembly.GetExecutingAssembly() };
    assemblies = assemblies.Concat(LoadPlugins()).ToArray();
    var manager = new CommandManager(assemblies);
    return manager.RunCommand() ?? 1;
}
```

The omitted `LoadPlugins()` method would presumably load some list of assemblies from the
application's configuration.

### Using source generation with subcommands

While the [`GeneratedParserAttribute`][] can be applied to commands, and the generated parser will be
used by the [`CommandManager`][] class, the [`CommandManager`][] class still uses reflection to
find the subcommand classes in the specified assemblies.

To use [source generation](SourceGeneration.md#generating-a-command-manager) to find the commands at
compile time and provide that information to a generated command manager, you must define a class as
follows, using the [`GeneratedCommandManagerAttribute`][]:

```csharp
[GeneratedCommandManager]
partial class GeneratedManager
{
}
```

The source generator will make this class inherit from [`CommandManager`][], so it can be used as
a drop-in replacement for [`CommandManager`][].

```csharp
public static async Task<int> Main()
{
    var manager = new GeneratedManager();
    return await manager.RunCommandAsync() ?? 1;
}
```

In this case, if you want to use commands from other assemblies, you must specify them using the
[`GeneratedCommandManagerAttribute`][], and they can only come from assemblies that are directly
referenced from your application, not dynamically loaded ones.

### Subcommand options

Just like when you use [`CommandLineParser`][] directly, there are many options available to
customize the parsing behavior. When using [`CommandManager`][], you use the [`CommandOptions`][]
class to provide options. This class derives from [`ParseOptions`][], so all the same options are
available, in addition to several options that apply only to subcommands.

> While you can use the [`ParseOptionsAttribute`][] to customize the behavior of a subcommand class,
> this will only apply to the class using the attribute. For a consistent experience, it's preferred
> to use [`CommandOptions`][].

For example, the following code enables some options:

```csharp
public static int Main()
{
    var options = new CommandOptions()
    {
        CommandNameComparison = StringComparison.InvariantCulture,
        CommandNameTransform = NameTransform.DashCase,
        UsageWriter = new UsageWriter()
        {
            IncludeApplicationDescriptionBeforeCommandList = true,
        }
    };

    var manager = new CommandManager(options); // or a generated command manager.
    return manager.RunCommand() ?? 1;
}
```

This code makes command names case sensitive by using the invariant string comparer (the default is
[`StringComparison.OrdinalIgnoreCase`][], which is case insensitive), enables a name transformation,
and also sets some [usage help options](#subcommand-usage-help).

### Custom error handling

As with the static [`CommandLineParser.Parse<T>()`][] method, [`RunCommand()`][] and
[`RunCommandAsync()`][] handle errors and display usage help. If for any reason you want to do this
manually, [`CommandManager`][] provides the tools to do so.

If you only want more information about the error, but still want the [`CommandManager`][] class to
handle and display errors and usage help, you can check the [`CommandManager.ParseResult`][]
property to get information if [`RunCommand()`][] or [`RunCommandAsync()`][] returned null. The
value of the [`ParseResult.Status`][] property of the returned structure will indicate whether the
command was not found, if an error occurred parsing the command's arguments, or if parsing was
canceled by one of the command's arguments.

If you want to handle errors entirely manually, the [`CommandManager.GetCommand()`][] method returns
information about a command, if one with the specified name exists. From there, you can manually
create a [`CommandLineParser`][] for the command, instantiate the class, and invoke its run method.

When doing this, it's your responsibility to handle things such as [`IAsyncCommand`][] or
[`ICommandWithCustomParsing`][]. Of course, you can omit those parts if you do not have any commands
using those interfaces.

Because of the complexity of this approach, it's probably easier to just redirect the error and
output of the regular `RunCommand(Async)` methods, as shown below:

```csharp
var writer = LineWrappingTextWriter.ForStringWriter();
var options = new CommandOptions()
{
    Error = writer,
    UsageWriter = new UsageWriter(writer),
};

var manager = new CommandManager(options); // or a generated command manager.
var exitCode = await manager.RunCommandAsync();
if (exitCode is int value)
{
    return value;
}

// For demonstration purposes only; probably not the best way to show this.
MessageBox.Show(writer.ToString());

// Return an error code only if the failure was not caused by an argument that canceled parsing.
return manager.ParseResult.Status == ParseStatus.Canceled ? 0 : 1;
```

This, combined with a custom [`UsageWriter`][] to format the usage help as you like, is probably
sufficient for most scenarios. You can also use separate writers for errors and usage help, so you
can display them separately.

However, if you do want to manually handle everything, the below is an example of what this would
look like.

```csharp
public static async Task<int> Main(string[] args)
{
    var options = new CommandOptions() { /* omitted */ };
    var manager = new CommandManager(options); // or a generated command manager.
    var commandInfo = args.Length > 0 ? manager.GetCommand(args[0]) : null;
    if (commandInfo == null)
    {
        // No command or unknown command.
        manager.WriteUsage();
        return 1;
    }

    ICommand? command = null;
    if (commandInfo.UseCustomArgumentParsing)
    {
        // Invoke the custom parsing method; how errors are handled depends on the command here.
        command = commandInfo.CreateInstanceWithCustomParsing();
        // Skip the command name in the arguments.
        command.Parse(args.AsMemory(1), manager);
    }
    else
    {
        var parser = commandInfo.CreateParser();
        try
        {
            // Skip the command name in the arguments.
            command = (ICommand?)parser.Parse(args.AsMemory(1));
        }
        catch (CommandLineArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
        }

        if (parser.HelpRequested)
        {
            parser.WriteUsage();
        }
    }

    // Run the command if successfully created, asynchronous if supported.
    if (command != null)
    {
        if (command is IAsyncCommand asyncCommand)
        {
            return await asyncCommand.RunAsync();
        }

        return command.Run();
    }

    return 1;
}
```

The [`CommandManager`][] class also offers the [`CreateCommand()`][] method, which instantiates the
command class but does not call the `Run(Async)` method. This method also handles errors and shows
usage help automatically.

## Subcommand usage help

Since subcommands are created using the [`CommandLineParser`][], they support showing usage help
when parsing errors occur, or the `-Help` argument is used. For example, with the
[subcommand sample](../src/Samples/Subcommand) you could run the following to get help on the `read`
command:

```text
./Subcommand read -help
```

In addition, the [`CommandManager`][] also prints usage help if no command name was supplied, or the
supplied command name did not match any command defined in the application. In this case, it prints
a list of commands, with their descriptions. This is what that looks like for the sample:

```text
Subcommand sample for Ookii.CommandLine.

Usage: Subcommand <command> [arguments]

The following commands are available:

    read
        Reads and displays data from a file using the specified encoding, wrapping the text to fit
        the console.

    version
        Displays version information.

    write
        Writes lines to a file, wrapping them to the specified width.

Run 'Subcommand <command> -Help' for more information about a command.
```

Usage help for a [`CommandManager`][] is also created using the [`UsageWriter`][], and can be
customized by setting the subcommand-specific properties of that class. The sample above uses one of
them: [`IncludeApplicationDescriptionBeforeCommandList`][], which causes the assembly description of
the first assembly used by the [`CommandManager`][] to be printed before the command list.

The usage help will show information at the bottom on how to get help for each command, using the
name of the automatic help argument. This message will only be shown if the automatic help argument
is enabled, none of the commands use custom parsing, and all commands use the same parsing mode,
argument name transformation, and argument name prefixes.

You can force or disable the inclusion of the command help instruction by using the
[`UsageWriter.IncludeCommandHelpInstruction`][] property.

Other properties let you configure indentation and colors, among others.

The actual help is created using a number of protected virtual methods on the [`UsageWriter`][], so
this can be further customized by deriving your own class from the [`UsageWriter`][] class. Creating
command list usage help is driven by the [`WriteCommandListUsageCore()`][] method. You can also
override other methods to customize parts of the usage help, such as
[`WriteCommandListUsageSyntax()`][], [`WriteCommandDescription()`][], and
[`WriteCommandHelpInstruction()`][], to name just a few.

## Automatic commands

As mentioned above, subcommand classes will not get an automatic `-Version` argument. Instead, there
is an automatic `version` command that gets added, which displays the same information.

**Important:** The `version` command takes the name, version information, and copyright text from
the *entry-point assembly* of the application, regardless of what assembly or assemblies were passed
to the [`CommandManager`][]. If this is not correct for your application, you should create your own
`version` command.

If you create a command named `version`, the automatic `version` command will not be added. You can
also disable the command with the [`CommandOptions.AutoVersionCommand`][] property. The name and
description of the command can be customized using the [`LocalizedStringProvider`][].

## Nested subcommands

Ookii.CommandLine supports nested subcommands through the [`ParentCommandAttribute`][], the
[`ParentCommand`][] class, and the [`CommandOptions.ParentCommand`][] property. The [`CommandManager`][]
will only return commands whose [`ParentCommandAttribute`][] matches the type specified in the
[`CommandOptions.ParentCommand`][] property. By default, this property is null, so commands that do not
have a parent command will be returned.

To create a command that can have nested commands, the easiest way is to create a class that derives
from the [`ParentCommand`][] class.

```csharp
[Command]
[Description("A command with nested subcommands.")]
class MyParentCommand : ParentCommand
{
}
```

> [`ParentCommand`][] uses [`ICommandWithCustomParsing`][], so it cannot use the [`GeneratedParserAttribute`][].

Typically, this class can be empty, although [`ParentCommand`][] provides several protected methods you
can override to customize the behavior.

To define a command that is nested under `MyParentCommand`, you need to use the
[`ParentCommandAttribute`][].

```csharp
[GeneratedParser]
[Command]
[ParentCommand(typeof(MyParentCommand))]
partial class ChildCommand : ICommand
{
    // Omitted.
}
```

When run, `MyParentCommand` will modify the [`CommandOptions.ParentCommand`][] property and use the
[`CommandManager`][] again to find and execute the nested commands.

Note that the automatic version command has no parent and will therefore only exist at the top
level.

The [nested commands sample](../src/Samples/NestedCommands) shows a an example of how to use this
functionality.

The next page will discuss Ookii.CommandLine's [source generation](SourceGeneration.md) in more
detail.

[`AliasAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_AliasAttribute.htm
[`ApplicationFriendlyNameAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_ApplicationFriendlyNameAttribute.htm
[`AsyncCommandBase`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_AsyncCommandBase.htm
[`CancelMode.Success`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_CancelMode.htm
[`CommandAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_CommandAttribute.htm
[`CommandLineArgumentAttribute.CancelParsing`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[`CommandLineParser.Parse<T>()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandManager.GetCommand()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Commands_CommandManager_GetCommand.htm
[`CommandManager.ParseResult`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Commands_CommandManager_ParseResult.htm
[`CommandManager.RunCommandAsync()`]: https://www.ookii.org/docs/commandline-4.0/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommandAsync.htm
[`CommandManager`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_CommandManager.htm
[`CommandNameTransform`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Commands_CommandOptions_CommandNameTransform.htm
[`CommandOptions.AutoCommandPrefixAliases`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Commands_CommandOptions_AutoCommandPrefixAliases.htm
[`CommandOptions.AutoVersionCommand`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Commands_CommandOptions_AutoVersionCommand.htm
[`CommandOptions.CommandFilter`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Commands_CommandOptions_CommandFilter.htm
[`CommandOptions.CommandNameTransform`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Commands_CommandOptions_CommandNameTransform.htm
[`CommandOptions.ParentCommand`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Commands_CommandOptions_ParentCommand.htm
[`CommandOptions.StripCommandNameSuffix`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Commands_CommandOptions_StripCommandNameSuffix.htm
[`CommandOptions`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_CommandOptions.htm
[`CreateCommand()`]: https://www.ookii.org/docs/commandline-4.0/html/Overload_Ookii_CommandLine_Commands_CommandManager_CreateCommand.htm
[`DescriptionAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute
[`Environment.GetCommandLineArgs()`]: https://learn.microsoft.com/dotnet/api/system.environment.getcommandlineargs
[`GeneratedCommandManagerAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_GeneratedCommandManagerAttribute.htm
[`GeneratedParserAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_GeneratedParserAttribute.htm
[`IAsyncCommand.RunAsync()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Commands_IAsyncCommand_RunAsync.htm
[`IAsyncCommand`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_IAsyncCommand.htm
[`ICommand.Run()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Commands_ICommand_Run.htm
[`ICommand`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_ICommand.htm
[`ICommandWithCustomParsing.Parse()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Commands_ICommandWithCustomParsing_Parse.htm
[`ICommandWithCustomParsing`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_ICommandWithCustomParsing.htm
[`IncludeApplicationDescriptionBeforeCommandList`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_UsageWriter_IncludeApplicationDescriptionBeforeCommandList.htm
[`LocalizedStringProvider`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_LocalizedStringProvider.htm
[`NameTransform.DashCase`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_NameTransform.htm
[`NameTransform.None`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_NameTransform.htm
[`Ookii.CommandLine.Commands`]: https://www.ookii.org/docs/commandline-4.0/html/N_Ookii_CommandLine_Commands.htm
[`ParentCommand`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_ParentCommand.htm
[`ParentCommandAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Commands_ParentCommandAttribute.htm
[`ParseOptions.AutoVersionArgument`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_ParseOptions_AutoVersionArgument.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`ParseResult.RemainingArguments`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_ParseResult_RemainingArguments.htm
[`ParseResult.Status`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_ParseResult_Status.htm
[`RunCommand()`]: https://www.ookii.org/docs/commandline-4.0/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommand.htm
[`RunCommand`]: https://www.ookii.org/docs/commandline-4.0/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommand.htm
[`RunCommandAsync()`]: https://www.ookii.org/docs/commandline-4.0/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommandAsync.htm
[`StringComparison.OrdinalIgnoreCase`]: https://learn.microsoft.com/dotnet/api/system.stringcomparison
[`StripCommandNameSuffix`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_Commands_CommandOptions_StripCommandNameSuffix.htm
[`UsageWriter.IncludeCommandHelpInstruction`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_UsageWriter_IncludeCommandHelpInstruction.htm
[`UsageWriter`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_UsageWriter.htm
[`WriteCommandDescription()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_UsageWriter_WriteCommandDescription.htm
[`WriteCommandHelpInstruction()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_UsageWriter_WriteCommandHelpInstruction.htm
[`WriteCommandListUsageCore()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_UsageWriter_WriteCommandListUsageCore.htm
[`WriteCommandListUsageSyntax()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_UsageWriter_WriteCommandListUsageSyntax.htm
[RunAsync()_0]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Commands_AsyncCommandBase_RunAsync.htm
