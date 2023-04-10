using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Converter for arguments with boolean values. These are typically switch arguments.
/// </summary>
/// <threadsafety instance="true" static="true"/>
public class BooleanConverter : ArgumentConverter
{
    /// <summary>
    /// A default instance of the converter.
    /// </summary>
    public static readonly BooleanConverter Instance = new();

    /// <inheritdoc/>
    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument) => bool.Parse(value);

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
    /// <inheritdoc/>
    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture, CommandLineArgument argument) => bool.Parse(value);
#endif
}
