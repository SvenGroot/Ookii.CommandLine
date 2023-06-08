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
        private ParseResult(ParseStatus status, CommandLineArgumentException? exception = null, string? argumentName = null,
            ReadOnlyMemory<string> remainingArguments = default)
        {
            Status = status;
            LastException = exception;
            ArgumentName = argumentName;
            RemainingArguments = remainingArguments;
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
        /// Gets any arguments that were not parsed by the <see cref="CommandLineParser"/> if
        /// parsing was canceled.
        /// </summary>
        /// <value>
        /// A <see cref="ReadOnlyMemory{T}"/> instance with the remaining arguments, or an empty
        /// collection if there were no remaining arguments, or parsing was not canceled.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This will always be an empty collection if parsing was not canceled. 
        /// </para>
        /// </remarks>
        public ReadOnlyMemory<string> RemainingArguments { get; }

        /// <summary>
        /// Gets a <see cref="ParseResult"/> instance that represents successful parsing.
        /// </summary>
        /// <param name="cancelArgumentName">
        /// The name of the argument that canceled parsing using <see cref="CancelMode.Success"/>.
        /// </param>
        /// <param name="remainingArguments">Any remaining arguments that were not parsed.</param>
        /// <returns>
        /// An instance of the <see cref="ParseResult"/> structure.
        /// </returns>
        public static ParseResult FromSuccess(string? cancelArgumentName = null, ReadOnlyMemory<string> remainingArguments = default)
            => new(ParseStatus.Success,argumentName: cancelArgumentName, remainingArguments: remainingArguments);

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
        /// <param name="remainingArguments">Any remaining arguments that were not parsed.</param>
        /// <returns>An instance of the <see cref="ParseResult"/> structure.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="argumentName"/> is <see langword="null"/>.
        /// </exception>
        public static ParseResult FromCanceled(string argumentName, ReadOnlyMemory<string> remainingArguments)
            => new(ParseStatus.Canceled, null, argumentName ?? throw new ArgumentNullException(nameof(argumentName)), remainingArguments);
    }
}
