# Subcommand sample

This sample is a simple demonstration of subcommands. The sample application defines two commands,
"read", and "write", which can be used to read or write a file, respectively.

The sample shows how to use both synchronous and asynchronous commands, and also contains an example
of a custom TypeConverter, used for the Encoding class.

For detailed information, check the source of the [ReadCommand class](ReadCommand.cs), the
[WriteCommand class](WriteCommand.cs), and the [Main method](Program.cs) to see how this all works.

When invoked without arguments, a subcommand application prints the list of commands.

```text
Subcommand sample for Ookii.CommandLine.

Usage: SubCommand <command> [arguments]

The following commands are available:

    read
        Reads and displays data from a file using the specified encoding, wrapping the text to fit
        the console.

    version
        Displays version information.

    write
        Writes lines to a file, wrapping them to the specified width.

Run 'SubCommand <command> -Help' for more information about a command.
```

Like the usage help format for arguments, the command list format can also be customized. If the
console is capable, the command list also uses color.

If we run `SubCommand write -Help`, we get the following:

```text
Writes lines to a file, wrapping them to the specified width.

Usage: SubCommand write [-Path] <FileInfo> [[-Lines] <String>...] [-Encoding <Encoding>] [-Help]
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

This is just like normal usage help for an application using CommandLineParser directly, but note it
shows the command description, not the application description, and the command name is included
in the usage syntax.

Subcommands have an automatic `-Help` argument, but as you can see, no `-Version` argument. Instead,
there is an automatic `version` command, which has the same function. We can see that if we run
`SubCommand version`:

```text
Ookii.CommandLine Subcommand Sample 3.0.0
Copyright (c) Sven Groot (Ookii.org)
This is sample code, so you can use it freely.
```
