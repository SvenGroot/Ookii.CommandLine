using System;
using System.Globalization;
using System.Reflection;

namespace Ookii.CommandLine.Conversion
{
    internal class ParseConverter : ArgumentConverter
    {
        private readonly MethodInfo _method;
        private readonly bool _hasCulture;

        public ParseConverter(MethodInfo method, bool hasCulture)
        {
            _method = method;
            _hasCulture = hasCulture;
        }

        public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument)
        {
            var parameters = _hasCulture
                ? new object?[] { value, culture }
                : new object?[] { value };

            try
            {
                return _method.Invoke(null, parameters);
            }
            catch (CommandLineArgumentException ex)
            {
                // Patch the exception with the argument name.
                throw new CommandLineArgumentException(ex.Message, argument.ArgumentName, ex.Category, ex.InnerException);
            }
            catch (FormatException)
            {
                throw;
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
