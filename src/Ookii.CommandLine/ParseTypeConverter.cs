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

            return _method.Invoke(null, parameters);
        }
    }
}
