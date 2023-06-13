// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Static class providing constants for the <see cref="KeyValuePairConverter{TKey, TValue}"/>
/// class.
/// </summary>
public static class KeyValuePairConverter
{
    /// <summary>
    /// Gets the default key/value separator, which is "=".
    /// </summary>
    public const string DefaultSeparator = "=";
}

/// <summary>
/// Converts key-value pairs to and from strings using "key=value" notation.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <remarks>
/// <para>
///   This <see cref="ArgumentConverter"/> is used for dictionary command line arguments by default.
/// </para>
/// </remarks>
public class KeyValuePairConverter<TKey, TValue> : ArgumentConverter
{
    private readonly ArgumentConverter _keyConverter;
    private readonly ArgumentConverter _valueConverter;
    private readonly bool _allowNullValues;
    private readonly string _separator;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValuePairConverter{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="keyConverter">
    /// Provides the <see cref="ArgumentConverter"/> used to convert the key/value pair's keys.
    /// </param>
    /// <param name="valueConverter">
    /// Provides the <see cref="ArgumentConverter"/> used to convert the key/value pair's values.
    /// </param>
    /// <param name="separator">
    /// Provides an optional custom key/value separator. If <see langword="null" />, the value
    /// of <see cref="KeyValuePairConverter.DefaultSeparator"/> is used.
    /// </param>
    /// <param name="allowNullValues">
    /// Indicates whether the type of the pair's value accepts <see langword="null"/> values.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="keyConverter"/> or <paramref name="valueConverter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="separator"/> is an empty string.
    /// </exception>
    public KeyValuePairConverter(ArgumentConverter keyConverter, ArgumentConverter valueConverter, string? separator, bool allowNullValues)
    {
        _allowNullValues = allowNullValues;
        _keyConverter = keyConverter ?? throw new ArgumentNullException(nameof(keyConverter));
        _valueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
        _separator = separator ?? KeyValuePairConverter.DefaultSeparator;
        if (_separator.Length == 0)
        {
            throw new ArgumentException(Properties.Resources.EmptyKeyValueSeparator, nameof(separator));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValuePairConverter{TKey, TValue}"/> class.
    /// </summary>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Key and value converters cannot be statically determined.")]
#endif
    public KeyValuePairConverter()
        : this(typeof(TKey).GetStringConverter(null), typeof(TValue).GetStringConverter(null), null, true)
    {
    }

    /// <inheritdoc />
    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument)
        => Convert((value ?? throw new ArgumentNullException(nameof(value))).AsSpan(), culture, argument);

    /// <inheritdoc />
    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        var (key, valueForKey) = value.SplitOnce(_separator.AsSpan(), out bool hasSeparator);
        if (!hasSeparator)
        {
            throw new FormatException(argument.Parser.StringProvider.MissingKeyValuePairSeparator(_separator));
        }

        var convertedKey = _keyConverter.Convert(key, culture, argument);
        var convertedValue = _valueConverter.Convert(valueForKey, culture, argument);
        if (convertedKey == null || !_allowNullValues && convertedValue == null)
        {
            throw argument.Parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.NullArgumentValue,
                argument.ArgumentName);
        }

        return new KeyValuePair<TKey, TValue?>((TKey)convertedKey, (TValue?)convertedValue);
    }
}
