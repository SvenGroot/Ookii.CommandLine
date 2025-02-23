#if NET7_0_OR_GREATER

using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// An argument converter for types that implement the <see cref="IParsable{TSelf}"/> interface.
/// </summary>
/// <typeparam name="T">The type to convert to.</typeparam>
/// <remarks>
/// <para>
///   Conversion is performed using the <see cref="IParsable{TSelf}.Parse" qualifyHint="true"/> method.
/// </para>
/// <para>
///   Only use this converter for types that implement the <see cref="IParsable{TSelf}"/> interface,
///   but not the <see cref="ISpanParsable{TSelf}"/> interface. For types that implement the
///   <see cref="ISpanParsable{TSelf}"/> interface, use the <see cref="SpanParsableConverter{T}"/>
///   class.
/// </para>
/// </remarks>
/// <threadsafety instance="true" static="true"/>
public class ParsableConverter<T> : ArgumentConverter
    where T : IParsable<T>
{
    /// <inheritdoc/>
    public override object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument)
        => T.Parse(value.ToString(), culture);
}

#endif
