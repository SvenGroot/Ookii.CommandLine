using Microsoft.CodeAnalysis;

namespace Ookii.CommandLine.Generator;

internal class TypeHelper
{
    private readonly Compilation _compilation;
    private const string NamespacePrefix = "Ookii.CommandLine.";

    public TypeHelper(Compilation compilation)
    {
        _compilation = compilation;
    }

    public INamedTypeSymbol? Boolean => _compilation.GetSpecialType(SpecialType.System_Boolean);

    public INamedTypeSymbol? Dictionary => _compilation.GetTypeByMetadataName(typeof(Dictionary<,>).FullName);

    public INamedTypeSymbol? IDictionary => _compilation.GetTypeByMetadataName(typeof(IDictionary<,>).FullName);

    public INamedTypeSymbol? ICollection => _compilation.GetTypeByMetadataName(typeof(ICollection<>).FullName);

    public INamedTypeSymbol? DescriptionAttribute => _compilation.GetTypeByMetadataName("System.ComponentModel.DescriptionAttribute");

    public INamedTypeSymbol? ISpanParsable => _compilation.GetTypeByMetadataName("System.ISpanParsable`1");

    public INamedTypeSymbol? IParsable => _compilation.GetTypeByMetadataName("System.IParsable`1");

    public INamedTypeSymbol? CommandLineParser => _compilation.GetTypeByMetadataName(NamespacePrefix + "CommandLineParser");

    public INamedTypeSymbol? IParser => _compilation.GetTypeByMetadataName(NamespacePrefix + "IParserProvider`1");

    public INamedTypeSymbol? GeneratedParserAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "GeneratedParserAttribute");

    public INamedTypeSymbol? CommandLineArgumentAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "CommandLineArgumentAttribute");

    public INamedTypeSymbol? ParseOptionsAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "ParseOptionsAttribute");

    public INamedTypeSymbol? ApplicationFriendlyNameAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "ApplicationFriendlyNameAttribute");

    public INamedTypeSymbol? CommandAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Commands.CommandAttribute");

    public INamedTypeSymbol? ClassValidationAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Validation.ClassValidationAttribute");

    public INamedTypeSymbol? MultiValueSeparatorAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "MultiValueSeparatorAttribute");

    public INamedTypeSymbol? KeyValueSeparatorAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Conversion.KeyValueSeparatorAttribute");

    public INamedTypeSymbol? AllowDuplicateDictionaryKeysAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "AllowDuplicateDictionaryKeysAttribute");

    public INamedTypeSymbol? AliasAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "AliasAttribute");

    public INamedTypeSymbol? ShortAliasAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "ShortAliasAttribute");

    public INamedTypeSymbol? ArgumentValidationAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Validation.ArgumentValidationAttribute");

    public INamedTypeSymbol? ArgumentConverterAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Conversion.ArgumentConverterAttribute"
    );
    public INamedTypeSymbol? KeyConverterAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Conversion.KeyConverterAttribute");

    public INamedTypeSymbol? ValueConverterAttribute => _compilation.GetTypeByMetadataName(NamespacePrefix + "Conversion.ValueConverterAttribute");

}
