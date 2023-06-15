# Tutorial: getting started with Ookii.CommandLine

This tutorial will show you the basics of how to use Ookii.CommandLine. It will show you how to
create an application that parses the command line and shows usage help, how to customize some of
the options—including the POSIX-like long/short mode—and how to use subcommands.

Refer to the [documentation](README.md) for more detailed information.

## Creating a project

Create a directory called "tutorial" for the project, and run the following command in that
directory:

```bash
dotnet new console --framework net7.0
```

Next, we will add a reference to Ookii.CommandLine's NuGet package:

```bash
dotnet add package Ookii.CommandLine
```

## Defining and parsing command line arguments

Add a file to your project called Arguments.cs, and insert the following code:

```csharp
using Ookii.CommandLine;
using System.ComponentModel;

namespace Tutorial;

[GeneratedParser]
[Description("Reads a file and displays the contents on the command line.")]
partial class Arguments
{
    [CommandLineArgument(IsPositional = true)]
    [Description("The path of the file to read.")]
    public required string Path { get; set; }
}
```

If you are targeting a .Net version before .Net 7.0, the `required` keyword is not available. In
that case, use the following code instead:

```csharp
[CommandLineArgument(IsPositional = true, IsRequired = true)]
[Description("The path of the file to read.")]
public string? Path { get; set; }
```

In Ookii.CommandLine, you define arguments by making a class that holds them, and adding properties
to that class. Every public property that has the [`CommandLineArgumentAttribute`][] defines an argument.

The code above defines a single argument called "Path", indicates it's the a positional argument,
and makes it required.

> You can use the [`CommandLineArgumentAttribute`][] to specify a custom name for your argument. If you
> don't, the property name is used.

The class above uses the `GeneratedParserAttribute`, which is not required, but is recommended
unless you are using an SDK older than .Net 6.0, or a language other than C# ([find out more](SourceGeneration.md)).

Now replace the contents of Program.cs with the following:

```csharp
using Tutorial;

var arguments = Arguments.Parse();
if (arguments == null)
{
    return 1;
}

foreach (var line in File.ReadLines(arguments.Path))
{
    Console.WriteLine(line);
}

return 0;
```

This code parses the arguments we defined, returns an error code if it was unsuccessful, and writes
the contents of the file specified by the path argument to the console.

The important part is the call to `Arguments.Parse()`. This static method was created by the
`GeneratedParserAttribute`, and will parse your arguments, handle and print any errors, and print
usage help if required.

> If you cannot use the `GeneratedParserAttribute`, call
> [`CommandLineParser.Parse<Arguments>()`][Parse<T>()_1] instead.

But wait, we didn't pass any arguments to this method? Actually, the method will call
[`Environment.GetCommandLineArgs()`][] to get the arguments. There are also overloads that take an
explicit `string[]` array with the arguments, if you want to pass them manually.

So, let's run our application. Build the application using `dotnet build`, and then, from the
`bin/Debug/net7.0` directory, run the following:

```bash
./tutorial ../../../tutorial.csproj
```

Which will give print the contents of the tutorial.csproj file:

```text
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Ookii.CommandLine" Version="4.0.0" />
  </ItemGroup>

</Project>
```

So far, so good. But what happens if we invoke the application without arguments? After all, we
made the `-Path` argument required. To try this, run the following command:

```bash
./tutorial
```

This gives the following output:

```text
The required argument 'Path' was not supplied.

Usage: tutorial [-Path] <String> [-Help] [-Version]

Run 'tutorial -Help' for more information.
```

As you can see, the generated `Parse()` method lets us know what's wrong (we didn't supply the
required argument), and shows some basic help, with an instruction on how to get more help.

Let's follow that instruction:

```bash
./tutorial -Help
```

Now we get this output:

```text
Reads a file and displays the contents on the command line.

Usage: tutorial [-Path] <String> [-Help] [-Version]

    -Path <String>
        The path of the file to read.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.
```

> The actual usage help uses color if your console supports it. See [here](images/color.png) for
> an example.

The generated `Parse()` method also took care of handling that `-Help` argument, and showed the
usage help.

This usage help includes the description we applied to the class (this is the application
description), and the `-Path` argument using the [`DescriptionAttribute`][]. This is how you can
provide detailed information about your arguments to your users. It's strongly recommended to
always add a description to your arguments.

You can also see that there are two more arguments that we didn't define: `-Help` and `-Version`.
These arguments are automatically added by Ookii.CommandLine.

We've already seen what `-Help` does: it shows the usage help. Even if you supply other arguments
along with `-Help`, it will still show the help and exit; it doesn't run the application. Basically,
the presence of `-Help` will override anything else.

The `-Version` argument shows version information about your application:

```text
$ ./tutorial -Version
tutorial 1.0.0
```

By default, it shows the assembly's name and informational version. It'll also show the assembly's
copyright information, if there is any (there's not in this case). You can also use the
`AssemblyTitleAttribute` or [`ApplicationFriendlyNameAttribute`][] attribute to specify a custom
name instead of the assembly name.

> If you define your own argument called "Help" or "Version", the automatic arguments won't be added.
> Also, you can disable the automatic arguments using the [`ParseOptionsAttribute`][] attribute or
> [`ParseOptions`][] class.

Note that the positional "Path" argument still has its name shown as `-Path`. That's because every
argument, even positional ones, can still be supplied by name. So if you run this:

```text
./tutorial -path ../../../tutorial.csproj
```

The output is the same as above.

> Argument names are case insensitive by default, so `-path` will work instead of `-Path`, as does
> `-PATH` or any other capitalization.

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
[Description("The maximum number of lines to output.")]
[ValueDescription("Number")]
[ValidateRange(1, null)]
[Alias("Lines")]
public int? MaxLines { get; set; }

[CommandLineArgument]
[Description("Use black text on a white background.")]
public bool Inverted { get; set; }
```

This defines two new arguments. The first, `-MaxLines`, uses `int?` as its type, so it will only
accept integer numbers, and be null if not supplied. This argument is not positional (you must use
the name), and it's optional. We've also added a validator to ensure the value is positive, and
since `-MaxLines` might be a bit verbose, we've given it an alias `-Lines`, which can be used as an
alternative name to supply the argument.

> An argument can have any number of aliases; just repeat the [`AliasAttribute`][] attribute.

The second argument, `-Inverted`, is a boolean, which means it's a switch argument. Switch arguments
don't need values, you either supply them or you don't.

Now, let's update Program.cs to use the new arguments:

```csharp
using Tutorial;

var arguments = Arguments.Parse();
if (arguments == null)
{
    return 1;
}

if (arguments.Inverted)
{
    Console.BackgroundColor = ConsoleColor.White;
    Console.ForegroundColor = ConsoleColor.Black;
}

var lines = File.ReadLines(arguments.Path);
if (arguments.MaxLines is int maxLines)
{
    lines = lines.Take(maxLines);
}

foreach (var line in lines)
{
    Console.WriteLine(line);
}

if (arguments.Inverted)
{
    Console.ResetColor();
}

return 0;
```

Now we can run the application like this:

```text
./tutorial ../../../tutorial.csproj -lines 5 -inverted
```

And it'll only show the first five lines of the file, using black-on-white text.

If you supply a value for `-MaxLines` that's not a valid integer, it shows an error message again:

```bash
./tutorial ../../../tutorial.csproj -lines hello
```

```text
The value 'hello' provided for argument 'MaxLines' could not be interpreted as a 'Number'.

Usage: tutorial [-Path] <String> [-Help] [-Inverted] [-MaxLines <Number>] [-Version]

Run 'tutorial -Help' for more information.
```

And because of the `ValidateRangeAttribute`, we can't specify a value less than 1 either.

```bash
./tutorial ../../../tutorial.csproj -lines 0
```

```text
The argument 'MaxLines' must be at least 1.

Usage: tutorial [-Path] <String> [-Help] [-Inverted] [-MaxLines <Number>] [-Version]

Run 'tutorial -Help' for more information.
```

Now, what do you think will happen if we run this command?

```text
./tutorial ../../../tutorial.csproj -m 5 -i
```

You might expect that to fail, as there are no arguments named `-m` or `-i`. However, if you tried
it, you can see that it worked. By default, Ookii.CommandLine will treat any unique prefix of a
command line argument's name or aliases as an alias for that argument. So, `-m` is automatically an
alias for `-MaxLines`. As is `-ma`, and `-max`, etc. And `-l` is as well, as it's a prefix of the
alias `-Lines`.

> This only works if the prefix matches exactly one argument. And if you don't like this behavior,
> it can be disabled using the `ParseOptionsAttribute.AutoPrefixAliases` property.

Let's take a look at the usage help for our updated application, by running `./tutorial -help`:

```text
Reads a file and displays the contents on the command line.

Usage: tutorial [-Path] <String> [-Help] [-Inverted] [-MaxLines <Number>] [-Version]

    -Path <String>
        The path of the file to read.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Inverted [<Boolean>]
        Use black text on a white background.

    -MaxLines <Number> (-Lines)
        The maximum number of lines to output. Must be at least 1.

    -Version [<Boolean>]
        Displays version information.
```

There's a few interesting things here. The `MaxLines` property has the `ValueDescriptionAttribute`
applied, and we can see that the value, "Number", is used inside the angle brackets after
`-MaxLines`. This is the *value description*, which is a short, typically one-word description of
the type of values the argument accepts. It defaults to the type name, but "Int32" might not be very
meaningful to people who aren't programmers, so we've changed it to "Number" instead.

You may have noticed above that the value description was also used in the error message when we
provided an invalid value.

You can also see that the [`ValidateRangeAttribute`][] doesn't just validate its condition, it also
adds that condition to the description of the argument (this can be disabled either globally or on
a per-validator basis if you want). So you don't have to worry about keeping the description and
the actual requirements in sync.

The `-MaxLines` argument also has its alias listed, just like the `-Help` argument.

> Don't like the way the usage help looks? It can be fully customized! Check out the
> [custom usage sample](../src/Samples/CustomUsage) for an example of that.

## Default values

Above, we used a nullable value type ([`Nullable<int>`][], or `int?`) so we could tell whether the
argument was supplied. Instead, we could also set a default value. This can easily be done by
initializing the property with that value:

```csharp
[CommandLineArgument]
[ValidateRange(1, null)]
[Alias("Lines")]
public int MaxLines { get; set; } = 10;
```

> Instead of initializing the property, you can also use the
> [`CommandLineArgumentAttribute.DefaultValue`][] property, which can be useful if e.g. you're not
> using an automatic property (so you can't have a direct initializer like that). And, that property
> accepts not just the argument's actual type, but also any string that can be converted to it. For
> example, both `[CommandLineArgument(DefaultValue = 10)]` and `[CommandLineArgument(DefaultValue = "10")]`
> are equivalent to the above. Handy if your argument's type doesn't have literals.

This default value would be shown in the usage help as well, similar to the validator:

```text
    -MaxLines <Number> (-Lines)
        The maximum number of lines to output. Must be at least 1. Default value: 10.
```

## POSIX conventions and other options

Ookii.CommandLine offers many options to customize the way it parses the command line. For example,
you can disable the use of white space as a separator between argument names and values, and specify
custom separators. You can specify custom argument name prefixes, instead of `-` which is the
default (on Windows only, `/` is also accepted by default). You can make the argument names case
sensitive. And there's more.

One thing you may want to do is use POSIX-like conventions, instead of the default PowerShell-like
parsing behavior. With POSIX conventions, arguments have separate long and short, one-character
names, which use different prefixes (typically `--` for long names and `-` for short). Argument
names are typically lowercase, with dashes between words, and are case sensitive. These are the same
conventions followed by tools such as `dotnet` or `git`, and many others. For a cross-platform
application, you may prefer these conventions over the default, but it's up to you of course.

A convenient way to change these options is to use the [`ParseOptionsAttribute`][], which you can
apply to your class. Let's use it to enable POSIX mode:

```csharp
[GeneratedParser]
[Description("Reads a file and displays the contents on the command line.")]
[ParseOptions(IsPosix = true)]
partial class Arguments
{
    [CommandLineArgument(IsPositional = true)]
    [Description("The path of the file to read.")]
    public required string Path { get; set; }

    [CommandLineArgument(IsShort = true)]
    [Description("The maximum number of lines to output.")]
    [ValueDescription("number")]
    [ValidateRange(1, null)]
    [Alias("lines")]
    public int? MaxLines { get; set; }

    [CommandLineArgument(IsShort = true)]
    [Description("Use black text on a white background.")]
    public bool Inverted { get; set; }
}
```

The `ParseOptionsAttribute.IsPosix` property is actually a shorthand way to set several related
properties. The above attribute is identical to this:

```csharp
[ParseOptions(Mode = ParsingMode.LongShort,
    CaseSensitive = true,
    ArgumentNameTransform = NameTransform.DashCase,
    ValueDescriptionTransform = NameTransform.DashCase)]
```

We've done a few things here: we've turned on an alternative set of parsing rules by setting the
[`Mode`][Mode_2] property to [`ParsingMode.LongShort`][], we've made argument names case sensitive,
and we've applied a name transformation to both argument names and value descriptions, which will
make them lower case with dashes between words (e.g. "max-lines").

Long/short mode is the key to getting POSIX-like behavior. It allows every argument to have two
separate names: a long name, using the `--` prefix by default, and a single-character short name
using the `-` prefix (and `/` on Windows).

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

    -m, --max-lines <number> (--lines)
            The maximum number of lines to output. Must be at least 1.

        --version [<boolean>]
            Displays version information.
```

As you can see, the format is slightly different, giving more prominence to the short names. You
can see the result of the name transformation on all the arguments and value descriptions, including
the automatic `--help` and `--version` arguments, which are now also lower case.

In addition to the [`ParseOptionsAttribute`][] attribute, you can also use the [`ParseOptions`][]
class to specify these and many other options. [`ParseOptions`][] can also be used to customize
where to write errors and help, and to customize the usage help. You can pass an instance of the
[`ParseOptions`][] class to the generated `Parse()` method.

## Using subcommands

Many applications have multiple functions, which are invoked through subcommands. Think for example
of the `dotnet` application, which has commands like `dotnet build` and `dotnet run`, or something
like `git` with commands like `git pull` or `git cherry-pick`. Each command does something
different, and needs its own command line arguments.

Creating subcommands with Ookii.CommandLine is very similar to what we've been doing already. A
subcommand is a class that defines arguments, same as before; the class will just have to implement
the [`ICommand`][] interface, and use the [`CommandAttribute`][] attribute. And, instead of using
`Parse()` directly, we'll use the command manager.

Let's change the example we've built so far to use subcommands. I'm going to continue with the
POSIX-like long/short mode settings, but if you prefer the defaults, you can go back to that version
too.

First, we'll add another `using` statement to Arguments.cs:

```csharp
using Ookii.CommandLine.Commands;
```

Then, we'll rename our `Arguments` class to `ReadCommand` (we'll use the class name to derive the
command name), and change it into a subcommand:

```csharp
[GeneratedParser]
[Command]
[Description("Reads a file and displays the contents on the command line.")]
partial class ReadCommand : ICommand
```

We've added the [`CommandAttribute`][], which indicates the class is a command, and can also be used
to set an explicit name if you don't want to use the class name. We've also added the [`ICommand`][]
interface, which all commands must implement.

Note that we've *removed* the [`ParseOptionsAttribute`][]. Options set with the attribute would
apply only to the command with the attribute, and usually you want to use the same options for all
commands. So, we'll set our options a different way further down.

We don't have to change anything about the properties defining the arguments. However, we do have to
implement the [`ICommand`][] interface, which has a single method called [`Run()`][Run()_1]. To
implement it, we take the code from Program.cs and move it into this method:

```csharp
public int Run()
{
    if (Inverted)
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
    }

    var lines = File.ReadLines(Path);
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

And that's it: we've now defined a command. However, we still need to change the application to
use commands instead of just parsing arguments from a single class. To do this, we'll use the
`CommandManager` class.

First, we'll add a file named GeneratedManager.cs, with these contents:

```csharp
using Ookii.CommandLine.Commands;

namespace Tutorial;

[GeneratedCommandManager]
partial class GeneratedManager
{
}
```

The `GeneratedCommandManagerAttribute` is similar to the `GeneratedParserAttribute`, except it turns
the target class into a command manager. The `GeneratedCommandManagerAttribute` will make your class
derive from `CommandManager`, and generates code to find and instantiate the commands in this
assembly.

> You can also use `CommandManager` directly, without a generated class, in which case reflection
> is used to find the commands. Do this if you can't use [source generation](SourceGeneration.md).

Now replace the code in Program.cs with the following.

```csharp
using Ookii.CommandLine.Commands;
using Tutorial;

var options = new CommandOptions()
{
    IsPosix = true,
};

var manager = new GeneratedManager(options);
return manager.RunCommand() ?? 1;
```

That's all you need to do to find, parse arguments for, and run any command in your application.

Here, we use the [`CommandOptions`][] to set the same options as before, so they'll apply to every
command (even if currently we have only one command). The [`CommandOptions`][] class derives from
the [`ParseOptions`][] class, so it can be used to specify all the same options, in addition to
some that are specific to commands.

Actually, for [`CommandOptions`][] the meaning of `IsPosix` is slightly different. It sets the same
options as before, but also sets two additional ones. It's actually equivalent to the following:

```csharp
var options = new CommandOptions()
{
    Mode = ParsingMode.LongShort,
    ArgumentNameComparison = StringComparison.InvariantCulture,
    ArgumentNameTransform = NameTransform.DashCase,
    ValueDescriptionTransform = NameTransform.DashCase,
    CommandNameComparison = StringComparison.InvariantCulture,
    CommandNameTransform = NameTransform.DashCase,
};
```

So in addition to enabling what it did before, it also made command names case sensitive (they are
case insensitive by default, just like argument names) and transforms their names to lowercase
separated by dashes as well.

> Note that [`ParseOptions`][], and by extension [`CommandOptions`][], use a `StringComparison`
> value instead of just a [`CaseSensitive`][] property.

The [`RunCommand()`][] method will take the arguments from [`Environment.GetCommandLineArgs()`][]
(as before, you can also pass them explicitly), and uses the first argument as the command name. If
a command with that name exists, it uses [`CommandLineParser`][] to parse the arguments for that
command, and finally invokes the [`ICommand.Run()`][] method. If anything goes wrong, it will either
display a list of commands, or if a command has been found, the help for that command. The return
value is the value returned from [`ICommand.Run()`][], or null if parsing failed, in which case we
return a non-zero exit code to indicate failure.

> If you want to customize any of these steps, there are methods like [`GetCommand()`][] and
> [`CreateCommand()`][] that you can call to do this manually.

If we build our application, and run it without arguments (`./tutorial`), we see the following:

```text
Usage: tutorial <command> [arguments]

The following commands are available:

    read
        Reads a file and displays the contents on the command line.

    version
        Displays version information.

Run 'tutorial <command> --help' for more information about a command.
```

When no command, or an unknown command, is supplied, a list of commands is printed. The
[`DescriptionAttribute`][] for our class, which was the application description before, is now the
description of the command.

But why is the command called `read`, and not `read-command`, if it's based on the class name
`ReadCommand`? If you use a name transformation for command names, it will strip the suffix
"Command" from the name by default. Use the `CommandOptions.StripCommandNameSuffix` property to
customize that behavior.

There is a second command, `version`, which is automatically added unless there already is a command
with that name. It does the same thing as the `-Version` argument before.

Let's see the usage help for our command:

```text
./tutorial read --help
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

    -m, --max-lines <number> (--lines)
            The maximum number of lines to output. Must be at least 1.
```

There are two differences to spot from the earlier version: the usage syntax now says `tutorial read`
before the arguments, indicating you have to use the command, and there is no automatic `--version`
argument, since that would be redundant with the `version` command.

## Adding an application description

The usage help for the single arguments class would print an application description at the top,
but the command list doesn't have anything like that. We can, however, add it.

To do, make the following change to the [`CommandOptions`][] (and add `using Ookii.CommandLine` at
the top of the file):

```csharp
var options = new CommandOptions()
{
    IsPosix = true,
    UsageWriter = new UsageWriter()
    {
        IncludeApplicationDescriptionBeforeCommandList = true,
    }
};
```

We've set the [`IncludeApplicationDescriptionBeforeCommandList`][] option, which prints the assembly
description before the command list. So to set a description, we'll add one in the tutorial.csproj
file.

```xml
<PropertyGroup>
  <Description>An application to read and write files.</Description>
</PropertyGroup>
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

## Multiple commands

An application with only one subcommand doesn't really need to use subcommands, so let's add a
second one. Create a new file in your project called WriteCommand.cs, and add the following code:

```csharp
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using System.ComponentModel;

namespace Tutorial;

[GeneratedParser]
[Command]
[Description("Writes text to a file.")]
partial class WriteCommand : ICommand
{
    [CommandLineArgument(IsPositional = true)]
    [Description("The path of the file to write.")]
    public required string Path { get; set; }

    [CommandLineArgument(IsPositional = true)]
    [Description("The text to write to the file.")]
    public required string[] Text { get; set; }

    [CommandLineArgument(IsShort = true)]
    [Description("Append to the file instead of overwriting it.")]
    public bool Append { get; set; }

    public int Run()
    {
        if (Append)
        {
            File.AppendAllLines(Path, Text);
        }
        else
        {
            File.WriteAllLines(Path, Text);
        }

        return 0;
    }
}
```

There's one thing here that we haven't seen before, and that's a multi-value argument. The `--text`
argument has an array type (`string[]`), which means it can have multiple values by supplying it
multiple times. We could, for example, use `--text foo --text bar` to assign the values "foo" and
"bar" to it. Because it's also a positional argument, we can simply use `foo bar` to do the same.

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

Run 'tutorial <command> --help' for more information about a command.
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

```bash
./tutorial write test.txt "Hello!" "Ookii.CommandLine is pretty neat." "At least I think so."
./tutorial write test.txt "Thanks for using it!" -a
./tutorial read test.txt
```

Here, we wrote three lines of text to a file, then appended one more line, and read them back using
the "read" command.

## Asynchronous commands

If you want to use asynchronous code in your application, subcommands provide a way to do that too.

To make a command asynchronous, we have to implement the [`IAsyncCommand`][] interface. This
interface derives from the [`ICommand`][] interface, and adds a [`RunAsync()`][RunAsync()_1] method
for you to implement. Then, you can invoke your command using the
[`CommandManager.RunCommandAsync()`][] method.

Because you still have to implement [`Run()`][Run()_1] when you use the [`IAsyncCommand`][]
interface, Ookii.CommandLine also provides the [`AsyncCommandBase`][] class for convenience, which
provides a default implementation of the [`Run()`][Run()_0] method that will invoke
[`RunAsync()`][RunAsync()_1] and wait for it to finish.

So, we'll make the following changes to `WriteCommand`:

```csharp
[GeneratedParser]
[Command]
[Description("Writes text to a file.")]
partial class WriteCommand : AsyncCommandBase
{
    /* Properties are unchanged */

    public override async Task<int> RunAsync()
    {
        if (Append)
        {
            await File.AppendAllLinesAsync(Path, Text);
        }
        else
        {
            await File.WriteAllLinesAsync(Path, Text);
        }

        return 0;
    }
}
```

If you build and run your application now, you'll find that it works, because of the
[`AsyncCommandBase.Run()`][] method.

However, to fully take advantage of asynchronous tasks, you'll want to replace the
[`RunCommand()`][] method call with [`RunCommandAsync()`][] in Program.cs:

```csharp
return await manager.RunCommandAsync() ?? 1;
```

You'll notice that even with this change, the "read" command still works, despite not being
asynchronous. That's because the [`RunCommandAsync()`][] supports both synchronous and asynchronous
commands, so you can mix and match them as you please.

Converting `ReadCommand` to use asynchronous code is left as an exercise to the reader (hint: you'll
need the [`System.Linq.Async`](https://www.nuget.org/packages/System.Linq.Async) package to be able
to use the [`Take()`][] extension method on the [`IAsyncEnumerable<T>`][] returned by
[`File.ReadLinesAsync()`][]).

## Common arguments for commands

Sometimes, you'll want some arguments to be available to all commands. With Ookii.CommandLine, the
way to do this is to make a common base class. [`CommandLineParser`][] will consider base class members
when determining what arguments are available. For example, here we could move the `--path` argument
to a common base class.

For more information on how to do this, see the
[documentation on subcommand base classes](Subcommands.md#multiple-commands-with-common-arguments).

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
[`CommandLineArgumentAttribute.DefaultValue`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValue.htm
[`CommandLineArgumentAttribute.IsLong`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IsLong.htm
[`CommandLineArgumentAttribute.ShortName`]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_ShortName.htm
[`CommandLineArgumentAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_CommandLineArgumentAttribute.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandManager.RunCommandAsync()`]: https://www.ookii.org/docs/commandline-3.1/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommandAsync.htm
[`CommandManager`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_Commands_CommandManager.htm
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
[`Nullable<int>`]: https://learn.microsoft.com/dotnet/api/system.nullable-1
[`ParseOptions`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`ParsingMode.LongShort`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_ParsingMode.htm
[`RunCommand()`]: https://www.ookii.org/docs/commandline-3.1/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommand.htm
[`RunCommandAsync()`]: https://www.ookii.org/docs/commandline-3.1/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommandAsync.htm
[`Take()`]: https://learn.microsoft.com/dotnet/api/system.linq.enumerable.take
[`Uri`]: https://learn.microsoft.com/dotnet/api/system.uri
[`ValidateRangeAttribute`]: https://www.ookii.org/docs/commandline-3.1/html/T_Ookii_CommandLine_Validation_ValidateRangeAttribute.htm
[Mode_2]: https://www.ookii.org/docs/commandline-3.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_Mode.htm
[Parse<T>()_1]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[Run()_0]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_Commands_AsyncCommandBase_Run.htm
[Run()_1]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_Commands_ICommand_Run.htm
[RunAsync()_1]: https://www.ookii.org/docs/commandline-3.1/html/M_Ookii_CommandLine_Commands_IAsyncCommand_RunAsync.htm
