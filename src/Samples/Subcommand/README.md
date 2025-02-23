# Subcommand sample

This sample is a simple demonstration of subcommands. The sample application defines two commands,
`read`, and `write`, which can be used to read or write a file, respectively.

The sample shows how to use both synchronous and asynchronous commands, and also contains an example
of a custom [`ArgumentConverter`][], used for the [`Encoding`][Encoding_1] class.

For detailed information, check the source of the [`ReadCommand`](ReadCommand.cs) class, the
[`WriteCommand`](WriteCommand.cs) class, and the [`Main()`](Program.cs) method to see it works.

This application uses [source generation](../../../docs/SourceGeneration.md) for both the commands,
and for the [`CommandManager`][] to find all commands and arguments at compile time. This enables
the application to be safely trimmed. You can try this out by running `dotnet publish --self-contained`
in the project's folder. This also works for applications without subcommands, even though this is
the only sample that demonstrates this by setting the `PublishTrimmed` property in the project file.

When invoked without arguments, a subcommand application prints the list of commands.

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

Like the usage help format for arguments, the command list format can also be customized using the
[`UsageWriter`][] class. If the console is capable, the command list also uses color.

If we run `./Subcommand write -Help`, we get the following:

```text
Writes lines to a file, wrapping them to the specified width.

Usage: Subcommand write [-Path] <FileInfo> [[-Lines] <String>...] [-Encoding <Encoding>] [-Help]
   [-MaximumLineLength <Int32>] [-Overwrite]

    -Path <FileInfo>
        The name of the file to write to.

    -Lines <String>
        The lines of text to write to the file; if no lines are specified, this application will
        read from standard input instead.

    -Encoding <Encoding>
        The encoding to use to write the file. The default value is utf-8.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -MaximumLineLength <Int32> (-Length)
        The maximum length of the lines in the file, or zero to have no limit. Must be at least 0.
        Default value: 79.

    -Overwrite [<Boolean>]
        When this option is specified, the file will be overwritten if it already exists.
```

This is just like normal usage help for an application using [`CommandLineParser`][] directly, but
note that it shows the command description, not the application description, and the command name is
included in the usage syntax.

Subcommands have an automatic `-Help` argument, but as you can see, no `-Version` argument. Instead,
there is an automatic `version` command, which has the same function. We can see that if we run
`./Subcommand version`:

```text
Ookii.CommandLine Subcommand Sample 4.0.0
Copyright (c) Sven Groot (Ookii.org)
This is sample code, so you can use it freely.
```

[`ArgumentConverter`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_ArgumentConverter.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandManager`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_CommandManager.htm
[`UsageWriter`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_UsageWriter.htm
[Encoding_1]: https://learn.microsoft.com/dotnet/api/system.text.encoding
