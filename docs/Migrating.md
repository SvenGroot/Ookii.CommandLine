# Migrating from Ookii.CommandLine 2.x

Ookii.CommandLine 3.0 has a number of breaking changes from version 2.4 and before. Here's what you
need to know to migrate your code to the new version.

Although there are quite a few changes, it's likely your application will not require many
modifications unless you used subcommands or heavily customized the usage help format.

## Breaking API changes

- It's strongly recommended to switch to the static `CommandLineParser.Parse<T>()` method, if you
  were not already using it from version 2.4.
- If you do need to manually handle errors, be aware of the following changes:
  - If the instance `CommandLineParser.Parse()` method returns null, you should only show usage help
    if the `CommandLineParser.HelpRequested` property is true.
  - Version 3.0 adds automatic "-Help" and "-Version" properties, which means the `Parse()` method
    can return null even if it previously wouldn't.
  - Recommended: use the `CommandLineParser<T>` class to get strongly typed instance `Parse()`
    methods.
  - The `CommandLineParser` constructor now takes a `ParseOptions` instance.
  - Several properties of the `CommandLineParser` class that could be used to change parsing behavior
    are now read-only and can only be changed through `ParseOptions`.
- The `WriteUsageOptions` class has been replaced with `UsageWriter`.
- Usage options that previously were formatting strings now require creating a class that derives
  from `UsageWriter` and overrides some of its methods (you have much more control over the
  formatting this way).
- The `CommandLineParser.WriteUsageToConsole()` method no longer exists; instead, the
  `CommandLineParser.WriteUsage()` method will write to the console when invoked with no arguments
  or a `UsageWriter` with no explicitly set output.
- The subcommand (formerly called "shell command") API has had substantial changes.
  - Subcommand related functionality has moved into the `Ookii.CommandLine.Commands` namespace.
  - The `ShellCommand` class as a base class for command classes has been replaced with the
    `ICommand` interface.
  - The `ShellCommand.ExitCode` property has been replaced with the return value of the
    `ICommand.Run()` method.
  - The `ShellCommand` class's static methods have been replaced with the `CommandManager` class.
  - The `ShellCommandAttribute` class has been renamed to `CommandAttribute`.
  - The `ShellCommandAttribute.CustomParsing` property has been replaced with the
    `ICommandWithCustomParsing` interface.
    - Commands with custom parsing no longer need a constructor, and must implement the
      `ICommandWithCustomParsing.Parse()` method instead.
  - The `CreateShellCommandOptions` class has been renamed to `CommandOptions`.
  - Usage related options have been moved into the `UsageWriter` class.
  - Recommended: use `IAsyncCommand` or `AsyncCommandBase`, along with
    `CommandManager.RunCommandAsync()`, if your commands need to run asynchronous code.
- A number of overloads have been removed in favor of optional parameters.
- The `CommandLineArgument.ElementType` property now returns the underlying type for arguments using
  the `Nullable<T>` type.

## Breaking behavior changes

- Argument type conversion now defaults to `CultureInfo.InvariantCulture`, instead of
  `CurrentCulture`. This change was made to ensure a consistent parsing experience regardless of the
  user's regional settings. Only change it if you have a good reason.
- `CommandLineParser` automatically adds "Help" and "Version" arguments by default. If you had
  arguments with these names, this will not affect you. The "Version" argument is not added for
  subcommands.
- `CommandManager` automatically adds a "version" command by default. If you had a command with
  this name, this will not affect you.
- The usage help now includes aliases and default values by default.
- The default format for showing aliases in the usage help has changed.
- Usage help uses color output by default (if supported).
- The `LineWrappingTextWriter` class does not count virtual terminal sequences as part of the line
  length by default.
