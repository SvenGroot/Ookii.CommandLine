// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Collections.Generic;
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
    private readonly string _argumentName;
    private readonly bool _allowNullValues;
    private readonly string _separator;
    private readonly LocalizedStringProvider _stringProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValuePairConverter{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="stringProvider">Provides a <see cref="LocalizedStringProvider"/> to get error messages.</param>
    /// <param name="argumentName">The name of the argument that this converter is for.</param>
    /// <param name="allowNullValues">Indicates whether the type of the pair's value accepts <see langword="null"/> values.</param>
    /// <param name="keyConverterType">Provides an optional <see cref="ArgumentConverter"/> type to use to convert keys.
    /// If <see langword="null"/>, the default converter for <typeparamref name="TKey"/> is used.</param>
    /// <param name="valueConverterType">Provides an optional <see cref="ArgumentConverter"/> type to use to convert values.
    /// If <see langword="null"/>, the default converter for <typeparamref name="TValue"/> is used.</param>
    /// <param name="separator">Provides an optional custom key/value separator. If <see langword="null" />, the value
    /// of <see cref="KeyValuePairConverter.DefaultSeparator"/> is used.</param>
    /// <exception cref="ArgumentNullException"><paramref name="stringProvider"/> or <paramref name="argumentName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="separator"/> is an empty string.</exception>
    /// <remarks>
    /// <para>
    ///   If either <paramref name="keyConverterType"/> or <paramref name="valueConverterType"/> is <see langword="null"/>,
    ///   conversion of those types is done using the rules outlined in the documentation for the <see cref="CommandLineArgument.ConvertToArgumentType(CultureInfo, string?)"/>
    ///   method.
    /// </para>
    /// </remarks>
    public KeyValuePairConverter(LocalizedStringProvider stringProvider, string argumentName, bool allowNullValues, Type? keyConverterType, Type? valueConverterType, string? separator)
    {
        _stringProvider = stringProvider ?? throw new ArgumentNullException(nameof(stringProvider));
        _argumentName = argumentName ?? throw new ArgumentNullException(nameof(argumentName));
        _allowNullValues = allowNullValues;
        _keyConverter = typeof(TKey).GetStringConverter(keyConverterType);
        _valueConverter = typeof(TValue).GetStringConverter(valueConverterType);
        _separator = separator ?? KeyValuePairConverter.DefaultSeparator;
        if (_separator.Length == 0)
        {
            throw new ArgumentException(Properties.Resources.EmptyKeyValueSeparator, nameof(separator));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValuePairConverter{TKey, TValue}"/> class.
    /// </summary>
    public KeyValuePairConverter()
        : this(new LocalizedStringProvider(), string.Empty, true, null, null, null)
    {
    }

    /// <inheritdoc />
    public override object? Convert(string value, CultureInfo culture)
        => Convert((value ?? throw new ArgumentNullException(nameof(value))).AsSpan(), culture);

    /// <inheritdoc />
    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture)
    {
        var (key, valueForKey) = value.SplitOnce(_separator.AsSpan(), out bool hasSeparator);
        if (!hasSeparator)
        {
            throw new FormatException(_stringProvider.MissingKeyValuePairSeparator(_separator));
        }

        object? convertedKey = _keyConverter.Convert(key, culture);
        object? convertedValue = _valueConverter.Convert(valueForKey, culture);
        if (convertedKey == null || !_allowNullValues && convertedValue == null)
        {
            throw _stringProvider.CreateException(CommandLineArgumentErrorCategory.NullArgumentValue, _argumentName);
        }

        return new KeyValuePair<TKey, TValue?>((TKey)convertedKey, (TValue?)convertedValue);
    }
}
