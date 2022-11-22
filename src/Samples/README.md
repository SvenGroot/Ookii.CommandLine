# Samples

Ookii.CommandLine comes with several samples that demonstrate various aspects of its functionality.

- The [**Parser sample**](Parser) demonstrates the basic functionality of defining, parsing and
  using arguments.
- The [**Long/short mode sample**](LongShort) demonstrates the long/short parsing mode, where
  arguments can have both a long name with `--` and a short name with `-`. It also showcases a few
  other non-default options.
- The [**Custom usage sample**](CustomUsage) demonstrates the flexibility of Ookii.CommandLine's
  usage help generation, by customizing it to use completely different formatting.
- The [**Argument dependencies sample**](ArgumentDependencies) demonstrates how you can have
  arguments that require or prohibit the presence of other arguments.
- The [**WPF sample**](Wpf) shows how you can use Ookii.CommandLine with a GUI application.

There are two samples demonstrating how to use subcommands:

- The [**Subcommand sample**](SubCommand) demonstrates how to create a simple application that has
  multiple subcommands.
- The [**Nested commands sample**](NestedCommands) demonstrates how to create an application where commands can
  contain other commands. It also demonstrates how to create common arguments for multiple commands
  using a common base class.
