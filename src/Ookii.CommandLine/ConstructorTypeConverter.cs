using System;
using System.ComponentModel;
using System.Globalization;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Type converter used to instantiate argument types with a string constructor.
    /// </summary>
    internal class ConstructorTypeConverter : TypeConverterBase<object>
    {
        private readonly Type _type;

        public ConstructorTypeConverter(Type type)
        {
            _type = type;
        }

        protected override object? Convert(ITypeDescriptorContext? context, CultureInfo? culture, string value)
        {
            try
            {
                return _type.CreateInstance(value);
            }
            catch (Exception ex)
            {
                // Since we don't know what the constructor will throw, we'll wrap anything in a
                // FormatException.
                throw new FormatException(ex.Message, ex);
            }
        }
    }
}