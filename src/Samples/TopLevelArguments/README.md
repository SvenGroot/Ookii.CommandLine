# Subcommands with top-level arguments sample

This sample shows an alternative way to define arguments that are common to every command. Rather
than using a base class with the common arguments, which makes the common arguments part of each
command (as shown in the [nested commands sample](../NestedCommands)), this sample defines several
top-level arguments that are not part of any command.

The commands themselves are based on the regular [subcommand sample](../Subcommand), so see that for
more detailed descriptions. This sample uses POSIX conventions, for variation, but this isn't
required.

These [top-level arguments](TopLevelArguments.cs) include a required positional argument that
indicates the command name to run. The argument uses [`CancelMode.Success`][] so that parsing will stop
at that point, while still returning success. The main function can then run that command, and pass
the remaining arguments to the command.

The sample also customizes the usage help in two ways. The [`TopLevelUsageWriter`](TopLevelUsageWriter.cs)
is used for the top-level arguments themselves. It alters the usage syntax to show the positional
arguments last, and to indicate additional command-specific arguments can follow them. It also
shows the command list after the usage help for the arguments.

The [`CommandUsageWriter`](CommandUsageWriter.cs) is used for the command manager and the commands
themselves. It is used to disable the command list usage syntax when writing the command list as part
of the top-level usage help, and to include text in the syntax to indicate there are additional
global arguments.

This means we get the following if running `./TopLevelArguments -Help`:

```text
Subcommands with top-level arguments sample for Ookii.CommandLine.

Usage: TopLevelArguments [--encoding <encoding>] [--help] [--version] [--path] <file-info>
   [--command] <string> [command arguments]

        --path <file-info>
            The path of the file to read or write.

        --command <string>
            The command to run. After this argument, all remaining arguments are passed to the
            command.

    -e, --encoding <encoding>
            The encoding to use to read the file. The default value is utf-8.

    -?, --help [<boolean>] (-h)
            Displays this help message.

        --version [<boolean>]
            Displays version information.

The following commands are available:

    read
        Reads and displays data from a file using the specified encoding, wrapping the text to fit
        the console.

    write
        Writes lines to a file, wrapping them to the specified width.

Run 'TopLevelArguments [global arguments] <command> --help' for more information about a command.
```

And the following if we run `./TopLevelArguments somefile.txt write -Help`

```text
Writes lines to a file, wrapping them to the specified width.

Usage:  TopLevelArguments [global arguments] write [[--lines] <string>...] [--help]
   [--maximum-line-length <int32>] [--overwrite]

        --lines <string>
            The lines of text to write to the file; if no lines are specified, this application will
            read from standard input instead.

    -?, --help [<boolean>] (-h)
            Displays this help message.

    -m, --maximum-line-length <int32>
            The maximum length of the lines in the file, or 0 to have no limit. Must be at least 0.
            Default value: 79.

    -o, --overwrite [<boolean>]
            When this option is specified, the file will be overwritten if it already exists.
```

[`CancelMode.Success`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CancelMode.htm
