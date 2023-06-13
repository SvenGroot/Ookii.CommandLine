// Commands to test loading commands from an external assembly.
using Ookii.CommandLine.Commands;

namespace Ookii.CommandLine.Tests.Commands;

#pragma warning disable OCL0034 // Subcommands should have a description.

[Command("external")]
[GeneratedParser]
public partial class ExternalCommand : ICommand
{
    public int Run() => throw new NotImplementedException();
}

[Command]
public class OtherExternalCommand : ICommand
{
    public int Run() => throw new NotImplementedException();
}

[Command]
internal class InternalCommand : ICommand
{
    public int Run() => throw new NotImplementedException();
}

public class NotACommand : ICommand
{
    public int Run() => throw new NotImplementedException();
}
