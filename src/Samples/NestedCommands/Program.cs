using NestedCommands;
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;

var options = new CommandOptions()
{
    UsageWriter = new UsageWriter()
    {
        // Add the application description.
        IncludeApplicationDescriptionBeforeCommandList = true,
        // The commands that derive from ParentCommand use ICommandWithCustomParsing, and don't
        // technically have a -Help argument. This prevents the instruction from being shown by
        // default. However, these commands will ignore -Help ignore it and print their child
        // command list anyway, so force the message to be shown.
        IncludeCommandHelpInstruction = true,
    },
};

var manager = new GeneratedManager(options);
return await manager.RunCommandAsync() ?? (int)ExitCode.CreateCommandFailure;
