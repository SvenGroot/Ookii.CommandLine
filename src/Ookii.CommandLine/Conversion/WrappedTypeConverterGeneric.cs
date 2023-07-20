using System.ComponentModel;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// An <see cref="ArgumentConverter"/> that wraps an existing <see cref="TypeConverter"/>.
/// </summary>
/// <typeparam name="T">The type of the <see cref="TypeConverter"/> to wrap.</typeparam>
/// <remarks>
/// <para>
///   This class will convert argument values from a string using the <see cref="TypeConverter"/>
///   class <typeparamref name="T"/>. If you wish to use the default <see cref="TypeConverter"/>
///   for a type, use the <see cref="WrappedDefaultTypeConverter{T}"/> class instead.
/// </para>
/// </remarks>
public class WrappedTypeConverter<T> : WrappedTypeConverter
    where T : TypeConverter, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedTypeConverter{T}"/> class.
    /// </summary>
    public WrappedTypeConverter()
        : base(new T())
    {
    }
}
