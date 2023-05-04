using System;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// Sets the namespace to use for argument converters generated for arguments classes with the
/// <see cref="GeneratedParserAttribute"/> attribute.
/// </summary>
/// <remarks>
/// <para>
///   To convert argument types for which no built-in non-reflection argument converter exists,
///   such as classes that have a constructor taking a <see cref="string"/> parameter, or those
///   that have a <c>Parse</c> method but don't implement <see cref="IParsable{TSelf}"/>, the source
///   generator will create a new argument converter. The generated converter class will be
///   internal to the assembly containing the generated parser, and will be placed in the namespace
///   <c>Ookii.CommandLine.Conversion.Generated</c> by default.
/// </para>
/// <para>
///   Use this attribute to modify the namespace used.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class GeneratedConverterNamespaceAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratedConverterNamespaceAttribute"/> class
    /// with the specified namespace.
    /// </summary>
    /// <param name="namespace">The namespace to use.</param>
    public GeneratedConverterNamespaceAttribute(string @namespace)
    {
        Namespace = @namespace;
    }

    /// <summary>
    /// Gets the namespace to use for generated argument converters.
    /// </summary>
    /// <value>
    /// The full name of the namespace.
    /// </value>
    public string Namespace { get; }
}
