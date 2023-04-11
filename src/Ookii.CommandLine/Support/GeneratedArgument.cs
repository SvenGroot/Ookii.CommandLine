using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    private GeneratedArgument(ArgumentInfo info, Action<object, object?>? setProperty, Func<object, object?>? getProperty) : base(info)
    {
        _setProperty = setProperty;
        _getProperty = getProperty;
    }

    /// <summary>
    /// 
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
    /// <param name="multiValueSeparatorAttribute"></param>
    /// <param name="descriptionAttribute"></param>
    /// <param name="allowDuplicateDictionaryKeys"></param>
    /// <param name="keyValueSeparatorAttribute"></param>
    /// <param name="aliasAttributes"></param>
    /// <param name="shortAliasAttributes"></param>
    /// <param name="validationAttributes"></param>
    /// <param name="setProperty"></param>
    /// <param name="getProperty"></param>
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
                                           Type? keyType = null,
                                           Type? valueType = null,
                                           MultiValueSeparatorAttribute? multiValueSeparatorAttribute = null,
                                           DescriptionAttribute? descriptionAttribute = null,
                                           bool allowDuplicateDictionaryKeys = false,
                                           KeyValueSeparatorAttribute? keyValueSeparatorAttribute = null,
                                           IEnumerable<AliasAttribute>? aliasAttributes = null,
                                           IEnumerable<ShortAliasAttribute>? shortAliasAttributes = null,
                                           IEnumerable<ArgumentValidationAttribute>? validationAttributes = null,
                                           Action<object, object?>? setProperty = null,
                                           Func<object, object?>? getProperty = null)
    {
        var info = CreateArgumentInfo(parser, argumentType, allowsNull, memberName, attribute,
            multiValueSeparatorAttribute, descriptionAttribute, allowDuplicateDictionaryKeys, keyValueSeparatorAttribute,
            aliasAttributes, shortAliasAttributes, validationAttributes);

        info.ElementType = elementType;
        info.ElementTypeWithNullable = elementTypeWithNullable;
        info.Converter = converter;
        info.Kind = kind;
        if (info.Kind == ArgumentKind.Dictionary)
        {
            info.KeyValueSeparator ??= KeyValuePairConverter.DefaultSeparator;
            info.KeyType = keyType;
            info.ValueType = valueType;
        }

        return new GeneratedArgument(info, setProperty, getProperty);
    }

    /// <inheritdoc/>
    protected override bool CanSetProperty => _setProperty != null;

    /// <inheritdoc/>
    protected override bool CallMethod(object? value) => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override object? GetProperty(object target)
    {
        if (_getProperty == null)
        {
            throw new InvalidOperationException();
        }

        try
        { 
            return _getProperty(target);
        }
        catch (Exception ex)
        {
            throw new TargetInvocationException(ex);
        }
    }

    /// <inheritdoc/>
    protected override void SetProperty(object target, object? value)
    {
        if (_setProperty == null)
        {
            throw new InvalidOperationException();
        }

        try
        {
            _setProperty(target, value);
        }
        catch (Exception ex)
        {
            throw new TargetInvocationException(ex);
        }
    }
}
