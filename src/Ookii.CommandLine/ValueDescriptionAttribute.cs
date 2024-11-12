using System;
using System.ComponentModel;

namespace Ookii.CommandLine;

/// <summary>
/// Supplies a short description of an argument value to use when printing usage information.
/// </summary>
/// <remarks>
/// <para>
///   The value description is a short, typically one-word description that indicates the
///   type of value that the user should supply.
/// </para>
/// <para>
///   The value description is used when generating usage help. For example, the usage for an
///   argument named Sample with a value description of String would look like "-Sample &lt;String&gt;".
/// </para>
/// <para>
///   You can apply this attribute to a property or method defining an argument to set the value
///   description for that argument. You can also apply it to a type to set a default value
///   description for that type.
/// </para>
/// <para>
///   If this attribute is not present, the name of the type of the argument is used by default,
///   applying the name transformation specified by the
///   <see cref="ParseOptions.ValueDescriptionTransform" qualifyHint="true"/> property or the
///   <see cref="ParseOptionsAttribute.ValueDescriptionTransform" qualifyHint="true"/> property.
/// </para>
/// <para>
///   If this attribute is applied to a property or method, the name transformation is not applied
///   to the value description given by the attribute. However, when it is applied to a type, the
///   name transformation is applied.
/// </para>
/// <para>
///   If you want to override the value description for all arguments of a specific type, and
///   you cannot apply the <see cref="ValueDescriptionAttribute"/> to this type, you can
///   use the <see cref="ParseOptions.DefaultValueDescriptions" qualifyHint="true"/> property.
/// </para>
/// <para>
///   You can derive from this attribute to use an alternative source for the value description,
///   such as a resource table that can be localized.
/// </para>
/// <note>
///   This is not the long description used to describe the purpose of the argument. That can be set
///   using the <see cref="DescriptionAttribute"/> attribute.
/// </note>
/// </remarks>
/// <seealso cref="CommandLineArgument.ValueDescription" qualifyHint="true"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct
    | AttributeTargets.Enum)]
public class ValueDescriptionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueDescriptionAttribute"/> attribute.
    /// </summary>
    /// <param name="valueDescription">The value description for the argument.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="valueDescription"/> is <see langword="null"/>.
    /// </exception>
    public ValueDescriptionAttribute(string valueDescription)
    {
        ValueDescriptionValue = valueDescription ?? throw new ArgumentNullException(nameof(valueDescription));
    }

    /// <summary>
    /// Gets the value description for the argument.
    /// </summary>
    /// <value>
    /// The value description.
    /// </value>
    public virtual string ValueDescription => ValueDescriptionValue;

    /// <summary>
    /// Gets the value description stored in this attribute.
    /// </summary>
    /// <value>
    /// The value description.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The default implementation of the <see cref="ValueDescription"/> property returns the
    ///   value of this property.
    /// </para>
    /// </remarks>
    protected string ValueDescriptionValue { get; }
}
