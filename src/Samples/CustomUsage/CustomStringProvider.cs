using Ookii.CommandLine;
using Ookii.CommandLine.Terminal;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUsage;

// The CustomStringProvider does the bulk of the work of customizing the usage format.
internal class CustomStringProvider : LocalizedStringProvider
{
    // By overriding this, the "--help" argument no longer uses "-?" as its short name.
    public override char AutomaticHelpShortName() => 'h';

    // Add a header before the description.
    public override string ApplicationDescription(string description, bool useColor)
    {
        // You must *always* check whether color is enabled before using VT sequences. It may be
        // disabled, for example if the output is redirected or the terminal doesn't support VT
        // sequences.
        var color = useColor ? TextFormat.ForegroundYellow : string.Empty;
        var colorEnd = useColor ? TextFormat.Default : string.Empty;

        return $"{color}DESCRIPTION:{colorEnd}\n{description}";
    }

    // Use a custom usage prefix with the string "USAGE:" in all caps and on its own line.
    public override string UsagePrefix(string executableName, string color, string colorReset)
    {
        // Add some additional formatting, but only if color was enabled.
        var executableColor = color.Length > 0
            ? TextFormat.Underline
            : string.Empty;

        return $"{color}USAGE:{colorReset}\n{executableColor}{executableName}{colorReset}";
    }

    // Add some color to the argument names in the syntax.
    public override string ArgumentName(string argumentName, string prefix, bool useColor)
    {
        var result = prefix + argumentName;

        if (useColor)
        {
            // "bright black", aka "gray"...
            result = TextFormat.BrightForegroundBlack + result + TextFormat.Default;
        }

        return result;
    }

    // Add a header before the argument description list (normally there is none).
    public override string? DescriptionHeader(bool useColor)
    {
        var result = "OPTIONS:";
        if (useColor)
        {
            // Match color with our custom UsagePrefixColor.
            result = TextFormat.ForegroundYellow + result + TextFormat.Default;
        }

        return result;
    }

    // Custom format for argument descriptions.
    public override string ArgumentDescription(CommandLineArgument argument, WriteUsageOptions options)
    {
        // Check whether we can use colors.
        bool useColor = options.UseColor ?? false;
        string colorStart = string.Empty;
        string colorEnd = string.Empty;
        if (useColor)
        {
            colorStart = options.ArgumentDescriptionColor;
            colorEnd = options.ColorReset;
        }

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
        var name = string.Join('|', names);

        // Unlike the default description format, we just omit the value description entirely
        // if the argument is a switch.
        if (!argument.IsSwitch)
        {
            name += " " + ValueDescriptionForDescription(argument.ValueDescription, useColor);
        }

        // Add the validators and default value if appropriate.
        var validators = options.IncludeValidatorsInDescription ? ValidatorDescriptions(argument) : string.Empty;
        string defaultValue = options.IncludeDefaultValueInDescription && argument.DefaultValue != null
            ? DefaultValue(argument.DefaultValue, useColor)
            : string.Empty;

        return $"  {colorStart}{name,-26}{colorEnd}  {argument.Description}{validators}{defaultValue}";
    }

    // Customize the format of the default values.
    public override string DefaultValue(object defaultValue, bool useColor)
        => $" [default: {defaultValue}]";

    // Customize the help for the ValidateRangeAttribute.
    public override string ValidateRangeUsageHelp(ValidateRangeAttribute attribute)
    {
        // Minimum and maximum are both optional (though one of them must be set), so take that
        // into account (even though this sample doesn't use that).
        if (attribute.Minimum == null)
        {
            return $"[max: {attribute.Maximum}";
        }
        else if (attribute.Maximum == null)
        {
            return $"[min: {attribute.Minimum}";
        }

        return $"[range: {attribute.Minimum}-{attribute.Maximum}]";
    }
}
