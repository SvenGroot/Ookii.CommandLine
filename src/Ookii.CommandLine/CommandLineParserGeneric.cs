using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Support;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
        /// <exception cref="InvalidOperationException">
        ///   The type indicated by <typeparamref name="T"/> has the <see cref="GeneratedParserAttribute"/>
        ///   attribute applied. Use the generated static <c>CreateParser()</c> or <c>Parse()</c>
        ///   methods on the arguments type to access the generated parser. For subcommands, use a
        ///   command provider with the <see cref="GeneratedCommandManagerAttribute"/> attribute to
        ///   create a <see cref="CommandManager"/> that will use generated parsers for subcommands. Set
        ///   the <see cref="ParseOptions.AllowReflectionWithGeneratedParser"/> property to
        ///   <see langword="true"/> to disable this exception.
        /// </exception>
        /// <remarks>
        ///   <inheritdoc cref="CommandLineParser(Type, ParseOptions?)"/>
        /// </remarks>
#if NET6_0_OR_GREATER
        [RequiresUnreferencedCode("Trimming cannot be used when determining arguments via reflection. Use the GeneratedArgumentsParserAttribute instead.")]
#endif
        public CommandLineParser(ParseOptions? options = null)
            : base(typeof(T), options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class using the
        /// specified options.
        /// </summary>
        /// <param name="provider">
        ///   <inheritdoc cref="CommandLineParser(ArgumentProvider, ParseOptions?)"/>
        /// </param>
        /// <param name="options">
        ///   <inheritdoc cref="CommandLineParser(ArgumentProvider, ParseOptions?)"/>
        /// </param>
        /// <exception cref="NotSupportedException">
        ///   The <see cref="CommandLineParser"/> cannot use type <typeparamref name="T"/> as the
        ///   command line arguments type, because it violates one of the rules concerning argument
        ///   names or positions, or has an argument type that cannot be parsed.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   The <see cref="ArgumentProvider.ArgumentsType"/> property for the <paramref name="provider"/>
        ///   if a different type than <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The provider uses <see cref="ProviderKind.Reflection"/>, but the type indicated by the
        ///   <typeparamref name="T"/> property has the <see cref="GeneratedParserAttribute"/>
        ///   attribute applied. Use the generated static <c>CreateParser()</c> or <c>Parse()</c>
        ///   methods on the arguments type to access the generated parser. For subcommands, use a
        ///   command provider with the <see cref="GeneratedCommandManagerAttribute"/> attribute to
        ///   create a <see cref="CommandManager"/> that will use generated parsers for subcommands. Set
        ///   the <see cref="ParseOptions.AllowReflectionWithGeneratedParser"/> property to
        ///   <see langword="true"/> to disable this exception.
        /// </exception>
        /// <remarks>
        ///   <inheritdoc cref="CommandLineParser(ArgumentProvider, ParseOptions?)"/>
        /// </remarks>
        public CommandLineParser(ArgumentProvider provider, ParseOptions? options = null)
            : base(provider, options)
        {
            if (provider.ArgumentsType != typeof(T))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.IncorrectProviderTypeFormat, typeof(T)), nameof(provider));
            }
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
