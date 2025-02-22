# What’s new in Ookii.CommandLine

## Ookii.CommandLine 5.0 (2025-02-22)

**IMPORTANT:** Version 5.0 contains breaking changes. If you are upgrading from an earlier version,
please check the [migration guide](Migrating.md).

- Command line parsing improvements:
  - Arguments using an enumeration type now only accept named values by default, and comma-separated
    values are only accepted if the enumeration has the [`FlagsAttribute`][]. These default can be
    changed using the [`ValidateEnumValueAttribute`][] attribute.
  - [Argument validators](Validation.md) can now define separate methods to validate the value
    before string conversion, after conversion, and after all arguments have been parsed. This
    allows a single validator to perform validation at each stage.
  - You can use the helper type [`NonSwitchBoolean`][] to create an argument that takes a boolean
    value, but is not a switch argument.
  - When an [automatic prefix alias](DefiningArguments.md#automatic-prefix-aliases) is used that is
    ambiguous between multiple arguments or commands, a message is shown that lists the arguments it
    could match.
  - An error message is now shown when the user provides an unknown command name, along with the
    usage help.
- Usage help improvements:
  - You can now group arguments into [categories](UsageHelp.md#argument-categories) in the usage
    help.
  - You can hide argument and command aliases from the usage help with the
    [`AliasAttribute.IsHidden`][] and [`ShortAliasAttribute.IsHidden`][] properties.
  - You can now apply the [`ValueDescriptionAttribute`][] to a type, as well as to an individual
    argument. This allows you to set a default description for a custom type you use for argument
    parsing.
  - Custom value descriptions can opt-in to having [name transformation](DefiningArguments.md#name-transformation)
    applied to the custom description, using the [`ValueDescriptionAttribute.ApplyTransform`][]
    property.
  - When an argument cancels parsing (e.g. using [`CommandLineArgumentAttribute.CancelParsing`][],
    or with the return value of a method argument), it must now ask for usage help to be shown using
    [`CancelMode.AbortWithHelp`][] instead of using the [`HelpRequested`][] property.
- Improved and simplified some APIs:
  - Custom argument converters now only have to implement a single method that takes a
    [`ReadOnlyMemory<char>`][], instead of separate `string` and [`ReadOnlySpan<char>`][] overloads.
  - The [`CommandAttribute`][] is no longer inherited by derived classes. Inheriting it didn't
    really make sense unless no command name was set.
  - Simplify the API for using a [`CancellationToken`][] with [`IAsyncCommand`][].
  - Some other small API cleanup.
  - See the [migration guide](Migrating.md) for details on all API changes.
- The .Net 8 SDK is now required for using source generation. As before, you can still target older
  .Net runtimes, as long as you build using .Net 8 or a newer SDK.
- Various bug fixes and minor improvements.
- The .Net 6 and .Net 7 assemblies have been removed, which may lead to some loss of features for
  those .Net versions. See the [migration guide](Migrating.md) for more details.
- The code snippets Visual Studio extension has been removed from the download. The existing
  snippets still work, but they will no longer be updated in the future.

## Ookii.CommandLine 4.2 (2024-09-12)

- The [helper methods](Utilities.md#virtual-terminal-support) in the [`VirtualTerminal`][] class now
  use the [`LineWrappingTextWriter`][] class to properly white-space wrap their output.
- Added the [`LineWrappingTextWriter.ForStandardStream()`][] method.
- When using the [`GeneratedParserAttribute`] on a class that has a base class that also uses the
  [`GeneratedParserAttribute`], the source generator will now emit the `new` keyword on generated
  methods as appropriate, to avoid compiler warnings about hidden base class members.
- The library is now fully compatible with native AOT (ahead-of-time) compilation, when
  [source generation](SourceGeneration.md) is used.
- Bug fixes.

## Ookii.CommandLine 4.1 (2024-01-26)

- Support for [using a `--` argument](Arguments.md#the----argument) to escape argument names for the
  remaining arguments, or to cancel parsing. This can be enabled using
  [`ParseOptions.PrefixTermination`][] or [`ParseOptionsAttribute.PrefixTermination`][].
- Ignore unknown arguments by using the new [`CommandLineParser.UnknownArgument`][] event.
- The [`ValidateEnumValueAttribute`][] has additional properties to control how
  [enumeration values are parsed](Arguments.md#enumeration-conversion):
  [`CaseSensitive`][CaseSensitive_1], [`AllowNumericValues`][], and [`AllowCommaSeparatedValues`][].
  - The [`EnumConverter`][] now also checks the
    [`ValidateEnumValueAttribute.IncludeValuesInErrorMessage`][] property, if the attribute is
    present on the argument, so that error messages from the converter and validator are consistent.
- Support for passing a cancellation token to the [`CommandManager.RunCommandAsync()`][] method.
  Tasks can access this token by implementing the `IAsyncCancelableCommand` interface. The
  [`AsyncCommandBase`][] class provides support as well.
- Usage help improvements:
  - Support for [custom default value formatting](UsageHelp.md#default-values), using
    [`CommandLineArgumentAttribute.DefaultValueFormat`][].
  - Add [`LineWrappingTextWriter.IndentAfterEmptyLine`][] and [`UsageWriter.IndentAfterEmptyLine`][]
    properties, which allow for proper formatting of [argument descriptions with blank lines](UsageHelp.md#descriptions-with-blank-lines)
    using the default usage help format.
  - [Add a footer](UsageHelp.md#usage-help-footer) to the usage help with the
    [`UsageFooterAttribute`][] attribute.
  - Some localizable text that could previously only be customized by deriving from the
    [`UsageWriter`][] class can now also be customized with the [`LocalizedStringProvider`][] class,
    so you only need to derive from [`LocalizedStringProvider`][] to customize all user-facing
    strings.
- Provide [helper methods](Utilities.md#virtual-terminal-support) in the [`VirtualTerminal`][] class
  for writing text with VT formatting to the standard output or error streams.
- Provide extension methods for [`StandardStream`][] in the [`StandardStreamExtensions`][] class.
- Emit a warning if a class isn't using the [`GeneratedParserAttribute`][] when it could, with an
  automatic code fix to easily apply it.

## Ookii.CommandLine 4.0.1 (2023-09-19)

- Fix an issue where arguments defined by methods could not have aliases.

## Ookii.CommandLine 4.0 (2023-07-20)

**IMPORTANT:** Version 4.0 contains breaking changes. If you are upgrading from version 2.x or 3.x,
please check the [migration guide](Migrating.md).

- Add support for [source generation](SourceGeneration.md).
  - Use the [`GeneratedParserAttribute`][] to determine command line arguments at compile time.
    - Get errors and warnings for many mistakes.
    - Automatically determine the order of positional arguments.
    - Use property initializers to set default values that are used in the usage help.
    - Allow your application to be trimmed.
    - Improved performance.
  - Use the [`GeneratedCommandManagerAttribute`][] to determine subcommands at compile time.
    - Allow an application with subcommands to be trimmed.
    - Improved performance.
  - Using source generation is recommended unless you are not able to meet the requirements.
- Constructor parameters can no longer be used to define command line arguments.
- Converting strings to argument types is now done using Ookii.CommandLine's own [`ArgumentConverter`][]
  class.
  - See the [migration guide](Migrating.md) for more information.
  - This enables conversion using [`ReadOnlySpan<char>`][] for better performance, makes it easier to
    implement new converters, provides better error messages for enumeration conversion, and enables
    the use of trimming (when source generation is used).
  - For .Net 7 and later, support value conversion using the [`ISpanParsable<TSelf>`][] and
    [`IParsable<TSelf>`][] interfaces.
- Automatically accept [any unique prefix](DefiningArguments.md#automatic-prefix-aliases) of an
  argument name as an alias.
- Use the `required` keyword in C# 11 and .Net 7.0 to create required arguments.
- Support for using properties with `init` accessors (only if they are `required`).
- Value descriptions are now specified using the [`ValueDescriptionAttribute`][] attribute. This
  attribute is not sealed to allow derived classes that implement localization.
- Conveniently set several related options to enable POSIX-like conventions using the
  [`ParseOptions.IsPosix`][], [`CommandOptions.IsPosix`][] or [`ParseOptionsAttribute.IsPosix`][] property.
- Support for multiple argument name/value separators, with the default now accepting both `:` and
  `=`.
- You can now [cancel parsing](DefiningArguments.md#arguments-that-cancel-parsing) and still return
  success.
- The remaining unparsed arguments, if parsing was canceled or encountered an error, are available
  through the [`CommandLineParser.ParseResult`][] property.
- Argument validators used before conversion can implement validation on [`ReadOnlySpan<char>`][] for
  better performance.
- Built-in support for [nested subcommands](Subcommands.md#nested-subcommands).
- The automatic version argument and command will use the [`AssemblyTitleAttribute`][] if the
  [`ApplicationFriendlyNameAttribute`][] was not used.
- By default, only usage syntax is shown if a parsing error occurs; the help argument must be used
  to get full help.
- Exclude the default value from the usage help on a per argument basis with the
  [`CommandLineArgumentAttribute.IncludeDefaultInUsageHelp`][] property.
- [Source link](https://github.com/dotnet/sourcelink) integration.
- Various bug fixes and minor improvements.

## Ookii.CommandLine 3.1.1 (2023-03-29)

- .Net Standard 2.0: use the System.Memory package to remove some downlevel-only code.
- There are no changes for the .Net Standard 2.1 and .Net 6.0 assemblies.

## Ookii.CommandLine 3.1 (2023-03-21)

- Added an instance [`CommandLineParser<T>.ParseWithErrorHandling()`][] method, which handles errors
  and displays usage help the same way as the static [`Parse<T>()`][Parse<T>()_1] method, but allows access to more
  information only available if you have an instance.
- Added the [`CommandLineParser.ParseResult`][] property, which provides information about errors or
  which argument canceled parsing, giving detailed failure information to the caller even if the
  [`ParseWithErrorHandling()`][ParseWithErrorHandling()_1] method was used.
- Also exposed the same information for subcommands through the [`CommandManager.ParseResult`][]
  property.
- [`LineWrappingTextWriter`][] improvements
  - Proper async support; the various [`WriteAsync()`][WriteAsync()_4] and [`WriteLineAsync()`][WriteLineAsync()_5] methods will now call
    down to the async methods of the base writer for the [`LineWrappingTextWriter`][], and a new method
    [`ResetIndentAsync()`][] is provided.
  - Add a [`Flush()`][Flush()_0] method overload that allows flushing a non-empty buffer without inserting an
    extra new line.
  - Control line wrapping behavior with the [`Wrapping`][] property: disable it, or disable forcibly
    breaking lines if no suitable white-space character is found.
  - If the base writer is a [`StringWriter`][], the [`LineWrappingTextWriter.ToString()`][] method now
    returns the text written to the writer, including text that hasn't been flushed to the base
    writer yet.
  - Some minor bug fixes.

## Ookii.CommandLine 3.0 (2022-12-01)

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
  - You can use [static methods to define arguments](DefiningArguments.md#using-static-methods).
  - Automatically add `-Help` and `-Version` arguments if not defined.
  - Optionally show a warning when duplicate arguments are supplied.
  - Optional support for [multi-value arguments](Arguments.md#arguments-with-multiple-values) that
    consume multiple argument tokens without a separator, e.g. `-Value 1 2 3` to assign three
    values.
  - Arguments classes can [use a constructor parameter](DefiningArguments.md) to receive the
    [`CommandLineParser`][] instance they were created with.
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

## Ookii.CommandLine 2.4 (2022-09-01)

- Ookii.CommandLine now comes in a .Net 6.0 version that fully supports nullable reference types
  (.Net Framework 2.0 and .Net Standard 2.0 versions are also still provided).
- New static [`Parse<T>()`][Parse<T>()_1] helper methods that make parsing command line arguments and printing errors
  and usage even easier.
- Support for customization of the separator between argument names and values.
- Support for customization of the separator between keys and values for dictionary arguments.
- Support for customizing a dictionary argument's key and value [`TypeConverter`][] separately.
- Arguments can indicate they cancel parsing to make adding a `-Help` or `-?` argument easier.
- Some small bug fixes.

## Ookii.CommandLine 2.3 (2019-09-05)

- Ookii.CommandLine now comes in both a .Net Framework 2.0 and .Net Standard 2.0 version.

## Ookii.CommandLine 2.2 (2013-02-06)

- Added support for alternative names (aliases) for command line arguments.
- An argument’s aliases and default value can be included in the argument description when
  generating usage.
- Added code snippets.

## Ookii.CommandLine 2.1 (2012-02-19)

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

## Ookii.CommandLine 2.0 (2011-08-13)

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

[`AliasAttribute.IsHidden`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_AliasAttribute_IsHidden.htm
[`AllowCommaSeparatedValues`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_AllowCommaSeparatedValues.htm
[`AllowNumericValues`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_AllowNumericValues.htm
[`ApplicationFriendlyNameAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_ApplicationFriendlyNameAttribute.htm
[`ArgumentConverter`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_ArgumentConverter.htm
[`AssemblyTitleAttribute`]: https://learn.microsoft.com/dotnet/api/system.reflection.assemblytitleattribute
[`AsyncCommandBase`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_AsyncCommandBase.htm
[`CancellationToken`]: https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken
[`CancelMode.AbortWithHelp`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CancelMode.htm
[`CommandAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_CommandAttribute.htm
[`CommandLineArgumentAttribute.CancelParsing`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[`CommandLineArgumentAttribute.DefaultValueFormat`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_DefaultValueFormat.htm
[`CommandLineArgumentAttribute.IncludeDefaultInUsageHelp`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_IncludeDefaultInUsageHelp.htm
[`CommandLineParser.ParseResult`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineParser_ParseResult.htm
[`CommandLineParser.UnknownArgument`]: https://www.ookii.org/docs/commandline-5.0/html/E_Ookii_CommandLine_CommandLineParser_UnknownArgument.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandLineParser<T>.ParseWithErrorHandling()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_CommandLineParser_1_ParseWithErrorHandling.htm
[`CommandLineParser<T>`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CommandLineParser_1.htm
[`CommandManager.ParseResult`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Commands_CommandManager_ParseResult.htm
[`CommandManager.RunCommandAsync()`]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommandAsync.htm
[`CommandOptions.IsPosix`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Commands_CommandOptions_IsPosix.htm
[`CultureInfo.InvariantCulture`]: https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo.invariantculture
[`EnumConverter`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_EnumConverter.htm
[`Environment.GetCommandLineArgs()`]: https://learn.microsoft.com/dotnet/api/system.environment.getcommandlineargs
[`FlagsAttribute`]: https://learn.microsoft.com/dotnet/api/system.flagsattribute
[`GeneratedCommandManagerAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_GeneratedCommandManagerAttribute.htm
[`GeneratedParserAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_GeneratedParserAttribute.htm
[`HelpRequested`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseResult_HelpRequested.htm
[`IAsyncCommand`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_IAsyncCommand.htm
[`IParsable<TSelf>`]: https://learn.microsoft.com/dotnet/api/system.iparsable-1
[`ISpanParsable<TSelf>`]: https://learn.microsoft.com/dotnet/api/system.ispanparsable-1
[`LineWrappingTextWriter.ForStandardStream()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_LineWrappingTextWriter_ForStandardStream.htm
[`LineWrappingTextWriter.IndentAfterEmptyLine`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_LineWrappingTextWriter_IndentAfterEmptyLine.htm
[`LineWrappingTextWriter.ToString()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_LineWrappingTextWriter_ToString.htm
[`LineWrappingTextWriter`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_LineWrappingTextWriter.htm
[`LocalizedStringProvider`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_LocalizedStringProvider.htm
[`NonSwitchBoolean`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_NonSwitchBoolean.htm
[`ParseOptions.IsPosix`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseOptions_IsPosix.htm
[`ParseOptions.PrefixTermination`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseOptions_PrefixTermination.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute.IsPosix`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseOptionsAttribute_IsPosix.htm
[`ParseOptionsAttribute.PrefixTermination`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseOptionsAttribute_PrefixTermination.htm
[`ParseOptionsAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_ParseOptionsAttribute.htm
[`ReadOnlyMemory<char>`]: https://learn.microsoft.com/dotnet/api/system.readonlymemory-1
[`ReadOnlySpan<char>`]: https://learn.microsoft.com/dotnet/api/system.readonlyspan-1
[`ResetIndentAsync()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_LineWrappingTextWriter_ResetIndentAsync.htm
[`ShortAliasAttribute.IsHidden`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ShortAliasAttribute_IsHidden.htm
[`StandardStream`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Terminal_StandardStream.htm
[`StandardStreamExtensions`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Terminal_StandardStreamExtensions.htm
[`StringWriter`]: https://learn.microsoft.com/dotnet/api/system.io.stringwriter
[`TypeConverter`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverter
[`UsageFooterAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_UsageFooterAttribute.htm
[`UsageWriter.IndentAfterEmptyLine`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_UsageWriter_IndentAfterEmptyLine.htm
[`UsageWriter`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_UsageWriter.htm
[`ValidateEnumValueAttribute.IncludeValuesInErrorMessage`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_IncludeValuesInErrorMessage.htm
[`ValidateEnumValueAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Validation_ValidateEnumValueAttribute.htm
[`ValueDescriptionAttribute.ApplyTransform`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ValueDescriptionAttribute_ApplyTransform.htm
[`ValueDescriptionAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_ValueDescriptionAttribute.htm
[`VirtualTerminal`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Terminal_VirtualTerminal.htm
[`Wrapping`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_LineWrappingTextWriter_Wrapping.htm
[CaseSensitive_1]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_CaseSensitive.htm
[Flush()_0]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_LineWrappingTextWriter_Flush_1.htm
[Parse()_6]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_CommandLineParser_Parse.htm
[Parse<T>()_1]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[ParseWithErrorHandling()_1]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_CommandLineParser_1_ParseWithErrorHandling.htm
[UsageWriter_1]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_UsageWriter.htm
[WriteAsync()_4]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_LineWrappingTextWriter_WriteAsync.htm
[WriteLineAsync()_5]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_LineWrappingTextWriter_WriteLineAsync.htm
