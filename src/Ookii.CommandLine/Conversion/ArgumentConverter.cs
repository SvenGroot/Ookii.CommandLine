using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Base class for converters from a string to the type of an argument.
/// </summary>
/// <remarks>
/// <para>
///   To create a custom argument converter, you must implement at least the
///   <see cref="Convert(string, CultureInfo, CommandLineArgument)"/> method. If it's possible to
///   convert to the target type from a <see cref="ReadOnlySpan{T}"/> structure, it's strongly
///   recommended to also implement the <see cref="Convert(ReadOnlySpan{char}, CultureInfo, CommandLineArgument)"/>
///   method.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public abstract class ArgumentConverter
{
    /// <summary>
    /// Converts a string to the type of the argument.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <param name="argument">
    /// The <see cref="CommandLineArgument"/> that will use the converted value.
    /// </param>
    /// <returns>An object representing the converted value.</returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="value"/> or <paramref name="culture"/> or <paramref name="argument"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   The value was not in a correct format for the target type.
    /// </exception>
    /// <exception cref="OverflowException">
    ///   The value was out of range for the target type.
    /// </exception>
    /// <exception cref="CommandLineArgumentException">
    ///   The value was not in a correct format for the target type. Unlike <see cref="FormatException"/>
    ///   and <see cref="OverflowException"/>, a <see cref="CommandLineArgumentException"/> thrown
    ///   by this method will be passed down to the user unmodified.
    /// </exception>
    public abstract object? Convert(string value, CultureInfo culture, CommandLineArgument argument);

    /// <summary>
    /// Converts a string span to the type of the argument.
    /// </summary>
    /// <param name="value">The <see cref="ReadOnlySpan{T}"/> containing the string to convert.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <param name="argument">
    /// The <see cref="CommandLineArgument"/> that will use the converted value.
    /// </param>
    /// <returns>An object representing the converted value.</returns>
    /// <remarks>
    /// <para>
    ///   The default implementation of this method will allocate a string and call
    ///   <see cref="Convert(string, CultureInfo, CommandLineArgument)"/>. Override this method if
    ///   a direct conversion from a <see cref="ReadOnlySpan{T}"/> is possible for the target
    ///   type.
    /// </para>
    /// </remarks>
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
    ///   The value was not in a correct format for the target type. Unlike <see cref="FormatException"/>
    ///   and <see cref="OverflowException"/>, a <see cref="CommandLineArgumentException"/> thrown
    ///   by this method will be passed down to the user unmodified.
    /// </exception>
    public virtual object? Convert(ReadOnlySpan<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        return Convert(value.ToString(), culture, argument);
    }
}
