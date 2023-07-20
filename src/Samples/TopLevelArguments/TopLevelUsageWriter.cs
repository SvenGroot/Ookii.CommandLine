using Ookii.CommandLine;
using Ookii.CommandLine.Commands;

namespace TopLevelArguments;

// Custom UsageWriter used for the top-level arguments.
internal class TopLevelUsageWriter : UsageWriter
{
    private readonly CommandManager _manager;

    public TopLevelUsageWriter(CommandManager manager)
    {
        _manager = manager;
    }

    // Show the positional arguments last to indicate arguments after --command must be command
    // arguments.
    protected override IEnumerable<CommandLineArgument> GetArgumentsInUsageOrder()
        => Parser.Arguments
               .Where(a => a.Position == null)
               .Concat(Parser.Arguments.Where(a => a.Position != null));

    // Indicate command arguments can follow the --command argument.
    protected override void WriteUsageSyntaxSuffix()
    {
        Writer.Write(" [command arguments]");
    }

    // Write the command list at the end of the usage.
    protected override void WriteArgumentDescriptions()
    {
        base.WriteArgumentDescriptions();
        _manager.WriteUsage();
    }
}
