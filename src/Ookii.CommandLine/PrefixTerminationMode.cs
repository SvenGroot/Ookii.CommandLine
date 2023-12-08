namespace Ookii.CommandLine;

/// <summary>
/// Indicates the effect of an argument that is just the long argument prefix ("--" by default)
/// by itself, not followed by a name.
/// </summary>
/// <seealso cref="ParseOptions.PrefixTermination"/>
/// <seealso cref="ParseOptionsAttribute.PrefixTermination"/>
public enum PrefixTerminationMode
{
    /// <summary>
    /// This argument has no special meaning.
    /// </summary>
    None,
    /// <summary>
    /// The argument terminates the used of named arguments. Any following arguments are interpreted
    /// as values for positional arguments, even if they begin with an argument name separator.
    /// </summary>
    PositionalOnly,
    /// <summary>
    /// The argument cancels parsing, returning an instance of the arguments type and making the
    /// values after this argument available in the <see cref="ParseResult.RemainingArguments" qualifyHint="true"/>
    /// property. This is identical to how an argument with <see cref="CancelMode.Success" qualifyHint="true"/>
    /// behaves.
    /// </summary>
    CancelWithSuccess
}
