# Parsing command line arguments

When you have [defined the command line arguments](DefiningArguments.md), you can parse the command
line to determine their values. There are two basic ways to do this, described below.

## Using the static helper method

The easiest way to do this is using the static [`CommandLineParser.Parse<T>()`][] methods. These methods
take care of parsing the arguments, and printing errors and usage if necessary. Its behavior
can optionally be customized with the [`ParseOptions`][] class.

A typical usage sample for the [`CommandLineParser`][] class is as follows:

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

This overload takes the arguments from the [`Environment.GetCommandLineArgs()`][] method, so there is
no need to pass them manually (though you can if desired).

If argument parsing is successful, the [`CommandLineParser`][] will create a new instance of the class
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
using the [`CommandLineArgumentAttribute.CancelParsing`][] property, a method argument, or the automatic
"Help" and "Version" arguments.

If argument parsing does fail or was canceled, the static [`Parse<T>()`][Parse<T>()_1] method returns `null`. The
method has already printed error and usage information, and there's nothing you need to do except
exit your application.

You can customize various aspects of the parsing behavior using either the [`ParseOptionsAttribute`][],
applied to your arguments class, or a [`ParseOptions`][] instance passed to the [`Parse<T>()`][Parse<T>()_1] method. The
latter can also be used to customize the usage help and error messages.

[`ParseOptions`][] can even be used to redirect where error and help are written.

```csharp
using var writer = LineWrappingTextWriter.ForStringWriter();
var options = new ParseOptions()
{
    Error = writer,
    Mode = ParsingMode.LongShort,
    DuplicateArguments = ErrorMode.Warning,
    UsageWriter = new UsageWriter(writer);
};

var arguments = CommandLineParser.Parse<MyArguments>(options);
if (arguments == null)
{
    // There are probably better ways to show help in a GUI app than this.
    MessageBox.Show(writer.BaseWriter.ToString());
    return 1;
}
```

In the vast majority of cases, [`ParseOptionsAttribute`][] and [`ParseOptions`][] should be sufficient to
customize the parsing behavior to your liking. If you need access to the [`CommandLineParser`][] instance
after parsing finished, you can use [injection](DefiningArguments.md#commandlineparser-injection),
so it will rarely be necessary to use any other method.

## Custom error messages

There are two ways you can customize the error messages shown to the user.

First, all error messages for the [`CommandLineArgumentException`][] class are obtained from the
[`LocalizedStringProvider`][] class. Create a class that derives from the [`LocalizedStringProvider`][]
class and override its members to customize any or all error messages. You can specify a custom
string provider using the [`ParseOptions.StringProvider`][] class.

The alternative is to use the manual parsing method below, and use the
[`CommandLineArgumentException.Category`][] property to determine the cause of the exception and create
your own error message.

## Manual parsing and error handling

However, sometimes you may want even more fine-grained control. This includes the ability to handle
the [`ArgumentParsed`][] and [`DuplicateArgument`][DuplicateArgument_0] events, and to get additional information about the
arguments using the [`Arguments`][Arguments_0] property or the [`GetArgument`][] function.

In this case, you can manually create an instance of the [`CommandLineParser<T>`][] class. Then, call
the [`Parse()`][Parse()_5] method, passing the arguments your application received.

> The [`CommandLineParser`][] class is not actually generic, and has an instance [`Parse()`][Parse()_6] method that
> returns an `object?`. The [`CommandLineParser<T>`][] class derives from the [`CommandLineParser`][] class
> and adds strongly-typed [`Parse()`][Parse()_5] methods so you don't need to cast the return value.

If argument parsing fails, the [`CommandLineParser<T>.Parse()`][] method will throw a
[`CommandLineArgumentException`][] exception, which you need to handle. You can simply print the
exception message, or check the [`CommandLineArgumentException.Category`][] property to determine the
cause of the error.

The non-static [`Parse()`][Parse()_5] method returns null _only_ if parsing was canceled. If you have no arguments
that cancel parsing, you can assume the return value is never null, but remember that the automatic
"Help" and "Version" properties also cancel parsing. Note that when using the instance methods, the
"Help" argument doesn't actually show help; it relies on the caller to do so.

If parsing was canceled by a method argument (including the automatic "Version" argument), that does
not necessarily mean that you should display usage help. Check the [`HelpRequested`][] property to see
whether you need to show help. This property will _always_ be true if an exception was thrown,
and _always_ be false if the [`Parse()`][Parse()_5] method returned an instance.

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
        parser.WriteUsage();
    }

    return 1;
}
```

You can customize some aspects of parsing by setting properties on the [`CommandLineParser`][] class
before calling [`Parse()`][Parse()_6], but some settings can only be changed using the [`ParseOptionsAttribute`][]
attribute and the [`ParseOptions`][] class (which you can pass to the [`CommandLineParser<T>`][]
constructor). Some properties of the [`ParseOptions`][] class (like [`Error`][]) are not used with the
instance methods, as they apply to the static [`Parse<T>()`][Parse<T>()_1] only.

Next, we'll take a look at [generating usage help](UsageHelp.md).

[`ArgumentParsed`]: https://www.ookii.org/docs/commandline-3.0-preview/html/E_Ookii_CommandLine_CommandLineParser_ArgumentParsed.htm
[`CommandLineArgumentAttribute.CancelParsing`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[`CommandLineArgumentException.Category`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineArgumentException_Category.htm
[`CommandLineArgumentException`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineArgumentException.htm
[`CommandLineParser.Parse<T>()`]: https://www.ookii.org/docs/commandline-3.0-preview/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandLineParser<T>.Parse()`]: https://www.ookii.org/docs/commandline-3.0-preview/html/Overload_Ookii_CommandLine_CommandLineParser_1_Parse.htm
[`CommandLineParser<T>`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineParser_1.htm
[`Environment.GetCommandLineArgs()`]: https://learn.microsoft.com/dotnet/api/system.environment.getcommandlineargs
[`Error`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_ParseOptions_Error.htm
[`GetArgument`]: https://www.ookii.org/docs/commandline-3.0-preview/html/M_Ookii_CommandLine_CommandLineParser_GetArgument.htm
[`HelpRequested`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineParser_HelpRequested.htm
[`LocalizedStringProvider`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_LocalizedStringProvider.htm
[`ParseOptions.StringProvider`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_ParseOptions_StringProvider.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[Arguments_0]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_CommandLineParser_Arguments.htm
[DuplicateArgument_0]: https://www.ookii.org/docs/commandline-3.0-preview/html/E_Ookii_CommandLine_CommandLineParser_DuplicateArgument.htm
[Parse()_5]: https://www.ookii.org/docs/commandline-3.0-preview/html/Overload_Ookii_CommandLine_CommandLineParser_1_Parse.htm
[Parse()_6]: https://www.ookii.org/docs/commandline-3.0-preview/html/Overload_Ookii_CommandLine_CommandLineParser_Parse.htm
[Parse<T>()_1]: https://www.ookii.org/docs/commandline-3.0-preview/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
