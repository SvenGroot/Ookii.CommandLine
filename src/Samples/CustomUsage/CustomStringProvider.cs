using Ookii.CommandLine;
using Ookii.CommandLine.Validation;

namespace CustomUsage;

// A custom string provider can be used to customize many of the strings used by Ookii.CommandLine,
// including error messages and automatic argument names. Here, it is used to customize the help
// argument short name and the usage help for the ValidateRangeAttribute.
internal class CustomStringProvider : LocalizedStringProvider
{
    // By overriding this, the "--help" argument no longer uses "-?" as its short name.
    public override char AutomaticHelpShortName() => 'h';

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
