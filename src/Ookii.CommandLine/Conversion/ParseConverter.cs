using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Ookii.CommandLine.Conversion;

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
        catch (TargetInvocationException ex)
        {
            if (ex.InnerException == null)
            {
                throw;
            }

            // Rethrow inner exception with original call stack.
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();

            // Actually unreachable.
            throw;
        }
    }
}
