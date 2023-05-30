using Ookii.CommandLine.Commands;
using System.Reflection;

namespace NestedCommands;

// This is the base class for all the commands that have child commands. It performs all
// the work necessary to find and run subcommands, so derived classes don't have to do anything
// except add the [Command] attribute.
//
// This class uses ICommandWithCustomParsing, so CommandLineParser won't be used to create it.
// Instead, CommandManager will just instantiate it and call the Parse method, where we can
// do whatever we want. In this case, we create another CommandManager to find and create a
// child command.
//
// Although this sample doesn't do this, you can use this to nest commands more than one
// level deep just as easily.
internal abstract class ParentCommand : AsyncCommandBase, ICommandWithCustomParsing
{
    private IAsyncCommand? _childCommand;

    public void Parse(ReadOnlyMemory<string> args, CommandManager manager)
    {
        // Nested commands don't need to have a "version" command.
        manager.Options.AutoVersionCommand = false;

        // Select only the commands that have a ParentCommandAttribute specifying this command
        // as their parent.
        manager.Options.CommandFilter =
            (command) => command.CommandType.GetCustomAttribute<ParentCommandAttribute>()?.ParentCommand == GetType();

        var info = CommandInfo.Create(GetType(), manager);

        // Use a custom UsageWriter to replace the application description with the
        // description of this command.
        manager.Options.UsageWriter = new CustomUsageWriter(info)
        {
            // Apply the same options as the parent command.
            IncludeApplicationDescriptionBeforeCommandList = true,
            IncludeCommandHelpInstruction = true,
        };

        // All commands in this sample are async, so this cast is safe.
        _childCommand = (IAsyncCommand?)manager.CreateCommand(args);
    }

    public override async Task<int> RunAsync()
    {
        // If the child command had a parsing error, it won't have been created.
        if (_childCommand == null)
        {
            return (int)ExitCode.CreateCommandFailure;
        }

        // Otherwise, we can run the command.
        return await _childCommand.RunAsync();
    }
}
