using Ookii.CommandLine;
using Ookii.CommandLine.Validation;

namespace CustomUsage;

// The CustomStringProvider does the bulk of the work of customizing the usage format.
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
