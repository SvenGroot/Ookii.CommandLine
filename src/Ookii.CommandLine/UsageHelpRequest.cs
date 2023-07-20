namespace Ookii.CommandLine;

/// <summary>
/// Indicates if and how usage is shown if an error occurred parsing the command line.
/// </summary>
/// <seealso cref="ParseOptions.ShowUsageOnError" qualifyHint="true"/>
public enum UsageHelpRequest
{
    /// <summary>
    /// Only the usage syntax is shown; the argument descriptions are not. In addition, the
    /// <see cref="UsageWriter.WriteMoreInfoMessage" qualifyHint="true"/> message is shown.
    /// </summary>
    SyntaxOnly,
    /// <summary>
    /// Full usage help is shown, including the argument descriptions.
    /// </summary>
    Full,
    /// <summary>
    /// No usage help is shown. Instead, the <see cref="UsageWriter.WriteMoreInfoMessage" qualifyHint="true"/>
    /// message is shown.
    /// </summary>
    None
}
