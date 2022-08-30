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
        private static readonly TypeConverter _keyConverter = GetConverter(typeof(TKey));
        private static readonly TypeConverter _valueConverter = GetConverter(typeof(TValue));

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
                int index = stringValue.IndexOf('=');
                if( index < 0 )
                    throw new FormatException(Properties.Resources.NoKeyValuePairSeparator);
                string key = stringValue.Substring(0, index);
                string valueForKey = stringValue.Substring(index + 1);
                return new KeyValuePair<TKey, TValue?>((TKey)_keyConverter.ConvertFromString(context, culture, key)!, (TValue?)_valueConverter.ConvertFromString(context, culture, valueForKey));
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

        private static TypeConverter GetConverter(Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if( converter == null || !(converter.CanConvertFrom(typeof(string)) && converter.CanConvertTo(typeof(string))) )
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.NoTypeConverterFormat, type));
            return converter;
        }    
    }
}
