using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Converter for arguments with <see cref="bool"/> values. These are typically switch arguments.
/// </summary>
/// <remarks>
/// <para>
///   For a switch argument, the converter is only used if the value was explicitly specified.
/// </para>
/// </remarks>
/// <threadsafety instance="true" static="true"/>
public class BooleanConverter : ArgumentConverter
{
    /// <summary>
    /// A default instance of the converter.
    /// </summary>
    public static readonly BooleanConverter Instance = new();

    /// <summary>
    /// Converts a string memory region to a <see cref="bool"/>.
    /// </summary>
    /// <param name="value">The <see cref="ReadOnlyMemory{T}"/> containing the string to convert.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <param name="argument">
    /// The <see cref="CommandLineArgument"/> that will use the converted value.
    /// </param>
    /// <returns>An object representing the converted value.</returns>
    /// <remarks>
    /// <para>
    ///   This method performs the conversion using the <see cref="bool.Parse(ReadOnlySpan{char})" qualifyHint="true"/> method.
    /// </para>
    /// </remarks>
    /// <exception cref="FormatException">
    ///   The value was not in a correct format for the target type.
    /// </exception>
    public override object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument)
#if NET6_0_OR_GREATER
        => bool.Parse(value.Span);
#else
        => bool.Parse(value.ToString());
#endif
}
