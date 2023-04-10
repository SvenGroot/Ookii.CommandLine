using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Converts from a string to a <see cref="Nullable{T}"/>.
/// </summary>
/// <remarks>
/// <para>
///   This converter uses the specified converter for the type T, except when the input is an
///   empty string, in which case it return <see langword="null"/>. This parallels the behavior
///   of the standard <see cref="System.ComponentModel.NullableConverter"/>.
/// </para>
/// </remarks>
/// <threadsafety instance="true" static="true" />
public class NullableConverter : ArgumentConverter
{
    private readonly ArgumentConverter _baseConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="NullableConverter"/> class.
    /// </summary>
    /// <param name="baseConverter">The converter to use for the type T.</param>
    public NullableConverter(ArgumentConverter baseConverter)
    {
        _baseConverter = baseConverter;
    }

    /// <inheritdoc/>
    public override object? Convert(string value, CultureInfo culture)
    {
        if (value.Length == 0)
        {
            return null;
        }

        return _baseConverter.Convert(value, culture);
    }

    /// <inheritdoc/>
    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture)
    {
        if (value.Length == 0)
        {
            return null;
        }

        return _baseConverter.Convert(value, culture);
    }
}
