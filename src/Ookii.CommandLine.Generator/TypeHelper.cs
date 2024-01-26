using Microsoft.CodeAnalysis;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Ookii.CommandLine.Generator;

internal class TypeHelper
{
    private readonly Compilation _compilation;
    private const string NamespacePrefix = "Ookii.CommandLine.";

    public TypeHelper(Compilation compilation)
    {
        _compilation = compilation;
    }

    public Compilation Compilation => _compilation;

    public INamedTypeSymbol Boolean => _compilation.GetSpecialType(SpecialType.System_Boolean);

    public INamedTypeSymbol Char => _compilation.GetSpecialType(SpecialType.System_Char);

    public INamedTypeSymbol? Dictionary => _compilation.GetTypeByMetadataName(typeof(Dictionary<,>).FullName);

    public INamedTypeSymbol? IDictionary => _compilation.GetTypeByMetadataName(typeof(IDictionary<,>).FullName);

    public INamedTypeSymbol? ICollection => _compilation.GetTypeByMetadataName(typeof(ICollection<>).FullName);

    public INamedTypeSymbol? DescriptionAttribute => _compilation.GetTypeByMetadataName(typeof(DescriptionAttribute).FullName);

    public INamedTypeSymbol? AssemblyDescriptionAttribute => _compilation.GetTypeByMetadataName(typeof(AssemblyDescriptionAttribute).FullName);

    public INamedTypeSymbol? TypeConverterAttribute => _compilation.GetTypeByMetadataName(typeof(TypeConverterAttribute).FullName);

    public INamedTypeSymbol? ISpanParsable => _compilation.GetTypeByMetadataName("System.ISpanParsable`1");

    public INamedTypeSymbol? IParsable => _compilation.GetTypeByMetadataName("System.IParsable`1");

    public INamedTypeSymbol? ReadOnlySpan => _compilation.GetTypeByMetadataName("System.ReadOnlySpan`1");

    public INamedTypeSymbol? ReadOnlySpanOfChar => ReadOnlySpan?.Construct(Char);

    public INamedTypeSymbol? CultureInfo => _compilation.GetTypeByMetadataName(typeof(CultureInfo).FullName);

    public INamedTypeSymbol? CommandLineParser => _compilation.GetTypeByMetadataName(NamespacePrefix + "CommandLineParser");

    public INamedTypeSymbol? IParser => _compilation.GetTypeByMetadataName(NamespacePrefix + "IParserProvider`1");

    public INamedTypeSymbol? GeneratedParserAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "GeneratedParserAttribute");

    public INamedTypeSymbol? CommandLineArgumentAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "CommandLineArgumentAttribute");

    public INamedTypeSymbol? ParseOptionsAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "ParseOptionsAttribute");

    public INamedTypeSymbol? ApplicationFriendlyNameAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "ApplicationFriendlyNameAttribute");

    public INamedTypeSymbol? MultiValueSeparatorAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "MultiValueSeparatorAttribute");

    public INamedTypeSymbol? AllowDuplicateDictionaryKeysAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "AllowDuplicateDictionaryKeysAttribute");

    public INamedTypeSymbol? AliasAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "AliasAttribute");

    public INamedTypeSymbol? ShortAliasAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "ShortAliasAttribute");

    public INamedTypeSymbol? ValueDescriptionAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "ValueDescriptionAttribute");

    public INamedTypeSymbol? CancelMode => _compilation.GetTypeByMetadataName(NamespacePrefix + "CancelMode");

    public INamedTypeSymbol? UsageFooterAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "UsageFooterAttribute");

    public INamedTypeSymbol? ArgumentValidationAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Validation.ArgumentValidationAttribute");

    public INamedTypeSymbol? ClassValidationAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Validation.ClassValidationAttribute");

    public INamedTypeSymbol? ValidateEnumValueAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Validation.ValidateEnumValueAttribute");

    public INamedTypeSymbol? KeyValueSeparatorAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Conversion.KeyValueSeparatorAttribute");

    public INamedTypeSymbol? ArgumentConverterAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Conversion.ArgumentConverterAttribute"
    );
    public INamedTypeSymbol? KeyConverterAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Conversion.KeyConverterAttribute");

    public INamedTypeSymbol? ValueConverterAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Conversion.ValueConverterAttribute");

    public INamedTypeSymbol? GeneratedConverterNamespaceAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Conversion.GeneratedConverterNamespaceAttribute");

    public INamedTypeSymbol? CommandAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Commands.CommandAttribute");

    public INamedTypeSymbol? GeneratedCommandManagerAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Commands.GeneratedCommandManagerAttribute");

    public INamedTypeSymbol? ICommand => _compilation.GetTypeByMetadataName(NamespacePrefix + "Commands.ICommand");

    public INamedTypeSymbol? ICommandWithCustomParsing => _compilation.GetTypeByMetadataName(NamespacePrefix + "Commands.ICommandWithCustomParsing");

    public INamedTypeSymbol? ICommandProvider => _compilation.GetTypeByMetadataName(NamespacePrefix + "Commands.ICommandProvider");

    public INamedTypeSymbol? ParentCommandAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Commands.ParentCommandAttribute");

}
