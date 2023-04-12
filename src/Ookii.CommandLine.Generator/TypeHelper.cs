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

    public INamedTypeSymbol? ISpanParsable => _compilation.GetTypeByMetadataName("System.ISpanParsable`1");

    public INamedTypeSymbol? IParsable => _compilation.GetTypeByMetadataName("System.IParsable`1");

    public INamedTypeSymbol? CommandLineParser => _compilation.GetTypeByMetadataName(NamespacePrefix + "CommandLineParser");

    public INamedTypeSymbol? IParser => _compilation.GetTypeByMetadataName(NamespacePrefix + "IParserProvider`1");
}
