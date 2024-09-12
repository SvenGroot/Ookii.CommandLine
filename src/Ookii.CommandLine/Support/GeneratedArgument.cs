using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Ookii.CommandLine.Support;

/// <summary>
/// Represents information about an argument determined by the source generator.
/// </summary>
/// <remarks>
/// This class is used by the source generator when using the <see cref="GeneratedParserAttribute"/>
/// attribute. It should not normally be used by other code.
/// </remarks>
/// <threadsafety static="true" instance="false"/>
// This class is here only for binary compatibility with previous versions of the library. It
// should be removed for the next major version.
#if NET7_0_OR_GREATER
[RequiresDynamicCode("Recompile your project to use the updated generator.")]
#endif
public class GeneratedArgument : CommandLineArgument
{
    private readonly Action<object, object?>? _setProperty;
    private readonly Func<object, object?>? _getProperty;
    private readonly Func<object?, CommandLineParser, CancelMode>? _callMethod;
    private readonly string _defaultValueDescription;
    private readonly string? _defaultKeyDescription;
    private MemberInfo? _member;

    private GeneratedArgument(ArgumentCreationInfo info) : base(new ArgumentInfo(info))
    {
        _setProperty = info.SetProperty;
        _getProperty = info.GetProperty;
        _callMethod = info.CallMethod;
        _defaultValueDescription = info.DefaultValueDescription;
        _defaultKeyDescription = info.DefaultKeyDescription;
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
        var creationInfo = new ArgumentCreationInfo()
        {
            Parser = parser,
            ArgumentType = argumentType,
            ElementType = elementType,
            ElementTypeWithNullable = elementTypeWithNullable,
            MemberName = memberName,
            Attribute = attribute,
            Kind = kind,
            Converter = converter,
            AllowsNull = allowsNull,
            DefaultValueDescription = defaultValueDescription,
            Position = position,
            DefaultKeyDescription = defaultKeyDescription,
            RequiredProperty = requiredProperty,
            AlternateDefaultValue = alternateDefaultValue,
            KeyType = keyType,
            ValueType = valueType,
            MultiValueSeparatorAttribute = multiValueSeparatorAttribute,
            DescriptionAttribute = descriptionAttribute,
            ValueDescriptionAttribute = valueDescriptionAttribute,
            AllowDuplicateDictionaryKeys = allowDuplicateDictionaryKeys,
            KeyValueSeparatorAttribute = keyValueSeparatorAttribute,
            AliasAttributes = aliasAttributes,
            ShortAliasAttributes = shortAliasAttributes,
            ValidationAttributes = validationAttributes,
            SetProperty = setProperty,
            GetProperty = getProperty,
            CallMethod = callMethod
        };

        return new GeneratedArgument(creationInfo);
    }

    /// <inheritdoc/>
    public override MemberInfo? Member => _member ??= (MemberInfo?)Parser.ArgumentsType.GetProperty(MemberName)
        ?? Parser.ArgumentsType.GetMethod(MemberName, BindingFlags.Public | BindingFlags.Static);

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

    private protected override IValueHelper CreateDictionaryValueHelper()
    {
        var type = typeof(DictionaryValueHelper<,>).MakeGenericType(ElementType.GetGenericArguments());
        return (IValueHelper)Activator.CreateInstance(type, DictionaryInfo!.AllowDuplicateKeys, AllowNull)!;
    }

    private protected override IValueHelper CreateMultiValueHelper()
    {
        var type = typeof(MultiValueHelper<>).MakeGenericType(ElementTypeWithNullable);
        return (IValueHelper)Activator.CreateInstance(type)!;
    }
}
