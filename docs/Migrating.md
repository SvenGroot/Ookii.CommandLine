# Migrating from previous versions of Ookii.CommandLine

Ookii.CommandLine can introduce breaking changes when the major version number changes. This
document outlines all the breaking changes in each major version.

Minor version number or revision changes are guaranteed to not have any breaking changes from their
corresponding major version release.

Although there are quite a few changes, it's likely your application will not require many
modifications unless you used subcommands, heavily customized the usage help format, or used
custom argument value conversion.

## .Net Framework support

As of version 3.0, .Net Framework 2.0 is no longer supported. You can still target .Net Framework
4.6.1 and later using the .Net Standard 2.0 assembly. If you need to support an older version of
.Net, please continue to use [version 2.4](https://github.com/SvenGroot/ookii.commandline/releases/tag/v2.4).

## Ookii.CommandLine 5.0

Version 5.0 was used as a cleanup release, tidying up the API and removing some elements that were
left for compatibility. As a result, you may need to make some changes if you used certain features,
though the scope of the changes is likely to be small.

### Breaking API changes in version 5.0

- The .Net 8 SDK is now required to use [source generation](SourceGeneration.md). You can still
  target older runtimes, even when using source generation, as long as the project is built using
  the .Net 8 or a newer SDK.
- The [`IAsyncCommand.RunAsync()`][] and [`AsyncCommandBase.RunAsync()`][] methods now take a
  [`CancellationToken`][] as an argument. The `IAsyncCancelableCommand` interface has been removed.
- The [`ArgumentConverter`][] class now has a single [`Convert(ReadOnlyMemory<char>)`][Convert(ReadOnlyMemory<char>)_0] method. The
  `Convert(string)` and `Convert(ReadOnlySpan<char>)` overloads have been removed.
  - This was done because [`ReadOnlyMemory<char>.ToString()`][] does not allocate a new `string` if the
    value represents an entire existing `string` object. Since the [`CommandLineParser`][] always has
    a [`ReadOnlyMemory<char>`][] available for an argument value, there is no need for separate
    overloads as an optimization.
  - All built-in classes that derive from [`ArgumentConverter`][] have been updated accordingly.
- The [`ArgumentValidationAttribute`][] now has separate [`IsValidPreConversion()`][IsValidPreConversion()_0],
  [`IsValidPostConversion()`][IsValidPostConversion()_0] and [`IsValidPostParsing()`][IsValidPostParsing()_0] methods.
  - The `ArgumentValidationAttribute.Mode` property and the `ValidationMode` enumeration
    have been removed.
  - To upgrade an existing custom validator, remove the `Mode` property and replace the `IsValid()`
    method with the appropriate method corresponding to when you want to perform validation.
  - All built-in validators have been upgraded accordingly.
- The [`CancelMode.AbortWithHelp`][] value was added to indicate usage help should be shown when parsing
  is canceled.
  - The [`CommandLineArgumentAttribute.CancelParsing`][] attribute no longer shows help automatically
    when using [`CancelMode.Abort`][]; you must change them to [`CancelMode.AbortWithHelp`][].
  - The `CommandLineParser.HelpRequested` property was moved to [`ParseResult.HelpRequested`][], and
    can no longer be set by event handlers or method arguments. Change code that set this property
    to return [`CancelMode.AbortWithHelp`][] instead.
- Method arguments using a [`Boolean`][] return value are no longer allowed; only `void` and
  [`CancelMode`][] can be used as the return value. Change methods that returned a [`Boolean`][] to return
  [`CancelMode`][], and use [`CancelMode.None`][] for `false`, [`CancelMode.Abort`][] for `true`, and
  [`CancelMode.AbortWithHelp`][] if you returned `true` and also set the
  `CommandLineParser.HelpRequested` property.
- Several uses of `bool?` where the null value meant "automatic" have been replaced with a more
  explicit [`TriState`][] enumeration:
  - The [`UsageWriter`][] constructor.
  - The [`UsageWriter.IncludeCommandHelpInstruction`][] property.
  - The [`ParseOptions.UseErrorColor`][] property.
  - The [`ValidateEnumValueAttribute.AllowNonDefinedValues`][] and
    [`ValidateEnumValueAttribute.AllowCommaSeparatedValues`][] properties.
- The [`CommandAttribute`][] is no longer inherited by derived classes. Inheriting this attribute only
  made sense if no explicit name was specified (otherwise you would have two commands with the same
  name), and it is better to be explicit about which classes you wish to be commands. If you have
  a class that derives from a command that is itself also a command, make sure you apply the
  [`CommandAttribute`][] to the derived class.
- When the user uses a ambiguous prefix alias, an [`AmbiguousPrefixAliasException`][] is thrown (this
  exception derives from [`CommandLineArgumentException`][]), with the category set to
  [`CommandLineArgumentErrorCategory.AmbiguousPrefixAlias`][]. Previously, these errors used
  [`CommandLineArgumentErrorCategory.UnknownArgument`][].
- The `CommandInfo.MatchesPrefix()` method was replaced with the [`CommandInfo.MatchingPrefix()`][]
  method.
- The [`DuplicateArgumentEventArgs`][] class constructor takes a [`ReadOnlyMemory<char>`][], and other
  overloads have been removed.
- The signature of the [`ParentCommand.OnDuplicateArgumentWarning()`][] method has been changed.
- The signature of the [`ParentCommand.OnChildCommandNotFound()`][] method has been changed.
- The signature of the [`UsageWriter.WriteAliases()`][] method has changed.
- The type of the [`CommandLineArgument.Aliases`][] and [`CommandLineArgument.ShortAliases`][] property
  has been changed.
- The type of the [`CommandInfo.Aliases`][] property has been changed.
- Several function overloads have been replaced in favor of optional arguments.
- Some internal types and members that were only maintained for binary compatibility have been
  removed.

### Breaking behavior changes in version 5.0

- The default settings for the [`ValidateEnumValueAttribute`][] have changed. Numeric values are now
  not allowed by default, and comma-separated values and undefined values are only allowed by
  default if the [`FlagsAttribute`][] is present on the enumeration.
  - The [`EnumConverter`][] class now also applies these defaults if the argument has no
    [`ValidateEnumValueAttribute`][].
- The [`ArgumentValidationAttribute.IsValidPostParsing()`][] method is called even if the argument has
  no value. If you used a custom validator with `ValidationMode.AfterParsing`, be aware of this
  difference and check the [`CommandLineArgument.HasValue`][] property to determine if your validation
  logic should be applied.

## Ookii.CommandLine 4.0

### Breaking API changes in version 4.0

- It's strongly recommended to apply the [`GeneratedParserAttribute`][] to your arguments classes
  unless you cannot meet the requirements for [source generation](SourceGeneration.md).
- The `CommandLineArgumentAttribute.ValueDescription` property has been replaced by the
  [`ValueDescriptionAttribute`][] attribute. This new attribute is not sealed, enabling derived
  attributes e.g. to load a value description from a localized resource.
- Converting argument values from a string to their final type is no longer done using the
  [`TypeConverter`][] class, but instead using a custom [`ArgumentConverter`][] class. Custom
  converters must be specified using the [`ArgumentConverterAttribute`][] instead of the
  [`TypeConverterAttribute`][].
  - If you have existing conversions that depend on a [`TypeConverter`][], use the
    [`WrappedTypeConverter<T>`][] and [`WrappedDefaultTypeConverter<T>`][] as a convenient way to
    keep using that conversion.
  - The [`KeyValuePairConverter<TKey, TValue>`][] class has moved into the
    [`Ookii.CommandLine.Conversion`][] namespace.
  - The [`KeyValueSeparatorAttribute`][] has moved into the [`Ookii.CommandLine.Conversion`][]
    namespace.
  - The `KeyTypeConverterAttribute` and `ValueTypeConverterAttribute` were renamed to
    [`KeyConverterAttribute`][] and [`ValueConverterAttribute`][] respectively
- Constructor parameters can no longer be used to define command line arguments. Instead, all
  arguments must be defined using properties.
- `ParseOptions.ArgumentNameComparer` and `CommandOptions.CommandNameComparer` have been replaced by
  [`ArgumentNameComparison`][ArgumentNameComparison_1] and [`CommandNameComparison`][] respectively,
  both now taking a [`StringComparison`][] value instead of an [`IComparer<string>`][].
- Overloads of the [`CommandLineParser.Parse()`][CommandLineParser.Parse()_2], [`CommandLineParser.ParseWithErrorHandling()`][],
  [`CommandLineParser<T>.Parse()`][], [`CommandLineParser<T>.ParseWithErrorHandling()`][],
  [`CommandManager.CreateCommand()`][] and [`CommandManager.RunCommand()`][] methods that took an index have
  been replaced by overloads that take a [`ReadOnlyMemory<string>`][].
- The [`CommandInfo`][] type is now a class instead of a structure.
- The [`ICommandWithCustomParsing.Parse()`][] method signature has changed to use a
  [`ReadOnlyMemory<string>`][] structure for the arguments and to receive a reference to the calling
  [`CommandManager`][] instance.
- The [`CommandLineArgumentAttribute.CancelParsing`][] property now takes a [`CancelMode`][]
  enumeration rather than a boolean.
- The [`ArgumentParsedEventArgs`][] class was changed to use the [`CancelMode`][] enumeration.
- Canceling parsing using the [`ArgumentParsed`][] event no longer automatically sets the
  `HelpRequested` property; instead, you must set it manually in the event handler if desired.
- The `ParseOptionsAttribute.NameValueSeparator` property was replaced with
  [`ParseOptionsAttribute.NameValueSeparators`][].
- The `ParseOptions.NameValueSeparator` property was replaced with
  [`ParseOptions.NameValueSeparators`][].
- Properties that previously returned a [`ReadOnlyCollection<T>`][] now return an
  [`ImmutableArray<T>`][].
- The `CommandLineArgument.MultiValueSeparator` and `CommandLineArgument.AllowMultiValueWhiteSpaceSeparator`
  properties have been moved into the [`CommandLineArgument.MultiValueInfo`][] property.
- The `CommandLineArgument.AllowsDuplicateDictionaryKeys` and `CommandLineArgument.KeyValueSeparator`
  properties have been moved into the [`CommandLineArgument.DictionaryInfo`][] property.
- The `CommandLineArgument.IsDictionary` and `CommandLineArgument.IsMultiValue` properties have been
  removed; instead, check [`CommandLineArgument.DictionaryInfo`][] or [`CommandLineArgument.MultiValueInfo`][]
  for null values, or use the [`CommandLineArgument.Kind`][] property.
- [`TextFormat`][] is now a structure with strongly-typed values for VT sequences, and that structure is
  used by the [`UsageWriter`][] class for the various color formatting options.

### Breaking behavior changes in version 4.0

- By default, both `:` and `=` are accepted as argument name/value separators.
- The default value of [`ParseOptions.ShowUsageOnError`][] has changed to [`UsageHelpRequest.SyntaxOnly`][].
- [Automatic prefix aliases](DefiningArguments.md#automatic-prefix-aliases) are enabled by default
  for both argument names and [command names](Subcommands.md#command-aliases).
- The [`CommandManager`][], when using an assembly that is not the calling assembly, will only use
  public command classes, where before it would also use internal ones. This is to better respect
  access modifiers, and to make sure generated and reflection-based command managers behave the
  same.

## Ookii.CommandLine 3.0

### Breaking API changes in version 3.0

- It's strongly recommended to switch to the static [`CommandLineParser.Parse<T>()`][] method, if you
  were not already using it from version 2.4.
- If you do need to manually handle errors, be aware of the following changes:
  - If the instance [`CommandLineParser.Parse()`][CommandLineParser.Parse()_2] method returns null,
    you should only show usage help if the `CommandLineParser.HelpRequested` property is true.
  - Version 3.0 adds automatic "-Help" and "-Version" properties, which means the [`Parse()`][Parse()_6] method
    can return null even if it previously wouldn't.
  - Recommended: use the [`CommandLineParser<T>`][] class to get strongly typed instance [`Parse()`][Parse()_5]
    methods.
  - The [`CommandLineParser`][] constructor now takes a [`ParseOptions`][] instance.
  - Several properties of the [`CommandLineParser`][] class that could be used to change parsing behavior
    are now read-only and can only be changed through [`ParseOptions`][].
  - See [manual parsing and error handling](ParsingArguments.md#manual-parsing-and-error-handling)
    for an updated example.
- The `WriteUsageOptions` class has been replaced with [`UsageWriter`][].
- Usage options that previously were formatting strings now require creating a class that derives
  from [`UsageWriter`][] and overrides some of its methods. You have much more control over the
  formatting this way. See [customizing the usage help](UsageHelp.md#customizing-the-usage-help).
- The `CommandLineParser.WriteUsageToConsole()` method no longer exists; instead, the
  [`CommandLineParser.WriteUsage()`][] method will write to the console when invoked using a
  [`UsageWriter`][] with no explicitly set output.
- The subcommand (formerly called "shell command") API has had substantial changes.
  - Subcommand related functionality has moved into the [`Ookii.CommandLine.Commands`][] namespace.
  - The `ShellCommand` class as a base class for command classes has been replaced with the
    [`ICommand`][] interface.
  - The `ShellCommand.ExitCode` property has been replaced with the return value of the
    [`ICommand.Run()`][] method.
  - The `ShellCommand` class's static methods have been replaced with the [`CommandManager`][] class.
  - The `ShellCommandAttribute` class has been renamed to [`CommandAttribute`][].
  - The `ShellCommandAttribute.CustomParsing` property has been replaced with the
    [`ICommandWithCustomParsing`][] interface.
    - Commands with custom parsing no longer need a special constructor, and must implement the
      [`ICommandWithCustomParsing.Parse()`][] method instead.
  - The `CreateShellCommandOptions` class has been renamed to [`CommandOptions`][].
  - Usage related options have been moved into the [`UsageWriter`][] class.
  - Recommended: use [`IAsyncCommand`][] or [`AsyncCommandBase`][], along with
    [`CommandManager.RunCommandAsync()`][], if your commands need to run asynchronous code.
- A number of explicit method overloads have been removed in favor of optional parameters.
- The [`CommandLineArgument.ElementType`][] property now returns the underlying type for arguments
  using the [`Nullable<T>`][] type.

### Breaking behavior changes in version 3.0

- Argument type conversion now defaults to [`CultureInfo.InvariantCulture`][], instead of
  [`CurrentCulture`][]. This change was made to ensure a consistent parsing experience regardless of the
  user's regional settings. Only change it if you have a good reason.
- [`CommandLineParser`][] automatically adds `-Help` and `-Version` arguments by default. If you had
  arguments with these names, this will not affect you. The `-Version` argument is not added for
  subcommands.
- [`CommandManager`][] automatically adds a `version` command by default. If you had a command with
  this name, this will not affect you.
- The usage help now includes aliases and default values by default.
- The default format for showing aliases in the usage help has changed.
- Usage help uses color output by default (where supported).
- The [`LineWrappingTextWriter`][] class does not count virtual terminal sequences as part of the
  line length by default.

## Breaking changes in version 2.0

Ookii.CommandLine 2.0 and newer versions have substantial changes from version 1.0 and are not
designed to be backwards compatible. There are changes in argument parsing behavior and API names
and usage.

Upgrading an existing project that is using Ookii.CommandLine 1.0 to Ookii.CommandLine 2.0 or newer
may require substantial code changes and may change how command lines are parsed.

[`AmbiguousPrefixAliasException`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_AmbiguousPrefixAliasException.htm
[`ArgumentConverter`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_ArgumentConverter.htm
[`ArgumentConverterAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_ArgumentConverterAttribute.htm
[`ArgumentParsed`]: https://www.ookii.org/docs/commandline-5.0/html/E_Ookii_CommandLine_CommandLineParser_ArgumentParsed.htm
[`ArgumentParsedEventArgs`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_ArgumentParsedEventArgs.htm
[`ArgumentValidationAttribute.IsValidPostParsing()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Validation_ArgumentValidationAttribute_IsValidPostParsing.htm
[`ArgumentValidationAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Validation_ArgumentValidationAttribute.htm
[`AsyncCommandBase.RunAsync()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Commands_AsyncCommandBase_RunAsync.htm
[`AsyncCommandBase`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_AsyncCommandBase.htm
[`Boolean`]: https://learn.microsoft.com/dotnet/api/system.boolean
[`CancellationToken`]: https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken
[`CancelMode.Abort`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CancelMode.htm
[`CancelMode.AbortWithHelp`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CancelMode.htm
[`CancelMode.None`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CancelMode.htm
[`CancelMode`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CancelMode.htm
[`CommandAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_CommandAttribute.htm
[`CommandInfo.Aliases`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Commands_CommandInfo_Aliases.htm
[`CommandInfo.MatchingPrefix()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Commands_CommandInfo_MatchingPrefix.htm
[`CommandInfo`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_CommandInfo.htm
[`CommandLineArgument.Aliases`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgument_Aliases.htm
[`CommandLineArgument.DictionaryInfo`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgument_DictionaryInfo.htm
[`CommandLineArgument.ElementType`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgument_ElementType.htm
[`CommandLineArgument.HasValue`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgument_HasValue.htm
[`CommandLineArgument.Kind`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgument_Kind.htm
[`CommandLineArgument.MultiValueInfo`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgument_MultiValueInfo.htm
[`CommandLineArgument.ShortAliases`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgument_ShortAliases.htm
[`CommandLineArgumentAttribute.CancelParsing`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_CommandLineArgumentAttribute_CancelParsing.htm
[`CommandLineArgumentErrorCategory.AmbiguousPrefixAlias`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CommandLineArgumentErrorCategory.htm
[`CommandLineArgumentErrorCategory.UnknownArgument`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CommandLineArgumentErrorCategory.htm
[`CommandLineArgumentException`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CommandLineArgumentException.htm
[`CommandLineParser.Parse<T>()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_CommandLineParser_Parse__1.htm
[`CommandLineParser.ParseWithErrorHandling()`]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_CommandLineParser_ParseWithErrorHandling.htm
[`CommandLineParser.WriteUsage()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_CommandLineParser_WriteUsage.htm
[`CommandLineParser`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandLineParser<T>.Parse()`]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_CommandLineParser_1_Parse.htm
[`CommandLineParser<T>.ParseWithErrorHandling()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_CommandLineParser_1_ParseWithErrorHandling.htm
[`CommandLineParser<T>`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_CommandLineParser_1.htm
[`CommandManager.CreateCommand()`]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_Commands_CommandManager_CreateCommand.htm
[`CommandManager.RunCommand()`]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommand.htm
[`CommandManager.RunCommandAsync()`]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_Commands_CommandManager_RunCommandAsync.htm
[`CommandManager`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_CommandManager.htm
[`CommandNameComparison`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Commands_CommandOptions_CommandNameComparison.htm
[`CommandOptions`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_CommandOptions.htm
[`CultureInfo.InvariantCulture`]: https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo.invariantculture
[`CurrentCulture`]: https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo.currentculture
[`DuplicateArgumentEventArgs`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_DuplicateArgumentEventArgs.htm
[`EnumConverter`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_EnumConverter.htm
[`FlagsAttribute`]: https://learn.microsoft.com/dotnet/api/system.flagsattribute
[`GeneratedParserAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_GeneratedParserAttribute.htm
[`IAsyncCommand.RunAsync()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Commands_IAsyncCommand_RunAsync.htm
[`IAsyncCommand`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_IAsyncCommand.htm
[`ICommand.Run()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Commands_ICommand_Run.htm
[`ICommand`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_ICommand.htm
[`ICommandWithCustomParsing.Parse()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Commands_ICommandWithCustomParsing_Parse.htm
[`ICommandWithCustomParsing`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Commands_ICommandWithCustomParsing.htm
[`IComparer<string>`]: https://learn.microsoft.com/dotnet/api/system.collections.generic.icomparer-1
[`ImmutableArray<T>`]: https://learn.microsoft.com/dotnet/api/system.collections.immutable.immutablearray-1
[`KeyConverterAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_KeyConverterAttribute.htm
[`KeyValuePairConverter<TKey, TValue>`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_KeyValuePairConverter_2.htm
[`KeyValueSeparatorAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_KeyValueSeparatorAttribute.htm
[`LineWrappingTextWriter`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_LineWrappingTextWriter.htm
[`Nullable<T>`]: https://learn.microsoft.com/dotnet/api/system.nullable-1
[`Ookii.CommandLine.Commands`]: https://www.ookii.org/docs/commandline-5.0/html/N_Ookii_CommandLine_Commands.htm
[`Ookii.CommandLine.Conversion`]: https://www.ookii.org/docs/commandline-5.0/html/N_Ookii_CommandLine_Conversion.htm
[`ParentCommand.OnChildCommandNotFound()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Commands_ParentCommand_OnChildCommandNotFound.htm
[`ParentCommand.OnDuplicateArgumentWarning()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Commands_ParentCommand_OnDuplicateArgumentWarning.htm
[`ParseOptions.NameValueSeparators`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseOptions_NameValueSeparators.htm
[`ParseOptions.ShowUsageOnError`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseOptions_ShowUsageOnError.htm
[`ParseOptions.UseErrorColor`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseOptions_UseErrorColor.htm
[`ParseOptions`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_ParseOptions.htm
[`ParseOptionsAttribute.NameValueSeparators`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseOptionsAttribute_NameValueSeparators.htm
[`ParseResult.HelpRequested`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseResult_HelpRequested.htm
[`ReadOnlyCollection<T>`]: https://learn.microsoft.com/dotnet/api/system.collections.objectmodel.readonlycollection-1
[`ReadOnlyMemory<char>.ToString()`]: https://learn.microsoft.com/dotnet/api/system.readonlymemory-1.tostring
[`ReadOnlyMemory<char>`]: https://learn.microsoft.com/dotnet/api/system.readonlymemory-1
[`ReadOnlyMemory<string>`]: https://learn.microsoft.com/dotnet/api/system.readonlymemory-1
[`StringComparison`]: https://learn.microsoft.com/dotnet/api/system.stringcomparison
[`TextFormat`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Terminal_TextFormat.htm
[`TriState`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_TriState.htm
[`TypeConverter`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverter
[`TypeConverterAttribute`]: https://learn.microsoft.com/dotnet/api/system.componentmodel.typeconverterattribute
[`UsageHelpRequest.SyntaxOnly`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_UsageHelpRequest.htm
[`UsageWriter.IncludeCommandHelpInstruction`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_UsageWriter_IncludeCommandHelpInstruction.htm
[`UsageWriter.WriteAliases()`]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_UsageWriter_WriteAliases.htm
[`UsageWriter`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_UsageWriter.htm
[`ValidateEnumValueAttribute.AllowCommaSeparatedValues`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_AllowCommaSeparatedValues.htm
[`ValidateEnumValueAttribute.AllowNonDefinedValues`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Validation_ValidateEnumValueAttribute_AllowNonDefinedValues.htm
[`ValidateEnumValueAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Validation_ValidateEnumValueAttribute.htm
[`ValueConverterAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_ValueConverterAttribute.htm
[`ValueDescriptionAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_ValueDescriptionAttribute.htm
[`WrappedDefaultTypeConverter<T>`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_WrappedDefaultTypeConverter_1.htm
[`WrappedTypeConverter<T>`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Conversion_WrappedTypeConverter_1.htm
[ArgumentNameComparison_1]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_ParseOptions_ArgumentNameComparison.htm
[CommandLineParser.Parse()_2]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_CommandLineParser_Parse.htm
[Convert(ReadOnlyMemory<char>)_0]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Conversion_ArgumentConverter_Convert.htm
[IsValidPostConversion()_0]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Validation_ArgumentValidationAttribute_IsValidPostConversion.htm
[IsValidPostParsing()_0]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Validation_ArgumentValidationAttribute_IsValidPostParsing.htm
[IsValidPreConversion()_0]: https://www.ookii.org/docs/commandline-5.0/html/M_Ookii_CommandLine_Validation_ArgumentValidationAttribute_IsValidPreConversion.htm
[Parse()_5]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_CommandLineParser_1_Parse.htm
[Parse()_6]: https://www.ookii.org/docs/commandline-5.0/html/Overload_Ookii_CommandLine_CommandLineParser_Parse.htm
