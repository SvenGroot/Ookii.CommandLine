using Microsoft.CodeAnalysis;
using System;

namespace Ookii.CommandLine.Generator;

internal class ArgumentAttributes
{
    private readonly AttributeData? _commandLineArgumentAttribute;
    private readonly AttributeData? _multiValueSeparator;
    private readonly AttributeData? _description;
    private readonly AttributeData? _valueDescription;
    private readonly AttributeData? _allowDuplicateDictionaryKeys;
    private readonly AttributeData? _keyValueSeparator;
    private readonly AttributeData? _converterAttribute;
    private readonly AttributeData? _keyConverterAttribute;
    private readonly AttributeData? _valueConverterAttribute;
    private readonly AttributeData? _validateEnumValue;
    private readonly List<AttributeData>? _aliases;
    private readonly List<AttributeData>? _shortAliases;
    private readonly List<AttributeData>? _validators;

    public ArgumentAttributes(ISymbol member, TypeHelper typeHelper, SourceProductionContext context)
    {
        AttributeData? typeConverterAttribute = null;
        foreach (var attribute in member.GetAttributes())
        {
            _ = attribute.CheckType(typeHelper.CommandLineArgumentAttribute, ref _commandLineArgumentAttribute) ||
                attribute.CheckType(typeHelper.MultiValueSeparatorAttribute, ref _multiValueSeparator) ||
                attribute.CheckType(typeHelper.DescriptionAttribute, ref _description) ||
                attribute.CheckType(typeHelper.ValueDescriptionAttribute, ref _valueDescription) ||
                attribute.CheckType(typeHelper.AllowDuplicateDictionaryKeysAttribute, ref _allowDuplicateDictionaryKeys) ||
                attribute.CheckType(typeHelper.KeyValueSeparatorAttribute, ref _keyValueSeparator) ||
                attribute.CheckType(typeHelper.ArgumentConverterAttribute, ref _converterAttribute) ||
                attribute.CheckType(typeHelper.KeyConverterAttribute, ref _keyConverterAttribute) ||
                attribute.CheckType(typeHelper.ValueConverterAttribute, ref _valueConverterAttribute) ||
                attribute.CheckType(typeHelper.AliasAttribute, ref _aliases) ||
                attribute.CheckType(typeHelper.ShortAliasAttribute, ref _shortAliases) ||
                attribute.CheckType(typeHelper.ValidateEnumValueAttribute, ref _validateEnumValue) ||
                attribute.CheckType(typeHelper.ArgumentValidationAttribute, ref _validators) ||
                attribute.CheckType(typeHelper.TypeConverterAttribute, ref typeConverterAttribute);
        }

        // Since it was checked for separately, it won't be in the list.
        if (_validateEnumValue != null)
        {
            _validators ??= [];
            _validators.Add(_validateEnumValue);
        }

        // Warn if the TypeConverterAttribute is present.
        if (CommandLineArgument != null && typeConverterAttribute != null)
        {
            context.ReportDiagnostic(Diagnostics.IgnoredTypeConverterAttribute(member, typeConverterAttribute));
        }
    }

    public AttributeData? CommandLineArgument => _commandLineArgumentAttribute;
    public AttributeData? MultiValueSeparator => _multiValueSeparator;
    public AttributeData? Description => _description;
    public AttributeData? ValueDescription => _valueDescription;
    public AttributeData? AllowDuplicateDictionaryKeys => _allowDuplicateDictionaryKeys;
    public AttributeData? KeyValueSeparator => _keyValueSeparator;
    public AttributeData? Converter => _converterAttribute;
    public AttributeData? KeyConverter => _keyConverterAttribute;
    public AttributeData? ValueConverter => _valueConverterAttribute;
    public List<AttributeData>? Aliases => _aliases;
    public List<AttributeData>? ShortAliases => _shortAliases;
    public List<AttributeData>? Validators => _validators;
    public AttributeData? ValidateEnumValue => _validateEnumValue;

}
