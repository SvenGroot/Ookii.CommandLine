# Tutorial: getting started with Ookii.CommandLine

This tutorial will show you the basics of how to use Ookii.CommandLine. It will show you how to
create an application that parses the command line and shows usage help, how to customize some of
the options—including the new long/short mode—and how to use subcommands.

Refer to the [documentation](README.md) for more detailed information.

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
to that class. Every public property that has the [`CommandLineArgumentAttribute`][] defines an argument.

The code above defines a single argument called "Path", indicates it's the first positional argument,
and makes it required.

> You can use the [`CommandLineArgumentAttribute`][] to specify a custom name for your argument. If you
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

But wait, we didn't pass any arguments to this method? Actually, the [`Parse<T>()`][Parse<T>()_1]
method will call [`Environment.GetCommandLineArgs()`][] to get the arguments. There are also
overloads that take an explicit `string[]` array with the arguments, if you want to pass them
manually.

So, let's run our application. Build the application using `dotnet build`, and then, from the
`bin/Debug/net6.0` directory, run the following:

```text
./tutorial ../../../tutorial.csproj
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

So far, so good. But what happens if we invoke the application without arguments? After all, we
made the `-Path` argument required. To try this, run the following command:

```text
./tutorial
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

As you can see, the [`Parse<T>()`][Parse<T>()_1] method lets us know what's wrong (we didn't supply
the required argument), and shows the usage help.

The usage syntax (the line starting with "Usage:") includes the `-Path` argument we defined.
However, the list of argument descriptions below it does not. That's because our argument doesn't
have a description, and only arguments with descriptions are shown in that list by default. We'll
add some descriptions [below](#expanding-the-usage-help).

You can also see that there are two more arguments that we didn't define: `-Help` and `-Version`.
These arguments are automatically added by Ookii.CommandLine. So, what do they do?

If you use the `-Help` argument (`./tutorial -Help`), it shows the same message as before. The only
difference is that there's no error message, even if you omitted the `-Path` argument. And even if
you do supply a path together with `-Help`, it still shows the help and exits, it doesn't run the
application. Basically, the presence of `-Help` will override anything else.

The `-Version` argument shows version information about your application:

```text
$ ./tutorial -Version
tutorial 1.0.0
```

By default, it shows the assembly's name and informational version. It'll also show the assembly's
copyright information, if there is any (there's not in this case). You can also use the
[`ApplicationFriendlyNameAttribute`][] attribute to specify a custom name instead of the assembly name.

> If you define an argument called "Help" or "Version", the automatic arguments won't be added.
> Also, you can disable the automatic arguments using the [`ParseOptionsAttribute`][] attribute.

Note that in the usage syntax, your positional "Path" argument still has its name shown as `-Path`.
That's because every argument, even positional ones, can still be supplied by name. So if you run
this:

```text
./tutorial -path ../../../tutorial.csproj
```

The output is the same as above.

> Argument names are case insensitive by default, so even though I used `-path` instead of `-Path`
> above, it still worked.

## Arguments with other types

Arguments don't have to be strings. In fact, they can have any type as long as there's a way to
[convert to that type](Arguments.md#argument-value-conversion) from a string. All of the basic .Net
types are supported (like `int`, `float`, `bool`, etc.), as well as many more that can be converted
from a string (like enumerations, or classes like [`FileInfo`][] or [`Uri`][], and many other
types).

Let's try this out by adding more arguments in the `Arguments` class. First, add this to the top of
Arguments.cs:

```csharp
using Ookii.CommandLine.Validation;
```

And then add the following properties to the `Arguments` class:

```csharp
[CommandLineArgument]
[ValidateRange(1, null)]
[Alias("Max")]
public int? MaxLines { get; set; }

[CommandLineArgument]
public bool Inverted { get; set; }
```

This defines two new arguments. The first, `-MaxLines`, uses `int?` as its type, so it will only
accept integer numbers, and be null if not supplied. This argument is not positional (you must use
the name), and it's optional. We've also added a validator to ensure the value is positive, and
since `-MaxLines` might be a bit verbose, we've given it an alias `-Max`, which can be used as an
alternative name to supply the argument.

> An argument can have any number of aliases; just repeat the [`AliasAttribute`][] attribute.

The second argument, `-Inverted`, is a boolean, which means it's a switch argument. Switch arguments
don't need values, you either supply them or you don't.

Now, let's update `ReadFile` to use the new arguments:

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
./tutorial ../../../tutorial.csproj -max 5 -inverted
```

And it'll only show the first five lines of the file, using black-on-white text.

If you supply a value that's not a valid integer for `-MaxLines`, or a value that's less than 1,
you'll once again get an error message and the usage help.

Above, we used a nullable value type ([`Nullable<int>`][], or `int?`) so we could tell whether the
argument was supplied. Instead, we could also set a default value. This can be done in two ways: the
first is using the [`DefaultValue`][DefaultValue_1] property:

```csharp
[CommandLineArgument(DefaultValue = 10)]
[ValidateRange(1, null)]
[Alias("Max")]
public int MaxLines { get; set; }
```

> If your argument's type doesn't have literals, you can also use a string to specify the default
> value, and the value will be converted when used. For example, `[CommandLineArgument(DefaultValue = "10")]`
> is equivalent to the above.

Alternatively, you can just initialize the property, since Ookii.CommandLine won't set the property
if the argument is not supplied and the default value is null:

```csharp
[CommandLineArgument]
public int MaxLines { get; set; } = 10;
```

The advantage of the former approach is that the default value will be included in the usage help.
The latter allows you to use non-constant values, and can sometimes be required if the type of an
argument is a non-nullable reference type (you can also use both, in which case the [`DefaultValue`][DefaultValue_1]
property will overwrite the initial value).

While we're talking about non-nullable reference types, consider the following alternative for the
`-Path` argument:

```csharp
[CommandLineArgument(Position = 0, IsRequired = true)]
public string Path { get; set; } = string.Empty;
```

An automatic property with a non-nullable type must be initialized with a non-null value, or the
code won't compile. Even though we know the property will be set by the [`CommandLineParser`][],
because the argument is required, this is still required because the C# compiler can't know that
(and the compiler is right in case you create an instance manually without using the
[`CommandLineParser`][]). So we must initialize the property, even if that value won't be used.

The advantage of doing this would be that we can remove the `!` from the value's usage in
`ReadFile`, at the cost of an unnecessary initialization. As a bonus, for .Net 6.0 and later only,
the [`CommandLineParser`][] will make sure that arguments with non-nullable types can't be set to
null, even if the [`TypeConverter`][] for the property's type returns null (it will treat that as an
error).

## Expanding the usage help

We saw before that our custom arguments were included in the usage syntax, but didn't have any
descriptions. Typically, you'll want to add descriptions to your arguments, so your users can tell
what they do. This is done using the [`System.ComponentModel.DescriptionAttribute`][] attribute.

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

The `MaxLines` property now also sets its *value description*. The value description is a short,
typically one-word description of the type of values the argument accepts, which is shown in angle
brackets in the usage help. It defaults to the type name, but "Int32" might not be very meaningful
to people who aren't programmers, so we've changed it to "Number" instead.

Now, let's run the application using `./tutorial -help`:

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
note how the [`ValidateRangeAttribute`][] validator we used automatically added its condition to the
description of `-MaxLines` (this can be disabled either globally or on a per-validator basis if you
want). Things like the default value, if an argument has one, are added in a similar fashion.

The `-MaxLines` argument also has its alias listed, just like the `-Help` argument.

> Don't like the way the usage help looks? It can be fully customized! Check out the
> [custom usage sample](../src/Samples/CustomUsage) for an example of that.

## Long/short mode and other customizations

Ookii.CommandLine offers many options to customize the way it parses the command line. For example,
you can disable the use of white space as a separator between argument names and values, and specify
a custom separator. You can specify custom argument name prefixes, instead of `-` which is the
default (on Windows only, `/` is also accepted by default). You can make the argument names case
sensitive. And there's more.

Most of these options can be specified using the [`ParseOptionsAttribute`][], which you can apply to
your class. Let's apply some options:

```csharp
[Description("Reads a file and displays the contents on the command line.")]
[ParseOptions(Mode = ParsingMode.LongShort,
    CaseSensitive = true,
    ArgumentNameTransform = NameTransform.DashCase,
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

We've done a few things here: we've turned on an alternative set of parsing rules by setting the
[`Mode`][Mode_2] property to [`ParsingMode.LongShort`][], we've made argument names case sensitive,
and we've applied a name transformation to both argument names and value descriptions, which will
make them lower case with dashes between words (e.g. "max-lines").

These options combined make the application's parsing behavior very similar to common POSIX
conventions; the same conventions followed by tools such as `dotnet` or `git`, and many others. For
a cross-platform application, you may prefer these conventions over the default, but it's up to you
of course.

Long/short mode is the key to this behavior. It allows every argument to have two separate names:
a long name, using the `--` prefix by default, and a single-character short name using the `-`
prefix (and `/` on Windows).

When using long/short mode, all arguments have long names by default, but you'll need to indicate
which arguments have short names. We've done that here with the `MaxLines` and `Inverted`
properties, by specifying `IsShort = true`. This gives them a short name using the first character
of their long name (after the name transformation is applied), so `-m` and `-i` in this case. You
can also specify a custom short name using the [`CommandLineArgumentAttribute.ShortName`][]
property.

If you want an argument to only have a short name, you can disable the long name using the
[`CommandLineArgumentAttribute.IsLong`][] property.

With all these changes, the `MaxLines` property now creates an argument with the long name
`--max-lines`, and the short name `-m`. We also have an argument with the long name `--inverted`,
and the short name `-i`. Finally, `--path` only has a long name, and is still positional. All of
these names are now case sensitive.

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

As you can see, the format is slightly different, giving more prominence to the short names. You
can see the result of the name transformation on all the arguments and value descriptions, including
the automatic `--help` and `--version` arguments, which are now also lower case.

In addition to the [`ParseOptionsAttribute`][] attribute, you can also use the [`ParseOptions`][]
class to specify these and many other options, including where to write errors and help, and
customization options for the usage help. You can pass an instance of the [`ParseOptions`][] class
to the [`Parse<T>()`][Parse<T>()_1] method.

For the options that are available on both the [`ParseOptionsAttribute`][] attribute and the
[`ParseOptions`][] class, you can choose which method to use based on your personal preference. If
you specify the same option in both the [`ParseOptionsAttribute`][] attribute and the
[`ParseOptions`][] class, the [`ParseOptions`][] class takes precedence.

## Using subcommands

Many applications have multiple functions, which are invoked through subcommands. Think for example
of the `dotnet` application, which has commands like `dotnet build` and `dotnet run`, or something
like `git` with commands like `git pull` or `git cherry-pick`. Each command does something
different, and needs its own command line arguments.

Creating subcommands with Ookii.CommandLine is very similar to what we've been doing already. A
subcommand is a class that defines arguments, same as before; the class will just have to implement
the [`ICommand`][] interface, and use the [`CommandAttribute`][] attribute. Additionally, we'll have
to change our `Main()` method to use subcommands.

Let's change the example we've built so far to use subcommands. I'm going to continue with the
POSIX-like long/short mode, but if you prefer the defaults, you can go back to that version too.

First, we'll add another `using` statement to Arguments.cs:

```csharp
using Ookii.CommandLine.Commands;
```

Then, we'll rename our `Arguments` class to `ReadCommand` (just for clarity), and change it into a
subcommand:

```csharp
[Command("read")]
[Description("Reads a file and displays the contents on the command line.")]
class ReadCommand : ICommand
```

We've added the [`CommandAttribute`][], which indicates the class is a command and lets us specify
the name of the command, which is "read" in this case. We've also added the [`ICommand`][]
interface, which all commands must implement.

Note that we've *removed* the [`ParseOptionsAttribute`][]. Don't worry, we'll add the options back
elsewhere later, so they'll apply to all commands.

We don't have to change anything about the properties defining the arguments. However, we do have
to implement the [`ICommand`][] interface, which has a single method called [`Run()`][Run()_1]. To implement it, we
move the implementation of `ReadFile()` from Program.cs into this method:

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

The [`Run()`][Run()_1] method is like the `Main()` method for your command, and its return value should be
treated like the exit code returned from `Main()`, because typically, you will return the executed
command's return value from `Main()`.

And that's it: we've now defined a command. However, we still need to change the `Main()` method to
use commands instead of just parsing arguments from a single class. Fortunately, this is very
simple. First add the `using Ookii.CommandLine.Commands;` statement to Program.cs, and then update
your `Main()` method:

```csharp
public static int Main()
{
    var options = new CommandOptions()
    {
        Mode = ParsingMode.Default,
        ArgumentNameComparer = StringComparer.InvariantCulture,
        ArgumentNameTransform = NameTransform.DashCase,
        ValueDescriptionTransform = NameTransform.DashCase,
    };

    var manager = new CommandManager(options);
    return manager.RunCommand() ?? 1;
}
```

The [`CommandManager`][] class handles finding your commands, and lets you specify various options.
The default constructor will look for subcommand classes in the calling assembly.

The [`CommandOptions`][] class derives from the [`ParseOptions`][] class, so it can be used to
specify all the same options, and these will be shared by every command. We've used this to apply
the options that we were previously setting using the [`ParseOptionsAttribute`][].

You could of course still use the [`ParseOptionsAttribute`][], but if you do, those options only
apply to that particular command, so for consistency between your commands using the
[`CommandOptions`][] class is often better.

Note that the [`ParseOptions`][] (and therefore, the [`CommandOptions`][]) class has no
[`CaseSensitive`][] property; instead, you have to set the
[`ArgumentNameComparer`][ArgumentNameComparer_1] property. We use
[`StringComparer.InvariantCulture`][] here to get case-sensitive argument names.

> For the default case-insensitive behavior, [`StringComparer.OrdinalIgnoreCase`][] is used. You can
> also use [`StringComparer.Ordinal`][] for case sensitivity, but [`StringComparer.InvariantCulture`]
> has better sorting for the usage help if you mix upper and lower case argument names.

The [`RunCommand()`][] method will take the arguments from [`Environment.GetCommandLineArgs()`][] (as
before, you can also pass them explicitly), and uses the first argument as the command name. If a
command with that name exists, it uses [`CommandLineParser`][] to parse the arguments for that command,
and finally invokes the [`ICommand.Run()`][] method. If anything goes wrong, it will either display a
list of commands, or if a command has been found, the help for that command. The return value is the
value returned from [`ICommand.Run()`][], or null if parsing failed, in which case we return a non-zero
exit code to indicate failure.

> If you want to customize any of these steps, there are methods like [`GetCommand()`][] and
> [`CreateCommand()`][] that you can call to do this manually.

If we build our application, and run it without arguments again (`./tutorial`), we see the
following:

```text
Usage: tutorial <command> [arguments]

The following commands are available:

    read
        Reads a file and displays the contents on the command line.

    version
        Displays version information.
```

When no command, or an unknown command, is supplied, a list of commands is printed. The
[`DescriptionAttribute`][] for our class, which was the application description before, is now the
description of the command.

There is a second command, `version`, which is automatically added unless there already is a command
with that name. It does the same thing as the `-Version` argument before.

Let's see the usage help for our command:

```text
./tutorial read -help
```

Which gives the following output:

```text
Reads a file and displays the contents on the command line.

Usage: tutorial read [--path] <string> [--help] [--inverted] [--max-lines <number>]

        --path <string>
            The path of the file to read.

    -?, --help [<boolean>] (-h)
            Displays this help message.

    -i, --inverted [<boolean>]
            Use black text on a white background.

    -m, --max-lines <number> (--max)
            The maximum number of lines to output. Must be at least 1.
```

There are two differences to spot from the earlier version: the usage syntax now says `tutorial read`
before the arguments, indicating you have to use the command, and there is no automatic `--version`
argument, since that would be redundant with the `version` command.

## Command options

We already used the [`CommandOptions`][] class to set some options relating to the argument parsing
behavior of the commands, but there are also several options that apply to commands directly.

Let's change our main method to add some more options:

```csharp
var options = new CommandOptions()
{
    Mode = ParsingMode.Default,
    ArgumentNameComparer = StringComparer.InvariantCulture,
    ArgumentNameTransform = NameTransform.DashCase,
    ValueDescriptionTransform = NameTransform.DashCase,
    CommandNameTransform = NameTransform.DashCase,
    CommandNameComparer = StringComparer.InvariantCulture,
    UsageWriter = new UsageWriter()
    {
        IncludeCommandHelpInstruction = true,
        IncludeApplicationDescriptionBeforeCommandList = true,
    },
};

var manager = new CommandManager(options);
return manager.RunCommand() ?? 1;
```

The first new option applies a name transformation to the command names if no explicit name is
specified, similar to the argument name transformation we used earlier. This means we can change our
class to this:

```csharp
[Command]
[Description("Reads a file and displays the contents on the command line.")]
class ReadCommand : ICommand
```

We've removed the explicit name from the [`CommandAttribute`][]. If you run the application, you'll
see the command is still called "read". That's because for subcommands, the name transformation will
strip the suffix "Command" from the name by default. This too can be customized with the
[`CommandOptions`][] class.

Next, we've set a [`CommandNameComparer`][] to make the command names case sensitive as well (the
default is case sensitive).

We also set some options to customize the usage help. The first one is the
[`IncludeCommandHelpInstruction`][] property, which causes the [`CommandManager`][] to print a
message like `Run 'tutorial <command> --help' for more information about a command.` after the
command list. This is disabled by default because the [`CommandManager`][] won't check if all the
commands actually have a `--help` argument. It's recommended to enable this if all your commands do.

> The help argument name is automatically adjusted based on the parsing mode and name transformation,
> so if you use the default mode, it'll say `-Help` instead.

The last option is [`IncludeApplicationDescriptionBeforeCommandList`][], which prints the assembly
description before the command list. However, if you run your application, you'll see it didn't do
anything. That's because the tutorial application doesn't have an assembly description. Insert the
following into a `<PropertyGroup>` in the tutorial.csproj file to fix that.

```xml
<Description>An application to read and write files.</Description>
```

Now, if you run the application without arguments, you'll see this:

```text
An application to read and write files.

Usage: tutorial <command> [arguments]

The following commands are available:

    read
        Reads a file and displays the contents on the command line.

    version
        Displays version information.

Run 'tutorial <command> --help' for more information about a command.
```

So we have an application description, and instructions for the user on how to get help for a
command. But, we still have only one command ("version" doesn't count), and the description we just
added is lying (the application only reads files).

## Multiple commands

An application with only one subcommand doesn't really need to use subcommands, so let's add a
second one. Create a new file in your project called WriteCommand.cs, and add the following code:

```csharp
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using System.ComponentModel;

namespace Tutorial;

[Command]
[Description("Writes text to a file.")]
class WriteCommand : ICommand
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    [Description("The path of the file to write.")]
    public string? Path { get; set; }

    [CommandLineArgument(Position = 1, IsRequired = true)]
    [Description("The text to write to the file.")]
    public string[]? Text { get; set; }

    [CommandLineArgument(IsShort = true)]
    [Description("Append to the file instead of overwriting it.")]
    public bool Append { get; set; }

    public int Run()
    {
        if (Append)
        {
            File.AppendAllLines(Path!, Text!);
        }
        else
        {
            File.WriteAllLines(Path!, Text!);
        }

        return 0;
    }
}
```

There's one thing here that we haven't seen before, and that's a multi-value argument. The `--text`
argument has an array type (`string[]`), which means it can have multiple values by supplying it
multiple times. We could, for example, use `--text foo --text bar` to assign the values "foo" and
"bar" to it. Because it's also a positional argument, we can also simply use `foo bar` to do the
same.

> A positional multi-value argument must always be the last positional argument.

This command will take the values from the `--text` argument and write them as lines to the specified
file, optionally appending to the file.

Let's build and run our application again, without arguments:

```text
./tutorial
```

Which now gives the following output:

```text
An application to read and write files.

Usage: tutorial <command> [arguments]

The following commands are available:

    read
        Reads a file and displays the contents on the command line.

    version
        Displays version information.

    write
        Writes text to a file.

Run 'tutorial <command> -Help' for more information about a command.
```

As you can see, our application picked up the new command without us needing to do anything. That's
because [`CommandManager`][] automatically looks for all command classes in the assembly.

If you run `./tutorial write --help`, you'll see the usage help for your new command:

```text
Writes text to a file.

Usage: tutorial write [--path] <string> [--text] <string>... [--append] [--help]

        --path <string>
            The path of the file to write.

        --text <string>
            The text to write to the file.

    -a, --append [<boolean>]
            Append to the file instead of overwriting it.

    -?, --help [<boolean>] (-h)
            Displays this help message.
```

We can test out our new command like this:

```text
$ ./tutorial write test.txt "Hello!" "Ookii.CommandLine is pretty neat." "At least I think so."
$ ./tutorial write test.txt "Thanks for using it!" -a
$ ./tutorial read test.txt
Hello!
Ookii.CommandLine is pretty neat.
At least I think so.
Thanks for using it!
```

Here, we wrote three lines of text to a file, then appended one more line, and read them back using
the "read" command.

## Asynchronous commands

If you want to use asynchronous code in your application, subcommands provide a way to do that too.

To make a command asynchronous, we have to implement the [`IAsyncCommand`][] interface. This interface
derives from the [`ICommand`][] interface, and adds a [`RunAsync()`][RunAsync()_1] method for you to implement. Then,
you can invoke your command using the [`CommandManager.RunCommandAsync()`][] method.

Let's make the `WriteCommand` asynchronous. When you do this, you typically only care about the
[`RunAsync()`][RunAsync()_1] method, but since [`IAsyncCommand`][] derives from [`ICommand`][], you must still provide a
[`Run()`][Run()_1] method. You could just leave it empty (or throw an exception), since [`RunCommandAsync()`][]
will never call it. An easier way is to derive your command from the [`AsyncCommandBase`][] class, which
provides a default implementation of the [`Run()`][Run()_0] method that will invoke [`RunAsync()`][RunAsync()_1] and wait for
it to finish.

So, we'll make the following changes to `WriteCommand`:

```csharp
[Command]
[Description("Writes text to a file.")]
class WriteCommand : AsyncCommandBase
{
    /* Properties are unchanged */

    public override async Task<int> RunAsync()
    {
        if (Append)
        {
            await File.AppendAllLinesAsync(Path!, Text!);
        }
        else
        {
            await File.WriteAllLinesAsync(Path!, Text!);
        }

        return 0;
    }
}
```

If you build and run your application now, you'll find that it works, despite not calling
[`RunCommandAsync()`][] yet. That's because [`RunCommand()`][] will invoke [`AsyncCommandBase.Run()`][], which
will create a task to run [`RunAsync()`][RunAsync()_0] and wait for it.

However, to fully take advantage of asynchronous tasks, you'll want to update the `Main()` method
as follows:

```csharp
public static async Task<int> Main()
{
    var options = new CommandOptions()
    {
        Mode = ParsingMode.Default,
        ArgumentNameComparer = StringComparer.InvariantCulture,
        ArgumentNameTransform = NameTransform.DashCase,
        ValueDescriptionTransform = NameTransform.DashCase,
        CommandNameTransform = NameTransform.DashCase,
        CommandNameComparer = StringComparer.InvariantCulture,
        UsageWriter = new UsageWriter()
        {
            IncludeCommandHelpInstruction = true,
            IncludeApplicationDescriptionBeforeCommandList = true,
        },
    };

    var manager = new CommandManager(options);
    return await manager.RunCommandAsync() ?? 1;
}
```

You'll notice that even with this change, the "read" command still works, despite not being
asynchronous. That's because the [`RunCommandAsync()`][] will check if a command implements
[`IAsyncCommand`][], and if it doesn't, it will fall back to just calling [`ICommand.Run()`][]. So you can
choose for each command to make it asynchronous or not according to its needs.

Converting `ReadCommand` to use asynchronous code is left as an exercise to the reader (hint: you'll
need .Net 7 for [`File.ReadLinesAsync()`][], and the
[`System.Linq.Async`](https://www.nuget.org/packages/System.Linq.Async) package to be able to use
the [`Take()`][] extension method on [`IAsyncEnumerable<T>`][]; or you can just use [`StreamReader`][]).

## Common arguments for commands

Sometimes, you'll want some arguments to be available to all commands. With Ookii.CommandLine, the
way to do this is to make a common base class. [`CommandLineParser`][] will consider base class members
when determining what arguments are available.

For example, if we wanted to make a common base class to share the `--path` argument between the
`read` and `write` commands, we could do so like this:

```csharp
abstract class BaseCommand : AsyncCommandBase
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    [Description("The path of the file.")]
    public string? Path { get; set; }
}

[Command]
class ReadCommand : BaseCommand
{
    /* Remove the Path property, leave everything else */
}

[Command]
class WriteCommand : BaseCommand
{
    /* Remove the Path property, leave everything else */
}
```

Now both commands share the `--path` argument defined in the base class, in addition to the arguments
they define themselves. Note that `BaseCommand` is not itself a command, because it doesn't have the
[`CommandAttribute`][] attribute (and also because it's `abstract`).

If you apply a [`ParseOptionsAttribute`][] attribute to the `BaseCommand` class, you can also share
parse options between multiple commands, without having to use [`CommandOptions`][] to do so.

## More information

I hope this tutorial helped you get started with Ookii.CommandLine. To learn more, check out the
following resources:

- [Usage documentation](README.md)
- [Class library documentation](https://www.ookii.org/Link/CommandLineDoc)
- [Sample applications](../src/Samples)

[`AliasAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_AliasAttribute.htm
[`ApplicationFriendlyNameAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ApplicationFriendlyNameAttribute.htm
[`AsyncCommandBase.Run()`]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_Commands_AsyncCommandBase_Run.htm
[`AsyncCommandBase`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_Commands_AsyncCommandBase.htm
[`CaseSensitive`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_CaseSensitive.htm
[`CommandAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_Commands_CommandAttribute.htm
[`CommandLineArgumentAttribute.IsLong`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsLong.htm
[`CommandLineArgumentAttribute.ShortName`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_ShortName.htm
[`CommandLineArgumentAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_CommandLineArgumentAttribute.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandManager.RunCommandAsync()`]: https://www.ookii.org/docs/commandline-3.1/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommandAsync.htm
[`CommandManager`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_Commands_CommandManager.htm
[`CommandNameComparer`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_Commands_CommandOptions_CommandNameComparer.htm
[`CommandOptions`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_Commands_CommandOptions.htm
[`CreateCommand()`]: https://www.ookii.org/docs/commandline-3.1/html/Overload_Ookii_CommandLine_Commands_CommandManager_CreateCommand.htm
[`DescriptionAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute
[`Environment.GetCommandLineArgs()`]: https://learn.microsoft.com/dotnet/api/system.environment.getcommandlineargs
[`File.ReadLinesAsync()`]: https://learn.microsoft.com/dotnet/api/system.io.file.readlinesasync
[`FileInfo`]: https://learn.microsoft.com/dotnet/api/system.io.fileinfo
[`GetCommand()`]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_Commands_CommandManager_GetCommand.htm
[`IAsyncCommand`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_Commands_IAsyncCommand.htm
[`IAsyncEnumerable<T>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1
[`ICommand.Run()`]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_Commands_ICommand_Run.htm
[`ICommand`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_Commands_ICommand.htm
[`IncludeApplicationDescriptionBeforeCommandList`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_UsageWriter_IncludeApplicationDescriptionBeforeCommandList.htm
[`IncludeCommandHelpInstruction`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_UsageWriter_IncludeCommandHelpInstruction.htm
[`Nullable<int>`]: https://learn.microsoft.com/dotnet/api/system.nullable-1
[`ParseOptions`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`ParsingMode.LongShort`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ParsingMode.htm
[`RunCommand()`]: https://www.ookii.org/docs/commandline-3.1/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommand.htm
[`RunCommandAsync()`]: https://www.ookii.org/docs/commandline-3.1/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommandAsync.htm
[`StreamReader`]: https://learn.microsoft.com/dotnet/api/system.io.streamreader
[`StringComparer.InvariantCulture`]: https://learn.microsoft.com/dotnet/api/system.stringcomparer.invariantculture
[`StringComparer.Ordinal`]: https://learn.microsoft.com/dotnet/api/system.stringcomparer.ordinal
[`StringComparer.OrdinalIgnoreCase`]: https://learn.microsoft.com/dotnet/api/system.stringcomparer.ordinalignorecase
[`System.ComponentModel.DescriptionAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute
[`Take()`]: https://learn.microsoft.com/dotnet/api/system.linq.enumerable.take
[`TypeConverter`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverter
[`Uri`]: https://learn.microsoft.com/dotnet/api/system.uri
[`ValidateRangeAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_Validation_ValidateRangeAttribute.htm
[ArgumentNameComparer_1]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_ParseOptions_ArgumentNameComparer.htm
[DefaultValue_1]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[Mode_2]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_Mode.htm
[Parse<T>()_1]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[Run()_0]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_Commands_AsyncCommandBase_Run.htm
[Run()_1]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_Commands_ICommand_Run.htm
[RunAsync()_0]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_Commands_AsyncCommandBase_RunAsync.htm
[RunAsync()_1]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_Commands_IAsyncCommand_RunAsync.htm
