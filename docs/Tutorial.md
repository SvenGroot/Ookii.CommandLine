# Tutorial: getting started with Ookii.CommandLine

This tutorial will show you the basics of how to use Ookii.CommandLine. The tutorial will show you
how to create a basic application that parses the command line and shows usage help, how to
customize some of the options, and how to use Subcommands.

Refer to the [documentation](Documentation.md) for more detailed information.

## Creating a project

Create a directory called "tutorial" for the project, and run the following command in that
directory:

```text
dotnet new console --framework net6.0
```

Next, we will add a reference to Ookii.CommandLine's NuGet package:

```text
dotnet add package Ookii.CommandLine
```

## Defining and parsing command line arguments

Add a file to your project called Arguments.cs, and insert the following code:

```csharp
using Ookii.CommandLine;

namespace Tutorial;

class Arguments
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    public string? Path { get; set; }
}
```

In Ookii.CommandLine, you define arguments by making a class that holds them, and adding properties
to that class. Every public property that has the `CommandLineArgumentAttribute` defines an argument.

The code above defines a single argument called "Path", indicates it's the first positional argument,
and makes it required.

> You can use the `CommandLineArgumentAttribute` to specify a custom name for your argument. If you
> don't, the property name is used.

Now replace the contents of Program.cs with the following:

```csharp
using Ookii.CommandLine;

namespace Tutorial;

static class Program
{
    public static int Main()
    {
        var args = CommandLineParser.Parse<Arguments>();
        if (args == null)
        {
            return 1;
        }

        ReadFile(args);
        return 0;
    }

    private static void ReadFile(Arguments args)
    {
        foreach (var line in File.ReadLines(args.Path!))
        {
            Console.WriteLine(line);
        }
    }
}
```

This code parses the arguments we defined, returns an error code if it was unsuccessful, and writes
the contents of the file specified by the path argument to the console.

The important part is the call to `CommandLineParser.Parse<Arguments>()`. This static method will
parse your arguments, handle and print any errors, and print usage help if required.

But wait, we didn't pass any arguments to this method? Actually, the `Parse<T>()` method will call
`Environment.GetCommandLineArgs()` to get the arguments. There are also overloads that take an
explicit `string[]` array with the arguments, if you want to pass them manually.

So, let's run our application:

```text
dotnet run -- tutorial.csproj
```

Which will give print the contents of the tutorial.csproj file:

```text
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Ookii.CommandLine" Version="3.0.0" />
  </ItemGroup>

</Project>
```

So far, so good. But what happens if we invoke the application without arguments? After all, the
"Path" argument is required. To try this, run the following command:

```text
dotnet run
```

This gives the following output:

```text
The required argument 'Path' was not supplied.

Usage: tutorial [-Path] <String> [-Help] [-Version]

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.
```

> The actual usage help uses color if your console supports it. See [here](images/color.png) for
> an example.

As you can see, the `Parse<T>()` method lets us know what's wrong (we didn't supply the required
argument), and shows the usage help.

The usage syntax (the line starting with "Usage:") shows the argument we defined. However, the list
of arguments below it does not. That's because our argument didn't have a description, and only
arguments with descriptions are shown in that list by default. We'll add some descriptions below.

You can also see that there are two more arguments that we didn't define: "-Help" and "-Version".
These arguments are automatically added by Ookii.CommandLine. So, what do they do?

If you use the "-Help" argument (`dotnet run -- -Help`), it shows the same message as before. The only difference is that
there's no error message, even if you omitted the "Path" argument. And even if you do supply a path
together with "-Help", it still shows the help and exits, it doesn't run the application.

The "-Version" argument shows version information about your application:

```text
$ dotnet run -- -Version
tutorial 1.0.0
```

By default, it shows the assembly name and informational version. It'll also show the assembly's
copyright information, if there is any (there's not in this case). You can also use the
`ApplicationFriendlyNameAttribute` attribute to specify a custom name instead of the assembly name.

> If you define an argument called "Help" or "Version", the automatic arguments won't be added.
> Also, you can disable the automatic arguments using the `ParseOptionsAttribute` attribute.

Note that in the usage syntax, your positional "Path" argument still has its name shown as "-Path".
That's because every argument, even positional ones, can still be supplied by name. So if you run
this:

```text
dotnet run -- -path tutorial.csproj
```

The output is the same as above.

> Argument names are case insensitive by default, so even though I used "-path" instead of "-Path"
> above, it still worked.

## Arguments with other types

Arguments don't have to be strings. In fact, they can have any type as long as there's a way to
[convert to them](Arguments.md#argument-value-conversion) from a string. All of the basic .Net
types are supported (like `int`, `float`, `bool`), as well as many more that can be converted from
a string (like enumerations, or classes like `FileInfo` or `Uri`).

Let's try this out by adding another argument in the Arguments class. First add this to the top
of Arguments.cs:

```csharp
using Ookii.CommandLine.Validation
```

And then add the following properties to the Arguments class:

```csharp
[CommandLineArgument]
[ValidateRange(1, null)]
[Alias("Max")]
public int? MaxLines { get; set; }

[CommandLineArgument]
public bool Inverted { get; set; }
```

This defines two new arguments. The first, "MaxLines", uses `int` as its type. This argument is not
positional (you must use the name), and it's optional. We've also added a validator to ensure the
value is positive, and since "-MaxLines" might be a bit verbose, we've given it an alias "-Max",
which can be used as an alternative name to supply the argument.

> An argument can have any number of aliases; just repeat the `AliasAttribute` attribute.

The second argument, "Inverted", is a boolean, which means it's a switch argument. Switch arguments
don't need values, you either supply them or you don't.

Now, let's update `ReadFile` to use this new argument:

```csharp
private static void ReadFile(Arguments args)
{
    if (args.Inverted)
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
    }

    var lines = File.ReadLines(args.Path!);
    if (args.MaxLines is int maxLines)
    {
        lines = lines.Take(maxLines);
    }

    foreach (var line in lines)
    {
        Console.WriteLine(line);
    }

    if (args.Inverted)
    {
        Console.ResetColor();
    }
}
```

Now we can run the application like this:

```text
dotnet run -- tutorial.csproj -max 5 -inverted
```

And it'll only show the first five lines of the file, using black-on-white text.

If you supply a value that's not a valid integer for "MaxLines", or a value that's less than 1,
you'll once again get an error message and the usage help.

Above, we used a nullable value type (`Nullable<int>`, or `int?`) so we could tell whether the
argument was supplied. Instead, we could also set a default value. This can be done in two ways: the
first is using the `DefaultValue` property (the validator and alias are omitted for brevity):

```csharp
[CommandLineArgument(DefaultValue = 10)]
public int MaxLines { get; set; }
```

> If your argument's type doesn't have literals you can use in an attribute, you can also use a
> string to specify the default value, and the value will be converted when used.

Alternatively, you can just initialize the property, since Ookii.CommandLine won't set the property
if it's not supplied and the default value is `null`:

```csharp
[CommandLineArgument]
public int MaxLines { get; set; } = 10
```

The advantage of the former approach is that the default value will be included in the usage help.

## Expanding the usage help

We saw before that our custom arguments were showing up in the usage syntax, but didn't have any
descriptions. Typically, you'll want to add descriptions to your arguments. This is done using the
`System.ComponentModel.DescriptionAttribute` attribute.

Let's add some for our arguments:

```csharp
using Ookii.CommandLine;
using Ookii.CommandLine.Validation;
using System.ComponentModel;

namespace Tutorial;

[Description("Reads a file and displays the contents on the command line.")]
class Arguments
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    [Description("The path of the file to read.")]
    public string? Path { get; set; }

    [CommandLineArgument(ValueDescription = "Number")]
    [Description("The maximum number of lines to output.")]
    [ValidateRange(1, null)]
    [Alias("Max")]
    public int? MaxLines { get; set; }

    [CommandLineArgument]
    [Description("Use black text on a white background.")]
    public bool Inverted { get; set; }
}
```

I've also added a description to the class itself. That description is shown before the usage syntax
as part of the usage help. Use it to provide a description for your application as a whole.

The "MaxLines" property now also sets its *value description*. The value description is a short,
typically one-word description of the type of values the argument accepts, which is shown in angle
brackets in the usage help. It defaults to the type name, but "Int32" might not be very meaningful
to people who aren't programmers, so we've changed it to "Number" instead.

Now, let's run the application using `dotnet run -- -help`:

```text
Reads a file and displays the contents on the command line.

Usage: tutorial [-Path] <String> [-Help] [-Inverted] [-MaxLines <Number>] [-Version]

    -Path <String>
        The path of the file to read.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Inverted [<Boolean>]
        Use black text on a white background.

    -MaxLines <Number> (-Max)
        The maximum number of lines to output. Must be at least 1.

    -Version [<Boolean>]
        Displays version information.
```

Now our usage help looks a lot better! All the arguments are present in the description list. Also
note how the `ValidateRangeAttribute` validator we used automatically added its condition to the
description of "MaxLines" (this can be disabled either globally or on a per-validator basis if you
want). Things like the default value are added in a similar fashion.

The "MaxLines" argument also has its alias listed, just like the "Help" argument.

> Don't like the way the usage help looks? It can be fully customized! Check out the [custom usage
> sample](../src/Samples/CustomUsage) for an example of that.

## Customizing the parsing behavior (long/short mode)

Ookii.CommandLine offers many ways in which the way it parses the command line can be customized.
For example, you can disable the use of white space as a separator between argument names and
values, and specify a custom separator. You can specify custom argument name prefixes, instead of
`-` which is the default (on Windows, `/` is also accepted by default). You can make the argument
names case sensitive. And there's more.

Most of these options can be specified using the `ParseOptionsAttribute`, which you can apply to
your class. Let's apply some options:

```csharp
[Description("Reads a file and displays the contents on the command line.")]
[ParseOptions(Mode = ParsingMode.LongShort,
    CaseSensitive = true,
    NameTransform = NameTransform.DashCase,
    ValueDescriptionTransform = NameTransform.DashCase)]
class Arguments
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    [Description("The path of the file to read.")]
    public string? Path { get; set; }

    [CommandLineArgument(IsShort = true, ValueDescription = "number")]
    [Description("The maximum number of lines to output.")]
    [ValidateRange(1, null)]
    [Alias("max")]
    public int? MaxLines { get; set; }

    [CommandLineArgument(IsShort = true)]
    [Description("Use black text on a white background.")]
    public bool Inverted { get; set; }
}
```

The biggest change here is that we've set the `Mode` to `ParsingMode.LongShort`. This is an
alternative set of parsing rules, where every argument can have a long name (using the `--` prefix
by default, and a single-character short name using the `-` prefix).

We've changed the arguments as well, to give both "MaxLines" and "Inverted" short names, which will
be derived using the first character of their long names. You can also specify a custom short name
using the `CommandLineArgumentAttribute.ShortName` property. Arguments always have a long name by
default, which can be disabled with the `CommandLineArgumentAttribute.IsLong` property (they must
have either a short or a long name).

I've also applied a name transformation to the argument names and value descriptions. In this case,
they'll be transformed to "dash-case" (lower case, with a dash between every word). This saves you
from having to give every argument a custom name if you want to use a different naming style.

> Name transformations don't apply to names or value descriptions that are explicitly specified, so
> we had to change "number" and "max" manually to match.

Now, the usage help looks like this:

```text
Reads a file and displays the contents on the command line.

Usage: tutorial [--path] <string> [--help] [--inverted] [--max-lines <number>] [--version]

        --path <string>
            The path of the file to read.

    -?, --help [<boolean>] (-h)
            Displays this help message.

    -i, --inverted [<boolean>]
            Use black text on a white background.

    -m, --max-lines <number> (--max)
            The maximum number of lines to output.

        --version [<boolean>]
            Displays version information.
```

As you can see, the format is slightly different, giving more prominence to the short names. And
the name transformation has changed the name of all of our arguments (which, remember, are now
case sensitive). We now have very different parsing behavior without having to change the code
that uses the arguments at all.

In addition to the `ParseOptionsAttribute` attribute, you can also use the `ParseOptions` class
to specify many of the same options and a bunch of other ones that didn't lend themselves well to
an attribute, including where to write errors and help, and customization options for the
usage help. You can pass an instance of the `ParseOptions` class to the `Parse<T>()` method.

If you specify the same option in both the `ParseOptionsAttribute` attribute and the `ParseOptions`
class, the `ParseOptions` class takes precedence.

## Using subcommands

Many applications have more than once function, which are invoked through subcommands. Think for
example of the `dotnet` command, which has commands like `dotnet build` and `dotnet run`, or
something like `git` with commands like `git pull` or `git cherry-pick`. Each command does something
different, and needs its own command line arguments.

Using Ookii.CommandLine, subcommands are just classes that define arguments, the exact same as we've
already been doing. The only difference is that they have to implement an interface, specify an
attribute, and how you invoke them from the main method.

Let's change the example we've built so far to use subcommands. I'm going to go back to the version
before we changed the options (so using the default parsing mode), but you can use the long/short
mode version as well if you want.

First, we'll rename our `Arguments` class to `ReadCommand` (this is optional, but it just makes more
sense that way; you can also change the file name if you want), and add another `using` statement:

```csharp
using Ookii.CommandLine.Commands;
```

Then, we'll change renamed `Arguments` class into a subcommand:

```csharp
[Description("Reads a file and displays the contents on the command line.")]
[Command("read")]
class ReadCommand : ICommand
```

We've added the `CommandAttribute`, which indicates the class is a command and lets us specify the
name of the command, which is "read" in this case. We've also added the `ICommand` interface, which
all commands must implement.

We don't have to change anything about the properties defining the arguments. However, we do have
to implement the `ICommand` interface, which has a single method called `Run`. To implement it, we
move the implementation of `ReadFile` from Program.cs (you can remove it from there if you want):

```csharp
public int Run()
{
    if (Inverted)
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
    }

    var lines = File.ReadLines(Path!);
    if (MaxLines is int maxLines)
    {
        lines = lines.Take(maxLines);
    }

    foreach (var line in lines)
    {
        Console.WriteLine(line);
    }

    if (Inverted)
    {
        Console.ResetColor();
    }

    return 0;
}
```

`Run` is like the `Main` method for your command, and its return value should be treated like the
exit code returned from `Main`.

And that's it: we've now defined a command. However, we still need to change the `Main` method to
use commands instead of just parsing arguments from a single class. Fortunately, this is very
simple. First add the `using Ookii.CommandLine.Commands;` statement, to Program.cs, and then update
your `Main` method:

```csharp
public static int Main()
{
    var manager = new CommandManager();
    return manager.RunCommand() ?? 1;
}
```

The `CommandManager` class handles the finding your commands, and lets you specify various options,
including the `ParseOptions` that will be shared by all commands. The default constructor will look
for classes that implement `ICommand` and have the `CommandAttribute` attribute in the calling
assembly, and uses the default options.

The `RunCommand()` method takes care of reading the command name from the first argument to your
application, finding the command, creating it, and running it. If anything goes wrong, it will
either display a list of commands, or if a command has been found, the help for that command. The
return value is the value returned from `Run()`, or `null` if parsing failed, in which case we return
an error exit code.

If we run our application without arguments again (`dotnet run`), we see the following:

```text
Usage: tutorial <command> [arguments]

The following commands are available:

    read
        Reads a file and displays the contents on the command line.

    version
        Displays version information.
```

When no command, or an unknown command, is supplied, a list of commands is printed. The
`DescriptionAttribute` for our class, which was the application description before, is now the
description of the command.

There is a second command, "version," which is automatically added unless there already is a command
with that name. It does the same thing as the "-Version" argument before.

Let's see the usage help for our command:

```text
dotnet run -- read -help
```

Which gives the following output:

```text
Reads a file and displays the contents on the command line.

Usage: tutorial read [-Path] <String> [-Help] [-Inverted] [-MaxLines <Number>]

    -Path <String>
        The path of the file to read.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Inverted [<Boolean>]
        Use black text on a white background.

    -MaxLines <Number> (-Max)
        The maximum number of lines to output. Must be at least 1.
```

There are two differences to spot from the earlier version: the usage syntax now says `tutorial read`
before the arguments, indicating you have to use the command, and there is no automatic "-Version"
argument, since that would be redundant with the "version" command.

## Command options

Just like with parsing, the behavior with commands can be customized. You can of course apply a
`ParseOptionsAttribute` to the command class, but those options then apply only to that command,
and options that are specific to subcommands are not available.

Instead, you can use the `CommandOptions` class, which you can pass to the command manager. For
example:

```csharp
var options = new CommandOptions()
{
    CommandNameTransform = NameTransform.DashCase,
    ShowCommandHelpInstruction = true,
    IncludeApplicationDescriptionBeforeCommandList = true,
};

var manager = new CommandManager(options);
return manager.RunCommand() ?? 1;
```

Here we're applying a name transformation to the command names, which means we can change our class
to this:

```csharp
[Description("Reads a file and displays the contents on the command line.")]
[Command]
class ReadCommand : ICommand
```

We've removed the explicit name from the `CommandAttribute`. If you run the application, you'll see
the command is still called "read". That's because for subcommands, the name transformation will
strip the suffix "Command" from the name by default. This too can be customized with the
`CommandOptions` class.

We also set the `ShowCommandHelpInstruction` property, which will cause the application to print
a message like `Run 'tutorial <command> -Help' for more information about a command.` after the
command list. This is disabled by default because the `CommandManager` won't check if all the
commands actually have a `-Help` argument. It's recommended to enable this if all your commands do.

The last option is `IncludeApplicationDescriptionBeforeCommandList`, which won't have done anything
right now. It lets you use the assembly description as an application description, which will be
printed before the usage help, but since there is no description right now, it didn't do anything.
Add a `<Description>` element to the tutorial.csproj file to add one.

## Common arguments for commands

Sometimes, you'll want some arguments to be available to all arguments. With Ookii.CommandLine, it's
not possible to define arguments outside of a command. However, you can share arguments by using
a common base class.

```csharp
abstract class BaseCommand : ICommand
{
    [CommandLineArgument]
    [Description("The description.")]
    public int CommonArgument { get; set; }

    public abstract int Run();
}

[Command]
class SomeCommand : BaseCommand
{
    /* ... */
}

[Command]
class OtherCommand : BaseCommand
{
    /* ... */
}
```

Now both commands share the "CommonArgument" argument defined in the base class, in addition to the
arguments they define. Note that "BaseCommand" is not itself a command, because it doesn't have the
`CommandAttribute` attribute (and also because it's `abstract`).

If you apply a `ParseOptionsAttribute` attribute to the `BaseCommand` class, you can also share
parse options between multiple commands, without having to pass a `CommandOptions` instance to the
`CommandManager` class.

## Asynchronous commands

You may wish to use asynchronous code in your applications. Fortunately, this is supported with the
`IAsyncCommand` interface. This interface adds a `RunAsync()` method, which you can implement. You
then run your command with the `CommandManager.RunCommandAsync()` method.

You must still implement `ICommand`, since every command must implement that interface. If you use
the `RunCommandAsync()` method you can just leave the `Run()` method blank, or throw an exception.
Alternatively, you can derive from the `AsyncCommandBase` class, which provides a default
implementation of the `Run()` method that invokes `RunAsync()`.

An asynchronous command could look like this.

```csharp
[Command]
class AsyncCommand : AsyncCommandBase
{
    public override async Task<int> RunAsync()
    {
        // Do something asynchronous.
        await Task.Delay(1000);
        return 0;
    }
}
```

And the `Main` method would look like this:

```csharp
public static Task<int> Main()
{
    var manager = new CommandManager();
    return await manager.RunCommandAsync() ?? 1;
}
```

## Multiple commands

An application with just one command isn't very useful. Add more commands by creating more classes
that implement `ICommand` and have a `CommandAttribute`. Since the `CommandManager` will
automatically look for all commands in your assembly, you don't have to do anything else. You can
also define the commands in one or more separate assemblies, and pass those to the
`CommandManager`'s constructor.

For an example of an application with multiple commands, check out the [subcommands sample](../src/Samples/SubCommand).
There is also a sample demonstrating [nested commands](../src/Samples/NestedCommands).

## More information

I hope this tutorial helped you get started with Ookii.CommandLine. To learn more, check out the
following resources:

- [Usage documentation](Documentation.md)
- [Class library documentation](https://www.ookii.org/Link/CommandLineDoc)
- [Sample applications](../src/Samples)
