# Custom usage sample

This sample shows the flexibility of Ookii.CommandLine's usage help generation. It uses a custom
UsageWriter, along with a custom LocalizedStringProvider, to completely transform the way the usage
help looks.

This sample also uses long/short parsing mode, but everything in it is applicable to default mode as
well.

It uses the same arguments as the [Parser Sample](../Parser), so see that for more details about
each argument.

The usage help for this sample looks very different:

```text
DESCRIPTION:
  Sample command line application with highly customized usage help. The application parses the
  command line and prints the results, but otherwise does nothing and none of the arguments are
  actually used for anything.

USAGE:
  CustomUsage [--source] <string> [--destination] <string> [arguments]

OPTIONS:
  -c|--count <number>         Provides the count for something to the application. [range: 0-100]
  -d|--destination <string>   The destination data.
  -D|--date <date-time>       Provides a date to the application.
  --day <day-of-week>         This is an argument using an enumeration type.
  -h|--help                   Displays this help message.
  --operation-index <number>  The operation's index. [default: 1]
  -p|--process                Does the processing.
  -s|--source <string>        The source data.
  -v|--verbose                Print verbose information; this is an example of a switch argument.
  --value <string>            This is an example of a multi-value argument, which can be repeated
                              multiple times to set more than one value.
  --version                   Displays version information.
```

Customizing the usage like this is fairly simple, thanks to the LocalizedStringProvider. That same
class also allows you to customize error messages and automatic argument names and descriptions.

The sample also customizes the colors of the output, as shown in the below screenshot:

![Custom usage colors](../../../docs/images/custom_usage.png)

If you compare this with the usage output of the [Parser sample](../Parser), which uses the default
output format, you can see just how much you can change without needing to write code to manually
handle writing usage help (though you could also do that; all the information you need is provided
by the CommandLineParser class).
