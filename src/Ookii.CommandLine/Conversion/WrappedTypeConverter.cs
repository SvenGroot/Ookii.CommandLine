﻿using System;
using System.ComponentModel;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// An <see cref="ArgumentConverter"/> that wraps an existing <see cref="TypeConverter"/>.
/// </summary>
/// <remarks>
/// <para>
///   For a convenient way to use to use any <see cref="TypeConverter"/> with the
///   <see cref="ArgumentConverterAttribute"/> attribute, use the <see cref="WrappedTypeConverter{T}"/>
///   class. To use the default <see cref="TypeConverter"/> for a type, use the
///   <see cref="WrappedDefaultTypeConverter{T}"/> class.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public class WrappedTypeConverter : ArgumentConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedTypeConverter"/> class.
    /// </summary>
    /// <param name="converter">The <see cref="TypeConverter"/> to use.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="converter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The <see cref="TypeConverter"/> specified by <paramref name="converter"/> cannot convert
    /// from a <see cref="string"/>.
    /// </exception>
    public WrappedTypeConverter(TypeConverter converter)
    {
        Converter = converter ?? throw new ArgumentNullException(nameof(converter));
        if (!converter.CanConvertFrom(typeof(string)))
        {
            throw new ArgumentException(Properties.Resources.InvalidTypeConverter, nameof(converter));
        }
    }

    /// <summary>
    /// Gets the type converter used by this instance.
    /// </summary>
    /// <value>
    /// An instance of the <see cref="TypeConverter"/> class.
    /// </value>
    public TypeConverter Converter { get; }

    /// <inheritdoc/>
    public override object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument)
        => Converter.ConvertFromString(null, culture, value.ToString());
}
