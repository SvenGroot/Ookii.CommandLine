# Parsing command line arguments

When you have [defined the command line arguments](DefiningArguments.md), you can parse the command
line to determine their values. There are two basic ways to do this, described below.

## Using the static helper method

The easiest way to do this is using the static `CommandLineParser.Parse<T>` methods. These methods
take care of parsing the arguments, and printing errors and usage if necessary. Its behavior
can optionally be customized with the `ParseOptions` class.

A typical usage sample for the `CommandLineParser` class is as follows:

```csharp
public static int Main()
{
    var arguments = CommandLineParser.Parse<MyArguments>();
    if (arguments == null)
    {
        return 1; // Or a suitable error code.
    }

    return RunApplication(arguments);
}
```

This overload takes the arguments from the `Environment.GetCommandLineArgs()` method, so there is
no need to pass them manually (though you can if desired).

If argument parsing is successful, the `CommandLineParser` will create a new instance of the class
defining the arguments, passing the values parsed from the command line to the constructor
parameters (if any). It will then set the value of each property to the value of the corresponding
argument. This is not done in any particular order, so do not write code that makes assumptions
about this. Finally, it will return the instance.

Argument parsing can fail for a number of reason, including:

- Not all required arguments were specified.
- An argument name was supplied without a value for an argument that’s not a switch
- An unknown argument name was supplied
- A non-multi-value argument was specified more than once
- Too many positional argument values were supplied
- Argument value conversion failed for one of the arguments.
- An argument failed [validation](Validation.md).

There can be other reasons as well. In addition, parsing could have been canceled by an argument
using the `CommandLineArgumentAttribute.CancelParsing` property, a method argument, or the automatic
"Help" and "Version" arguments.

If argument parsing does fail or was canceled, the static `Parse<T>` method returns `null`. The
method has already printed error and usage information, and there's nothing you need to do except
exit your application.

You can customize various aspects of the parsing behavior using either the `ParseOptionsAttribute`,
applied to your arguments class, or a `ParseOptions` instance passed to the `Parse<T>()` method. The
latter can also be used to customize the usage help and error messages.

`ParseOptions` can even be used to redirect where error and help are written.

```csharp
using var writer = new StringWriter();
var options = new ParseOptions()
{
    Out = writer,
    Error = writer,
    Mode = ParsingMode.LongShort,
    DuplicateArguments = ErrorMode.Warning,
};

var arguments = CommandLineParser.Parse<MyArguments>(options);
if (arguments == null)
{
    // There are probably better ways to show help in a GUI app than this.
    MessageBox.Show(writer.ToString());
    return 1;
}
```

In the vast majority of cases, `ParseOptionsAttribute` and `ParseOptions` should be sufficient to
customize the parsing behavior to your liking. If you need access to the `CommandLineParser` instance
after parsing finished, you can use [injection](DefiningArguments.md#commandlineparser-injection),
so it will rarely be necessary to use any other method.

## Custom error messages

There are two ways you can customize the error messages shown to the user.

First, all error messages for the `CommandLineException` class are obtained from the
`LocalizedStringProvider` class. Create a class that derives from the `LocalizedStringProvider`
class and override its members to customize any or all error messages. You can specify a custom
string provider using the `ParseOptions.StringProvider` class.

The alternative is to use the manual parsing method below, and use the
`CommandLineArgumentException.Category` property to determine the cause of the exception and create
your own error message.

## Manual parsing and error handling

However, sometimes you may want even more fine-grained control. This includes the ability to handle
the `ArgumentParsed` and `DuplicateArgument` events, and to get additional information about the
arguments using the `Arguments` property or the `GetArgument` function.

In this case, you can manually create an instance of the `CommandLineParser<T>` class. Then, call
the `CommandLineParser<T>.Parse` method, passing the arguments your application received.

> The `CommandLineParser` class is not actually generic, and has an instance `Parse` method that
> returns an `object?`. The `CommandLineParser<T>` class derives from the `CommandLineParser` class
> and adds strongly-typed `Parse` methods so you don't need to cast the return value.

If argument parsing fails, the `CommandLineParser<T>.Parse` method will throw a
`CommandLineArgumentException` exception, which you need to handle. You can simply print the
exception message, or check the `CommandLineArgumentException.Category` property to determine the
cause of the error.

The non-static `Parse` method returns null _only_ if parsing was canceled. If you have no arguments
that cancel parsing, you can assume the return value is never null, but remember that the automatic
"Help" and "Version" properties also cancel parsing. Note that when using the instance methods, the
"Help" argument doesn't actually show help; it relies on the caller to do so.

If parsing was canceled by a method argument (including the automatic "Version" argument), that does
not necessarily mean that you should display usage help. Check the `HelpRequested` property to see
whether you need to show help. This property will _always_ be true if an exception was thrown,
and _always_ be false if the `Parse` method returned an instance.

Here is a basic sample of how to do this:

```csharp
static int Main()
{
    var parser = new CommandLineParser<MyArguments>();
    try
    {
        var arguments = parser.Parse();
        if (arguments != null)
        {
            return RunApplication(arguments);
        }
    }
    catch (CommandLineArgumentException ex)
    {
        Console.Error.WriteLine(ex.Message);
    }

    if (parser.HelpRequested)
    {
        parser.WriteUsageToConsole();
    }

    return 1;
}
```

You can customize some aspects of parsing by setting properties on the `CommandLineParser` class
before calling `Parse`, but some settings can only be changed using the `ParseOptionsAttribute`
attribute and the `ParseOptions` class (which you can pass to the `CommandLineParser<T>`
constructor). Some properties of the `ParseOptions` class (like `Out` and `Error`) are not used with
the instance methods, and you must manually pass the `WriteUsageOptions` class to
`WriteUsageToConsole` (or one of the other `WriteUsage` methods); the one contained in the
`ParseOptions` will not be used.

Next, we'll take a look at [generating usage help](UsageHelp.md).
