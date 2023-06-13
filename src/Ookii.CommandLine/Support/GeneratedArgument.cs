using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Support;

/// <summary>
/// This class is for internal use by the source generator, and should not be used in your code.
/// </summary>
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
    /// This class is for internal use by the source generator, and should not be used in your code.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="argumentType"></param>
    /// <param name="memberName"></param>
    /// <param name="attribute"></param>
    /// <param name="kind"></param>
    /// <param name="converter"></param>
    /// <param name="elementType"></param>
    /// <param name="elementTypeWithNullable"></param>
    /// <param name="keyType"></param>
    /// <param name="valueType"></param>
    /// <param name="allowsNull"></param>
    /// <param name="position"></param>
    /// <param name="defaultValueDescription"></param>
    /// <param name="defaultKeyDescription"></param>
    /// <param name="requiredProperty"></param>
    /// <param name="alternateDefaultValue"></param>
    /// <param name="multiValueSeparatorAttribute"></param>
    /// <param name="descriptionAttribute"></param>
    /// <param name="valueDescriptionAttribute"></param>
    /// <param name="allowDuplicateDictionaryKeys"></param>
    /// <param name="keyValueSeparatorAttribute"></param>
    /// <param name="aliasAttributes"></param>
    /// <param name="shortAliasAttributes"></param>
    /// <param name="validationAttributes"></param>
    /// <param name="setProperty"></param>
    /// <param name="getProperty"></param>
    /// <param name="callMethod"></param>
    /// <returns></returns>
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
            multiValueSeparatorAttribute, descriptionAttribute, valueDescriptionAttribute, allowDuplicateDictionaryKeys,
            keyValueSeparatorAttribute, aliasAttributes, shortAliasAttributes, validationAttributes);

        info.ElementType = elementType;
        info.ElementTypeWithNullable = elementTypeWithNullable;
        info.Converter = converter;
        info.Kind = kind;
        info.DefaultValue ??= alternateDefaultValue;
        if (info.Kind == ArgumentKind.Dictionary)
        {
            info.KeyValueSeparator ??= KeyValuePairConverter.DefaultSeparator;
            info.KeyType = keyType;
            info.ValueType = valueType;
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
        Debug.Assert(type  == KeyType || type == ValueType || (ValueType == null && type == ElementType));
        if (KeyType != null && type == KeyType)
        {
            return _defaultKeyDescription!;
        }

        return _defaultValueDescription;
    }
}
