using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Terminal;

[assembly: ApplicationFriendlyName("Ookii.CommandLine Top-level Arguments Sample")]

namespace TopLevelArguments;

static class Program
{
    static async Task<int> Main()
    {
        // Modified usage format for the command list and commands to account for global arguments.
        var commandUsageWriter = new CommandUsageWriter();

        // You can use the CommandOptions class to customize the parsing behavior and usage help
        // output. CommandOptions inherits from ParseOptions so it supports all the same options.
        var commandOptions = new CommandOptions()
        {
            IsPosix = true,
            // The top-level arguments will have a -Version argument, so no need for a version
            // command.
            AutoVersionCommand = false,
            UsageWriter = commandUsageWriter,
        };

        var manager = new GeneratedManager(commandOptions);

        // Use different options for the top-level arguments.
        var parseOptions = new ParseOptions()
        {
            IsPosix = true,
            // Modified usage format to list commands as well as top-level usage.
            UsageWriter = new TopLevelUsageWriter(manager)
        };

        // First parse the top-level arguments.
        var parser = TopLevelArguments.CreateParser(parseOptions);
        Arguments = parser.ParseWithErrorHandling();
        if (Arguments == null)
        {
            return (int)ExitCode.CreateCommandFailure;
        }

        // Run the command indicated in the top-level --command argument, and pass along the
        // arguments that weren't consumed by the top-level CommandLineParser.
        commandUsageWriter.IncludeCommandUsageSyntax = true;
        return await manager.RunCommandAsync(Arguments.Command, parser.ParseResult.RemainingArguments)
            ?? (int)ExitCode.CreateCommandFailure;
    }

    // Utility method used by the commands to write exception messages to the console.
    public static void WriteErrorMessage(string message)
    {
        using var support = VirtualTerminal.EnableColor(StandardStream.Error);
        using var writer = LineWrappingTextWriter.ForConsoleError();

        // Add some color if we can.
        if (support.IsSupported)
        {
            writer.Write(TextFormat.ForegroundRed);
        }

        writer.WriteLine(message);
        if (support.IsSupported)
        {
            writer.Write(TextFormat.Default);
        }
    }

    // Provides access to the top-level arguments for use by the commands.
    public static TopLevelArguments? Arguments { get; private set; }
}
