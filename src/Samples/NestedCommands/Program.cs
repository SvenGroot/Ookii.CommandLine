using Ookii.CommandLine;
using Ookii.CommandLine.Commands;

namespace NestedCommands;

internal static class Program
{
    static async Task<int> Main()
    {
        var options = new CommandOptions()
        {
            // For the top-level, we only want commands that don't have the ParentCommandAttribute.
            CommandFilter = (command) => !Attribute.IsDefined(command.CommandType, typeof(ParentCommandAttribute)),
            UsageWriter = new UsageWriter()
            {
                // Add the application description.
                IncludeApplicationDescriptionBeforeCommandList = true,
                // Commands with child commands don't technically have a -Help argument, but they'll
                // ignore it and print their child command list anyway, so let's show the message.
                IncludeCommandHelpInstruction = true,
            },
        };

        var manager = new CommandManager(options);
        return await manager.RunCommandAsync() ?? (int)ExitCode.CreateCommandFailure;
    }
}
