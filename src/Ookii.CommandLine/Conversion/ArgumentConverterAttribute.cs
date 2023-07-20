using System;
using System.Diagnostics.CodeAnalysis;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Specifies a custom <see cref="ArgumentConverter"/> to use for converting the value of an
/// argument from a string.
/// </summary>
/// <remarks>
/// <para>
///   The type specified by this attribute must derive from the <see cref="ArgumentConverter"/>
///   class, and must convert to the type of the argument the attribute is applied to.
/// </para>
/// <para>
///   Apply this attribute to the property or method defining an argument to use a custom
///   conversion from a string to the type of the argument.
/// </para>
/// <para>
///   If this attribute is not present, the default conversion will be used.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class ArgumentConverterAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentConverterAttribute"/> class with the
    /// specified converter type.
    /// </summary>
    /// <param name="converterType">
    ///   The <see cref="Type"/> to use as a converter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="converterType"/> is <see langword="null"/>
    /// </exception>
#if NET6_0_OR_GREATER
    public ArgumentConverterAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type converterType)
#else
    public ArgumentConverterAttribute(Type converterType)
#endif
    {
        ConverterTypeName = converterType?.AssemblyQualifiedName ?? throw new ArgumentNullException(nameof(converterType));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentConverterAttribute"/> class with the
    /// specified converter type name.
    /// </summary>
    /// <param name="converterTypeName">
    ///   The fully qualified name of the <see cref="Type"/> to use as a converter.
    /// </param>
    /// <remarks>
    /// <para>
    ///   This constructor is not compatible with the <see cref="GeneratedParserAttribute"/>;
    ///   use the <see cref="ArgumentConverterAttribute(Type)"/> constructor instead.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="converterTypeName"/> is <see langword="null"/>
    /// </exception>
#if NET6_0_OR_GREATER
    public ArgumentConverterAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] string converterTypeName)
#else
    public ArgumentConverterAttribute(string converterTypeName)
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
