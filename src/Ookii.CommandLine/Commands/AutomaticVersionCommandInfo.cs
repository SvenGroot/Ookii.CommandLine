using System;
using System.Collections.Generic;
using System.Linq;

namespace Ookii.CommandLine.Commands;

internal class AutomaticVersionCommandInfo : CommandInfo
{
    public AutomaticVersionCommandInfo(CommandManager manager)
        : base(typeof(AutomaticVersionCommand), manager.Options.AutoVersionCommandName(), manager)
    {
    }

    public override string? Description => Manager.Options.StringProvider.AutomaticVersionCommandDescription();

    public override bool UseCustomArgumentParsing => false;

    public override IEnumerable<string> Aliases => Enumerable.Empty<string>();

    public override ICommandWithCustomParsing CreateInstanceWithCustomParsing()
        => throw new InvalidOperationException(Properties.Resources.NoCustomParsing);

    public override CommandLineParser CreateParser()
        => AutomaticVersionCommand.CreateParser(Manager.Options);
}
