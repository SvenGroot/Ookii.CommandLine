using Ookii.CommandLine;
using Ookii.CommandLine.Terminal;

namespace CustomUsage;

internal class CustomUsageWriter : UsageWriter
{
    public CustomUsageWriter()
    {
        // Only list the positional arguments in the syntax.
        UseAbbreviatedSyntax = true;

        // Set the indentation to work with the format used by the CustomStringProvider.
        ApplicationDescriptionIndent = 2;
        SyntaxIndent = 2;
        ArgumentDescriptionIndent = 30;

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
        // sequences. WriteOptionalColor does this for you.
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
        // I'm not including CommandName here because this application doesn't have subcommands.
        ResetColor();
    }

    // Add some color to the argument names in the syntax.
    protected override void WriteArgumentName(string argumentName, string prefix)
    {
        // "bright black", aka "gray"...
        WriteColor(TextFormat.BrightForegroundBlack);
        base.WriteArgumentName(argumentName, prefix);
        ResetColor();
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
        Writer.Write($"{name,-28}");
        ResetColor();
    }

    // Customize the format of the default values.
    protected override void WriteDefaultValue(object defaultValue)
        => Writer.Write($" [default: {defaultValue}]");
}
