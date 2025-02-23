using System;
using System.Diagnostics.CodeAnalysis;

namespace Ookii.CommandLine;

/// <summary>
/// Helper type that allows the creation of a <see cref="Boolean"/> argument that is not a switch.
/// </summary>
/// <remarks>
/// <para>
///   A command line argument using this type will be a regular named argument that requires an
///   explicit value of either "true" or "false". This is contrary to switch arguments, which do
///   not require a value.
/// </para>
/// <para>
///   There is no need to use the <see cref="NonSwitchBoolean"/> structure with positional
///   arguments, as a positional argument cannot be a switch even if its type is
///   <see cref="Boolean"/>.
/// </para>
/// </remarks>
[ValueDescription(nameof(Boolean), ApplyTransform = true)]
public struct NonSwitchBoolean
#if NET7_0_OR_GREATER
    : ISpanParsable<NonSwitchBoolean>
#endif
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NonSwitchBoolean"/> structure.
    /// </summary>
    /// <param name="value">The boolean value for the structure.</param>
    public NonSwitchBoolean(bool value) => Value = value;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// Either <see langword="true"/> or <see langword="false"/>.
    /// </value>
    public bool Value { get; set; }

    /// <summary>
    /// Converts the specified string to a <see cref="NonSwitchBoolean"/> value.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> is equivalent to <see cref="bool.TrueString"/>;
    /// <see langword="false"/> if <paramref name="value"/> is equivalent to <see cref="bool.FalseString"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="value"/> is not a valid boolean value.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This method uses the <see cref="bool.Parse(string)" qualifyHint="true"/> method to perform
    ///   the conversion.
    /// </para>
    /// </remarks>
    public static NonSwitchBoolean Parse(string value) => new(bool.Parse(value));

    /// <summary>
    /// Tries to convert the specified string to a <see cref="NonSwitchBoolean"/> value.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">
    /// If conversion was successful, receives <see langword="true"/> if <paramref name="value"/> is
    /// equivalent to <see cref="bool.TrueString"/>; <see langword="false"/> if <paramref name="value"/>
    /// is equivalent to <see cref="bool.FalseString"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the conversion was successful; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   This method uses the <see cref="bool.TryParse(string?, out bool)" qualifyHint="true"/>
    ///   method to perform the conversion.
    /// </para>
    /// </remarks>
    public static bool TryParse(
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        [NotNullWhen(true)]
#endif
        string? value, out NonSwitchBoolean result)
    {
        if (bool.TryParse(value, out bool boolValue))
        {
            result = boolValue;
            return true;
        }

        result = default;
        return false;
    }

#if NET6_0_OR_GREATER

    /// <summary>
    /// Converts the specified string span to a <see cref="NonSwitchBoolean"/> value.
    /// </summary>
    /// <param name="value">The string span to convert.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> is equivalent to <see cref="bool.TrueString"/>;
    /// <see langword="false"/> if <paramref name="value"/> is equivalent to <see cref="bool.FalseString"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="value"/> is not a valid boolean value.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This method uses the <see cref="bool.Parse(ReadOnlySpan{char})" qualifyHint="true"/>
    ///   method to perform the conversion.
    /// </para>
    /// </remarks>
    public static NonSwitchBoolean Parse(ReadOnlySpan<char> value) => new(bool.Parse(value));

    /// <summary>
    /// Tries to convert the specified string span to a <see cref="NonSwitchBoolean"/> value.
    /// </summary>
    /// <param name="value">The string span to convert.</param>
    /// <param name="result">
    /// If conversion was successful, receives <see langword="true"/> if <paramref name="value"/> is
    /// equivalent to <see cref="bool.TrueString"/>; <see langword="false"/> if <paramref name="value"/>
    /// is equivalent to <see cref="bool.FalseString"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the conversion was successful; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   This method uses the <see cref="bool.TryParse(ReadOnlySpan{char}, out bool)" qualifyHint="true"/>
    ///   method to perform the conversion.
    /// </para>
    /// </remarks>
    public static bool TryParse(ReadOnlySpan<char> value, out NonSwitchBoolean result)
    {
        if (bool.TryParse(value, out bool boolValue))
        {
            result = boolValue;
            return true;
        }

        result = default;
        return false;
    }

#endif

#if NET7_0_OR_GREATER

    static NonSwitchBoolean ISpanParsable<NonSwitchBoolean>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => Parse(s);

    static NonSwitchBoolean IParsable<NonSwitchBoolean>.Parse(string s, IFormatProvider? provider)
        => Parse(s);

    static bool ISpanParsable<NonSwitchBoolean>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out NonSwitchBoolean result)
        => TryParse(s, out result);

    static bool IParsable<NonSwitchBoolean>.TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out NonSwitchBoolean result)
        => TryParse(s, out result);

#endif

    /// <summary>
    /// Converts a <see cref="NonSwitchBoolean"/> structure to a <see cref="Boolean"/>.
    /// </summary>
    /// <param name="value">The <see cref="NonSwitchBoolean"/> to convert.</param>
    /// <returns>The value of the <see cref="Value"/> property.</returns>
    public static implicit operator bool(NonSwitchBoolean value) => value.Value;

    /// <summary>
    /// Converts a <see cref="Boolean"/> to a <see cref="NonSwitchBoolean"/>.
    /// </summary>
    /// <param name="value">The <see cref="Boolean"/> value to convert.</param>
    /// <returns>
    /// A new instance of the <see cref="NonSwitchBoolean"/> structure with the specified value.
    /// </returns>
    public static implicit operator NonSwitchBoolean(bool value) => new(value);
}
