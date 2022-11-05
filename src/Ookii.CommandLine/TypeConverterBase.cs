using System;
using System.ComponentModel;
using System.Globalization;

namespace Ookii.CommandLine
{
    public abstract class TypeConverterBase<T> : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                return Convert(context, culture, stringValue);
            }

            return base.ConvertFrom(context, culture, value);
        }

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

        protected abstract T? Convert(ITypeDescriptorContext? context, CultureInfo? culture, string value);

        protected virtual string? Convert(ITypeDescriptorContext? context, CultureInfo? culture, T value) => null;
    }
}