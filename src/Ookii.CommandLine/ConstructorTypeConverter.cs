using System;
using System.ComponentModel;
using System.Globalization;

namespace Ookii.CommandLine
{
    internal class ConstructorTypeConverter : TypeConverterBase<object>
    {
        private readonly Type _type;

        public ConstructorTypeConverter(Type type)
        {
            _type = type;
        }

        protected override object? Convert(ITypeDescriptorContext? context, CultureInfo? culture, string value)
        {
            return _type.CreateInstance(value);
        }
    }
}