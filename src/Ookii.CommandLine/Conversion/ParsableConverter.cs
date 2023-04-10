#if NET7_0_OR_GREATER

using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// An argument converter for types that implement <see cref="IParsable{TSelf}"/>.
/// </summary>
/// <typeparam name="T">The type to convert.</typeparam>
/// <remarks>
/// <para>
///   Conversion is performed using the <see cref="IParsable{TSelf}.Parse"/> method.
/// </para>
/// <para>
///   Only use this converter for types that implement <see cref="IParsable{TSelf}"/>, but not
///   <see cref="ISpanParsable{TSelf}"/>. For types that implement <see cref="ISpanParsable{TSelf}"/>,
///   use the <see cref="SpanParsableConverter{T}"/>.
/// </para>
/// </remarks>
/// <threadsafety instance="true" static="true"/>
public class ParsableConverter<T> : ArgumentConverter
    where T : IParsable<T>
{
    /// <inheritdoc/>
    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument) => T.Parse(value, culture);
}

#endif
