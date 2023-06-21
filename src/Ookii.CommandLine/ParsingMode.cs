namespace Ookii.CommandLine;

/// <summary>
/// Indicates what argument parsing rules should be used to interpret the command line.
/// </summary>
/// <remarks>
/// <para>
///   To set the parsing mode for a <see cref="CommandLineParser"/>, use the <see cref="ParseOptionsAttribute.Mode" qualifyHint="true"/>
///   property or the <see cref="ParseOptions.Mode" qualifyHint="true"/> property.
/// </para>
/// </remarks>
/// <seealso cref="CommandLineParser.Mode" qualifyHint="true"/>
public enum ParsingMode
{
    /// <summary>
    /// Use the normal Ookii.CommandLine parsing rules.
    /// </summary>
    Default,
    /// <summary>
    /// Allow arguments to have both long and short names, using the <see cref="CommandLineParser.LongArgumentNamePrefix" qualifyHint="true"/>
    /// to specify a long name, and the regular <see cref="CommandLineParser.ArgumentNamePrefixes" qualifyHint="true"/>
    /// to specify a short name.
    /// </summary>
    LongShort
}
