// Copyright (c) Sven Groot (Ookii.org)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Static class providing constants for <see cref="KeyValuePairConverter{TKey, TValue}"/>.
    /// </summary>
    public static class KeyValuePairConverter
    {
        /// <summary>
        /// Gets the default key/value separator, which is "=".
        /// </summary>
        public const string DefaultSeparator = "=";
    }

    /// <summary>
    /// Converts key-value pairs to and from strings using key=value notation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <remarks>
    /// <para>
    ///   This <see cref="TypeConverter"/> is used for dictionary command line arguments.
    /// </para>
    /// </remarks>
    public class KeyValuePairConverter<TKey, TValue> : TypeConverterBase<KeyValuePair<TKey, TValue?>>
    {
        private readonly TypeConverter _keyConverter;
        private readonly TypeConverter _valueConverter;
        private readonly string _argumentName;
        private readonly bool _allowNullValues;
        private readonly string _separator;
        private readonly LocalizedStringProvider _stringProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairConverter{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="stringProvider">Provides a <see cref="LocalizedStringProvider"/> to get error messages.</param>
        /// <param name="argumentName">The name of the argument that this converter is for.</param>
        /// <param name="allowNullValues">Indicates whether the value type accepts <see langword="null"/> values.</param>
        /// <param name="keyConverterType">Provides an optional <see cref="TypeConverter"/> type to use to convert keys.
        /// If <see langword="null"/>, the default converter for <typeparamref name="TKey"/> is used.</param>
        /// <param name="valueConverterType">Provides an optional <see cref="TypeConverter"/> type to use to convert values.
        /// If <see langword="null"/>, the default converter for <typeparamref name="TValue"/> is used.</param>
        /// <param name="separator">Provides an optional custom key/value separator. If <see langword="null" />, the value
        /// of <see cref="KeyValuePairConverter.DefaultSeparator"/> is used.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stringProvider"/> or <paramref name="argumentName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="separator"/> is an empty string.</exception>
        /// <exception cref="NotSupportedException">Either the key or value <see cref="TypeConverter"/> does not support converting from a string.</exception>
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
        /// <exception cref="NotSupportedException">Either the key or value <see cref="TypeConverter"/> does not support converting from a string.</exception>
        public KeyValuePairConverter()
            : this(new LocalizedStringProvider(), string.Empty, true, null, null, null)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Converts from a string to the type of this converter.
        /// </summary>
        /// <exception cref="FormatException">The <paramref name="value"/> could not be converted.</exception>
        protected override KeyValuePair<TKey, TValue?> Convert(ITypeDescriptorContext? context, CultureInfo? culture, string value)
        {
            int index = value.IndexOf(_separator);
            if (index < 0)
            {
                throw new FormatException(_stringProvider.MissingKeyValuePairSeparator(_separator));
            }

            string key = value.Substring(0, index);
            string valueForKey = value.Substring(index + _separator.Length);
            object? convertedKey = _keyConverter.ConvertFromString(context, culture, key);
            object? convertedValue = _valueConverter.ConvertFromString(context, culture, valueForKey);
            if (convertedKey == null || (!_allowNullValues && convertedValue == null))
            {
                throw _stringProvider.CreateException(CommandLineArgumentErrorCategory.NullArgumentValue, _argumentName);
            }

            return new((TKey)convertedKey, (TValue?)convertedValue);
        }

        /// <inheritdoc/>
        /// <returns>
        /// A string representing the object.
        /// </returns>
        protected override string? Convert(ITypeDescriptorContext? context, CultureInfo? culture, KeyValuePair<TKey, TValue?> value)
        {
            var key = _keyConverter.ConvertToString(context, culture, value.Key);
            var valueString = _keyConverter.ConvertToString(context, culture, value.Value);
            return key + _separator + valueString;
        }
    }
}
