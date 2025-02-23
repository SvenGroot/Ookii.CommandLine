namespace Ookii.CommandLine;

/// <summary>
/// Indicates whether and how an argument should cancel parsing.
/// </summary>
/// <seealso cref="CommandLineArgumentAttribute.CancelParsing" qualifyHint="true"/>
/// <seealso cref="ArgumentParsedEventArgs"/>
public enum CancelMode
{
    /// <summary>
    /// The argument does not cancel parsing.
    /// </summary>
    None,
    /// <summary>
    /// The argument cancels parsing, discarding the results so far. Parsing, using for example the
    /// <see cref="CommandLineParser.Parse{T}(ParseOptions?)" qualifyHint="true"/> method, will
    /// return a <see langword="null"/> value. The <see cref="ParseResult.Status" qualifyHint="true"/>
    /// property will be <see cref="ParseStatus.Canceled" qualifyHint="true"/>. The
    /// <see cref="ParseResult.HelpRequested" qualifyHint="true"/> property will be
    /// <see langword="false"/>.
    /// </summary>
    Abort,
    /// <summary>
    /// The same as <see cref="Abort"/>, but the <see cref="ParseResult.HelpRequested"
    /// qualifyHint="true"/> property will be <see langword="true"/>.
    /// </summary>
    AbortWithHelp,
    /// <summary>
    /// The argument cancels parsing, returning success using the results so far. Remaining
    /// arguments are not parsed, and will be available in the <see cref="ParseResult.RemainingArguments" qualifyHint="true"/>
    /// property. The <see cref="ParseResult.Status" qualifyHint="true"/> property will be <see cref="ParseStatus.Success" qualifyHint="true"/>.
    /// If not all required arguments have values at this point, an exception will be thrown.
    /// </summary>
    Success,
}

static class CancelModeExtensions
{
    public static bool IsAborted(this CancelMode self) => self is CancelMode.Abort or CancelMode.AbortWithHelp;

    public static bool HelpRequested(this CancelMode self) => self == CancelMode.AbortWithHelp;
}
