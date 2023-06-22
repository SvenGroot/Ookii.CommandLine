#if NET7_0_OR_GREATER

using System;

namespace Ookii.CommandLine;

/// <summary>
/// Defines a mechanism to create a <see cref="CommandLineParser{T}"/> for a type.
/// </summary>
/// <typeparam name="TSelf">The type that implements this interface.</typeparam>
/// <remarks>
/// <note>
///   This type is only available when using .Net 7 or later.
/// </note>
/// <para>
///   This interface is automatically implemented on a class when the
///   <see cref="GeneratedParserAttribute"/> is used. Classes without that attribute must create
///   the parser directly by using the <see cref="CommandLineParser{T}.CommandLineParser(Ookii.CommandLine.ParseOptions?)"/>
///   constructor; these classes do not support this interface unless it is manually implemented.
/// </para>
/// <para>
///   When using a version of .Net where static interface methods are not supported (versions prior
///   to .Net 7.0), the <see cref="GeneratedParserAttribute"/> will still generate the same method
///   as defined by this interface, just without having it implement the interface.
/// </para>
/// </remarks>
public interface IParserProvider<TSelf>
    where TSelf : class, IParserProvider<TSelf>
{
    /// <summary>
    /// Creates a <see cref="CommandLineParser{T}"/> instance using the specified options.
    /// </summary>
    /// <param name="options">
    /// The options that control parsing behavior, or <see langword="null"/> to use the
    /// default options.
    /// </param>
    /// <returns>
    /// An instance of the <see cref="CommandLineParser{T}"/> class for the type
    /// <typeparamref name="TSelf"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///   The <see cref="CommandLineParser{T}"/> cannot use type <typeparamref name="TSelf"/> as the
    ///   command line arguments type, because it violates one of the rules concerning argument
    ///   names or positions. Even when the parser was generated using the <see cref="GeneratedParserAttribute"/>
    ///   class, not all those rules can be checked at compile time.
    /// </exception>
    /// <seealso cref="CommandLineParser{T}.CommandLineParser(ParseOptions?)" qualifyHint="true"/>
    public static abstract CommandLineParser<TSelf> CreateParser(ParseOptions? options = null);
}

#endif
