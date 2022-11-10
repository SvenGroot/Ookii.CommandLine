using Ookii.CommandLine;
using Ookii.CommandLine.Commands;

namespace NestedCommands;

internal class CustomStringProvider : LocalizedStringProvider
{
    private readonly CommandInfo _command;

    public CustomStringProvider(CommandInfo command)
    {
        _command = command;
    }

    // Override this to return the command description instead of the application description.
    public override string CommandListApplicationDescription(string description, bool useColor)
        => _command.Description ?? string.Empty;

    // Override this to make it clear these ar nested commands.
    public override string AvailableCommandsHeader(bool useColor)
        => $"The '{_command.Name}' command has the following subcommands:";
}
