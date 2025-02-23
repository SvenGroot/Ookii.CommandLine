using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// A converter for arguments with string values.
/// </summary>
/// <remarks>
/// This converter does not perform any actual conversion, and returns the existing string as-is.
/// If the input was a <see cref="ReadOnlySpan{T}"/> for <see cref="char"/>, a new string is
/// allocated for it.
/// </remarks>
/// <threadsafety instance="true" static="true"/>
public class StringConverter : ArgumentConverter
{
    /// <summary>
    /// A default instance of the converter.
    /// </summary>
    public static readonly StringConverter Instance = new();

    /// <summary>
    /// Converts a string memory region to a string.
    /// </summary>
    /// <param name="value">The string memory region to convert.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <param name="argument">
    /// The <see cref="CommandLineArgument"/> that will use the converted value.
    /// </param>
    /// <returns>The value of the <paramref name="value"/> parameter as a string.</returns>
    public override object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument)
        => value.ToString();
}
