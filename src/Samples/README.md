# Samples

Ookii.CommandLine comes with several samples that demonstrate various aspects of its functionality.

- The [**parser sample**](Parser) demonstrates the basic functionality of defining, parsing and
  using arguments.
- The [**long/short mode sample**](LongShort) demonstrates the POSIX-like long/short parsing mode,
  where arguments can have both a long name with `--` and a short name with `-`.
- The [**custom usage sample**](CustomUsage) demonstrates the flexibility of Ookii.CommandLine's
  usage help generation, by customizing it to use completely different formatting.
- The [**argument dependencies sample**](ArgumentDependencies) demonstrates how you can have
  arguments that require or prohibit the presence of other arguments.
- The [**categories sample**](Categories) demonstrates how to group arguments into categies in the
  usage help.
- The [**WPF sample**](Wpf) shows how you can use Ookii.CommandLine with a GUI application.

There are three samples demonstrating how to use subcommands:

- The [**subcommand sample**](Subcommand) demonstrates how to create a simple application that has
  multiple subcommands.
- The [**nested commands sample**](NestedCommands) demonstrates how to create an application where
  commands can contain other commands. It also demonstrates how to create common arguments for
  multiple commands using a common base class.
- The [**top-level arguments sample**](TopLevelArguments) demonstrates how to use arguments that
  don't belong to any subcommand before the command name.
