using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Specifies a <see cref="ArgumentConverter"/> to use for the keys of a dictionary argument.
/// </summary>
/// <remarks>
/// <para>
///   This attribute can be used along with the <see cref="KeyConverterAttribute"/> and 
///   <see cref="KeyValueSeparatorAttribute"/> attribute to customize the parsing of a dictionary
///   argument without having to write a custom <see cref="ArgumentConverter"/> that returns a
///   <see cref="KeyValuePair{TKey, TValue}"/>.
/// </para>
/// <para>
///   The type specified by this attribute must derive from the <see cref="ArgumentConverter"/>
///   class.
/// </para>
/// <para>
///   This attribute is ignored if the argument uses the <see cref="ArgumentConverterAttribute"/>
///   or if the argument is not a dictionary argument.
/// </para>
/// </remarks>
/// <seealso cref="KeyValuePairConverter{TKey, TValue}"/>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class ValueConverterAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueConverterAttribute"/> class with the
    /// specified converter type.
    /// </summary>
    /// <param name="converterType">
    ///   The <see cref="Type"/> to use as a converter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="converterType"/> is <see langword="null"/>
    /// </exception>
#if NET6_0_OR_GREATER
    public ValueConverterAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type converterType)
#else
    public ValueConverterAttribute(Type converterType)
#endif
    {
        ConverterTypeName = converterType?.AssemblyQualifiedName ?? throw new ArgumentNullException(nameof(converterType));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueConverterAttribute"/> class with the
    /// specified converter type name.
    /// </summary>
    /// <param name="converterTypeName">
    ///   The fully qualified name of the <see cref="Type"/> to use as a converter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="converterTypeName"/> is <see langword="null"/>
    /// </exception>
#if NET6_0_OR_GREATER
    public ValueConverterAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] string converterTypeName)
#else
    public ValueConverterAttribute(string converterTypeName)
#endif
    {
        ConverterTypeName = converterTypeName ?? throw new ArgumentNullException(nameof(converterTypeName));
    }

    /// <summary>
    /// Gets the fully qualified name of the <see cref="Type"/> to use as a converter.
    /// </summary>
    /// <value>
    /// The fully qualified name of the <see cref="Type"/> to use as a converter.
    /// </value>
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
    public string ConverterTypeName { get; }

    internal Type GetConverterType()
    {
        return Type.GetType(ConverterTypeName, true)!;
    }
}
