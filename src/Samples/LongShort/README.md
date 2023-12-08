# Long/short mode sample

This sample alters the behavior of Ookii.CommandLine to be more like the POSIX conventions for
command line arguments. To do this, it enables the alternate long/short parsing mode, uses a
[name transformation](../../../docs/DefiningArguments.md#name-transformation) to make all the
argument names lower case with dashes between the words, and uses case-sensitive argument names.

The [`ParseOptionsAttribute.IsPosix`][] property is used to enable all these options at once. It is
equivalent to the following:

```csharp
[ParseOptions(Mode = ParsingMode.LongShort,
    CaseSensitive = true,
    ArgumentNameTransform = NameTransform.DashCase,
    ValueDescriptionTransform = NameTransform.DashCase)]
```

This sample uses the same arguments as the [parser sample](../Parser), so see that sample's source
for more details about each argument.

In long/short mode, each argument can have a long name, using the `--` prefix, and a one-character
short name, using the `-` prefix (and `/` on Windows). The prefixes can be customized if desired.

When in this mode, the default usage help has a slightly different format to accommodate the short
names.

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
both `--verbose` and `--process` to true.

[`ParseOptionsAttribute.IsPosix`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_ParseOptionsAttribute_IsPosix.htm
