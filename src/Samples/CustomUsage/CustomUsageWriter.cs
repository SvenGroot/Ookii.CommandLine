using Ookii.CommandLine;
using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUsage;

internal class CustomUsageWriter : UsageWriter
{
    // Add a header before the description.
    protected override void WriteApplicationDescription(string description)
    {
        Writer.Indent = ShouldIndent ? ApplicationDescriptionIndent : 0;

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
    protected override void WriteDescriptionHeader()
    {
        WriteColor(UsagePrefixColor);
        Writer.Write("OPTIONS:");
        ResetColor();
        Writer.WriteLine();
    }

    // Custom format for argument descriptions.
    protected override void WriteArgumentDescription(CommandLineArgument argument)
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

        Writer.ResetIndent();
        Writer.Write("  ");
        WriteColor(ArgumentDescriptionColor);
        Writer.Write($"{name,-28}");
        ResetColor();

        // Write the actual description.
        if (argument.Description != null)
        {
            WriteArgumentDescription(argument.Description);
        }

        // Add the validators and default value if appropriate.
        if (IncludeValidatorsInDescription)
        {
            WriteArgumentValidators(argument);
        }

        if (IncludeDefaultValueInDescription && argument.DefaultValue != null)
        {
            WriteDefaultValue(argument.DefaultValue);
        }

        Writer.WriteLine();
    }

    // Customize the format of the default values.
    protected override void WriteDefaultValue(object defaultValue)
        => Writer.Write($" [default: {defaultValue}]");
}
