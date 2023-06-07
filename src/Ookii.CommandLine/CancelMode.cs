namespace Ookii.CommandLine;

/// <summary>
/// Indicates whether and how the argument should cancel parsing.
/// </summary>
/// <seealso cref="CommandLineArgumentAttribute.CancelParsing"/>
/// <seealso cref="ArgumentParsedEventArgs"/>
public enum CancelMode
{
    /// <summary>
    /// The argument does not cancel parsing.
    /// </summary>
    None,
    /// <summary>
    /// The argument cancels parsing, discarding the results so far. Parsing, using for example the
    /// <see cref="CommandLineParser.Parse{T}(ParseOptions?)"/> method, will return a <see langword="null"/>
    /// value. The <see cref="ParseResult.Status"/> property will be <see cref="ParseStatus.Canceled"/>.
    /// </summary>
    Abort,
    /// <summary>
    /// The argument cancels parsing, returning success using the results so far. Remaining
    /// arguments are not parsed, and will be available in the <see cref="ParseResult.RemainingArguments"/>
    /// property. The <see cref="ParseResult.Status"/> property will be <see cref="ParseStatus.Success"/>.
    /// If not all required arguments have values at this point, an exception will be thrown.
    /// </summary>
    Success
}
