namespace Ookii.CommandLine;

enum StringSegmentType
{
    Text,
    LineBreak,
    Formatting,
    PartialLineBreak,
    // Must be the last group of values in the enum
    PartialFormattingUnknown,
    PartialFormattingSimple,
    PartialFormattingCsi,
    PartialFormattingOsc,
    PartialFormattingOscWithEscape,
}
