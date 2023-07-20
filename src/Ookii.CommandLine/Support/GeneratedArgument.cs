using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Ookii.CommandLine.Support;

/// <summary>
/// Represents information about an argument determined by the source generator.
/// </summary>
/// <remarks>
/// This class is used by the source generator when using the <see cref="GeneratedParserAttribute"/>
/// attribute. It should not normally be used by other code.
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public class GeneratedArgument : CommandLineArgument
{
    private readonly Action<object, object?>? _setProperty;
    private readonly Func<object, object?>? _getProperty;
    private readonly Func<object?, CommandLineParser, CancelMode>? _callMethod;
    private readonly string _defaultValueDescription;
    private readonly string? _defaultKeyDescription;

    private GeneratedArgument(ArgumentInfo info, Action<object, object?>? setProperty, Func<object, object?>? getProperty,
        Func<object?, CommandLineParser, CancelMode>? callMethod, string defaultValueDescription, string? defaultKeyDescription) : base(info)
    {
        _setProperty = setProperty;
        _getProperty = getProperty;
        _callMethod = callMethod;
        _defaultValueDescription = defaultValueDescription;
        _defaultKeyDescription = defaultKeyDescription;
    }

    /// <summary>
    /// Creates a <see cref="GeneratedArgument"/> instance.
    /// </summary>
    /// <param name="parser">The <see cref="CommandLineParser"/> this argument belongs to.</param>
    /// <param name="argumentType">The type of the argument.</param>
    /// <param name="elementTypeWithNullable">The element type including <see cref="Nullable{T}"/>.</param>
    /// <param name="elementType">The element type excluding <see cref="Nullable{T}"/>.</param>
    /// <param name="memberName">The name of the property or method.</param>
    /// <param name="attribute">The <see cref="CommandLineArgumentAttribute"/>.</param>
    /// <param name="kind">The kind of argument.</param>
    /// <param name="converter">The <see cref="ArgumentConverter"/> for the argument's type.</param>
    /// <param name="allowsNull">Indicates if <see langword="null"/> values are allowed.</param>
    /// <param name="defaultValueDescription">
    /// The value description to use if the <see cref="ValueDescriptionAttribute"/> attribute is
    /// not present. For dictionary arguments, this is the value description for the value of the
    /// key/value pair.
    /// </param>
    /// <param name="position">The position for positional arguments that use automatic positioning.</param>
    /// <param name="defaultKeyDescription">
    /// The value description to use for the key of a dictionary argument if the
    /// <see cref="ValueDescriptionAttribute"/> attribute is not present.
    /// </param>
    /// <param name="requiredProperty">
    /// Indicates if the argument used a C# 11 <c>required</c> property.
    /// </param>
    /// <param name="alternateDefaultValue">
    /// Default value to use if the <see cref="CommandLineArgumentAttribute.DefaultValue" qualifyHint="true"/> property
    /// is not set.
    /// </param>
    /// <param name="keyType">The type of the key of a dictionary argument.</param>
    /// <param name="valueType">The type of the value of a dictionary argument.</param>
    /// <param name="multiValueSeparatorAttribute">The <see cref="MultiValueSeparatorAttribute"/>.</param>
    /// <param name="descriptionAttribute">The <see cref="DescriptionAttribute"/>.</param>
    /// <param name="valueDescriptionAttribute">The <see cref="ValueDescriptionAttribute"/>.</param>
    /// <param name="allowDuplicateDictionaryKeys">The <see cref="AllowDuplicateDictionaryKeysAttribute"/>.</param>
    /// <param name="keyValueSeparatorAttribute">The <see cref="KeyValueSeparatorAttribute"/>.</param>
    /// <param name="aliasAttributes">A collection of <see cref="AliasAttribute"/> values.</param>
    /// <param name="shortAliasAttributes">A collection of <see cref="ShortAliasAttribute"/> values.</param>
    /// <param name="validationAttributes">A collection of <see cref="ArgumentValidationAttribute"/> values.</param>
    /// <param name="setProperty">
    /// A delegate that sets the value of the property that defined the argument.
    /// </param>
    /// <param name="getProperty">
    /// A delegate that gets the value of the property that defined the argument.
    /// </param>
    /// <param name="callMethod">
    /// A delegate that calls the method that defined the argument.
    /// </param>
    /// <returns>A <see cref="GeneratedArgument"/> instance.</returns>
    public static GeneratedArgument Create(CommandLineParser parser,
                                           Type argumentType,
                                           Type elementTypeWithNullable,
                                           Type elementType,
                                           string memberName,
                                           CommandLineArgumentAttribute attribute,
                                           ArgumentKind kind,
                                           ArgumentConverter converter,
                                           bool allowsNull,
                                           string defaultValueDescription,
                                           int? position = null,
                                           string? defaultKeyDescription = null,
                                           bool requiredProperty = false,
                                           object? alternateDefaultValue = null,
                                           Type? keyType = null,
                                           Type? valueType = null,
                                           MultiValueSeparatorAttribute? multiValueSeparatorAttribute = null,
                                           DescriptionAttribute? descriptionAttribute = null,
                                           ValueDescriptionAttribute? valueDescriptionAttribute = null,
                                           bool allowDuplicateDictionaryKeys = false,
                                           KeyValueSeparatorAttribute? keyValueSeparatorAttribute = null,
                                           IEnumerable<AliasAttribute>? aliasAttributes = null,
                                           IEnumerable<ShortAliasAttribute>? shortAliasAttributes = null,
                                           IEnumerable<ArgumentValidationAttribute>? validationAttributes = null,
                                           Action<object, object?>? setProperty = null,
                                           Func<object, object?>? getProperty = null,
                                           Func<object?, CommandLineParser, CancelMode>? callMethod = null)
    {
        if (position is int pos)
        {
            Debug.Assert(attribute.IsPositional && attribute.Position < 0);
            attribute.Position = pos;
        }

        var info = CreateArgumentInfo(parser, argumentType, allowsNull, requiredProperty, memberName, attribute,
            descriptionAttribute, valueDescriptionAttribute, aliasAttributes, shortAliasAttributes, validationAttributes);

        info.ElementType = elementType;
        info.ElementTypeWithNullable = elementTypeWithNullable;
        info.Converter = converter;
        info.Kind = kind;
        info.DefaultValue ??= alternateDefaultValue;
        if (info.Kind is ArgumentKind.MultiValue or ArgumentKind.Dictionary)
        {
            info.MultiValueInfo = GetMultiValueInfo(multiValueSeparatorAttribute);
            if (info.Kind == ArgumentKind.Dictionary)
            {
                info.DictionaryInfo = new(allowDuplicateDictionaryKeys, keyType!, valueType!,
                    keyValueSeparatorAttribute?.Separator ?? KeyValuePairConverter.DefaultSeparator);
            }
        }

        return new GeneratedArgument(info, setProperty, getProperty, callMethod, defaultValueDescription, defaultKeyDescription);
    }

    /// <inheritdoc/>
    protected override bool CanSetProperty => _setProperty != null;

    /// <inheritdoc/>
    protected override CancelMode CallMethod(object? value)
    {
        if (_callMethod == null)
        {
            throw new InvalidOperationException(Properties.Resources.InvalidMethodAccess);
        }

        return _callMethod(value, this.Parser);
    }

    /// <inheritdoc/>
    protected override object? GetProperty(object target)
    {
        if (_getProperty == null)
        {
            throw new InvalidOperationException(Properties.Resources.InvalidPropertyAccess);
        }

        return _getProperty(target);
    }

    /// <inheritdoc/>
    protected override void SetProperty(object target, object? value)
    {
        if (_setProperty == null)
        {
            throw new InvalidOperationException(Properties.Resources.InvalidPropertyAccess);
        }

        _setProperty(target, value);
    }

    /// <inheritdoc/>
    protected override string DetermineValueDescriptionForType(Type type)
    {
        Debug.Assert(DictionaryInfo == null ? type == ElementType : (type == DictionaryInfo.KeyType || type == DictionaryInfo.ValueType));
        if (DictionaryInfo != null && type == DictionaryInfo.KeyType)
        {
            return _defaultKeyDescription!;
        }

        return _defaultValueDescription;
    }
}
