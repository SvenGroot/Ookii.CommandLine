using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Terminal;
using System.Threading.Tasks;

// For an application using subcommands, set the friendly name used for the automatic version
// command by using this attribute on the assembly rather than an arguments type.
[assembly: ApplicationFriendlyName("Ookii.CommandLine Subcommand Sample")]

namespace SubcommandSample;

static class Program
{
    // No need to use the Main(string[] args) overload (though you can if you want), because
    // CommandManager can take the arguments directly from Environment.GetCommandLineArgs().
    static async Task<int> Main()
    {
        // You can use the CommandOptions class to customize the parsing behavior and usage help
        // output. CommandOptions inherits from ParseOptions so it supports all the same options.
        var options = new CommandOptions()
        {
            // Set options so the command names are determined by the class name, transformed to
            // dash-case and with the "Command" suffix stripped.
            CommandNameTransform = NameTransform.DashCase,
            UsageWriter = new UsageWriter()
            {
                // Since all the commands have an automatic "-Help" argument, show the instruction
                // how to get help on a command.
                IncludeCommandHelpInstruction = true,
                // Show the application description before the command list.
                IncludeApplicationDescriptionBeforeCommandList = true,
            },
        };

        // Create a CommandManager for the commands in the current assembly.
        //
        // In addition to our commands, it will also have an automatic "version" command (this can
        // be disabled with the options).
        var manager = new CommandManager(options);

        // Run the command indicated in the first argument to this application, and use the return
        // value of its Run method as the application exit code. If the command could not be
        // created, we return an error code.
        //
        // We use the async version because our commands use the IAsyncCommand interface. Note that
        // you can use this method even if not all of your commands use IAsyncCommand.
        return await manager.RunCommandAsync() ?? (int)ExitCode.CreateCommandFailure;
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
}
