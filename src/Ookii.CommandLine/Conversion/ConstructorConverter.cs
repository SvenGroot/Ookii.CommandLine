using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Ookii.CommandLine.Conversion;

internal class ConstructorConverter : ArgumentConverter
{
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif

    private readonly Type _type;

    public ConstructorConverter(
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        Type type)
    {
        _type = type;
    }

    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument)
    {
        try
        {
            // Since we are passing BindingFlags.Public, the correct annotation is present.
            return Activator.CreateInstance(_type, value);
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
