using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Base class for converters from a string to the type of an argument.
/// </summary>
/// <remarks>
/// <para>
///   To create a custom argument converter, you must implement the
///   <see cref="Convert"/> method.
/// </para>
/// <para>
///   The source of the conversion is a <see cref="ReadOnlyMemory{T}"/> that
///   contains the argument text. This may be an entire argument, or a
///   substring of an argument if the user used a non-whitespace argument name
///   separator like <c>-Name:Value</c>.
/// </para>
/// <para>
///   <see cref="ReadOnlyMemory{T}"/> is used because
///   <see cref="ReadOnlyMemory{T}.ToString" qualifyHint="true"/> does not
///   allocate when the memory is an entire string. However, since it still
///   allocates for substrings, it's still recommended to convert using the
///   <see cref="ReadOnlyMemory{T}"/> or <see cref="ReadOnlySpan{T}"/> directly
///   if possible.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public abstract class ArgumentConverter
{
    /// <summary>
    /// Converts a string memory region to the type of the argument.
    /// </summary>
    /// <param name="value">
    /// The <see cref="ReadOnlyMemory{T}"/> containing the string to convert.
    /// </param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <param name="argument">
    /// The <see cref="CommandLineArgument"/> that will use the converted value.
    /// </param>
    /// <returns>An object representing the converted value.</returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="culture"/> or <paramref name="argument"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   The value was not in a correct format for the target type.
    /// </exception>
    /// <exception cref="OverflowException">
    ///   The value was out of range for the target type.
    /// </exception>
    /// <exception cref="CommandLineArgumentException">
    ///   The value was not in a correct format for the target type. Unlike other exceptions,
    ///   which will be wrapped in a <see cref="CommandLineArgumentException"/>, a
    ///   <see cref="CommandLineArgumentException"/> thrown by this method will be passed down to
    ///   the user unmodified.
    /// </exception>
    public abstract object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument);
}
