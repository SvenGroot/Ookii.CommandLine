using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using SubcommandSample;

// For an application using subcommands, set the friendly name used for the automatic version
// command by using this attribute on the assembly rather than an arguments type.
// You can also use the <AssemblyTitle> property in the .csproj file.
[assembly: ApplicationFriendlyName("Ookii.CommandLine Subcommand Sample")]

// You can use the CommandOptions class to customize the parsing behavior and usage help
// output. CommandOptions inherits from ParseOptions so it supports all the same options.
var options = new CommandOptions()
{
    // Set options so the command names are determined by the class name, transformed to
    // dash-case and with the "Command" suffix stripped.
    CommandNameTransform = NameTransform.DashCase,
    UsageWriter = new UsageWriter()
    {
        // Show the application description before the command list.
        IncludeApplicationDescriptionBeforeCommandList = true,
    },
};

// Create a CommandManager for the commands in the current assembly. We use the manager we
// defined to use source generation, which allows trimming even when using commands.
//
// In addition to our commands, it will also have an automatic "version" command (this can
// be disabled with the options).
var manager = new GeneratedManager(options);

// Run the command indicated in the first argument to this application, and use the return
// value of its Run method as the application exit code. If the command could not be
// created, we return an error code.
//
// We use the async version because our commands use the IAsyncCommand interface. Note that
// you can use this method even if not all of your commands use IAsyncCommand.
return await manager.RunCommandAsync() ?? (int)ExitCode.CreateCommandFailure;
