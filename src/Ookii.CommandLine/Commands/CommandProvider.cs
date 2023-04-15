using Ookii.CommandLine.Support;
using System.Collections.Generic;

namespace Ookii.CommandLine.Commands;

public abstract class CommandProvider
{
    public virtual ProviderKind Kind => ProviderKind.Unknown;

    public abstract IEnumerable<CommandInfo> GetCommandsUnsorted(CommandManager manager);

    public abstract CommandInfo? GetCommand(string commandName, CommandManager manager);

    public abstract string? GetApplicationDescription();
}
