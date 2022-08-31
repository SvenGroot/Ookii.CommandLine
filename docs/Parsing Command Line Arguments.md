# Parsing command line arguments

When you have defined the command line arguments, you can now parse the command line to determine the argument values.
There are two ways to do this, described below.

More detailed examples of both methods are available with the sample projects in the source code,
and as part of the Ookii.CommandLine.Sample NuGet package.

## Using the static helper method

The easiest way to do this is using the static `CommandLineParser.Parse<T>` methods. These methods
take care of parsing the arguments, and printing errors and usage if necessary. Its behavior
can optionally be customized with the `ParseOptions` class.

A typical usage sample for the `CommandLineParser` class is as follows:

```csharp
static void Main(string[] args)
{
    // You can use the invariant culture to ensure a consistent parsing experience for arguments
    // that have culture-specific parsing behavior such as dates and floating point numbers.
    var options = new ParseOptions()
    {
        Culture = CultureInfo.InvariantCulture
    };

    var arguments = CommandLineParser.Parse<MyArguments>(args, options);
    if (arguments != null)
        RunApplication(arguments);
}
```

The static `Parse<T>` method returns `null` if an error occurred, or if parsing was cancelled by
an argument that has the `CommandLineArgumentAttribute.CancelParsing` property set to `true`.
In those cases, the static `Parse<T>` method has already printed error and usage information, and
there's nothing you need to do (except possibly return an error status code if desired).

## Manually parsing

In most cases, the above method should be sufficient for your needs, but sometimes you may want
more control over the parsing behavior than the `ParseOptions` class allows. This includes the
ability to handle the `ArgumentParsed` event and to get additional information about the arguments
using the `Arguments` property or the `GetArgument` function.

In this case, you can manually create an instance of the `CommandLineParser` class, passing the type
of the class that defines the arguments to the constructor. Then call the `CommandLineParser.Parse`
method, passing the arguments your application received.

If argument parsing is successful, the `CommandLineParser.Parse` method will create a new instance of the class defining the arguments, passing the values parsed from the command line to the constructor parameters (if any). It will then set the value of each property to the value of the corresponding argument. This is not done in any particular order, so do not write code that makes assumptions about this. Finally, it will return this instance.

If argument parsing fails, the `CommandLineParser.Parse` method will throw a `CommandLineArgumentException` exception. Argument parsing can fail because not all required arguments were specified, an argument name was supplied without a value for an argument that’s not a switch, an unknown argument name was supplied, a non-multi-value argument was specified more than once, too many positional argument values were supplied, or argument value conversion failed for one of the arguments. Check the `CommandLineParser.Category` property to determine the cause of the exception.

The non-static `Parse` method returns `null` _only_ if parsing was cancelled using the
`CommandLineParser.ArgumentParsed` event, or by an argument that has the
`CommandLineArgumentAttribute.CancelParsing` property set to `true`. If you do not handle the event
and have no arguments with that property set, you can assume the return value is not `null`.

Here is a small sample of this kind of usage:

```csharp
static void Main(string[] args)
{
    CommandLineParser parser = new CommandLineParser(typeof(MyArguments));
    try
    {
        var arguments = (MyArguments)parser.Parse(args);
        RunApplication(arguments);
    }
    catch( CommandLineArgumentException ex )
    {
        Console.WriteLine(ex.Message);
        parser.WriteUsageToConsole();
    }
}
```

This sample tries to parse the arguments, and if it fails prints the error message and usage help to the console.

The `CommandLineParser` class provides several ways to customize the parsing behavior; check the class library documentation for more details.
