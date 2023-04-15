using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands;

internal class BasicCommandInfo : CommandInfo
{
    private readonly string _description;

    public BasicCommandInfo(Type commandType, string name, string description, CommandManager manager)
        : base(commandType, name, manager)
    {
        _description = description;
    }

    public override string? Description => _description;

    public override bool UseCustomArgumentParsing => false;

    public override IEnumerable<string> Aliases => Enumerable.Empty<string>();

    public override ICommandWithCustomParsing CreateInstanceWithCustomParsing()
        => throw new InvalidOperationException(Properties.Resources.NoCustomParsing);
}
