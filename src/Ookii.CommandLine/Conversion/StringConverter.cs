using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// A converter for arguments with string values.
/// </summary>
/// <remarks>
/// This converter does not performan any actual conversion, and returns the existing string as-is.
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

    /// <inheritdoc/>
    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument) => value;
}
