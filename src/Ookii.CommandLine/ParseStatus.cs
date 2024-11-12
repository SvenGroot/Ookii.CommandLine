namespace Ookii.CommandLine;

/// <summary>
/// Indicates the status of the last call to the <see cref="CommandLineParser.Parse()" qualifyHint="true"/>
/// method or one of its overloads.
/// </summary>
/// <seealso cref="ParseResult"/>
public enum ParseStatus
{
    /// <summary>
    /// The <see cref="CommandLineParser.Parse()" qualifyHint="true"/> method has not been called yet.
    /// </summary>
    None,
    /// <summary>
    /// The operation successfully parsed all arguments, or was canceled using
    /// <see cref="CancelMode.Success" qualifyHint="true"/>. Check the
    /// <see cref="ParseResult.ArgumentName" qualifyHint="true"/> property to differentiate between
    /// the two.
    /// </summary>
    Success,
    /// <summary>
    /// An error occurred while parsing the arguments.
    /// </summary>
    Error,
    /// <summary>
    /// Parsing was canceled by one of the arguments using <see cref="CancelMode.Abort" qualifyHint="true"/>
    /// or <see cref="CancelMode.AbortWithHelp" qualifyHint="true"/>
    /// </summary>
    Canceled
}
