namespace Ookii.CommandLine
{
    /// <summary>
    /// Indicates if and how usage is shown if an error occurred parsing the command line.
    /// </summary>
    /// <seealso cref="ParseOptions.ShowUsageOnError"/>
    public enum UsageHelpRequest
    {
        /// <summary>
        /// Full usage help is shown, including the argument descriptions.
        /// </summary>
        Full,
        /// <summary>
        /// Only the usage syntax is shown; the argument descriptions are not. In addition, the
        /// <see cref="UsageWriter.WriteMoreInfoMessage"/> message is shown.
        /// </summary>
        SyntaxOnly,
        /// <summary>
        /// No usage help is shown. Instead, the <see cref="UsageWriter.WriteMoreInfoMessage"/>
        /// message is shown.
        /// </summary>
        None
    }
}
