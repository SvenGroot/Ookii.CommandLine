# Parsing command line arguments

When you have [defined the command line arguments](DefiningArguments.md), you can parse the command
line to determine their values. There are two basic ways to do this, described below.

## Using the static helper method

The easiest way to parse the arguments is using the static [`CommandLineParser.Parse<T>()`][] helper
methods. These methods take care of parsing the arguments, handling errors, and printing usage help
if necessary.

A basic usage sample for the [`CommandLineParser`][] class is as follows:

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
no need to pass them manually (though you can if desired), and the default [`ParseOptions`][].

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

See the [`CommandLineArgumentErrorCategory`][] enumeration for more information. In addition, parsing
could have been canceled by an argument using the [`CommandLineArgumentAttribute.CancelParsing`][]
property, a method argument, or the automatic `-Help` and `-Version` arguments.

If argument parsing does fail or was canceled, the static [`Parse<T>()`][Parse<T>()_1] method
returns null. The method has already printed error and usage information, and there's nothing you
need to do except exit your application.

The static [`Parse<T>()`][Parse<T>()_1] will not throw an exception, unless the arguments type
violates one of the rules for valid arguments (such as defining an optional positional argument
after a required one). An exception from this method typically indicates a mistake in your arguments
class.

You can customize various aspects of the parsing behavior using either the
[`ParseOptionsAttribute`][], applied to your arguments class, or a [`ParseOptions`][] instance
passed to the [`Parse<T>()`][Parse<T>()_1] method. The latter can be used to set a few options not
available with the [`ParseOptionsAttribute`][], including options to customize the usage help and
error messages.

The [`ParseOptions`][] class can even be used to redirect where errors and help are written.

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
    MessageBox.Show(writer.ToString());
    return 1;
}
```

In the vast majority of cases, [`ParseOptionsAttribute`][] and [`ParseOptions`][] should be sufficient to
customize the parsing behavior to your liking. If you need access to the [`CommandLineParser`][] instance
after parsing finished, you can use [injection](DefiningArguments.md#commandlineparser-injection),
so it should rarely be necessary to use the manual parsing method.

### Custom error messages

If you wish to customize the error messages shown to the user if parsing fails, for example to
localize them, you can do that using the [`LocalizedStringProvider`][] class. This class is used as
the source for all error messages, as well as a number of other strings used by Ookii.CommandLine.

Create a class that derives from the [`LocalizedStringProvider`][] class and override its members to
customize any strings you wish to change. You can specify a custom string provider using the
[`ParseOptions.StringProvider`][] class.

Alternatively, if you need more error information, you can use the manual parsing method below, and
use the [`CommandLineArgumentException.Category`][] property to determine the cause of the exception
and create your own error message.

## Manual parsing and error handling

The static [`Parse<T>()`][Parse<T>()_1] method and its overloads will likely be sufficient for most
use cases. However, sometimes you may want even more fine-grained control. This includes the ability
to handle the [`ArgumentParsed`][] and [`DuplicateArgument`][DuplicateArgument_0] events, and to get
additional information about the arguments using the [`Arguments`][Arguments_0] property or the
[`GetArgument`][] function.

In this case, you can manually create an instance of the [`CommandLineParser<T>`][] class. Then, call
the instance `ParseWithErrorHandling()` or [`Parse()`][Parse()_5] method.

> The [`CommandLineParser<T>`][] class is a helper class that derives from [`CommandLineParser`][]
> and provides strongly-typed [`Parse()`][Parse()_5] and `ParseWithErrorHandling()` methods. You can
> also instantiate [`CommandLineParser`][] directly, and use its instance [`Parse()`][Parse()_6]
> and ``ParseWithErrorHandling()`` methods that return an `object?`.

Using `ParseWithErrorHandling()` is the easiest in this case, because it will still handling
printing error messages and usage help, the same as the static `Parse<T>()` method. If you want
more information about the error that occurred, you can access the `CommandLineParser.ParseResult`
property.

For example, you can use this approach if you want to return a success status when parsing was
canceled, but not when a parsing error occurred:

```csharp
var parser = new CommandLineParser<MyArguments>();
var arguments = parser.ParseWithErrorHandling();
if (parser == null)
{
    return parser.ParseResult.Status == ParseStatus.Canceled ? 0 : 1;
}
```

You can also use the `ParseResult.ArgumentName` property to determine which argument canceled
parsing in this case.

The [`CommandLineParser<T>.Parse()`][] method offers the most fine grained control, letting you
handle errors manually.

If argument parsing fails, the instance [`CommandLineParser<T>.Parse()`][] method will throw a
[`CommandLineArgumentException`][] exception, which you need to handle. You can simply print the
exception message, or check the [`CommandLineArgumentException.Category`][] property to determine
the cause of the error.

The non-static [`Parse()`][Parse()_5] method returns null _only_ if parsing was canceled. This also
happens if the automatic `-Help` or `-Version` arguments were used.

The automatic `-Help` argument implementation doesn't actually show help; it relies on the caller to
do so. However, if the [`Parse()`][Parse()_5] method returns null, it does _not_ necessarily mean
that you should display usage help. For example, the automatic `-Version` argument cancels parsing,
but should typically not result in a help message.

To see whether you should show usage help, check the [`HelpRequested`][] property. This property
will _always_ be true if an exception was thrown, and _always_ be false if the
[`Parse()`][Parse()_5] method returned an instance.

If the [`Parse()`][Parse()_5] method returned null, you can also check the
`CommandLineParser.ParseResult` property to see which argument canceled parsing.

Here is a basic sample of manual parsing and error handling using the [`Parse()`][Parse()_5] method:

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

If you wish to customize the behavior, that can still be done using the [`ParseOptionsAttribute`][]
attribute and the [`ParseOptions`][] class (which you can pass to the [`CommandLineParser<T>`][]
constructor). Some properties of the [`ParseOptions`][] class (like [`Error`][]) are not used with
the [`Parse()`][Parse()_5]  methods, as they apply to the `ParseWithErrorHandling()` and the static
[`Parse<T>()`][Parse<T>()_1] methods only.

Next, we'll take a look at [generating usage help](UsageHelp.md).

[`ArgumentParsed`]: https://www.ookii.org/docs/commandline-3.0/html/E_Ookii_CommandLine_CommandLineParser_ArgumentParsed.htm
[`CommandLineArgumentAttribute.CancelParsing`]: https://www.ookii.org/docs/commandline-3.0/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[`CommandLineArgumentErrorCategory`]: https://www.ookii.org/docs/commandline-3.0/html/T_Ookii_CommandLine_CommandLineArgumentErrorCategory.htm
[`CommandLineArgumentException.Category`]: https://www.ookii.org/docs/commandline-3.0/html/P_Ookii_CommandLine_CommandLineArgumentException_Category.htm
[`CommandLineArgumentException`]: https://www.ookii.org/docs/commandline-3.0/html/T_Ookii_CommandLine_CommandLineArgumentException.htm
[`CommandLineParser.Parse<T>()`]: https://www.ookii.org/docs/commandline-3.0/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-3.0/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandLineParser<T>.Parse()`]: https://www.ookii.org/docs/commandline-3.0/html/Overload_Ookii_CommandLine_CommandLineParser_1_Parse.htm
[`CommandLineParser<T>`]: https://www.ookii.org/docs/commandline-3.0/html/T_Ookii_CommandLine_CommandLineParser_1.htm
[`Environment.GetCommandLineArgs()`]: https://learn.microsoft.com/dotnet/api/system.environment.getcommandlineargs
[`Error`]: https://www.ookii.org/docs/commandline-3.0/html/P_Ookii_CommandLine_ParseOptions_Error.htm
[`GetArgument`]: https://www.ookii.org/docs/commandline-3.0/html/M_Ookii_CommandLine_CommandLineParser_GetArgument.htm
[`HelpRequested`]: https://www.ookii.org/docs/commandline-3.0/html/P_Ookii_CommandLine_CommandLineParser_HelpRequested.htm
[`LocalizedStringProvider`]: https://www.ookii.org/docs/commandline-3.0/html/T_Ookii_CommandLine_LocalizedStringProvider.htm
[`ParseOptions.StringProvider`]: https://www.ookii.org/docs/commandline-3.0/html/P_Ookii_CommandLine_ParseOptions_StringProvider.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-3.0/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-3.0/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[Arguments_0]: https://www.ookii.org/docs/commandline-3.0/html/P_Ookii_CommandLine_CommandLineParser_Arguments.htm
[DuplicateArgument_0]: https://www.ookii.org/docs/commandline-3.0/html/E_Ookii_CommandLine_CommandLineParser_DuplicateArgument.htm
[Parse()_5]: https://www.ookii.org/docs/commandline-3.0/html/Overload_Ookii_CommandLine_CommandLineParser_1_Parse.htm
[Parse()_6]: https://www.ookii.org/docs/commandline-3.0/html/Overload_Ookii_CommandLine_CommandLineParser_Parse.htm
[Parse<T>()_1]: https://www.ookii.org/docs/commandline-3.0/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
