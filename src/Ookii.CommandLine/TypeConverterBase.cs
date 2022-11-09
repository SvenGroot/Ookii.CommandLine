using System;
using System.ComponentModel;
using System.Globalization;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Base class to help with implementing a <see cref="TypeConverter"/> that can convert to/from
    /// a <see cref="string"/>.
    /// </summary>
    /// <typeparam name="T">The type of object that can be converted to/from a string.</typeparam>
    /// <remarks>
    /// <para>
    ///   This class handles checking whether the source or destination type is a string, and calls
    ///   strongly typed conversion methods that inheritors can implement.
    /// </para>
    /// <para>
    ///   For the <see cref="TypeConverter.CanConvertTo(ITypeDescriptorContext?, Type?)"/> method,
    ///   it relies on the fact that the base <see cref="TypeConverter"/> implementation already
    ///   returns <see langword="true"/> for the <see cref="string"/> type.
    /// </para>
    /// </remarks>
    public abstract class TypeConverterBase<T> : TypeConverter
    {
        /// <inheritdoc/>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="sourceType"/> is <see cref="string"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>
        ///   If the <paramref name="value"/> is an instance of the <see cref="string"/> type, this
        ///   method calls <see cref="Convert(ITypeDescriptorContext?, CultureInfo?, string)"/>.
        ///   Otherwise, it calls the base <see cref="TypeConverter.ConvertFrom(ITypeDescriptorContext?, CultureInfo?, object)"/>
        ///   method.
        /// </para>
        /// </remarks>
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                return Convert(context, culture, stringValue);
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>
        ///   If the <paramref name="destinationType"/> is <see cref="string"/>, this method will
        ///   call the <see cref="Convert(ITypeDescriptorContext?, CultureInfo?, T)"/> method. Otherwise,
        ///   the base <see cref="TypeConverter.ConvertTo(ITypeDescriptorContext?, CultureInfo?, object?, Type)"/>
        ///   method is called.
        /// </para>
        /// <para>
        ///   If the <see cref="Convert(ITypeDescriptorContext?, CultureInfo?, T)"/> method returns
        ///   <see langword="null"/>, conversion falls back to the base <see cref="TypeConverter.ConvertTo(ITypeDescriptorContext?, CultureInfo?, object?, Type)"/>
        ///   method, which uses <see cref="object.ToString"/> to convert to a <see cref="string"/>.
        /// </para>
        /// </remarks>
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is T typedValue && destinationType == typeof(string))
            {
                var converted = Convert(context, culture, typedValue);
                if (converted != null)
                {
                    return converted;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// When implemented in a derived class, converts from a string to the type of this
        /// converter.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides format context.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> to use for the conversion.</param>
        /// <param name="value">The <see cref="string"/> value to convert.</param>
        /// <returns>The converted object.</returns>
        protected abstract T? Convert(ITypeDescriptorContext? context, CultureInfo? culture, string value);

        /// <summary>
        /// Converts the type of this converter to a string.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides format context.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> to use for the conversion.</param>
        /// <param name="value">The object to convert.</param>
        /// <returns>
        /// A string representing the object, or <see langword="null"/> if the caller should fall
        /// back to using <see cref="object.ToString"/>. The base class implementation always returns
        /// <see langword="null"/>.
        /// </returns>
        protected virtual string? Convert(ITypeDescriptorContext? context, CultureInfo? culture, T value) => null;
    }
}