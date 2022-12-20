using Ookii.CommandLine;
using Ookii.CommandLine.Terminal;

namespace CustomUsage;

// A custom usage writer is used to change the format of the usage help.
internal class CustomUsageWriter : UsageWriter
{
    // Override some defaults to suit the custom format.
    public CustomUsageWriter()
    {
        // Only list the positional arguments in the syntax.
        UseAbbreviatedSyntax = true;

        // Set the indentation to work with the formatting used.
        ApplicationDescriptionIndent = 2;
        SyntaxIndent = 2;

        // Sort the description list by short name.
        ArgumentDescriptionListOrder = DescriptionListSortMode.AlphabeticalShortName;

        // Customize some of the colors.
        UsagePrefixColor = TextFormat.ForegroundYellow;
        ArgumentDescriptionColor = TextFormat.BoldBright;

        // No blank lines between arguments in the description list.
        BlankLineAfterDescription = false;
    }

    // Add a header before the description.
    protected override void WriteApplicationDescription(string description)
    {
        SetIndent(ApplicationDescriptionIndent);

        // You must *always* check whether color is enabled before using VT sequences. It may be
        // disabled, for example if the output is redirected or the terminal doesn't support VT
        // sequences. WriteColor does this check for you.
        WriteColor(UsagePrefixColor);
        Writer.Write("DESCRIPTION:");
        ResetColor();
        Writer.WriteLine();
        Writer.WriteLine(description);
        Writer.WriteLine();
    }

    // Use a custom usage prefix with the string "USAGE:" in all caps and on its own line.
    protected override void WriteUsageSyntaxPrefix()
    {
        WriteColor(UsagePrefixColor);
        Writer.Write("USAGE:");
        ResetColor();
        Writer.WriteLine();
        WriteColor(TextFormat.Underline);
        Writer.Write(ExecutableName);
        ResetColor();

        // This application does not use subcommands, but if you do, you have to include the command
        // name in this function too.
        if (CommandName != null)
        {
            Writer.Write(' ');
            Writer.Write(CommandName);
        }
    }

    // Add some color to the argument names in the syntax.
    //
    // This doesn't apply to the description list because it uses WriteArgumentNameForDescription.
    protected override void WriteArgumentName(string argumentName, string prefix)
    {
        // "bright black", aka "gray"...
        WriteColor(TextFormat.BrightForegroundBlack);
        base.WriteArgumentName(argumentName, prefix);
        ResetColor();
    }

    protected override void WriteArgumentDescriptions()
    {
        // Calculate the amount of indentation needed based on the longest names, with two spaces
        // before and after. This way the usage dynamically adapts if you change the argument
        // names.
        ArgumentDescriptionIndent = Parser.Arguments.Max(arg => CalculateNamesLength(arg)) + 4;
        base.WriteArgumentDescriptions();
    }

    // Add a header before the argument description list (normally there is none).
    protected override void WriteArgumentDescriptionListHeader()
    {
        WriteColor(UsagePrefixColor);
        Writer.Write("OPTIONS:");
        ResetColor();
        Writer.WriteLine();
    }

    // Custom format for argument names and aliases.
    protected override void WriteArgumentDescriptionHeader(CommandLineArgument argument)
    {
        // Collect all the argument's names and aliases, short names and aliases first.
        var names = Enumerable.Empty<string>();
        if (argument.HasShortName)
        {
            names = names.Append(argument.ShortNameWithPrefix!);
        }

        if (argument.ShortAliases != null)
        {
            var shortPrefix = argument.Parser.ArgumentNamePrefixes[0];
            names = names.Concat(argument.ShortAliases.Select(alias => shortPrefix + alias));
        }

        if (argument.HasLongName)
        {
            names = names.Append(argument.LongNameWithPrefix!);
        }

        if (argument.Aliases != null)
        {
            names = names.Concat(argument.Aliases.Select(alias => argument.Parser.LongArgumentNamePrefix + alias));
        }

        // Join up all the names.
        string name = string.Join('|', names);

        // Unlike the default description format, we just omit the value description entirely
        // if the argument is a switch.
        if (!argument.IsSwitch)
        {
            name += " <" + argument.ValueDescription + ">";
        }

        // WriteArgumentDescriptions adjusts the indentation when in long/short mode, which we don't
        // want here, so set it manually.
        SetIndent(ArgumentDescriptionIndent);
        Writer.ResetIndent();
        Writer.Write("  ");
        WriteColor(ArgumentDescriptionColor);
        Writer.Write(name);
        ResetColor();

        // Pad until the indentation is reached.
        WriteSpacing(ArgumentDescriptionIndent - name.Length - 2);
    }

    // Customize the format of the default values.
    protected override void WriteDefaultValue(object defaultValue)
        => Writer.Write($" [default: {defaultValue}]");

    // Calculate the length of the names and value prefix using the same logic as
    // WriteArgumentDescriptionHeader.
    private static int CalculateNamesLength(CommandLineArgument argument)
    {
        int length = 0;
        if (argument.HasShortName)
        {
            // +1 for separator
            length += argument.ShortNameWithPrefix!.Length + 1;
        }

        if (argument.ShortAliases != null)
        {
            var shortPrefixLength = argument.Parser.ArgumentNamePrefixes[0].Length;
            // Space for prefix, short name, separator.
            length += argument.ShortAliases.Count * (shortPrefixLength + 1 + 1);
        }

        if (argument.HasLongName)
        {
            // +1 for separator
            length += argument.LongNameWithPrefix!.Length + 1;
        }

        if (argument.Aliases != null)
        {
            var longPrefixLength = argument.Parser.LongArgumentNamePrefix!.Length;
            // Space for prefix, long name, separator.
            length += argument.Aliases.Sum(alias => longPrefixLength + alias.Length + 1);
        }

        // There is one separator too many
        length -= 1;

        // Length of value description.
        if (!argument.IsSwitch)
        {
            length += 3 + argument.ValueDescription.Length;
        }

        return length;
    }
}
