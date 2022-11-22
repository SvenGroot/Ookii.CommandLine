# Long/short mode sample

This sample shows the alternate long/short parsing mode, as well as some other options that can
be customized. For example, it uses a NameTransform to make all the arguments dash-case, and uses
case-sensitive argument matching.

In long/short mode, each argument can have a long name, using the `--` prefix (by default), and a
one-character short name, using the `-` prefix.

It uses the same arguments as the [Parser Sample](../Parser), so see that for more details about
each argument.

The default usage help has a slightly different format to accommodate the short names.

```text
Sample command line application using long/short parsing mode. The application parses the command
line and prints the results, but otherwise does nothing and none of the arguments are actually used
for anything.

Usage: LongShort [--source] <string> [--destination] <string> [[--operation-index] <number>]
   [--count <number>] [--date <date-time>] [--day <day-of-week>] [--help] [--process] [--value
   <string>...] [--verbose] [--version]

    -s, --source <string>
            The source data.

    -d, --destination <string>
            The destination data.

        --operation-index <number>
            The operation's index. Default value: 1.

    -c, --count <number>
            Provides the count for something to the application.

    -D, --date <date-time>
            Provides a date to the application.

        --day <day-of-week>
            This is an argument using an enumeration type. Possible values: Sunday, Monday, Tuesday,
            Wednesday, Thursday, Friday, Saturday.

    -?, --help [<boolean>] (-h)
            Displays this help message.

    -p, --process [<boolean>]
            Does the processing.

        --value <string>
            This is an example of a multi-value argument, which can be repeated multiple times to
            set more than one value.

    -v, --verbose [<boolean>]
            Print verbose information; this is an example of a switch argument.

        --version [<boolean>]
            Displays version information.
```

Note that there is both a `-d` and a `-D` argument, possible due to the use of case-sensitive
argument names.

Long/short mode allows you to combine switches with short names, so running `LongShort -vp` sets
both `Verbose` and `Process` to true.
