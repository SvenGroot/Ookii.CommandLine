#if NET7_0_OR_GREATER

using System;

namespace Ookii.CommandLine;

/// <summary>
/// Defines a mechanism to parse command line arguments into a type.
/// </summary>
/// <typeparam name="TSelf">The type that implements this interface.</typeparam>
/// <remarks>
/// <note>
///   This type is only available when using .Net 7 or later.
/// </note>
/// <para>
///   This interface is automatically implemented on a class when the
///   <see cref="GeneratedParserAttribute"/> is used. Classes without that attribute must parse
///   arguments using the <see cref="CommandLineParser.Parse{T}(Ookii.CommandLine.ParseOptions?)"/>
///   method, or create the parser directly by using the <see cref="CommandLineParser{T}.CommandLineParser(Ookii.CommandLine.ParseOptions?)"/>
///   constructor; these classes do not support this interface unless it is manually implemented.
/// </para>
/// <para>
///   When using a version of .Net where static interface methods are not supported (versions prior
///   to .Net 7.0), the <see cref="GeneratedParserAttribute"/> will still generate the same methods
///   as defined by this interface, just without having them implement the interface.
/// </para>
/// </remarks>
public interface IParser<TSelf> : IParserProvider<TSelf>
    where TSelf : class, IParser<TSelf>
{
    /// <summary>
    /// Parses the arguments returned by the <see cref="Environment.GetCommandLineArgs" qualifyHint="true"/>
    /// method using the type <typeparamref name="TSelf"/>, handling errors and showing usage help
    /// as required.
    /// </summary>
    /// <param name="options">
    ///   The options that control parsing behavior and usage help formatting. If
    ///   <see langword="null" />, the default options are used.
    /// </param>
    /// <returns>
    ///   An instance of the type <typeparamref name="TSelf"/>, or <see langword="null"/> if an
    ///   error occurred, or argument parsing was canceled by the <see cref="CommandLineArgumentAttribute.CancelParsing" qualifyHint="true"/>
    ///   property or a method argument that returned <see langword="false"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///   The <see cref="CommandLineParser{T}"/> cannot use type <typeparamref name="TSelf"/> as the
    ///   command line arguments type, because it violates one of the rules concerning argument
    ///   names or positions. Even when the parser was generated using the <see cref="GeneratedParserAttribute"/>
    ///   class, not all those rules can be checked at compile time.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This method is typically generated for a class that defines command line arguments by
    ///   the <see cref="GeneratedParserAttribute"/> attribute.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.Parse{T}(ParseOptions?)" qualifyHint="true"/>
    public static abstract TSelf? Parse(ParseOptions? options = null);

    /// <summary>
    /// Parses the specified command line arguments using the type <typeparamref name="TSelf"/>,
    /// handling errors and showing usage help as required.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <param name="options">
    ///   The options that control parsing behavior and usage help formatting. If
    ///   <see langword="null" />, the default options are used.
    /// </param>
    /// <returns>
    ///   An instance of the type <typeparamref name="TSelf"/>, or <see langword="null"/> if an
    ///   error occurred, or argument parsing was canceled by the <see cref="CommandLineArgumentAttribute.CancelParsing" qualifyHint="true"/>
    ///   property or a method argument that returned <see langword="false"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///   The <see cref="CommandLineParser{T}"/> cannot use type <typeparamref name="TSelf"/> as the
    ///   command line arguments type, because it violates one of the rules concerning argument
    ///   names or positions. Even when the parser was generated using the <see cref="GeneratedParserAttribute"/>
    ///   class, not all those rules can be checked at compile time.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This method is typically generated for a class that defines command line arguments by
    ///   the <see cref="GeneratedParserAttribute"/> attribute.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineParser.Parse{T}(string[], ParseOptions?)" qualifyHint="true"/>
    public static abstract TSelf? Parse(string[] args, ParseOptions? options = null);

    /// <inheritdoc cref="Parse(string[], ParseOptions?)"/>
    /// <seealso cref="CommandLineParser.Parse{T}(ReadOnlyMemory{string}, ParseOptions?)" qualifyHint="true"/>
    public static abstract TSelf? Parse(ReadOnlyMemory<string> args, ParseOptions? options = null);
}

#endif
