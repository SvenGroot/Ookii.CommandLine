using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// An <see cref="ArgumentConverter"/> that wraps the default <see cref="TypeConverter"/> for a
/// type.
/// </summary>
/// <typeparam name="T">The type to convert to.</typeparam>
#if NET6_0_OR_GREATER
[RequiresUnreferencedCode("Determining the TypeConverter for a type may require the type to be annotated.")]
#endif
public class TypeConverterArgumentConverter<T> : TypeConverterArgumentConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeConverterArgumentConverter{T}"/> class.
    /// </summary>
    public TypeConverterArgumentConverter()
        : base(TypeDescriptor.GetConverter(typeof(T)))
    {
    }
}
