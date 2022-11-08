using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// A convenience wrapper around <see cref="CommandLineParser"/> that lets you specify the
    /// arguments type using generics.
    /// </summary>
    /// <typeparam name="T">The type that defines the arguments.</typeparam>
    /// <remarks>
    /// <para>
    ///   This class provides the same functionality as <see cref="CommandLineParser"/>. The
    ///   only difference is that the <see cref="Parse()"/> method and overloads return the correct type, which
    ///   avoids casting.
    /// </para>
    /// <para>
    ///   In most cases, the generic <see cref="CommandLineParser.Parse{T}(string[], ParseOptions?)"/>
    ///   method is likely more useful than this class.
    /// </para>
    /// </remarks>
    public class CommandLineParser<T> : CommandLineParser
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class using the
        /// specified arguments type and options.
        /// </summary>
        /// <param name="options">
        ///   The options that control parsing behavior, or <see langword="null"/> to use the
        ///   default options.
        /// </param>
        /// <exception cref="NotSupportedException">
        ///   The <see cref="CommandLineParser"/> cannot use <typeparamref name="T"/> as the command
        ///   line arguments type, because it violates one of the rules concerning argument names or
        ///   positions, or has an argument type that cannot be parsed.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   The <see cref="ParseOptions.UsageOptions"/> are not used here. If you want those to
        ///   take effect, they must still be passed to <see cref="CommandLineParser.WriteUsage(System.IO.TextWriter, int, WriteUsageOptions?)"/>.
        /// </para>
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
    }
}
