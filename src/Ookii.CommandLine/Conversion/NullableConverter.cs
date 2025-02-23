using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Converts from a string to a <see cref="Nullable{T}"/>.
/// </summary>
/// <remarks>
/// <para>
///   This converter uses the specified converter for the type T, except when the input is an
///   empty string, in which case it return <see langword="null"/>. This parallels the behavior
///   of the <see cref="System.ComponentModel.NullableConverter" qualifyHint="true"/>.
/// </para>
/// </remarks>
/// <threadsafety instance="true" static="true" />
public class NullableConverter : ArgumentConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NullableConverter"/> class.
    /// </summary>
    /// <param name="baseConverter">The converter to use for the target type.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="baseConverter"/> is <see langword="null"/>.
    /// </exception>
    public NullableConverter(ArgumentConverter baseConverter)
    {
        BaseConverter = baseConverter ?? throw new ArgumentNullException(nameof(baseConverter));
    }

    /// <summary>
    /// Gets the converter for the underlying type.
    /// </summary>
    /// <value>
    /// The <see cref="ArgumentConverter"/> for the underlying type.
    /// </value>
    public ArgumentConverter BaseConverter { get; }

    /// <inheritdoc/>
    /// <returns>
    /// An object representing the converted value, or <see langword="null"/> if the value was an
    /// empty string span.
    /// </returns>
    public override object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        if (value.Length == 0)
        {
            return null;
        }

        return BaseConverter.Convert(value, culture, argument);
    }
}
