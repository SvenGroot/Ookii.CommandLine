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

    public INamedTypeSymbol? String => _compilation.GetTypeByMetadataName("System.String");

    public INamedTypeSymbol? Void => _compilation.GetTypeByMetadataName("System.Void");

    public INamedTypeSymbol? Boolean => _compilation.GetTypeByMetadataName("System.Boolean");

    public INamedTypeSymbol? CommandLineParser => _compilation.GetTypeByMetadataName(NamespacePrefix + "CommandLineParser");
}
