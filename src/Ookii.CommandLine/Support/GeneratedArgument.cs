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

    private GeneratedArgument(ArgumentInfo info, Action<object, object?>? setProperty) : base(info)
    {
        _setProperty = setProperty;
    }

    /// <summary>
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="argumentType"></param>
    /// <param name="allowsNull"></param>
    /// <param name="memberName"></param>
    /// <param name="attribute"></param>
    /// <param name="multiValueSeparatorAttribute"></param>
    /// <param name="descriptionAttribute"></param>
    /// <param name="allowDuplicateDictionaryKeys"></param>
    /// <param name="keyValueSeparatorAttribute"></param>
    /// <param name="aliasAttributes"></param>
    /// <param name="shortAliasAttributes"></param>
    /// <param name="validationAttributes"></param>
    /// <param name="converter"></param>
    /// <param name="setProperty"></param>
    /// <returns></returns>
    public static GeneratedArgument Create(CommandLineParser parser,
                                           Type argumentType,
                                           string memberName,
                                           CommandLineArgumentAttribute attribute,
                                           ArgumentConverter converter,
                                           bool allowsNull = false,
                                           MultiValueSeparatorAttribute? multiValueSeparatorAttribute = null,
                                           DescriptionAttribute? descriptionAttribute = null,
                                           bool allowDuplicateDictionaryKeys = false,
                                           KeyValueSeparatorAttribute? keyValueSeparatorAttribute = null,
                                           IEnumerable<AliasAttribute>? aliasAttributes = null,
                                           IEnumerable<ShortAliasAttribute>? shortAliasAttributes = null,
                                           IEnumerable<ArgumentValidationAttribute>? validationAttributes = null,
                                           Action<object, object?>? setProperty = null)
    {
        var info = CreateArgumentInfo(parser, argumentType, allowsNull, memberName, attribute,
            multiValueSeparatorAttribute, descriptionAttribute, allowDuplicateDictionaryKeys, keyValueSeparatorAttribute,
            aliasAttributes, shortAliasAttributes, validationAttributes);

        // TODO: Set property for multi-value and Nullable<T>.
        info.ElementType = argumentType;
        info.Converter = converter;

        return new GeneratedArgument(info, setProperty);
    }

    /// <inheritdoc/>
    protected override bool CanSetProperty => true;

    /// <inheritdoc/>
    protected override bool CallMethod(object? value) => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override object? GetProperty(object target) => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override void SetProperty(object target, object? value)
    {
        if (_setProperty == null)
        {
            throw new InvalidOperationException();
        }

        _setProperty(target, value);
    }
}
