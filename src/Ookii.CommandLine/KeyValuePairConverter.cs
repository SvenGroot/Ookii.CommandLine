// Copyright (c) Sven Groot (Ookii.org)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at https://github.com/SvenGroot/ookii.commandline. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
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
    public class KeyValuePairConverter<TKey, TValue> : TypeConverter
    {
        private readonly TypeConverter _keyConverter;
        private readonly TypeConverter _valueConverter;
        private readonly string _argumentName;
        private readonly bool _allowNullValues;
        private readonly string _separator;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairConverter{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="argumentName">The name of the argument that this converter is for.</param>
        /// <param name="allowNullValues">Indicates whether the value type accepts <see langword="null"/> values.</param>
        /// <param name="keyConverterType">Provides an optional <see cref="TypeConverter"/> type to use to convert keys.
        /// If <see langword="null"/>, the default converter for <typeparamref name="TKey"/> is used.</param>
        /// <param name="valueConverterType">Provides an optional <see cref="TypeConverter"/> type to use to convert values.
        /// If <see langword="null"/>, the default converter for <typeparamref name="TValue"/> is used.</param>
        /// <param name="separator">Provides an optional custom key/value separator. If <see langword="null" />, the value
        /// of <see cref="KeyValuePairConverter.DefaultSeparator"/> is used.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argumentName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="separator"/> is an empty string.</exception>
        /// <exception cref="NotSupportedException">Either the key or value <see cref="TypeConverter"/> does not support converting from a string.</exception>
        public KeyValuePairConverter(string argumentName, bool allowNullValues, Type? keyConverterType, Type? valueConverterType, string? separator)
        {
            _argumentName = argumentName ?? throw new ArgumentNullException(nameof(argumentName));
            _allowNullValues = allowNullValues;
            _keyConverter = GetConverter(keyConverterType, typeof(TKey));
            _valueConverter = GetConverter(valueConverterType, typeof(TValue));
            _separator = separator ?? KeyValuePairConverter.DefaultSeparator;
            if (_separator.Length == 0)
                throw new ArgumentException(Properties.Resources.EmptyKeyValueSeparator, nameof(separator));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairConverter{TKey, TValue}"/> class.
        /// </summary>
        /// <exception cref="NotSupportedException">Either the key or value <see cref="TypeConverter"/> does not support converting from a string.</exception>
        public KeyValuePairConverter()
            : this(string.Empty, true, null, null, null)
        {
        }

        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="T:System.Type"/> that represents the type you want to convert from.</param>
        /// <returns>
        /// <see langword="true"/> if this converter can perform the conversion; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if( sourceType == typeof(string) )
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Returns whether this converter can convert the object to the specified type, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="destinationType">A <see cref="T:System.Type"/> that represents the type you want to convert to.</param>
        /// <returns>
        /// <see langword="true"/> if this converter can perform the conversion; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            if( destinationType == typeof(string) )
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture.</param>
        /// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the converted value.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// The conversion cannot be performed.
        /// </exception>
        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            var stringValue = value as string;
            if( stringValue != null )
            {
                int index = stringValue.IndexOf(_separator);
                if( index < 0 )
                    throw new FormatException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.NoKeyValuePairSeparatorFormat, _separator));

                string key = stringValue.Substring(0, index);
                string valueForKey = stringValue.Substring(index + _separator.Length);
                object? convertedKey = _keyConverter.ConvertFromString(context, culture, key);
                object? convertedValue = _valueConverter.ConvertFromString(context, culture, valueForKey);
                if (convertedKey == null || (!_allowNullValues && convertedValue == null))
                    throw new CommandLineArgumentException(String.Format(CultureInfo.CurrentCulture, Properties.Resources.NullArgumentValueFormat, _argumentName), _argumentName, CommandLineArgumentErrorCategory.NullArgumentValue);

                return new KeyValuePair<TKey, TValue?>((TKey)convertedKey, (TValue?)convertedValue);
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"/>. If null is passed, the current culture is assumed.</param>
        /// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
        /// <param name="destinationType">The <see cref="T:System.Type"/> to convert the <paramref name="value"/> parameter to.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the converted value.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="destinationType"/> parameter is null.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The conversion cannot be performed.
        ///   </exception>
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if( destinationType == null )
                throw new ArgumentNullException(nameof(destinationType));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            KeyValuePair<TKey, TValue> pair = (KeyValuePair<TKey, TValue>)value;
            if( destinationType == typeof(string) )
                return _keyConverter.ConvertToString(context, culture, pair.Key) + "=" + _valueConverter.ConvertToString(context, culture, pair.Value);

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static TypeConverter GetConverter(Type? converterType, Type type)
        {
            TypeConverter converter = converterType == null ? TypeDescriptor.GetConverter(type) : (TypeConverter)Activator.CreateInstance(converterType)!;
            if( converter == null || !(converter.CanConvertFrom(typeof(string)) && converter.CanConvertTo(typeof(string))) )
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.NoTypeConverterFormat, type));
            return converter;
        }    
    }
}
