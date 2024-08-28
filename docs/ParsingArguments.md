# Parsing command line arguments

When you have [defined the command line arguments](DefiningArguments.md), you can parse the command
line to determine their values. There are two basic ways to do this, described below.

## Using the static helper methods

The easiest way to parse the arguments is using the static [`Parse()`][Parse()_7] methods that are
generated for your arguments class when using [source generation](SourceGeneration.md) with the
[`GeneratedParserAttribute`][]. These methods take care of parsing the arguments, handling errors,
and printing usage help if necessary.

A basic usage sample for the [`CommandLineParser`][] class is as follows:

```csharp
public static int Main()
{
    var arguments = MyArguments.Parse();
    if (arguments == null)
    {
        return 1; // Or a suitable error code.
    }

    return RunApplication(arguments);
}
```

This overload takes the arguments from the [`Environment.GetCommandLineArgs()`][] method, so there is
no need to pass them manually (though you can if desired), and the default [`ParseOptions`][].

If you cannot use source generation, you can call one of the [`CommandLineParser.Parse<T>()`][]
methods, which work the same way as the generated method.

If argument parsing is successful, the [`CommandLineParser`][] will create a new instance of the
class defining the arguments. It will then set the value of each property to the value of the
corresponding argument. This is not done in any particular order, so do not write code that makes
assumptions about this. Finally, it will return the instance.

Argument parsing can fail for a number of reason, including:

- Not all required arguments were specified.
- An argument name was supplied without a value for an argument that’s not a switch
- An unknown argument name was supplied
- A non-multi-value argument was specified more than once
- Too many positional argument values were supplied
- Argument value conversion failed for one of the arguments.
- An argument failed [validation](Validation.md).

See the [`CommandLineArgumentErrorCategory`][] enumeration for more information. In addition,
parsing could have been canceled by an argument using the
[`CommandLineArgumentAttribute.CancelParsing`][] property with [`CancelMode.Abort`][], a method
argument, or the automatic `-Help` and `-Version` arguments.

If argument parsing does fail or was canceled, the generated [`Parse()`][Parse()_7] method (as well as the static
[`CommandLineParser.Parse<T>()`][] method) returns null. The method has already printed error and
usage information, and there's nothing you need to do except exit your application.

The generated [`Parse()`][Parse()_7] methods and the static [`Parse<T>()`][Parse<T>()_1] method will never throw
a [`CommandLineArgumentException`][]. They can throw other exceptions if the arguments type violates one
of the rules for valid arguments (such as defining an optional positional argument after a required
one). An exception from this method typically indicates a mistake in your arguments class. When
using source generation, these kinds of errors are often caught at compile time.

You can customize various aspects of the parsing behavior using either the
[`ParseOptionsAttribute`][], applied to your arguments class, or a [`ParseOptions`][] instance
passed to the [`Parse()`][Parse()_7] method. The latter can be used to set a few options not available with the
[`ParseOptionsAttribute`][], including options to customize the usage help and error messages.

The [`ParseOptions`][] class can even be used to redirect where errors and help are written.

```csharp
using var writer = LineWrappingTextWriter.ForStringWriter();
var options = new ParseOptions()
{
    Error = writer,
    IsPosix = true,
    DuplicateArguments = ErrorMode.Warning,
    UsageWriter = new UsageWriter(writer);
};

var arguments = MyArguments.Parse(options);
if (arguments == null)
{
    // There are probably better ways to show help in a GUI app than this.
    MessageBox.Show(writer.ToString());
    return 1;
}
```

### Custom error messages

If you wish to customize the error messages shown to the user if parsing fails, for example to
localize them, you can do that using the [`LocalizedStringProvider`][] class. This class is used as
the source for all error messages, as well as a number of other strings used by Ookii.CommandLine.

Create a class that derives from the [`LocalizedStringProvider`][] class and override its members to
customize any strings you wish to change. You can specify a custom string provider using the
[`ParseOptions.StringProvider`][] class. Localizing some strings used in the usage help may also
require you to create a custom [`UsageWriter`][].

Alternatively, if you need more error information, you can use the manual parsing method below, and
use the [`CommandLineArgumentException.Category`][] property to determine the cause of the exception
and create your own error message.

## Manual parsing and error handling

The generated [`Parse()`][Parse()_7] methods and the static [`Parse<T>()`][Parse<T>()_1] method and
their overloads will likely be sufficient for most use cases. However, sometimes you may want even
more fine-grained control. This includes the ability to handle the [`ArgumentParsed`][],
[`UnknownArgument`][] and [`DuplicateArgument`][DuplicateArgument_0] events, and to get additional
information about the arguments using the [`Arguments`][Arguments_0] property or the
[`GetArgument`][] function.

In this case, you can manually create an instance of the [`CommandLineParser<T>`][] class. Then, call
the instance [`ParseWithErrorHandling()`][ParseWithErrorHandling()_1] or [`Parse()`][Parse()_5] method.

> The [`CommandLineParser<T>`][] class is a helper class that derives from [`CommandLineParser`][]
> and provides strongly-typed [`Parse()`][Parse()_5] and
> [`ParseWithErrorHandling()`][ParseWithErrorHandling()_1] methods.

If you are using source generation, you can call the generated [`CreateParser()`][CreateParser()_1] method that is added
to your class to get a [`CommandLineParser<T>`][] instance. Otherwise, simply use
`new CommandLineParser<MyArguments>()`.

Using [`ParseWithErrorHandling()`][ParseWithErrorHandling()_1] is the easiest in this case, because
it will still handle printing error messages and usage help, the same as the generated [`Parse()`][Parse()_7]
method and static [`Parse<T>()`][Parse<T>()_1] methods. If you want more information about the error
that occurred, you can access the [`CommandLineParser.ParseResult`][] property after parsing.

For example, you can use this approach if you want to return a success status when parsing was
canceled, but not when a parsing error occurred:

```csharp
var parser = MyArguments.CreateParser();
var arguments = parser.ParseWithErrorHandling();
if (arguments == null)
{
    return parser.ParseResult.Status == ParseStatus.Canceled ? 0 : 1;
}
```

Or, you could use this to handle the [`UnknownArgument`][] event to collect a list of unrecognized
arguments:

```csharp
var unknownArguments = new List<string>();
var parser = MyArguments.CreateParser();
parser.UnknownArgument += (_, e) =>
{
    // Note: in long/short mode, this may not have the desired effect for a combined switch argument
    // where one of the switches is unknown.
    unknownArguments.Add(e.Token);
    e.Ignore = true;
};

var arguments = parser.ParseWithErrorHandling();
if (arguments == null)
{
    return 1;
}
```

The status will be set to [`ParseStatus.Canceled`][] if parsing was canceled with [`CancelMode.Abort`][].

You can also use the [`ParseResult.ArgumentName`][] property to determine which argument canceled
parsing in this case. If an error occurred, the status will be [`ParseStatus.Error`][] and you can
use the [`ParseResult.LastException`][] property to access the actual error that occurred.

If parsing was canceled using [`CancelMode.Success`][], the status will be [`ParseStatus.Success`][], but
[`ParseResult.ArgumentName`][] will be non-null and set to the argument that canceled parsing. Use the
[`ParseResult.RemainingArguments`][] property to get any arguments that were not parsed.

For the most fine grained control, you can use the [`CommandLineParser<T>.Parse()`][] method, which
lets you handle errors manually.

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
[`CommandLineParser.ParseResult`][] property to see which argument canceled parsing.

Here is a basic sample of manual parsing and error handling using the [`Parse()`][Parse()_5] method:

```csharp
static int Main()
{
    var parser = MyArguments.CreateParser();
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
constructor or the generated [`CreateParser()`][CreateParser()_1] method). Some properties of the [`ParseOptions`][]
class (like [`Error`][]) are not used with the [`Parse()`][Parse()_5]  methods, as they apply to the
[`ParseWithErrorHandling()`][ParseWithErrorHandling()_1] and the static [`Parse<T>()`][Parse<T>()_1]
methods only.

Next, we'll take a look at [generating usage help](UsageHelp.md).

[`ArgumentParsed`]: https://www.ookii.org/docs/commandline-4.2/html/E_Ookii_CommandLine_CommandLineParser_ArgumentParsed.htm
[`CancelMode.Abort`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_CancelMode.htm
[`CancelMode.Success`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_CancelMode.htm
[`CommandLineArgumentAttribute.CancelParsing`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[`CommandLineArgumentErrorCategory`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_CommandLineArgumentErrorCategory.htm
[`CommandLineArgumentException.Category`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineArgumentException_Category.htm
[`CommandLineArgumentException`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_CommandLineArgumentException.htm
[`CommandLineParser.Parse<T>()`]: https://www.ookii.org/docs/commandline-4.2/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[`CommandLineParser.ParseResult`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineParser_ParseResult.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandLineParser<T>.Parse()`]: https://www.ookii.org/docs/commandline-4.2/html/Overload_Ookii_CommandLine_CommandLineParser_1_Parse.htm
[`CommandLineParser<T>`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_CommandLineParser_1.htm
[`Environment.GetCommandLineArgs()`]: https://learn.microsoft.com/dotnet/api/system.environment.getcommandlineargs
[`Error`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_ParseOptions_Error.htm
[`GeneratedParserAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_GeneratedParserAttribute.htm
[`GetArgument`]: https://www.ookii.org/docs/commandline-4.2/html/M_Ookii_CommandLine_CommandLineParser_GetArgument.htm
[`HelpRequested`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineParser_HelpRequested.htm
[`LocalizedStringProvider`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_LocalizedStringProvider.htm
[`ParseOptions.StringProvider`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_ParseOptions_StringProvider.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`ParseResult.ArgumentName`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_ParseResult_ArgumentName.htm
[`ParseResult.LastException`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_ParseResult_LastException.htm
[`ParseResult.RemainingArguments`]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_ParseResult_RemainingArguments.htm
[`ParseStatus.Canceled`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_ParseStatus.htm
[`ParseStatus.Error`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_ParseStatus.htm
[`ParseStatus.Success`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_ParseStatus.htm
[`UnknownArgument`]: https://www.ookii.org/docs/commandline-4.2/html/E_Ookii_CommandLine_CommandLineParser_UnknownArgument.htm
[`UsageWriter`]: https://www.ookii.org/docs/commandline-4.2/html/T_Ookii_CommandLine_UsageWriter.htm
[Arguments_0]: https://www.ookii.org/docs/commandline-4.2/html/P_Ookii_CommandLine_CommandLineParser_Arguments.htm
[CreateParser()_1]: https://www.ookii.org/docs/commandline-4.2/html/M_Ookii_CommandLine_IParserProvider_1_CreateParser.htm
[DuplicateArgument_0]: https://www.ookii.org/docs/commandline-4.2/html/E_Ookii_CommandLine_CommandLineParser_DuplicateArgument.htm
[Parse()_5]: https://www.ookii.org/docs/commandline-4.2/html/Overload_Ookii_CommandLine_CommandLineParser_1_Parse.htm
[Parse()_7]: https://www.ookii.org/docs/commandline-4.2/html/Overload_Ookii_CommandLine_IParser_1_Parse.htm
[Parse<T>()_1]: https://www.ookii.org/docs/commandline-4.2/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[ParseWithErrorHandling()_1]: https://www.ookii.org/docs/commandline-4.2/html/M_Ookii_CommandLine_CommandLineParser_1_ParseWithErrorHandling.htm
