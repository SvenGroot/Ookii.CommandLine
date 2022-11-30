# What’s new in Ookii.CommandLine

## Ookii.CommandLine 3.0

**IMPORTANT:** Several of the changes in version 3.0 are *breaking changes*. There are breaking API
changes as well as several behavior changes. In general, it's not expected that you'll need to make
many changes, unless you were using subcommands or extensively customized the usage help. Please see
the [information on migrating from Ookii.CommandLine 2.x](Migrating.md) if you are upgrading an
existing application.

- Argument parsing
  - Added support for a [new parsing mode](Arguments.md#longshort-mode) where arguments can have a
    separate long name using the `--` prefix (customizable, of course) and single-character short
    name using the `-` prefix. This allows you to use a parsing style that's similar to common tools,
    including `dotnet` itself.
  - Allow [automatic name transformation](DefiningArguments.md#name-transformation) of arguments,
    value descriptions, and [subcommand names](Subcommands.md#name-transformation) so you don't need
    to use custom names everywhere if you want a different convention for names than you're using
    for .Net identifiers.
  - Added support for [argument validation and dependencies](Validation.md).
  - Allow the use of [types](Arguments.md#argument-value-conversion) with `public static Parse()`
    methods or constructors taking a string parameter, even without a [`TypeConverter`][].
  - You can now customize parsing behavior by applying the [`ParseOptionsAttribute`][] to your class, as
    an alternative to passing [`ParseOptions`][] to the static [`Parse<T>()`][Parse<T>()_1] method.
  - [`ParseOptions`][] is now also used to set options when manually creating a [`CommandLineParser`][]
    instance.
  - Argument value conversion now defaults to using [`CultureInfo.InvariantCulture`][].
  - Added a [`Parse()`][Parse()_6] method overloads that takes arguments from [`Environment.GetCommandLineArgs()`][].
  - Added a generic [`CommandLineParser<T>`][] helper class, for easier usage when you don't want to use
    the static helper method.
  - You can use [static methods to define arguments](DefiningArguments.md#using-methods).
  - Automatically add `-Help` and `-Version` arguments if not defined.
  - Optionally show a warning when duplicate arguments are supplied.
  - Optional support for [multi-value arguments](Arguments.md#arguments-with-multiple-values) that
    consume multiple argument tokens without a separator, e.g. `-Value 1 2 3` to assign three
    values.
  - Arguments classes can [use a constructor parameter](DefiningArguments.md#commandlineparser-injection)
    to receive the [`CommandLineParser`][] instance they were created with.
  - Added the ability to customize error messages and other strings.
- Subcommands
  - Renamed "shell commands" to "subcommands" because I never liked the old name.
  - Completely reworked [subcommand](Subcommands.md) support, with a brand new, more powerful and
    easier to use API.
  - Support for asynchronous subcommands.
  - Support for subcommand aliases.
  - You can now use subcommands from multiple assemblies at once, and apply a filter.
  - Automatically add a `version` subcommand if one does not exist.
- Usage help
  - [Color output](UsageHelp.md#color-output) support.
  - Greatly expanded [usage help customization options](UsageHelp.md#customizing-the-usage-help)
    with the new [`UsageWriter`][UsageWriter_1] class, including abbreviated syntax, description
    list ordering and filtering, the ability to override any string or format, and more.
  - Arguments and subcommands can be hidden from the usage help.
  - Improved detection logic for the application executable name in the usage syntax.
  - Arguments that have no description but that have other information not shown in the usage syntax
    (like aliases, a default value, or validators) will be included in the description list by
    default.
  - Aliases and the default value are now shown in the usage help by default.
  - Changed the default format for how aliases are displayed.
  - With the static [`Parse<T>()`][Parse<T>()_1] method, you can choose to show no or partial usage
    help on error.
- Updated and improved documentation.
- More [samples](../src/Samples) with descriptions and explanations.
- Various bug fixes.
- No longer targets .Net Framework 2.0
  - Now targets .Net Standard 2.0, .Net Standard 2.1, and .Net 6.0 and later.

## Ookii.CommandLine 2.4

- Ookii.CommandLine now comes in a .Net 6.0 version that fully supports nullable reference types
  (.Net Framework 2.0 and .Net Standard 2.0 versions are also still provided).
- New static [`Parse<T>()`][Parse<T>()_1] helper methods that make parsing command line arguments and printing errors
  and usage even easier.
- Support for customization of the separator between argument names and values.
- Support for customization of the separator between keys and values for dictionary arguments.
- Support for customizing a dictionary argument's key and value [`TypeConverter`][] separately.
- Arguments can indicate they cancel parsing to make adding a `-Help` or `-?` argument easier.
- Some small bug fixes.

## Ookii.CommandLine 2.3

- Ookii.CommandLine now comes in both a .Net Framework 2.0 and .Net Standard 2.0 version.

## Ookii.CommandLine 2.2

- Added support for alternative names (aliases) for command line arguments.
- An argument’s aliases and default value can be included in the argument description when
  generating usage.
- Added code snippets.

## Ookii.CommandLine 2.1

- Added support for dictionary arguments; these are special multi-value arguments whose values take
  the form key=value.
- Multi-value arguments can be specified using a read-only property of any collection type (in
  addition to the previous array support).
- Multi-value properties can optionally use a separator character to allow multiple values to be
  specified without specifying the argument multiple times.
- Added support for specifying a custom type converter for individual arguments.
- When specifying the default value for an argument defined by a property you can now use any type
  that can be converted to the argument’s type using its type converter. This makes it possible to
  define default values for arguments with a type for which there are no literals.
- A CommandLineArgumentException is thrown when the argument type’s constructor or a property setter
  throws an exception (instead of a TargetInvocationException).
- The CommandLineParser no longer sets the property value for an unspecified argument with a default
  value of null.
- Shell commands can take their name from the type name.
- Shell commands can use custom argument parsing.
- Various minor bug fixes.

## Ookii.CommandLine 2.0

- Improved argument parsing:
  - All arguments can be specified by name.
  - Support for using whitespace to separate an argument name from its value.
  - Support for multiple argument name prefixes.
  - Support for using a custom StringComparer for argument name matching (to allow case sensitive or
    insensitive matching).
  - Support for use a custom CultureInfo for argument value conversion.
  - Non-positional arguments can be required arguments.
- Properties can be used to define positional arguments.
- More customizable generation of usage help text.
- The new shell commands functionality lets you easily create shell utilities with multiple
  operations that each uses its own command line arguments.
- The LineWrappingTextWriter class provides support for writing word-wrapped text to any output
  stream, with greater flexibility than the SplitLines method provided in Ookii.CommandLine 1.0.
- Targets .Net 2.0 for wider applicability.

## Upgrading from Ookii.CommandLine 1.0

Ookii.CommandLine 2.0 and newer versions have substantial changes from version 1.0 and are not
designed to be backwards compatible. There are changes in argument parsing behavior and API names
and usage.

Upgrading an existing project that is using Ookii.CommandLine 1.0 to Ookii.CommandLine 2.0 or newer
may require substantial code changes and may change how command lines are parsed.

[`CommandLineParser`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandLineParser<T>`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineParser_1.htm
[`CultureInfo.InvariantCulture`]: https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo.invariantculture
[`Environment.GetCommandLineArgs()`]: https://learn.microsoft.com/dotnet/api/system.environment.getcommandlineargs
[`ParseOptions`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`TypeConverter`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverter
[Parse()_6]: https://www.ookii.org/docs/commandline-3.0-preview/html/M_Ookii_CommandLine_CommandLineParser_Parse.htm
[Parse<T>()_1]: https://www.ookii.org/docs/commandline-3.0-preview/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[UsageWriter_1]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_UsageWriter.htm
