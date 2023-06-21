namespace Ookii.CommandLine;

/// <summary>
/// Indicates the status of the last call to the <see cref="CommandLineParser.Parse(string[], int)" qualifyHint="true"/>
/// method.
/// </summary>
/// <seealso cref="ParseResult"/>
public enum ParseStatus
{
    /// <summary>
    /// The <see cref="CommandLineParser.Parse(string[], int)" qualifyHint="true"/> method has not been called yet.
    /// </summary>
    None,
    /// <summary>
    /// The operation was successful.
    /// </summary>
    Success,
    /// <summary>
    /// An error occurred while parsing the arguments.
    /// </summary>
    Error,
    /// <summary>
    /// Parsing was canceled by one of the arguments.
    /// </summary>
    Canceled
}
