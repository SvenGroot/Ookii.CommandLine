using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Type converter for types with a public static Parse method.
    /// </summary>
    internal class ParseTypeConverter : TypeConverterBase<object?>
    {
        private readonly MethodInfo _method;
        private readonly bool _hasCulture;

        public ParseTypeConverter(MethodInfo method, bool hasCulture)
        {
            _method = method;
            _hasCulture = hasCulture;
        }

        protected override object? Convert(ITypeDescriptorContext? context, CultureInfo? culture, string value)
        {
            var parameters = _hasCulture
                ? new object?[] { value, culture }
                : new object?[] { value };

            try
            {
                return _method.Invoke(null, parameters);
            }
            catch (Exception ex)
            {
                // Since we don't know what the method will throw, we'll wrap anything in a
                // FormatException.
                throw new FormatException(ex.Message, ex);
            }
        }
    }
}
