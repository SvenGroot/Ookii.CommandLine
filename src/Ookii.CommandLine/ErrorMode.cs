namespace Ookii.CommandLine
{
    /// <summary>
    /// Indicates whether something is an error, warning, or allowed.
    /// </summary>
    /// <seealso cref="ParseOptions.DuplicateArguments"/>
    public enum ErrorMode
    {
        /// <summary>
        /// The operation should raise an error.
        /// </summary>
        Error,
        /// <summary>
        /// The operation should display a warning, but continue.
        /// </summary>
        Warning,
        /// <summary>
        /// The operation should continue silently.
        /// </summary>
        Allow,
    }
}
