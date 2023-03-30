using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Base class for converters from a string to the type of an argument.
/// </summary>
/// <remarks>
/// <para>
///   To create a custom argument converter, you must implement at least the
///   <see cref="Convert(string, CultureInfo)"/> method. If it's possible to convert to the target
///   type from a <see cref="ReadOnlySpan{T}"/> structure, it's strongly recommended to also
///   implement the <see cref="Convert(ReadOnlySpan{char}, CultureInfo)"/> method.
/// </para>
/// </remarks>
public abstract class ArgumentConverter
{
    /// <summary>
    /// Convert a string to the type of the argument.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <returns>An object representing the converted value.</returns>
    /// <exception cref="FormatException">
    ///   The value was not in a correct format for the target type.
    /// </exception>
    public abstract object? Convert(string value, CultureInfo culture);

    /// <summary>
    /// Convert a string to the type of the argument.
    /// </summary>
    /// <param name="value">The <see cref="ReadOnlySpan{T}"/> containing the string to convert.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <returns>An object representing the converted value.</returns>
    /// <remarks>
    /// <para>
    ///   The default implementation of this method will allocate a string and call
    ///   <see cref="Convert(string, CultureInfo)"/>. Implement this method if it's possible to
    ///   
    /// </para>
    /// </remarks>
    /// <exception cref="FormatException">
    ///   The value was not in a correct format for the target type.
    /// </exception>
    public virtual object? Convert(ReadOnlySpan<char> value, CultureInfo culture)
    {
        return Convert(value.ToString(), culture);
    }
}
