using Ookii.Common;
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
/// <para>
///   The behavior of this converter can be customized by applying the <see cref="KeyConverterAttribute"/>,
///   <see cref="ValueConverterAttribute"/> or <see cref="KeyValueSeparatorAttribute"/> attribute
///   to the property or method defining a dictionary argument.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public class KeyValuePairConverter<TKey, TValue> : ArgumentConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValuePairConverter{TKey, TValue}"/> class
    /// with the specified key and value converters and options.
    /// </summary>
    /// <param name="keyConverter">
    /// The <see cref="ArgumentConverter"/> used to convert the key/value pair's keys.
    /// </param>
    /// <param name="valueConverter">
    /// The <see cref="ArgumentConverter"/> used to convert the key/value pair's values.
    /// </param>
    /// <param name="separator">
    /// An optional custom key/value separator. If <see langword="null" />, the value
    /// of <see cref="KeyValuePairConverter.DefaultSeparator" qualifyHint="true"/> is used.
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
        AllowNullValues = allowNullValues;
        KeyConverter = keyConverter ?? throw new ArgumentNullException(nameof(keyConverter));
        ValueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
        Separator = separator ?? KeyValuePairConverter.DefaultSeparator;
        if (Separator.Length == 0)
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Creating key and value converters may require dynamic code.")]
#endif
    public KeyValuePairConverter()
        : this(typeof(TKey).GetStringConverter(null), typeof(TValue).GetStringConverter(null), null,
              !typeof(TValue).IsValueType || typeof(TValue).IsNullableValueType())
    {
    }

    /// <summary>
    /// Gets the converter used for the keys of the key/value pair.
    /// </summary>
    /// <remarks>
    /// The <see cref="ArgumentConverter"/> used for the keys.
    /// </remarks>
    public ArgumentConverter KeyConverter { get; }

    /// <summary>
    /// Gets the converter used for the values of the key/value pair.
    /// </summary>
    /// <remarks>
    /// The <see cref="ArgumentConverter"/> used for the values.
    /// </remarks>
    public ArgumentConverter ValueConverter { get; }

    /// <summary>
    /// Gets the key/value separator.
    /// </summary>
    /// <value>
    /// The string used to separate the key and value in a key/value pair.
    /// </value>
    public string Separator { get; }

    /// <summary>
    /// Gets a value which indicates whether the values of the key/value pair can be
    /// <see langword="null"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if <see langword="null"/> values are allowed; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property should only be true if <typeparamref name="TValue"/> is a value type other
    ///   than <see cref="Nullable{T}"/> or a reference type without a nullable annotation.
    /// </para>
    /// <para>
    ///   The keys of a key/value pair can never be <see langword="null"/>.
    /// </para>
    /// </remarks>
    public bool AllowNullValues { get; }

    /// <summary>
    /// Converts a string memory region to a <see cref="KeyValuePair{TKey, TValue}"/>.
    /// </summary>
    /// <param name="value">The <see cref="ReadOnlyMemory{T}"/> containing the string to convert.</param>
    /// <param name="culture">The culture to use for the conversion.</param>
    /// <param name="argument">
    /// The <see cref="CommandLineArgument"/> that will use the converted value.
    /// </param>
    /// <returns>An object representing the converted value.</returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="culture"/> or <paramref name="argument"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   The value was not in a correct format for the target type.
    /// </exception>
    /// <exception cref="CommandLineArgumentException">
    ///   The value was not in a correct format for the target type.
    /// </exception>
    public override object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(nameof(argument));
        }

        if (value.SplitOnce(Separator.AsSpan(), StringComparison.Ordinal) is not (ReadOnlyMemory<char>, ReadOnlyMemory<char>) splits)
        {
            throw new FormatException(argument.Parser.StringProvider.MissingKeyValuePairSeparator(Separator));
        }

        var convertedKey = KeyConverter.Convert(splits.Item1, culture, argument);
        var convertedValue = ValueConverter.Convert(splits.Item2, culture, argument);
        if (convertedKey == null || !AllowNullValues && convertedValue == null)
        {
            throw argument.Parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.NullArgumentValue,
                argument.ArgumentName);
        }

        return new KeyValuePair<TKey, TValue?>((TKey)convertedKey, (TValue?)convertedValue);
    }
}
