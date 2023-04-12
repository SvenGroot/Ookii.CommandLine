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

    public INamedTypeSymbol? String => _compilation.GetSpecialType(SpecialType.System_String);

    public INamedTypeSymbol? Void => _compilation.GetSpecialType(SpecialType.System_Void);

    public INamedTypeSymbol? Boolean => _compilation.GetSpecialType(SpecialType.System_Boolean);

    public INamedTypeSymbol? CommandLineParser => _compilation.GetTypeByMetadataName(NamespacePrefix + "CommandLineParser");

    public INamedTypeSymbol? IParser => _compilation.GetTypeByMetadataName(NamespacePrefix + "IParserProvider`1");
}
