using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using System.Diagnostics.CodeAnalysis;

namespace NestedCommands;

internal class CustomUsageWriter : UsageWriter
{
    private readonly CommandInfo _command;

    public CustomUsageWriter(CommandInfo command)
    {
        _command = command;
    }

    // Override this to add the command name.
    [AllowNull]
    public override string ExecutableName
    {
        get => base.ExecutableName + " " + _command.Name;
        set => base.ExecutableName = value;
    }

    // Override this to return the command description instead of the application description.
    protected override void WriteApplicationDescription(string description)
    {
        if (OperationInProgress != Operation.CommandListUsage)
        {
            base.WriteApplicationDescription(description);
            return;
        }

        Writer.Indent = ShouldIndent ? ApplicationDescriptionIndent : 0;
        Writer.WriteLine(_command.Description);
        Writer.WriteLine();
    }

    // Override this to make it clear these are nested commands.
    protected override void WriteAvailableCommandsHeader()
    {
        Writer.WriteLine($"The '{_command.Name}' command has the following subcommands:");
        Writer.WriteLine();
    }
}
