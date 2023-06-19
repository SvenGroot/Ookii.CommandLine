namespace Ookii.CommandLine;

/// <summary>
/// Indicates how the arguments in the description list should be sorted.
/// </summary>
/// <seealso cref="UsageWriter.ArgumentDescriptionListOrder"/>
public enum DescriptionListSortMode
{
    /// <summary>
    /// The descriptions are listed in the same order as the usage syntax: first the positional
    /// arguments, then the required named arguments sorted by name, then the remaining
    /// arguments sorted by name.
    /// </summary>
    UsageOrder,
    /// <summary>
    /// The descriptions are listed in alphabetical order by argument name. If the parsing mode
    /// is <see cref="ParsingMode.LongShort"/>, this uses the long name of the argument, unless
    /// the argument has no long name, in which case the short name is used.
    /// </summary>
    Alphabetical,
    /// <summary>
    /// The same as <see cref="Alphabetical"/>, but in reverse order.
    /// </summary>
    AlphabeticalDescending,
    /// <summary>
    /// The descriptions are listed in alphabetical order by the short argument name. If the
    /// argument has no short name, the long name is used. If the parsing mode is not
    /// <see cref="ParsingMode.LongShort"/>, this has the same effect as <see cref="Alphabetical"/>.
    /// </summary>
    AlphabeticalShortName,
    /// <summary>
    /// The same as <see cref="AlphabeticalShortName"/>, but in reverse order.
    /// </summary>
    AlphabeticalShortNameDescending,
}
