using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// An <see cref="ArgumentConverter"/> that wraps the default <see cref="TypeConverter"/> for a
/// type.
/// </summary>
/// <typeparam name="T">The type to convert to.</typeparam>
/// <para>
///   This class will convert argument values from a string using the default <see cref="TypeConverter"/>
///   for the type <typeparamref name="T"/>. If you wish to use a specific custom <see cref="TypeConverter"/>,
///   use the <see cref="WrappedTypeConverter{T}"/> class instead.
/// </para>
/// <threadsafety static="true" instance="false"/>
#if NET6_0_OR_GREATER
[RequiresUnreferencedCode("Determining the TypeConverter for a type may require the type to be annotated.")]
#endif
public class WrappedDefaultTypeConverter<T> : WrappedTypeConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedDefaultTypeConverter{T}"/> class.
    /// </summary>
    public WrappedDefaultTypeConverter()
        : base(TypeDescriptor.GetConverter(typeof(T)))
    {
    }
}
