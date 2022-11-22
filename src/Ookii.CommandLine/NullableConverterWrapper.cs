using System;
using System.ComponentModel;
using System.Globalization;

namespace Ookii.CommandLine
{
    // Unfortunately the regular NullableConverter can't be used for this because it doesn't allow
    // the use of a custom TypeConverter. It otherwise behaves the same (converts an empty string
    // to null).
    internal class NullableConverterWrapper : TypeConverter
    {
        private readonly Type _underlyingType;
        private readonly TypeConverter _baseConverter;

        public NullableConverterWrapper(Type underlyingType, TypeConverter baseConverter)
        {
            _underlyingType = underlyingType;
            _baseConverter = baseConverter;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string) || sourceType == _underlyingType || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value == null || value.GetType() == _underlyingType)
            {
                return value;
            }

            if (value is string stringValue && stringValue.Length == 0)
            {
                return null;
            }

            return _baseConverter.ConvertFrom(context, culture, value);
        }
    }
}
