using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Indicates the result of the last call to the <see cref="CommandLineParser.Parse(string[], int)"/>
    /// method.
    /// </summary>
    /// <seealso cref="CommandLineParser.ParseResult"/>
    public readonly struct ParseResult
    {
        private ParseResult(ParseStatus status, CommandLineArgumentException? exception = null, string? argumentName = null)
        {
            Status = status;
            LastException = exception;
            ArgumentName = argumentName;
        }

        /// <summary>
        /// Gets the status of the last call to the <see cref="CommandLineParser.Parse(string[], int)"/>
        /// method.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="ParseStatus"/> enumeration.
        /// </value>
        public ParseStatus Status { get; }

        /// <summary>
        /// Gets the exception that occurred during the last call to the
        /// <see cref="CommandLineParser.Parse(string[], int)"/> method, if any.
        /// </summary>
        /// <value>
        /// The exception, or <see langword="null"/> if parsing was successful or canceled.
        /// </value>
        public CommandLineArgumentException? LastException { get; }

        /// <summary>
        /// Gets the name of the argument that caused the error or cancellation.
        /// </summary>
        /// <value>
        /// If the <see cref="Status"/> property is <see cref="ParseStatus.Error"/>, the value of
        /// the <see cref="CommandLineArgumentException.ArgumentName"/> property. If it's
        /// <see cref="ParseStatus.Canceled"/>, the name of the argument that canceled parsing.
        /// Otherwise, <see langword="null"/>.
        /// </value>
        public string? ArgumentName { get; }

        /// <summary>
        /// Gets a <see cref="ParseResult"/> instance that represents successful parsing.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="ParseResult"/> structure.
        /// </value>
        public static ParseResult Success => new(ParseStatus.Success);

        /// <summary>
        /// Creates a <see cref="ParseResult"/> instance that represents a parsing error.
        /// </summary>
        /// <param name="exception">The exception that occurred during parsing.</param>
        /// <returns>An instance of the <see cref="ParseResult"/> structure.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="exception"/> is <see langword="null"/>.
        /// </exception>
        public static ParseResult FromException(CommandLineArgumentException exception)
            => new(ParseStatus.Error, exception ?? throw new ArgumentNullException(nameof(exception)), exception.ArgumentName);

        /// <summary>
        /// Creates a <see cref="ParseResult"/> instance that represents canceled parsing.
        /// </summary>
        /// <param name="argumentName">The name of the argument that canceled parsing.</param>
        /// <returns>An instance of the <see cref="ParseResult"/> structure.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="argumentName"/> is <see langword="null"/>.
        /// </exception>
        public static ParseResult FromCanceled(string argumentName)
            => new(ParseStatus.Canceled, null, argumentName ?? throw new ArgumentNullException(nameof(argumentName)));
    }
}
