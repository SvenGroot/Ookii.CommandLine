#if NET7_0_OR_GREATER

using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// An argument converter for types that implement the <see cref="ISpanParsable{TSelf}"/> interface.
/// </summary>
/// <typeparam name="T">The type to convert to.</typeparam>
/// <remarks>
/// <para>
///   Conversion is performed using the <see cref="ISpanParsable{TSelf}.Parse" qualifyHint="true"/>
///   method.
/// </para>
/// <para>
///   For types that implement the <see cref="IParsable{TSelf}"/> interface, but not the <see cref="ISpanParsable{TSelf}"/>
///   interface, use the <see cref="ParsableConverter{T}"/> class.
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
