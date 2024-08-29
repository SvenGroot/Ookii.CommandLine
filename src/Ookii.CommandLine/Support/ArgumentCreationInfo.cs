using System.Collections.Generic;
using System.ComponentModel;
using System;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;

namespace Ookii.CommandLine.Support;

/// <summary>
/// Provides information needed to create an instance of the <see cref="GeneratedArgumentBase"/>
/// class.
/// </summary>
/// <remarks>
/// This structure is used by the source generator when using the <see cref="GeneratedParserAttribute"/>
/// attribute. It should not normally be used by other code.
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public struct ArgumentCreationInfo
{
    /// <summary>
    /// Gets or sets the parser for this argument.
    /// </summary>
    /// <value></value>
    public CommandLineParser Parser { get; set; }

    /// <summary>
    /// Gets or sets the type of the argument.
    /// </summary>
    /// <value></value>
    public Type ArgumentType { get; set; }

    /// <summary>
    /// Gets or sets the element type of the argument, without <see cref="Nullable{T}"/>.
    /// </summary>
    /// <value></value>
    public Type ElementType { get; set; }

    /// <summary>
    /// Gets or sets the element type of the argument, including <see cref="Nullable{T}"/> if it is
    /// one.
    /// </summary>
    /// <value></value>
    public Type ElementTypeWithNullable { get; set; }

    /// <summary>
    /// Gets or sets the name of the property or method that defined the argument.
    /// </summary>
    /// <value></value>
    public string MemberName { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="CommandLineArgumentAttribute"/>.
    /// </summary>
    /// <value></value>
    public CommandLineArgumentAttribute Attribute { get; set; }

    /// <summary>
    /// Gets or sets the argument kind.
    /// </summary>
    /// <value>One of the <see cref="ArgumentKind"/> values.</value>
    public ArgumentKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the argument converter.
    /// </summary>
    /// <value></value>
    public ArgumentConverter Converter { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the argument allows null values.
    /// </summary>
    /// <value></value>
    public bool AllowsNull { get; set; }

    /// <summary>
    /// Gets or sets the default value description.
    /// </summary>
    public string DefaultValueDescription { get; set; }

    /// <summary>
    /// Gets or sets the implicit position if <see cref="CommandLineArgumentAttribute.IsPositional" qualityHint="true"/>
    /// was used.
    /// </summary>
    /// <value></value>
    public int? Position { get; set; }

    /// <summary>
    /// Gets or sets the default value description for the key type.
    /// </summary>
    /// <value></value>
    public string? DefaultKeyDescription { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the argument was defined by a C# 11 required
    /// property.
    /// </summary>
    /// <value></value>
    public bool RequiredProperty { get; set; }

    /// <summary>
    /// Gets or sets the default value determined from the property initializer.
    /// </summary>
    /// <value></value>
    public object? AlternateDefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the key type of a dictionary argument.
    /// </summary>
    /// <value></value>
    public Type? KeyType { get; set; }

    /// <summary>
    /// Gets or sets the value type of a dictionary argument.
    /// </summary>
    /// <value></value>
    public Type? ValueType { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="MultiValueSeparatorAttribute"/>.
    /// </summary>
    /// <value></value>
    public MultiValueSeparatorAttribute? MultiValueSeparatorAttribute { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DescriptionAttribute"/>.
    /// </summary>
    /// <value></value>
    public DescriptionAttribute? DescriptionAttribute { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ValueDescriptionAttribute"/>.
    /// </summary>
    /// <value></value>
    public ValueDescriptionAttribute? ValueDescriptionAttribute { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether a dictionary argument allows duplicate keys.
    /// </summary>
    /// <value></value>
    public bool AllowDuplicateDictionaryKeys { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="KeyValueSeparatorAttribute"/>.
    /// </summary>
    /// <value></value>
    public KeyValueSeparatorAttribute? KeyValueSeparatorAttribute { get; set; }

    /// <summary>
    /// Gets or sets a list of <see cref="AliasAttribute"/> attributes.
    /// </summary>
    /// <value></value>
    public IEnumerable<AliasAttribute>? AliasAttributes { get; set; }

    /// <summary>
    /// Gets or sets a list of <see cref="ShortAliasAttribute"/> attributes.
    /// </summary>
    /// <value></value>
    public IEnumerable<ShortAliasAttribute>? ShortAliasAttributes { get; set; }

    /// <summary>
    /// Gets or sets a list of <see cref="ArgumentValidationAttribute"/> attributes.
    /// </summary>
    /// <value></value>
    public IEnumerable<ArgumentValidationAttribute>? ValidationAttributes { get; set; }

    /// <summary>
    /// Gets or sets a delegate used to set the value of property that defined the argument.
    /// </summary>
    /// <value></value>
    public Action<object, object?>? SetProperty { get; set; }

    /// <summary>
    /// Gets or sets a delegate used to get the value of property that defined the argument.
    /// </summary>
    /// <value></value>
    public Func<object, object?>? GetProperty { get; set; }

    /// <summary>
    /// Gets or sets a delegate used to call the method that defined the argument.
    /// </summary>
    /// <value></value>
    public Func<object?, CommandLineParser, CancelMode>? CallMethod { get; set; }
}
