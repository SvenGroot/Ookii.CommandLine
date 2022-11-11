using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

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
            catch (TargetInvocationException ex)
            {
                throw new FormatException(ex.InnerException?.Message ?? ex.Message, ex);
            }
        }
    }
}