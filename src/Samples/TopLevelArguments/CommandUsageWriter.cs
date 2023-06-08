using Ookii.CommandLine;

namespace TopLevelArguments;

// Custom usage writer used for commands.
class CommandUsageWriter : UsageWriter
{
    // This lets us exclude the command usage syntax when appending command usage with the
    // TopLevelUsageWriter, and include it when we're running a command and may need to show usage
    // for that.
    public bool IncludeCommandUsageSyntax { get; set; }

    public CommandUsageWriter()
    {
        IncludeCommandHelpInstruction = true;
    }

    // Indicate there are global arguments in the command usage syntax.
    protected override void WriteUsageSyntaxPrefix()
    {
        WriteColor(UsagePrefixColor);
        Write("Usage: ");
        ResetColor();
        Write(' ');
        Write(ExecutableName);
        Writer.Write(" [global arguments]");
        if (CommandName != null)
        {
            Write(' ');
            Write(CommandName);
        }
    }

    // Omit the usage syntax when writing the command list after the top-level usage help.
    protected override void WriteCommandListUsageSyntax()
    {
        if (IncludeCommandUsageSyntax)
        {
            base.WriteCommandListUsageSyntax();
        }
    }

    // Also include the global arguments in the help instruction.
    protected override void WriteCommandHelpInstruction(string name, string argumentNamePrefix, string argumentName)
        => base.WriteCommandHelpInstruction(name + " [global arguments]", argumentNamePrefix, argumentName);
}
