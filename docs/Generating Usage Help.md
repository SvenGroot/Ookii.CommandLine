# Generating usage help

When you have an application that accepts command line arguments, it’s often useful to be able to provide the user with information about how to invoke the application, including an overview of all the arguments.

Creating this kind of usage help text is tedious, and you must make sure it is kept up to date whenever you change the arguments to your application. For this reason, Ookii.CommandLine can generate this usage help text automatically. The generated usage help can be customized by adding descriptions to the arguments.

Usage help can be generated using the `CommandLineParser.WriteUsage` method. The output can be customized using the `WriteUsageOptions` class. The `CommandLineParser.WriteUsageToConsole` method provides a convenient way to write the usage help to the standard output stream, properly word-wrapping the text at the console width.

The following example shows the usage help generated for the sample application included with the Ookii.CommandLine library:

```
Sample command line application. The application parses the command
line and prints the results, but otherwise does nothing, and none
of the arguments are actually used for anything.

Usage: CommandLineSampleCS.exe [-source] <String>
   [-destination] <String> [[-index] <Number>] [[-id] <String>] [-?]
   [-Count <Number>] [-Date <DateTime>] [-v] [-val <String>...]

    -source <String>
        The source data.

    -destination <String>
        The destination data.

    -index <Number>
        The operation's index. This argument is optional, and the
        default value is 1.

    -id <String>
        Sets the operation ID. The default value is "default".

    -? [<Boolean>]
        Displays this help message.

    -Count <Number>
        Provides the count for something to the application. This
        argument is required.

    -Date <DateTime>
        Provides a date to the application; the format to use
        depends on your regional settings.

    -v [<Boolean>]
        Print verbose information; this is an example of a switch
        argument.

    -val <String>
        This is an example of an array argument, which can be
        repeated multiple times to set more than one value.
```

The usage help consists of three components: the application description, the argument syntax, and the argument descriptions.

## Application description

The first part of the usage help is a description of your application. This is a short description that explains what your application does and how it can be used. It can be any text you like, though it’s recommended to keep it short.

The description is specified by specifying the `System.ComponentModel.DescriptionAttribute` to the class that defines the command line arguments, as in the following example:

{code: C#}
[Description("This is the application description that is included in the usage help.")]
class MyArguments
{
}
{code: C#}

If this attribute is not specified, no description is included in the usage help. The description can also be omitted by setting the `WriteUsageOptions.IncludeApplicationDescription` property to false.

## Argument syntax

The argument syntax indicates how your application can be invoked from the command line. The argument syntax typically starts with the name of your application, and is followed by all the arguments, indicating their name and type. There is an indication of which arguments are required or optional, and whether they allow multiple values. For positional arguments, the order is indicated as well.

The syntax for a single argument has the following format:

    -ArgumentName <ValueDescription>

For optional arguments, the name and value description are enclosed by square brackets. For a positional argument, the name is enclosed by square brackets to indicate the name itself is optional. For an array argument, the value description is followed by three periods.

The value description of an argument is short description (typically one word) that describes what kind of value the argument expects. It default to the type of the argument (for array arguments, the element type is used; for nullable types, the underlying type is used).

The value description can be specified explicitly. For example, you may want to set the value description of a numeric argument to “Number” rather than “Int32”. For arguments defined using constructor parameters, use the `ValueDescriptionAttribute` attribute. For arguments defined by a property, use the `CommandLineArgumentAttribute.ValueDescription` property.

The value description is omitted for switch arguments.

The exact format of the argument syntax can be customized using the `WriteUsageOptions` class. You can specify the usage prefix, and various format strings that control how optional arguments, value descriptions, and multi-value arguments are displayed.

## Argument descriptions

The final part of the usage help is a description for all the arguments. A list is written to the output of all arguments, followed by their description.

The description of an argument can be specified using the `System.ComponentModel.DescriptionAttribute` attribute. Apply this attribute to the constructor parameter or property defining the argument.

The exact format of the argument descriptions can be customized using the `WriteUsageOptions` class. You can specify a format string that controls how the argument name and description are laid out, and the amount of indentation to use for additional lines of the description.

By default, the default value and aliases of an argument are not included in the argument syntax or description. If you wish to advertise these to the user, you can add them in the argument’s description itself, or set the `WriteUsageOptions.IncludeDefaultValueInDescription` and `WriteUsageOptions.IncludeAliasInDescription` properties to true.
