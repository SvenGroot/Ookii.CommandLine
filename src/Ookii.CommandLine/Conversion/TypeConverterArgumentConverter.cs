using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// A <see cref="ArgumentConverter"/> that wraps an existing <see cref="TypeConverter"/> for a
/// type.
/// </summary>
/// <remarks>
/// <para>
///   For a convenient way to use the default <see cref="TypeConverter"/> for a type, use the
///   <see cref="TypeConverterArgumentConverter{T}"/> class.
/// </para>
/// </remarks>
public class TypeConverterArgumentConverter : ArgumentConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeConverterArgumentConverter"/> class.
    /// </summary>
    /// <param name="converter">The <see cref="TypeConverter"/> to use.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="converter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The <see cref="TypeConverter"/> specified by <paramref name="converter"/> cannot convert
    /// from a <see cref="string"/>.
    /// </exception>
    public TypeConverterArgumentConverter(TypeConverter converter)
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
    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument)
        => Converter.ConvertFromString(null, culture, value);
}
