#if NET7_0_OR_GREATER

using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// An argument converter for types that implement <see cref="ISpanParsable{TSelf}"/>.
/// </summary>
/// <typeparam name="T">The type to convert.</typeparam>
/// <remarks>
/// <para>
///   Conversion is performed using the <see cref="ISpanParsable{TSelf}.Parse"/> method.
/// </para>
/// <para>
///   For types that implement <see cref="IParsable{TSelf}"/>, but not <see cref="ISpanParsable{TSelf}"/>,
///   use the <see cref="ParsableConverter{T}"/>.
/// </para>
/// </remarks>
/// <threadsafety instance="true" static="true"/>
public class SpanParsableConverter<T> : ArgumentConverter
    where T : ISpanParsable<T>
{
    /// <inheritdoc/>
    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument) => T.Parse(value, culture);

    /// <inheritdoc/>
    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture, CommandLineArgument argument)
        => T.Parse(value, culture);
}

#endif
