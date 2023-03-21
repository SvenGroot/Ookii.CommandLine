using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// A generic version of the <see cref="CommandLineParser"/> class that offers strongly typed
    /// <see cref="Parse()"/> methods.
    /// </summary>
    /// <typeparam name="T">The type that defines the arguments.</typeparam>
    /// <remarks>
    /// <para>
    ///   This class provides the same functionality as the <see cref="CommandLineParser"/> class.
    ///   The only difference is that the <see cref="Parse()"/> method and overloads return the
    ///   correct type, which avoids casting.
    /// </para>
    /// <para>
    ///   If you don't intend to manually handle errors and usage help printing, and don't need
    ///   to inspect the state of the <see cref="CommandLineParser"/> instance, the static
    ///   <see cref="CommandLineParser.Parse{T}(string[], ParseOptions?)"/> should be used instead.
    /// </para>
    /// </remarks>
    public class CommandLineParser<T> : CommandLineParser
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class using the
        /// specified options.
        /// </summary>
        /// <param name="options">
        ///   <inheritdoc cref="CommandLineParser(Type, ParseOptions?)"/>
        /// </param>
        /// <exception cref="NotSupportedException">
        ///   The <see cref="CommandLineParser"/> cannot use type <typeparamref name="T"/> as the
        ///   command line arguments type, because it violates one of the rules concerning argument
        ///   names or positions, or has an argument type that cannot be parsed.
        /// </exception>
        /// <remarks>
        ///   <inheritdoc cref="CommandLineParser(Type, ParseOptions?)"/>
        /// </remarks>
        public CommandLineParser(ParseOptions? options = null)
            : base(typeof(T), options)
        {
        }

        /// <inheritdoc cref="CommandLineParser.Parse()"/>
        public new T? Parse()
        {
            return (T?)base.Parse();
        }

        /// <inheritdoc cref="CommandLineParser.Parse(string[], int)"/>
        public new T? Parse(string[] args, int index = 0)
        {
            return (T?)base.Parse(args, index);
        }

        /// <inheritdoc cref="CommandLineParser.ParseWithErrorHandling()"/>
        public new T? ParseWithErrorHandling()
        {
            return (T?)base.ParseWithErrorHandling();
        }

        /// <inheritdoc cref="CommandLineParser.ParseWithErrorHandling(string[], int)"/>
        public new T? ParseWithErrorHandling(string[] args, int index = 0)
        {
            return (T?)base.ParseWithErrorHandling(args, index);
        }
    }
}
