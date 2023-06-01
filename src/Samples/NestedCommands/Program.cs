using NestedCommands;
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;

var options = new CommandOptions()
{
    UsageWriter = new UsageWriter()
    {
        // Add the application description.
        IncludeApplicationDescriptionBeforeCommandList = true,
        // Commands with child commands don't technically have a -Help argument, but they'll
        // ignore it and print their child command list anyway, so let's show the message.
        IncludeCommandHelpInstruction = true,
    },
};

var manager = new GeneratedManager(options);
return await manager.RunCommandAsync() ?? (int)ExitCode.CreateCommandFailure;
